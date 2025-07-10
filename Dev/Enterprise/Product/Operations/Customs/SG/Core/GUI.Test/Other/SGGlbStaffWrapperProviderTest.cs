using CargoWise.EntityFramework;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.GUI.Testing
{
	[TestedType(typeof(SGGlbStaffWrapperProvider))]
	sealed class SGGlbStaffWrapperProviderTest : MasterFiles.GUI.Testing.GlbStaffWrapperProviderTest<SGGlbStaffWrapperProvider>
	{
		public void TestShowPreSaveDialogsCore()
		{
			var userID = "TESTUSER00";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var existingPassword = Factory.New<GlbExternalPassword_SGA>();
			existingPassword.GP_GC = GlbCompany.CurrentCompany.PK;
			existingPassword.GP_GS = staff.PK;
			existingPassword.GP_MailBoxID = "USER1@MAIL.COM";
			existingPassword.CurrentDecryptedPassword = "CPW001";
			existingPassword.NextDecryptedPassword = "NPW001";
			existingPassword.GP_PasswordStatus = "INV";
			existingPassword.GP_UserID = userID;
			Factory.Save();
			staff = Factory.NewWithValidTestData<GlbStaff>();
			var password = Factory.New<GlbExternalPassword_SGA>();
			password.GP_GC = GlbCompany.CurrentCompany.PK;
			password.GP_GS = staff.PK;
			password.GP_MailBoxID = "USER2@MAIL.COM";
			password.CurrentDecryptedPassword = "CPW002";
			password.NextDecryptedPassword = "NPW002";
			password.GP_PasswordStatus = Core.Constants.PasswordOK;
			password.GP_UserID = userID;
			var wrapper = SGGlbStaffWrapper.Get(staff);
			var provider = new SGGlbStaffWrapperProvider();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddYesAnswer();
			var result = provider.ShowPreSaveDialogs(wrapper);
			AssertEquals("Should always be yes.", ContinueWithSave.Yes, result);
			AssertEquals("Should sync the CurrentDecryptedPassword.", "CPW002", existingPassword.CurrentDecryptedPassword);
			AssertEquals("Should sync the NextDecryptedPassword.", "NPW002", existingPassword.NextDecryptedPassword);
			AssertEquals("Should sync the GP_PasswordStatus.", Core.Constants.PasswordOK, existingPassword.GP_PasswordStatus);
			AssertEquals("Would you like to apply the same change to other linked staff records which are using same ACCESS User ID in this company?", UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}
}
