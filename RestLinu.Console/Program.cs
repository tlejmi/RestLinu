using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RestLinu.Services;

namespace RestLinu.Console
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //setup our DI
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<UfwCommand>()
                .BuildServiceProvider();

            
            var bar = serviceProvider.GetService<UfwCommand>();
            var result = await bar.UfwStatus();

            System.Console.WriteLine(result.Output);
            System.Console.WriteLine(result.Completed);
            System.Console.WriteLine(result.ExitCode);

        }
    }
}