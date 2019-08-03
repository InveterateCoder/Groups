using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Chat.Web.Models
{
    public class ChatterersDbContext : DbContext
    {
        public class Message
        {
            public long Date;
            [StringLength(64, MinimumLength = 3)]
            public string From { get; set; }
            [MaxLength(2040)]
            public string Text { get; set; }
        }
        public ChatterersDbContext(DbContextOptions<ChatterersDbContext> opts) : base(opts) { }
        public DbSet<Chatterer> Chatterers { get; set; }
        public class Group
        {
            public int Id { get; set; }
            [StringLength(64, MinimumLength = 3)]
            public string Name { get; set; }
            [StringLength(64, MinimumLength = 6)]
            public string Password { get; set; }
        }
        public class Chatterer
        {
            public int Id { get; set; }
            [StringLength(64, MinimumLength = 3)]
            public string Name { get; set; }
            [StringLength(256, MinimumLength = 6)]
            public string Email { get; set; }
            [StringLength(64, MinimumLength = 6)]
            public string Password { get; set; }
            public long LastActive { get; set; }
            public Group Group { get; set; }
            public long GroupLastActive { get; set; }
            public Message[] GroupMessages { get; set; }
            public Group[] BlockedFromGroups { get; set; }
            [MaxLength(100)]
            public string Token { get; set; }
        }
    }
}
