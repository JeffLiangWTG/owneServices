using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccSurchargeBasisCollection))]
	sealed class AccSurchargeBasisCollectionTest : BusinessObjectCollectionTestCase
	{
		AccSurchargeConfiguration Master;

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			Master = Factory.NewWithValidTestData<AccSurchargeConfiguration>();
			return new AccSurchargeBasisCollection(Master);
		}

		public void TestSetDefaultsForNewChild()
		{
			var collection = (AccSurchargeBasisCollection)GetCollectionToTest();
			var surchargeBasis = collection.AddNew();

			AssertEquals("ASB_ASC_SurchargeConfiguration is set to surchargeConfig.PK", Master.PK, surchargeBasis.ASB_ASC_SurchargeConfiguration);
		}
	}
}
