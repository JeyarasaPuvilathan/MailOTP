using System.ComponentModel.DataAnnotations;

namespace EmailModuleAPI.Entities
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        public string Email { get; set; }
    }
}
