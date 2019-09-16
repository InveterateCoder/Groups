using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Chat.Web.Models
{
    public class ChatterersDb : DbContext
    {
        public class Chatterer
        {
            public int Id { get; set; }
            [StringLength(64, MinimumLength = 5)]
            public string Name { get; set; }
            [StringLength(256, MinimumLength = 6)]
            public string Email { get; set; }
            [StringLength(32, MinimumLength = 8)]
            public string Password { get; set; }
            public long LastActive { get; set; }
            [StringLength(64, MinimumLength = 5)]
            public string Group { get; set; }
            [StringLength(32, MinimumLength = 8)]
            public long GroupLastCleaned { get; set; }
            public string GroupPassword { get; set; }
            public int InGroupId { get; set; }
            [StringLength(32, MinimumLength = 8)]
            public string InGroupPassword { get; set; }
            [MaxLength(50)]
            public string Token { get; set; }
            [MaxLength(64)]
            public string ConnectionId { get; set; }
        }
        public class Message
        {
            public int Id { get; set; }
            public long Time { get; set; }
            [StringLength(64, MinimumLength = 5)]
            public string From { get; set; }
            [MaxLength(2048)]
            public string Text { get; set; }
            [Required]
            public int GroupId { get; set; }
        }
        public ChatterersDb(DbContextOptions<ChatterersDb> opts) : base(opts) { }
        public DbSet<Chatterer> Chatterers { get; set; }
        public DbSet<Message> Messages { get; set; }
    }
}
