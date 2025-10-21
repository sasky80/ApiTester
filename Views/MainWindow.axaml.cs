using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Input;
using Avalonia;

namespace ApiTester.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Ensure client-area extension is enabled at runtime
            this.ExtendClientAreaToDecorationsHint = true;
        }

        // Password binding is handled directly via TextBox with PasswordChar in XAML

        private void MinimizeButton_Click(object? sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaxRestoreButton_Click(object? sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;
            else
                this.WindowState = WindowState.Maximized;
        }

        private void CloseButton_Click(object? sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            // Left button drag to move window
            if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                return;

            // Only start dragging the window when the click is directly on the title bar container itself.
            // If the click is on a child control (Menu, MenuItem, Button, etc.), let that control handle the event so its Command can fire.
            if (e.Source == sender)
            {
                BeginMoveDrag(e);
            }
        }
    }
}