using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Web.Http;
using AuthenticationService.Client.Models;
using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.Rating.Business;
using Enterprise.Rating.Web.Configuration;
using Enterprise.Rating.Web.Model;
using Enterprise.Rating.Web.Model.Validation;
using FluentValidation;
using Swashbuckle.Examples;
using Swashbuckle.Swagger.Annotations;

namespace Enterprise.Rating.Web.Controllers
{
	/// <summary>
	/// RatesAPIController class is the main controller to serve all Rates APIs.
	/// </summary>
	[Authorize]
	[RoutePrefix("api/rating")]
	public class RatesAPIController : ApiController
	{
		/// <summary>
		/// RatesAPIController Constructor
		/// </summary>
		/// <param name="serviceProvider"></param>
		/// <param name="logger"></param>
		public RatesAPIController(ICWServiceProvider serviceProvider, ILoggerExtended logger)
		{
			this.cwServiceProvider = Argument.NotNull(serviceProvider, nameof(serviceProvider));
			this.logger = Argument.NotNull(logger, nameof(logger));
		}

		readonly ICWServiceProvider cwServiceProvider;
		readonly ILoggerExtended logger;

		/// <summary>
		/// The costings endpoint provides the pricing information from buy rates, maintained on CargoWise Costings and any other rate providers such as CargoSphere and Cargoguide integrated with Rates Service.
		/// 
		/// </summary>
		/// <param name="branchCode"></param>
		/// <param name="departmentCode"></param>
		/// <param name="rateQuery">rateQuery is provided in the body.</param>
		/// <returns></returns>
		[HttpPost]
		[Route("costing/{branchCode}/{departmentCode}")]
		[SwaggerResponse(System.Net.HttpStatusCode.OK, Type = typeof(ApiResponse))]
		[SwaggerResponse(System.Net.HttpStatusCode.BadRequest)]
		[SwaggerResponse(System.Net.HttpStatusCode.Unauthorized)]
		[SwaggerResponse(System.Net.HttpStatusCode.InternalServerError)]
		[SwaggerRequestExample(typeof(RateQuery), typeof(RateQueryExample))]
		[SwaggerResponseExample(System.Net.HttpStatusCode.OK, typeof(CostingApiResponseExample))]
		public IHttpActionResult Costing(string branchCode, string departmentCode, [FromBody] RateQuery rateQuery)
		{
			return GetServiceResults(branchCode, departmentCode, SourceEndpoint.Costing, rateQuery, (user, branch, department, query, logger) => cwServiceProvider.GetCosts(user, branch, department, query, logger));
		}

		/// <summary>
		/// The Companytariffs endpoint provides the pricing information from Company Tariffs maintained on CargoWise matching the attributes under the RateQuery object in the request sending to Rates API.
		/// </summary>
		/// <param name="branchCode"></param>
		/// <param name="departmentCode"></param>
		/// <param name="rateQuery"></param>
		/// <returns></returns>
		[HttpPost]
		[Route("companytariffs/{branchCode}/{departmentCode}")]
		[SwaggerResponse(System.Net.HttpStatusCode.OK, Type = typeof(ApiResponse))]
		[SwaggerResponse(System.Net.HttpStatusCode.Unauthorized)]
		[SwaggerResponse(System.Net.HttpStatusCode.InternalServerError)]
		[SwaggerRequestExample(typeof(RateQuery), typeof(RateQueryExample))]
		[SwaggerResponseExample(System.Net.HttpStatusCode.OK, typeof(CompanyTariffsApiResponseExample))]
		public IHttpActionResult CompanyTariffs(string branchCode, string departmentCode, [FromBody] RateQuery rateQuery)
		{
			return GetServiceResults(branchCode, departmentCode, SourceEndpoint.CompanyTariffs, rateQuery, (user, branch, department, query, logger) => cwServiceProvider.GetCompanyTariffs(user, branch, department, query, logger));
		}

		/// <summary>
		/// The intercompanytariffs endpoint provides the pricing information from Intercompany Tariffs maintained on CargoWise matching the attributes under the RateQuery object in the request sending to Rates API.
		/// </summary>
		/// <param name="branchCode"></param>
		/// <param name="departmentCode"></param>
		/// <param name="rateQuery"></param>
		/// <returns></returns>
		[HttpPost]
		[Route("intercompanytariffs/{branchCode}/{departmentCode}")]
		[SwaggerResponse(System.Net.HttpStatusCode.OK, Type = typeof(ApiResponse))]
		[SwaggerResponse(System.Net.HttpStatusCode.Unauthorized)]
		[SwaggerResponse(System.Net.HttpStatusCode.InternalServerError)]
		[SwaggerRequestExample(typeof(RateQuery), typeof(RateQueryExample))]
		[SwaggerResponseExample(System.Net.HttpStatusCode.OK, typeof(IntercompanyTariffsApiResponseExample))]
		public IHttpActionResult IntercompanyTariffs(string branchCode, string departmentCode, [FromBody] RateQuery rateQuery)
		{
			return GetServiceResults(branchCode, departmentCode, SourceEndpoint.IntercompanyTariffs, rateQuery, (user, branch, department, query, logger) => cwServiceProvider.GetIntercompanyTariffs(user, branch, department, query, logger));
		}

