using System.ComponentModel;

namespace EmailModuleAPI.Enum
{
    public static class StatusCode
    {
     public static string STATUS_EMAIL_OK = "Email containing OTP has been sent successfully.";

     public static string STATUS_EMAIL_FAIL = "Email address does not exist or sending to the email has failed.";

     public static string STATUS_EMAIL_INVALID = "Email address is invalid.";

     public static string STATUS_OTP_OK = "OTP is valid and checked.";

     public static string STATUS_OTP_FAIL = "OTP is wrong after 10 tries.";

      public static string STATUS_OTP_INVALID = "OTP is invalid.";

      public static string STATUS_OTP_TIMEOUT = "Timeout after 1 min";

    }
}
