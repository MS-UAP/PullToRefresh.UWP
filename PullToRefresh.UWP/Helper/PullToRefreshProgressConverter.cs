using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace PullToRefresh.UWP.Helper
{
    internal class PullToRefreshProgressConverter : IValueConverter
    {
        /// <summary>
        /// Count the default circular progress ring DashArray.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="language"></param>
        /// <returns>DoubleCollection for StrokeDashArray</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            double minDisPer = 0.2;
            double per = 0;
            if (value != null)
            {
                try
                {
                    per = System.Convert.ToDouble(value);
                }
                catch (Exception e)
                {
                    // TODO Auto-generated catch block
                    System.Diagnostics.Debug.WriteLine(e.Message);
                }

            }
            
            double dash = Math.Min(1, Math.Max(0, per - minDisPer) / (1 - minDisPer)) * 87;
            return new DoubleCollection { dash, 200 };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
