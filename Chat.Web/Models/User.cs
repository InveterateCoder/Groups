namespace Chat.Web.Models
{
    public class User
    {
        public ChatterersDb.Chatterer Chatterer { get; set; }
        public string Email { get => Chatterer?.Email; set { if (Chatterer != null) Chatterer.Email = value; else Throw(); } }
        public string Name { get => Chatterer?.Name; set { if (Chatterer != null) Chatterer.Name = value; else Throw(); } }
        public string Password { get => Chatterer?.Password; set { if (Chatterer != null) Chatterer.Password = value; else Throw(); } }
        public string Group { get => Chatterer?.Group; set { if (Chatterer != null) Chatterer.Group = value; else Throw(); } }
        public string GroupPassword { get => Chatterer?.GroupPassword; set { if (Chatterer != null) Chatterer.GroupPassword = value; else Throw(); } }
        public string InGroup { get => Chatterer?.InGroup; set { if (Chatterer != null) Chatterer.InGroup = value; else Throw(); } }
        public string InGroupPassword { get => Chatterer?.InGroupPassword; set { if (Chatterer != null) Chatterer.InGroupPassword = value; else Throw(); } }
        public string Token { get => Chatterer?.Token; set { if (Chatterer != null) Chatterer.Token = value; else Throw(); } }
        private void Throw() => throw new System.Exception("User is not signed");
    }
}
