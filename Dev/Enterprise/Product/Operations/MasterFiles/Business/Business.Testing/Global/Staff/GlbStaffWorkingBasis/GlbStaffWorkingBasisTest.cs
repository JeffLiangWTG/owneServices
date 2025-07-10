using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbStaffWorkingBasis))]
	sealed class GlbStaffWorkingBasisTest : EnterpriseBusinessObjectTestCase
	{
		[UseSnapshotProtection]
		public override void TestSaveAndDeleteBusinessObject()
		{
			using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
			{
				base.TestSaveAndDeleteBusinessObject();
			}
		}
	}
}
