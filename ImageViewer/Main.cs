using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp;
using CefSharp.Enums;
using CefSharp.WinForms;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace ImageViewer
{
    public partial class Main : Form
    {
        //参数区域
        internal ChromiumWebBrowser cwb;
        internal string MainUrl = @"F:\lol image\无标题.png";
        internal string DragFileName;
        internal bool manyTime;
        internal int SortType;


        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        //发送消息
        public static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);


        public const int WM_SYSCOMMAND = 0x0112;
        public const int SC_MOVE = 0xF010;
        public const int HTCAPTION = 0x0002;
        public Button but;

        [System.Runtime.InteropServices.DllImport("user32.dll ")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int wndproc);
        [System.Runtime.InteropServices.DllImport("user32.dll ")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        public const int GWL_STYLE = -16;
        public const int WS_DISABLED = 0x8000000;

        public static void SetControlEnabled(Control c, bool enabled)
        {
            if (enabled)
            { SetWindowLong(c.Handle, GWL_STYLE, (~WS_DISABLED) & GetWindowLong(c.Handle, GWL_STYLE)); }
            else
            { SetWindowLong(c.Handle, GWL_STYLE, WS_DISABLED + GetWindowLong(c.Handle, GWL_STYLE)); }
        }
        public Main(string url)
        {
            //new Form1().Show();
            var dpi = 96f / CreateGraphics().DpiX;
            Font = new Font(Font.Name, 8.25f * 96f / CreateGraphics().DpiX, Font.Style, Font.Unit, Font.GdiCharSet, Font.GdiVerticalFont);
            InitializeComponent();
            this.AutoScaleMode = AutoScaleMode.None;
            mainhandle = this.Handle;
            //string ProgID = System.Reflection.Assembly.GetExecutingAssembly().EntryPoint.DeclaringType.FullName;
            //Microsoft.Win32.Registry.LocalMachine.CreateSubKey(@"Software\Classes\" + ".png").SetValue("", ProgID);
            //SaveReg(FileLocation,".gif");
            this.Opacity = 0;
            //MessageBox.Show(AppDomain.CurrentDomain.BaseDirectory + @"..\..\..\View.html");
            cwb = new ChromiumWebBrowser(AppDomain.CurrentDomain.BaseDirectory + @"..\..\View.html");
            //cwb.JavascriptObjectRepository.Register("boundAsyncc", new BoundObject(), true, BindingOptions.DefaultBinder);
            cwb.JavascriptObjectRepository.Register("main", new jsMain(this), false, BindingOptions.DefaultBinder);
            cwb.FrameLoadEnd += new EventHandler<FrameLoadEndEventArgs>(cwb_LoadEnd);
            //cwb.Dock = DockStyle.None;
            //cwb.Location = new Point(0, 100);
            this.BackColor = Color.Black;
            cwb.Size = this.Size;
            //cwb.Load("https://www.baidu.com");
            cwb.DragHandler = new DragHandler(this);
            cwb.MenuHandler = new MenuHandler();
            cwb.KeyboardHandler = new KeyBoardHander(this);

            this.Controls.Add(cwb);
            this.AllowDrop = true;
            this.DoubleBuffered = true;
            //this.FormBorderStyle = FormBorderStyle.None;
            this.MinimumSize = new Size(500, 300);
            if (url != null && url != "") { MainUrl = url; }
            this.SizeChanged += (o, e) =>
            {
                if (this.WindowState == FormWindowState.Maximized)
                {
                    cwb.Dock = DockStyle.None;
                    cwb.Size = new Size(this.Size.Width - (int)(16.0 / dpi), this.Size.Height - (int)(16.0 / dpi));
                    cwb.Location = new Point(0, (int)(8.0 / dpi));
                }
                else
                {
                    if (cwb.Dock != DockStyle.Fill)
                    {
                        cwb.Dock = DockStyle.Fill;
                    }
                }
            };
            RegistryKey myReg1, myReg2; //声明注册表对象
            myReg1 = Registry.CurrentUser; //获取当前用户注册表项
            try
            {
                myReg2 = myReg1.CreateSubKey("Software\\MyImageView"); //在注册表项中创建子项
                this.Location = new Point(Convert.ToInt32(myReg2.GetValue("1")) < 0 ? 100 : Convert.ToInt32(myReg2.GetValue("1")), Convert.ToInt32(myReg2.GetValue("2")) < 0 ? 100 : Convert.ToInt32(myReg2.GetValue("2"))); //设置窗体的显示位置
                this.Size = new Size(Convert.ToInt32(myReg2.GetValue("3")), Convert.ToInt32(myReg2.GetValue("4")));
                manyTime = Convert.ToBoolean(myReg2.GetValue("5"));
                SortType = Convert.ToInt32(myReg2.GetValue("6"));
            }
            catch
            {
                this.Size = new Size(1280, 720);
                this.Location = new Point(100, 100);
                SortType = 1;
            }
            this.Opacity = 1;
            //but = new Button();
            //but.Size = new Size(1000, 100);
            //but.Location = new Point(1, 1);
            //this.Controls.Add(but);

        }
        [DllImport("user32.dll", EntryPoint = "GetDoubleClickTime")]

        public extern static int GetDoubleClickTime();
        //网页加载完成
        private void cwb_LoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            //CEFSHARP 的放大比例计算方式为1.2^zoomlevel 所以传参为1.2为低放大倍率的对数
            cwb.SetZoomLevel(Math.Log((double)96.00 / (double)CreateGraphics().DpiX, 1.2));
            //this.Invoke(new EventHandler(delegate
            //{

            //}));
            if (manyTime)
            {
                cwb.ExecuteScriptAsync("$('.isMany').html('点击禁用多窗口（重启生效）')");
            }
            else
            {
                cwb.ExecuteScriptAsync("$('.isMany').html('点击启动多窗口（重启生效）')");
            }
            cwb.ExecuteScriptAsync("DoubleClickTime=" + GetDoubleClickTime());
            changeImage(MainUrl);
            //cwb.ExecuteScriptAsync("alert('a')");
        }
        //切换图片
        public void changeImage(string url)
        {
            if (url != null && url.Length > 1 && File.Exists(url))
            {
                //切换图片
                cwb.ExecuteScriptAsync(string.Format("ChangeImage(\"{0}\")", url.Replace(@"\", @"/")));
                //开启线程读取当前目录下所有的支持图片并创建缩略图
                Thread t = new Thread(new ParameterizedThreadStart(getUrl));
                t.Start(url);
            }
        }
        public void getUrl(object src)
        {
            string url = src.ToString();
            string xurl = url.Substring(0, url.LastIndexOf(@"\"));
            //获取当前目录下的所有支持图片
            string[] pic = Directory.GetFiles(xurl).Where(s => { return IsImage(s); }).ToArray();

            IComparer fileNameComparer = new FilesNameComparerClass();
            Array.Sort(pic, fileNameComparer);
            FileInfo info = null;
            List<FileData> list = new List<FileData>();
            //初始化图片列表
            foreach (string item in pic)
            {
                info = new FileInfo(item);
                list.Add(new FileData() { Name = info.Name, FullName = info.FullName.Replace(@"\", @"/"), CreateTime = info.CreationTime, Size = info.Length, Base = info.Length > 36000 ? true : false });
            }
            //图片列表转换成json
            string output = JsonConvert.SerializeObject(list);
            //修改默认排序规则
            cwb.ExecuteScriptAsync(string.Format("setUrlArray({0})", output));
            switch (SortType)
            {
                case 1:
                    cwb.ExecuteScriptAsync("whatSort=0;$('.SortUp').click()");
                    break;
                case 2:
                    cwb.ExecuteScriptAsync("whatSort=0;$('.SortDown').click()");
                    break;
                case 3:
                    cwb.ExecuteScriptAsync("whatSort=1;$('.SortUp').click()");
                    break;
                case 4:
                    cwb.ExecuteScriptAsync("whatSort=1;$('.SortDown').click()");
                    break;
                case 5:
                    cwb.ExecuteScriptAsync("whatSort=2;$('.SortUp').click()");
                    break;
                case 6:
                    cwb.ExecuteScriptAsync("whatSort=2;$('.SortDown').click()");
                    break;
            }
            //Chromium.ShowDevTools();
            //开启线程保存缩略图 

            if (xurl != AppDomain.CurrentDomain.BaseDirectory + "ImageTum")
            {
                Thread t = new Thread(new ParameterizedThreadStart(saveImageX));
                t.Start(pic);
            }
            else
            {
                cwb.ExecuteScriptAsync("setMinImg()");
            }

        }
        FileInfo Sinfo = null;
        string name = null;
        /// <summary>
        /// 保存图片
        /// </summary>
        /// <param name="o">图片路径数组</param>
        public void saveImageX(object o)
        {



            foreach (string item in (string[])o)
            {
                Sinfo = new FileInfo(item);
                name = Sinfo.FullName;
                if (Process.GetCurrentProcess().WorkingSet64 > 524288000)
                {
                    GC.Collect();
                }
                if (Sinfo.Length > 36000)
                {
                    if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "ImageTum/" + name.Replace(@"\", @"-").Replace(":", "")))
                    {
                        if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "ImageTum"))
                        {
                            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "ImageTum");
                        }
                        try
                        {
                            //保存压缩图片
                            CompressImage(Image.FromFile(name), AppDomain.CurrentDomain.BaseDirectory + "ImageTum/" + name.Replace(@"\", @"-").Replace(":", ""), 90L);
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
            }
            //设置缩略图
            cwb.ExecuteScriptAsync("setMinImg()");


        }
        //压缩图片并保存
        private void CompressImage(Image img, string toPath, long ratio)
        {
            //修改大小
            using (Bitmap thumbImage = new Bitmap(img, new Size((int)((double)img.Width / (double)img.Height * 100.0), 100)))
            {
                //设置编码
                ImageCodecInfo jgpEncoder = GetEncoder(ImageFormat.Jpeg);
                System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
                EncoderParameters myEncoderParameters = new EncoderParameters(1);
                //设置压缩质量
                EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, ratio);
                myEncoderParameters.Param[0] = myEncoderParameter;
                //保存图片
                thumbImage.Save(toPath, jgpEncoder, myEncoderParameters);
            }
        }
        //设定图片编码
        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        Point mPosition, mLocation;
        Size mSize;
        int MaxLocX = 0, MaxLocY;
        IntPtr mainhandle;
        public class jsMain
        {
            public Main main;
            public jsMain(Main _main)
            {
                main = _main;
            }
            //发送消息
            public void sendMove(int type)
            {
                if (type == 1)//main.WindowState != FormWindowState.Maximized || 
                {

                    switch (type)
                    {
                        case 1:
                            //拖动窗体
                            main.Invoke(new EventHandler(delegate
                            {
                                Point p = main.Location;
                                main.cwb.Enabled = false;
                                ReleaseCapture();
                                SendMessage(main.Handle, WM_SYSCOMMAND, SC_MOVE + HTCAPTION, 0);
                                main.cwb.Enabled = true;
                                if (Math.Abs(p.X - main.Location.X) < 1 && Math.Abs(p.Y - main.Location.Y) < 1)
                                {

                                }
                                else
                                {
                                    main.cwb.ExecuteScriptAsync("clickTime=0");
                                }
                            }));
                            break;
                        case 2:

                            //拖动右边框
                            main.Invoke(new EventHandler(delegate
                            {
                                ReleaseCapture();
                                SendMessage(main.Handle, 274, 61442, 0);
                            }));
                            break;
                        case 3:
                            //拖动下边框
                            main.Invoke(new EventHandler(delegate
                            {
                                ReleaseCapture();
                                SendMessage(main.Handle, 274, 61446, 0);
                            }));
                            break;
                        case 4:
                            //拖动右上角
                            main.Invoke(new EventHandler(delegate
                            {
                                ReleaseCapture();
                                SendMessage(main.Handle, 274, 61445, 0);
                            }));
                            break;
                        case 5:
                            //拖动右下角
                            main.Invoke(new EventHandler(delegate
                            {
                                ReleaseCapture();
                                SendMessage(main.Handle, 274, 61448, 0);
                            }));
                            break;
                        case 6:
                            //拖动左下角
                            main.Invoke(new EventHandler(delegate
                            {
                                ReleaseCapture();
                                SendMessage(main.Handle, 274, 61447, 0);
                            }));
                            break;
                        case 7:
                            //拖动左上角
                            main.Invoke(new EventHandler(delegate
                            {
                                ReleaseCapture();
                                SendMessage(main.Handle, 274, 61444, 0);
                            }));
                            break;
                        case 8:
                            //拖动左边框
                            main.Invoke(new EventHandler(delegate
                            {
                                ReleaseCapture();
                                SendMessage(main.Handle, 274, 61441, 0);
                            }));
                            break;
                        case 9:
                            //拖动上边框
                            main.Invoke(new EventHandler(delegate
                            {
                                ReleaseCapture();
                                SendMessage(main.Handle, 274, 61443, 0);
                            }));
                            break;
                    }
                }
            }
            //获取窗口数值
            public void getpoint()
            {
                main.mPosition = System.Windows.Forms.Form.MousePosition;//获取鼠标位置
                main.mLocation = main.Location;//获取窗口位置
                main.mSize = main.Size;//获取窗口大小
                main.MaxLocX = main.Location.X + main.Size.Width - 500;//设置最大和最小位置
                main.MaxLocY = main.Location.Y + main.Size.Height - 300;
            }
            //已过时
            public void hideborder(object val)
            {
                int[] data = new int[4];
                for (int i = 0; i < (val as Object[]).Length; i++)
                {
                    data[i] = Convert.ToInt32((val as Object[]).GetValue(i));
                }
                main.Invoke(new EventHandler(delegate
                {
                    //var jsArray = val.Arguments.FirstOrDefault(p => p.IsArray);
                    //if (jsArray != null && jsArray.ArrayLength == 4)
                    //{
                    switch (data[0])
                    {
                        case 100:
                            main.Size = new Size(main.mSize.Width + (main.mPosition.X - System.Windows.Forms.Form.MousePosition.X), main.mSize.Height);
                            main.Location = new Point(MaxNum(System.Windows.Forms.Form.MousePosition.X + data[2], main.MaxLocX), main.mPosition.Y + data[3]);
                            break;
                        case 200:
                            main.Size = new Size(main.mSize.Width, main.mSize.Height + (main.mPosition.Y - System.Windows.Forms.Form.MousePosition.Y));
                            main.Location = new Point(main.mPosition.X + data[2], MaxNum(System.Windows.Forms.Form.MousePosition.Y + data[3], main.MaxLocY));
                            break;
                        case 300:
                            main.Size = new Size(main.mSize.Width - (main.mPosition.X - System.Windows.Forms.Form.MousePosition.X), main.mSize.Height + (main.mPosition.Y - System.Windows.Forms.Form.MousePosition.Y));
                            main.Location = new Point(main.mPosition.X + data[2], MaxNum(System.Windows.Forms.Form.MousePosition.Y + data[3], main.MaxLocY));
                            break;
                        case 400:
                            main.Size = new Size(main.mSize.Width + (main.mPosition.X - System.Windows.Forms.Form.MousePosition.X), main.mSize.Height - (main.mPosition.Y - System.Windows.Forms.Form.MousePosition.Y));
                            main.Location = new Point(MaxNum(System.Windows.Forms.Form.MousePosition.X + data[2], main.MaxLocX), main.mPosition.Y + data[3]);
                            break;
                        case 600:
                            main.Location = new Point(main.mLocation.X + Form.MousePosition.X - main.mPosition.X, main.mLocation.Y + Form.MousePosition.Y - main.mPosition.Y);
                            break;
                        default:
                            main.Size = new Size(main.mSize.Width + (main.mPosition.X - System.Windows.Forms.Form.MousePosition.X), main.mSize.Height + (main.mPosition.Y - System.Windows.Forms.Form.MousePosition.Y));
                            main.Location = new Point(MaxNum(System.Windows.Forms.Form.MousePosition.X + data[2], main.MaxLocX), MaxNum(System.Windows.Forms.Form.MousePosition.Y + data[3], main.MaxLocY));
                            break;

                    }
                }));
                //}
            }
            //已过时
            public void showborder(object val)
            {
                if (val != null && (val as object[]).Length == 2)
                {
                    main.Invoke(new EventHandler(delegate
                    {
                        main.Size = new Size(Convert.ToInt32((val as object[]).GetValue(0)), Convert.ToInt32((val as object[]).GetValue(1)));
                    }));
                }
            }
            //拖入结束时触发
            public void drap()
            {
                main.changeImage(main.DragFileName);
            }
            /// <summary>
            /// 限制最大值
            /// </summary>
            /// <param name="a">被限制的值</param>
            /// <param name="b">最大值</param>
            /// <returns></returns>
            public int MaxNum(int a, int b)
            {
                if (a < b)
                {
                    return a;
                }
                else
                {
                    return b;
                }
            }
            //改变窗体状态
            public void changeWindow(int val)
            {
                main.Invoke(new EventHandler(delegate
                {
                    //修改窗口状态
                    switch (val)
                    {
                        case 1:
                            main.WindowState = FormWindowState.Minimized;
                            break;
                        case 2:
                            if (main.WindowState == FormWindowState.Maximized)
                            {
                                main.WindowState = FormWindowState.Normal;
                            }
                            else
                            {
                                main.WindowState = FormWindowState.Maximized;

                            }
                            break;
                        case 3:
                            main.Close();
                            break;
                        default:
                            break;
                    }
                }));
            }
            //改变窗口是否为单窗口模式
            public bool changeMany()
            {
                if (main.manyTime)
                {
                    main.manyTime = false;
                }
                else
                {
                    main.manyTime = true;
                }
                Registry.CurrentUser.CreateSubKey("Software\\MyImageView").SetValue("5", main.manyTime);
                return main.manyTime;
            }

            public void showDevTools()
            {
                main.cwb.ShowDevTools();
            }
            public bool isimg(string val)
            {
                return main.IsImage(val);
            }
            public void changeSortType(object o)
            {
                main.SortType = Convert.ToInt32(o);
            }
        }
        //重写windows消息
        const int WM_COPYDATA = 0x004A;
        protected override void DefWndProc(ref Message m)
        {

            switch (m.Msg)
            {
                case WM_COPYDATA:

                    COPYDATASTRUCT myStr = new COPYDATASTRUCT();
                    Type myType = myStr.GetType();
                    myStr = (COPYDATASTRUCT)m.GetLParam(myType);    //m中获取LParam参数以myType类型的方式，让后转换问结构体。
                    if (myStr.lpData != null)
                    {
                        MainUrl = myStr.lpData;
                        changeImage(myStr.lpData);
                    }
                    break;
                case 0x0083:
                    if (Convert.ToBoolean(m.WParam.ToInt32()))
                    {

                        NCCALCSIZE_PARAMS t = (NCCALCSIZE_PARAMS)m.GetLParam(typeof(NCCALCSIZE_PARAMS));
                        int CaptionHeight = GetSystemMetrics(4);
                        int Border3DWidth = System.Windows.Forms.SystemInformation.Border3DSize.Width;
                        //这里使用FW或者API都可以取到窗体的相关属性
                        int BorderWidth = System.Windows.Forms.SystemInformation.BorderSize.Width;

                        //尝试过了，只有修改RECT[0]才有效果撒…
                        Graphics currentGraphics = Graphics.FromHwnd(this.Handle);
                        double dpixRatio = currentGraphics.DpiX / 96; //获取屏幕DPI放大倍数
                        int x = t.rect0.top - this.Height;
                        t.rect0.top = t.rect0.top - (CaptionHeight) - (int)((double)7 * dpixRatio) - 1;


                        Marshal.StructureToPtr(t, m.LParam, false);// 结构体转指针

                        // base.WndProc(ref m);
                        //t.rect0.top = t.rect0.top - CaptionHeight - Border3DWidth - BorderWidth  - 5;
                        //t.rect0.top = t.rect0.top - 35;

                        //t.rect0.left = t.rect0.left ;

                        //t.rect0.right = t.rect0.right;
                        //59d8d69e44858dad3d90875bb8d5e097

                        //t.rect0.bottom = t.rect0.bottom ;
                    }
                    break;

                default:
                    break;
            }
            base.DefWndProc(ref m);
        }
        const int HTTOP = 12;

        const int HTTOPLEFT = 13;

        const int HTTOPRIGHT = 14;
        //作用于重绘非客户区域
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
        /// <summary>
        /// 键盘事件
        /// </summary>
        internal class KeyBoardHander : IKeyboardHandler
        {
            private Main main;
            public KeyBoardHander(Main _main)
            {
                main = _main;
            }
            public bool OnKeyEvent(IWebBrowser chromiumWebBrowser, IBrowser browser, KeyType type, int windowsKeyCode, int nativeKeyCode, CefEventFlags modifiers, bool isSystemKey)
            {
                return false;
            }

            public bool OnPreKeyEvent(IWebBrowser chromiumWebBrowser, IBrowser browser, KeyType type, int windowsKeyCode, int nativeKeyCode, CefEventFlags modifiers, bool isSystemKey, ref bool isKeyboardShortcut)
            {
                //f12
                if (windowsKeyCode == 123)
                {
                    try
                    {
                        main.cwb.ShowDevTools();
                    }
                    catch (Exception)
                    {

                    }
                }
                return false;
            }
        }
        /// <summary>
        /// 判断是否是支持的图片
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public bool IsImage(string url)
        {

            if (url != null)
            {
                url = Path.GetExtension(url.ToLower());
                switch (url.Substring(0, url.LastIndexOf("?") > -1 ? url.LastIndexOf("?") : url.Length))
                {
                    case ".jpg":
                        return true;
                    case ".jpeg":
                        return true;
                    case ".png":
                        return true;
                    case ".gif":
                        return true;
                    case ".bmp":
                        return true;
                    case ".jp2":
                        return true;
                    case ".webp":
                        return true;
                    default:
                        return false;
                }
            }
            else
            {
                return false;
            }

        }
        public struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpData;
        }
        public class FileData
        {
            public string Name { get; set; }
            public string FullName { get; set; }
            public DateTime CreateTime { get; set; }
            public long Size { get; set; }
            public bool Base { get; set; }
        }
        ///<summary>
        ///主要用于文件名的比较。
        ///</summary>
        public class FilesNameComparerClass : System.Collections.IComparer
        {
            // Calls CaseInsensitiveComparer.Compare with the parameters reversed.
            ///<summary>
            ///比较两个字符串，如果含用数字，则数字按数字的大小来比较。
            ///</summary>
            ///<param name="x"></param>
            ///<param name="y"></param>
            ///<returns></returns>
            int System.Collections.IComparer.Compare(Object x, Object y)
            {
                if (x == null || y == null)
                    throw new ArgumentException("Parameters can't be null");
                string fileA = x as string;
                string fileB = y as string;
                char[] arr1 = fileA.ToCharArray();
                char[] arr2 = fileB.ToCharArray();
                int i = 0, j = 0;
                while (i < arr1.Length && j < arr2.Length)
                {
                    if (char.IsDigit(arr1[i]) && char.IsDigit(arr2[j]))
                    {
                        string s1 = "", s2 = "";
                        while (i < arr1.Length && char.IsDigit(arr1[i]))
                        {
                            s1 += arr1[i];
                            i++;
                        }
                        while (j < arr2.Length && char.IsDigit(arr2[j]))
                        {
                            s2 += arr2[j];
                            j++;
                        }
                        if (double.Parse(s1) > double.Parse(s2))
                        {
                            return 1;
                        }
                        if (double.Parse(s1) < double.Parse(s2))
                        {
                            return -1;
                        }
                    }
                    else
                    {
                        if (arr1[i] > arr2[j])
                        {
                            return 1;
                        }
                        if (arr1[i] < arr2[j])
                        {
                            return -1;
                        }
                        i++;
                        j++;
                    }
                }
                if (arr1.Length == arr2.Length)
                {
                    return 0;
                }
                else
                {
                    return arr1.Length > arr2.Length ? 1 : -1;
                }
            }
        }


        public void cwb_DragDrop(IWebBrowser browserControl, IBrowser browser, IDragData dragData, DragOperationsMask mask)
        {
            //MessageBox.Show(e.Data.ToString());
            DragFileName = dragData.FileNames[0];


        }

        private void Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            RegistryKey myReg1, myReg2; //声明注册表对象
            myReg1 = Registry.CurrentUser; //获取当前用户注册表项
            myReg2 = myReg1.CreateSubKey("Software\\MyImageView"); //在注册表项中创建子项 
            try
            {
                if (this.WindowState != FormWindowState.Maximized)
                {
                    myReg2.SetValue("1", this.Location.X);
                    myReg2.SetValue("2", this.Location.Y);
                    myReg2.SetValue("3", this.Size.Width);
                    myReg2.SetValue("4", this.Size.Height);
                    myReg2.SetValue("5", manyTime);
                    myReg2.SetValue("6", SortType);
                }
            }
            catch
            {
                Application.Exit();
            }
            Application.Exit();
        }

        private void Main_Deactivate(object sender, EventArgs e)
        {
            //cwb.ExecuteScriptAsync("$('.bottom').slideUp(100);$('.ShowMinImg').stop().animate({ opacity: 0 }, 300); xONbotton = false;$('.top').slideUp(100); ");
        }

        private void Main_Leave(object sender, EventArgs e)
        {
            cwb.ExecuteScriptAsync("$('.bottom').slideUp(100);$('.ShowMinImg').stop().animate({ opacity: 0 }, 300); xONbotton = false;$('.top').slideUp(100); ");

        }

        public class MenuHandler : IContextMenuHandler
        {

            public void OnBeforeContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model)
            {
                model.Clear();
            }
            public bool OnContextMenuCommand(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, CefMenuCommand commandId, CefEventFlags eventFlags)
            {
                return false;
            }
            public void OnContextMenuDismissed(IWebBrowser browserControl, IBrowser browser, IFrame frame)
            {
            }
            public bool RunContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model, IRunContextMenuCallback callback)
            {
                return false;
            }
        }
        public class DragHandler : IDragHandler
        {
            private DragHandler()
            {

            }
            private Main main;
            public DragHandler(Main _main)
            {
                main = _main;
            }
            public bool OnDragEnter(IWebBrowser browserControl, IBrowser browser, IDragData dragData, DragOperationsMask mask)
            {
                if (dragData.FragmentText != "")
                {
                    return false;
                }
                main.cwb_DragDrop(browserControl, browser, dragData, mask);

                return false;
            }

            public void OnDraggableRegionsChanged(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IList<DraggableRegion> regions)
            {
            }
        }
        //public class mouseh:mouseh
    }
}
