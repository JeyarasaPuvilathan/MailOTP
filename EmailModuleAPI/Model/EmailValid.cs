using EmailModuleAPI.Enum;

namespace EmailModuleAPI.Model
{
    public class EmailValid
    {
        public int? UserId { get; set; }
        public string Email { get; set; }
        public bool IsEmailValid { get; set; }
        public string statusCode { get; set; }

    }
}
