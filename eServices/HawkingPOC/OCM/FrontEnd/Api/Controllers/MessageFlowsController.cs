using System;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using OcmPoc.Core.Services;
using OcmPoc.FrontEnd.Core.Dtos;
using OcmPoc.Infrastructure.MessageInterfaces.Documents;

namespace OcmPoc.FrontEnd.Api.Controllers
{
	[ApiController]
	[Route("api/message-flows")]
	public class MessageFlowsController : Controller
	{
		readonly IMessageFlowService messageFlowService;
		readonly IMapper mapper;

		public MessageFlowsController(IMessageFlowService messageFlowService, IMapper mapper)
		{
			this.messageFlowService = messageFlowService;
			this.mapper = mapper;
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<MessageFlowDto>> Get(int id)
		{
			var flow = await messageFlowService.GetFlowAsync(id);

			if (flow == null) { return NotFound(); }

			return mapper.Map<MessageFlowDto>(flow);
		}

		[HttpPost]
		[ProducesResponseType(201)]
		[ProducesResponseType(400)]
		public async Task<ActionResult<MessageFlowDto>> Post()
		{
			var contentType = Request.ContentType;

			if (contentType != "application/xml") { ModelState.AddModelError("ContentType", "Must be application/xml"); }
			XDocument content = null;
			
			try
			{
				content = await XDocument.LoadAsync(Request.Body, LoadOptions.None, CancellationToken.None);
			}
			catch (Exception ex)
			{
				ModelState.AddModelError("Content", ex.ToString());
			}

			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var message = Message.Create(content);
			message.From = "CW1";

			var messageFlow = await messageFlowService.CreateNewFlowAsync(message);

			return CreatedAtAction(nameof(Get), new { id = messageFlow.Id }, mapper.Map<MessageFlowDto>(messageFlow));
		}
	}               
}
