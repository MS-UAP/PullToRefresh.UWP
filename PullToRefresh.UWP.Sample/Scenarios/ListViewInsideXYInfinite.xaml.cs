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
    [ScenarioDescription("X&Y in infinite layout.\nPress Q to refresh, W to set size.")]
    public sealed partial class ListViewInsideXYInfinite : Page
    {
        public ListViewInsideXYInfinite()
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
                else if (e.Key == Windows.System.VirtualKey.W)
                {
                    keyW_Click(keyW, null);
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

        private void keyW_Click(object sender, RoutedEventArgs e)
        {
            pr.Width = 200;
            pr.Height = 200;
        }
    }
}
