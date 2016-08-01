using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using RefreshHandler = Windows.Foundation.TypedEventHandler<Windows.UI.Xaml.DependencyObject, System.Object>;

namespace PullToRefresh.UWP
{
    internal class AttachedInfoForSv
    {
        public FrameworkElement Target;
        public ScrollViewer OriginalSv;

    }


    public sealed class PullToRefreshAttacher
    {
        private const string TEMPLATE_XNAME = "PullToRefreshScrollViewer";
        private const string DATATEMPLATE_XKEY = "DefaultPullToRefreshTopIndicator";

        private const string OUTER_SCROLLVIEWER = "OuterScrollViewer";
        private const string INNER_SCROLLVIEWER = "InnerScrollViewer";
        private const string TOP_REFRESH_INDICATOR = "TopRefreshIndicator";
        private const string GRID = "Grid";

        #region DP
        public static DataTemplate GetTopIndicatorTemplate(DependencyObject o) => (DataTemplate)o.GetValue(TopIndicatorTemplateProperty);
        public static void SetTopIndicatorTemplate(DependencyObject o, DataTemplate v) => o.SetValue(TopIndicatorTemplateProperty, v);
        public static DependencyProperty TopIndicatorTemplateProperty { get; private set; } =
            DependencyProperty.RegisterAttached("TopIndicatorTemplate", typeof(DataTemplate), typeof(PullToRefreshAttacher), 
                new PropertyMetadata(/*ThisDictionary[DATATEMPLATE_XKEY]*/null));

        public static RefreshHandler GetRefreshInvoked(DependencyObject obj) => (RefreshHandler)obj.GetValue(RefreshInvokedProperty);

        /// <summary>
        /// Setting a non-null Handler before target control is ready will apply pull-down-to-refresh.
        /// </summary>
        public static void SetRefreshInvoked(DependencyObject obj, RefreshHandler value) => obj.SetValue(RefreshInvokedProperty, value);
        public static DependencyProperty RefreshInvokedProperty { get; private set; } =
            DependencyProperty.RegisterAttached("RefreshInvoked", typeof(RefreshHandler), typeof(PullToRefreshAttacher), 
                new PropertyMetadata(null, RefreshInvokedChanged));

        public static double GetThreshold(DependencyObject obj) => (double)obj.GetValue(ThresholdProperty);
        public static void SetThreshold(DependencyObject obj, double value) => obj.SetValue(ThresholdProperty, value);
        public static DependencyProperty ThresholdProperty { get; private set; } =
            DependencyProperty.RegisterAttached("Threshold", typeof(double), typeof(PullToRefreshAttacher), new PropertyMetadata(80.0));
        
        // User does not use this.
        private static AttachedInfoForSv GetAttachedInfoForSv(ScrollViewer sv) => (AttachedInfoForSv)sv.GetValue(AttachedInfoForSvProperty);
        private static void SetAttachedInfoForSv(ScrollViewer sv, AttachedInfoForSv value) => sv.SetValue(AttachedInfoForSvProperty, value);
        private static readonly DependencyProperty AttachedInfoForSvProperty =
            DependencyProperty.RegisterAttached("AttachedInfoForSv", typeof(AttachedInfoForSv), typeof(PullToRefreshAttacher), new PropertyMetadata(null));


        #endregion

        /// <summary>
        /// Get original ScrollViewer which behaves as normal ScrollViewer, after applying PushToRefresh.
        /// </summary>
        /// <param name="sv">ScrollViewer to which PullToRefresh is applied</param>
        /// <returns>null if calling this method is not needed.</returns>
        public static ScrollViewer GetOriginalScrollViewer(ScrollViewer sv)
        {
            var infoSv = GetAttachedInfoForSv(sv);
            if (infoSv == null)
            {
                return null;
            }
            return infoSv.OriginalSv;
        }

