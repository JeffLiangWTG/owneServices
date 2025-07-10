using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.NL.ExitControl.Business.Testing;

sealed class TemplateTestCase : TestCaseWithFactory
{
	public void TestTemplate()
	{
		AssertEquals("Please implement a test before checkin", true, IsTemplate);
	}

	bool IsTemplate => GetType().FullName.IndexOf("NL.ExitControl") > -1;
}
