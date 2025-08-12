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
using System.Threading.Tasks;

namespace WhatsAppClient
{
    public partial class MainForm : Form
    {
        private Timer timerResizeDebounce = new Timer();
        private Timer timerActualizarMensajesNuevos = new Timer();
        private bool timerActualizarMensajesNuevosEjecutandose = false;
        private string chatActualId = null;
        private HashSet<string> mensajesMostrados = new HashSet<string>();
        private NotifyIcon notifyIcon;
        private Dictionary<string, string> ultimosMensajes = new Dictionary<string, string>();
        private Timer timerNuevosMensajes = new Timer();
        private bool timerNuevosMensajesEjecutandose = false;
        private bool timerNuevosMensajesIniciado = false;
        private string ServerIP = "whatsnet.arcy-v.win";
  
        private static readonly HttpClient httpClient = new HttpClient();
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
        private void MainForm_Load(object sender, EventArgs e)
        {
            IniciarTimerDeChats();

            timerNuevosMensajes.Interval = 3000; // cada 3 segundos
            timerNuevosMensajes.Tick += TimerNuevosMensajes_Tick;

            notifyIcon = new NotifyIcon
            {
                Icon = SystemIcons.Information, // Podés cambiarlo por un ícono tuyo
                Visible = true
            };
            // En el constructor o en Load()
            notifyIcon.BalloonTipClicked += NotifyIcon_BalloonTipClicked;
            timerActualizarMensajesNuevos.Interval = 1500; // por ejemplo cada 3 segundos
            timerActualizarMensajesNuevos.Tick += TimerActualizarMensajesNuevos_Tick;
            timerActualizarMensajesNuevos.Start();
            timerResizeDebounce.Interval = 200; // 200 ms tras el último resize
            timerResizeDebounce.Tick += TimerResizeDebounce_Tick;
        }
        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            timerActualizarChats?.Stop();
            timerActualizarChats?.Dispose();
            timerNuevosMensajes?.Stop();
            notifyIcon?.Dispose();
            Environment.Exit(0);
            Application.Exit(); // Cierra toda la aplicación, no solo el formulario
        }
        private async void listBoxChats_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxChats.SelectedItem is ChatItem chat)
            {
                userNameShow.Text = listBoxChats.SelectedItem.ToString();
                if (chatActualId != chat.Id)
                {
                    // Cambió de chat: limpiar y cargar todo desde cero
                    chatActualId = chat.Id;
                    mensajesMostrados.Clear();
                    flowLayoutPanel1.Controls.Clear();

                    await CargarMensajesCompleto(chat.Id);
                }
            }
        }
        private async Task CargarMensajesCompleto(string chatId)
        {
            try
            {
                string response = await httpClient.GetStringAsync($"http://{ServerIP}/session/{sessionId}/messages/{chatId}");
                var json = JObject.Parse(response);
                string userId = json["userId"].ToString();
                var messages = (JArray)json["messages"];

                foreach (var msg in messages)
                {
                    string msgId = msg["id"]?.ToString();
                    if (mensajesMostrados.Contains(msgId))
                        continue; // por si acaso

                    AgregarMensajeAlPanel(msg, userId);
                    mensajesMostrados.Add(msgId);
                }
            }
            catch
            {
                // Manejar error
            }
        }
        private void AgregarMensajeAlPanel(JToken msg, string userId)
        {
            string body = msg["body"].ToString();
            string from = msg["from"].ToString();
            string fromName = msg["fromName"]?.ToString();
            string timestampStr = msg["timestamp"]?.ToString();

            bool isMine = from == userId;
            bool isGroup = !string.IsNullOrEmpty(fromName) && from != userId;

            string timeFormatted = "";
            if (long.TryParse(timestampStr, out long ts))
            {
                DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                DateTime dateTime = epoch.AddSeconds(ts).ToLocalTime();
                timeFormatted = dateTime.ToString("HH:mm");
            }

            var bubble = AddMessageBubble(body, isMine, timeFormatted, fromName, isGroup);
            bubble.Tag = msg["id"]?.ToString();
        }
        
        private async Task ActualizarMensajesNuevos()
        {
            if (string.IsNullOrEmpty(chatActualId)) return;

            try
            {
                string response = await httpClient.GetStringAsync($"http://{ServerIP}/session/{sessionId}/messages/{chatActualId}");
                var json = JObject.Parse(response);
                string userId = json["userId"].ToString();
                var messages = (JArray)json["messages"];

                foreach (var msg in messages)
                {
                    string msgId = msg["id"]?.ToString();
                    if (mensajesMostrados.Contains(msgId))
                        continue; // ya mostrado

                    AgregarMensajeAlPanel(msg, userId);
                    mensajesMostrados.Add(msgId);
                }
            }
            catch
            {
                // manejar error
            }
        }
        private Panel AddMessageBubble(string text, bool isMine, string time, string senderName = null, bool isGroup = false)
        {
            int maxWidth = flowLayoutPanel1.Width - 120;
            Font msgFont = new Font("Segoe UI", 10);

            // Medir el texto para ajustar tamaño
            Size textSize = TextRenderer.MeasureText(text, msgFont, new Size(maxWidth - 50, int.MaxValue), TextFormatFlags.WordBreak);

            Panel container = new Panel
            {
                AutoSize = true,
                Dock = DockStyle.Top,
                Padding = new Padding(0),
                Margin = new Padding(0),
                Width = flowLayoutPanel1.Width - 20
            };

            Panel bubble = new Panel
            {
                AutoSize = false,
                Width = Math.Min(textSize.Width + 50, maxWidth),
                Height = textSize.Height + 30, // espacio extra para padding y hora
                BackColor = isMine ? Color.LightGreen : Color.LightGray,
                Padding = new Padding(10, isGroup && !isMine ? 20 : 10, 40, 15),
                Margin = new Padding(isMine ? 50 : 5, 5, isMine ? 5 : 50, 5)
            };

            bubble.Paint += (s, e) =>
            {
                var rect = bubble.ClientRectangle;
                int radius = 15;
                using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                    path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
                    path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
                    path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
                    path.CloseAllFigures();
                    bubble.Region = new Region(path);
                }
            };

            if (isGroup && !isMine && !string.IsNullOrEmpty(senderName))
            {
                bubble.Controls.Add(new Label
                {
                    AutoSize = true,
                    Text = senderName,
                    Font = new Font("Segoe UI", 8, FontStyle.Bold),
                    ForeColor = Color.FromArgb(0, 128, 255),
                    Location = new Point(8, 4)
                });
            }

            Label lblMessage = new Label
            {
                AutoSize = true,
                MaximumSize = new Size(maxWidth - 50, 0),
                Text = text,
                Font = msgFont,
                Location = new Point(8, isGroup && !isMine ? 20 : 4)
            };
            bubble.Controls.Add(lblMessage);

            Label lblTime = new Label
            {
                AutoSize = true,
                Text = time,
                Font = new Font("Segoe UI", 7),
                ForeColor = Color.DimGray
            };
            lblTime.Location = new Point(bubble.Width - lblTime.PreferredWidth - 8, bubble.Height - lblTime.PreferredHeight - 5);
            bubble.Controls.Add(lblTime);

            if (isMine)
            {
                bubble.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                bubble.Left = container.Width - bubble.Width - 10;
            }
            else
            {
                bubble.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                bubble.Left = 10;
            }

            container.Controls.Add(bubble);
            flowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel1.WrapContents = false;
            flowLayoutPanel1.AutoScroll = true;
            flowLayoutPanel1.Controls.Add(container);
            flowLayoutPanel1.ScrollControlIntoView(container);

            return container;
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
        protected override async void OnResize(EventArgs e)
        {
            base.OnResize(e);

            timerResizeDebounce.Stop();
            timerResizeDebounce.Start();

            if (this.WindowState == FormWindowState.Minimized)
            {
                if (!timerNuevosMensajesIniciado)
                {
                    await InicializarUltimosMensajes();
                    timerNuevosMensajes.Start();
                    timerNuevosMensajesIniciado = true;
                }
            }
            else
            {
                timerNuevosMensajes.Stop();
                timerNuevosMensajesIniciado = false;
            }
        }
        private void IniciarTimerDeChats()
        {
            timerActualizarChats.Interval = 1000;
            timerActualizarChats.Tick += TimerActualizarChats_Tick;
            timerActualizarChats.Start();
        }
        private async Task InicializarUltimosMensajes()
        {
            try
            {
                string response = await httpClient.GetStringAsync($"http://{ServerIP}/session/{sessionId}/chats");
                var chats = JArray.Parse(response);

                foreach (var chat in chats)
                {
                    string chatId = chat["id"].ToString();

                    string msgResponse = await httpClient.GetStringAsync($"http://{ServerIP}/session/{sessionId}/messages/{chatId}");
                    var json = JObject.Parse(msgResponse);
                    var messages = (JArray)json["messages"];

                    if (messages.Count > 0)
                    {
                        string ultimoBody = messages.Last["body"].ToString();
                        ultimosMensajes[chatId] = ultimoBody;
                    }
                }
            }
            catch
            {
                // Ignorar errores de red iniciales
            }
        }
        private void NotifyIcon_BalloonTipClicked(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal; // Restaurar
            this.Show(); // Asegurarse que se muestre
            this.Activate(); // Darle foco
        }
        private void TimerResizeDebounce_Tick(object sender, EventArgs e)
        {
            timerResizeDebounce.Stop();

            int maxWidth = flowLayoutPanel1.ClientSize.Width - 120;

            foreach (Control container in flowLayoutPanel1.Controls)
            {
                container.Width = flowLayoutPanel1.ClientSize.Width;

                // Buscar el panel "bubble" dentro del container (asumiendo que es el primer control)
                if (container.Controls.Count > 0)
                {
                    var bubble = container.Controls[0];

                    // Recalcular ancho del bubble para que no supere maxWidth
                    // Y que el texto se envuelva de nuevo

                    // Buscar la etiqueta del mensaje para medir texto
                    Label lblMessage = null;
                    foreach (Control c in bubble.Controls)
                    {
                        if (c is Label l && !string.IsNullOrEmpty(l.Text) && l.Font.Size == 10) // o alguna forma de identificar
                        {
                            lblMessage = l;
                            break;
                        }
                    }
                    if (lblMessage != null)
                    {
                        // Medir el texto con nuevo maxWidth
                        Size textSize = TextRenderer.MeasureText(lblMessage.Text, lblMessage.Font, new Size(maxWidth - 50, int.MaxValue), TextFormatFlags.WordBreak);

                        bubble.Width = Math.Min(textSize.Width + 50, maxWidth);
                        bubble.Height = textSize.Height + 30;

                        // Ajustar posición del tiempo
                        foreach (Control c in bubble.Controls)
                        {
                            if (c is Label lblTime && lblTime.Font.Size == 7)
                            {
                                lblTime.Location = new Point(bubble.Width - lblTime.PreferredWidth - 8, bubble.Height - lblTime.PreferredHeight - 5);
                            }
                        }

                        // Ajustar ubicación de la burbuja según si es propia o ajena
                        bool isMine = bubble.BackColor == Color.LightGreen;
                        if (isMine)
                            bubble.Left = container.Width - bubble.Width - 10;
                        else
                            bubble.Left = 10;

                        // Finalmente invalidar para que se redibuje
                        bubble.Invalidate();
                    }
                }
                container.Invalidate();
            }

            flowLayoutPanel1.PerformLayout();
        }
        private async void TimerNuevosMensajes_Tick(object sender, EventArgs e)
        {
            if (timerNuevosMensajesEjecutandose) return; // evita solapamiento

            timerNuevosMensajesEjecutandose = true;

            if (string.IsNullOrWhiteSpace(sessionId))
            {
                timerNuevosMensajesEjecutandose = false;
                return;
            }

            try
            {
                string response = await httpClient.GetStringAsync($"http://{ServerIP}/session/{sessionId}/chats");
                var chats = JArray.Parse(response);

                foreach (var chat in chats)
                {
                    string chatId = chat["id"].ToString();
                    string chatName = chat["name"].ToString();

                    string msgResponse = await httpClient.GetStringAsync($"http://{ServerIP}/session/{sessionId}/messages/{chatId}");
                    var json = JObject.Parse(msgResponse);
                    var messages = (JArray)json["messages"];

                    if (messages.Count > 0)
                    {
                        string ultimoBody = messages.Last["body"].ToString();
                        string from = messages.Last["from"].ToString();
                        string userId = json["userId"].ToString();

                        if (from != userId)
                        {
                            if (!ultimosMensajes.ContainsKey(chatId) || ultimosMensajes[chatId] != ultimoBody)
                            {
                                ultimosMensajes[chatId] = ultimoBody;

                                if (this.WindowState == FormWindowState.Minimized)
                                {
                                    notifyIcon.BalloonTipTitle = $"Nuevo mensaje de {chatName}";
                                    notifyIcon.BalloonTipText = ultimoBody;
                                    notifyIcon.ShowBalloonTip(3000);
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // Ignorar errores temporales
            }
            finally
            {
                timerNuevosMensajesEjecutandose = false;
            }
        }
        private async void TimerActualizarMensajesNuevos_Tick(object sender, EventArgs e)
        {
            if (timerActualizarMensajesNuevosEjecutandose) return; // evitar reentradas
            timerActualizarMensajesNuevosEjecutandose = true;

            try
            {
                await ActualizarMensajesNuevos();
            }
            catch
            {
                // Podés loggear o ignorar errores temporales
            }
            finally
            {
                timerActualizarMensajesNuevosEjecutandose = false;
            }
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
                listBoxChats.TopIndex = scrollIndex;
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show("Error al actualizar chats: " + ex.Message);
            }
            catch (TaskCanceledException)
            {
                // Timeout o cancelación - ignorar o loggear según convenga
                // Console.WriteLine("Petición cancelada o timeout en TimerActualizarChats_Tick.");
            }
            catch (Exception ex)
            {
                // Aquí otros errores, p.ej. red
                MessageBox.Show("Error al actualizar chats: " + ex.Message);
            }
        }
    }
}
