using System;
using System.Web.Http;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Services.ServiceHost
{
	[GlowTicketAuthentication]
	public class UniversalPublisherController : ApiController
	{
		public UniversalPublisherController()
		{
		}

		[Route("api/Universal/PublishUniversal/{jobType}")]
		[HttpPost]
		public IHttpActionResult PublishUniversal([FromBody] Guid[] jobPKs, [FromUri] string jobType)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var response = GetPublishUniversalJSONResponse(jobPKs, jobType);
				return Json(response);
			}
		}

		UniversalJsonResponse GetPublishUniversalJSONResponse(Guid[] jobPKs, string jobType)
		{
			var service = ObjectFactory.Get<ITransitUniversalService>();
			var result = service.PublishUniversal(Array.ConvertAll(jobPKs, item => (ZGuid)item), jobType);

			var response = new UniversalJsonResponse(result.ErrorType, result.Message);
			return response;
		}

		public class UniversalJsonResponse
		{
			public UniversalJsonResponse(string messageType, string message)
			{
				this.messageType = messageType;
				this.message = message;
			}
			public string messageType { get; set; }
			public string message { get; set; }
		}
	}
}
