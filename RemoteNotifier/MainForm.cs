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
using System.Media;
using RemoteNotifier.Properties;

namespace RemoteNotifier
{
    public partial class MainFrm : Form
    {
        public readonly CommandForm commandForm = new CommandForm();
        private UdpClient server;
        private Thread serverThread;
        private Process process;

        private void StopProcess()
        {
            if (process != null)
            {
                try
                {
                    process.Kill();
                    process.WaitForExit();
                }
                catch (Exception)
                { }
                process.Dispose();
                process = null;
            }
        }

        const int serverPort = 3000;

        public void RunCommand()
        {
            try
            {
                StopProcess();
                if (!commandForm.HasCommand)
                {
                    return;
                }

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

        private readonly SoundPlayer sound;
        private int notificationCount = 0;

        public MainFrm()
        {
            InitializeComponent();
            sound = new SoundPlayer(Resources.Alarm01);
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
            server?.Dispose();
            serverThread?.Abort();
            server = new UdpClient(serverPort);
            serverThread = new Thread(new ThreadStart(ReceiveMessages));
            serverThread.Start();
            RunCommand();
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
                Task.Run(() =>
                {
                    if (Interlocked.Increment(ref notificationCount) == 1)
                    {
                        sound.PlayLooping();
                    }
                    MessageBox.Show(status);
                    if (Interlocked.Decrement(ref notificationCount) == 0)
                    {
                        sound.Stop();
                    }
                });
            }
        }

        private void MainFrm_FormClosed(object sender, FormClosedEventArgs e)
        {
            StopProcess();
        }
    }
}
