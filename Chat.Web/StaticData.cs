using System.Collections.Concurrent;

namespace Chat.Web
{
    public static class StaticData
    {
        public static string RootPath;
        public static readonly string AuthenticationCookieName = "Chat_Authentication_Token";
        public static long JsMsToTicks(long jsMs) => 621355968000000000 + jsMs * 10000;
        public static ConcurrentDictionary<string, ActiveUser> ActiveUsers = new ConcurrentDictionary<string, ActiveUser>();
    }
    public class ActiveUser
    {
        public string Name { get; set; }
        public string ActiveGroup { get; set; }
    }
}
