using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace ResumeNotifier
{
    public partial class MainFrm : Form
    {
        public readonly CommandForm commandForm = new CommandForm();

        private Process process;

        public void RunCommand()
        {
            try
            {
                if (process != null)
                {
                    process.Kill();
                    process.WaitForExit();
                    process.Dispose();
                    process = null;
                }

                if (!commandForm.HasCommand)
                {
                    return;
                }

                //process = Process.Start(commandForm.CommandText, commandForm.ArgsText);
                process = Process.Start(new ProcessStartInfo
                {
                    FileName = commandForm.CommandText,
                    Arguments = commandForm.CommandArgs,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    WorkingDirectory = commandForm.RunDirectory
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public MainFrm()
        {
            InitializeComponent();
            if (!commandForm.HasCommand)
            {
                commandForm.ShowDialog();
            }
            RunCommand();
        }

        const int WM_POWERBROADCAST = 0x218;
        const int PBT_APMRESUMEAUTOMATIC = 0x12;

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_POWERBROADCAST && (int)m.WParam == PBT_APMRESUMEAUTOMATIC)
            {
                RunCommand();
            }
        }
    }
}
