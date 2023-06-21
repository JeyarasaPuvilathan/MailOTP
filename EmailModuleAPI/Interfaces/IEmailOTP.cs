using EmailModuleAPI.Entities;
using EmailModuleAPI.Model;

namespace EmailModuleAPI.Interfaces
{
    public interface IEmailOTP
    {
        Task<EmailValid> checkEmailValid(String email);

        Task<OtpValid> CheckOTP(string otp, string email);
    }
}
