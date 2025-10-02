using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using PlayerAvalonia.Models;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ManagedBass;
using PlayerAvalonia.Helpers;

namespace PlayerAvalonia.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {

        public ObservableCollection<MenuListItem> MenuItems { get; } = new();
        [ObservableProperty]
        public MenuListItem? selectedMenuItem;
        public IRelayCommand<MenuListItem> MenuItemSelectedCommand { get; }
        public ObservableCollection<Song> Songs { get; set; } = new();
        public IAsyncRelayCommand LoadMusicCommand { get; }
        public ICommand NextCommand => new RelayCommand(PlayNextSong);
        public ICommand PreviuosCommand => new RelayCommand(PlayPreviuosSong);
        public ICommand PlayPauseCommand => new RelayCommand(PauseOrResume);
        public ICommand IndexCommand => new RelayCommand(SortByIndex);
        public ICommand ArtistCommand => new RelayCommand(SortByArtist);
        public ICommand TitleCommand => new RelayCommand(SortByTitle);
        
        private int _streamHandle;
    private bool _isDraggingSlider;

    public bool IsDraggingSlider
    {
        get => _isDraggingSlider;
        set => SetProperty(ref _isDraggingSlider, value);
    }
        
        [ObservableProperty]
        private string? lastFolder;
        
        
        private Song _selectedSong;

