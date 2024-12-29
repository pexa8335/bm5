using FireSharp;
using FireSharp.Config;
using FireSharp.Interfaces;
using Microsoft.Extensions.Configuration;
using Server.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FireSharp.Response;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.DataProtection.KeyManagement;


namespace Server.Services
{
    public class FirebaseService : IFirebaseService
    {
        private readonly IFirebaseClient _client;
        private readonly string _key = "DaylaKeyRatDaiCuaQuangtien160505";
        public FirebaseService(IConfiguration configuration)
        {
            var config = new FirebaseConfig
            {
                AuthSecret = "ptadAFZjKIegVxEFzWhRrhn5VUj0qbWM0upbVKEa",
                BasePath = "https://bombmaster-14f3a-default-rtdb.asia-southeast1.firebasedatabase.app"
            };
            _client = new FireSharp.FirebaseClient(config);
        }
        private string EncryptPassword(string password)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(_key.PadRight(32));  // Sử dụng key 32 bytes
                aes.IV = new byte[16];  // IV cố định cho đơn giản

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(password);
                    }

                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }
        private string DecryptPassword(string encryptedPassword)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(_key.PadRight(32));
                aes.IV = new byte[16];

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(encryptedPassword)))
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                {
                    return srDecrypt.ReadToEnd();
                }
            }
        }
        public async Task<RegisterResult> RegisterUser(Register user)
        {
            var response = await _client.GetAsync("Users/" + user.Username);
            if (response.Body != "null")
            {
                return new RegisterResult { Success = false, Message = "Tên tài khoản đã tồn tại. Vui lòng đặt tên khác!" };
            }
            var emailResponse = await _client.GetAsync("Users");
            var users = emailResponse.ResultAs<Dictionary<string, Register>>();
            if (users != null && users.Values.Any(u => u.Email == user.Email))
            {
                return new RegisterResult { Success = false, Message = "Email này đã được đăng ký!" };
            }

            // Mã hóa mật khẩu trước khi lưu
            user.Password = EncryptPassword(user.Password);

            var setResponse = await _client.SetAsync("Users/" + user.Username, user);
            if (setResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return new RegisterResult { Success = true, Message = "Tạo tài khoản thành công. Chúc bạn có trải nghiệm game vui vẻ!" };
            }
            else
            {
                return new RegisterResult { Success = false, Message = "Không thể tạo tài khoản." };
            }
        }


        public async Task<LoginResult> LoginUser(string username, string password)
        {
            try
            {
                var response = await _client.GetAsync("Users/" + username);
                if (response.Body == "null")
                {
                    return new LoginResult { Success = false, Message = "Tài khoản không tồn tại" };
                }

                var user = response.ResultAs<Register>();

                // Mã hóa password người dùng nhập vào
                string encryptedInputPassword = EncryptPassword(password);

                // So sánh trực tiếp với password đã mã hóa trong database
                if (user.Password == encryptedInputPassword)
                {
                    return new LoginResult { Success = true, Message = "Đăng nhập thành công" };
                }
                else
                {
                    // Để debug, in ra các giá trị (chỉ dùng trong quá trình phát triển)
                    Console.WriteLine($"Stored password: {user.Password}");
                    Console.WriteLine($"Input encrypted: {encryptedInputPassword}");
                    return new LoginResult { Success = false, Message = "Sai mật khẩu" };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
                return new LoginResult { Success = false, Message = "Lỗi đăng nhập: " + ex.Message };
            }
        }

        public async Task<UpdatePasswordResult> UpdatePasswordInFirebase(string username, string newPassword)
        {
            var response = await _client.GetAsync("Users/" + username);
            if (response.Body == "null")
            {
                return new UpdatePasswordResult { Success = false, Message = "Tài khoản không tồn tại." };
            }

            var user = response.ResultAs<Register>();
            string currentDecryptedPassword = DecryptPassword(user.Password);

            // So sánh mật khẩu mới với mật khẩu cũ đã giải mã
            if (currentDecryptedPassword == newPassword)
            {
                return new UpdatePasswordResult { Success = false, Message = "Mật khẩu mới giống với mật khẩu cũ." };
            }

            // Mã hóa mật khẩu mới
            string encryptedNewPassword = EncryptPassword(newPassword);

            var path = $"Users/{username}/Password";
            var setResponse = await _client.SetAsync(path, encryptedNewPassword);
            if (setResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return new UpdatePasswordResult { Success = true, Message = "Mật khẩu đã được cập nhật thành công." };
            }
            else
            {
                return new UpdatePasswordResult { Success = false, Message = "Không thể cập nhật mật khẩu." };
            }
        }

        public async Task<bool> IsEmailExists(string email)
        {
            var emailResponse = await _client.GetAsync("Users");
            var users = emailResponse.ResultAs<Dictionary<string, Register>>();
            return users != null && users.Values.Any(u => u.Email == email);
        }

        public async Task SaveVerificationCodeToFirebase(string email, string verificationCode)
        {
            var path = $"VerificationCodes/{email.Replace(".", ",")}";
            await _client.SetAsync(path, verificationCode);
        }

        public async Task<VerificationCodeResult> GetVerificationCodeFromFirebase(string email)
        {
            // Retrieve all users
            var emailResponse = await _client.GetAsync("Users");
            var users = emailResponse.ResultAs<Dictionary<string, Register>>();

            if (users == null || users.Count == 0)
            {
                return new VerificationCodeResult { Success = false, Message = "Không thể truy cập dữ liệu người dùng." };
            }

            // Find the user with the matching email
            var userEntry = users.FirstOrDefault(u => u.Value.Email == email);

            if (string.IsNullOrEmpty(userEntry.Key))
            {
                return new VerificationCodeResult { Success = false, Message = "Email không tồn tại." };
            }

            var username = userEntry.Key;

            // Get the verification code
            var path = $"VerificationCodes/{email.Replace(".", ",")}";
            var response = await _client.GetAsync(path);
            var verificationCode = response.ResultAs<string>();

            if (string.IsNullOrEmpty(verificationCode))
            {
                return new VerificationCodeResult { Success = false, Message = "Không tìm thấy mã xác nhận.", Username = username };
            }
            return new VerificationCodeResult { Success = true, VerificationCode = verificationCode, Username = username };
        }
    }

}