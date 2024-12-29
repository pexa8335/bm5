namespace DangKy_FirebaseDB
{
    partial class DangNhap
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DangNhap));
            this.tb_username = new System.Windows.Forms.TextBox();
            this.tb_password = new System.Windows.Forms.TextBox();
            this.bt_login = new System.Windows.Forms.Button();
            this.lb_username = new System.Windows.Forms.Label();
            this.llb_registry = new System.Windows.Forms.LinkLabel();
            this.lb_password = new System.Windows.Forms.Label();
            this.llb_forgotpw = new System.Windows.Forms.LinkLabel();
            this.bt_hide = new System.Windows.Forms.Button();
            this.bt_show = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // tb_username
            // 
            this.tb_username.BackColor = System.Drawing.Color.White;
            this.tb_username.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tb_username.Font = new System.Drawing.Font("Cascadia Code", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.tb_username.Location = new System.Drawing.Point(215, 313);
            this.tb_username.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tb_username.Name = "tb_username";
            this.tb_username.Size = new System.Drawing.Size(302, 27);
            this.tb_username.TabIndex = 0;
            this.tb_username.Text = "daithang";
            // 
            // tb_password
            // 
            this.tb_password.BackColor = System.Drawing.Color.White;
            this.tb_password.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tb_password.Font = new System.Drawing.Font("Cascadia Code", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.tb_password.Location = new System.Drawing.Point(215, 366);
            this.tb_password.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tb_password.Name = "tb_password";
            this.tb_password.PasswordChar = '*';
            this.tb_password.Size = new System.Drawing.Size(302, 27);
            this.tb_password.TabIndex = 1;
            this.tb_password.Text = "123123";
            // 
            // bt_login
            // 
            this.bt_login.BackColor = System.Drawing.Color.Sienna;
            this.bt_login.Font = new System.Drawing.Font("Cascadia Mono", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.bt_login.ForeColor = System.Drawing.Color.White;
            this.bt_login.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.bt_login.Location = new System.Drawing.Point(291, 401);
            this.bt_login.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.bt_login.Name = "bt_login";
            this.bt_login.Size = new System.Drawing.Size(144, 43);
            this.bt_login.TabIndex = 2;
            this.bt_login.Text = "LOGIN";
            this.bt_login.UseVisualStyleBackColor = false;
            this.bt_login.Click += new System.EventHandler(this.bt_login_Click);
            // 
            // lb_username
            // 
            this.lb_username.AutoSize = true;
            this.lb_username.BackColor = System.Drawing.Color.Transparent;
            this.lb_username.Font = new System.Drawing.Font("Cascadia Code", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lb_username.ForeColor = System.Drawing.Color.Black;
            this.lb_username.Location = new System.Drawing.Point(217, 290);
            this.lb_username.Name = "lb_username";
            this.lb_username.Size = new System.Drawing.Size(100, 22);
            this.lb_username.TabIndex = 3;
            this.lb_username.Text = "Username:";
            // 
            // llb_registry
            // 
            this.llb_registry.AutoSize = true;
            this.llb_registry.BackColor = System.Drawing.Color.Transparent;
            this.llb_registry.Font = new System.Drawing.Font("Cascadia Code", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.llb_registry.ForeColor = System.Drawing.SystemColors.ControlText;
            this.llb_registry.LinkColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.llb_registry.Location = new System.Drawing.Point(235, 444);
            this.llb_registry.Name = "llb_registry";
            this.llb_registry.Size = new System.Drawing.Size(80, 22);
            this.llb_registry.TabIndex = 4;
            this.llb_registry.TabStop = true;
            this.llb_registry.Text = "Sign up";
            this.llb_registry.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llb_registry_LinkClicked);
            // 
            // lb_password
            // 
            this.lb_password.AutoSize = true;
            this.lb_password.BackColor = System.Drawing.Color.Transparent;
            this.lb_password.Font = new System.Drawing.Font("Cascadia Code", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.lb_password.ForeColor = System.Drawing.Color.Black;
            this.lb_password.Location = new System.Drawing.Point(217, 343);
            this.lb_password.Name = "lb_password";
            this.lb_password.Size = new System.Drawing.Size(100, 22);
            this.lb_password.TabIndex = 5;
            this.lb_password.Text = "Password:";
            // 
            // llb_forgotpw
            // 
            this.llb_forgotpw.AutoSize = true;
            this.llb_forgotpw.BackColor = System.Drawing.Color.Transparent;
            this.llb_forgotpw.Font = new System.Drawing.Font("Cascadia Code", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.llb_forgotpw.LinkColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.llb_forgotpw.Location = new System.Drawing.Point(365, 444);
            this.llb_forgotpw.Name = "llb_forgotpw";
            this.llb_forgotpw.Size = new System.Drawing.Size(160, 22);
            this.llb_forgotpw.TabIndex = 6;
            this.llb_forgotpw.TabStop = true;
            this.llb_forgotpw.Text = "Forgot password";
            this.llb_forgotpw.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llb_forgetedpw_LinkClicked);
            // 
            // bt_hide
            // 
            this.bt_hide.BackColor = System.Drawing.Color.WhiteSmoke;
            this.bt_hide.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bt_hide.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.bt_hide.Image = ((System.Drawing.Image)(resources.GetObject("bt_hide.Image")));
            this.bt_hide.Location = new System.Drawing.Point(491, 366);
            this.bt_hide.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.bt_hide.Name = "bt_hide";
            this.bt_hide.Size = new System.Drawing.Size(27, 27);
            this.bt_hide.TabIndex = 20;
            this.bt_hide.UseVisualStyleBackColor = false;
            this.bt_hide.Click += new System.EventHandler(this.bt_hide_Click);
            // 
            // bt_show
            // 
            this.bt_show.BackColor = System.Drawing.Color.WhiteSmoke;
            this.bt_show.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bt_show.Image = ((System.Drawing.Image)(resources.GetObject("bt_show.Image")));
            this.bt_show.Location = new System.Drawing.Point(491, 366);
            this.bt_show.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.bt_show.Name = "bt_show";
            this.bt_show.Size = new System.Drawing.Size(27, 27);
            this.bt_show.TabIndex = 5;
            this.bt_show.UseVisualStyleBackColor = false;
            this.bt_show.Click += new System.EventHandler(this.bt_show_Click);
            // 
            // DangNhap
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(735, 650);
            this.Controls.Add(this.bt_show);
            this.Controls.Add(this.bt_hide);
            this.Controls.Add(this.llb_forgotpw);
            this.Controls.Add(this.lb_password);
            this.Controls.Add(this.llb_registry);
            this.Controls.Add(this.lb_username);
            this.Controls.Add(this.bt_login);
            this.Controls.Add(this.tb_password);
            this.Controls.Add(this.tb_username);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "DangNhap";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "BOMB MASTER";
            this.Load += new System.EventHandler(this.DangNhap_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tb_username;
        private System.Windows.Forms.TextBox tb_password;
        private System.Windows.Forms.Button bt_login;
        private System.Windows.Forms.Label lb_username;
        private System.Windows.Forms.LinkLabel llb_registry;
        private System.Windows.Forms.Label lb_password;
        private System.Windows.Forms.LinkLabel llb_forgotpw;
        private System.Windows.Forms.Button bt_hide;
        private System.Windows.Forms.Button bt_show;
    }
}