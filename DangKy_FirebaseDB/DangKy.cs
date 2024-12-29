using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DangKy_FirebaseDB
{
    public partial class DangKy : Form
    {
        private static readonly HttpClient client = new HttpClient();

        public DangKy()
        {
            InitializeComponent();
        }

        public bool checkAccount(string ac)
        {
            return Regex.IsMatch(ac, @"^[a-zA-Z0-9]{6,24}$");
        }

        public bool checkPassword(string pw)
        {
            return Regex.IsMatch(pw, @"^[a-zA-Z0-9!@#$%^&*()_+]{6,24}$");
        }

        public bool checkEmail(string em)
        {
            return Regex.IsMatch(em, @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$");
        }

        private async void bt_registry_Click(object sender, EventArgs e)
        {
            var data = new Register
            {
                Username = tb_username.Text,
                Password = tb_password.Text,
                Email = tb_email.Text
            };

            try
            {
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                if(tb_confirmpassword.Text != data.Password)
                {
                    MessageBox.Show("Mật khẩu xác nhận không khớp", "Thông báo", MessageBoxButtons.OK);
                    return;
                }
                var response = await client.PostAsync("https://localhost:7029/api/account/register", content);
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Đăng kí thành công, chúc bạn có một trải nghiệm vui vẻ!", "Thông báo", MessageBoxButtons.OK);
                    this.Hide();
                }
                else
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Đăng kí thất bại: {responseContent}", "Thông báo", MessageBoxButtons.RetryCancel);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Đã xảy ra lỗi: {ex.Message}", "Thông báo", MessageBoxButtons.OK);
            }
        }

        private void DangKy_Load(object sender, EventArgs e)
        {
            this.AutoScaleMode = AutoScaleMode.None;
            this.AutoSize = false; // Tắt tự động thay đổi kích thước
            this.CenterToScreen();
        }

        private void bt_hidepw_Click(object sender, EventArgs e)
        {
            if (tb_password.PasswordChar == '\0')
            {
                tb_password.PasswordChar = '*';
                bt_showpw.BringToFront();
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

        private void bt_hideConfirmpw_Click(object sender, EventArgs e)
        {
            if (tb_confirmpassword.PasswordChar == '\0')
            {
                tb_confirmpassword.PasswordChar = '*';
                bt_showConfirmpw.BringToFront();
            }
        }

        private void bt_showConfirmpw_Click(object sender, EventArgs e)
        {
            if (tb_confirmpassword.PasswordChar == '*')
            {
                tb_confirmpassword.PasswordChar = '\0';
                bt_hideConfirmpw.BringToFront();
            }
        }
    }

}