        private static void RefreshInvokedChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                var target = (FrameworkElement)o;
                int count = VisualTreeHelper.GetChildrenCount(o);
                if (count == 0)
                {
                    // First loaded. Handle in Loaded event.
                    if (target is ScrollViewer)
                    {
                        ScrollViewer sv = (ScrollViewer)target;
                        ApplyCustomTemplate(sv, target);
                    }
                    else
                    {
                        // Before it gets loaded, VisualTreeHelper gets no children.
                        // Or it just has no children, Loaded handler will return.
                        target.Loaded += Target_Loaded;
                    }
                }
                else
                {
                    //TODO: do not support setting DP after loaded.
                    throw new NotImplementedException("Should not set Template after loaded");
                }
            }
            else
            {
                //TODO: should not set DP to null
                throw new NotImplementedException("Should not set Template to null");
            }
        }

        private static void ApplyCustomTemplate(ScrollViewer sv, FrameworkElement target)
        {
            // Loaded fired after template is applied.
            sv.Loaded += Sv_Loaded;

            AttachedInfoForSv infoSv = new AttachedInfoForSv {
                Target = target,
            };
            SetAttachedInfoForSv(sv, infoSv);

            var template = (ControlTemplate)ThisDictionary[TEMPLATE_XNAME];
            sv.Template = template;
            sv.ApplyTemplate();
        }

        private static void Target_Loaded(object sender, RoutedEventArgs e)
        {
            var target = (FrameworkElement)sender;
            target.Loaded -= Target_Loaded;
            var sv = FindScrollViewer(target);

            ApplyCustomTemplate(sv, target);
        }

        private static void Sv_Loaded(object sender, RoutedEventArgs args)
        {
            ScrollViewer sv = (ScrollViewer)sender;
            sv.Loaded -= Sv_Loaded;

            var infoSv = GetAttachedInfoForSv(sv);

            var rootBorder = (FrameworkElement)VisualTreeHelper.GetChild(sv, 0);
            var outerSv = (ScrollViewer)rootBorder.FindName(OUTER_SCROLLVIEWER);
            var grid = (FrameworkElement)outerSv.FindName(GRID);
            var topContent = (ContentControl)outerSv.FindName(TOP_REFRESH_INDICATOR);
            var inner = (ScrollViewer)outerSv.FindName(INNER_SCROLLVIEWER);

            topContent.ContentTemplate = GetTopIndicatorTemplate(infoSv.Target);

            infoSv.OriginalSv = inner;

            // Make inner ScrollViewer as large as outer ScrollViewer.
            // Since we put sv's Border out of the OuterScrollViewer.
            System.Action __AdjustInnerSize = () => {
                grid.Width = outerSv.ActualWidth;
                inner.Height = outerSv.ActualHeight;
            };

            System.Action __AdjustPlaceholders = () => {
                var newHeight = outerSv.ActualHeight;
            };

            outerSv.SizeChanged += (s, e) => {
                __AdjustPlaceholders();
                __AdjustInnerSize();
            };

            inner.Loaded += (s, e) => {
                __AdjustPlaceholders();
                __AdjustInnerSize();
            };

            topContent.Loaded += (s, e) => {
                __AdjustPlaceholders();
                __AdjustInnerSize();
            };

            inner.SizeChanged += (s, e) => {
                outerSv.ChangeView(null, topContent.ActualHeight, null, true);
            };

            topContent.SizeChanged += (s, e) => {
                // Only if height changed, do we adjust outer vertical offset.
                if (e.NewSize.Height != e.PreviousSize.Height)
                {
                    outerSv.ChangeView(null, topContent.ActualHeight, null, true);
                }
            };

            // For Desktop PC.
            inner.PointerWheelChanged += (s, e) => {
                e.Handled = true;
            };

            bool beingRefreshed = false;
            outerSv.ViewChanged += (s, e) => {
                var pt = inner.TransformToVisual(outerSv).TransformPoint(new Point(0, 0));
                var offset = pt.Y;
                var threshold = GetThreshold(infoSv.Target);

                if (!beingRefreshed)
                {
                    topContent.Content = offset / threshold;
                }

                // [IsIntermediate] gets false both on [DirectManipCompleted] and last [ViewChanged].
                // If we want to record it as being refreshed, we need to test if it's beyond threshold.
                // If it is, it means the action we lift finger, also DirectManipCompleted. Don't set [beingRefreshed] false.
                if (!e.IsIntermediate)
                {
                    beingRefreshed &= offset > threshold;
                }
            };

            outerSv.DirectManipulationCompleted += (s, e) => {
                var pt = inner.TransformToVisual(outerSv).TransformPoint(new Point(0, 0));
                var offset = pt.Y;
                var threshold = GetThreshold(infoSv.Target);
                if (offset > threshold)
                {
                    beingRefreshed = true;
                    GetRefreshInvoked(infoSv.Target)?.Invoke(infoSv.Target, null);
                }

                // With animation.
                if (!outerSv.ChangeView(null, topContent.ActualHeight, null))
                {
                    // Sometimes the first ChangeView does not work.
                    System.Action refreshAgain = async () => {
                        await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        outerSv.ChangeView(null, Math.Max(0, topContent.ActualHeight - Math.Min(1, threshold / 2)), null));
                    };
                    refreshAgain();
                }
            };
        }


        private static ScrollViewer FindScrollViewer(DependencyObject root)
        {
            ScrollViewer sv = null;

            Queue<DependencyObject> queue = new Queue<DependencyObject>();
            queue.Enqueue(root);

            while (queue.Any())
            {
                var o = queue.Dequeue();
                sv = o as ScrollViewer;
                if (sv != null)
                {
                    break;
                }
                else
                {
                    int count = VisualTreeHelper.GetChildrenCount(o);
                    for (int i = 0; i < count; i++)
                    {
                        queue.Enqueue(VisualTreeHelper.GetChild(o, i));
                    }
                }
            }

            if (sv == null)
            {
                throw new InvalidOperationException("No ScrollViewer.");
            }
            return sv;
        }

        private static ResourceDictionary ThisDictionary
        {
            get
            {
                if (_dic == null)
                {
                   /* Assembly thisAssembly = Assembly.Load(
                        new AssemblyName("PullToRefresh.UWP") {
                            Version = new Version(1, 0, 0, 0),
                            ContentType = AssemblyContentType.WindowsRuntime,
                        });
                    using (var stream = thisAssembly.GetManifestResourceStream("PullToRefresh.UWP.Style.Dictionary.xaml"))
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            var xaml = reader.ReadToEnd();
                            _dic = (ResourceDictionary)XamlReader.Load(xaml);
                        }
                    }
                    */
                }
                return _dic;
            }
        }
        private static ResourceDictionary _dic;
    }
}
