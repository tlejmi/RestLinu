using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace RestLinu.Services.ufw
{
    public class UfwCommand : RestLinuProcess
    {
        public UfwCommand(ILogger<RestLinuProcess> logger) : base(logger)
        {
        }

        public async Task<ProcessResult> UfwStatus(int timout = default)
        {
            const string bin = "ufw";
            const string arguments = "status";
            var process = new Process();
            process.StartInfo.FileName = bin;
            process.StartInfo.Arguments = arguments;
            var result = await ExecuteShellCommand(process);

            return result;
        }
    }
}