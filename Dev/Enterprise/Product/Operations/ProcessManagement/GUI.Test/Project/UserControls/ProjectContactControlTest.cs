using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ProcessManagement.GUI.Test
{
	class ProjectContactControlTest : TestCaseWithFactory
	{
		public void TestContactEmailBox()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var contact = client.Contacts.AddNew();
			contact.OC_ContactName = "Jenny";
			contact.OC_Email = "jenny.nguyen@wisetechglobal.com";
			Factory.Save();

			var project = Factory.New<Project>();
			project.WKP_OA_ClientAddress = client.MainAddress.PK;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (var form = new ProjectForm(project))
			using (var control = new ProjectContactControl())
			{
				form.Controls.Add(control);
				form.Show();
				var emailBox = control.GetContactEmailBox();
				AssertEquals("Disable box to avoid accidental clicking and bringing up the Send Email form", false, emailBox.Enabled);
				project.WKP_OC_Contact = contact.PK;
				AssertEquals("Enable box to be able to click and bring up the Send Email form", true, emailBox.Enabled);

				UnitTestUserNotification.Instance.AddOKAnswer();
				PerformClick(emailBox);
				AssertEquals(false, project.IsInDatabase);
				AssertEquals(true, project.HasChanges);
				AssertEquals("LastMessage", "Changes have been made to this record. Please save before an E-mail can be sent.", UnitTestUserNotification.Instance.LastMessage.Text);

				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				AssertEquals(false, project.HasChanges);
				PerformClick(emailBox);
				AssertEquals(true, project.IsInDatabase);
				AssertEquals("LastMessage", null, UnitTestUserNotification.Instance.LastMessage.Text);
				EmailSenderForm senderForm = ZFormModaliser.LastFormShownDialogForTest as EmailSenderForm;
				AssertNotNull(senderForm);

				project.WKP_Summary = "Trigger HasChanges event";

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				AssertEquals(true, project.HasChanges);
				PerformClick(emailBox);
				AssertEquals(true, project.IsInDatabase);
				AssertEquals("LastMessage", "Changes have been made to this record. Please save before an E-mail can be sent.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		void PerformClick(Control control)
		{
			var onClickMethodInfo = control.GetType().GetMethod("OnClick", BindingFlags.NonPublic | BindingFlags.Instance);
			onClickMethodInfo.Invoke(control, new object[] { System.EventArgs.Empty });
		}
	}
}
