using RemoteNotifier.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RemoteNotifier
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainContext());
        }

        public class MainContext : ApplicationContext
        {
            private readonly NotifyIcon trayIcon;
            private readonly MainFrm mainFrm;

            public MainContext()
            {
                trayIcon = new NotifyIcon()
                {
                    Icon = Icon.FromHandle(Resources.NotifierIcon.GetHicon()),
                    ContextMenu = new ContextMenu(new MenuItem[]
                    {
                        new MenuItem("Exit", Exit),
                        new MenuItem("Modify", Modify)
                    }),
                    Visible = true
                };
                mainFrm = new MainFrm();
                mainFrm.Show();
            }

            private void Exit(object sender, EventArgs e)
            {
                trayIcon.Visible = false;
                Application.Exit();
            }

            private void Modify(object sender, EventArgs e)
            {
                if (mainFrm.commandForm.ShowDialog() == DialogResult.OK)
                {
                    mainFrm.RunCommand();
                }
            }
        }
    }
}
