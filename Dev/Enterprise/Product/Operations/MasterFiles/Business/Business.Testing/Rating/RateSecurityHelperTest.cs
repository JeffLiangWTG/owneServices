using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RateSecurityHelperTest : TestCaseWithFactory
	{
		public void TestGetFirstDeniedSecurityCheckPoint()
		{
			var setupResult = RateSecurityTestHelper.GetTwoRateSecurityGroups(Factory);
			var blankRateSecurityOrg = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			using (Env.SetTemporaryUserContext(setupResult.Staff.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				Env.Security.CachingEnabled = false;

				var ohSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
				ohSubQuery.AddToFilter(OrgHeaderSchema.PK, new[] { setupResult.AllowedOrg.PK, blankRateSecurityOrg.PK });

				AssertNull(RateSecurityHelper.GetFirstDeniedSecurityCheckPoint(new List<ZGuid> { setupResult.AllowedOrg.PK, blankRateSecurityOrg.PK }, Factory));

				ohSubQuery.AddToFilter(JoinCondition.Or, OrgHeaderSchema.PK, setupResult.DeniedOrg.PK);
				AssertEquals(setupResult.DeniedSecurity, RateSecurityHelper.GetFirstDeniedSecurityCheckPoint(new List<ZGuid> { setupResult.AllowedOrg.PK, blankRateSecurityOrg.PK, setupResult.DeniedOrg.PK }, Factory).SecurityCheckPoint);
			}
		}
	}
}
