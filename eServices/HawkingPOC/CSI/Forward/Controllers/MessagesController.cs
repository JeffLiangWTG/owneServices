using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hawking.CSI.Forward.Services;
using Hawking.CSI.Tracking.Client;
using Hawking.CSI.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Hawking.CSI.Forward.Controllers
{
    [Route("v1/messages")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IApplicationRegistryClient applicationRegistryClient;
        private readonly IForwardEnqueueService forwardEnqueueService;
        private readonly IMessageEventClient messageEventClient;
        private readonly ILogger logger;

        public MessagesController(IApplicationRegistryClient applicationRegistryClient, 
            IForwardEnqueueService forwardEnqueueService, 
            IMessageEventClient messageEventClient, 
            ILogger<MessagesController> logger)
        {
            this.applicationRegistryClient = applicationRegistryClient;
            this.forwardEnqueueService = forwardEnqueueService;
            this.messageEventClient = messageEventClient;
            this.logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            try
            {
                var messageHeaders = Request.GetCustomHeaders();
                var destinationAddress = await applicationRegistryClient.GetApplicationAddressAsync(messageHeaders, CancellationToken.None);
                await forwardEnqueueService.EnqueueMessageAsync(destinationAddress, messageHeaders, CancellationToken.None);
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error Forwarding Message");
                return BadRequest(ex.Message);
            }
        }
    }
}
