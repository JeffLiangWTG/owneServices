using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.Shared
{
	[TestedType(typeof(TaxInfo))]
	sealed class TaxInfoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new TaxInfo
			{
				Code = "Test",
				Country = new Country(Factory, new CommonContext(Factory).Countries) { Code = "AU" },
				Description = "destesttest",
				LongLabel = "long test",
				ShortLabel = "short test",
				Number = "num1001222"
			};
		}
	}
}
