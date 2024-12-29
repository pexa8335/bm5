using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace DangKy_FirebaseDB
{
    public partial class DatLaiMatKhau : Form
    {
        private readonly HttpClient _httpClient;
        private readonly string _username;

        public DatLaiMatKhau(string username)
        {
            InitializeComponent();
            _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7029/api/") };
            _username = username;
        }

        private async void bt_changepw_Click(object sender, EventArgs e)
        {
            string newPassword = tb_password.Text;
            string confirmPassword = tb_confirmpw.Text;

            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 6)
            {
                MessageBox.Show("Mật khẩu mới phải có ít nhất 6 ký tự.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (newPassword != confirmPassword)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var result = await UpdatePasswordInFirebase(_username, newPassword);
            if (result.Success)
            {
                MessageBox.Show("Mật khẩu đã được cập nhật thành công.", "Thông báo", MessageBoxButtons.OK);
                DangNhap dangNhap = new DangNhap();
                dangNhap.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show($"Không thể cập nhật mật khẩu: {result.Message}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<RegisterResult> UpdatePasswordInFirebase(string username, string newPassword)
        {
            var request = new UpdatePasswordRequest { Username = username, NewPassword = newPassword };
            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("account/update-password", content);
                var responseString = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<RegisterResult>(responseString);
            }
            catch (HttpRequestException ex)
            {
                return new RegisterResult { Success = false, Message = $"Lỗi kết nối: {ex.Message}" };
            }
        }

        private void bt_showpw_Click(object sender, EventArgs e)
        {
            if (tb_password.PasswordChar == '*')
            {
                tb_password.PasswordChar = '\0';
                bt_hidepw.BringToFront();
            }
        }

        private void bt_hidepw_Click(object sender, EventArgs e)
        {
            if (tb_password.PasswordChar == '\0')
            {
                tb_password.PasswordChar = '*';
                bt_showpw.BringToFront();
            }
        }

        private void bt_showConfirmpw_Click(object sender, EventArgs e)
        {
            if (tb_confirmpw.PasswordChar == '*')
            {
                tb_confirmpw.PasswordChar = '\0';
                bt_hideConfirmpw.BringToFront();
            }
        }

        private void bt_hideConfirmpw_Click(object sender, EventArgs e)
        {
            if (tb_confirmpw.PasswordChar == '\0')
            {
                tb_confirmpw.PasswordChar = '*';
                bt_showConfirmpw.BringToFront();
            }
        }

        private void DatLaiMatKhau_Load(object sender, EventArgs e)
        {
            this.AutoScaleMode = AutoScaleMode.None;
            this.AutoSize = false; // Tắt tự động thay đổi kích thước

        }

        private void lb_resetpw_Click(object sender, EventArgs e)
        {

        }
    }

    public class RegisterResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class UpdatePasswordRequest
    {
        public string Username { get; set; }
        public string NewPassword { get; set; }
    }
}


