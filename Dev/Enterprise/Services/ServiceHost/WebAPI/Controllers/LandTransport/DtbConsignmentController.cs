using System;
using System.Web.Http;
using CargoWise.Application;
using CargoWise.Data;
using Enterprise.TransportConsignment.Integration;

namespace Enterprise.Services.ServiceHost
{
	[GlowTicketAuthentication]
	public class DtbConsignmentController : ApiController
	{
		[Route("api/LandTransport/GetDefaultValuesForConsignment/{pickupAddressPK}/{deliveryAddressPK}/{jobType}")]
		[HttpGet]
		public IHttpActionResult GetDefaultValuesForConsignment([FromUri] Guid pickupAddressPK, [FromUri] Guid deliveryAddressPK, [FromUri] string jobType)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var service = ObjectFactory.Get<IDtbConsignmentService>();
				var result = service.GetDefaultValuesForConsignment(pickupAddressPK, deliveryAddressPK, jobType);
				var incoTerm = result[service.IncoTermKey];
				var serviceLevel = result[service.ServiceLevelKey];

				return Json(new GetDefaultValuesForConsignmentJsonResponse(incoTerm, serviceLevel));
			}
		}

		class GetDefaultValuesForConsignmentJsonResponse(string incoTerm, string serviceLevel)
		{
			public string incoTerm { get; set; } = incoTerm;
			public string serviceLevel { get; set; } = serviceLevel;
		}
	}
}
