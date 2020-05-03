using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace RestLinu.Services
{
    public enum ResultType : byte
    {
        Completed = 0,
        CancelledAndKilled = 1,
        CancelledAndNotKilled = 2,
        ProcessError = 3
    }
    public abstract class RestLinuProcess
    {
        protected readonly ILogger<RestLinuProcess> Logger;
        protected RestLinuProcess(ILogger<RestLinuProcess> logger)
        {
            Logger = logger;
        }

        protected async Task<ProcessResult> ExecuteShellCommand(Process process, int timeout = default)
        {
            var result = new ProcessResult();

            using (process)
            {
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
                        outputCloseEvent.SetResult(true);
                    else
                        outputBuilder.AppendLine(e.Data);
                }

                process.OutputDataReceived += OnProcessOnOutputDataReceived;


                var errorBuilder = new StringBuilder();
                var errorCloseEvent = new TaskCompletionSource<bool>();

                void OnProcessOnErrorDataReceived(object s, DataReceivedEventArgs e)
                {
                    if (e.Data == null)
                        errorCloseEvent.SetResult(true);
                    else
                        errorBuilder.AppendLine(e.Data);
                }

                process.ErrorDataReceived += OnProcessOnErrorDataReceived;

                bool isStarted;

                try
                {
                    isStarted = process.Start();
                }
                catch (Exception error)
                {
                    Logger.LogError(error,
                        $"unable to Start process {process.StartInfo.FileName} {process.StartInfo.Arguments} ");
                    result.Result = ResultType.ProcessError;
                    result.ExitCode = -1;
                    result.Output = error.Message;
                    isStarted = false;
                }

                if (!isStarted) return result;

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                Task processTask = null;
                var resultTask = false;
                try
                {
                    // if timeout is not specified 
                    if (timeout == default)
                    {
                        var waitForExit = WaitForExitAsync(process);
                        processTask = Task.WhenAll(waitForExit, outputCloseEvent.Task, errorCloseEvent.Task);
                        resultTask = await Task.WhenAny(processTask) == processTask && waitForExit.IsCompleted;
                    }
                    // if timeout is specified => kill in time out  
                    else
                    {
                        var waitForExit = WaitForExitAsync(process, timeout);
                        processTask = Task.WhenAll(waitForExit, outputCloseEvent.Task, errorCloseEvent.Task);
                        resultTask = await Task.WhenAny(Task.Delay(timeout), processTask) == processTask &&
                                     waitForExit.Result;
                    }
                }
                catch (Exception e)
                {
                    outputBuilder.AppendLine(processTask != null ? processTask.Exception?.Message : e.Message);
                    result.Output = outputBuilder.ToString().Trim();
                    result.Result = ResultType.ProcessError;
                }

                if (resultTask)
                {
                    result.IsProcessSuccessful = processTask.IsCompletedSuccessfully;
                    result.Result = ResultType.Completed;
                    result.ExitCode = process.ExitCode;

                    if (process.ExitCode == 0)
                    {
                        result.IsErrorOutput = false;
                        result.Output = $"{outputBuilder.ToString().Trim()}";
                    }
                    else
                    {
                        result.IsErrorOutput = true;
                        result.Output = $"{errorBuilder.ToString().Trim()}";
                    }


                    return result;
                }

                if (processTask != null && processTask.IsCanceled)
                    outputBuilder.AppendLine(
                        $"Timeout {timeout} Reached for {process.StartInfo.FileName}  {process.StartInfo.Arguments}");

                result.IsProcessSuccessful = false;
                result.ExitCode = 1;
                try
                {
                    process.Kill();
                    outputBuilder.AppendLine(
                        $"Success :Process '{process.StartInfo.FileName} {process.StartInfo.Arguments}' with ID '{process.Id}' is Killed ");
                    result.Result = ResultType.CancelledAndKilled;
                }
                catch (Exception e)
                {
                    outputBuilder.AppendLine(
                        $"Error : Process '{process.StartInfo.FileName} {process.StartInfo.Arguments}' with ID : '{process.Id}' is Killed ");
                    Logger.LogError(e, $"unable to kill process {process.StartInfo.FileName} {process.Id} ");
                    result.Result = ResultType.CancelledAndNotKilled;
                }

                result.Output = outputBuilder.ToString().Trim();

                return result;
            }
        }

        /// <summary>
        ///     Waiting for exit with a specific timeout
        /// </summary>
        /// <param name="process"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        private static Task<bool> WaitForExitAsync(Process process, int timeout)
        {
            return Task.Run(() => process.WaitForExit(timeout));
        }

        private static Task WaitForExitAsync(Process process)
        {
            return Task.Run(process.WaitForExit);
        }
    }
    public class ProcessResult
    {
       
        public bool IsProcessSuccessful { get; set; }
        public ResultType Result  { get; set; }
        public int? ExitCode  { get; set; }
        public string Output  { get; set; }

        public string[] OutputLines  =>
            !string.IsNullOrWhiteSpace(this.Output) ? this.Output.Split(new[] { "\r\n" , "\n" }, StringSplitOptions.None)
                .ToArray() : new string[]{};
        
        
        public bool IsErrorOutput { get; set; }
    }
}