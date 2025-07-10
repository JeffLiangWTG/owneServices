using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(TaxFrameworkAccTaxRateCollection))]
	sealed class TaxFrameworkAccTaxRateCollectionTest : ActiveBusinessObjectCollectionTestCase<TaxFrameworkAccTaxRateCollection>
	{
		public void TestSetDefaultsForNewElementCore()
		{
			var collection = new TaxFrameworkAccTaxRateCollection(Factory);
			var item = collection.AddNew();
			AssertEquals(AccTaxRate.Types.NotReportable, item.AT_Type);
		}

		public void TestElementsNotLoadedIfEmptyTaxSystemCode()
		{
			var item1 = Factory.NewWithValidTestData<TaxFrameworkAccTaxRate>();
			item1.AT_TaxSystemCode = "";
			Assert(!item1.IsInDatabase);
			var item2 = Factory.NewWithValidTestData<TaxFrameworkAccTaxRate>();
			item2.AT_TaxSystemCode = "PIB";
			Assert(!item2.IsInDatabase);

			var loaded1 = new ActiveBusinessObjectCollection<TaxFrameworkAccTaxRate>(Factory);
			var loaded2 = new TaxFrameworkAccTaxRateCollection(Factory);

			Assert(loaded1.Contains(item1));
			Assert(loaded1.Contains(item2));

			Assert("item1 should be in TaxFrameworkAccTaxRates as it has empty value on Tax System but not yet saved in database", loaded2.Contains(item1));
			Assert(loaded2.Contains(item2));

			Factory.Save();
			Assert(item1.IsInDatabase);
			Assert(item2.IsInDatabase);

			Assert(loaded1.Contains(item1));
			Assert(loaded1.Contains(item2));

			Assert("item1 should not be in TaxFrameworkAccTaxRates as it has empty value on Tax System", !loaded2.Contains(item1));
			Assert("item2 should be in TaxFrameworkAccTaxRates as it has non-empty value on Tax System", loaded2.Contains(item2));
		}
	}
}
