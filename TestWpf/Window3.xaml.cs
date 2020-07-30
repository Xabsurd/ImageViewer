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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TestWpf
{
    /// <summary>
    /// Window3.xaml 的交互逻辑
    /// </summary>
    public partial class Window3 : Window
    {
        public Window3()
        {
            InitializeComponent();
            grid.MouseLeftButtonDown += (o, e) => { StartDrag(this); };
            this.MouseLeftButtonDown += (o, e) => { StartDrag(this); };
        }
        public static void StartDrag(Window window)
        {
            WindowInteropHelper helper = new WindowInteropHelper(window);
            SendMessage(helper.Handle, 161, 2, 0);
        }
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);

        //protected override void OnSourceInitialized(EventArgs e)
        //{
        //    base.OnSourceInitialized(e);
        //    HwndSource hwndSource = PresentationSource.FromVisual(this) as HwndSource;
        //    if (hwndSource != null)
        //    {
        //        //hwndSource.AddHook(new HwndSourceHook(WindowProc));
        //    }

        //    //HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
        //    //source.AddHook(new HwndSourceHook(myHook));
        //}
        public const int WM_NCLBUTTONDBLCLK = 0x00A3;
        private List<int> msgList;
        protected IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x0024:
                    WindowInteropHelper helper = new WindowInteropHelper(this);
                    if (helper.Handle != hwnd)
                    {
                        System.Windows.Forms.MessageBox.Show("Test");
                    }
                    return hwnd;
                case 0x0083:
                    NCCALCSIZE_PARAMS t = (NCCALCSIZE_PARAMS)Marshal.PtrToStructure(lParam, typeof(NCCALCSIZE_PARAMS));
                    t.rect0.top = t.rect0.top - 31;
                    Marshal.StructureToPtr(t, lParam, false);// 结构体转指针
                    //handled = true; 
                    return IntPtr.Zero;
                case 0x0084:
                    //COPYDATASTRUCT cds = (COPYDATASTRUCT)Marshal.PtrToStructure(lParam, typeof(COPYDATASTRUCT)); // 接收封装的消息
                    Point vPoint = new Point(((int)lParam & 0xFFFF) - this.Left, ((int)lParam >> 16 & 0xFFFF)-this.Top);
                    this.tb.Text = vPoint.ToString();
                    if (vPoint.Y <= 5)
                    {
                        handled = true;
                        if (vPoint.X <= 15)
                        {
                            return (IntPtr)HTTOPLEFT;
                        }
                        else if (vPoint.X >= this.Width - 15)
                        {
                            //MessageBox.Show("Test");
                            return (IntPtr)HTTOPRIGHT;
                        }
                        else
                        {
                            return (IntPtr)HTTOP;
                        }
                        
                    }
                    else
                    {
                        return (IntPtr)2;
                    }
            }
            return IntPtr.Zero;
        }
        public struct COPYDATASTRUCT

        {

            public IntPtr dwData; // 任意值

            public int cbData;    // 指定lpData内存区域的字节数

            [MarshalAs(UnmanagedType.LPStr)]

            public string lpData; // 发送给目标窗口所在进程的数据

        }
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
        private void Window_StateChanged(object sender, EventArgs e)
        {
        }
    }
}
