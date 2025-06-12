using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Hawking.CSI.Apps.XXXXXX.Models;
using Hawking.CSI.Apps.XXXXXX.Send.Services;
using Hawking.CSI.Apps.XXXXXX.Send.Services.ContentStoreClients;
using Hawking.CSI.Apps.XXXXXX.Send.Services.ExternalClients;
using Hawking.CSI.Tracking.Client;
using Hawking.CSI.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hawking.CSI.Apps.XXXXXX.Send.Controllers
{
    [Route("messages")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IContentStoreClient contentStoreClient;
        private readonly IExternalClient externalClient;
        private readonly IMessageEventClient messageEventClient;
        private readonly IConfiguration configuration;
        private readonly ILogger logger;

        public MessagesController(IContentStoreClient contentStoreClient, 
            IExternalClient externalClient, 
            IMessageEventClient messageEventClient, 
            IConfiguration configuration, 
            ILogger<MessagesController> logger)
        {
            this.contentStoreClient = contentStoreClient;
            this.externalClient = externalClient;
            this.messageEventClient = messageEventClient;
            this.configuration = configuration;
            this.logger = logger;
        }

        [HttpPost()]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Post()
        {
            try
            {
                var messageHeaders = Request.GetCustomHeaders();
                await messageEventClient.TrackMessageEventAsync(MessageEventType.Sending, messageHeaders, CancellationToken.None);

                using (var message = await contentStoreClient.ReadContentAsync(messageHeaders["X-Content-Store-Reference"], CancellationToken.None))
                {
                    await externalClient.SendAsync(messageHeaders, message, CancellationToken.None);
                }

                await messageEventClient.TrackMessageEventAsync(MessageEventType.Sent, messageHeaders, CancellationToken.None);
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Message failed");
                return BadRequest(ex.Message);
            }
        }
    }
}
