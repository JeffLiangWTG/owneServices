using CargoWise.EntityFramework.Testing;

namespace Enterprise.Rating.GUI.Testing
{
	public class WizardPageStepTest : TestCaseWithFactory
	{
		public void TestWizardHeader()
		{
			using (var wizard = new WizardForm(null))
			{
				var page = new WizardPageStep();
				wizard.Pages.Add(page);
				wizard.Show();
				AssertEquals(true, wizard.PageHeaderVisible);
			}
		}
	}
}
