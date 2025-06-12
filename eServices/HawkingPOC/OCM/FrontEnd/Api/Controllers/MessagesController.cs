using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using OcmPoc.FrontEnd.Core.Dtos;
using OcmPoc.Infrastructure.MessageInterfaces.Repositories;

namespace OcmPoc.FrontEnd.Api.Controllers
{
	[ApiController]
	[Route("api/messages")]
    public class MessagesController : Controller
    {
	    readonly IMessageRepository repository;
		readonly IMapper mapper;

	    public MessagesController(IMessageRepository repository, IMapper mapper)
	    {
		    this.repository = repository;
			this.mapper = mapper;
	    }

        [HttpGet("{id}")]
        public async Task<ActionResult<MessageFlowDto>> Get(Guid id)
        {
			var message = await repository.RetrieveAsync(id);

			if (message == null) { return NotFound(); }

			return new FileContentResult(message.Body, new MediaTypeHeaderValue("application/octet-stream"))
			{
				FileDownloadName = $"Message_{message.Id}"
			};
		}
    }               
}
