using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using RestLinu.Services.apt;
using RestLinu.Services.ufw;

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
                .AddSingleton<AptCommand>()
                .BuildServiceProvider();
            
            var bar = serviceProvider.GetService<UfwCommand>();
            var result = await bar.UfwStatus(1);
            System.Console.WriteLine(result.IsProcessSuccessful);
            System.Console.WriteLine(result.Output);
            System.Console.WriteLine(result.Result);
            System.Console.WriteLine(result.ExitCode);
            System.Console.WriteLine(result.IsErrorOutput);
            
            
           // var aptCommand = serviceProvider.GetService<AptCommand>();

            // var command = await aptCommand.IsInstalled("ufw");
            // System.Console.WriteLine(command.IsProcessSuccessful);
            // System.Console.WriteLine(command.Output);
            // System.Console.WriteLine(command.Result);
            // System.Console.WriteLine(command.ExitCode);
            
            //
            // var command = await aptCommand.IsInstalled("nginx");
            // System.Console.WriteLine(command.IsProcessSuccessful);
            // System.Console.WriteLine(command.Output);
            // System.Console.WriteLine(command.Result);
            // System.Console.WriteLine(command.ExitCode);
            // System.Console.WriteLine(command.IsErrorOutput);



        }
    }
}