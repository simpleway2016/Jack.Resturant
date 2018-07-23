using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Jack.Resturant.MvcCore.Test
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
             //.UseKestrel(option => {
             //    option.Listen(System.Net.IPAddress.Any, 9888, (lop) => {
             //        lop.UseHttps("server.pfx", "linezero");
             //    });
             //})
            .UseUrls("http://*:9888")
                .UseStartup<Startup>()
                .Build();
    }
}
