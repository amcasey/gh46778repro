using ClassLibrary1;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApplication1.Controllers
{
    [Route("echo")]
    [ApiController]
    public class EchoController : ControllerBase
    {
        ILogger<EchoController> _logger;

        public EchoController(ILogger<EchoController> logger)
        {
            _logger = logger;
            _logger.LogInformation($"Constructor called");
        }

        // POST api/<TestController>
        [HttpGet]
        public string Get()
        {
            _logger.LogInformation($"Get called");
            return $"Hello from {this.GetType().FullName}.  Send me a post.";
        }
        
        // POST api/<TestController>
        [HttpPost]
        public EchoPayload Post([FromBody] EchoPayload thePayload)
        {
            _logger.LogInformation($"Post called");
            return thePayload;
        }
    }
}