		/// <summary>
		/// The jobcharges endpoint provides the calculated job charges from buy rates, maintained on CargoWise and any other rate providers such as CargoSphere and Cargoguide integrated with Rates Service, and sell rates matching the attributes and measurements under the RateQuery object in the request sending to Rates API. The calculation logic and behaviour align with the current Autorating engine on CargoWise.
		/// </summary>
		/// <param name="branchCode"></param>
		/// <param name="departmentCode"></param>
		/// <param name="rateQuery"></param>
		/// <returns></returns>
		[HttpPost]
		[Route("jobcharges/{branchCode}/{departmentCode}")]
		[SwaggerResponse(System.Net.HttpStatusCode.OK, Type = typeof(ApiResponse))]
		[SwaggerResponse(System.Net.HttpStatusCode.Unauthorized)]
		[SwaggerResponse(System.Net.HttpStatusCode.InternalServerError)]
		[SwaggerRequestExample(typeof(RateQuery), typeof(RateQueryExample))]
		[SwaggerResponseExample(System.Net.HttpStatusCode.OK, typeof(JobChargesApiResponseExample))]
		public IHttpActionResult JobCharges(string branchCode, string departmentCode, [FromBody] RateQuery rateQuery)
		{
			return GetServiceResults(branchCode, departmentCode, SourceEndpoint.JobCharges, rateQuery, (user, branch, department, query, logger) => cwServiceProvider.GetJobCharges(user, branch, department, query, logger));
		}

		/// <summary>
		/// The client rates endpoint provides the pricing information from Client Rates maintained on CargoWise matching the attributes under the RateQuery object in the request sending to Rates APIs.
		/// </summary>
		/// <param name="branchCode"></param>
		/// <param name="departmentCode"></param>
		/// <param name="rateQuery"></param>
		/// <returns></returns>
		[HttpPost]
		[Route("clientrates/{branchCode}/{departmentCode}")]
		[SwaggerResponse(System.Net.HttpStatusCode.OK, Type = typeof(ApiResponse))]
		[SwaggerResponse(System.Net.HttpStatusCode.Unauthorized)]
		[SwaggerResponse(System.Net.HttpStatusCode.InternalServerError)]
		[SwaggerRequestExample(typeof(RateQuery), typeof(RateQueryExample))]
		[SwaggerResponseExample(System.Net.HttpStatusCode.OK, typeof(ClientRatesApiResponseExample))]
		public IHttpActionResult ClientRates(string branchCode, string departmentCode, [FromBody] RateQuery rateQuery)
		{
			return GetServiceResults(branchCode, departmentCode, SourceEndpoint.ClientRates, rateQuery, (user, branch, department, query, logger) => cwServiceProvider.GetClientRates(user, branch, department, query, logger));
		}

		IHttpActionResult GetServiceResults(string branchCode, string departmentCode, SourceEndpoint sourceEndpoint, RateQuery query, Func<string, string, string, RateQuery, ILogger, IReadOnlyCollection<Rate>> requestedService)
		{
			var stopwatch = Stopwatch.StartNew();
			var identity = (ClaimsIdentity)User.Identity;
			var userName = identity.Claims.Where(c => c.Type == WTGClaimTypes.UserCode).Select(c => c.Value).Single();

			if (query == null)
			{
				cwServiceProvider.ReportUsage(userName, branchCode, departmentCode, sourceEndpoint, HttpStatusCode.BadRequest, Array.Empty<Rate>(), stopwatch.Elapsed);
				return BadRequest(Res.GetString("2ACD96C3-0490-4375-85A3-75FC5AA8330C", "The request is invalid. Please provide {0}.", nameof(RateQuery)));
			}

			try
			{
				var response = new ApiResponse();
				response.Rates = requestedService(userName, branchCode, departmentCode, query, logger).ToArray();
				response.PushLogs(sourceEndpoint, logger);

				cwServiceProvider.ReportUsage(userName, branchCode, departmentCode, sourceEndpoint, HttpStatusCode.OK, response.Rates, stopwatch.Elapsed);
				return Ok(response);
			}
			catch (ValidationException ex)
			{
				ex.AddErrorsToModelState(ModelState);
				cwServiceProvider.ReportUsage(userName, branchCode, departmentCode, sourceEndpoint, HttpStatusCode.BadRequest, Array.Empty<Rate>(), stopwatch.Elapsed);
				return BadRequest(ModelState);
			}
		}
	}
}
