using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hawking.CSI.ContentStore.Services;
using Microsoft.AspNetCore.Mvc;

namespace Hawking.CSI.ContentStore.Controllers
{
    [Route("v1/contents")]
    [ApiController]
    public class ContentsController : ControllerBase
    {
        private readonly IObjectStoreClient objectStoreClient;

        public ContentsController(IObjectStoreClient objectStoreClient)
        {
            this.objectStoreClient = objectStoreClient;
        }

        [HttpPost]
        [RequestSizeLimit(104_857_600)]
        [ProducesResponseType(201)]
        public async Task<IActionResult> Post()
        {
            string name = Request.Headers["X-TrackingId"].ToString() ?? DateTime.UtcNow.ToString("O");
            var refId = await objectStoreClient.WriteObjectAsync(name, Request.Body, CancellationToken.None);
            Response.Headers["X-Content-Store-Reference"] = refId;
            return Created(refId, null);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Get(string id)
        {
            var content = await objectStoreClient.ReadObjectAsync(id, CancellationToken.None);
            if (content == null)
                return NotFound();

            return new FileStreamResult(content, "application/octet-stream");
        }
    }
}
