using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace DangKy_FirebaseDB
{
    public partial class QuenMatKhau : Form
    {
        private readonly HttpClient _httpClient;
        string username;

        public QuenMatKhau()
        {
            InitializeComponent();
            _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7029/api/account/") }; 
        }

        private async void bt_getVeriCode_Click(object sender, EventArgs e)
        {
            string email = tb_email.Text;

            if (!IsValidEmail(email))
            {
                MessageBox.Show("Email không hợp lệ. Vui lòng nhập đúng định dạng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var content = new StringContent(JsonConvert.SerializeObject(new { Email = email }), System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("send-verification-code", content);
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Mã xác nhận đã được gửi đến email của bạn.", "Thông báo", MessageBoxButtons.OK);
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                MessageBox.Show(errorMessage, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                var domainPart = addr.Host.Split('.');
                return addr.Address == email && domainPart.Length > 1 && domainPart[domainPart.Length - 1].Length > 1;
            }
            catch
            {
                return false;
            }
        }

        private async void bt_confirm_Click(object sender, EventArgs e)
        {
            try
            {
                string email = tb_email.Text;
                string enteredCode = tb_veriCode.Text;

                var content = new StringContent(JsonConvert.SerializeObject(new { Email = email, Code = enteredCode }), System.Text.Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("verify-code", content);
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<VerificationCodeResult>(responseString);

                    if (result != null && result.Success)
                    {
                        MessageBox.Show("Xác nhận thành công. Vui lòng đặt lại mật khẩu!", "Thông báo", MessageBoxButtons.OK);
                        DatLaiMatKhau dlmk = new DatLaiMatKhau(result.Username);
                        dlmk.Show();
                        this.Hide();
                    }
                    else
                    {
                        MessageBox.Show(result?.Message ?? "Mã xác nhận không chính xác.", "Thông báo", MessageBoxButtons.RetryCancel);
                    }
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    MessageBox.Show("Vui lòng nhập mã xác nhận!", "Thông báo", MessageBoxButtons.RetryCancel);
                }
            }
            catch
            {
                MessageBox.Show("Vui lòng nhập mã xác nhận!", "Thông báo", MessageBoxButtons.RetryCancel);
            }
        }

        private void QuenMatKhau_Load(object sender, EventArgs e)
        {
            this.AutoScaleMode = AutoScaleMode.None;
            this.AutoSize = false; // Tắt tự động thay đổi kích thước

        }
        public class VerificationCodeResult
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public string VerificationCode { get; set; }
            public string Username { get; set; }
        }
    }
}

