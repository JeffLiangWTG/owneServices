using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusSCAOceanBillValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCB_GB()
		{
			var oceanBill = Factory.New<Integration.Customs.AU.ICusSCAOceanBill>();

			oceanBill.CB_GB = ZGuid.Empty;
			AssertHasError(oceanBill.CB_GBInfo, "Valid branch is required.");

			oceanBill.CB_GB = Factory.New<GlbBranch>().PK;
			AssertHasError(oceanBill.CB_GBInfo, "Please select a branch of " + GlbCompany.CurrentCompany.GC_Name + ".");

			oceanBill.CB_GB = GlbBranch.CurrentBranch.PK;
			AssertNoErrors(oceanBill.CB_GBInfo);
		}
	}
}
