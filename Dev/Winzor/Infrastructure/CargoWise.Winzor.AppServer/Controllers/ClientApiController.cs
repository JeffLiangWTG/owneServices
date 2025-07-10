using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Winzor.AppServer.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WinzorFramework;

namespace CargoWise.Winzor.AppServer.Controllers
{
	[Route("_clientapi")]
	public class ClientApiController : Controller
	{
		readonly ILogger<ClientApiController> logger;
		readonly IEnumerable<ITrustedDomainGenerator> trustedDomainGenerators;

		public ClientApiController(
			ILogger<ClientApiController> logger,
			IEnumerable<ITrustedDomainGenerator> trustedDomainGenerators)
		{
			this.logger = logger;
			if (trustedDomainGenerators.IsNullOrEmpty())
			{
				this.trustedDomainGenerators = new[] { new GlowTrustedDomainGenerator() };
			}
			else
			{
				this.trustedDomainGenerators = trustedDomainGenerators;
			}
		}

		//Return the list of trusted Domains for RestrictedForm
		[HttpGet("trusted-domains")]
		public ActionResult GetTrustedAuxiliaryDomainsForUri()
		{
			logger.LogInformation("GetTrustedAuxiliaryDomainsForUriAsync");
			try
			{
				var domains = GetDefaultUriIncludeList(trustedDomainGenerators);
				return Ok(new { TrustedDomains = domains });
			}
			catch (Exception)
			{
				return new StatusCodeResult(StatusCodes.Status500InternalServerError);
			}
		}

		List<string> GetDefaultUriIncludeList(IEnumerable<ITrustedDomainGenerator> trustedDomainGenerators)
		{
			var trustedDomains = new List<string>();
			foreach (var generator in trustedDomainGenerators)
			{
				try
				{
					var domains = generator.GetTrustedDomains();
					trustedDomains.AddRange(domains);
				}
				catch (Exception ex)
				{
					logger.LogWarning($"Exception thrown when trying to get {generator.GetType()} domains\n{ex}");
					throw;
				}
			}

			return trustedDomains;
		}
	}
}
