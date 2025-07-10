using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.DocDataObjects;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	[TestedType(typeof(LumpSum))]
	sealed class LumpSumTest : NonPersistentBusinessObjectTestCase
	{
		public void TestToString()
		{
			var lumpSum = (LumpSum)GetNewBusinessObject();

			AssertEquals("ToString", "FREIGHT LUMP SUM: 12345.67 USD", lumpSum.ToString());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var lookups = new HouseBillLookups(Factory);

			return new LumpSum(new Money
			{
				Amount = 12345.67,
				Currency = new CodeDescription(lookups.Currencies)
				{
					Code = "USD"
				}
			});
		}
	}
}
