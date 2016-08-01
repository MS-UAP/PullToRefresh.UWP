using PullToRefresh.UWP.Sample.Scenarios;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PullToRefresh.UWP.Sample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var thisAsm = Assembly.Load(new AssemblyName { Name = "PullToRefresh.UWP.Sample" });
            var pageTypes = thisAsm.GetTypes()
                .Where(tp_ => tp_.GetTypeInfo().IsSubclassOf(typeof(Page)) && tp_.Namespace.EndsWith("Scenarios"));

            var src = pageTypes.Select(tp_ => new ScenarioItem(tp_));
            lv.ItemsSource = src;

            var defaultPageType = pageTypes.Where(tp_ => tp_.GetTypeInfo().GetCustomAttribute<DefaultScenarioAttribute>() != null).FirstOrDefault();
            if (defaultPageType != null)
            {
                NavigateToTestPage(defaultPageType);
            }
        }

        private void lv_ItemClick(object sender, ItemClickEventArgs e)
        {
            var si = (ScenarioItem)e.ClickedItem;
            NavigateToTestPage(si._pageType);

            split.IsPaneOpen = false;
        }

        private void NavigateToTestPage(Type pageType)
        {
            frame.BackStack.Clear();
            frame.Navigate(pageType);

            var descAttr = pageType.GetTypeInfo().GetCustomAttribute<ScenarioDescriptionAttribute>();
            var descp = descAttr == null ? "" : descAttr.Description;
            descTxt.Text = descp;
            ToolTipService.SetToolTip(descTxt, descp);
        }

        private void lvHeader_Tapped(object sender, TappedRoutedEventArgs e)
        {
            split.IsPaneOpen = !split.IsPaneOpen;
        }

        private void ham_Click(object sender, RoutedEventArgs e)
        {
            split.IsPaneOpen = true;
        }
    }

    internal class ScenarioItem
    {
        private static int s_id = 1;

        private int _id;
        internal Type _pageType;

        public ScenarioItem(Type pageType)
        {
            _id = s_id++;
            _pageType = pageType;
        }

        public int Id
        {
            get
            {
                return _id;
            }
        }

        public string Name
        {
            get
            {
                string name = _pageType.Name;
                return name;
            }
        }
    }

    public class ScenarioDescriptionAttribute : Attribute
    {
        public string Description
        {
            get;
            private set;
        }

        public ScenarioDescriptionAttribute(string desc)
        {
            Description = desc;
        }
    }

    public class DefaultScenarioAttribute : Attribute
    { }
}
