﻿using System;
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
            string[] arr = { "hello", "this", "me", "time", null };
            foreach (var s in arr)
                if (s != null && s.StartsWith("t"))
                    console.WriteLine(s);
        }
    }
}
