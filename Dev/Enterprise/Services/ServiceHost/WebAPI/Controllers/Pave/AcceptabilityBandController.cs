using System;
using System.Net;
using System.Web.Http;
using Enterprise.BufferManagement.Service;
using Enterprise.BufferManagement.Service.Shared;

namespace Enterprise.Services.ServiceHost
{
	[GlowTicketAuthentication]
	public class AcceptabilityBandController : BasePaveController
	{
		public AcceptabilityBandController(IAcceptabilityBandService abService) => AcceptabilityBandService = abService;

		public AcceptabilityBandController() : this(new AcceptabilityBandsService())
		{ }

		protected readonly IAcceptabilityBandService AcceptabilityBandService;

		[HttpGet]
		[Route("api/pave/boards/{boardId}/acceptability-bands/{acceptabilityBandId}/matching-workflows")]
		public IHttpActionResult GetMatchingWorkflows(Guid boardId, Guid acceptabilityBandId)
		{
			return RunWithUserContext(() =>
			{
				if (!AcceptabilityBandService.TryGetWorkflowsMatchingAcceptabilityBand(boardId, acceptabilityBandId, out var acceptabilityBandWorkflowResultDto, out var businessResponse))
				{
					return UnprocessableEntity(businessResponse);
				}

				return Ok(acceptabilityBandWorkflowResultDto);
			});
		}

		[HttpGet]
		[Route("api/pave/boards/{boardId}/acceptability-bands/{acceptabilityBandId}")]
		public IHttpActionResult GetAcceptabilityBands(Guid boardId, Guid acceptabilityBandId)
		{
			return RunWithUserContext(() =>
			{
				if (!AcceptabilityBandService.TryGetAcceptabilityBand(boardId, acceptabilityBandId,out var acceptabilityBandResultDto, out var businessResponse))
				{
					//TODO: EZS, we need to create the BusinessMessage for timeout with the correct canonical :)!
					if (businessResponse.Message.LocalizationKey == AcceptabilityBandsService.BusinessMessages.AcceptabilityBandTimeout.LocalizationKey)
					{
						//And maybe create a TimeoutEntity() ??
						return Content((HttpStatusCode)408, businessResponse);
					}

					return UnprocessableEntity(businessResponse);
				}

				return Ok(acceptabilityBandResultDto);
			});
		}
	}
}
