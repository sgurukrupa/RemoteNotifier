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
using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace RemoteNotifier
{
    public partial class MainFrm : Form
    {
        public readonly CommandForm commandForm = new CommandForm();
        private UdpClient server;
        private Thread serverThread;
        private Process process;

        const int serverPort = 3000;

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
                    Arguments = $"{commandForm.CommandArgs} {serverPort}",
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

        /* Haribol! Moved from the Designer.cs file to this place. */
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                commandForm.Dispose();
                server?.Dispose();
            }

            serverThread?.Abort();

            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        public void SetupService()
        {
            if (server != null)
            {
                server.Dispose();
            }
            if (serverThread != null)
            {
                serverThread.Abort();
            }
            server = new UdpClient(serverPort);
            RunCommand();
            serverThread = new Thread(new ThreadStart(ReceiveMessages));
            serverThread.Start();
        }

        private void MainFrm_Load(object sender, EventArgs e)
        {
            if (!commandForm.HasCommand)
            {
                commandForm.ShowDialog();
            }
            SetupService();
        }

        //List<string> messages = new List<string>();

        private void ReceiveMessages()
        {
            var client = new IPEndPoint(IPAddress.Any, 0);
            while (true)
            {
                var status = Encoding.ASCII.GetString(server.Receive(ref client));
                Task.Run(() => MessageBox.Show(status));
            }
        }
    }
}
