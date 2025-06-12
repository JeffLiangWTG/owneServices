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
using Hawking.CSI.Apps.YYYYYY.Models;
using Hawking.CSI.Apps.YYYYYY.Outbound.Services;
using Hawking.CSI.Apps.YYYYYY.Outbound.Services.ContentStoreClients;
using Hawking.CSI.Tracking.Client;
using Hawking.CSI.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hawking.CSI.Apps.YYYYYY.Outbound.Controllers
{
    [Route("messages")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IContentStoreClient contentStoreClient;
        private readonly IMessageEventClient messageEventClient;
        private readonly IConfiguration configuration;
        private readonly ILogger logger;

        public MessagesController(IContentStoreClient contentStoreClient, 
            IMessageEventClient messageEventClient, 
            IConfiguration configuration, 
            ILogger<MessagesController> logger)
        {
            this.contentStoreClient = contentStoreClient;
            this.messageEventClient = messageEventClient;
            this.configuration = configuration;
            this.logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(201)]
        public async Task<IActionResult> Post()
        {
            try
            {
                var messageHeaders = Request.GetCustomHeaders();
                await messageEventClient.TrackMessageEventAsync(MessageEventType.Processing, messageHeaders, CancellationToken.None);

                MessageY1 msgY1;
                using (var body 
                    = await contentStoreClient.ReadContentAsync(messageHeaders["X-Content-Store-Reference"], 
                        CancellationToken.None))
                using (var rdr = new StreamReader(body))
                {
                    msgY1 = JsonConvert.DeserializeObject<MessageY1>(rdr.ReadToEnd());
                }

                var mapper = new StringBuilder();
                foreach(var item in msgY1.Message)
                    mapper.Append(item);
                
                var msgY2 = new MessageY2
                {
                    TrackingId = msgY1.TrackingId,
                    CreatedUtc = msgY1.CreatedUtc,
                    Message = mapper.ToString()
                };

                var result = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msgY2));

                string contentStoreReference;
                using (var message = new MemoryStream(result))
                {
                    contentStoreReference = await contentStoreClient.WriteContentAsync(messageHeaders, message, CancellationToken.None);
                }

                messageHeaders["X-Content-Store-Reference"] = contentStoreReference;
                messageHeaders["X-Size"] = result.Length.ToString();
                messageHeaders["X-Forward"] = "true";
                messageHeaders["X-Destination-Address"] = new ApplicationAddress { ApplicationName = "YYYYYY-Send" }.ToString();
                await messageEventClient.TrackMessageEventAsync(MessageEventType.Processed, messageHeaders, CancellationToken.None);

                messageHeaders.CopyTo(Response.Headers);
                return Created(contentStoreReference, null);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Message failed");
                return BadRequest(ex.Message);
            }
        }
    }
}
