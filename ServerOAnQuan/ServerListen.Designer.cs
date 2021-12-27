
namespace ServerOAnQuan
{
    partial class ServerListen
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.bt_StartListen = new System.Windows.Forms.Button();
            this.tb_Nofitication = new System.Windows.Forms.TextBox();
            this.tb_Port = new System.Windows.Forms.TextBox();
            this.lb_Port = new System.Windows.Forms.Label();
            this.tb_IP = new System.Windows.Forms.TextBox();
            this.lb_IP = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // bt_StartListen
            // 
            this.bt_StartListen.Location = new System.Drawing.Point(335, 10);
            this.bt_StartListen.Name = "bt_StartListen";
            this.bt_StartListen.Size = new System.Drawing.Size(138, 23);
            this.bt_StartListen.TabIndex = 1;
            this.bt_StartListen.Text = "Bắt đầu nhận kết nối";
            this.bt_StartListen.UseVisualStyleBackColor = true;
            this.bt_StartListen.Click += new System.EventHandler(this.bt_StartListen_Click);
            // 
            // tb_Nofitication
            // 
            this.tb_Nofitication.Location = new System.Drawing.Point(12, 41);
            this.tb_Nofitication.Multiline = true;
            this.tb_Nofitication.Name = "tb_Nofitication";
            this.tb_Nofitication.ReadOnly = true;
            this.tb_Nofitication.Size = new System.Drawing.Size(461, 256);
            this.tb_Nofitication.TabIndex = 2;
            // 
            // tb_Port
            // 
            this.tb_Port.Location = new System.Drawing.Point(171, 12);
            this.tb_Port.Name = "tb_Port";
            this.tb_Port.Size = new System.Drawing.Size(63, 20);
            this.tb_Port.TabIndex = 6;
            this.tb_Port.Text = "8080";
            // 
            // lb_Port
            // 
            this.lb_Port.AutoSize = true;
            this.lb_Port.Location = new System.Drawing.Point(136, 15);
            this.lb_Port.Name = "lb_Port";
            this.lb_Port.Size = new System.Drawing.Size(29, 13);
            this.lb_Port.TabIndex = 7;
            this.lb_Port.Text = "Port:";
            // 
            // tb_IP
            // 
            this.tb_IP.Location = new System.Drawing.Point(38, 12);
            this.tb_IP.Name = "tb_IP";
            this.tb_IP.Size = new System.Drawing.Size(83, 20);
            this.tb_IP.TabIndex = 8;
            this.tb_IP.Text = "192.168.100.7";
            // 
            // lb_IP
            // 
            this.lb_IP.AutoSize = true;
            this.lb_IP.Location = new System.Drawing.Point(12, 15);
            this.lb_IP.Name = "lb_IP";
            this.lb_IP.Size = new System.Drawing.Size(20, 13);
            this.lb_IP.TabIndex = 9;
            this.lb_IP.Text = "IP:";
            // 
            // ServerListen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(485, 309);
            this.Controls.Add(this.lb_IP);
            this.Controls.Add(this.tb_IP);
            this.Controls.Add(this.lb_Port);
            this.Controls.Add(this.tb_Port);
            this.Controls.Add(this.tb_Nofitication);
            this.Controls.Add(this.bt_StartListen);
            this.Name = "ServerListen";
            this.Text = "Server Listen";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button bt_StartListen;
        private System.Windows.Forms.TextBox tb_Nofitication;
        private System.Windows.Forms.TextBox tb_Port;
        private System.Windows.Forms.Label lb_Port;
        private System.Windows.Forms.TextBox tb_IP;
        private System.Windows.Forms.Label lb_IP;
    }
}