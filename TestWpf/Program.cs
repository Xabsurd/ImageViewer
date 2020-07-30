using CefSharp;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TestWpf
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += Resolver;
            LoadApp(args);
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
        private static void LoadApp(string[] arg)
        {

            Cef.EnableHighDPISupport();
            CefSettings settings = new CefSettings();
            settings.Locale = "zh-CN";
            //settings.WcfEnabled = true;
            settings.BrowserSubprocessPath = @"x64\CefSharp.BrowserSubprocess.exe";
            settings.PersistSessionCookies = true;
            Cef.Initialize(settings, performDependencyCheck: false, browserProcessHandler: null);
            App app = new App();
            app.Run(new Calculator());
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
