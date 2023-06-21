using EmailModuleAPI.Enum;

namespace EmailModuleAPI.Model
{
    public class OtpValid
    {
        public string OTP { get; set; }
        public bool IsOTPValid { get; set; }
        public string statusCode { get; set; }
    }
}
