using System.ComponentModel.DataAnnotations;

namespace jwtAuth_Identity_.Model
{
    public class AuthDto
    {
        public class RegisterDTO
        {
            public string FullName { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public string PhoneNumber { get; set; }

        }
        public class LoginDto
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        public class AuthResponseDto
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public string Token { get; set; }
            public UserDto User { get; set; }
        }

        public class UserDto
        {
            public long Id { get; set; }
            public string FullName { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
        }

        public class ChangePasswordDto
        {
            public string CurrentPassword { get; set; }
            public string NewPassword { get; set; }

            [Required]
            [Compare ("NewPassword",ErrorMessage = "رمز عبور جدید و تکرار آن یکسان نیستند") ]
            public string ConfirmNewPassword { get; set; }
        }

    }
}
