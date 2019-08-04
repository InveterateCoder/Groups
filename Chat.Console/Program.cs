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
            List<string> arr = new List<string>() { "hello", "boom", "looomia", "moon", "gorrial", "af", "bf", "ac", "язык", "повар", "повариха", "Я", "я", "кукла", "куколка"};
            var some = arr.OrderBy(o => o, StringComparer.OrdinalIgnoreCase).Take(35);
            console.WriteLine(string.Join(',', some));
        }
    }
}
