using System.ComponentModel;
using System.Windows;

namespace ReactiveClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainViewModel viewModel;
        public MainWindow()
        {
            InitializeComponent();
            this.viewModel = new MainViewModel();
            this.DataContext = this.viewModel;
            this.Closing += MainWindow_Closing;
        }

        /// <summary>
        /// Handles the Closing event of the MainWindow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CancelEventArgs"/> instance containing the event data.</param>
        void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (this.viewModel != null)
            {
                if (!this.viewModel.IsCancelCommand && !this.viewModel.ClosingConfirmation())
                    e.Cancel = true;
            }
        }

        /// <summary>
        /// WizardListBox Preview Mouse Down event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs" /> instance containing the event data.</param>
        private void WizardListBox_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MainViewModel mainViewModel = this.DataContext as MainViewModel;

            bool result = mainViewModel.ValidateActiveStageData();
            if (!result)
            {
                e.Handled = true;
            }
        }
    }
}
