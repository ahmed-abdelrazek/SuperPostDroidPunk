using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using SuperPostDroidPunk.ViewModels;
using ReactiveUI;
using System.Reactive.Disposables;

namespace SuperPostDroidPunk.Views
{
    public class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public Button Button => this.FindControl<Button>("SendButton");

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            this.WhenActivated(disposables =>
            {
                // Bind the 'ExampleCommand' to 'ExampleButton' defined above.
                this.BindCommand(ViewModel, viewModel => viewModel.SendRequest, view => view.Button)
                    .DisposeWith(disposables);
            });
            AvaloniaXamlLoader.Load(this);
        }
    }
}
