using System;
using Swashbuckle.Examples;

namespace Enterprise.Rating.Web.Model
{
	class RateQueryExample : IExamplesProvider
	{
		public object GetExamples()
		{
			var query = new RateQuery()
			{
				Origin = new Location() { Type = Location.Types.UNLOCO, Value = "AUSYD" },
				Destination = new Location() { Type = Location.Types.UNLOCO, Value = "USLAX" },
				TransportMode = "AIR",
				ContainerMode = "LSE",
				EffectiveDate = DateTimeOffset.UtcNow.Date,
				ServiceLevels = new string[] { "STD" },
				GatewayServiceLevels = new string[] { "", "STD" },
				ShipmentGatewayServiceLevels = new string[] { "" },
				RateProviders = new[]
				{
					RatesAPIsConstants.RateProviders.CargoWise,
					RatesAPIsConstants.RateProviders.CGCS,
				},
				Client = "CWCodeC0",
				CalculationScope = RateQuery.CalculationScopes.BuyAndSellRates,
				CarrierPayTerm = Core.Constants.PaymentType.Prepaid,
				Incoterm = Core.Constants.IncoTerms.ExWorks,
				JobInfo = new JobInfo()
				{
					GoodsValue = 2000M,
					GoodsValueCurrency = "AUD",
					InsuranceValue = 110M,
					InsuranceValueCurrency = "AUD",
					CustomsValue = 560,
					PickupDropMode = "ANY",
					DeliveryDropMode = "ANY",
					JobServices = new[]
					{
						new JobService()
						{
							Type = "FUM",
							Contractor = new Organisation() { IATACode = "QA" },
							Location = new Location() { Type = Location.Types.UNLOCO, Value = "AUSYD" },
							Booked = null,
							Completed = DateTimeOffset.UtcNow.Date,
							Count = 2,
							Duration = 300,
							Rate = 1.1M,
							RateCurrency = "AUD"
						}
					},
					Containers = new[]
					{
						new JobContainer()
						{
							Commodity = "GEN",
							Ownership = "CAR",
							PackLines = new []
							{
								new JobPackLine()
								{
									PackageType = "PLT",
									Unit = 2 ,
									Weight = 500,
									WeightUnit = "KG",
									Volume = 1.2M,
									VolumeUnit = "M3",
									DGSubstance = "1200N"
								}
							}
						}
					}
				},
				RateParties = new[]
				{
					new OrganisationRole { Role = OrganisationRole.Roles.CNE, Code = "CWCD01" },
				},
			};

			return query;
		}
	}
}
