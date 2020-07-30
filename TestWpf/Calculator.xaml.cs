using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TestWpf
{
    /// <summary>
    /// Calculator.xaml 的交互逻辑
    /// </summary>
    public partial class Calculator : Window
    {
        public Calculator()
        {
            InitializeComponent();
            this.MouseLeftButtonDown += (o, e) =>
            {
                //System.Windows.MessageBox.Show("ok");
                StartDrag(this);
            };
            grid.MouseLeftButtonDown += (o, e) =>
            {
                StartDrag(this);
            };
            for (int i = 0; i < 24; i++)
            {
                
                Button b = new Button();
                b.Margin = new Thickness(2,2,2,2);
               
                if (i<8)
                {
                    b.Style = this.FindResource("WhiteBut") as Style;
                }
                else if(i<16)
                {
                    b.Style = this.FindResource("GrayBut") as Style;
                }
                else
                {
                    b.Style = this.FindResource("BlackBut") as Style;
                }
                b.ApplyTemplate();
                ButtonBox.Children.Add(b);
            }
            
        }
        private static void StartDrag(Window w)
        {
            //发送拖动窗口的消息
            SendMessage(new WindowInteropHelper(w).Handle, 161, 2, 0);

        }
        //注册WindowProc
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            if (hwndSource != null)
            {
                hwndSource.AddHook(new HwndSourceHook(WindowProc));
            }

        }
        //接收消息
        protected IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == 0x0047)
            {
                //System.Windows.Forms.MessageBox.Show("Test");
            }
            switch (msg)
            {

                case 0x0083:
                    //修改非客户区大小
                    System.Drawing.Graphics currentGraphics = System.Drawing.Graphics.FromHwnd(new WindowInteropHelper(this).Handle);
                    double dpixRatio = currentGraphics.DpiX / 96;
                    NCCALCSIZE_PARAMS t = (NCCALCSIZE_PARAMS)Marshal.PtrToStructure(lParam, typeof(NCCALCSIZE_PARAMS));
                    t.rect0.top = t.rect0.top - System.Windows.Forms.SystemInformation.CaptionHeight - (int)(7.0 * dpixRatio) - 1;
                    Marshal.StructureToPtr(t, lParam, false);// 结构体转指针
                    //handled = true; 
                    return IntPtr.Zero;

                case 0x0084:
                    //拖动上边框的代码
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
                        else if (vPoint.Y <= 1)
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

            return IntPtr.Zero;


        }
        //发送windows消息
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);



        //作用于重绘非客户区域
        public const int WM_NCLBUTTONDBLCLK = 0x00A3;
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }
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
    }
}
