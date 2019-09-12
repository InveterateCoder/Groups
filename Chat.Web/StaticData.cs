
namespace Chat.Web
{
    public static class StaticData
    {
        public static string RootPath;
        public static readonly string AuthenticationCookieName = "Chat_Authentication_Token";
        public static long JsMsToTicks(long jsMs) => 621355968000000000 + jsMs * 10000;
        public static long TicksToJsMs(long ticks) => (ticks - 621355968000000000) / 10000;
    }
}
