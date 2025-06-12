using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using OcmPoc.FrontEnd.Core.Dtos;
using OcmPoc.Infrastructure.MessageInterfaces.Queueing;

namespace OcmPoc.FrontEnd.Api.Controllers
{
	[ApiController]
	[Route("api/received-messages")]
    public class ReceivedMessagesController : Controller
    {
	    readonly IQueueReader<QueueItem> queueReader;
		readonly IMapper mapper;

	    public ReceivedMessagesController(IQueueReader<QueueItem> queueReader, IMapper mapper)
	    {
		    this.queueReader = queueReader;
			this.mapper = mapper;
	    }

		[HttpGet]
		[ProducesResponseType(200)]
		public ActionResult<IEnumerable<ReceivedMessageDto>> Get()
		{
			var newMessages = queueReader.GetMessages(10).ToList();

			return Ok(mapper.Map<IEnumerable<ReceivedMessageDto>>(newMessages));
		}

		[HttpPut("{flowId}/ack")]
		[ProducesResponseType(200)]
		public ActionResult<IEnumerable<ReceivedMessageDto>> Ack(int flowId)
		{
			var message = queueReader.GetMessages(10).FirstOrDefault(m => m.MessageFlowId == flowId);

			if (message != null)
			{
				queueReader.Ack(message.Delivery);
			}

			return Ok();
		}
	}
}
