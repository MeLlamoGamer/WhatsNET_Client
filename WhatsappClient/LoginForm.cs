using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace WhatsAppClient
{
    public partial class LoginForm : Form
    {
        public string SessionId { get; private set; }
        private string ServerIP = "whatsnet.arcy-v.win";
        private Timer timerQR = new Timer();

        public LoginForm()
        {
            InitializeComponent();
            MessageBox.Show("Warning: The login process can take a few minutes to complete(First login/Server Restart only), Please be patient.");

            timerQR.Interval = 2000;
            timerQR.Tick += TimerQR1_Tick;
        }

        private async void LoginForm_Load(object sender, EventArgs e)
        {
            if (File.Exists("session_id.txt"))
            {
                SessionId = File.ReadAllText("session_id.txt");
            }
            else
            {
                SessionId = Guid.NewGuid().ToString("N").Substring(0, 10);
                File.WriteAllText("session_id.txt", SessionId);
            }

            await StartSessionAsync();
        }

        private async Task StartSessionAsync()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    await client.PostAsync($"http://{ServerIP}/session/{SessionId}", null);
                }
                timerQR.Start();
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show("Error al crear sesión: " + ex.Message);
                button1.Enabled = true;
            }
        }

        private async void TimerQR1_Tick(object sender, EventArgs e)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    string response = await client.GetStringAsync($"http://{ServerIP}/session/{SessionId}/qr");

                    var json = JObject.Parse(response);

                    if (json["qr"] != null)
                    {
                        string qrBase64 = json["qr"].ToString();
                        byte[] imageBytes = Convert.FromBase64String(qrBase64.Replace("data:image/png;base64,", ""));

                        using (var ms = new MemoryStream(imageBytes))
                        {
                            pictureBoxQR.Image = System.Drawing.Image.FromStream(ms);
                        }
                    }
                    else if (json["status"]?.ToString() == "ready")
                    {
                        timerQR.Stop();
                        OpenMainForm();
                        return;
                    }
                }
                catch (HttpRequestException ex)
                {
                    timerQR.Stop();
                    MessageBox.Show("Error al obtener QR: " + ex.Message);
                    button1.Enabled = true;
                }
            }
        }


        private void OpenMainForm()
        {
            this.Invoke((Action)(() =>
            {
                MainForm mainForm = new MainForm(SessionId);
                mainForm.Show();
                this.Hide();
            }));
        }

        private void pictureBoxQR_Click(object sender, EventArgs e)
        {

        }

        private async void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            await StartSessionAsync();
        }
    }
}
