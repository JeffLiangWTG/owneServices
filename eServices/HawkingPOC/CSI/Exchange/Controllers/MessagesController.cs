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
using Hawking.CSI.Tracking.Client;
using Hawking.CSI.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hawking.CSI.Exchange
{
    [Route("v1/messages")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IContentStoreClient contentStoreClient;
        private readonly IForwardClient forwardClient;
        private readonly IMessageEventClient messageEventClient;
        private readonly IConfiguration configuration;
        private readonly ILogger logger;

        public MessagesController(IContentStoreClient contentStoreClient, 
            IForwardClient forwardClient, 
            IMessageEventClient messageEventClient, 
            IConfiguration configuration, 
            ILogger<MessagesController> logger)
        {
            this.contentStoreClient = contentStoreClient;
            this.forwardClient = forwardClient;
            this.messageEventClient = messageEventClient;
            this.configuration = configuration;
            this.logger = logger;
        }

        [HttpPost()]
        [RequestSizeLimit(104_857_600)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Post()
        {
            try
            {
                var messageHeaders = Request.GetCustomHeaders();
                messageHeaders["X-Size"] = Request.ContentLength.ToString();
                await messageEventClient.TrackMessageEventAsync(MessageEventType.Received, messageHeaders, CancellationToken.None);
                messageHeaders["X-Content-Store-Reference"] = await contentStoreClient.WriteContentAsync(messageHeaders, Request.Body, CancellationToken.None);
                await forwardClient.SendAsync(messageHeaders, CancellationToken.None);
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
