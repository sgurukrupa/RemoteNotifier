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

namespace RemoteNotifier
{
    public partial class CommandForm : Form
    {
        private const string commandFile = "process.txt";

        public CommandForm()
        {
            InitializeComponent();
            try
            {
                var lines = File.ReadAllLines(commandFile);
                CommandText = lines.ElementAtOrDefault(0);
                RunDirectory = lines.ElementAtOrDefault(1);
                CommandArgs = lines.ElementAtOrDefault(2);
            }
            catch (FileNotFoundException)
            { }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public bool HasCommand => !String.IsNullOrWhiteSpace(CommandText);

        public string CommandText { get; private set; }
        public string RunDirectory { get; private set; }
        public string CommandArgs { get; private set; }

        private void btnSave_Click(object sender, EventArgs e)
        {
            CommandText = txtCommand.Text;
            RunDirectory = txtDirectory.Text;
            CommandArgs = txtArguments.Text;
            try
            {
                File.WriteAllLines(commandFile, new string[] { CommandText, RunDirectory, CommandArgs });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            DialogResult = DialogResult.OK;
            Hide();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Restore();
            DialogResult = DialogResult.Cancel;
            Hide();
        }

        private void Restore()
        {
            txtCommand.Text = CommandText;
            txtDirectory.Text = RunDirectory;
            txtArguments.Text = CommandArgs;
        }

        private void CommandForm_Load(object sender, EventArgs e)
        {
            Restore();
        }
    }
}
