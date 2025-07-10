using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Web.Http;
using System.Web.Http.Description;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.CarrierConnect;
using Enterprise.Rating.CarrierConnect.RateSelection.Models;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Services.ServiceHost
{
	[GlowTicketAuthentication]
	[RoutePrefix("api/rating/rate-search")]
	public class RateSearchController : ApiController
	{
		[HttpPost]
		[Route("search")]
		[ResponseType(typeof(RateSearchResponseDto))]
		public IHttpActionResult SearchRates([FromBody] RateQueryDto rateSearchQuery)
		{
			if (rateSearchQuery is null)
			{
				return BadRequest();
			}

			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var errors = ValidateRateQuery(rateSearchQuery).ToList();
				if (errors.Count > 0)
				{
					var response = new RateSearchResponseDto
					{
						Errors = errors.ToArray()
					};
					return Content(HttpStatusCode.BadRequest, response, new JsonMediaTypeFormatter());
				}

				// This is a workaround to clear the UberFactory cache for the exchange rate table. Currently UberFactory cannot be updated without running Factory.Save.
				// It is a temporary solution until UberFactory can be invalidated every 10 minutes on Enterprise Service.
				RowFactory.ClearSpecificTableFromUberFactory(RefExchangeRateSchema.Constants.TableName);
				var logger = new ElementaryLogger();
				var rateSelectorService = new RateSelectorService(logger);
				var rateSearchResponse = rateSelectorService.SearchAndCalculateRates(rateSearchQuery);
				rateSearchResponse.Log = string.Join(System.Environment.NewLine, logger.GetAllLogs());
				rateSearchResponse.Errors = [.. logger.Errors];
				rateSearchResponse.Warnings = [.. logger.Warnings];

				return Json(rateSearchResponse);
			}
		}

		IEnumerable<string> ValidateRateQuery(RateQueryDto rateQueryDto)
		{
			var validTransportModes = new[] { "AIR", "SEA" };
			if (!validTransportModes.Contains(rateQueryDto.TransportMode))
			{
				yield return (NoResString)"Transport mode is not valid.";
			}

			var validContainerModes = new[] { "FCL", "LCL", "ULD", "LSE" };
			if (!validContainerModes.Contains(rateQueryDto.ContainerMode))
			{
				yield return (NoResString)"Container mode is not valid.";
			}

			var factory = new ReadOnlyBusinessObjectFactory();
			var unlocoHelper = new UnlocoHelper(factory);
			if (!unlocoHelper.IsUnloco(rateQueryDto.Origin ?? string.Empty))
			{
				yield return (NoResString)"Origin is not a valid UNLOCO.";
			}

			if (!unlocoHelper.IsUnloco(rateQueryDto.Destination ?? string.Empty))
			{
				yield return (NoResString)"Destination is not a valid UNLOCO.";
			}

			var validRateTypes = RateSelectorService.SupportedRateTypes.Select(kv => kv.Key);
			if (rateQueryDto.RateTypes == null || rateQueryDto.RateTypes.Except(validRateTypes).Any())
			{
				yield return (NoResString)"Rate type is not valid.";
			}

			var freightMode = FreightRatingHelper.CalculateFreightMode(rateQueryDto.TransportMode, rateQueryDto.ContainerMode);
			if ((freightMode & FreightMode.Containerised) == FreightMode.Containerised)
			{
				var hasContainers = rateQueryDto.JobInfo?.Containers?.Any() ?? false;
				if (!hasContainers)
				{
					yield return (NoResString)"Containerized rate search must have at least one container specified.";
				}
			}
		}

		[HttpGet]
		[Route("fetch/{requestId}")]
		[ResponseType(typeof(RateSearchResponseDto))]
		public IHttpActionResult FetchedStoredResult(Guid requestId)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var bizoFactory = new BusinessObjectFactory();
				var result = bizoFactory.Load<RateSearchResult>(requestId);

				if (result == null)
				{
					return StatusCode(HttpStatusCode.NoContent);
				}

				var resultJson = result.RR_JsonContent.ToUTF8();
				var response = Request.CreateResponse(HttpStatusCode.OK);
				response.Content = new StringContent(resultJson, Encoding.UTF8, "application/json");

				result.Delete();
				bizoFactory.Save();

				return ResponseMessage(response);
			}
		}
	}
}
