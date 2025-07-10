using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class SalesTeamDetailsControlTest : TestCaseWithFactory
	{
		public void TestCoveredUnlocosModuleButtonGrid_AllowAttachDetachWithoutEditSecurity()
		{
			using (var control = new SalesTeamDetailsControlForTest())
			{
				Assert(control.CoveredUnlocosModuleButtonGrid_Exposed.AllowAttachDetachWithoutEditSecurity);
			}
		}

		[RequiresSTA]
		public void TestCoveredCountriesModuleButtonGrid_AllowAttachDetachWithoutEditSecurity()
		{
			using (var control = new SalesTeamDetailsControlForTest())
			{
				Assert(control.CoveredCountriesModuleButtonGrid_Exposed.AllowAttachDetachWithoutEditSecurity);
			}
		}
	}
}
