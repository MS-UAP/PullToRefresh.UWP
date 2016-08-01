using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PullToRefresh.UWP.Sample.Scenarios
{
    [ScenarioDescription("To test this, put lots of (50+ to see difference) big images in Assets/PTRTestBigImages.\nAlso test large content scretch.")]
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UIVirtualization : Page
    {
        public UIVirtualization()
        {
            this.InitializeComponent();
        }

        public static IEnumerable<string> StaticFiles;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            try
            {
                string root = Windows.ApplicationModel.Package.Current.InstalledLocation.Path;
                var folder = await StorageFolder.GetFolderFromPathAsync(root + @"\Assets\PTRTestBigImages");
                var files = await folder.GetFilesAsync();
                if (files.Count == 0)
                {
                    ShowWarning("No images.\nAdd some images to project when test.");
                }
                else
                {
                    var images = files.Select(f_ => f_.Path);
                    StaticFiles = images;

                    btns.IsHitTestVisible = true;
                }
            }
            catch (Exception ex)
            {
                ShowWarning(ex.Message + "Have you added Assets/PTRTestBigImages folder?");
            }
        }

        private void ShowWarning(string msg)
        {
            var ignore = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                warning.Text = msg;
            });
        }

        private void ClearFrame()
        {
            if (coreFrame.Content != null)
            {
                coreFrame.Content = null;
                coreFrame.BackStack.Clear();
                coreFrame.ForwardStack.Clear();

                System.GC.AddMemoryPressure(1024 * 1024 * 1024);
                System.GC.Collect();
            }
        }

        private void noVir_Click(object sender, RoutedEventArgs e)
        {
            ClearFrame();
            var ignore = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () => {
                await Task.Delay(1000);
                coreFrame.Navigate(typeof(Virtualization.UIVirtualizationCoreNoVir));
            });
        }

        private void withVir_Click(object sender, RoutedEventArgs e)
        {
            ClearFrame();
            var ignore = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () => {
                await Task.Delay(1000);
                coreFrame.Navigate(typeof(Virtualization.UIVirtualizationCore));
            });
        }

        private void clear_Click(object sender, RoutedEventArgs e)
        {
            ClearFrame();
        }
    }
}
