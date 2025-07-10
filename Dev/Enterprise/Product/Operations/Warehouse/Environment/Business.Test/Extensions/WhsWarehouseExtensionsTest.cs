using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	class WhsWarehouseExtensionsTest : TestCaseWithFactory
	{
		public void TestGetWarehouseBranchDateTimeOffset()
		{
			var helper = new WhsTestHelperFunctionsEnv(Factory);
			var sgBranch = helper.CreateGlbBranch("ABC");
			sgBranch.GB_RL_NKHomePort = "SGSIN";

			var whs = helper.CreateWarehouse("WH1");
			whs.WW_GB_RelatedCompanyBranch = sgBranch.PK;

			var utcNow = ZDateTime.UtcNow;
			AssertEquals(utcNow.ToLocationTime(Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "SGSIN"))), whs.GetWarehouseBranchDateTimeOffset(utcNow));
		}

		public void TestGetWarehouseBranchDateTimeOffset_InvalidHomePortCode()
		{
			var helper = new WhsTestHelperFunctionsEnv(Factory);
			var dummyBranch = helper.CreateGlbBranch("ABC");
			dummyBranch.GB_RL_NKHomePort = "XXYYZ";

			var whs = helper.CreateWarehouse("WH1");
			whs.WW_GB_RelatedCompanyBranch = dummyBranch.PK;

			AssertNoExceptionThrown("No exception is thrown if port code is invalid and there is no branch time zone.", () => whs.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow));
		}
	}
}
