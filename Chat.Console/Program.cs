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
using System.Linq;
using System.Collections.Generic;
using Chat.Web;

namespace Chat.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            console.WriteLine(1568277275355);
            long ticks = StaticData.JsMsToTicks(1568277275355);
            console.WriteLine(ticks);
            console.WriteLine(StaticData.TicksToJsMs(ticks));
        }
    }
}
