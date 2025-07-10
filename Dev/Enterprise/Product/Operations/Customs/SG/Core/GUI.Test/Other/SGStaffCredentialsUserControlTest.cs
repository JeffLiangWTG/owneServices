using System.Windows.Forms;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.GUI.Testing
{
	[TestedType(typeof(SGStaffCredentialsUserControl))]
	sealed class SGStaffCredentialsUserControlTest : MasterFiles.GUI.Testing.StaffCredentialsUserControlTest
	{
		public void TestShouldDefaultPasswordsEvent()
		{
			var userID = "TESTUSER00";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var existingPassword = Factory.New<GlbExternalPassword_SGA>();
			existingPassword.GP_GC = GlbCompany.CurrentCompany.PK;
			existingPassword.GP_GS = staff.PK;
			existingPassword.GP_MailBoxID = "USER1@MAIL.COM";
			existingPassword.CurrentDecryptedPassword = "CPW001";
			existingPassword.NextDecryptedPassword = "NPW001";
			existingPassword.GP_PasswordStatus = Core.Constants.PasswordOK;
			existingPassword.GP_UserID = userID;
			Factory.Save();
			staff = Factory.NewWithValidTestData<GlbStaff>();
			var password = Factory.New<GlbExternalPassword_SGA>();
			password.GP_GC = GlbCompany.CurrentCompany.PK;
			password.GP_GS = staff.PK;
			password.GP_MailBoxID = "USER2@MAIL.COM";
			password.CurrentDecryptedPassword = string.Empty;
			password.NextDecryptedPassword = string.Empty;
			password.GP_UserID = string.Empty;
			var wrapper = SGGlbStaffWrapper.Get(staff);
			using (var frm = new ZForm())
			using (var control = new SGStaffCredentialsUserControl())
			{
				frm.Controls.Add(control);
				control.SetDataBinding(wrapper, string.Empty);
				frm.Show();
				Application.DoEvents();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddYesAnswer();
				password.GP_UserID = userID;
				AssertEquals("Should default the CurrentDecryptedPassword.", "CPW001", password.CurrentDecryptedPassword);
				AssertEquals("Should default the NextDecryptedPassword.", "NPW001", password.NextDecryptedPassword);
				AssertEquals("Would you like to default the password and status from the linked staff record which is using same ACCESS User ID in this company?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
