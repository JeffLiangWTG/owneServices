using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hawking.CSI.Tracking.Services;
using Hawking.CSI.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Hawking.CSI.Tracking.Controllers
{
    [Route("v1/events")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly IEventStreamClient eventStreamClient;
        private readonly ILogger logger;

        public EventsController(IEventStreamClient eventStreamClient, ILogger<EventsController> logger)
        {
            this.eventStreamClient = eventStreamClient;
            this.logger = logger;
        }

        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        [HttpPost]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Post()
        {
            var messageHeaders = Request.GetCustomHeaders();
            await eventStreamClient.PublishEventAsync(messageHeaders);
            return Ok();
        }
    }
}
