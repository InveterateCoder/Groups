using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Chat.Web.Models
{
    public class ChatterersDb : DbContext
    {
        public class Message
        {
            public int Id { get; set; }
            public long Date { get; set; }
            [StringLength(64, MinimumLength = 5)]
            public string From { get; set; }
            [MaxLength(2040)]
            public string Text { get; set; }
        }
        public ChatterersDb(DbContextOptions<ChatterersDb> opts) : base(opts) { }
        public DbSet<Chatterer> Chatterers { get; set; }
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
            public string GroupPassword { get; set; }
            [StringLength(64, MinimumLength = 5)]
            public string InGroup { get; set; }
            [StringLength(32, MinimumLength = 8)]
            public string InGroupPassword { get; set; }
            public Message[] GroupMessages { get; set; }
            [MaxLength(50)]
            public string Token { get; set; }
        }
    }
}
