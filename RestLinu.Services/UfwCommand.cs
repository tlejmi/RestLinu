using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace RestLinu.Services
{
    public class UfwCommand : RestLinuProcess
    {
        
        public UfwCommand(ILogger<RestLinuProcess> logger) : base(logger)
        {
        }


        public async Task<ProcessResult> UfwStatus()
        {
            const string bin = "ufw"; 
            const string command = "status";
            var process = new Process();

            process.StartInfo.FileName = bin;

            process.StartInfo.Arguments = command; 

            var result = await ExecuteShellCommand(process, 1000);

            return result; 
        }
    }
}