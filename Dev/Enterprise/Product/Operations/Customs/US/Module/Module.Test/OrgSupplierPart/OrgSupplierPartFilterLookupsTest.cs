using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.Module.Testing
{
	sealed class OrgSupplierPartFilterLookupsTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		public void TestLookups()
		{
			AssertEquals("Tariffs", typeof(USCTariffCollection), lookups.Tariffs.GetType());
			AssertEquals("Tariffs", typeof(ClassificationTypeList), lookups.TariffTypeList.GetType());
		}

		protected override void SetUp()
		{
			base.SetUp();
			var filterBizObj = new OrgSupplierPartFilterStripBusinessObject();
			lookups = filterBizObj.Lookups;
		}

		OrgSupplierPartFilterLookups lookups;
	}
}
