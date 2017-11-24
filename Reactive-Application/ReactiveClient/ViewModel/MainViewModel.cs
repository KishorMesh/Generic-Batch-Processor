using Microsoft.Practices.Prism.Commands;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace ReactiveClient
{
    public class MainViewModel : BaseViewModel
    {
        #region Fields

        /// <summary>
        /// Represents current index of tab control.
        /// </summary>
        private int currentIndex = 0;

        /// <summary>
        /// Next button enabled flag.
        /// </summary>
        private bool isNextButtonEnabled;

        /// <summary>
        /// Back button enabled flag.
        /// </summary>
        private bool isBackButtonEnabled;
        

        /// <summary>
        /// Represents the Child View Models.
        /// </summary>
        private Collection<BaseViewModel> childViewModels;

        /// <summary>
        /// Represents the Current View Model.
        /// </summary>
        private BaseViewModel currentViewModel;

        /// <summary>
        /// Represents the Completed Stage Number.
        /// </summary>
        private int completedStage;

        /// <summary>
        /// The is busy
        /// </summary>
        private bool isBusy;

        /// <summary>
        /// Back command.
        /// </summary>
        private ICommand backCommand;

        /// <summary>
        /// Next command.
        /// </summary>
        private ICommand nextCommand;
        /// <summary>
        /// Cancel command.
        /// </summary>
        private ICommand cancelCommand;

        /// <summary>
        /// The is cancel command
        /// </summary>
        private bool isCancelCommand;

        /// <summary>
        /// Staus bar messages
        /// </summary>
        private string statusMessage;
        #endregion


        #region Properties

        public string StatusMessage
        {
            get { return this.statusMessage; }
            set
            {
                this.statusMessage = value;
                this.RaisePropertyChanged("StatusMessage");
            }
        }

        /// <summary>
        /// Gets or sets child view models.
        /// </summary>
        /// <value>
        /// Holds list of view model.
        /// </value>
        public Collection<BaseViewModel> ChildViewModels
        {
            get { return this.childViewModels; }
            private set { this.childViewModels = value; }
        }

        /// <summary>
        /// Gets or sets Current view models.
        /// </summary>
        /// <value>
        /// The current view model.
        /// </value>
        public BaseViewModel CurrentViewModel
        {
            get
            {
                return this.currentViewModel;
            }
            set
            {
                bool result = this.ValidateActiveStageData();
                if (result)
                {                    
                    this.currentViewModel = value;
                    this.SetCurrentIndex(this.currentViewModel);
                    this.SetButtonStatus();
                    this.RaisePropertyChanged("CurrentViewModel");
                    UpdateStaus();
                }
            }
        }

       

        /// <summary>
        /// Gets or sets current index of tab control.
        /// </summary>
        /// <value>
        /// Holds current index of tab control.
        /// </value>
        public int CurrentIndex
        {
            get
            {
                return this.currentIndex;
            }
            set
            {
                bool result = this.ValidateActiveStageData();
                if (result)
                {
                    this.currentIndex = value;
                    this.CurrentViewModel = this.ChildViewModels[this.currentIndex];
                    this.RaisePropertyChanged("CurrentIndex");
                }
            }
        }

        /// <summary>
        /// Gets or sets Completed Stage.
        /// </summary>
        /// <value>
        /// The completed stage.
        /// </value>
        public int CompletedStage
        {
            get
            {
                return this.completedStage;
            }
            private set
            {
                if (this.completedStage != value)
                {
                    this.completedStage = value;
                    this.RaisePropertyChanged("CompletedStage");
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [is busy].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is busy]; otherwise, <c>false</c>.
        /// </value>
        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                isBusy = value;
                this.RaisePropertyChanged("IsBusy");
            }
        }
        /// <summary>
        /// Gets next command.
        /// </summary>
        /// <value>
        /// Holds next button command.
        /// </value>
        public ICommand NextCommand
        {
            get
            {
                if (this.nextCommand == null)
                {
                    this.nextCommand = new DelegateCommand(ExecuteNextCommand);
                }

                return this.nextCommand;
            }
        }

        /// <summary>
        /// Gets a back command
        /// </summary>
        /// <value>
        /// Holds back button command.
        /// </value>
        public ICommand BackCommand
        {
            get
            {
                if (this.backCommand == null)
                {
                    this.backCommand = new DelegateCommand(ExecuteBackCommand);
                }

                return this.backCommand;
            }
        }

        /// <summary>
        /// Gets a back command
        /// </summary>
        /// <value>
        /// Holds back button command.
        /// </value>
        public ICommand CancelCommand
        {
            get
            {
                if (this.cancelCommand == null)
                {
                    this.cancelCommand = new DelegateCommand(CloseWindow);
                }

                return this.cancelCommand;
            }
        }

        /// <summary>
        /// Gets a value indicating whether [is cancel command].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is cancel command]; otherwise, <c>false</c>.
        /// </value>
        public bool IsCancelCommand
        {
            get { return isCancelCommand; }
        }

        /// <summary>
        /// Next command execution.
        /// </summary>
        public void ExecuteNextCommand()
        {
            if (this.CurrentIndex < this.ChildViewModels.Count - 1)
            {
                this.CurrentIndex++;
            }
        }

        /// <summary>
        /// Back command execution.
        /// </summary>
        public void ExecuteBackCommand()
        {
            if (this.CurrentIndex > 0)
            {
                this.CurrentIndex--;
            }
        }

       

        /// <summary>
        /// Closes the window.
        /// </summary>
        public void CloseWindow()
        {
            if (ClosingConfirmation())
            {
                this.isCancelCommand = true;
                Application.Current.Shutdown();
            }
        }
        
        /// <summary>
        /// Closings the confirmation.
        /// </summary>
        /// <returns></returns>
        public bool ClosingConfirmation()
        {
            bool closingStatus = false;
            MessageBoxResult result = MessageBox.Show("Do you want to close the application.", "Batch Processor Client", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
            {
                closingStatus = true;
            }           
            return closingStatus;
        }


        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowViewModel" /> class.
        /// </summary>
        public MainViewModel()
        {
            AddChildViewModels();
            this.IsNextButtonEnabled = true;
            this.IsBackButtonEnabled = false;
           
            this.CurrentViewModel = this.childViewModels[0];
        }

        #endregion

        #region Methods

        /// <summary>
        /// It is used to add view models and set current collection to another view model.
        /// </summary>
        private void AddChildViewModels()
        {
            this.ChildViewModels = new Collection<BaseViewModel>();

            JobManagerViewModel jobManagerViewModel = new JobManagerViewModel(this);
            this.ChildViewModels.Add(jobManagerViewModel);
            SummaryViewModel summaryViewModel = new SummaryViewModel(this, jobManagerViewModel);
            this.ChildViewModels.Add(summaryViewModel);          

            this.CompletedStage = this.ChildViewModels.Count - 1;
        }

        /// <summary>
        /// Validate All the required data for the wizard
        /// </summary>
        /// <returns>
        ///   <c>true</c> if XXXX, <c>false</c> otherwise.
        /// </returns>
        public bool ValidateActiveStageData()
        {           
            return true;
        }

        /// <summary>
        /// Set Current Index for active control.
        /// </summary>
        /// <param name="activeVM">The active vm.</param>
        private void SetCurrentIndex(BaseViewModel activeVM)
        {
            for (int i = 0; i < ChildViewModels.Count; i++)
            {
                if (ChildViewModels[i] == activeVM && i != this.currentIndex)
                {
                    this.currentIndex = i;
                    break;
                }
            }
        }

        /// <summary>
        /// Gets or sets visibilty of Next button.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is next button enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsNextButtonEnabled
        {
            get
            {
                return this.isNextButtonEnabled;
            }
            set
            {
                this.isNextButtonEnabled = value;
                RaisePropertyChanged("IsNextButtonEnabled");
            }
        }

        /// <summary>
        /// Gets or sets visibilty of Back button.
        /// </summary>
        /// <value>
        /// <c>true</c> if [is back button enabled]; otherwise, <c>false</c>.
        /// </value>
        public bool IsBackButtonEnabled
        {
            get
            {
                return this.isBackButtonEnabled;
            }
            set
            {
                this.isBackButtonEnabled = value;
                RaisePropertyChanged("IsBackButtonEnabled");
            }
        }

       

        /// <summary>
        /// It sets status of back and next button
        /// </summary>
        private void SetButtonStatus()
        {
            this.IsBackButtonEnabled = (this.currentIndex == 0) ? false : true;
            this.IsNextButtonEnabled = (this.currentIndex == this.ChildViewModels.Count - 1) ? false : true;
        }
        private void UpdateStaus()
        {
            if (this.currentViewModel is JobManagerViewModel)
            {
                this.StatusMessage = "Display job details...";
            }
            if (this.currentViewModel is SummaryViewModel)
            {
                this.StatusMessage = "Display summary details...";
                (this.currentViewModel as SummaryViewModel).AddTaskDataforView();
            }
        }
        #endregion
    }
}
