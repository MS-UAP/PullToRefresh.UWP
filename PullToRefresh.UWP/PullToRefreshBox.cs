using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using RefreshHandler = Windows.Foundation.TypedEventHandler<Windows.UI.Xaml.DependencyObject, System.Object>;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace PullToRefresh.UWP
{
    public sealed class PullToRefreshBox : ContentControl
    {
        private const string OUTER_SCROLLVIEWER = "OuterScrollViewer";
        private const string INNER = "Inner";
        private const string TOP_REFRESH_INDICATOR = "TopRefreshIndicator";
        private const string GRID = "Grid";
        private const string CANVAS = "Canvas";

        public static DependencyProperty TopIndicatorTemplateProperty { get; private set; } =
            DependencyProperty.Register("TopIndicatorTemplate", typeof(DataTemplate), typeof(PullToRefreshBox), new PropertyMetadata(null));
        /// <summary>
        /// This's DataContext will be progress of pulling down, in percentage. The threshold will be 1.0.
        /// </summary>
        public DataTemplate TopIndicatorTemplate
        {
            get { return (DataTemplate)GetValue(TopIndicatorTemplateProperty); }
            set { SetValue(TopIndicatorTemplateProperty, value); }
        }

        public static DependencyProperty RefreshThresholdProperty { get; private set; } =
            DependencyProperty.Register("RefreshThreshold", typeof(double), typeof(PullToRefreshBox), new PropertyMetadata(80.0, OnRefreshThresholdChanged));
        /// <summary>
        /// Default distance is 80.
        /// </summary>
        /// <remarks>
        /// Must be greater than 0.
        /// </remarks>
        public double RefreshThreshold
        {
            get { return (double)GetValue(RefreshThresholdProperty); }
            set { SetValue(RefreshThresholdProperty, value); }
        }
        private static void OnRefreshThresholdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((double)e.NewValue <= 0)
            {
                throw new ArgumentOutOfRangeException("RefreshThreshold must be greater than 0.");
            }
        }

        /// <summary>
        /// Sender is current Content. Args is always null.
        /// </summary>
        public event RefreshHandler RefreshInvoked;

        private static readonly HashSet<VirtualKey> HANDLING_KEYS = new HashSet<VirtualKey> {
            VirtualKey.Up, VirtualKey.Down, VirtualKey.PageUp, VirtualKey.PageDown, VirtualKey.Home, VirtualKey.End
        };

        public PullToRefreshBox()
        {
            this.DefaultStyleKey = typeof(PullToRefreshBox);
        }

        private ScrollViewer _outerSv;
        private FrameworkElement _inner;
        private FrameworkElement _grid;
        private ContentControl _topContent;
        private FrameworkElement _canvas;

        protected override Size MeasureOverride(Size availableSize)
        {
            bool inf = true;
            if (!DesignMode.DesignModeEnabled)
            {
                inf = double.IsInfinity(availableSize.Height);
            }

            if (_canvas != null)
            {
                _canvas.Visibility = inf ? Visibility.Collapsed : Visibility.Visible;
            }

            if (_grid != null)
            {
                double contentWidth = Math.Max(0, availableSize.Width - BorderThickness.Left - BorderThickness.Right);
                _grid.Width = double.IsInfinity(contentWidth) ? double.NaN : contentWidth;
            }
            if (_inner != null)
            {
                double contentHeight = Math.Max(0, availableSize.Height - BorderThickness.Top - BorderThickness.Bottom);
                _inner.Height = double.IsInfinity(contentHeight) ? double.NaN : contentHeight;
            }

            return base.MeasureOverride(availableSize);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _outerSv = GetTemplateChild(OUTER_SCROLLVIEWER) as ScrollViewer;
            _inner = GetTemplateChild(INNER) as FrameworkElement;
            _grid = GetTemplateChild(GRID) as FrameworkElement;
            _canvas = GetTemplateChild(CANVAS) as FrameworkElement;
            _topContent = GetTemplateChild(TOP_REFRESH_INDICATOR) as ContentControl;


            if (!DesignMode.DesignModeEnabled)
            {
                if (_outerSv != null)
                {
                    // Swallow events for Desktop PC.
                    _outerSv.Loaded += _outerSv_Loaded;

                    // Refresh related action events.
                    _outerSv.ViewChanged += _outerSv_ViewChanged;
                    _outerSv.DirectManipulationCompleted += _outerSv_DirectManipulationCompleted;
                }

                if (_inner != null)
                {
                    _inner.SizeChanged += async (s, e) => {
                        await Dispatch(AdjustOuterOffset);
                    };
                }

                if (_topContent != null)
                {
                    // TopContent does not influence Inner size, just adjust outer offset.
                    _topContent.Loaded += async (s, e) => {
                        await Dispatch(AdjustOuterOffset);
                    };
                    _topContent.SizeChanged += async (s, e) => {
                        // Only if height changed, do we adjust outer vertical offset.
                        if (e.NewSize.Height != e.PreviousSize.Height)
                        {
                            if (_canvas != null)
                            {
                                _canvas.Height = e.NewSize.Height;
                            }
                            await Dispatch(AdjustOuterOffset);
                        }
                    };
                }
            }
        }


        /// <summary>
        /// Only when OuterSv is loaded, its visual children get accessable.
        /// We should swallow MouseWheel and some Key events at ScrollViewer's root Border,
        /// in case of OuterSv gets aware of these event.
        /// 
        /// Because OuterSv and its Border are out of ScrollContentPresenter surface,
        /// so handling these events in any child of OuterSv, does not cover those in outer Border.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _outerSv_Loaded(object sender, RoutedEventArgs args)
        {
            if (VisualTreeHelper.GetChildrenCount(_outerSv) == 0)
            {
                return;
            }
            var outerRoot = VisualTreeHelper.GetChild(_outerSv, 0) as FrameworkElement;
            if (outerRoot == null)
            {
                return;
            }

            outerRoot.PointerWheelChanged += (s, e) => {
                e.Handled = !IsInf() && true;
            };

            outerRoot.KeyDown += (s, e) => {
                // Even it's in infinite layout, do not allow KeyEvent to pass.
                // Because default ScrollViewer will show empty area above top to indicate it comes to end.
                // We do not want this effect.
                if (HANDLING_KEYS.Contains(e.Key))
                {
                    e.Handled = true;
                }
            };
        }

        private bool IsInf()
        {
            if (_canvas == null)
            {
                return true;
            }
            return _canvas.Visibility == Visibility.Collapsed;
        }

        private bool _sysScrolling = false;
        private bool AdjustOuterOffset()
        {
            if (_outerSv == null)
            {
                return true;
            }

            double actualHeight = 0;
            if (_topContent != null)
            {
                actualHeight = _topContent.ActualHeight;
            }

            bool scroll = _outerSv.ChangeView(null, actualHeight, null, true);
            if (scroll)
            {
                _sysScrolling = true;
            }
            return scroll;
        }

        private async Task Dispatch(Func<bool> action, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal)
        {
            if (!action())
            {
                await Dispatcher.RunAsync(priority, () => action());
            }
        }

        private void SetTopContentPercent(double per)
        {
            if (_topContent != null)
            {
                _topContent.DataContext = per;
            }
        }

        private bool _beingRefreshed = false;
        private void _outerSv_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            // ChangeView works async.
            if (_sysScrolling)
            {
                _sysScrolling = false;
                SetTopContentPercent(0);
                return;
            }

            var offset = CountInnerOuterOffset();
            var threshold = RefreshThreshold;

            if (!_beingRefreshed)
            {
                // In process of pulling down.
                SetTopContentPercent(offset / threshold);
            }
            else
            {
                // Have released and being drawing back.
            }

            // [IsIntermediate] gets false both on [DirectManipCompleted] and last [ViewChanged].
            // If we want to record it as being refreshed, we need to test if it's beyond threshold.
            // If it is, it means the action we lift finger, also DirectManipCompleted. Don't set [beingRefreshed] false.
            if (!e.IsIntermediate)
            {
                // End, also not manipulated.
                if (_beingRefreshed && offset < threshold)
                {
                    // Clear progress.
                    SetTopContentPercent(0);
                }
                _beingRefreshed &= offset > threshold;

                //! When lifting finger, this occurs first, then is ManipCompleted.
            }

            // Because scrolling back has accelaration, sometimes progress does not get cleared immediately.
            // But we should keep end test above, in case of it could also scroll back very fast.
            if (e.IsIntermediate && _beingRefreshed)
            {
                if (offset / threshold < 0.2)
                {
                    _beingRefreshed = false;
                    SetTopContentPercent(offset / threshold);
                }
            }
        }

        private void _outerSv_DirectManipulationCompleted(object sender, object e)
        {
            var offset = CountInnerOuterOffset();

            var threshold = RefreshThreshold;
            if (offset > threshold)
            {
                if (!_beingRefreshed)
                {
                    _beingRefreshed = true;
                    RefreshInvoked?.Invoke(Content as DependencyObject, null);
                }
            }

            /*
            double actualHeight = 0;
            if (_topContent != null)
            {
                actualHeight = _topContent.ActualHeight;
            }

            // With animation.
            if (!_outerSv.ChangeView(null, actualHeight, null))
            {
                // Sometimes the first ChangeView does not work.
                System.Action changeAgain = async () => {
                    await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                        double actualHeight2 = 0;
                        if (_topContent != null)
                        {
                            actualHeight2 = _topContent.ActualHeight;
                        }
                        _outerSv.ChangeView(null, actualHeight2, null);
                    });
                };
                changeAgain();
            }*/

            var ignore = Dispatch(() => {
                double actualHeight = 0;
                if (_topContent != null)
                {
                    actualHeight = _topContent.ActualHeight;
                }
                return _outerSv.ChangeView(null, actualHeight, null);
            });
        }


        private double CountInnerOuterOffset()
        {
            if (_inner == null || _outerSv == null)
            {
                return 0;
            }

            var pt = _inner.TransformToVisual(_outerSv).TransformPoint(new Point(0, 0));

            // We must care about outer's BorderThickness.
            var offset = Math.Max(0, pt.Y - _outerSv.BorderThickness.Top);
            return offset;
        }

    }
}
