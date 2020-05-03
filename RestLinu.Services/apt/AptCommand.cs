using Microsoft.Extensions.Logging;

namespace RestLinu.Services.apt
{
    public class AptCommand : RestLinuProcess
    {
        public AptCommand(ILogger<RestLinuProcess> logger) : base(logger)
        {
        }
        
    }
}