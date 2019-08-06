using System;
using System.Collections.Concurrent;

namespace Chat.Web.Models
{
    public class ActiveUsers
    {
        public class User
        {
            public string Name { get; set; }
            public string ConnectionId { get; set; }
            public DateTime LastActive { get; set; }
        }
        public ConcurrentBag<User> Users = new System.Collections.Concurrent.ConcurrentBag<User>();
    }
}
