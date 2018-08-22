using InjectionLibrary;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Configuration;
using System.Globalization;

namespace BasicInjector
{
    public sealed partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            AllowDrop = true;
            DragDrop += Form1_DragDrop;
        }

        private int _injectionStyle;
        private IntPtr _hModule;
        private Process _process;
        private bool _injected;
        private InjectionMethod _injector;

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            _injectionStyle = comboBox1.SelectedIndex;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (_injected)
            {
                _injector.Unload(_hModule, _process.Id);

                if (_injector.GetLastError() != null)
                    MessageBox.Show(_injector.GetLastError().ToString());

                comboBox1.Enabled = true;
                textBox1.Enabled = true;
                textBox2.Enabled = true;
                button2.Enabled = true;
                checkBox1.Enabled = true;
                numericUpDown1.Enabled = true;
                _injected = false;
                button1.Text = @"Inject";
                return;
            }

            if (checkBox1.Checked)
            {
                while (Process.GetProcessesByName(textBox2.Text).Length == 0)
                {
                    Thread.Sleep(500);
                }

                _process = Process.GetProcessesByName(textBox2.Text)[0];
            }
            else
            {
                _process = Process.GetProcessesByName(textBox2.Text)[0];

                if (_process.Id == 0 && !checkBox1.Checked)
                {
                    MessageBox.Show(@"Process not found.");
                    return;
                }
            }

            Thread.Sleep((int)numericUpDown1.Value);

            _injector = InjectionMethod.Create((InjectionMethodType)_injectionStyle);
            _hModule = _injector.Inject(textBox1.Text, _process.Id);

            //if no errors, return
            if (_injector.GetLastError() == null)
            {
                comboBox1.Enabled = false;
                textBox1.Enabled = false;
                textBox2.Enabled = false;
                button2.Enabled = false;
                checkBox1.Enabled = false;
                numericUpDown1.Enabled = false;
                _injected = true;
                button1.Text = @"Eject";
                return;
            }

            MessageBox.Show(_injector.GetLastError().ToString());
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
            var directoryInfo = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).Parent;
            if (directoryInfo != null)
                textBox1.Text = $@"{directoryInfo.FullName}\source\";

            var processes = Process.GetProcesses();
            foreach (var process in processes)
            {
                if (!listBox1.Items.Contains(process.ProcessName))
                {
                    listBox1.Items.Add(process.ProcessName);
                }
            }

            LoadSettings();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var file = new OpenFileDialog();
            file.InitialDirectory = @"C:\";
            if (file.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = file.FileName;
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_Click(object sender, EventArgs e)
        {
            var file = new OpenFileDialog();
            if (file.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = file.FileName;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDown1.Enabled = checkBox1.Checked;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            var file = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
            textBox1.Text = file;
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var processes = Process.GetProcesses();
            foreach (var process in processes)
            {
                if (!listBox1.Items.Contains(process.ProcessName))
                {
                    listBox1.Items.Add(process.ProcessName);
                }
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox2.Text = listBox1.SelectedItem.ToString();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            
        }

        private void button5_Click(object sender, EventArgs e)
        {
            LoadSettings();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SaveSettings();
        }

        private void LoadSettings()
        {
            if (ConfigurationManager.AppSettings["dllpath"] != "")
                textBox1.Text = ConfigurationManager.AppSettings["dllpath"];
            textBox2.Text = ConfigurationManager.AppSettings["process"];
            checkBox1.Checked = Convert.ToBoolean(Convert.ToInt32(ConfigurationManager.AppSettings["waitforprocess"]));
            numericUpDown1.Value = Convert.ToInt32(ConfigurationManager.AppSettings["delay"]);
            comboBox1.SelectedIndex = Convert.ToInt32(ConfigurationManager.AppSettings["method"]);
        }

        private void SaveSettings()
        {
            SetSetting("dllpath", textBox1.Text);
            SetSetting("process", textBox2.Text);
            SetSetting("waitforprocess", Convert.ToInt32(checkBox1.Checked).ToString());
            SetSetting("delay", numericUpDown1.Value.ToString(CultureInfo.InvariantCulture));
            SetSetting("method", comboBox1.SelectedIndex.ToString(CultureInfo.InvariantCulture));
        }

        internal static bool SetSetting(string key, string value)
        {
            var config =
                ConfigurationManager.OpenExeConfiguration(
                    ConfigurationUserLevel.None);

            config.AppSettings.Settings.Remove(key);
            var kvElem = new KeyValueConfigurationElement(key, value);
            config.AppSettings.Settings.Add(kvElem);

            // Save the configuration file.
            config.Save(ConfigurationSaveMode.Modified);

            // Force a reload of a changed section.
            ConfigurationManager.RefreshSection("appSettings");

            return true;
        }
    }
}
