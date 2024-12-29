using Microsoft.AspNetCore.Mvc;
using Server.Models;
using Server.Services;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;

namespace Server.Controllers
{
    //tạo api
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IFirebaseService _firebaseService;

        //truyền đối tượng firebaseService
        public AccountController(IFirebaseService firebaseService)
        {
            _firebaseService = firebaseService;
        }

        //api http
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Register register)
        {
            if (register == null)
            {
                //return signal message
                return BadRequest(new { Success = false, Message = "Invalid user data." });
            }

            var result = await _firebaseService.RegisterUser(register);
            return Ok(result);
        }

        //khi bấm login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { Success = false, Message = "Invalid login data." });
            }

            var result = await _firebaseService.LoginUser(request.Username, request.Password);
            return Ok(result);
        }

        [HttpPost("update-password")]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.NewPassword))
            {
                return BadRequest(new { Success = false, Message = "Invalid request data." });
            }

            var result = await _firebaseService.UpdatePasswordInFirebase(request.Username, request.NewPassword);
            return Ok(result);
        }
        [HttpPost("send-verification-code")]
        public async Task<IActionResult> SendVerificationCode([FromBody] VerificationCodeRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Email))
            {
                return BadRequest("Invalid request data.");
            }

            if (!await _firebaseService.IsEmailExists(request.Email))
            {
                return BadRequest("Email không tồn tại.");
            }

            string verificationCode = GenerateVerificationCode();
            await SendVerificationCode(request.Email, verificationCode);
            await _firebaseService.SaveVerificationCodeToFirebase(request.Email, verificationCode);
            return Ok("Mã xác nhận đã được gửi đến email của bạn.");
        }

        private string GenerateVerificationCode()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        [HttpPost("verify-code")]
        public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Code))
            {
                return BadRequest(new { Success = false, Message = "Vui lòng nhập mã xác nhận." });
            }

            var result = await _firebaseService.GetVerificationCodeFromFirebase(request.Email);
            if (result.Success && result.VerificationCode == request.Code)
            {
                return Ok(new { Success = true, Message = "Xác nhận thành công.", Username = result.Username });
            }
            else
            {
                return BadRequest(new { Success = false, Message = result.Message ?? "Mã xác nhận không chính xác." });
            }
        }
        private async Task SendVerificationCode(string email, string verificationCode)
        {
            var message = new MailMessage();
            message.From = new MailAddress("noreplybombmaster@gmail.com", "BombMaster");
            message.To.Add(new MailAddress(email));
            message.Subject = "BombMaster: Mã xác nhận thay đổi mật khẩu.";
            message.Body = $"Mã xác nhận của bạn là: {verificationCode}, mã này là duy nhất xin đừng chia sẻ cho bất kì ai.";
            message.IsBodyHtml = true;

            using (var smtpClient = new SmtpClient("smtp.gmail.com", 587))
            {
                smtpClient.Credentials = new NetworkCredential("noreplybombmaster@gmail.com", "sgka twxe wyce smfj");
                smtpClient.EnableSsl = true;
                await smtpClient.SendMailAsync(message);
            }
        }

    }

}
