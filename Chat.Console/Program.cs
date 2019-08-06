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

namespace Chat.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            List<DateTime> list = new List<DateTime>()
            {
                new DateTime(9956665456323),
                new DateTime(6554655658899),
                new DateTime(10554655658899),
                new DateTime(5554655658899),
                new DateTime(6554655658899),
                new DateTime(32554655658899),
                new DateTime(1254655658899),
                new DateTime(5854655658899),
                new DateTime(7854655658899),
                new DateTime(77254655658899),
                new DateTime(58854655658899),
                new DateTime(8854655658899)
            };
            foreach(var item in list.OrderBy(o=>o))
            {
                console.WriteLine(item);
            }
            console.WriteLine();
            int max = 3;
            long ticks = 8854655658899;
            console.WriteLine(new DateTime(ticks));
            console.WriteLine();
            foreach (var date in list.Where(d => d.Ticks > ticks).OrderBy(d => d).Take(max))
            {
                console.WriteLine(date);
            }
        }
    }
}
