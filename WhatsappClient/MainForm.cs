using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text;
using System.Runtime.InteropServices;


namespace WhatsAppClient
{
    public partial class MainForm : Form
    {
        private string ServerIP = "whatsnet.arcy-v.win";
        const int WM_VSCROLL = 0x0115;
        const int SB_THUMBPOSITION = 4;
        const int SB_VERT = 1;
        const int WM_SETREDRAW = 0x000B;
        private static readonly HttpClient httpClient = new HttpClient();

        [DllImport("user32.dll")]
        static extern int GetScrollPos(IntPtr hWnd, int nBar);

        [DllImport("user32.dll")]
        static extern int SetScrollPos(IntPtr hWnd, int nBar, int nPos, bool bRedraw);

        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
        private string sessionId;
        private Timer timerActualizarChats = new Timer();

        public MainForm(string sessionId)
        {
            InitializeComponent();
            this.sessionId = sessionId;

            this.AutoScaleMode = AutoScaleMode.Dpi;
            //this.AutoSize = true;
            //this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        }

        private void ActualizarMensajesSinParpadeo(RichTextBox rtb, string texto, int posicionScrollAnterior)
        {
            // Suspender redibujado
            SendMessage(rtb.Handle, WM_SETREDRAW, (IntPtr)0, IntPtr.Zero);

            rtb.Text = texto;

            // Restaurar posición scroll vertical
            SetScrollPos(rtb.Handle, SB_VERT, posicionScrollAnterior, true);
            SendMessage(rtb.Handle, WM_VSCROLL, (IntPtr)(SB_THUMBPOSITION + 0x10000 * posicionScrollAnterior), IntPtr.Zero);

            // Reactivar redibujado
            SendMessage(rtb.Handle, WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);
            rtb.Invalidate();
        }


        private void IniciarTimerDeChats()
        {
            timerActualizarChats.Interval = 1000;
            timerActualizarChats.Tick += TimerActualizarChats_Tick;
            timerActualizarChats.Start();
        }

        private async void TimerActualizarChats_Tick(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(sessionId)) return;

            try
            {
                string response = await httpClient.GetStringAsync($"http://{ServerIP}/session/{sessionId}/chats");
                var chats = JArray.Parse(response);

                int scrollIndex = listBoxChats.TopIndex;
                bool estabaAbajo = (listBoxChats.TopIndex + listBoxChats.ClientSize.Height / listBoxChats.ItemHeight) >= listBoxChats.Items.Count;

                var selected = listBoxChats.SelectedItem as ChatItem;

                listBoxChats.BeginUpdate();
                listBoxChats.Items.Clear();

                int indexToSelect = -1;
                for (int i = 0; i < chats.Count; i++)
                {
                    string name = chats[i]["name"].ToString();
                    string id = chats[i]["id"].ToString();
                    var item = new ChatItem { Name = name, Id = id };
                    listBoxChats.Items.Add(item);

                    if (selected != null && id == selected.Id)
                        indexToSelect = i;
                }

                if (indexToSelect != -1)
                    listBoxChats.SelectedIndex = indexToSelect;

                listBoxChats.EndUpdate();

                // Restaurar scroll
                if (estabaAbajo)
                {
                    listBoxChats.TopIndex = listBoxChats.Items.Count - 1;
                }
                else if (scrollIndex >= 0 && scrollIndex < listBoxChats.Items.Count)
                {
                    listBoxChats.TopIndex = scrollIndex;
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show("Error al actualizar chats: " + ex.Message);
            }
        }


        private async void listBoxChats_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxChats.SelectedItem is ChatItem chat)
            {
                try
                {
                    string response = await httpClient.GetStringAsync($"http://{ServerIP}/session/{sessionId}/messages/{chat.Id}");
                    var json = JObject.Parse(response);
                    string userId = json["userId"].ToString();
                    var messages = (JArray)json["messages"];

                    int scrollPos = GetScrollPos(richTextBoxMensajes.Handle, SB_VERT);

                    StringBuilder sb = new StringBuilder();
                    foreach (var msg in messages)
                    {
                        string from = msg["from"].ToString();
                        string body = msg["body"].ToString();
                        string who = from == userId ? "Yo" : from;

                        sb.AppendLine($"{who}: {body}");
                    }
                    userNameShow.Text = listBoxChats.SelectedItem.ToString();
                    ActualizarMensajesSinParpadeo(richTextBoxMensajes, sb.ToString(), scrollPos);
                }
                catch (HttpRequestException ex)
                {
                    MessageBox.Show("Error al obtener mensajes: " + ex.Message);
                }
            }
        }



        private async void btnEnviar_Click(object sender, EventArgs e)
        {
            if (listBoxChats.SelectedItem is ChatItem chat)
            {
                string mensaje = txtMensaje.Text;
                var json = $"{{\"to\":\"{chat.Id}\",\"message\":\"{mensaje}\"}}";
                txtMensaje.Text = "";
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    try
                    {
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        var response = await client.PostAsync($"http://{ServerIP}/session/{sessionId}/send", content);

                        if (!response.IsSuccessStatusCode)
                        {
                            string responseBody = await response.Content.ReadAsStringAsync();
                            MessageBox.Show($"Error al enviar mensaje:\n{response.StatusCode}\n{responseBody}");
                        }
                    }
                    catch (HttpRequestException ex)
                    {
                        MessageBox.Show("Error de red: " + ex.Message);
                    }
                }
            }
        }


        class ChatItem
        {
            public string Name { get; set; }
            public string Id { get; set; }
            public override string ToString() => Name;
        }
        private void MainForm_Load(object sender, EventArgs e)
        {
            IniciarTimerDeChats();
        }
        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            timerActualizarChats?.Stop();
            timerActualizarChats?.Dispose();
            Environment.Exit(0);
            Application.Exit(); // Cierra toda la aplicación, no solo el formulario
        }

    }
}
