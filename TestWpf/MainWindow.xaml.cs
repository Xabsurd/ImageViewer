using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CefSharp;
using CefSharp.WinForms;
namespace TestWpf
{
    public static class GridW_H
    {
        public static int Num = 10;
    }
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool OnMove = true;
        ChromiumWebBrowser cwb;
        //public Window2 w;
        public MainWindow()
        {
            InitializeComponent();
            //w = new Window2(this);

            //w.Show();
            //this.SizeChanged += (o, e) => { if (w != null) { if (OnMove) { w.WResize = false; w.Height = this.Height + 16; w.Width = this.Width + 22; w.Left = this.Left - 6; w.Top = this.Top; } } };
            //this.LocationChanged += (o, e) => { if (OnMove) { w.Left = this.Left - 6; w.Top = this.Top; } };
            this.Closed += (o, e) => { System.Windows.Application.Current.Shutdown(); };
            //this.Loaded += (o, e) => { if (w != null) { this.Owner = w; w.Height = this.Height + 16; w.Width = this.Width + 22; w.Left = this.Left - 6; w.Top = this.Top; w.IsLoadEnd = true; } };
            msgList = new List<int>();
            int[] array = new int[] { 512, 675, 160, 161, 274, 533, 561, 534, 562, 513, 514, 256, 257 };
            //for (int i = 0; i < array.Length; i++)
            //{
            //    msgList.Add(array[i]);
            //}
            //this.KeyDown += Window_KeyDown;
            cwb = new ChromiumWebBrowser("www.douyu.com");
            this.WinForm.Child = cwb;
            cwb.DownloadHandler = new MyDownLoadFile();
            //this.MouseMove += Window_MouseMove;
            //this.MouseUp += (s, e) => { OnMove = false; };
            this.MouseLeftButtonDown += Window_MouseLeftButtonDown;
            this.MouseDoubleClick += (o, e) =>
            {
                if (this.WindowState == WindowState.Maximized)
                {
                    this.WindowState = WindowState.Normal;
                }
                else
                {
                    this.WindowState = WindowState.Maximized;
                }
            };
            this.DpiChanged += (o, e) => { System.Windows.Forms.MessageBox.Show("Test");};
            //this.MouseMove += Window_MouseMove;
            //this.BorderThickness = new Thickness();

        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            StartDrag(this);
            //Point MousePoint = Mouse.GetPosition(e.Source as FrameworkElement);

            //if (MousePoint.Y<35)
            //{
            //    //base.OnMouseLeftButtonDown(e);
            //    //Begin dragging the window
            //    this.DragMove();
            //    if (this.WindowState == WindowState.Maximized)
            //    {

            //    }
            //}
            //base.OnMouseLeftButtonDown(e);
            //Begin dragging the window
            //this.DragMove();
        }

