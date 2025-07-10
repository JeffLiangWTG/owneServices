using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbStaffEntitlementType))]
	class GlbStaffEntitlementTypeTest : EnterpriseBusinessObjectTestCase
	{
		[UseSnapshotProtection]
		public override void TestSaveAndDeleteBusinessObject()
		{
			Db.ConnectionOverrideForTest = Db.NewAdminConnection();
			base.TestSaveAndDeleteBusinessObject();
		}

		[UseSnapshotProtection]
		public override void TestFetchForLoad()
		{
			Db.ConnectionOverrideForTest = Db.NewAdminConnection();
			base.TestFetchForLoad();
		}

		[UseSnapshotProtection]
		public override void TestCalcPropertiesWithDbHitsUseFetchHints()
		{
			Db.ConnectionOverrideForTest = Db.NewAdminConnection();
			base.TestCalcPropertiesWithDbHitsUseFetchHints();
		}

		protected override void TearDown()
		{
			base.TearDown();

			if (Db.ConnectionOverrideForTest != null)
			{
				Db.ConnectionOverrideForTest.Dispose();
				Db.ConnectionOverrideForTest = null;
			}
		}
	}
}
