using Server.Models;

namespace Server.Services
{
    //firebase service là 1 object gồm tên dki, tên dnhap, updatepassword, isemailexist,...
    public interface IFirebaseService
    {
        Task<RegisterResult> RegisterUser(Register user);
        Task<LoginResult> LoginUser(string username, string password);
        Task<UpdatePasswordResult> UpdatePasswordInFirebase(string username, string newPassword);
        Task<bool> IsEmailExists(string email);
        Task SaveVerificationCodeToFirebase(string email, string verificationCode);
        Task<VerificationCodeResult> GetVerificationCodeFromFirebase(string email); 
    }
    public class LoginRequest
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
    }

    public class UpdatePasswordRequest
    {
        public required string Username { get; set; }
        public required string NewPassword { get; set; }
    }
    public class VerificationCodeRequest
    {
        public string Email { get; set; }
    }

    public class VerifyCodeRequest
    {
        public string Email { get; set; }
        public string Code { get; set; }
    }
    public class RegisterResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
    public class LoginResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
    public class UpdatePasswordResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
    public class VerificationCodeResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string VerificationCode { get; set; }
        public string Username { get; set; }
    }
}
