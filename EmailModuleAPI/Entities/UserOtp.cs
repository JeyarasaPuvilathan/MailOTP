using System.ComponentModel.DataAnnotations;

namespace EmailModuleAPI.Entities
{
    public class UserOtp
    {
        [Key]
        public int UserId { get; set; }
        public string Otp_pass { get; set; }
        public DateTime Otp_timestamp { get; set; }
        public byte Otp_tries { get; set; }
    }
}
