namespace AWS_S3.ViewModels
{
    public class RegisterModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class LoginModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class TwoFactorRequest
    {
        public string SecretKey { get; set; }
        public string Code { get; set; }
    }
}
