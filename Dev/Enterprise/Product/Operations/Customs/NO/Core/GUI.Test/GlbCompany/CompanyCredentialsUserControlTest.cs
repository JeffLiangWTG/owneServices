using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI.Testing;

namespace Enterprise.Customs.NO.GUI.Testing;

sealed class CompanyCredentialsUserControlTest : TestCaseWithFactory
{
	public void TestCompanyBrokerageUserControl()
	{
		using var control = new CompanyCredentialsUserControl();
		_ = control.AssertContainsControl<CompanyCredentialsDetailsUserControl>("CompanyCredentialsDetailsUserControl", x => x.WithBindTo("."));
	}
}
