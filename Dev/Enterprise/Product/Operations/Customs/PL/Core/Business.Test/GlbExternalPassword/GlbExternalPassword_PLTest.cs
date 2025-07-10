using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(GlbExternalPassword_PL))]
sealed class GlbExternalPassword_PLTest : MasterFiles.Business.Testing.GlbExternalPasswordWithCertificateTest<GlbExternalPassword_PL>
{
	public override void TestSetDefaultValues()
	{
		var password = Factory.New<GlbExternalPassword_PL>();

		AssertEquals("PLB", password.GP_PasswordType);
		AssertEquals(ZString.Empty, password.GP_PasswordStatus);
	}

	public void TestCaptions()
	{
		var password = Factory.New<GlbExternalPassword_PL>();
		AssertEquals("PUESC login", DataBoundResourceStrings.GetDataForProperty(password.GP_MailBoxIDInfo).Caption);
		AssertEquals("Password", DataBoundResourceStrings.GetDataForProperty(password.CurrentDecryptedPasswordInfo).Caption);
	}

	public void TestCurrentDecryptedPassword_IsCleared()
	{
		var password = Factory.New<GlbExternalPassword_PL>();
		password.GP_MailBoxID = "Login";
		password.CurrentDecryptedPassword = "Password";
		password.GP_MailBoxID = ZString.Empty;
		AssertEquals(ZString.Empty, password.CurrentDecryptedPassword);
	}

	public void TestCurrentDecryptedPasswordInfo_IsReadOnly()
	{
		var password = Factory.New<GlbExternalPassword_PL>();
		CombineAssertions(() =>
		{
			AssertEquals("CurrentDecryptedPassword should be readonly", true, password.CurrentDecryptedPasswordInfo.ReadOnly);
			password.GP_MailBoxID = "SomeLogin";
			AssertEquals("CurrentDecryptedPassword should not be readonly", false, password.CurrentDecryptedPasswordInfo.ReadOnly);
		});
	}

	public void TestGP_PasswordStatus_IsReadOnly()
	{
		var password = Factory.New<GlbExternalPassword_PL>();
		AssertEquals(true, password.GP_PasswordStatusInfo.ReadOnly);
	}

	public void TestIsLogged()
	{
		var glbStaff = Factory.CreateStaffAndGlbExternalPasswordWithBranch(login: "Login", isActive: true, mailBox: default, password: "Password");
		var password = GlbStaffWrapper.Get(glbStaff).GlbExternalPassword;
		Factory.Save();

		AssertNotNull("AutoCreatedLog is not null", password.Logs.AutoCreatedLog);
		AssertEquals("Log reference is PLB", PasswordTypesList.Codes.PLB, password.Logs.AutoCreatedLog.SL_Reference);
	}
}
