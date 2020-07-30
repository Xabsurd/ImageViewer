using CefSharp;
using CefSharp.WinForms;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageViewer
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {

            AppDomain.CurrentDomain.AssemblyResolve += Resolver;
            RegistryKey myReg1, myReg2; //声明注册表对象
            myReg1 = Registry.CurrentUser; //获取当前用户注册表项
            bool manyTime = true;
            try
            {
                myReg2 = myReg1.CreateSubKey("Software\\MyImageView"); //在注册表项中创建子项
                manyTime = Convert.ToBoolean(myReg2.GetValue("5"));
            }
            catch
            {

            }

            if (manyTime)
            {
                //多窗口模式
                LoadApp(args);
            }
            else
            {
                //但窗口模式
                string strProcessName = Application.CompanyName;
                if (System.Diagnostics.Process.GetProcessesByName(strProcessName).Length <= 1)
                {
                    try
                    {
                        //单窗口模式下没有程序启动时启动程序
                        LoadApp(args);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.ToString());
                    }
                  
                }
                else
                {
                    if (args != null && args.Length > 0)
                    {
                        //当程序已经启动时向程序发送消息，传递启动参数
                        //获取程序进程
                        Process[] processes = System.Diagnostics.Process.GetProcessesByName(strProcessName);
                        //判断启动参数是否为空
                        string message = args.Length == 0 ? null : args[0];
                        //转码
                        byte[] sarr = System.Text.Encoding.Default.GetBytes(message);
                        //初始化传递参数
                        COPYDATASTRUCT cds;
                        cds.dwData = (IntPtr)100;
                        cds.lpData = message;
                        cds.cbData = sarr.Length + 1; //此值错误会引发接收端崩溃
                        foreach (Process item in processes)
                        {
                            //指定窗口为活动窗口
                            SetForegroundWindow(item.MainWindowHandle);
                            //发送消息
                            SendMessage(item.MainWindowHandle, WM_COPYDATA, 0, ref cds);
                            
                        }
                    }
                }
            }

        }
        const int WM_COPYDATA = 0x004A;
        public struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpData;
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        //启动应用程序
        private static void LoadApp(string[] arg)
        {

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Cef.EnableHighDPISupport();
            //设置Cefsharp
            CefSettings settings = new CefSettings();
            settings.Locale = "zh-CN";
            //设置缓存路径
            settings.CachePath = AppDomain.CurrentDomain.BaseDirectory;
            //settings.WcfEnabled = true;
            settings.BrowserSubprocessPath = AppDomain.CurrentDomain.BaseDirectory+@"\x64\CefSharp.BrowserSubprocess.exe";
            Cef.Initialize(settings, performDependencyCheck: false, browserProcessHandler: null);
            Application.Run(new Main(arg.Length<1 ? "" : arg[0]));
        }

        // Will attempt to load missing assembly from either x86 or x64 subdir
        private static Assembly Resolver(object sender, ResolveEventArgs args)
        {
            if (args.Name.StartsWith("CefSharp"))
            {
                string assemblyName = args.Name.Split(new[] { ',' }, 2)[0] + ".dll";
                string archSpecificPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
                                                       Environment.Is64BitProcess ? "x64" : "x86",
                                                       assemblyName);

                return File.Exists(archSpecificPath)
                           ? Assembly.LoadFile(archSpecificPath)
                           : null;
            }
            return null;
        }
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, ref COPYDATASTRUCT lParam);
        [DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}
