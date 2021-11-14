using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml;
using SuperPostDroidPunk.ViewModels;
using SuperPostDroidPunk.Views;

namespace SuperPostDroidPunk
{
    public class App : Application
    {
        private WindowNotificationManager? _notificationManager;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
                _notificationManager = new WindowNotificationManager(desktop.MainWindow);
                _notificationManager.Position = NotificationPosition.BottomRight;
                desktop.MainWindow.DataContext = new MainWindowViewModel(_notificationManager);
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}