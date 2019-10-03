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
            [MaxLength(64)]
            public string Name { get; set; }
            [MaxLength(256)]
            public string Email { get; set; }
            [MaxLength(32)]
            public string Password { get; set; }
            public long LastActive { get; set; }
            [MaxLength(64)]
            public string Group { get; set; }
            public long GroupLastCleaned { get; set; }
            [MaxLength(32)]
            public string GroupPassword { get; set; }
            public int InGroupId { get; set; }
            [MaxLength(32)]
            public string InGroupPassword { get; set; }
            [MaxLength(50)]
            public string Token { get; set; }
            [MaxLength(64)]
            public string ConnectionId { get; set; }
            public string WebSubscription { get; set; }
            public long LastNotified { get; set; }
            public byte[] IPAddress { get; set; }
        }
        public class Message
        {
            public int Id { get; set; }
            public long SharpTime { get; set; }
            public long JsTime { get; set; }
            [MaxLength(64)]
            public string StringTime { get; set; }
            [MaxLength(64)]
            public string From { get; set; }
            [MaxLength(10000)]
            public string Text { get; set; }
            [Required]
            public int GroupId { get; set; }
        }
        public ChatterersDb(DbContextOptions<ChatterersDb> opts) : base(opts) { }
        public DbSet<Chatterer> Chatterers { get; set; }
        public DbSet<Message> Messages { get; set; }
    }
}
