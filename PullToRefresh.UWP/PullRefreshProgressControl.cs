using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace PullToRefresh.UWP
{
    public sealed class PullRefreshProgressControl : Control
    {
        private static string STATE_NORMAL = "Normal";
        private static string STATE_RELEASE = "ReleaseToRefresh";

        public PullRefreshProgressControl()
        {
            this.DefaultStyleKey = typeof(PullRefreshProgressControl);
        }

        public double Progress
        {
            get { return (double)GetValue(ProgressProperty); }
            set { SetValue(ProgressProperty, value); }
        }
        public static DependencyProperty ProgressProperty { get; private set; } =
            DependencyProperty.Register("Progress", typeof(double), typeof(PullRefreshProgressControl), new PropertyMetadata(0, ProgressChanged));


        public string PullToRefreshText
        {
            get { return (string)GetValue(PullToRefreshTextProperty); }
            set { SetValue(PullToRefreshTextProperty, value); }
        }
        public static DependencyProperty PullToRefreshTextProperty { get; private set; } =
            DependencyProperty.Register("PullToRefreshText", typeof(string), typeof(PullRefreshProgressControl), new PropertyMetadata(string.Empty));

        public string ReleaseToRefreshText
        {
            get { return (string)GetValue(ReleaseToRefreshTextProperty); }
            set { SetValue(ReleaseToRefreshTextProperty, value); }
        }
        public static DependencyProperty ReleaseToRefreshTextProperty { get; private set; } =
            DependencyProperty.Register("ReleaseToRefreshText", typeof(string), typeof(PullRefreshProgressControl), new PropertyMetadata(string.Empty));
        

        private static void ProgressChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            double newV = Convert.ToDouble(e.NewValue);
            double oldV = Convert.ToDouble(e.OldValue);
            if (newV > 1 && oldV <= 1)
            {
                VisualStateManager.GoToState((Control)o, STATE_RELEASE, true);
            }
            else if (newV <= 1 && oldV > 1)
            {
                VisualStateManager.GoToState((Control)o, STATE_NORMAL, true);
            }
        }
    }
}
