using System.IO;
using Avalonia.Controls;
using Avalonia.Input;
using PlayerAvalonia.ViewModels;
using Avalonia.Interactivity;

namespace PlayerAvalonia.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
            {
            var files = Directory.GetFiles(@"/home/gogenskyi/Музика", "*.mp3");
            vm.LoadSongs(files);
            }
        }
        private void Slider_DragStarted(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                  vm.IsDraggingSlider = true;
            }
        }
        
        private void Slider_DragCompleted(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
                vm.IsDraggingSlider = false;
        }
        private void Slider_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.IsDraggingSlider = true;
    
                // одразу підхоплюємо Value при кліку по доріжці
                if (sender is Slider slider)
                {
                    vm.TrackPositionSeconds = slider.Value;
                }
            }
        }
    
        private void Slider_PointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.IsDraggingSlider = false;
    
                // при відпусканні виставляємо нову позицію у Bass
                if (sender is Slider slider && vm.StreamHandle != 0)
                {
                    long bytePos = ManagedBass.Bass.ChannelSeconds2Bytes(vm.StreamHandle, slider.Value);
                    ManagedBass.Bass.ChannelSetPosition(vm.StreamHandle, bytePos);
                }
            }
        }
    }
}