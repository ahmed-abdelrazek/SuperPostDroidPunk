using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using SuperPostDroidPunk.ViewModels;
using System.Reactive.Disposables;
using Xilium.CefGlue.Avalonia;

namespace SuperPostDroidPunk.Views
{
    public class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        private AvaloniaCefBrowser browser;

        public Button Button => this.FindControl<Button>("SendButton");

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            this.WhenActivated(disposables =>
            {
                // Bind the 'ExampleCommand' to 'ExampleButton' defined above.
                this.BindCommand(ViewModel, viewModel => viewModel.SendRequest, view => view.Button)
                    .DisposeWith(disposables);
            });

            var browserWrapper = this.FindControl<Decorator>("browserWrapper");

            browser = new AvaloniaCefBrowser
            {
                Address = "https://www.google.com"
            };
            browserWrapper.Child = browser;

        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}