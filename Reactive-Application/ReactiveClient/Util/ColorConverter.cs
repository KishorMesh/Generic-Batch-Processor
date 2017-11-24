using API;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ReactiveClient
{
    public class ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            JobStatus status = (JobStatus)Enum.Parse(typeof(JobStatus), value.ToString());
            switch (status)
            {                
                case JobStatus.Started:
                    return Brushes.Orange;
                case JobStatus.Completed:
                    return Brushes.Green;
                case JobStatus.Failed:
                    return Brushes.Red;
                case JobStatus.Cancelled:
                    return Brushes.Cyan;
                case JobStatus.NotStarted:
                    return Brushes.White;
                case JobStatus.Invalid:
                    return Brushes.Yellow;
                case JobStatus.Timeout:
                    return Brushes.Purple;
                default:
                    return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
