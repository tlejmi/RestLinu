using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace RestLinu.Services.bash
{
    public class BashCommand : RestLinuProcess
    {
        public BashCommand(ILogger<RestLinuProcess> logger) : base(logger)
        {
        }
        
        public async Task<ProcessResult> Execute(string bin,string arguements)
        {
            if (string.IsNullOrWhiteSpace(bin))
            {
                return new ProcessResult()
                {
                    IsProcessSuccessful = false,
                    Output = "Empty File Name",
                    ExitCode = 2,
                    IsErrorOutput = true,
                    Result = ResultType.ProcessError
                };
            }
            var process = new Process {StartInfo = {FileName = bin}};
            if (!string.IsNullOrWhiteSpace(arguements))
            {
                process.StartInfo.Arguments = arguements;
            }
         
            var result = await ExecuteShellCommand(process);

            return result; 
            
        }
    }
}