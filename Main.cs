using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Web;


namespace Wox.Plugin.OpenCMD
{
    public class Main : IPlugin
    {
        private PluginInitContext context;

        /// <summary>
        /// file explorer window.
        /// </summary>
        private static List<SystemWindow> openingWindows = new List<SystemWindow>();

        static Main()
        {
            // use to auto load Interop.SHDocVw.dll from resources
            // only copy to plugin folder can not load correctly
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
        }

        public List<Result> Query(Query query)
        {
            var list = new List<Result>();

            SystemWindow win = null;
            GetOpeningWindows();
            if (openingWindows.Count > 0)
            {
                foreach (SystemWindow window in openingWindows)
                {
                    if (IsValidateProcess(window.Process.ProcessName))
                    {
                        win = window;
                        break;
                    }
                }
            }

            if (win != null)
            {
                foreach (SHDocVw.InternetExplorer window in new SHDocVw.ShellWindowsClass())
                {
                    var filename = Path.GetFileNameWithoutExtension(window.FullName).ToLower();
                    if (filename.ToLowerInvariant() == "explorer")
                    {
                        if (!window.LocationURL.ToLower().Contains("file:"))
                            continue;

                        // immediately open the windows command
                        if (win.HWnd == (IntPtr)window.HWND)
                        {
                            var path = window.LocationURL.Replace("file:///", "");
                            StartShell(path);
                            this.context.API.HideApp();
                            return list;
                        }
                    }

                }
            }

            if (list.Count <= 0)
            {
                // list all opening folder
                foreach (SHDocVw.InternetExplorer window in new SHDocVw.ShellWindowsClass())
                {
                    var filename = Path.GetFileNameWithoutExtension(window.FullName).ToLower();
                    if (filename.ToLowerInvariant() == "explorer")
                    {
                        if (!window.LocationURL.ToLower().Contains("file:"))
                            continue;

                        var path = window.LocationURL.Replace("file:///", "");
                        path = HttpUtility.UrlDecode(path);
                        if (!Directory.Exists(path))
                            continue;

                        list.Add(new Result()
                        {
                            IcoPath = "Images\\app.png",
                            Title = path,
                            SubTitle = "Open cmd in this path",
                            Action = (c) =>
                            {
                                StartShell(path);
                                return true;
                            }
                        });

                    }
                }
            }

            return list;
        }

        public void Init(PluginInitContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// 启动Shell.
        /// </summary>
        /// <param name="path">路径.</param>
        private static void StartShell(string path)
        {
            path = HttpUtility.UrlDecode(path);
            var cmder = UserSettingStorage.Instance.Cmder;
            var cmderArgs = UserSettingStorage.Instance.CmderArgs;
            if (!string.IsNullOrEmpty(cmder))
            {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = cmder,
                    WorkingDirectory = path,
                    Arguments = String.Format(cmderArgs,path)
                });
            }
            else
            {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = "cmd",
                    WorkingDirectory = path
                });
            }
        }

        /// <summary>
        /// 获得正在打开着的窗口.获得的窗口对象列表放在openingWindows中.
        /// </summary>
        private static void GetOpeningWindows()
        {
            openingWindows = new List<SystemWindow>();
            WinApi.EnumWindowsProc callback = EnumWindows;
            WinApi.EnumWindows(callback, 0);
        }

        private static bool EnumWindows(IntPtr hWnd, int lParam)
        {
            if (!WinApi.IsWindowVisible(hWnd))
                return true;

            var title = new StringBuilder(256);
            WinApi.GetWindowText(hWnd, title, 256);

            if (string.IsNullOrEmpty(title.ToString()))
            {
                return true;
            }

            if (title.Length != 0 || (title.Length == 0 & hWnd != WinApi.statusbar))
            {
                var window = new SystemWindow(hWnd);

                // IsTopmostWindow为的是去掉start窗口
                if (!window.IsTopmostWindow())
                {
                    if (IsValidateProcess(window.Process.ProcessName))
                    {
                        openingWindows.Add(window);
                    }
                    else if (window.IsAltTabWindow())
                    {
                        openingWindows.Add(window);
                    }
                }
            }

            return true;
        }

        public static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string dllName = args.Name.Contains(',') ? args.Name.Substring(0, args.Name.IndexOf(',')) : args.Name.Replace(".dll", "");

            dllName = dllName.Replace(".", "_");

            if (dllName.EndsWith("_resources")) return null;

            System.Resources.ResourceManager rm = new System.Resources.ResourceManager(typeof(Main).Namespace + ".Properties.Resources", System.Reflection.Assembly.GetExecutingAssembly());

            byte[] bytes = (byte[])rm.GetObject(dllName);

            return System.Reflection.Assembly.Load(bytes);
        }


        private static bool IsValidateProcess(string processName)
        {
            List<string> explorers = UserSettingStorage.Instance.Explorers;
            if (explorers != null && explorers.Count > 0)
            {
                foreach (string explorer in explorers)
                {
                    if (String.Compare(processName, explorer, true) == 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