        private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //Point MousePoint = Mouse.GetPosition(e.Source as FrameworkElement);
            //this.but.Content = MousePoint.ToString();
            //if (MousePoint.X > 15 && MousePoint.Y < 8)
            //{
            //    this.Cursor = Cursors.SizeNWSE;
            //}
            //if (OnMove)
            //{
            //}
        }
        private void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Max_Button_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }
        private void Min_Button_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            if (hwndSource != null)
            {
                hwndSource.AddHook(new HwndSourceHook(WindowProc));
            }

            //HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            //source.AddHook(new HwndSourceHook(myHook));
        }
        public const int WM_NCLBUTTONDBLCLK = 0x00A3;
        private List<int> msgList;
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }
        //作用于重绘非客户区域
        [StructLayout(LayoutKind.Sequential)]
        public struct NCCALCSIZE_PARAMS
        {
            public RECT rect0;
            public RECT rect1;
            public RECT rect2;
            public WINDOWPOS IntPtr;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPOS
        {
            public IntPtr hwnd;
            public IntPtr hwndInsertAfter;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public int flags;
        }
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static public extern int GetSystemMetrics(int nIndex);
        const int WM_NCCALCSIZE = 0x0083;


        const int HTTOP = 12;

        const int HTTOPLEFT = 13;

        const int HTTOPRIGHT = 14;
        protected IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == 0x0047)
            {
                //System.Windows.Forms.MessageBox.Show("Test");
            }
            switch (msg)
            {
                case WM_NCLBUTTONDBLCLK:
                    //System.Windows.Forms.MessageBox.Show(((IntPtr)msg).ToString());
                    if (this.WindowState == WindowState.Maximized)
                    {
                        this.WindowState = WindowState.Normal;
                        this.Max_But_Path.SetResourceReference(Path.DataProperty, "Max_Logo");
                    }
                    else
                    {
                        this.WindowState = WindowState.Maximized;

                        this.Max_But_Path.SetResourceReference(Path.DataProperty, "Non_Logo");
                    }
                    handled = true;
                    break;
                case 0x0083:
                    Graphics currentGraphics = Graphics.FromHwnd(new WindowInteropHelper(this).Handle);
                    double dpixRatio = currentGraphics.DpiX / 96;
                    NCCALCSIZE_PARAMS t = (NCCALCSIZE_PARAMS)Marshal.PtrToStructure(lParam, typeof(NCCALCSIZE_PARAMS));
                    t.rect0.top = t.rect0.top - SystemInformation.CaptionHeight - (int)(7.0*dpixRatio)-1;
                    Marshal.StructureToPtr(t, lParam, false);// 结构体转指针
                    //handled = true; 
                    return IntPtr.Zero;
                case 0x0084:
                    //if (this.WindowState!=WindowState.Maximized)
                    //{
                    //COPYDATASTRUCT cds = (COPYDATASTRUCT)Marshal.PtrToStructure(lParam, typeof(COPYDATASTRUCT)); // 接收封装的消息
                    System.Windows.Point vPoint = new System.Windows.Point(((int)lParam & 0xFFFF) - this.Left, ((int)lParam >> 16 & 0xFFFF) - this.Top);
                    this.tb.Text = vPoint.ToString();
                    if (WindowState == WindowState.Normal)
                    {
                        if (vPoint.Y <= 5 && vPoint.X < this.Width - 157)
                        {
                            handled = true;
                            if (vPoint.X <= 15)
                            {
                                return (IntPtr)HTTOPLEFT;
                            }
                            else
                            {
                                return (IntPtr)HTTOP;
                            }

                        }
                        else if(vPoint.Y <= 1)
                        {
                            handled = true;
                            if (vPoint.X > this.Width - 15)
                            {
                                return (IntPtr)HTTOPRIGHT;
                            }
                            else
                            {
                                return (IntPtr)HTTOP;
                            }
                        }

                    }

                    return IntPtr.Zero;
                    //}
                    //return IntPtr.Zero;
            }
            if (msgList.IndexOf(msg) < 0)
            {
                this.TextBlock1.Text += "     " + msg;
                msgList.Add(msg);
            }

            return IntPtr.Zero;
            //if (msg == 8)
            //{
            //    //失去焦点
            //    //MessageBox.Show("Test");

            //}
            //if (msg == 532)
            //{
            //    //目前不知
            //    //MessageBox.Show("Test");
            //    this.grid.Margin = new Thickness(5,5,0,0);
            //    GridW_H.Num = 10;
            //    //System.Windows.Forms.MessageBox.Show("Test");
            //    this.grid.Height = this.Height - 10;
            //    this.grid.Width = this.Width - 10;

            //}
            //if (msg == 562)
            //{
            //    //移动结束
            //    //if ()
            //    //{
            //    double x1 = SystemParameters.PrimaryScreenWidth;//得到屏幕整体宽度
            //    double y1 = SystemParameters.PrimaryScreenHeight;//得到屏幕整体高度
            //    if (this.Width == x1 / 2 && this.Height == y1 / 2)
            //    {
            //        if (this.Left == 0 && this.Top == 0)
            //        {
            //            //System.Windows.Forms.MessageBox.Show("Test");
            //            setGrid();
            //        }
            //        else if (this.Left == 0 && this.Top == y1 / 2)
            //        {
            //            setGrid();
            //        }
            //        else if (this.Left == x1 / 2 && this.Top == 0)
            //        {
            //            setGrid();
            //        }
            //        else if (this.Left == x1 / 2 && this.Top == y1 / 2)
            //        {
            //            setGrid();
            //        }
            //    }
            //    if (this.Width == x1 / 2 && this.Height == y1)
            //    {
            //        if (this.Left == 0)
            //        {
            //            setGrid();

            //        }
            //        else if (this.Left == x1 / 2)
            //        {
            //            setGrid();

            //        }
            //    }

            //    //}
            //}

        }
        public void setGrid()
        {
            this.grid.Margin = new Thickness(0);
            this.grid.Height = this.Height;
            this.grid.Width = this.Width;
            //GridW_H.Num = 0;
        }
        public int MaxMin(int a, int b, int c)
        {
            if (a <= b)
            {
                return b;
            }
            else if (a >= c)
            {
                return c;
            }
            else
            {
                return a;
            }
        }
        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //MessageBox.Show("asd");

        }
        //DragMove（）方法仅在表单的标题栏中起作用，因此请使用：
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);

        public static void StartDrag(Window window)
        {
            WindowInteropHelper helper = new WindowInteropHelper(window);
            SendMessage(helper.Handle, 161, 2, 0);
        }
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);
        public struct POINT
        {
            public int X;
            public int Y;
            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }

        private void but_Click(object sender, RoutedEventArgs e)
        {
            cwb.Load(tb.Text);
        }
        public class MyDownLoadFile : IDownloadHandler
        {
            public void OnBeforeDownload(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IBeforeDownloadCallback callback)
            {
                if (!callback.IsDisposed)
                {
                    using (callback)
                    {
                        callback.Continue(@"E:\Users\Administrator\Downloads" +
                                downloadItem.SuggestedFileName,
                            showDialog: true);
                    }
                }
            }

            public void OnDownloadUpdated(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IDownloadItemCallback callback)
            {
                //throw new NotImplementedException();
            }
            public bool OnDownloadUpdated(CefSharp.DownloadItem downloadItem)
            {
                return false;
            }
        }
    }
}
