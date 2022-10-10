using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace getCopyWifiPassword
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            System.Windows.Forms.Form.CheckForIllegalCrossThreadCalls = false;//threadlarda çakışma olursa ben hallederim

            string wifiName = getWifiName();
            string password = getCopyWifiPassWord(wifiName);


            this.Close();
        }

        public string getWifiName()
        {
            var process = new Process
            {
                StartInfo =
    {
    FileName = "netsh.exe",
    Arguments = "wlan show interfaces",
    UseShellExecute = false,
    RedirectStandardOutput = true,
    CreateNoWindow = true
    }
            };
            process.Start();

            var output = process.StandardOutput.ReadToEnd();
            var line = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault(l => l.Contains("SSID") && !l.Contains("BSSID"));
            if (line == null)
            {
                return string.Empty;
            }
            var ssid = line.Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1].TrimStart();
            return ssid;
        }
        public string getCopyWifiPassWord(string wifiName)
        {

            string path = "WifiCopy.bat";

            File.WriteAllText("WifiCopy.bat", "netsh wlan show profile " + wifiName + " key=clear | find \"Key Content\"");
            Process process = new Process();
            ProcessStartInfo startinfo = new ProcessStartInfo(path);


            startinfo.RedirectStandardOutput = true;
            startinfo.UseShellExecute = false;
            process.StartInfo = startinfo;
            process.OutputDataReceived += (sender, args) => copyAll(wifiName, args.Data);
            //process.OutputDataReceived += (sender, args) => Console.WriteLine("ss : "+args.Data); // do whatever processing you need to do in this handler
            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();
            if (varmi == false) MessageBox.Show("Wifi bulunamadı!");

            return "";
        }

        bool varmi = false;

        public void copyAll(string wifiName, string password)
        {
            try
            {
                if (password != null && password.Contains("Key Content            :"))
                {
                    password = password.Split(':')[1].Trim();
                    MessageBox.Show("WİFİ ŞİFRENİZ KOPYALANDI \n\nWifi :   " + wifiName + "\n\nŞifre : " + password+"\n\n");
                    File.Delete("WifiCopy.bat");
                    varmi = true;

                    var t = new Thread((ThreadStart)(() =>
                    {
                        Clipboard.SetText(password);
                    }));
                    t.SetApartmentState(ApartmentState.STA);
                    t.Start();
                    t.Join();

                    return;
                }
                File.Delete("WifiCopy.bat");
            }
            catch (Exception ex)
            {
                //   Clipboard.SetText(password);
            }

        }


    }
}
