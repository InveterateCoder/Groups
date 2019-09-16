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
            Dictionary<string, long> test = new Dictionary<string, long>()
            {
                ["Third"] = DateTime.UtcNow.Subtract(TimeSpan.FromDays(3)).Ticks,
                ["Forth"] = DateTime.UtcNow.Subtract(TimeSpan.FromDays(4)).Ticks,
                ["First"] = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks,
                ["Second"] = DateTime.UtcNow.Subtract(TimeSpan.FromDays(2)).Ticks
            };
            foreach(var i in test.OrderBy(i => i.Value))
                console.WriteLine(i);
        }
    }
}
