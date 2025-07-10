using CargoWise.EntityFramework.Testing;

namespace Enterprise.Rating.GUI.Testing
{
	public class WizardPageTest : TestCaseWithFactory
	{
		class TestWizardPage : WizardPage
		{
			public new WizardForm Wizard
			{
				get { return base.Wizard; }
			}

			public override void NotifyActivated(WizardForm wizard)
			{
			}

			public override void NotifyLeaving(WizardSteppingEventArgs args)
			{
			}
		}

		public void TestProperties()
		{
			using (var wizard = new WizardForm(null))
			{
				var page = new TestWizardPage();
				wizard.Pages.Add(page);
				wizard.Show();
				AssertEquals(wizard, page.Wizard);
			}
		}
	}
}
