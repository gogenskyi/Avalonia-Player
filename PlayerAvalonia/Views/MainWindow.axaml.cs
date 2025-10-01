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
        
        private void Menu_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm && vm.SelectedMenuItem != null)
            {
                vm.SelectedMenuItem.Command.Execute(null);
            }
        }
    }
}