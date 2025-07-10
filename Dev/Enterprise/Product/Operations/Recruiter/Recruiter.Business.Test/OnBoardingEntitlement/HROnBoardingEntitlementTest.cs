using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(HROnBoardingEntitlement))]
	sealed class HROnBoardingEntitlementTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => Factory.New<HROnBoardingEntitlement>();

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
