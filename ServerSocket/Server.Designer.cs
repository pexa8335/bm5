namespace ServerSocket
{
    partial class Server
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            ShowingInfo = new RichTextBox();
            SuspendLayout();
            // 
            // ShowingInfo
            // 
            ShowingInfo.Location = new Point(1, 1);
            ShowingInfo.Margin = new Padding(1);
            ShowingInfo.Name = "ShowingInfo";
            ShowingInfo.Size = new Size(558, 443);
            ShowingInfo.TabIndex = 0;
            ShowingInfo.Text = "";
            ShowingInfo.TextChanged += richTextBox1_TextChanged;
            // 
            // Server
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(560, 443);
            Controls.Add(ShowingInfo);
            Margin = new Padding(2);
            Name = "Server";
            Text = "Form1";
            Load += Form1_Load;
            ResumeLayout(false);
        }

        #endregion

        private RichTextBox ShowingInfo;
    }
}
