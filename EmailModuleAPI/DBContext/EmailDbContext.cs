using EmailModuleAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmailModuleAPI.DBContext
{
    public class EmailDbContext : DbContext
    {
        public EmailDbContext(DbContextOptions<EmailDbContext> options) : base(options)
        {

        }
        public DbSet<User> User { get; set; }

        public DbSet<UserOtp> userOtp { get; set; }
    }
}