using Microsoft.Owin.Hosting;
using System;

namespace owin_console_queuing
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string baseAddress = "http://localhost:9001/";
            WebApp.Start<Startup>(url: baseAddress);

            Console.ReadLine();
        }
    }
}