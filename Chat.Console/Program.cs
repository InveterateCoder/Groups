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
            SHA256 sha256 = SHA256.Create();
            byte[] arr = sha256.ComputeHash(Encoding.ASCII.GetBytes("What is it?"));
            string str = Convert.ToBase64String(arr);
            console.WriteLine(str.Length);
        }
    }
}
