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
//        private void Slider_DragStarted(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
//        {
//            if (DataContext is MainWindowViewModel vm)
//            {
//                  vm.IsDraggingSlider = true;
//            }
//        }
//        
//        private void Slider_DragCompleted(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
//        {
//            if (DataContext is MainWindowViewModel vm)
//                vm.IsDraggingSlider = false;
//        }
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