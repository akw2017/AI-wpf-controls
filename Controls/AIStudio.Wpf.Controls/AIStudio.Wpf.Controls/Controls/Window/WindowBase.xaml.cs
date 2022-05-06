﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AIStudio.Wpf.Controls.Helper;

namespace AIStudio.Wpf.Controls
{
    /// <summary>
    /// WindowBase.xaml 的交互逻辑
    /// </summary>
    [TemplatePart(Name = PART_FlyoutModal, Type = typeof(Rectangle))]
    [TemplatePart(Name = PART_OverlayBox, Type = typeof(Grid))]
    [TemplatePart(Name = PART_MetroActiveDialogContainer, Type = typeof(Grid))]
    [TemplatePart(Name = PART_MetroInactiveDialogsContainer, Type = typeof(Grid))]
    [TemplatePart(Name = PART_MainSnackbar, Type = typeof(Snackbar))]
    [TemplatePart(Name = PART_AnimationGrid, Type = typeof(Grid))]
    [TemplatePart(Name = PART_OverlayGrid, Type = typeof(Grid))]
    [TemplatePart(Name = PART_WaitingGrid, Type = typeof(Grid))]
    [TemplatePart(Name = PART_TitleBar, Type = typeof(UIElement))]

    public class WindowBase : System.Windows.Window
    {
        private const string PART_FlyoutModal = "PART_FlyoutModal";
        private const string PART_OverlayBox = "PART_OverlayBox";
        private const string PART_MetroActiveDialogContainer = "PART_MetroActiveDialogContainer";
        private const string PART_MetroInactiveDialogsContainer = "PART_MetroInactiveDialogsContainer";
        private const string PART_MainSnackbar = "PART_MainSnackbar";
        private const string PART_AnimationGrid = "PART_AnimationGrid";
        private const string PART_OverlayGrid = "PART_OverlayGrid";
        private const string PART_WaitingGrid = "PART_WaitingGrid";
        private const string PART_TitleBar = "PART_TitleBar";

        internal Grid overlayBox;
        internal Grid metroActiveDialogContainer;
        internal Grid metroInactiveDialogContainer;
        private Storyboard overlayStoryboard;
        internal Snackbar snackbar;
        internal Grid animationGrid;
        internal Grid overlayGrid;
        internal Grid waitingGrid;
        internal UIElement titleBar;
        private System.Windows.Forms.Screen screen;

