using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RestLinu.Services.bash;

namespace RestLinu.API.Controllers
{
    [ApiController]
    [Route("bash")]
    public class BashController : ControllerBase
    {
        private readonly ILogger<BashController> _logger;
        private readonly BashCommand _bashCommand;

        public BashController(ILogger<BashController> logger,BashCommand bashCommand)
        {
            _logger = logger;
            _bashCommand = bashCommand;
        }

        [HttpPost]
        [Route("execute")]
        public async Task<string> Execute([FromBody]BashCommandQuery commandQuery)
        {
            var result = await _bashCommand.Execute(commandQuery.Bin, commandQuery.Arguements);
        
                var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                Converters =
                {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                }
            };
            return JsonSerializer.Serialize(result, options);
            // return new JsonResult(result);
        }
    }
    public class BashCommandQuery
    {
        public string Bin { get; set; }
        public string Arguements { get; set; }
    }
}