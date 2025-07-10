using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class PutawayHelperTest : WhsSecureServiceTestCase
	{
		[TestDate(2022, 7, 15, 10, 10, 10)]
		public void TestFinalisedPutawayJobCreated()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("S1", "Staff1");

			var putawayJob = PutawayHelper.CreateFinalisedPutawayJobWithPutawayLine(Helper.Factory, data.Whs1, staff, "PLAT");
			AssertEquals("User correct", staff.GS_Code, putawayJob.WPJ_GS_NKUser);
			AssertEquals("Warehouse correct", data.Whs1.PK, putawayJob.WPJ_WW_Warehouse);

			var putawayLines = Helper.Factory.Load<WhsPutawayLine>(new ZQuery(WhsPutawayLineSchema.WPL_WPJ_PutawayJob, putawayJob.PK));
			AssertNotNull(putawayLines);
			AssertEquals(1, putawayLines.Length);

			var putawayLine = putawayLines[0];
			AssertEquals("IsPuttingAway correct", false, putawayLine.WPL_IsPuttingAway);
			AssertEquals("Finalised Date correct", new ZDateTime(2022, 7, 15, 10, 10, 10), putawayJob.WPJ_FinalizedTimeUtc);
		}
	}
}
