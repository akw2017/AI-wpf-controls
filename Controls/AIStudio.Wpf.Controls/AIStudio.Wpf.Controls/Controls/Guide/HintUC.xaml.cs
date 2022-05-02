﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AIStudio.Wpf.Controls
{
    [TemplatePart(Name = PART_Viewbox, Type = typeof(Viewbox))]
    [TemplatePart(Name = PART_Padgrid, Type = typeof(Grid))]
    [TemplatePart(Name = PART_Btn_close, Type = typeof(Button))]
    [TemplatePart(Name = PART_Tb_xh, Type = typeof(TextBlock))]
    [TemplatePart(Name = PART_Tb_nr, Type = typeof(TextBlock))]
    [TemplatePart(Name = PART_Btn_stack, Type = typeof(StackPanel))]
    [TemplatePart(Name = PART_Btn_pre, Type = typeof(Button))]
    [TemplatePart(Name = PART_Btn_next, Type = typeof(Button))]
    /// <summary>
    /// HintUC.xaml 的交互逻辑
    /// </summary>
    public class HintUC : Control
    {
        private const string PART_Viewbox = "PART_Viewbox";
        private const string PART_Padgrid = "PART_Padgrid";
        private const string PART_Btn_close = "PART_Btn_close";
        private const string PART_Tb_xh = "PART_Tb_xh";
        private const string PART_Tb_nr = "PART_Tb_nr";
        private const string PART_Btn_stack = "PART_Btn_stack";
        private const string PART_Btn_pre = "PART_Btn_pre";
        private const string PART_Btn_next = "PART_Btn_next";

        Window _win;
        Point _point;
        FrameworkElement _fe;

        Viewbox viewbox;
        Grid padgrid;
        Button btn_close;
        TextBlock tb_xh;
        TextBlock tb_nr;
        StackPanel btn_stack;
        Button btn_pre;
        Button btn_next;

        #region Title 标题
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(HintUC), new PropertyMetadata(null));
        /// <summary>
        /// 标题
        /// </summary>
        public string Title
        {
            get
            {
                return (string)GetValue(TitleProperty);
            }
            set
            {
                SetValue(TitleProperty, value);
            }
        }
        #endregion

        #region Content 内容
        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register(nameof(Content), typeof(string), typeof(HintUC), new PropertyMetadata(null));
        /// <summary>
        /// 内容
        /// </summary>
        public string Content
        {
            get
            {
                return (string)GetValue(ContentProperty);
            }
            set
            {
                SetValue(ContentProperty, value);
            }
        }
        #endregion

        #region NextContent 上一步按钮是否显示
        public static readonly DependencyProperty PreVisibilityProperty =
            DependencyProperty.Register(nameof(PreVisibility), typeof(Visibility), typeof(HintUC), new PropertyMetadata(Visibility.Visible));
        /// <summary>
        /// 下一步按钮内容
        /// </summary>
        public Visibility PreVisibility
        {
            get
            {
                return (Visibility)GetValue(PreVisibilityProperty);
            }
            set
            {
                SetValue(PreVisibilityProperty, value);
            }
        }
        #endregion

        #region NextContent 下一步按钮内容
        public static readonly DependencyProperty NextContentProperty =
            DependencyProperty.Register(nameof(NextContent), typeof(string), typeof(HintUC), new PropertyMetadata("下一步"));
        /// <summary>
        /// 下一步按钮内容
        /// </summary>
        public string NextContent
        {
            get
            {
                return (string)GetValue(NextContentProperty);
            }
            set
            {
                SetValue(NextContentProperty, value);
            }
        }
        #endregion

        static HintUC()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HintUC), new FrameworkPropertyMetadata(typeof(HintUC)));
        }

        public HintUC(Window win, Point point, FrameworkElement fe, string xh, string content, int? width = null, int? height = null, bool first = false, bool last = false)
        {
            this._win = win;
            this._point = point;
            this._fe = fe;

            if (width != null)
            {
                this.Width = width.Value;
            }
            if (height != null)
            {
                this.Height = height.Value;
            }

            Title = xh;
            Content = content;

            if (first)
            {
                PreVisibility = Visibility.Collapsed;
            }
            if (last)
            {
                NextContent = "完成";
            }

            this.Loaded += HintUC_Loaded;
        }

        private void HintUC_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= HintUC_Loaded;
            double left1 = _point.X + _fe.ActualWidth + 5;
            double left2 = _point.X - this.ActualWidth - 5;
            double top1 = _point.Y + _fe.ActualHeight - 12;
            double top2 = _point.Y - this.ActualHeight + 12;

            if (this.ActualWidth + left1 <= _win.Width && this.ActualHeight + top1 <= _win.Height)
            {
                Canvas.SetLeft(this, left1);
                Canvas.SetTop(this, top1);
            }
            else if (this.ActualWidth + left1 <= _win.Width && top2 >= 0)
            {
                Canvas.SetLeft(this, left1);
                Canvas.SetTop(this, top2);

                ScaleTransform scaleTransform = new ScaleTransform();
                scaleTransform.ScaleY = -1;
                viewbox.RenderTransform = scaleTransform;
            }
            else if (left2 >= 0 && this.ActualHeight + top1 <= _win.Height)
            {
                Canvas.SetLeft(this, left2);
                Canvas.SetTop(this, top1);

                ScaleTransform scaleTransform = new ScaleTransform();
                scaleTransform.ScaleX = -1;
                viewbox.RenderTransform = scaleTransform;

                DockPanel.SetDock(padgrid, Dock.Right);
                //btn_close.HorizontalAlignment = HorizontalAlignment.Left;
                //tb_xh.HorizontalAlignment = HorizontalAlignment.Right;
                //btn_stack.HorizontalAlignment = HorizontalAlignment.Left;
            }
            else if (left2 >= 0 && top2 >= 0)
            {
                Canvas.SetLeft(this, left2);
                Canvas.SetTop(this, top2);

                ScaleTransform scaleTransform = new ScaleTransform();
                scaleTransform.ScaleX = -1;
                scaleTransform.ScaleY = -1;
                viewbox.RenderTransform = scaleTransform;

                DockPanel.SetDock(padgrid, Dock.Right);
                //btn_close.HorizontalAlignment = HorizontalAlignment.Left;
                //tb_xh.HorizontalAlignment = HorizontalAlignment.Right;
                //btn_stack.HorizontalAlignment = HorizontalAlignment.Left;
            }
            else//怎么放都不行，就按第一种放吧
            {
                Canvas.SetLeft(this, left1);
                Canvas.SetTop(this, top1);
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (btn_next != null)
            {
                btn_next.Click -= new RoutedEventHandler(btn_next_Click);
            }
            if (btn_pre != null)
            {
                btn_pre.Click -= new RoutedEventHandler(btn_pre_Click);
            }
            if (btn_close != null)
            {
                btn_close.Click -= new RoutedEventHandler(btn_close_Click);
            }

            viewbox = GetTemplateChild(PART_Viewbox) as Viewbox;
            padgrid = GetTemplateChild(PART_Padgrid) as Grid;
            btn_close = GetTemplateChild(PART_Btn_close) as Button;
            tb_xh = GetTemplateChild(PART_Tb_xh) as TextBlock;
            tb_nr = GetTemplateChild(PART_Tb_nr) as TextBlock;
            btn_stack = GetTemplateChild(PART_Btn_stack) as StackPanel;
            btn_pre = GetTemplateChild(PART_Btn_pre) as Button;
            btn_next = GetTemplateChild(PART_Btn_next) as Button;

            if (btn_next != null)
            {
                btn_next.Click += new RoutedEventHandler(btn_next_Click);
            }
            if (btn_pre != null)
            {
                btn_pre.Click += new RoutedEventHandler(btn_pre_Click);
            }
            if (btn_close != null)
            {
                btn_close.Click += new RoutedEventHandler(btn_close_Click);
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            this.btn_next.Focus();
        }

        public delegate void NextHintDelegate();
        public event NextHintDelegate nextHintEvent;
        private void btn_next_Click(object sender, RoutedEventArgs e)
        {
            if (this.btn_next.Content.ToString() != "完成")
            {
                nextHintEvent();
            }
            else
            {
                Window.GetWindow(this).Close();
            }
        }

        public delegate void PreHintDelegate();
        public event PreHintDelegate preHintEvent;
        private void btn_pre_Click(object sender, RoutedEventArgs e)
        {
            preHintEvent();
        }


        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).Close();
        }

    }
}
