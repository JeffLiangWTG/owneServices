using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccAlternateChartFormatDependentCollection))]
	sealed class AccAlternateChartFormatDCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AccAlternateChartFormatDependentCollection(Factory.NewWithValidTestData<AccAlternateChart>());
		}

		public void TestSetDefaultsForNewChild()
		{
			var collection = (AccAlternateChartFormatDependentCollection)GetCollectionToTest();
			var bizo = collection.AddNew();
			AssertEquals((ZShort)1, bizo.ANF_Tier);
		}
	}
}
