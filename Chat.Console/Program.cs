using System;
using System.Security.Cryptography;
using System.Text;
using System.Net.Mail;
using console = System.Console;
using System.IO;
using System.Timers;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;

namespace Chat.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            CookieContainer container = new CookieContainer();
            HttpClientHandler handler = new HttpClientHandler() { CookieContainer = container };
            using (HttpClient client = new HttpClient(handler, true))
            {
                
            }
        }
    }
}
