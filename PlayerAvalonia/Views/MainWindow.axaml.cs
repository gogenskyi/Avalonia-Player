using System.IO;
using System.Reflection;
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
            PositionSlider.AddHandler(InputElement.PointerPressedEvent, Slider_PointerPressed, RoutingStrategies.Tunnel);
            PositionSlider.AddHandler(InputElement.PointerReleasedEvent, Slider_PointerReleased, RoutingStrategies.Tunnel);
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
            {
              vm.LoadLastUsedFolder();  
            }
        }
        private void PlayIconIsEnabled(object? sender, PointerPressedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                if(vm.IsPlaying)
                {
                    IsEnabled = true;
                }
                else
                    IsEnabled = false;
            }
        }
        private void PauseIconIsEnabled(object? sender, PointerPressedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                if(vm.IsPlaying)
                {
                    IsEnabled = false;
                }
                else
                    IsEnabled = true;
            }
        }
        private void Slider_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
                vm.GetType().GetField("_isDraggingSlider", BindingFlags.NonPublic | BindingFlags.Instance)
                  ?.SetValue(vm, true);
        }

        private void Slider_PointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.GetType().GetField("_isDraggingSlider", BindingFlags.NonPublic | BindingFlags.Instance)
                  ?.SetValue(vm, false);

                vm.SeekToPositionFromSlider();
            }
        }
    }
}