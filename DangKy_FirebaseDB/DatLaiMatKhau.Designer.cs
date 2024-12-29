namespace DangKy_FirebaseDB
{
    partial class DatLaiMatKhau
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DatLaiMatKhau));
            this.tb_password = new System.Windows.Forms.TextBox();
            this.lb_password = new System.Windows.Forms.Label();
            this.bt_changepw = new System.Windows.Forms.Button();
            this.lb_confirmpw = new System.Windows.Forms.Label();
            this.tb_confirmpw = new System.Windows.Forms.TextBox();
            this.bt_showpw = new System.Windows.Forms.Button();
            this.bt_hidepw = new System.Windows.Forms.Button();
            this.bt_showConfirmpw = new System.Windows.Forms.Button();
            this.bt_hideConfirmpw = new System.Windows.Forms.Button();
            this.lb_resetpw = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // tb_password
            // 
            this.tb_password.BackColor = System.Drawing.Color.Gainsboro;
            this.tb_password.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tb_password.Font = new System.Drawing.Font("Cascadia Code", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.tb_password.Location = new System.Drawing.Point(40, 101);
            this.tb_password.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tb_password.Name = "tb_password";
            this.tb_password.PasswordChar = '*';
            this.tb_password.Size = new System.Drawing.Size(385, 27);
            this.tb_password.TabIndex = 0;
            // 
            // lb_password
            // 
            this.lb_password.AutoSize = true;
            this.lb_password.Font = new System.Drawing.Font("Cascadia Code", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.lb_password.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.lb_password.Location = new System.Drawing.Point(36, 78);
            this.lb_password.Name = "lb_password";
            this.lb_password.Size = new System.Drawing.Size(200, 22);
            this.lb_password.TabIndex = 2;
            this.lb_password.Text = "Nhập mật khẩu mới: ";
            // 
            // bt_changepw
            // 
            this.bt_changepw.BackColor = System.Drawing.Color.Sienna;
            this.bt_changepw.Font = new System.Drawing.Font("Cascadia Code", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.bt_changepw.ForeColor = System.Drawing.Color.AliceBlue;
            this.bt_changepw.Location = new System.Drawing.Point(155, 199);
            this.bt_changepw.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.bt_changepw.Name = "bt_changepw";
            this.bt_changepw.Size = new System.Drawing.Size(132, 41);
            this.bt_changepw.TabIndex = 4;
            this.bt_changepw.Text = "XÁC NHẬN";
            this.bt_changepw.UseVisualStyleBackColor = false;
            this.bt_changepw.Click += new System.EventHandler(this.bt_changepw_Click);
            // 
            // lb_confirmpw
            // 
            this.lb_confirmpw.AutoSize = true;
            this.lb_confirmpw.Font = new System.Drawing.Font("Cascadia Code", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.lb_confirmpw.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.lb_confirmpw.Location = new System.Drawing.Point(36, 139);
            this.lb_confirmpw.Name = "lb_confirmpw";
            this.lb_confirmpw.Size = new System.Drawing.Size(200, 22);
            this.lb_confirmpw.TabIndex = 6;
            this.lb_confirmpw.Text = "Xác nhận mật khẩu: ";
            // 
            // tb_confirmpw
            // 
            this.tb_confirmpw.BackColor = System.Drawing.Color.Gainsboro;
            this.tb_confirmpw.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tb_confirmpw.Font = new System.Drawing.Font("Cascadia Code", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.tb_confirmpw.Location = new System.Drawing.Point(40, 162);
            this.tb_confirmpw.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tb_confirmpw.Name = "tb_confirmpw";
            this.tb_confirmpw.PasswordChar = '*';
            this.tb_confirmpw.Size = new System.Drawing.Size(385, 27);
            this.tb_confirmpw.TabIndex = 5;
            // 
            // bt_showpw
            // 
            this.bt_showpw.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.bt_showpw.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bt_showpw.Image = ((System.Drawing.Image)(resources.GetObject("bt_showpw.Image")));
            this.bt_showpw.Location = new System.Drawing.Point(395, 101);
            this.bt_showpw.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.bt_showpw.Name = "bt_showpw";
            this.bt_showpw.Size = new System.Drawing.Size(31, 27);
            this.bt_showpw.TabIndex = 8;
            this.bt_showpw.UseVisualStyleBackColor = false;
            this.bt_showpw.Click += new System.EventHandler(this.bt_showpw_Click);
            // 
            // bt_hidepw
            // 
            this.bt_hidepw.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.bt_hidepw.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bt_hidepw.Image = ((System.Drawing.Image)(resources.GetObject("bt_hidepw.Image")));
            this.bt_hidepw.Location = new System.Drawing.Point(395, 101);
            this.bt_hidepw.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.bt_hidepw.Name = "bt_hidepw";
            this.bt_hidepw.Size = new System.Drawing.Size(31, 27);
            this.bt_hidepw.TabIndex = 21;
            this.bt_hidepw.UseVisualStyleBackColor = false;
            this.bt_hidepw.Click += new System.EventHandler(this.bt_hidepw_Click);
            // 
            // bt_showConfirmpw
            // 
            this.bt_showConfirmpw.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.bt_showConfirmpw.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bt_showConfirmpw.Image = ((System.Drawing.Image)(resources.GetObject("bt_showConfirmpw.Image")));
            this.bt_showConfirmpw.Location = new System.Drawing.Point(395, 162);
            this.bt_showConfirmpw.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.bt_showConfirmpw.Name = "bt_showConfirmpw";
            this.bt_showConfirmpw.Size = new System.Drawing.Size(31, 27);
            this.bt_showConfirmpw.TabIndex = 22;
            this.bt_showConfirmpw.UseVisualStyleBackColor = false;
            this.bt_showConfirmpw.Click += new System.EventHandler(this.bt_showConfirmpw_Click);
            // 
            // bt_hideConfirmpw
            // 
            this.bt_hideConfirmpw.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.bt_hideConfirmpw.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bt_hideConfirmpw.Image = ((System.Drawing.Image)(resources.GetObject("bt_hideConfirmpw.Image")));
            this.bt_hideConfirmpw.Location = new System.Drawing.Point(395, 162);
            this.bt_hideConfirmpw.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.bt_hideConfirmpw.Name = "bt_hideConfirmpw";
            this.bt_hideConfirmpw.Size = new System.Drawing.Size(31, 27);
            this.bt_hideConfirmpw.TabIndex = 23;
            this.bt_hideConfirmpw.UseVisualStyleBackColor = false;
            this.bt_hideConfirmpw.Click += new System.EventHandler(this.bt_hideConfirmpw_Click);
            // 
            // lb_resetpw
            // 
            this.lb_resetpw.BackColor = System.Drawing.Color.Transparent;
            this.lb_resetpw.Font = new System.Drawing.Font("Segoe UI Black", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.lb_resetpw.ForeColor = System.Drawing.Color.Maroon;
            this.lb_resetpw.Location = new System.Drawing.Point(41, 31);
            this.lb_resetpw.Name = "lb_resetpw";
            this.lb_resetpw.Size = new System.Drawing.Size(316, 39);
            this.lb_resetpw.TabIndex = 28;
            this.lb_resetpw.Text = "RESET PASSWORD";
            this.lb_resetpw.Click += new System.EventHandler(this.lb_resetpw_Click);
            // 
            // DatLaiMatKhau
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.DarkKhaki;
            this.ClientSize = new System.Drawing.Size(460, 257);
            this.Controls.Add(this.lb_resetpw);
            this.Controls.Add(this.bt_showConfirmpw);
            this.Controls.Add(this.bt_hideConfirmpw);
            this.Controls.Add(this.bt_showpw);
            this.Controls.Add(this.bt_hidepw);
            this.Controls.Add(this.lb_confirmpw);
            this.Controls.Add(this.tb_confirmpw);
            this.Controls.Add(this.bt_changepw);
            this.Controls.Add(this.lb_password);
            this.Controls.Add(this.tb_password);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "DatLaiMatKhau";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "BOMB MASTER";
            this.Load += new System.EventHandler(this.DatLaiMatKhau_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tb_password;
        private System.Windows.Forms.Label lb_password;
        private System.Windows.Forms.Button bt_changepw;
        private System.Windows.Forms.Label lb_confirmpw;
        private System.Windows.Forms.TextBox tb_confirmpw;
        private System.Windows.Forms.Button bt_showpw;
        private System.Windows.Forms.Button bt_hidepw;
        private System.Windows.Forms.Button bt_showConfirmpw;
        private System.Windows.Forms.Button bt_hideConfirmpw;
        private System.Windows.Forms.Label lb_resetpw;
    }
}