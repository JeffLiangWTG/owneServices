using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(TaxFrameworkAccTaxRateLoader))]
	sealed class TaxFrameworkAccTaxRateLoaderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLoadTaxFrameworkAccTaxRates()
		{
			var factory = new BusinessObjectFactory();
			var collection = new TaxFrameworkAccTaxRateCollection(factory);
			var item = collection.AddNew();
			item.AT_TaxSystemCode = "PIB";
			Assert(!item.IsInDatabase);

			var loader = new TaxFrameworkAccTaxRateLoader();
			var loaded = loader.TaxFrameworkAccTaxRates;
			AssertEquals(0, loaded.Count);
			Assert("Tax Rate is not loaded as it is not yet in DB", !loaded.Contains(item));

			factory.Save();
			Assert(item.IsInDatabase);

			loader = new TaxFrameworkAccTaxRateLoader();
			loaded = loader.TaxFrameworkAccTaxRates;
			Assert("IsRegisteredEditableChildObject", loader.IsRegisteredEditableChildObject(loaded));
			AssertEquals(1, loaded.Count);
			Assert("Tax Rate is loaded", loaded.Contains(item));
		}
	}
}
