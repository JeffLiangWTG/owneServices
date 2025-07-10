using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ViewMostRecentGenApprovalRequest))]
	sealed class ViewMostRecentGenApprovalRequestTest : EnterpriseBusinessObjectTestCase
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This business object is a view", condition: true);
		}

		public void TestOnlyMostRecentGenApprovalRequestBySameParent()
		{
			var older = Factory.NewWithValidTestData<GenApprovalRequest>();
			var newer = Factory.NewWithValidTestData<GenApprovalRequest>();

			older.XP_ParentID = newer.XP_ParentID = ZGuid.NewZGuid();
			older.XP_SubSystem = newer.XP_SubSystem = "TST";

			newer.XP_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(-1);
			older.XP_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(-4);

			Factory.Save();

			var results = Factory.Load<ViewMostRecentGenApprovalRequest>(new ZQuery(ViewMostRecentGenApprovalRequestSchema.XP_SubSystem, "TST"));

			AssertEquals("Only one result expected", 1, results.Length);
			AssertEquals("Only the newest record expected", newer.PK, results[0].PK);
		}
	}
}
