using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(EmailToApplicantBusinessObject))]
	sealed class EmailToApplicantBusinessObjectTest : HtmlFormatEmailToContactBusinessObjectTestCase<EmailToApplicantBusinessObject>
	{
		public void TestDefaultToAndFrom()
		{
			HRJobApplicant applicant = Factory.New<HRJobApplicant>();
			applicant.HA_FullName = "Dirk Diggler";
			applicant.HA_EmailAddress = "dirk@cargowise.com";

			EmailToApplicantBusinessObject bizO = new EmailToApplicantBusinessObject(applicant);
			AssertEquals("Dirk Diggler", bizO.ToDisplayName);
			AssertEquals("dirk@cargowise.com", bizO.ToEmailAddress);
			AssertEquals(applicant.FromDisplayName, bizO.FromDisplayName);
			AssertEquals(applicant.FromAddress, bizO.FromEmailAddress);
		}

		public void TestPreformattedBody()
		{
			HRJobApplicant applicant = Factory.New<HRJobApplicant>();
			applicant.HA_FullName = "name";
			applicant.HA_EmailAddress = "test@cargowise.com";
			EmailToApplicantBusinessObject bizO = new EmailToApplicantBusinessObject(applicant);
			bizO.Body = "meh";
			bizO.SendEmail();
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			Assert(Env.OutgoingMailManager.EmailsCreated[0].Body.Contains("<pre style='font-family:verdana,arial;font-size:12px'><br/><br/>meh</pre>"));
		}

		protected override CargoWise.EntityFramework.BusinessObject GetNewBusinessObject()
		{
			return new EmailToApplicantBusinessObject((HRJobApplicant)BusinessObjectSendingEmail);
		}

		protected override string DefaultFromEmailAddress { get { return Env.Instance.Registry.SMTPDefaultDoNotReplyEmailAddress; } }
		protected override string DefaultFromDisplayName { get { return ((HRJobApplicant)GetNewBusinessObjectSendingEmail()).FromDisplayName; } }

		protected override CargoWise.EntityFramework.BusinessObject GetNewBusinessObjectSendingEmail()
		{
			var applicant = Factory.New<HRJobApplicant>();
			applicant.HA_FullName = "name";

			return applicant;
		}

		public override void TestSetupDefaultFromAddressCore()
		{
			var emailWithAttachment = GetEmailWithAttachment();
			AssertEquals("UseCurrentUsersNameAndTitle by default", false, emailWithAttachment.UseCurrentUsersNameAndTitle);
			AssertEquals("UseCurrentUsersEmailAddress by default", false, emailWithAttachment.UseCurrentUsersEmailAddress);
			AssertEquals("FromDisplayName", DefaultFromDisplayName, emailWithAttachment.FromDisplayName);
			AssertEquals("FromEmailAddress", DefaultFromEmailAddress, emailWithAttachment.FromEmailAddress);
			AssertEquals("FromDisplayName readonly", false, emailWithAttachment.FromDisplayNameInfo.ReadOnly);
			AssertEquals("FromEmailAddress readonly", false, emailWithAttachment.FromEmailAddressInfo.ReadOnly);
		}
	}
}
