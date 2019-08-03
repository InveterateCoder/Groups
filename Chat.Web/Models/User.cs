namespace Chat.Web.Models
{
    public class User
    {
        public string Name { get; set; }
        public string Group { get; set; }
        public string[] GroupsAuthorizedIn { get; set; }
        public string[] BlockedFrom { get; set; }
    }
}
