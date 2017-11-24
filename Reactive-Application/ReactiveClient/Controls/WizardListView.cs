using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Data;
using System.Diagnostics;

namespace ReactiveClient
{
    /// <summary>
    /// Interface IWizardItemAware
    /// </summary>
    public interface IWizardItemAware
    {
        /// <summary>
        /// Activateds this instance.
        /// </summary>
        void Activated();
        /// <summary>
        /// Movings to next stage.
        /// </summary>
        /// <returns><c>true</c> if moved, <c>false</c> otherwise.</returns>
        bool MovingToNextStage();
        /// <summary>
        /// Movings to previous stage.
        /// </summary>
        /// <returns><c>true</c> if moved, <c>false</c> otherwise.</returns>
        bool MovingToPreviousStage();
        /// <summary>
        /// Determines whether all data is valid.
        /// </summary>
        /// <returns><c>true</c> if all data is valid.; otherwise, <c>false</c>.</returns>
        bool IsAllDataValid();
    }

    /// <summary>
    /// Class WizardListBox.
    /// </summary>
    public class WizardListBox : ListBox, IDisposable
    {
        private TemplateSelector templateSelector;
        private Dictionary<object, ContentControl> loadedControls;
    
        /// <summary>
        /// Initializes a new instance of the <see cref="WizardListBox"/> class.
        /// </summary>
        public WizardListBox()
        {
            this.Loaded += WizardListBox_Loaded;
            this.SelectionChanged += WizardListBox_SelectionChanged;
            this.Unloaded += WizardListBox_Unloaded;
            
            this.CompletedIndex = -1;
            this.templateSelector = new TemplateSelector();
            this.loadedControls = new Dictionary<object, ContentControl>();
        }

        private void NextStageExecute(object parameter)
        {
            if (this.SelectedIndex < (this.Items.Count - 1))
            {
                this.SelectedIndex++;
            }
        }

        private void BackStageExecute(object parameter)
        {
            if (this.SelectedIndex > 0)
                this.SelectedIndex--;
        }

        private bool CanNextStageExecute(object parameter)
        {
            if ((this.SelectedIndex + 1) >= this.Items.Count)
                return false;

            var itemAware = this.SelectedItem as IWizardItemAware;
            if (itemAware != null && itemAware.IsAllDataValid() == false)
            {
                this.EnableSteps(this.SelectedIndex);
                return false;
            }
            else
            {
                this.EnableSteps(this.Items.Count);
            }

            return true;
        }

        private bool CanBackStageExecute(object parameter)
        {
            if (this.SelectedIndex <= 0)
                return false;

            return true;
        }

        void WizardListBox_Loaded(object sender, RoutedEventArgs e)
        {
            var wirzardList = sender as WizardListBox;
            if (wirzardList == null)
                return;

            this.LoadSelectedItemContent(this.SelectedItem);
        }

        bool insideSelectionChanged;
        void WizardListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (insideSelectionChanged)
                return;
            
            if (e.RemovedItems.Count > 0)
            {
                var aware = e.RemovedItems[0] as IWizardItemAware;
                if (aware != null)
                {
                    var awareIndex = this.Items.IndexOf(e.RemovedItems[0]);
                    bool canChange;
                    if (awareIndex > this.SelectedIndex)
                        canChange = aware.MovingToPreviousStage();
                    else
                        canChange = aware.MovingToNextStage();

                    if (!canChange)
                    {
                        this.insideSelectionChanged = true;
                        this.SelectedItem = e.RemovedItems[0];
                        this.insideSelectionChanged = false;

                        e.Handled = true;
                        return;
                    }
                }
            }

            if (e.AddedItems.Count > 0)
                LoadSelectedItemContent(e.AddedItems[0]);
        }

