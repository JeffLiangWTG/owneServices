using System;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using CargoWise.RefDbRepo.Common.Utils;

namespace CargoWise.RefDbRepo.Staging.Rule
{
	static class Program
	{
		static void Main(string[] args)
		{
			Environment.SetEnvironmentVariable("BASEDIR", AppDomain.CurrentDomain.BaseDirectory);
			var safeUpdateServiceUri = ApplicationConfig.SafeUpdateServiceUri;
			RunAsync(new Uri(safeUpdateServiceUri))?.Wait();
		}

		static async Task<int> RunAsync(Uri safeUpdateServiceUri)
		{
			Argument.NotNull(safeUpdateServiceUri, nameof(safeUpdateServiceUri));
			var result = 0;
			using (var accessTokenProvider = new AccessTokenProvider(ApplicationConfig.TenantId, ApplicationConfig.ClientId, ApplicationConfig.ServiceId, ApplicationConfig.PrivateKeyFileName, ApplicationConfig.CertificateFileName, ApplicationConfig.RefreshAdvanceInMinutes))
			{
				var safeRepo = new SafeRepository(safeUpdateServiceUri, accessTokenProvider: accessTokenProvider);
				var ruleRepo = new TariffRuleRepository(safeRepo);
				var applier = new RuleApplier(safeRepo);
				foreach (var rule in await ruleRepo.GetUnAppliedRuleHeaderWrappersAsync())
				{
					result += await RunIt(safeRepo, rule, applier);
				}
				return result;
			}
		}

		static async Task<int> RunIt(SafeRepository safeRepo, ITariffRuleHeaderWrapper rule, RuleApplier applier)
		{
			Argument.NotNull(safeRepo, nameof(safeRepo));
			Argument.NotNull(rule, nameof(rule));
			Argument.NotNull(applier, nameof(applier));
			var result = 0;
			var tariffs = await rule.GetMatchingTariffsWithRelatedObjects();
			foreach (var tariff in tariffs)
			{
				await applier.ApplyAsync(rule, tariff, tariff.RefCusTariffUOMs.ToArray(),
					tariff.RefCusTariffAttributes.ToArray(),
					tariff.RefCusRates.ToArray());
			}
			rule.MarkAsApplied();
			result += tariffs.Length;
			await safeRepo.SaveChangesAysnc();
			return result;
		}
	}
}
