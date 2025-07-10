using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.DocDataObjects;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects.Testing
{
	[TestedType(typeof(Money))]
	class MoneyTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var lookups = new AgencyHouseBillLookups(Factory);

			return new Money
			{
				Amount = 123.45,
				Currency = new CodeDescription(lookups.Currencies)
				{
					Code = "AUD"
				}
			};
		}
	}
}
