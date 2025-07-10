#if NETFRAMEWORK
using System.Web;
using System.Web.Http;
using IActionResult = System.Web.Http.IHttpActionResult;
using RouteAttribute = System.Web.Http.RouteAttribute;
#else
using System;
#endif
using System.Collections;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Integration;
using Microsoft.AspNetCore.Mvc;

namespace Enterprise.Services.ServiceHost.NetCore
{
	[GlowTicketAuthentication]
	public class CustomsStatusController : ControllerBase
	{
		[Route("api/Customs/CustomsStatusDescription/{reasonCode}/{countryCode}")]
		[HttpGet]
		public IActionResult GetCustomsStatusReasonDescription([FromQuery] string reasonCode, [FromQuery] string countryCode)
		{
			if (isValidRequestParameters(reasonCode, countryCode))
			{
				try
				{
					using (Db.DisposableActionForDbConnection())
					using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
					{
						var customsStatusReasonHelpers = ObjectFactory.Get<Hashtable>("CountrySpecificCustomsStatusReasonHelpers");
						if (customsStatusReasonHelpers.Contains(countryCode))
						{
							var handle = (ObjectHandle)customsStatusReasonHelpers[countryCode];
							var customsStatusReasonHelper = (ICustomsStatusReasonHelper)handle?.GetObject();
							var description = customsStatusReasonHelper.GetCustomsStatusReasonDescription(reasonCode);

							if (!description.IsEmpty)
							{
								return Ok(description);
							}
							else
							{
								return BadRequest(ResString.GetMultilingualString("cef6dcdb-2559-4545-86f2-9326bc182b66", "Did not find the reason description for code '{0}' from country/region '{1}'.", reasonCode, countryCode));
							}
						}
						else
						{
							return BadRequest(ResString.GetMultilingualString("7403e5c9-7684-4727-8ca6-853d15c5549f", "Could not find customs status reason code mapping for the country/region '{0}'.", countryCode));
						}
					}
				}
#if NETFRAMEWORK
				catch (HttpException ex)
				{
					return InternalServerError(ex);
				}
#else
				catch (Exception ex)
				{
					return StatusCode(500, ex.Message);
				}
#endif
			}

			return BadRequest(ResString.GetMultilingualString("b1aa8f85-e0e7-4bb9-93d2-7db384a5da7c", "Incorrect reason code ({0}) or country/region code ({1}) length - reason code should be 3 characters long and country/region code should be 2 characters long.", reasonCode, countryCode));
		}

		bool isValidRequestParameters(ZString reasonCode, ZString countryCode)
		{
			return (reasonCode.Length == 3 && countryCode.Length == 2);
		}
	}
}