        private void LoadSelectedItemContent(object selectedItem)
        {
            if (this.SelectedItemContentControl != null && selectedItem != null)
            {
                // Getting existing child content control
                var childContentControl = this.SelectedItemContentControl.Content as ContentControl;
                Debug.Assert(childContentControl != null);

                // Loading the persisted content control against data context
                if (this.loadedControls.ContainsKey(selectedItem))
                {
                    this.SelectedItemContentControl.Content = this.loadedControls[selectedItem];
                }
                else
                {
                    PresistContentControlForDataContext(this, this.SelectedItemContentControl, selectedItem);
                }

                var aware = selectedItem as IWizardItemAware;
                if (aware != null)
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        aware.Activated();
                    }), System.Windows.Threading.DispatcherPriority.Loaded, null);
                }
            }
        }

        void WizardListBox_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= WizardListBox_Loaded;
            this.Unloaded -= WizardListBox_Unloaded;
        }

        /// <summary>
        /// Gets or sets the selected item content control.
        /// </summary>
        /// <value>The selected item content control.</value>
        public ContentControl SelectedItemContentControl
        {
            get { return (ContentControl)GetValue(SelectedItemContentControlProperty); }
            set { SetValue(SelectedItemContentControlProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedItemContainer.  This enables animation, styling, binding, etc...
        /// <summary>
        /// The SelectedItemContentControl property
        /// </summary>
        public static readonly DependencyProperty SelectedItemContentControlProperty =
            DependencyProperty.Register("SelectedItemContentControl", typeof(ContentControl), typeof(WizardListBox), new UIPropertyMetadata(SelectedItemContainerPropertyChangedCallback));

        static void SelectedItemContainerPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var wizardList = d as WizardListBox;
            if (wizardList == null)
                return;

            var selectedItemContentControl = e.NewValue as ContentControl;

            if (selectedItemContentControl == null)
                return;

            if (wizardList.SelectedItem != null)
                PresistContentControlForDataContext(wizardList, selectedItemContentControl, wizardList.SelectedItem);
        }

        private static void PresistContentControlForDataContext(WizardListBox wizardListBox, 
                                                                ContentControl selectedItemContentControl, 
                                                                object dataContext)
        {
            Debug.Assert(!wizardListBox.loadedControls.ContainsKey(dataContext), "Data context already exist");

            var childContentControl = new ContentControl();

            childContentControl.ContentTemplateSelector = wizardListBox.templateSelector;
            childContentControl.SetBinding(ContentControl.ContentProperty, new Binding());

            childContentControl.DataContext = dataContext;

            selectedItemContentControl.Content = childContentControl;

            wizardListBox.loadedControls.Add(dataContext, childContentControl);
        }

        /// <summary>
        /// Gets or sets the index of the completed.
        /// </summary>
        /// <value>The index of the completed.</value>
        public int CompletedIndex
        {
            get { return (int)GetValue(CompletedIndexProperty); }
            set { SetValue(CompletedIndexProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CompletedIndex.  This enables animation, styling, binding, etc...
        /// <summary>
        /// The CompletedIndex property
        /// </summary>
        public static readonly DependencyProperty CompletedIndexProperty =
            DependencyProperty.Register("CompletedIndex", typeof(int), typeof(WizardListBox), new UIPropertyMetadata(CompletedIndexPropertyChangedCallback));

        static void CompletedIndexPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var wirzardList = d as WizardListBox;
            if (wirzardList == null)
                return;
            int newIndex = (int)e.NewValue;

            wirzardList.EnableSteps(newIndex);
        }

        private void EnableSteps(int newIndex)
        {
            for (int i = 0; i < this.Items.Count; i++)
            {
                var listBoxItem = this.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;
                if (listBoxItem == null)
                    continue;

                var canEnable = i <= newIndex;
                if (canEnable && i > 0)
                {
                    var itemAware = this.Items[i - 1] as IWizardItemAware;
                    if (itemAware != null)
                    {
                        canEnable = itemAware.IsAllDataValid();
                    }
                }

                listBoxItem.IsEnabled = canEnable;
            }
        }
        
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
               // this.mNextStageCommand.Dispose();
               // this.mBackStageCommand.Dispose();
            }
        }
    }

    /// <summary>
    /// Class TemplateSelector.
    /// </summary>
    public class TemplateSelector : System.Windows.Controls.DataTemplateSelector
    {
        /// <summary>
        /// When overridden in a derived class, returns a <see cref="T:System.Windows.DataTemplate" /> based on custom logic.
        /// </summary>
        /// <param name="item">The data object for which to select the template.</param>
        /// <param name="container">The data-bound object.</param>
        /// <returns>Returns a <see cref="T:System.Windows.DataTemplate" /> or null. The default value is null.</returns>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item == null)
            {
                return null;
            }

            // Getting content control
            ContentControl contentControl = null;
            var currentItem = container;
            while (currentItem != null)
            {
                contentControl = currentItem as ContentControl;
                if (contentControl != null && contentControl.DataContext == item)
                {
                    break;
                }
                currentItem = VisualTreeHelper.GetParent(currentItem);
            }

            // Get datatmeplate resources
            currentItem = container;
            while (currentItem != null)
            {
                var frameworkElement = currentItem as FrameworkElement;
                if (frameworkElement != null)
                {
                    foreach (var resource in frameworkElement.Resources)
                    {
                        var resourceDictonary = (DictionaryEntry)resource;
                        var dataTemplate = resourceDictonary.Value as DataTemplate;
                        
                        if (dataTemplate != null)
                        {
                            var dataType = dataTemplate.DataType as Type;
                            Debug.Assert(contentControl != null, "currentItem is null");
                            if (dataType != null && dataType.IsInstanceOfType(item))
                            {
                                return dataTemplate;
                            }
                        }
                    }
                }

                currentItem = VisualTreeHelper.GetParent(currentItem);
            }

            return null;
        }
    }
}