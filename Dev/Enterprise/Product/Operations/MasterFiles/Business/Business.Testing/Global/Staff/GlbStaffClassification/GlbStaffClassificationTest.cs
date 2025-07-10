using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbStaffClassification))]
	sealed class GlbStaffClassificationTest : EnterpriseBusinessObjectTestCase
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
