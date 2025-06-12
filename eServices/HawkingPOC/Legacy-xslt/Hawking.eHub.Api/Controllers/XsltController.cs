using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Hawking.eHub.Model.Services;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Hawking.eHub.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class XsltController : ControllerBase
    {
        #region Member Variables

        readonly ILogger logger;
        readonly IMappingConfigService mappingConfigService;

        #endregion

        #region Constructor

        public XsltController(ILogger logger, IMappingConfigService mappingConfigService)
        {
            this.logger = logger.ForContext<XsltController>();
            this.mappingConfigService = mappingConfigService;
        }

        #endregion

        /// <summary>
        /// With the provided <paramref name="senderID"/>, <paramref name="recipientID"/> and the <paramref name="sourceMessageType"/>, find
        /// the transformation set, then perform transformations step by step, produce a final result message and return.
        /// </summary>
        /// <param name="senderID">The sender ID</param>
        /// <param name="recipientID">The recipient ID</param>
        /// <param name="sourceMessageType">The source message type</param>
        /// <returns>
        /// Returns a stream for the transformed result message.
        /// </returns>
        /// <example>http://localhost/ehub/api/xslt/Transform?senderID=sender1&recipientID=recipient1&sourceMessageType=messageType1 + message body strings</example>
        [HttpPost]
        public async Task<Stream> Transform(string senderID, string recipientID, string sourceMessageType)
        {
            mappingConfigService.SelectTransformsByPartiesMessage(senderID, recipientID, sourceMessageType, out var bestMatchingSetID);
            if (!bestMatchingSetID.HasValue)
            {
                throw new Exception($"No transformation set found for the specified sender: {senderID}, recipient: {recipientID}, and sourceMessageType: {sourceMessageType}.");
            }

            var message = string.Empty;
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                message = await reader.ReadToEndAsync();
            }

            // save to temp file
            var tempFile = Path.GetTempFileName();
            System.IO.File.WriteAllText(tempFile, message);

            try
            {
                // open the file stream and return
                var fileStream = new FileStream(tempFile, FileMode.Open, FileAccess.Read);

                return await Task.FromResult<Stream>(fileStream);
            }
            finally
            {
                if (System.IO.File.Exists(tempFile))
                {
                    System.IO.File.Delete(tempFile);
                }
            }
        }
    }
}
