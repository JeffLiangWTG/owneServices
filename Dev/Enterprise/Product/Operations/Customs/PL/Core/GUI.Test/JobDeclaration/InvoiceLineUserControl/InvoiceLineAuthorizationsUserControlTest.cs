using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.GUI;

namespace Enterprise.Customs.PL.GUI.Testing;

sealed class InvoiceLineAuthorizationsUserControlTest : TestCaseWithFactory
{
	public void TestUserControl()
	{
		using (var control = new InvoiceLineAuthorisationsUserControl())
		{
			CombineAssertions(() =>
			{
				AssertEquals("Base type should be of EU invoice line authorization type"
					, typeof(InvoiceLineAuthorizationsComputedUserControl)
					, control.GetType().BaseType);
			});
		}
	}
}
