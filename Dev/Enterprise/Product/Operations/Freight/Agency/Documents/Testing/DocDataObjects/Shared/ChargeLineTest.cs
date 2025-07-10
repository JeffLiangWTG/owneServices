using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.DocDataObjects;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects.Testing
{
	[TestedType(typeof(ChargeLine))]
	class ChargeLineTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var lookups = new AgencyHouseBillLookups(Factory);

			return new ChargeLine("zzz")
			{
				Description = "Hakuna Matata",
				IsPrepaid = true,
				Cost = new Money
				{
					Amount = 100,
					Currency = new CodeDescription(lookups.Currencies)
					{
						Code = "AUD"
					}
				},
				LocalCost = new Money
				{
					Amount = 100,
					Currency = new CodeDescription(lookups.Currencies)
					{
						Code = "AUD"
					}
				},
				CostExchangeRate = 1,
				Sell = new Money
				{
					Amount = 200,
					Currency = new CodeDescription(lookups.Currencies)
					{
						Code = "AUD"
					}
				},
				LocalSell = new Money
				{
					Amount = 200,
					Currency = new CodeDescription(lookups.Currencies)
					{
						Code = "AUD"
					}
				},
				SellExchangeRate = 0.79,
				CFX = 0.7,
				ChargeLineAttributes = new[]
				{
					new ChargeLineAttribute
					{
						Name = "FMC",
						Value = "FMC Value",
						Amount = 1.5M
					}
				}
			};
		}
	}
}
