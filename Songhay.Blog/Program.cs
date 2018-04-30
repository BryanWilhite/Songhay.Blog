using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo(assemblyName: "Songhay.Blog.Tests")]

namespace Songhay.Blog
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args, builderAction: null).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args, Action<WebHostBuilderContext, IConfigurationBuilder> builderAction) =>
            WebHost
                .CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((builderContext, configBuilder) =>
                {
                    builderAction?.Invoke(builderContext, configBuilder);
                    configBuilder.AddJsonFile(conventionalSettingsFile, optional: false);
                })
                .UseStartup<Startup>()
                ;

        internal const string conventionalSettingsFile = "app-settings.songhay-system.json";
    }
}
