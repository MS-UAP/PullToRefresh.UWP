using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
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
    [ScenarioDescription("Incremental loading test.\nQ to refresh.")]
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class IncrementalLoading : Page
    {
        public IncrementalLoading()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs args)
        {
            base.OnNavigatedTo(args);

            lv.ItemsSource = new IncrementalLoadingSource(Dispatcher);
            KeyDown += (s, e) => {
                if (e.Key == Windows.System.VirtualKey.Q)
                {
                    PullToRefreshBox_RefreshInvoked(lv, null);
                }
            };
        }

        private void PullToRefreshBox_RefreshInvoked(DependencyObject sender, object args)
        {
            ((IList)lv.ItemsSource).Insert(0, 999);
        }
    }

    class IncrementalLoadingSource : ObservableCollection<int>, ISupportIncrementalLoading
    {
        public IncrementalLoadingSource(CoreDispatcher dsp)
        {
            _dsp = dsp;
        }

        private CoreDispatcher _dsp;

        public bool HasMoreItems
        {
            get
            {
                return true;
            }
        }

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            return Task.Run(async () => {
                await Task.Delay(1000);

                await _dsp.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                    for (int i = 0; i < count; i++)
                    {
                        this.Add(i);
                    }
                });

                return new LoadMoreItemsResult { Count = count };
            }).AsAsyncOperation();
        }
    }
}
