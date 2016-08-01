using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PullToRefresh.UWP.Sample.Scenarios
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    [ScenarioDescription("Normal content.\nAlso mouse wheel/keyboard up&down event disabled.\nPress Q to refresh, 1~3 to Change top template.")]
    [DefaultScenario]
    public sealed partial class ListViewInside : Page
    {
        public ListViewInside()
        {
            this.InitializeComponent();
        }

        ObservableCollection<Color> Items = new ObservableCollection<Color>();

        protected override void OnNavigatedTo(NavigationEventArgs args)
        {
            base.OnNavigatedTo(args);

            lv.ItemsSource = Items;
            KeyDown += (s, e) => {
                if (e.Key == Windows.System.VirtualKey.Q)
                {
                    PullToRefreshBox_RefreshInvoked(lv, null);
                }
                else if (e.Key == Windows.System.VirtualKey.Number1)
                {
                    key1_Click(key1, null);
                }
                else if (e.Key == Windows.System.VirtualKey.Number2)
                {
                    key2_Click(key2, null);
                }
                else if (e.Key == Windows.System.VirtualKey.Number3)
                {
                    key3_Click(key3, null);
                }
            };
        }

        private void PullToRefreshBox_RefreshInvoked(DependencyObject sender, object args)
        {
            if (Items.Count > 7)
            {
                Items.Clear();
            }
            else
            {
                Random r = new Random();
                Items.Insert(0, Color.FromArgb(255, (byte)r.Next(), (byte)r.Next(), (byte)r.Next()));
            }
        }

        private void key1_Click(object sender, RoutedEventArgs e)
        {
            pr.TopIndicatorTemplate = (DataTemplate)Resources["dt1"];
            pr.RefreshThreshold = 150;
        }

        private void key2_Click(object sender, RoutedEventArgs e)
        { 
            pr.TopIndicatorTemplate = (DataTemplate)Resources["dt2"];
            pr.BorderThickness = new Thickness(30);
            pr.RefreshThreshold = 70;
        }

        private void key3_Click(object sender, RoutedEventArgs e)
        {
            pr.TopIndicatorTemplate = (DataTemplate)Resources["dt3"];
            pr.RefreshThreshold = 100;
        }

        private void borderThick_Click(object sender, RoutedEventArgs e)
        {
            pr.BorderThickness = new Thickness(40);
        }
    }
}
