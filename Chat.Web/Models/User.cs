﻿using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Chat.Web.Models
{
    public class User
    {
        public ChatterersDb Database { get; set; }
        public ChatterersDb.Chatterer Chatterer { get; set; }
        public DbSet<ChatterersDb.Chatterer> Chatterers { get => Database?.Chatterers; }
        public string Email { get => Chatterer?.Email; set { if (Chatterer != null) Chatterer.Email = value; else Throw(); } }
        public string Name { get => Chatterer?.Name; set { if (Chatterer != null) Chatterer.Name = value; else Throw(); } }
        public string Password { get => Chatterer?.Password; set { if (Chatterer != null) Chatterer.Password = value; else Throw(); } }
        public string Group { get => Chatterer?.Group; set { if (Chatterer != null) Chatterer.Group = value; else Throw(); } }
        public string GroupPassword { get => Chatterer?.GroupPassword; set { if (Chatterer != null) Chatterer.GroupPassword = value; else Throw(); } }
        public int InGroupId { get => Chatterer.InGroupId; set { if (Chatterer != null) Chatterer.InGroupId = value; else Throw(); } }
        public string InGroupPassword { get => Chatterer?.InGroupPassword; set { if (Chatterer != null) Chatterer.InGroupPassword = value; else Throw(); } }
        public string Token { get => Chatterer?.Token; set { if (Chatterer != null) Chatterer.Token = value; else Throw(); } }
        public string ConnectionId { get => Chatterer?.ConnectionId; set { if (Chatterer != null) Chatterer.ConnectionId = value; else Throw(); } }
        public string WebSubscription { get => Chatterer?.WebSubscription; set { if (Chatterer != null) Chatterer.WebSubscription = value; else Throw(); } }
        public WebPush.PushSubscription PushSubscription
        {
            get
            {
                string subscription = WebSubscription;
                if (subscription == null)
                    return null;
                else
                    return Subscription.FromJson(subscription).FormWebPushSubscription();
            }
        }
        public async Task SaveAsync() => await Database?.SaveChangesAsync();
        private void Throw() => throw new System.Exception("User is not signed");
    }
    public class Subscription
    {
        public struct KeysStruct
        {
            public string P256DH;
            public string Auth;
        }
        public string Endpoint;
        public string ExpirationTime;
        public KeysStruct Keys;
        public WebPush.PushSubscription FormWebPushSubscription()
        {
            return new WebPush.PushSubscription
            {
                Auth = Keys.Auth,
                P256DH = Keys.P256DH,
                Endpoint = Endpoint
            };
        }
        public static Subscription FromJson(string json) => Newtonsoft.Json.JsonConvert.DeserializeObject<Subscription>(json);
        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }
}
