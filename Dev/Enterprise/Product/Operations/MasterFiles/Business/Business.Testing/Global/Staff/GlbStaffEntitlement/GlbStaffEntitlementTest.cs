using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbStaffEntitlement))]
	sealed class GlbStaffEntitlementTest : EnterpriseBusinessObjectTestCase
	{
		[UseSnapshotProtection]
		public override void TestSaveAndDeleteBusinessObject()
		{
			var disposable = new DisposableAction(() =>
			{
				if (Db.ConnectionOverrideForTest != null)
				{
					Db.ConnectionOverrideForTest.Dispose();
					Db.ConnectionOverrideForTest = null;
				}
			});

			Db.ConnectionOverrideForTest = Db.NewAdminConnection();
			using (disposable)
			{
				base.TestSaveAndDeleteBusinessObject();
			}
		}
	}
}
