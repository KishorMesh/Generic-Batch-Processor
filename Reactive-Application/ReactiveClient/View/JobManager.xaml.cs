using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ReactiveClient
{
    /// <summary>
    /// Interaction logic for JobManager.xaml
    /// </summary>
    public partial class JobManagerView : UserControl
    {
        
        public string ControlTitle
        {
            get
            {
                return "Job Manager";
            }
        }
        public JobManagerView()
        {
            InitializeComponent();
        }
    }
}
