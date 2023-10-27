using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using GS.Unitive.Framework.Core;
using System.Runtime.InteropServices;

namespace Cirno.ChinaGS.LockMouse.Plus
{
    public class Program : IAddonActivator
    {
        public static string[] taskKillList;
        public static string[] hideWindowList;
        public IAddonContext addonContext;
        public bool flagDontKill;
        public bool flagDontHide;
        public bool flagStopping;
        public Thread thread;
        public void Start(IAddonContext context)
        {
            this.addonContext = context;
            this.flagDontKill = false;
            this.flagDontHide = false;
            this.flagStopping = false;
            taskKillList = addonContext.DictionaryValue("baseConfig", "taskKillList").Split(';');
            hideWindowList = addonContext.DictionaryValue("baseConfig", "hideWindowList").Split(';');
            this.thread = new Thread(new ThreadStart(this.Taskkill))
            {
                IsBackground = true
            };

            dynamic uiService = addonContext.GetFirstOrDefaultService("GS.Terminal.MainShell", "GS.Terminal.MainShell.Services.UIService");
            uiService.RegistBackgroundCommand("114514191981032766", new Action(delegate
            {
                this.flagDontKill = true;
                this.flagDontHide = true;
            }));
        }
        public void Stop(IAddonContext context)
        {
            this.flagStopping = true;
        }
        public void Taskkill()
        {
            while(!flagStopping)
            {
                if (!flagDontKill)
                {
                    foreach (string kill in taskKillList)
                    {

                        Process[] processes = Process.GetProcessesByName(kill);
                        if (processes != null)
                        {
                            for (int i = 0; i < processes.Length; i++)
                            {
                                processes[i].Kill();
                            }
                        }
                    }

                    Process[] lm = Process.GetProcessesByName("LockMouse.exe");
                    if (lm == null)
                    {
                        try
                        {
                            Process.Start("LockMouse.exe");
                        }
                        catch
                        {
                            this.addonContext.Logger.Info("[cirno] lockmouse missing.");
                        }
                    }

                    if (!flagDontHide)
                    {
                        foreach (string wndName in hideWindowList)
                        {
                            IntPtr hwnd = FindWindowEx(IntPtr.Zero, IntPtr.Zero, wndName, null);
                            ShowWindow(hwnd, 0U);
                        }
                    }
                    
                }
                Thread.Sleep(4000);
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);
    }
}
