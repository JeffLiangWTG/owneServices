using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(DuplicateOrgForm))]
	sealed class DuplicateOrgFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new DuplicateOrgForm(Factory.New<OrgHeader>());
		}

		public void TestFormHeading()
		{
			using (DuplicateOrgForm testForm = new DuplicateOrgForm(Factory.New<OrgHeader>()))
			{
				testForm.Show();
				AssertEquals("Form text", "Possible Duplicate Organization", testForm.Text);
			}
		}

		public void TestUserDecision()
		{
			DuplicateOrgForm testForm;
			using (testForm = new DuplicateOrgForm(Factory.New<OrgHeader>()))
			{
				testForm.Show();
				testForm.SaveNewOrgButton.PerformClick();
				AssertEquals("User decision should be Yes", ContinueWithSave.Yes, testForm.UserDecision);
			}

			using (testForm = new DuplicateOrgForm(Factory.New<OrgHeader>()))
			{
				testForm.Show();
				testForm.CancelSaveButton.PerformClick();
				AssertEquals("User decision should be No", ContinueWithSave.No, testForm.UserDecision);
			}

			using (testForm = new DuplicateOrgForm(Factory.New<OrgHeader>()))
			{
				testForm.Show();
				testForm.Close();
				AssertEquals("User decision should be No", ContinueWithSave.No, testForm.UserDecision);
			}
		}
	}
}
