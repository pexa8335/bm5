namespace DangKy_FirebaseDB
{
    partial class QuenMatKhau
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QuenMatKhau));
            this.tb_email = new System.Windows.Forms.TextBox();
            this.bt_getVeriCode = new System.Windows.Forms.Button();
            this.bt_confirm = new System.Windows.Forms.Button();
            this.tb_veriCode = new System.Windows.Forms.TextBox();
            this.lb_email = new System.Windows.Forms.Label();
            this.lb_verifyCode = new System.Windows.Forms.Label();
            this.lb_signup = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // tb_email
            // 
            this.tb_email.BackColor = System.Drawing.Color.Gainsboro;
            this.tb_email.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tb_email.Font = new System.Drawing.Font("Cascadia Code", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.tb_email.Location = new System.Drawing.Point(43, 102);
            this.tb_email.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tb_email.Name = "tb_email";
            this.tb_email.Size = new System.Drawing.Size(377, 27);
            this.tb_email.TabIndex = 0;
            // 
            // bt_getVeriCode
            // 
            this.bt_getVeriCode.BackColor = System.Drawing.Color.Gainsboro;
            this.bt_getVeriCode.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.bt_getVeriCode.Font = new System.Drawing.Font("Segoe UI Black", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.bt_getVeriCode.ForeColor = System.Drawing.Color.Black;
            this.bt_getVeriCode.Location = new System.Drawing.Point(332, 161);
            this.bt_getVeriCode.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.bt_getVeriCode.Name = "bt_getVeriCode";
            this.bt_getVeriCode.Size = new System.Drawing.Size(87, 27);
            this.bt_getVeriCode.TabIndex = 1;
            this.bt_getVeriCode.Text = "GỬI MÃ";
            this.bt_getVeriCode.UseVisualStyleBackColor = false;
            this.bt_getVeriCode.Click += new System.EventHandler(this.bt_getVeriCode_Click);
            // 
            // bt_confirm
            // 
            this.bt_confirm.BackColor = System.Drawing.Color.Sienna;
            this.bt_confirm.Font = new System.Drawing.Font("Cascadia Code", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.bt_confirm.ForeColor = System.Drawing.Color.AliceBlue;
            this.bt_confirm.Location = new System.Drawing.Point(163, 199);
            this.bt_confirm.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.bt_confirm.Name = "bt_confirm";
            this.bt_confirm.Size = new System.Drawing.Size(120, 41);
            this.bt_confirm.TabIndex = 3;
            this.bt_confirm.Text = "XÁC NHẬN";
            this.bt_confirm.UseVisualStyleBackColor = false;
            this.bt_confirm.Click += new System.EventHandler(this.bt_confirm_Click);
            // 
            // tb_veriCode
            // 
            this.tb_veriCode.BackColor = System.Drawing.Color.Gainsboro;
            this.tb_veriCode.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tb_veriCode.Font = new System.Drawing.Font("Cascadia Code", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.tb_veriCode.Location = new System.Drawing.Point(41, 161);
            this.tb_veriCode.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tb_veriCode.Name = "tb_veriCode";
            this.tb_veriCode.Size = new System.Drawing.Size(270, 27);
            this.tb_veriCode.TabIndex = 2;
            // 
            // lb_email
            // 
            this.lb_email.AutoSize = true;
            this.lb_email.Font = new System.Drawing.Font("Cascadia Code", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.lb_email.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.lb_email.Location = new System.Drawing.Point(37, 78);
            this.lb_email.Name = "lb_email";
            this.lb_email.Size = new System.Drawing.Size(220, 22);
            this.lb_email.TabIndex = 4;
            this.lb_email.Text = "Vui lòng nhập email: ";
            // 
            // lb_verifyCode
            // 
            this.lb_verifyCode.AutoSize = true;
            this.lb_verifyCode.Font = new System.Drawing.Font("Cascadia Code", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.lb_verifyCode.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.lb_verifyCode.Location = new System.Drawing.Point(37, 138);
            this.lb_verifyCode.Name = "lb_verifyCode";
            this.lb_verifyCode.Size = new System.Drawing.Size(190, 22);
            this.lb_verifyCode.TabIndex = 5;
            this.lb_verifyCode.Text = "Nhập mã xác nhận: ";
            // 
            // lb_signup
            // 
            this.lb_signup.AutoSize = true;
            this.lb_signup.Font = new System.Drawing.Font("Segoe UI Black", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.lb_signup.ForeColor = System.Drawing.Color.Maroon;
            this.lb_signup.Location = new System.Drawing.Point(36, 31);
            this.lb_signup.Name = "lb_signup";
            this.lb_signup.Size = new System.Drawing.Size(269, 35);
            this.lb_signup.TabIndex = 27;
            this.lb_signup.Text = "FORGOT PASSWORD";
            // 
            // QuenMatKhau
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.DarkKhaki;
            this.ClientSize = new System.Drawing.Size(460, 257);
            this.Controls.Add(this.lb_signup);
            this.Controls.Add(this.lb_verifyCode);
            this.Controls.Add(this.lb_email);
            this.Controls.Add(this.bt_confirm);
            this.Controls.Add(this.tb_veriCode);
            this.Controls.Add(this.bt_getVeriCode);
            this.Controls.Add(this.tb_email);
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "QuenMatKhau";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "BOMB MASTER";
            this.Load += new System.EventHandler(this.QuenMatKhau_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tb_email;
        private System.Windows.Forms.Button bt_getVeriCode;
        private System.Windows.Forms.Button bt_confirm;
        private System.Windows.Forms.TextBox tb_veriCode;
        private System.Windows.Forms.Label lb_email;
        private System.Windows.Forms.Label lb_verifyCode;
        private System.Windows.Forms.Label lb_signup;
    }
}