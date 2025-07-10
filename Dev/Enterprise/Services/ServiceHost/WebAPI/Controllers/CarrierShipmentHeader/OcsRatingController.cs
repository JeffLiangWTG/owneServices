using System.Net;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Description;
using CargoWise.Application;
using CargoWise.Data;
using Enterprise.Rating.Business;

namespace Enterprise.Services.ServiceHost;

[GlowTicketAuthentication]
[RoutePrefix("api/oceancarrier/shipment/rates")]
public class OcsRatingController : ApiController
{
	[HttpPost]
	[Route("calculateRates")]
	[ResponseType(typeof(OcsRatingResponse))]
	public IHttpActionResult CalculateRates([FromBody] CalculateRatesQueryParameters queryParameters)
	{
		if (queryParameters is null)
		{
			return BadRequest(Res.GetString("cbcb68cd-8522-4310-9581-8a86aeb62f4f", "Please provide parameters for the rate calculation."));
		}

		using var disposableDbConnection = Db.DisposableActionForDbConnection();
		using var disposableUserContext = GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext);

		// If service is created before user context is set, then the default currency would be null
		var carrierShipmentRateService = ObjectFactory.Get<ICarrierShipmentRateService>();

		var results = carrierShipmentRateService.GetCarrierShipmentRates(queryParameters);
		if (results.Errors.Length != 0)
		{
			var response = new OcsRatingResponse() { Errors = results.Errors, Log = results.Logs, Rates = results.Rates };

			return Content(HttpStatusCode.BadRequest, response, new JsonMediaTypeFormatter());
		}

		return Json(new OcsRatingResponse() { Rates = results.Rates, Log = results.Logs, Errors = results.Errors });
	}
}
