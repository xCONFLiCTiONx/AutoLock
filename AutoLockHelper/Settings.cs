using Microsoft.Win32;
using System;
using System.Windows.Forms;

namespace AutoLockHelper
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();

            logoutTimeBox.SelectedIndex = 3;

            logoutTimeBox.SelectedIndexChanged += LogoutTimeBox_SelectedIndexChanged;

            logoutTimeBox.KeyDown += LogoutTimeBox_KeyDown;
        }

        private void LogoutTimeBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
            {
                SaveSettings();
            }
        }

        private void LogoutTimeBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SaveSettings();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SaveSettings();

            MessageBox.Show("Your settings have been saved successfully!" + Environment.NewLine + Environment.NewLine + "Next time AutoLock is started, the new settings will be applied.", "AutoLock Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void SaveSettings()
        {
            try
            {
                RegistryKey key = Registry.LocalMachine.CreateSubKey("SOFTWARE\\AutoLock");
                key.SetValue("LogoutTime", TimeSpan.Parse(logoutTimeBox.Text));
                key.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "AutoLock Settings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