        public Song SelectedSong
        {
            get => _selectedSong;
            set
            {
                if (_selectedSong != value)
                {
                    _selectedSong = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SelectedArtist));
                    OnPropertyChanged(nameof(SelectedTitle));
                    OnPropertyChanged(nameof(SelectedAlbum));
                    PlaySelectedSong();

                }
            }
        }
        public void SortByTitle()
        {
            Songs.Sort(s => s.Title);
        }
        public void SortByIndex()
        {
            Songs.Sort(s => s.Index);
        }
        public void SortByArtist()
        {
            Songs.Sort(s => s.Artist);
        }
        
        public bool IsPlaying
        {
            get => _isPlaying;
            set
            {
                if (_isPlaying != value)
                {
                    _isPlaying = value;
                    OnPropertyChanged(nameof(IsPlaying));
                    OnPropertyChanged(nameof(PlayPauseIcon));
                }
            }
        }
        public string SelectedTitle
        {
            get
            {
                if(_selectedSong?.Title == null)
                {
                    _selectedSong.Title = "Unknown Title";
                }
                else if(_selectedSong.Title.Length >= 30)
                {
                     _selectedSong.Title = FullTitle.Substring(0, 30 - 3) + "...";
                }
                return _selectedSong.Title;
            }
        }
        public string FullTitle;
        public string FullArtist;
        public string SelectedArtist
        {
            get
            {
                if(_selectedSong.Artist == null)
                {
                    _selectedSong.Artist = "Unknown Artist";
                }
                else if(_selectedSong.Artist.Length >= 20)
                {
                     _selectedSong.Artist = FullArtist.Substring(0, 20 - 3) + "...";
                }
                return _selectedSong?.Artist;
            }
        }
        public Bitmap SelectedAlbum => _selectedSong?.AlbumArt ?? DefaultImage();
            
        private DispatcherTimer _positionTimer;
        protected bool _isPlaying = false;
        public string PlayPauseIcon => IsPlaying
    ? "/Assets/Pause.png"
    : "/Assets/Play.png";
        
        private double _trackPositionSeconds;
        public double TrackPositionSeconds
    {
        get => _trackPositionSeconds;
        set
        {
            if (SetProperty(ref _trackPositionSeconds, value))
            {
                if (_isDraggingSlider && _streamHandle != 0)
                {
                    long bytePos = Bass.ChannelSeconds2Bytes(_streamHandle, _trackPositionSeconds);
                    Bass.ChannelSetPosition(_streamHandle, bytePos);
                }
            }
        }
    }
        private double _trackDurationSeconds;
        public double TrackDurationSeconds
    {
        get => _trackDurationSeconds;
        private set => SetProperty(ref _trackDurationSeconds, value);
    }

        public void LoadSongs(string[] filePaths)
        {
            int index = 1;
            Songs.Clear();
            foreach (var path in filePaths)
            {
                var file = TagLib.File.Create(path);
                var song = new Song
                {
                    Index = index++,
                    FilePath = path,
                    Title = file.Tag.Title ?? System.IO.Path.GetFileNameWithoutExtension(path),
                    Artist = file.Tag.FirstPerformer ?? "Unknown",
                    AlbumArt = LoadImage(file)
                };
                FullTitle = song.Title;
                FullArtist = song.Artist;
                if (song.Title.Length > 30)
                {
                    song.Title = song.Title.Substring(0, 30 - 3) + "...";
                }
                Songs.Add(song);
            }
        }
        public void StartTimer()
        {
            DispatcherTimer.Run(() =>
            {
                if (_streamHandle != 0 && !_isDraggingSlider)
                {
                    long pos = Bass.ChannelGetPosition(_streamHandle);
                    TrackPositionSeconds = Bass.ChannelBytes2Seconds(_streamHandle, pos);
                }
                return true; // повторювати
            }, TimeSpan.FromMilliseconds(500));
        }
        
        private Bitmap LoadImage(TagLib.File file)
        {
            if (file.Tag.Pictures.Length > 0)
            {
                using var ms = new MemoryStream(file.Tag.Pictures[0].Data.Data);
                return new Bitmap(ms);
            }
        
            return DefaultImage();
        }
        
        public Bitmap DefaultImage()
        {
            // якщо Assets лежать у проекті (копіюються в output), використовуй шлях напряму:
            return new Bitmap("Assets/albumdefault.png");
        }
        public void SeekToPositionFromSlider()
        {
            if (_streamHandle != 0)
            {
                // Конвертуємо секунди в байти
                long bytePos = Bass.ChannelSeconds2Bytes(_streamHandle, TrackPositionSeconds);
                // Встановлюємо позицію відтворення
                Bass.ChannelSetPosition(_streamHandle, bytePos);
            }
        }
        private async Task SelectFolderAndLoadMusic(Window owner)
        {
            // Зупиняємо відтворення, якщо щось грає
            if (IsPlaying && _streamHandle != 0)
            {
                Bass.ChannelPause(_streamHandle); // пауза через ManagedBass
                _positionTimer?.Stop();
                IsPlaying = false;
            }
        
            // Діалог вибору папки
            if (owner == null)
            {
                owner = (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            }
        
            if (!owner.StorageProvider.CanOpen)
            {
                Console.WriteLine("StorageProvider is not available on this platform.");
                return;
            }
        
            var folders = await owner.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Choose Directory",
                AllowMultiple = false
            });
        
            if (folders != null && folders.Count > 0)
            {
                var selectedPath = folders[0].Path.LocalPath;
        
                SettingsHelper.SaveLastFolder(selectedPath);
                GetPlayListName(selectedPath);
                LoadMusicFromFolder(selectedPath);
            }
        }
        public void LoadLastUsedFolder()
        {
            string? lastFolder = SettingsHelper.LoadLastFolder();
            if(lastFolder == null)
            {
                LoadMusicFromFolder(@"/home/gogenskyi/Музика/");
            }
            if (!string.IsNullOrEmpty(lastFolder) && Directory.Exists(lastFolder))
            {
                LoadMusicFromFolder(lastFolder);
                GetPlayListName(lastFolder);
            }
        }

        private string _playlistName;
        public string PlaylistName
        {
            get => _playlistName;
        }
        public void GetPlayListName(string FolderPath)
        {
            if(FolderPath == null)
            {
                _playlistName = "Select Playlist";
                OnPropertyChanged(nameof(PlaylistName));

            }
            else
            {
                _playlistName = Path.GetFileName(Path.GetFullPath(FolderPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                OnPropertyChanged(nameof(PlaylistName));

            }

        }
        private void LoadMusicFromFolder(string folderPath)
        {
            var supportedExtensions = new[] { ".mp3", ".wav", ".flac" };
        
            var files = Directory.EnumerateFiles(folderPath, "*.*", SearchOption.AllDirectories)
                                 .Where(f => supportedExtensions.Contains(Path.GetExtension(f).ToLower()));
        
            Songs.Clear();
            int i = 1;
            foreach (var path in files)
            {
                try
                {
                    var tagFile = TagLib.File.Create(path);
        
                    Bitmap albumArtImage;
        
                    if (tagFile.Tag.Pictures.Length > 0)
                    {
                        using var ms = new MemoryStream(tagFile.Tag.Pictures[0].Data.Data);
                        albumArtImage = new Bitmap(ms);
                    }
                    else
                    {
                        // fallback image (Assets має бути позначена як AvaloniaResource у .csproj)
                        albumArtImage = new Bitmap("Assets/albumdefault.png");
                    }
            
                    var song = new Song
                    {
                        FilePath = path,
                        Index = i++,
                        Title = tagFile.Tag.Title ?? Path.GetFileNameWithoutExtension(path),
                        Artist = tagFile.Tag.FirstPerformer ?? "Unknown",
                        AlbumArt = albumArtImage
                    };
                    i = i++;
        
                    Songs.Add(song);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Помилка при зчитуванні {path}: {ex.Message}");
                }
            }
        }
        private void PlaySelectedSong()
        {
            if (SelectedSong == null)
                return;
        
            PlaySong(SelectedSong.FilePath);
        }
        private void PlaySong(string filePath)
        {
            try
            {
                // Зупиняємо попередній стрім
                if (_streamHandle != 0)
                {
                    Bass.ChannelStop(_streamHandle);
                    Bass.StreamFree(_streamHandle);
                    _streamHandle = 0;
                }
        
                _positionTimer?.Stop();
        
                // Ініціалізація аудіосистеми (один раз на застосунок достатньо)
                if (!Bass.Init())
                {
                    Bass.Init();
                }
        
                // Створюємо новий стрім
                _streamHandle = Bass.CreateStream(filePath);
        
                if (_streamHandle == 0)
                {
                    Console.WriteLine($"Не вдалося відкрити файл: {Bass.LastError}");
                    IsPlaying = false;
                    return;
                }
        
                // Старт відтворення
                IsPlaying = true;
                Bass.ChannelPlay(_streamHandle);
        
                // Таймер для оновлення позиції
                _positionTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(1)
                };
        
                _positionTimer.Tick += (s, e) =>
                {
                    if (!_isDraggingSlider && _streamHandle != 0)
                    {
                        // Поточна позиція (в секундах)
                        long pos = Bass.ChannelGetPosition(_streamHandle);
                        long len = Bass.ChannelGetLength(_streamHandle);
        
                        TrackPositionSeconds = Bass.ChannelBytes2Seconds(_streamHandle, pos);
                        TrackDurationSeconds = Bass.ChannelBytes2Seconds(_streamHandle, len);

        
                        OnPropertyChanged(nameof(TrackDurationSeconds));
                        OnPropertyChanged(nameof(CurrentTimePosition));
                    }
                };
        
                _positionTimer.Start();
                OnPropertyChanged(nameof(DurationTimePosition));
            }
            catch (Exception ex)
            {
                IsPlaying = false;
                _positionTimer?.Stop();
                Console.WriteLine($"Помилка при відтворенні: {ex.Message}");
            }
        }
        private void PauseOrResume()
        {
            if (_streamHandle == 0)
                return;
        
            if (IsPlaying)
            {
                // Пауза
                Bass.ChannelPause(_streamHandle);
                _positionTimer?.Stop();
                IsPlaying = false;
            }
            else
            {
                // Продовжити відтворення
                Bass.ChannelPlay(_streamHandle);
                _positionTimer?.Start();
                IsPlaying = true;
            }
        
            OnPropertyChanged(nameof(IsPlaying));
        }
        private void PlayNextSong()
        {
            if (SelectedSong == null || Songs == null || Songs.Count == 0)
                return;

            int Index = Songs.IndexOf(SelectedSong);
            if (Index >= 0 && Index < Songs.Count - 1)
            {
                SelectedSong = Songs[Index + 1];
                Index++;

            }
            else if (Index == Songs.Count - 1)
            {
                SelectedSong = Songs[0];
                Index = 0;
            }
        }
        public int StreamHandle { get; set; }
        private void PlayPreviuosSong()
        {
            if (SelectedSong == null || Songs == null || Songs.Count == 0)
                return;

            int Index = Songs.IndexOf(SelectedSong);
            try
            {

               
                if (Index >= 0 && Index < Songs.Count - 1)
                {
                    SelectedSong = Songs[Index - 1];
                    Index--;
                }
                else if (Index <= 0)
                {
                    SelectedSong = Songs[Songs.Count - 1];
                    Index = Songs.Count;
                }
                else
                {
                    return;
                }
            }
            catch
            {
                Index = Songs.Count;
            }
        }
        private void CheckPlaybackStopped()
        {
            if (_streamHandle == 0) return;
    
            long pos = Bass.ChannelGetPosition(_streamHandle);
            long len = Bass.ChannelGetLength(_streamHandle);
    
            if (len > 0 && (len - pos) <= Bass.ChannelSeconds2Bytes(_streamHandle, 0.2))
            {
                Dispatcher.UIThread.Post(() =>
                {
                    PlayNextSong();
                });
            }
        }
        private void OnMenuItemSelected(MenuListItem? item)
        {
            item?.Command.Execute(null);
        }

        public MainWindowViewModel()
        {
            MenuItems.Add(new MenuListItem("Home", new RelayCommand(SortByIndex)));
            MenuItems.Add(new MenuListItem("Title", new RelayCommand(SortByTitle)));
            MenuItems.Add(new MenuListItem("Artist", new RelayCommand(SortByArtist)));
            MenuItemSelectedCommand = new RelayCommand<MenuListItem>(OnMenuItemSelected);
            LoadMusicCommand = new AsyncRelayCommand<Window>(SelectFolderAndLoadMusic);
        }
        public string CurrentTimePosition =>
            TimeSpan.FromSeconds(TrackPositionSeconds).ToString(@"mm\:ss");
        public string DurationTimePosition =>
            TimeSpan.FromSeconds(TrackDurationSeconds).ToString(@"mm\:ss");
    }
    
}
