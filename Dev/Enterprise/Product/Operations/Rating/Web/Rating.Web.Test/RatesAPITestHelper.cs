using System;
using Enterprise.Rating.Web.Model;

namespace Enterprise.Rating.Web.Test
{
	public static class RatesAPITestHelper
	{
		public static RateQuery GetValidRateQuery(SourceEndpoint source = SourceEndpoint.Costing)
		{
			var result = new RateQuery()
			{
				Origin = new Location()
				{
					Type = Location.Types.UNLOCO,
					Value = "AUSYD"
				},
				Destination = new Location()
				{
					Type = Location.Types.UNLOCO,
					Value = "USLAX"
				},
				TransportMode = "AIR",
				ContainerMode = "LSE",
				EffectiveDate = DateTimeOffset.UtcNow
			};

			if (source == SourceEndpoint.Costing)
			{
				result.RateProviders = new[]
				{
					RatesAPIsConstants.RateProviders.CargoWise,
					RatesAPIsConstants.RateProviders.CGCS,
				};
			}

			if (source == SourceEndpoint.JobCharges)
			{
				result.CalculationScope = RateQuery.CalculationScopes.BuyAndSellRates;
				result.CarrierPayTerm = Core.Constants.PaymentType.Prepaid;
				result.Incoterm = Core.Constants.IncoTerms.ExWorks;
				result.JobInfo = new JobInfo()
				{
					Containers = new[]
					{
						new JobContainer() { }
					}
				};

				result.RateParties = new[]
				{
					new OrganisationRole() { Role = OrganisationRole.Roles.CNE, Code = "ZZZZ" }
				};
			}

			if (source == SourceEndpoint.CompanyTariffs)
			{
				result.CTLevel = 1;
			}

			if (source == SourceEndpoint.ClientRates)
			{
				result.Client = "ZZZZZ";
			}

			return result;
		}
	}
}
