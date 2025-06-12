using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Hawking.CSI.Apps.XXXXXX.Models;
using Hawking.CSI.Apps.XXXXXX.Outbound.Services;
using Hawking.CSI.Apps.XXXXXX.Outbound.Services.ContentStoreClients;
using Hawking.CSI.Tracking.Client;
using Hawking.CSI.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hawking.CSI.Apps.XXXXXX.Outbound.Controllers
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

                MessageX1 msgX1;
                var x1Serializer = new XmlSerializer(typeof(MessageX1));
                using (var body 
                    = await contentStoreClient.ReadContentAsync(messageHeaders["X-Content-Store-Reference"], 
                        CancellationToken.None))
                    msgX1 = (MessageX1)x1Serializer.Deserialize(body);

                var mapper = new StringBuilder();
                foreach(var item in msgX1.Message)
                    mapper.Append(item);
                
                var msgX2 = new MessageX2
                {
                    TrackingId = msgX1.TrackingId,
                    CreatedUtc = msgX1.CreatedUtc,
                    Message = mapper.ToString()
                };

                string contentStoreReference;
                long length;
                using (var result = new MemoryStream())
                using (var xw = XmlWriter.Create(result, new XmlWriterSettings { Indent = false, OmitXmlDeclaration = true }))
                {
                    var x2Serializer = new XmlSerializer(typeof(MessageX2));
                    var ns = new XmlSerializerNamespaces();
                    ns.Add("", "");
                    x2Serializer.Serialize(xw, msgX2, ns);
                    length = result.Length;
                    result.Position = 0;
                    contentStoreReference = await contentStoreClient.WriteContentAsync(messageHeaders, result, CancellationToken.None);
                }

                messageHeaders["X-Content-Store-Reference"] = contentStoreReference;
                messageHeaders["X-Size"] = length.ToString();
                messageHeaders["X-Forward"] = "true";
                messageHeaders["X-Destination-Address"] = new ApplicationAddress { ApplicationName = "XXXXXX-Send" }.ToString();
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
