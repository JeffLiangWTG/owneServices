using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class GlbStaffEmailValidationSenderTest : TestCaseWithFactory
	{
		public void TestGenerateValidationEmail()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var email = Factory.NewWithValidTestData<GlbStaffEmailAddress>();
			email.GSE_EmailAddress = "email@email.com";
			email.GSE_GS = staff.PK;
			Factory.Save();

			var sender = new GlbStaffEmailValidationSender();

			Env.OutgoingMailManager.EmailsCreated.Clear();
			sender.GenerateValidationEmail(staff, "email@email.com");
			var emailDef = Env.OutgoingMailManager.EmailsCreated[0];
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var token = email.GSE_VerifyToken;
			var time = email.GSE_VerifyTokenCreateTimeUtc;
			CombineAssertions(() =>
			{
				Assert("Time is set", !time.IsEmpty);
				Assert("Token is set", !token.IsEmpty);
				Assert("Email is sent to correct address", emailDef.Recipients.Contains("email@email.com"));
				AssertContains("Email body has link to verification GLOW portal", $"stubportal.com?token={token}", emailDef.Body);
			});
		}

		public void TestGenerateValidationEmail_WhenEmailDoesntExist()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "AAA";
			var email = Factory.NewWithValidTestData<GlbStaffEmailAddress>();
			email.GSE_EmailAddress = "email@email.com";
			email.GSE_GS = staff.PK;
			Factory.Save();

			var sender = new GlbStaffEmailValidationSender();

			AssertExceptionThrown("Exception should be thrown", typeof(Exception), "Could not find email differentemail@email.com on staff AAA", () =>
			{
				sender.GenerateValidationEmail(staff, "differentemail@email.com");
			});
		}

		public void TestGenerateValidationEmail_WhenStaffHasNoEmails()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "AAA";
			Factory.Save();

			var sender = new GlbStaffEmailValidationSender();

			AssertExceptionThrown("Exception should be thrown", typeof(Exception), "Could not find email differentemail@email.com on staff AAA", () =>
			{
				sender.GenerateValidationEmail(staff, "differentemail@email.com");
			});
		}
	}
}
