using System;
using System.Web.Http;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.GateManagement.DataTransfer;
using Enterprise.Warehouse.GateManagement.Integration;
using Enterprise.ZArchitecture.Core;
using Newtonsoft.Json;
using GateManagementConstants = Enterprise.Core.Constants.GateManagementConstants;

namespace Enterprise.Services.ServiceHost
{
	[GateManagementOAuth2Authorization]
	[FacilitiesAuthentication]
	[RoutePrefix("facilities/gate")]
	public sealed class GateController : ApiController
	{
		[Route("")]
		[HttpGet]
		public IHttpActionResult Get()
		{
			return Ok((NoResString)"Welcome to the Facilities Gate Web Service");
		}

		#region Validate booking

		[Route("validate-{direction}-cy")]
		[HttpPost]
		public IHttpActionResult ValidateBookingContainerYard([FromBody] GateBookingValidationRequest request, string direction)
		{
			return ValidateBookingCore(request, direction, nameof(RecipientRoleType.CYD));
		}

		[Route("validate-{direction}-{facilityTypeCode}", Order = 1)]
		[HttpPost]
		public IHttpActionResult ValidateFacilityBooking([FromBody] GateBookingValidationRequest request, string direction, string facilityTypeCode)
		{
			return ValidateBookingCore(request, direction, facilityTypeCode);
		}

		IHttpActionResult ValidateBookingCore(GateBookingValidationRequest request, string direction, string facilityTypeCode)
		{
			if (direction == (NoResString)"dropoff" || direction == (NoResString)"delivery" || direction == (NoResString)"pickup")
			{
				request.Direction = direction == (NoResString)"dropoff" || direction == (NoResString)"delivery" ? GateManagementConstants.TransportBookingDirections.Codes.Delivery : GateManagementConstants.TransportBookingDirections.Codes.Pickup;
				return InvokeFacilityEndpointCore(request, facilityTypeCode, (GateBookingValidationRequest req) => GateFacilityEndpointRouter.ValidateFacilityBooking(req));
			}
			else
			{
				return NotFound();
			}
		}

		#endregion

		IHttpActionResult InvokeFacilityEndpointCore<T, R>(T request, string facilityTypeCode, Func<T, R> endpointFunction)
			where T : IGateBookingValidationRequest
		{
			request.FacilityTypeCode = facilityTypeCode.ToUpperInvariant();

			try
			{
				var data = endpointFunction.Invoke(request);
				return Json(data, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
			}
			catch (HttpResponseException e)
			{
				if (e.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
				{
					return NotFound();
				}
				else
				{
					return (IHttpActionResult)e;
				}
			}
			catch (ArgumentException e)
			{
				return BadRequest(e.Message);
			}
			catch (NotImplementedException)
			{
				return NotFound();
			}
			catch (Exception e)
			{
				return InternalServerError(e);
			}
		}
	}
}