        static WindowBase()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WindowBase), new FrameworkPropertyMetadata(typeof(WindowBase)));
        }

        public WindowBase()
        {
            this.WindowStyle = WindowStyle.None;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            this.Icon = new BitmapImage(new Uri("pack://application:,,,/AIStudio.Wpf.Controls;component/Resources/A.ico", UriKind.RelativeOrAbsolute));

            //12=6+6//Margin=6,Border.Effect.BlueRadius=6

            //this.MaxHeight = SystemParameters.WorkArea.Height;
            //this.MaxHeight = SystemParameters.PrimaryScreenHeight;//全屏
            WindowBase_LocationChanged(null, null);

            //bind command
            this.CloseWindowCommand = new RoutedUICommand();
            this.MaximizeWindowCommand = new RoutedUICommand();
            this.MinimizeWindowCommand = new RoutedUICommand();

            this.BindCommand(CloseWindowCommand, this.CloseCommand_Execute);
            this.BindCommand(MaximizeWindowCommand, this.MaxCommand_Execute);
            this.BindCommand(MinimizeWindowCommand, this.MinCommand_Execute);

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;

            this.LocationChanged += WindowBase_LocationChanged;
        }

        private void WindowBase_LocationChanged(object sender, EventArgs e)
        {
            var localscreen = System.Windows.Forms.Screen.FromRectangle(new System.Drawing.Rectangle((int)this.Left, (int)this.Top, (int)this.Width, (int)this.Height));
            if ((localscreen != null && screen?.DeviceName != localscreen.DeviceName) || object.ReferenceEquals(sender, "ToggleFullScreen"))
            {
                screen = localscreen;
                if (screen.Primary)
                {
                    if (ToggleFullScreen)
                    {
                        this.MaxHeight = SystemParameters.PrimaryScreenHeight + 13;//全屏
                    }
                    else
                    {
                        this.MaxHeight = SystemParameters.WorkArea.Height + 13;
                    }
                }
                else if (screen != null)
                {
                    this.MaxHeight = screen.WorkingArea.Height + 13;
                }
            }
        }

        private bool IsPrimaryScreen()
        {
            return screen.Primary;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            overlayBox = GetTemplateChild(PART_OverlayBox) as Grid;
            metroActiveDialogContainer = GetTemplateChild(PART_MetroActiveDialogContainer) as Grid;
            metroInactiveDialogContainer = GetTemplateChild(PART_MetroInactiveDialogsContainer) as Grid;
            flyoutModal = (Rectangle)GetTemplateChild(PART_FlyoutModal);
            if (flyoutModal != null)
            {
                flyoutModal.PreviewMouseDown += FlyoutsPreviewMouseDown;
                this.PreviewMouseDown += FlyoutsPreviewMouseDown;
            }
            snackbar = GetTemplateChild(PART_MainSnackbar) as Snackbar;
            animationGrid = GetTemplateChild(PART_AnimationGrid) as Grid;
            overlayGrid = GetTemplateChild(PART_OverlayGrid) as Grid;
            waitingGrid = GetTemplateChild(PART_WaitingGrid) as Grid;
            titleBar = GetTemplateChild(PART_TitleBar) as UIElement;
            if (titleBar != null)
            {
                titleBar.MouseLeftButtonDown += TitleBar_MouseLeftButtonDown;
            }
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            switch (e.ClickCount)
            {
                case 1://单击
                    {
                        this.DragMove();
                        break;
                    }
                case 2://双击
                    {
                        if (MaxboxEnable == true)
                        {
                            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
                        }
                        break;
                    }
            }
        }

        /// <summary>
        /// TitleTag,打上标记支持拖拽
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool GetTitleTag(DependencyObject obj)
        {
            return (bool)obj.GetValue(TitleTagProperty);
        }
        /// <summary>
        /// TitleTag,打上标记支持拖拽
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetTitleTag(DependencyObject obj, bool value)
        {
            obj.SetValue(TitleTagProperty, value);
        }
        /// <summary>
        /// TitleTag,打上标记支持拖拽
        /// </summary>
        public static readonly DependencyProperty TitleTagProperty =
            DependencyProperty.RegisterAttached("TitleTag", typeof(bool), typeof(WindowBase), new FrameworkPropertyMetadata(false, (d, f) => {
                var element = d as FrameworkElement;
                if (element != null)
                {
                    element.MouseLeftButtonDown += Element_MouseLeftButtonDown;
                }
            }));

        private static void Element_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var element = sender as FrameworkElement;
            WindowBase windowBase = (element as WindowBase) ?? element.TryFindParent<WindowBase>();
            if (windowBase != null)
            {
                windowBase.TitleBar_MouseLeftButtonDown(sender, e);
            }
        }

        #region 获得窗口
        /// <summary>
        /// Identifier which is used in conjunction with <see cref="Show(object)"/> to determine where a dialog should be shown.
        /// </summary>
        public string Identifier
        {
            get
            {
                return (string)GetValue(IdentifierProperty);
            }
            set
            {
                SetValue(IdentifierProperty, value);
            }
        }

        private static readonly HashSet<WindowBase> LoadedInstances = new HashSet<WindowBase>();

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            LoadedInstances.Remove(this);
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            LoadedInstances.Add(this);
        }

        public static WindowBase GetWindowBase(string windowIdentifier)
        {
            if (LoadedInstances.Count == 0)
                throw new InvalidOperationException("No loaded WindowHost instances.");
            LoadedInstances.First().Dispatcher.VerifyAccess();

            var targets = LoadedInstances.Where(dh => windowIdentifier == null || Equals(dh.Identifier, windowIdentifier)).ToList();
            if (targets.Count == 0)
                throw new InvalidOperationException("No loaded WindowHost have an Identifier property matching windowIndetifier argument.");
            if (targets.Count > 1)
                throw new InvalidOperationException("Multiple viable WindowHosts.  Specify a unique Identifier on each WindowHost, especially where multiple Windows are a concern.");

            return targets[0];
        }
        #endregion

        #region 弹出消息
        public static void ShowMessageQueue(object message, string windowIdentifier, object actionContent = null, Action actionHandler = null, ControlStatus controlStatus = ControlStatus.Default, VerticalAlignment verticalAlignment = VerticalAlignment.Bottom)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            var win = GetWindowBase(windowIdentifier);
            if (win == null) throw new ArgumentNullException(nameof(win));

            if (win.snackbar == null) throw new ArgumentNullException(nameof(win.snackbar));

            if (actionContent == null)
            {
                win.snackbar.MessageQueue.Enqueue(message, verticalAlignment: verticalAlignment, controlStatus: controlStatus);
            }
            else
            {
                win.snackbar.MessageQueue.Enqueue(message, actionContent: actionContent, actionHandler: actionHandler, verticalAlignment: verticalAlignment, controlStatus: controlStatus);
            }

        }
        #endregion

        #region 等待框

        public static void ShowWaiting(UIElement waiting, string windowIdentifier, bool showOverlay = true)
        {
            if (waiting == null) throw new ArgumentNullException(nameof(waiting));

            var win = GetWindowBase(windowIdentifier);
            if (win == null) throw new ArgumentNullException(nameof(win));

            if (win.animationGrid == null || win.overlayGrid == null || win.waitingGrid == null) throw new ArgumentNullException(nameof(waitingGrid));
            win.animationGrid.Visibility = Visibility.Visible;
            if (showOverlay)
            {
                win.overlayGrid.Visibility = Visibility.Visible;
            }
            else
            {
                win.overlayGrid.Visibility = Visibility.Collapsed;
            }
            win.waitingGrid.Children.Clear();
            win.waitingGrid.Children.Add(waiting);
        }

        public static void ShowWaiting(WaitingStyle waitingType, string windowIdentifier, Action callback, IBusyBox datacontent, string text = "系统加载中，请耐心等待", bool canCancel = false, bool showOverlay = true)
        {
            BusyBox componet = ShowWaiting(waitingType, windowIdentifier, text, showOverlay);

            if (datacontent != null)
            {
                Binding bindingPercent = new Binding("Percent") { Source = datacontent };
                componet.SetBinding(BusyBox.PercentProperty, bindingPercent);

                Binding bindingWaitInfo = new Binding("WaitInfo") { Source = datacontent };
                componet.SetBinding(BusyBox.WaitInfoProperty, bindingWaitInfo);
            }

            componet.CanCancel = canCancel;
            componet.CancelAction = () => {
                if (datacontent != null && datacontent.CancelAction != null)
                {
                    datacontent.CancelAction();
                }
                HideWaiting(windowIdentifier);
            };


            var workTask = Task.Run(() => callback.Invoke());
            workTask.ContinueWith(t => {
                Application.Current.Dispatcher.Invoke(() => {
                    HideWaiting(windowIdentifier);
                });
            });
        }

        public static BusyBox ShowWaiting(WaitingStyle waitingType, string windowIdentifier, string text, bool showOverlay = true)
        {
            BusyBox waiting = new BusyBox();
            waiting.WaitInfo = text;
            waiting.WaitingStyle = waitingType;

            var win = GetWindowBase(windowIdentifier);
            if (win == null) throw new ArgumentNullException(nameof(win));

            if (win.animationGrid == null || win.overlayGrid == null || win.waitingGrid == null) throw new ArgumentNullException(nameof(waitingGrid));
            win.animationGrid.Visibility = Visibility.Visible;
            if (showOverlay)
            {
                win.overlayGrid.Visibility = Visibility.Visible;
            }
            else
            {
                win.overlayGrid.Visibility = Visibility.Collapsed;
            }
            win.waitingGrid.Children.Clear();
            win.waitingGrid.Children.Add(waiting);

            return waiting;
        }

        public static void HideWaiting(string windowIdentifier)
        {
            var win = GetWindowBase(windowIdentifier);
            if (win == null) throw new ArgumentNullException(nameof(win));

            if (win.animationGrid == null || win.waitingGrid == null) throw new ArgumentNullException(nameof(waitingGrid));
            win.animationGrid.Visibility = Visibility.Hidden;
            win.waitingGrid.Children.Clear();
        }
        #endregion

        #region 弹出对话框
        public static event EventHandler<EventArgs> DialogOpened;
        public static event EventHandler<EventArgs> DialogClosed;

        public static Task<object> ShowDialogAsync(BaseDialog content, string windowIdentifier, bool animate = true)
        {
            var win = GetWindowBase(windowIdentifier);
            if (win == null) throw new ArgumentNullException(nameof(win));
            return win.ShowDialogAsync(content, animate);
        }


        /// <summary>
        /// Creates a LoginDialog inside of the current window.
        /// </summary>
        /// <param name="window">The window that is the parent of the dialog.</param>
        /// <param name="title">The title of the LoginDialog.</param>
        /// <param name="message">The message contained within the LoginDialog.</param>
        /// <param name="settings">Optional settings that override the global metro dialog settings.</param>
        /// <returns>The text that was entered or null (Nothing in Visual Basic) if the user cancelled the operation.</returns>
        public Task<object> ShowDialogAsync(BaseDialog dialog, bool animate)
        {
            Dispatcher.VerifyAccess();
            return HandleOverlayOnShow(animate).ContinueWith(z => {
                return (Task<object>)Dispatcher.Invoke(new Func<Task<object>>(() => {
                    SizeChangedEventHandler sizeHandler = SetupAndOpenDialog(dialog);
                    dialog.SizeChangedHandler = sizeHandler;

                    if (DialogOpened != null)
                    {
                        Dispatcher.BeginInvoke(new Action(() => DialogOpened(this, new EventArgs())));
                    }

                    return dialog.WaitForButtonPressAsync().ContinueWith(y => {
                        //once a button as been clicked, begin removing the dialog.

                        if (DialogClosed != null)
                        {
                            Dispatcher.BeginInvoke(new Action(() => DialogClosed(this, new EventArgs())));
                        }

                        return ((Task)Dispatcher.Invoke(new Func<Task>(() => {
                            SizeChanged -= sizeHandler;

                            RemoveDialog(dialog);

                            return HandleOverlayOnHide(animate);
                        }))).ContinueWith(y3 => y).Unwrap();

                    }).Unwrap();
                }));
            }).Unwrap();
        }

        private SizeChangedEventHandler SetupAndOpenDialog(UserControl dialog)
        {
            dialog.SetValue(Panel.ZIndexProperty, (int)overlayBox.GetValue(Panel.ZIndexProperty) + 1);
            dialog.MinHeight = this.ActualHeight / 4.0;
            dialog.MaxHeight = this.ActualHeight;

            SizeChangedEventHandler sizeHandler = (sender, args) => {
                dialog.MinHeight = this.ActualHeight / 4.0;
                dialog.MaxHeight = this.ActualHeight;
            };

            this.SizeChanged += sizeHandler;

            this.AddDialog(dialog);

            return sizeHandler;
        }

        private void AddDialog(UserControl dialog)
        {
            // if there's already an active dialog, move to the background
            var activeDialog = metroActiveDialogContainer.Children.Cast<UIElement>().SingleOrDefault();
            if (activeDialog != null)
            {
                metroActiveDialogContainer.Children.Remove(activeDialog);
                metroInactiveDialogContainer.Children.Add(activeDialog);
            }

            metroActiveDialogContainer.Children.Add(dialog); //add the dialog to the container}
        }

        private void RemoveDialog(UserControl dialog)
        {
            if (metroActiveDialogContainer.Children.Contains(dialog))
            {
                metroActiveDialogContainer.Children.Remove(dialog); //remove the dialog from the container

                // if there's an inactive dialog, bring it to the front
                var dlg = metroInactiveDialogContainer.Children.Cast<UIElement>().LastOrDefault();
                if (dlg != null)
                {
                    metroInactiveDialogContainer.Children.Remove(dlg);
                    metroActiveDialogContainer.Children.Add(dlg);
                }
            }
            else
            {
                metroInactiveDialogContainer.Children.Remove(dialog);
            }
        }

        public async void CloseDialog(bool animate = true)
        {
            metroInactiveDialogContainer.Children.Clear();
            metroActiveDialogContainer.Children.Clear();
            await HandleOverlayOnHide(animate);
        }
        public Task WaitForLoadAsync(bool animateShow)
        {
            Dispatcher.VerifyAccess();

            if (this.IsLoaded) return new Task(() => { });

            if (!animateShow)
                this.Opacity = 1.0; //skip the animation

            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();

            RoutedEventHandler handler = null;
            handler = (sender, args) => {
                this.Loaded -= handler;

                this.Focus();

                tcs.TrySetResult(BaseDialogResult.None);
            };

            this.Loaded += handler;

            return tcs.Task;
        }

        private Task HandleOverlayOnShow(bool animateShow)
        {
            if (metroActiveDialogContainer.Children.Count == 0)
            {
                return (animateShow ? ShowOverlayAsync() : Task.Factory.StartNew(() => Dispatcher.Invoke(new Action(ShowOverlay))));
            }
            else
            {
                var tcs = new System.Threading.Tasks.TaskCompletionSource<object>();
                tcs.SetResult(BaseDialogResult.None);
                return tcs.Task;
            }
        }

        private Task HandleOverlayOnHide(bool animateHide)
        {
            if (metroActiveDialogContainer.Children.Count == 0)
            {
                return (animateHide ? HideOverlayAsync() : Task.Factory.StartNew(() => Dispatcher.Invoke(new Action(HideOverlay))));
            }
            else
            {
                var tcs = new System.Threading.Tasks.TaskCompletionSource<object>();
                tcs.SetResult(BaseDialogResult.None);
                return tcs.Task;
            }
        }

        /// <summary>
        /// Begins to show the MetroWindow's overlay effect.
        /// </summary>
        /// <returns>A task representing the process.</returns>
        public System.Threading.Tasks.Task ShowOverlayAsync()
        {
            if (overlayBox == null) throw new InvalidOperationException("OverlayBox can not be founded in this MetroWindow's template. Are you calling this before the window has loaded?");

            var tcs = new System.Threading.Tasks.TaskCompletionSource<object>();

            if (IsOverlayVisible() && overlayStoryboard == null)
            {
                //No Task.FromResult in .NET 4.
                tcs.SetResult(null);
                return tcs.Task;
            }

            Dispatcher.VerifyAccess();

            overlayBox.Visibility = Visibility.Visible;

            var sb = (Storyboard)this.Template.Resources["OverlayFastSemiFadeIn"];

            sb = sb.Clone();

            EventHandler completionHandler = null;
            completionHandler = (sender, args) => {
                sb.Completed -= completionHandler;

                if (overlayStoryboard == sb)
                {
                    overlayStoryboard = null;
                }

                tcs.TrySetResult(BaseDialogResult.None);
            };

            sb.Completed += completionHandler;

            overlayBox.BeginStoryboard(sb);

            overlayStoryboard = sb;

            return tcs.Task;
        }
        /// <summary>
        /// Begins to hide the MetroWindow's overlay effect.
        /// </summary>
        /// <returns>A task representing the process.</returns>
        public System.Threading.Tasks.Task HideOverlayAsync()
        {
            if (overlayBox == null) throw new InvalidOperationException("OverlayBox can not be founded in this MetroWindow's template. Are you calling this before the window has loaded?");

            var tcs = new System.Threading.Tasks.TaskCompletionSource<object>();

            if (overlayBox.Visibility == Visibility.Visible && overlayBox.Opacity == 0.0)
            {
                //No Task.FromResult in .NET 4.
                tcs.SetResult(BaseDialogResult.None);
                return tcs.Task;
            }

            Dispatcher.VerifyAccess();

            var sb = (Storyboard)this.Template.Resources["OverlayFastSemiFadeOut"];

            sb = sb.Clone();

            EventHandler completionHandler = null;
            completionHandler = (sender, args) => {
                sb.Completed -= completionHandler;

                if (overlayStoryboard == sb)
                {
                    overlayBox.Visibility = Visibility.Hidden;
                    overlayStoryboard = null;
                }

                tcs.TrySetResult(BaseDialogResult.None);
            };

            sb.Completed += completionHandler;

            overlayBox.BeginStoryboard(sb);

            overlayStoryboard = sb;

            return tcs.Task;
        }
        public bool IsOverlayVisible()
        {
            if (overlayBox == null) throw new InvalidOperationException("OverlayBox can not be founded in this MetroWindow's template. Are you calling this before the window has loaded?");

            return overlayBox.Visibility == Visibility.Visible && overlayBox.Opacity >= 0.7;
        }
        public void ShowOverlay()
        {
            overlayBox.Visibility = Visibility.Visible;
            //overlayBox.Opacity = 0.7;
            overlayBox.SetCurrentValue(Grid.OpacityProperty, 0.7);
        }
        public void HideOverlay()
        {
            //overlayBox.Opacity = 0.0;
            overlayBox.SetCurrentValue(Grid.OpacityProperty, 0.0);
            overlayBox.Visibility = System.Windows.Visibility.Hidden;
        }
        #endregion

        #region Flyout
        Rectangle flyoutModal;

        public static readonly DependencyProperty FlyoutsProperty = DependencyProperty.Register("Flyouts", typeof(FlyoutsControl), typeof(WindowBase), new PropertyMetadata(null));
        public FlyoutsControl Flyouts
        {
            get
            {
                return (FlyoutsControl)GetValue(FlyoutsProperty);
            }
            set
            {
                SetValue(FlyoutsProperty, value);
            }
        }

        public static readonly RoutedEvent FlyoutsStatusChangedEvent = EventManager.RegisterRoutedEvent(
           "FlyoutsStatusChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(WindowBase));

        // Provide CLR accessors for the event
        public event RoutedEventHandler FlyoutsStatusChanged
        {
            add
            {
                AddHandler(FlyoutsStatusChangedEvent, value);
            }
            remove
            {
                RemoveHandler(FlyoutsStatusChangedEvent, value);
            }
        }

        public static readonly DependencyProperty IdentifierProperty = DependencyProperty.Register(
           nameof(Identifier), typeof(string), typeof(WindowBase), new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.Inherits));
        public static void ShowFlyout(Flyout flyout, string windowIdentifier)
        {
            if (flyout == null) throw new ArgumentNullException(nameof(flyout));

            var win = GetWindowBase(windowIdentifier);
            if (win == null) throw new ArgumentNullException(nameof(win));

            if (win.Flyouts == null)
            {
                win.Flyouts = new FlyoutsControl();
            }

            RoutedEventHandler closingFinishedHandler = null;
            closingFinishedHandler = (o, args) => {
                flyout.ClosingFinished -= closingFinishedHandler;
                win.Flyouts.Items.Remove(flyout);
            };
            flyout.ClosingFinished += closingFinishedHandler;

            win.Flyouts.Items.Add(flyout);

            flyout.IsOpen = true;
        }

        public void CloseFlyout()
        {
            if (Flyouts != null)
            {
                foreach (var flyout in Flyouts.Items.OfType<Flyout>())
                {
                    flyout.IsOpen = false;
                }
            }
        }

        internal void HandleFlyoutStatusChange(Flyout flyout, IEnumerable<Flyout> visibleFlyouts)
        {
            //checks a recently opened flyout's position.
            if (flyout.Position == Position.Left || flyout.Position == Position.Right || flyout.Position == Position.Top)
            {
                //get it's zindex
                var zIndex = flyout.IsOpen ? Panel.GetZIndex(flyout) + 3 : visibleFlyouts.Count() + 2;

                this.HandleWindowCommandsForFlyouts(visibleFlyouts);
            }

            flyoutModal.Visibility = visibleFlyouts.Any(x => x.IsModal) ? Visibility.Visible : Visibility.Hidden;

            RaiseEvent(new FlyoutStatusChangedRoutedEventArgs(FlyoutsStatusChangedEvent, this)
            {
                ChangedFlyout = flyout
            });
        }


        //todo 自动关闭BaseDiaglog
        private void FlyoutsPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var element = (e.OriginalSource as DependencyObject);
            if (element != null)
            {
                // no preview if we just clicked these elements
                if (element.TryFindParent<Flyout>() != null)
                {
                    return;
                }
            }

            if (Flyouts == null)
                return;

            if (Flyouts.OverrideExternalCloseButton == null)
            {
                foreach (var flyout in Flyouts.GetFlyouts().Where(x => x.IsOpen && x.ExternalCloseButton == e.ChangedButton && (!x.IsPinned || Flyouts.OverrideIsPinned)))
                {
                    flyout.IsOpen = false;
                    e.Handled = true;
                }
            }
            else if (Flyouts.OverrideExternalCloseButton == e.ChangedButton)
            {
                foreach (var flyout in Flyouts.GetFlyouts().Where(x => x.IsOpen && (!x.IsPinned || Flyouts.OverrideIsPinned)))
                {
                    flyout.IsOpen = false;
                    e.Handled = true;
                }
            }
        }
        #endregion

        #region CaptionHeight 标题栏高度

        public static readonly DependencyProperty CaptionHeightProperty =
            DependencyProperty.Register("CaptionHeight", typeof(double), typeof(WindowBase), new PropertyMetadata(26D));

        /// <summary>
        /// 标题高度
        /// </summary>
        public double CaptionHeight
        {
            get
            {
                return (double)GetValue(CaptionHeightProperty);
            }
            set
            {
                SetValue(CaptionHeightProperty, value);
                //this._WC.CaptionHeight = value;
            }
        }

        #endregion

        #region CaptionBackground 标题栏背景色

        public static readonly DependencyProperty CaptionBackgroundProperty = DependencyProperty.Register(
            "CaptionBackground", typeof(Brush), typeof(WindowBase), new PropertyMetadata(null));

        public Brush CaptionBackground
        {
            get
            {
                return (Brush)GetValue(CaptionBackgroundProperty);
            }
            set
            {
                SetValue(CaptionBackgroundProperty, value);
            }
        }

        #endregion

        #region CaptionForeground 标题栏前景景色

        public static readonly DependencyProperty CaptionForegroundProperty = DependencyProperty.Register(
            "CaptionForeground", typeof(Brush), typeof(WindowBase), new PropertyMetadata(null));

        public Brush CaptionForeground
        {
            get
            {
                return (Brush)GetValue(CaptionForegroundProperty);
            }
            set
            {
                SetValue(CaptionForegroundProperty, value);
            }
        }

        #endregion

        #region Header 标题栏内容模板，以提高默认模板，可自定义

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            "Header", typeof(ControlTemplate), typeof(WindowBase), new PropertyMetadata(null));

        public ControlTemplate Header
        {
            get
            {
                return (ControlTemplate)GetValue(HeaderProperty);
            }
            set
            {
                SetValue(HeaderProperty, value);
            }
        }

        #endregion

        #region MaxboxEnable 是否显示最大化按钮

        public static readonly DependencyProperty MaxboxEnableProperty = DependencyProperty.Register(
            "MaxboxEnable", typeof(bool), typeof(WindowBase), new PropertyMetadata(true));

        public bool MaxboxEnable
        {
            get
            {
                return (bool)GetValue(MaxboxEnableProperty);
            }
            set
            {
                SetValue(MaxboxEnableProperty, value);
            }
        }

        #endregion

        #region MinboxEnable 是否显示最小化按钮

        public static readonly DependencyProperty MinboxEnableProperty = DependencyProperty.Register(
            "MinboxEnable", typeof(bool), typeof(WindowBase), new PropertyMetadata(true));

        public bool MinboxEnable
        {
            get
            {
                return (bool)GetValue(MinboxEnableProperty);
            }
            set
            {
                SetValue(MinboxEnableProperty, value);
            }
        }

        #endregion

        #region CloseboxEnable 是否显示最小化按钮

        public static readonly DependencyProperty CloseboxEnableProperty = DependencyProperty.Register(
            "CloseboxEnable", typeof(bool), typeof(WindowBase), new PropertyMetadata(true));

        public bool CloseboxEnable
        {
            get
            {
                return (bool)GetValue(CloseboxEnableProperty);
            }
            set
            {
                SetValue(CloseboxEnableProperty, value);
            }
        }

        #endregion

        #region LeftWindowCommands 标题栏左侧
        public static readonly DependencyProperty LeftWindowCommandsProperty = DependencyProperty.Register(nameof(LeftWindowCommands), typeof(object), typeof(WindowBase), new PropertyMetadata(null));

        /// <summary>
        /// Gets/sets the left window commands that hosts the user commands.
        /// </summary>
        public object LeftWindowCommands
        {
            get
            {
                return (object)GetValue(LeftWindowCommandsProperty);
            }
            set
            {
                SetValue(LeftWindowCommandsProperty, value);
            }
        }
        #endregion

        #region RightWindowCommands 标题栏右侧
        public static readonly DependencyProperty RightWindowCommandsProperty = DependencyProperty.Register(nameof(RightWindowCommands), typeof(object), typeof(WindowBase), new PropertyMetadata(null));
        /// <summary>
        /// Gets/sets the right window commands that hosts the user commands.
        /// </summary>
        public object RightWindowCommands
        {
            get
            {
                return (object)GetValue(RightWindowCommandsProperty);
            }
            set
            {
                SetValue(RightWindowCommandsProperty, value);
            }
        }
        #endregion

        /****************** commands ******************/
        public ICommand CloseWindowCommand
        {
            get; protected set;
        }
        public ICommand MaximizeWindowCommand
        {
            get; protected set;
        }
        public ICommand MinimizeWindowCommand
        {
            get; protected set;
        }

        #region 托盘显示
        public ICommand ShowNotifyCommand
        {
            get; protected set;
        }
        public ICommand ClickedNotifyCommand
        {
            get; protected set;
        }
        System.Windows.Forms.NotifyIcon NotifyIcon;
        private System.Drawing.Icon blank;
        private System.Drawing.Icon normal;
        public void InitNotifyIcon()
        {
            if (NotifyIcon == null)
            {
                this.ShowNotifyCommand = new RoutedCommand();
                this.ClickedNotifyCommand = new RoutedCommand();
                this.BindCommand(ShowNotifyCommand, this.ShowNotify_Execute);
                this.BindCommand(ClickedNotifyCommand, this.ClickedNotify_Execute);

                this.NotifyIcon = new System.Windows.Forms.NotifyIcon();
                this.NotifyIcon.BalloonTipText = "双击打开";
                this.NotifyIcon.Text = this.Title;
                normal = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);
                blank = new System.Drawing.Icon(Application.GetResourceStream(new Uri("pack://application:,,,/AIStudio.Wpf.Controls;component/Resources/blank.ico", UriKind.RelativeOrAbsolute)).Stream);
                this.NotifyIcon.Icon = normal;
                this.NotifyIcon.Visible = false;
                this.NotifyIcon.MouseDoubleClick += OnNotifyIconDoubleClick;
                //this.NotifyIcon.MouseClick += OnNotifyIconMouseClick;           

                System.Windows.Forms.ContextMenuStrip contextMenu = new System.Windows.Forms.ContextMenuStrip();
                System.Windows.Forms.ToolStripMenuItem openitem = new System.Windows.Forms.ToolStripMenuItem();
                openitem.Click += OnNotifyIconDoubleClick;
                openitem.Text = "显示";
                System.Windows.Forms.ToolStripMenuItem closeitem = new System.Windows.Forms.ToolStripMenuItem();
                closeitem.Text = "退出";
                closeitem.Click += OnNotifyIconCloseClick;
                contextMenu.Items.Add(openitem);
                contextMenu.Items.Add(closeitem);
                this.NotifyIcon.ContextMenuStrip = contextMenu;
            }
            else
            {
                this.NotifyIcon.Icon = normal;
            }
        }

        private void OnNotifyIconMouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            this.NotifyIcon.ShowBalloonTip(500, "提示", "关闭闪烁", System.Windows.Forms.ToolTipIcon.Info);
        }

        private void OnNotifyIconDoubleClick(object sender, System.EventArgs e)
        {
            this.ShowInTaskbar = true;
            this.NotifyIcon.Visible = false;
            this.WindowState = WindowState.Maximized;
            this.Activate();
        }

        private void OnNotifyIconCloseClick(object sender, System.EventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void ShowNotify_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
            this.ShowInTaskbar = false;
            this.NotifyIcon.Visible = true;
            if (e != null)
            {
                e.Handled = true;
            }
        }

        private void ClickedNotify_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            OnNotifyIconDoubleClick(null, null);
            e.Handled = true;
        }

        private static void ShowNotifyIconPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var metroWindow = (WindowBase)dependencyObject;
            if (e.OldValue != e.NewValue)
            {
                var showNotifyIcon = (bool)e.NewValue;
                if (showNotifyIcon)
                {
                    metroWindow.InitNotifyIcon();

                    if (metroWindow.NotifyIcon.Visible == false)
                    {
                        metroWindow.ShowNotify_Execute(null, null);
                    }
                }
                else
                {
                    if (metroWindow.NotifyIcon != null)
                    {
                        if (metroWindow.NotifyIcon.Visible == true)
                        {
                            metroWindow.ClickedNotify_Execute(null, null);
                        }
                    }
                }
            }
        }

        private System.Timers.Timer blinkTimer;
        private static void NotifyIconBlinkPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var metroWindow = (WindowBase)dependencyObject;
            if (e.OldValue != e.NewValue)
            {
                var blink = (bool)e.NewValue;
                if (blink)
                {
                    if (metroWindow.blinkTimer == null)
                    {
                        metroWindow.blinkTimer = new System.Timers.Timer();
                        metroWindow.blinkTimer.Interval = 500;
                        metroWindow.blinkTimer.Elapsed += metroWindow.Blinking_Tick;
                    }
                    metroWindow.blinkTimer.Start();
                }
                else
                {
                    if (metroWindow.blinkTimer != null)
                    {
                        metroWindow.blinkTimer.Stop();
                        metroWindow.blinkTimer = null;
                    }
                }
            }
        }

        private bool _status = true;
        private void Blinking_Tick(object sender, EventArgs e)
        {
            if (NotifyIcon != null)
            {
                if (_status)
                    NotifyIcon.Icon = normal;
                else
                    NotifyIcon.Icon = blank;
            }
            _status = !_status;
        }
        #endregion

        #region 最大化，最小化
        private void CloseCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        private void MaxCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            e.Handled = true;
        }

        private void MinCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
            e.Handled = true;
        }

        #endregion

        #region 窗口控制
        public static readonly DependencyProperty ShowTitleBarProperty = DependencyProperty.Register(
            "ShowTitleBar", typeof(bool), typeof(WindowBase), new PropertyMetadata(true, OnShowTitleBarPropertyChangedCallback));
        public bool ShowTitleBar
        {
            get
            {
                return (bool)GetValue(ShowTitleBarProperty);
            }
            set
            {
                SetValue(ShowTitleBarProperty, value);
            }
        }

        private static void OnShowTitleBarPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var window = (WindowBase)d;
            if (e.NewValue != e.OldValue)
            {
                window.SetVisibiltyForAllTitleElements((bool)e.NewValue);
            }
        }

        private void SetVisibiltyForAllTitleElements(bool visible)
        {
            var newVisibility = visible && this.ShowTitleBar ? Visibility.Visible : Visibility.Collapsed;
            if (this.titleBar != null)
            {
                this.titleBar.Visibility = newVisibility;
            }
        }

        public static readonly DependencyProperty ToggleFullScreenProperty = DependencyProperty.Register(
            "ToggleFullScreen", typeof(bool), typeof(WindowBase), new PropertyMetadata(default(bool), ToggleFullScreenPropertyChangedCallback));

        public bool ToggleFullScreen
        {
            get
            {
                return (bool)GetValue(ToggleFullScreenProperty);
            }
            set
            {
                SetValue(ToggleFullScreenProperty, value);
            }
        }

        private static void ToggleFullScreenPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var metroWindow = (WindowBase)dependencyObject;
            if (e.OldValue != e.NewValue)
            {
                var fullScreen = (bool)e.NewValue;
                metroWindow.WindowBase_LocationChanged("ToggleFullScreen", null);
                if (fullScreen)
                {
                    metroWindow.WindowState = WindowState.Maximized;
                }
                else
                {
                    metroWindow.WindowState = WindowState.Normal;
                }
            }
        }

        public static readonly DependencyProperty ShowNotifyIconProperty = DependencyProperty.Register(
          "ShowNotifyIcon", typeof(bool), typeof(WindowBase), new PropertyMetadata(false, ShowNotifyIconPropertyChangedCallback));

        public bool ShowNotifyIcon
        {
            get
            {
                return (bool)GetValue(ShowNotifyIconProperty);
            }
            set
            {
                SetValue(ShowNotifyIconProperty, value);
            }
        }

        public static readonly DependencyProperty NotifyIconBlinkProperty = DependencyProperty.Register(
            "NotifyIconBlink", typeof(bool), typeof(WindowBase), new PropertyMetadata(false, NotifyIconBlinkPropertyChangedCallback));

        public bool NotifyIconBlink
        {
            get
            {
                return (bool)GetValue(NotifyIconBlinkProperty);
            }
            set
            {
                SetValue(NotifyIconBlinkProperty, value);
            }
        }

        #endregion

        #region 设置窗口控制
        public static bool SetWindowStatus(string status, string windowIdentifier, object value = null)
        {
            if (status == null) throw new ArgumentNullException(nameof(status));

            var win = GetWindowBase(windowIdentifier);
            if (win == null) throw new ArgumentNullException(nameof(win));

            switch (status)
            {
                case nameof(WindowState):
                    {
                        if (value is WindowState windowState)
                        {
                            win.WindowState = windowState;
                        }
                        else
                        {
                            win.WindowState = win.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
                        }
                        return win.WindowState == WindowState.Maximized;
                    }
                case nameof(ShowTitleBar):
                    {
                        if (value is bool showTitleBar)
                        {
                            win.ShowTitleBar = showTitleBar;
                        }
                        else
                        {
                            win.ShowTitleBar = !win.ShowTitleBar;
                        }
                        return win.ShowTitleBar;
                    }
                case nameof(ShowInTaskbar):
                    {
                        if (win.IsPrimaryScreen())
                        {
                            if (value is bool showInTaskbar)
                            {
                                win.ShowInTaskbar = showInTaskbar;
                            }
                            else
                            {
                                win.ShowInTaskbar = !win.ShowInTaskbar;
                            }
                        }
                        return win.ShowInTaskbar;
                    }
                case nameof(Topmost):
                    {
                        if (value is bool topmost)
                        {
                            win.Topmost = topmost;
                        }
                        else
                        {
                            win.Topmost = !win.Topmost;
                        }
                        return win.Topmost;
                    }
                case nameof(ToggleFullScreen):
                    {
                        if (value is bool toggleFullScreen)
                        {
                            win.ToggleFullScreen = toggleFullScreen;
                        }
                        else
                        {
                            win.ToggleFullScreen = !win.ToggleFullScreen;
                        }
                        return win.ToggleFullScreen;
                    }
                case nameof(ShowNotifyIcon):
                    {
                        if (value is bool showNotifyIcon)
                        {
                            win.ShowNotifyIcon = showNotifyIcon;
                        }
                        else
                        {
                            win.ShowNotifyIcon = !win.ShowNotifyIcon;
                        }
                        return win.ShowNotifyIcon;
                    }
                case nameof(NotifyIconBlink):
                    {
                        if (value is bool notifyIconBlink)
                        {
                            win.NotifyIconBlink = notifyIconBlink;
                        }
                        else
                        {
                            win.NotifyIconBlink = !win.NotifyIconBlink;
                        }
                        return win.NotifyIconBlink;
                    }
                default: return false;
            }

        }

        public static bool GetWindowStatus(string status, string windowIdentifier)
        {
            if (status == null) throw new ArgumentNullException(nameof(status));

            var win = GetWindowBase(windowIdentifier);
            if (win == null) throw new ArgumentNullException(nameof(win));

            switch (status)
            {
                case nameof(WindowState): return win.WindowState == WindowState.Maximized;
                case nameof(ShowTitleBar): return win.ShowTitleBar;
                case nameof(ShowInTaskbar): return win.ShowInTaskbar;
                case nameof(Topmost): return win.Topmost;
                case nameof(ToggleFullScreen): return win.ToggleFullScreen;
                case nameof(NotifyIconBlink): return win.NotifyIconBlink;
                default: return false;
            }
        }
        #endregion

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if (snackbar != null)
                snackbar.MessageQueue.Dispose();
        }
    }



    public class FlyoutStatusChangedRoutedEventArgs : RoutedEventArgs
    {
        internal FlyoutStatusChangedRoutedEventArgs(RoutedEvent rEvent, object source) : base(rEvent, source)
        {
        }

        public Flyout ChangedFlyout
        {
            get; internal set;
        }
    }

    /// <summary>
    /// This class eats little children.
    /// </summary>
    internal static class WindowBaseHelpers
    {
        /// <summary>
        /// Adapts the WindowCommands to the theme of the first opened, topmost &amp;&amp; (top || right || left) flyout
        /// </summary>
        /// <param name="window">The MetroWindow</param>
        /// <param name="flyouts">All the flyouts! Or flyouts that fall into the category described in the summary.</param>
        /// <param name="resetBrush">An optional brush to reset the window commands brush to.</param>
        public static void HandleWindowCommandsForFlyouts(this WindowBase window, IEnumerable<Flyout> flyouts, Brush resetBrush = null)
        {
            var allOpenFlyouts = flyouts.Where(x => x.IsOpen);

            var anyFlyoutOpen = allOpenFlyouts.Any(x => x.Position != Position.Bottom);
            if (!anyFlyoutOpen)
            {
                if (resetBrush == null)
                {
                    window.ResetAllWindowCommandsBrush();
                }
                else
                {
                    window.ChangeAllWindowCommandsBrush(resetBrush);
                }
            }

            var topFlyout = allOpenFlyouts
                .Where(x => x.Position == Position.Top)
                .OrderByDescending(Panel.GetZIndex)
                .FirstOrDefault();
            if (topFlyout != null)
            {
                window.UpdateWindowCommandsForFlyout(topFlyout);
            }
            else
            {
                var leftFlyout = allOpenFlyouts
                    .Where(x => x.Position == Position.Left)
                    .OrderByDescending(Panel.GetZIndex)
                    .FirstOrDefault();
                if (leftFlyout != null)
                {
                    window.UpdateWindowCommandsForFlyout(leftFlyout);
                }
                var rightFlyout = allOpenFlyouts
                    .Where(x => x.Position == Position.Right)
                    .OrderByDescending(Panel.GetZIndex)
                    .FirstOrDefault();
                if (rightFlyout != null)
                {
                    window.UpdateWindowCommandsForFlyout(rightFlyout);
                }
            }
        }

        public static void ResetAllWindowCommandsBrush(this WindowBase window)
        {

        }

        public static void UpdateWindowCommandsForFlyout(this WindowBase window, Flyout flyout)
        {
            window.ChangeAllWindowCommandsBrush(flyout.Foreground, flyout.Position);
        }

        private static void InvokeActionOnWindowCommands(this WindowBase window, Action<Control> action1, Action<Control> action2 = null, Position position = Position.Top)
        {

        }

        private static void ChangeAllWindowCommandsBrush(this WindowBase window, Brush brush, Position position = Position.Top)
        {

        }
    }



}