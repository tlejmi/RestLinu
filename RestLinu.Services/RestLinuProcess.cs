using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace RestLinu.Services
{
    public abstract class RestLinuProcess
    {
        protected readonly ILogger<RestLinuProcess> Logger;
        protected RestLinuProcess(ILogger<RestLinuProcess> logger)
        {
            Logger = logger;
        }

        protected async Task<ProcessResult> ExecuteShellCommand(Process process, int timeout)
        {
            var result = new ProcessResult();

            using (process){
                process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;

            var outputBuilder = new StringBuilder();
            var outputCloseEvent = new TaskCompletionSource<bool>();
            
            void OnProcessOnOutputDataReceived(object s, DataReceivedEventArgs e)
            {
                if (e.Data == null)
                {
                    outputCloseEvent.SetResult(true);
                }
                else
                {
                    outputBuilder.AppendLine(e.Data);
                }
            }
            
            process.OutputDataReceived += OnProcessOnOutputDataReceived;
            
            
            
            var errorBuilder = new StringBuilder();
            var errorCloseEvent = new TaskCompletionSource<bool>();
            void OnProcessOnErrorDataReceived(object s, DataReceivedEventArgs e)
            {
                if (e.Data == null)
                {
                    errorCloseEvent.SetResult(true);
                }
                else
                {
                    errorBuilder.AppendLine(e.Data);
                }
            }
            process.ErrorDataReceived += OnProcessOnErrorDataReceived;
            
            bool isStarted;

            try
            {
                isStarted = process.Start();
            }
            catch (Exception error)
            {
                Logger.LogError(error,$"unable to Start process {process.StartInfo.FileName} {process.StartInfo.Arguments} ");
                result.Completed = true;
                result.ExitCode = -1;
                result.Output = error.Message;
                isStarted = false;
            }

            if (!isStarted) return result;
            
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            
            var waitForExit = WaitForExitAsync(process, timeout);
            
            var processTask = Task.WhenAll(waitForExit, outputCloseEvent.Task, errorCloseEvent.Task);
            
            if (await Task.WhenAny(Task.Delay(timeout), processTask) == processTask && waitForExit.Result)
            {
                result.Completed = true;
                result.ExitCode = process.ExitCode;
                
                result.Output = process.ExitCode != 0 ? 
                    $"{errorBuilder.ToString().Trim()}" : 
                    $"{outputBuilder.ToString().Trim()}";
            }
            else
            {
                try
                {
                    process.Kill();
                }
                catch(Exception e)
                {
                    Logger.LogError(e,$"unable to kill process {process.StartInfo.FileName} {process.Id} ");
                }
            }
            return result;
            }
        }

        private Task<bool> WaitForExitAsync(Process process, int timeout)
        {
            return Task.Run(() => process.WaitForExit(timeout));
        }
        
        public struct ProcessResult
        {
            public bool Completed;
            public int? ExitCode;
            public string Output;
        }
    }
}