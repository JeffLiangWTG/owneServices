using System.Web.Http;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Services.ServiceHost
{
	[Authorize]
	[DDDBasicAuthentication]
	[RoutePrefix("api/forwarding")]
	public class DeliveryDueDateController : ApiController
	{
		[Route("querydeliveryduedate")]
		[HttpPost]
		public IHttpActionResult QueryAdjustedDueDate([FromBody]DeliveryDueDateQuery query)
		{
			if (query == null)
			{
				return BadRequest(Res.GetString("79F98301-4745-40B0-B645-952419A63454", "Delivery Due Date Query should be provided."));
			}

			var validator = new DeliveryDueDateQueryValidator();
			var validationResult = validator.Validate(query);

			if (validationResult.IsValid)
			{
				var result =
					new DeliveryDueDateCalculatorManager()
					.Calculate(
						new CargoWise.EntityFramework.BusinessObjectFactory(),
						new ZDateTime(query.PickupDate),
						query.ServiceLevel,
						query.HBLDlvMode,
						query.PickupOrg,
						query.PickupAddr,
						query.PickupCFSOrg,
						query.PickupCFSAddr,
						query.DeliveryCFSOrg,
						query.DeliveryCFSAddr,
						query.DeliveryOrg,
						query.DeliveryAddr,
						query.TransportMode,
						query.DeliveryType);

				var response = new DeliveryDueDateAPIResponse()
				{
					DeliveryDueDate = result.ToISO8601String(),
					Messages = System.Array.Empty<string>()
				};

				return Ok(response);
			}
			else
			{
				validationResult.AddErrorsToModelState(ModelState);
				return BadRequest(ModelState);
			}
		}
	}
}
