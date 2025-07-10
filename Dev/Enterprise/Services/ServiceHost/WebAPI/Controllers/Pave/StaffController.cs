using System;
using System.Web.Http;
using Enterprise.BufferManagement.Service;
using Enterprise.BufferManagement.Service.Shared.Staff;

namespace Enterprise.Services.ServiceHost.WebAPI.Controllers.Pave
{
	[GlowTicketAuthentication]
	public class StaffController : BasePaveController
	{
		public IStaffService StaffService { get; }

		public StaffController(IStaffService staffService) => StaffService = staffService;

		public StaffController() : this(new StaffService())
		{ }

		[HttpGet]
		[Route("api/pave/staff/{staffId}/capacity/{componentId}")]
		public IHttpActionResult GetMatchingWorkflows(Guid staffId, Guid componentId)
		{
			return RunWithUserContext(() =>
			{
				var capacityDto = StaffService.GetStaffCapacity(componentId, staffId);

				return Ok(capacityDto);
			});
		}
	}
}
