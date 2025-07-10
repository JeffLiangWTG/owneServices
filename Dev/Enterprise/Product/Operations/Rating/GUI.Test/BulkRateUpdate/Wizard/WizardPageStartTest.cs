using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.GUI.Testing
{
	public class WizardPageStartTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestNullImageThrowsNoExceptions()
		{
			using (var wizard = new WizardForm(null))
			{
				var page = new WizardPageStart();
				wizard.Pages.Add(page);
				AssertNull(page.Image);
				AssertNoExceptionThrown(() =>
				{
					wizard.Show();
				});
			}
		}

		public void TestWizardHeader()
		{
			using (var wizard = new WizardForm(null))
			{
				var page = new WizardPageStart();
				wizard.Pages.Add(page);
				wizard.Show();
				AssertEquals(false, wizard.PageHeaderVisible);
			}
		}
	}
}
