using System.Windows.Forms;
using System.Drawing;
namespace WhatsAppClient

{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.listBoxChats = new System.Windows.Forms.ListBox();
            this.txtMensaje = new System.Windows.Forms.TextBox();
            this.btnEnviar = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.richTextBoxMensajes = new System.Windows.Forms.RichTextBox();
            this.userNameShow = new System.Windows.Forms.Label();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.SuspendLayout();
            // 
            // listBoxChats
            // 
            this.listBoxChats.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listBoxChats.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listBoxChats.FormattingEnabled = true;
            this.listBoxChats.ItemHeight = 16;
            this.listBoxChats.Location = new System.Drawing.Point(38, 12);
            this.listBoxChats.Name = "listBoxChats";
            this.listBoxChats.Size = new System.Drawing.Size(230, 356);
            this.listBoxChats.TabIndex = 3;
            this.listBoxChats.SelectedIndexChanged += new System.EventHandler(this.listBoxChats_SelectedIndexChanged);
            // 
            // txtMensaje
            // 
            this.txtMensaje.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMensaje.Location = new System.Drawing.Point(277, 348);
            this.txtMensaje.Name = "txtMensaje";
            this.txtMensaje.Size = new System.Drawing.Size(371, 20);
            this.txtMensaje.TabIndex = 5;
            // 
            // btnEnviar
            // 
            this.btnEnviar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnEnviar.Location = new System.Drawing.Point(651, 348);
            this.btnEnviar.Name = "btnEnviar";
            this.btnEnviar.Size = new System.Drawing.Size(72, 23);
            this.btnEnviar.TabIndex = 6;
            this.btnEnviar.Text = "Send";
            this.btnEnviar.UseVisualStyleBackColor = true;
            this.btnEnviar.Click += new System.EventHandler(this.btnEnviar_Click);
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(4, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(28, 28);
            this.button1.TabIndex = 7;
            this.button1.Text = "WIP";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button2.Location = new System.Drawing.Point(4, 57);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(28, 28);
            this.button2.TabIndex = 8;
            this.button2.Text = "WIP";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button3.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button3.Location = new System.Drawing.Point(4, 340);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(28, 28);
            this.button3.TabIndex = 9;
            this.button3.Text = "WIP";
            this.button3.UseVisualStyleBackColor = true;
            // 
            // richTextBoxMensajes
            // 
            this.richTextBoxMensajes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBoxMensajes.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.richTextBoxMensajes.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.richTextBoxMensajes.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBoxMensajes.Location = new System.Drawing.Point(277, 35);
            this.richTextBoxMensajes.Name = "richTextBoxMensajes";
            this.richTextBoxMensajes.ReadOnly = true;
            this.richTextBoxMensajes.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.richTextBoxMensajes.Size = new System.Drawing.Size(446, 307);
            this.richTextBoxMensajes.TabIndex = 10;
            this.richTextBoxMensajes.Text = "";
            // 
            // userNameShow
            // 
            this.userNameShow.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.userNameShow.AutoSize = true;
            this.userNameShow.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.userNameShow.Location = new System.Drawing.Point(274, 15);
            this.userNameShow.Name = "userNameShow";
            this.userNameShow.Size = new System.Drawing.Size(0, 17);
            this.userNameShow.TabIndex = 11;
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.Text = "notifyIcon";
            this.notifyIcon1.Visible = true;
            // 
            // MainForm
            // 
            this.AcceptButton = this.btnEnviar;
            this.ClientSize = new System.Drawing.Size(735, 388);
            this.Controls.Add(this.userNameShow);
            this.Controls.Add(this.richTextBoxMensajes);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnEnviar);
            this.Controls.Add(this.txtMensaje);
            this.Controls.Add(this.listBoxChats);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(669, 379);
            this.Name = "MainForm";
            this.Text = "WhatsNET";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        private System.Windows.Forms.ListBox listBoxChats;
        private System.Windows.Forms.TextBox txtMensaje;
        private System.Windows.Forms.Button btnEnviar;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.RichTextBox richTextBoxMensajes;
        private Label userNameShow;
        private NotifyIcon notifyIcon1;
    }
}
