using System;
using System.Drawing;
using System.Text;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.AutomatedRejection;
using Enterprise.Recruiter.Business.Testing;
using Enterprise.Recruitment.Registry;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Recruiter.Testing.AutomatedRejection
{
	sealed class RejectionEmailHelpersTest : TransactionedTestCase
	{
		public void TestBuildRejectionEmail()
		{
			var img = new Bitmap(1, 1);
			img.SetPixel(0, 0, Color.MediumAquamarine);
			var dummyEmailAddress = @"test@dummytest.com";
			using (RecruitmentDataRegistry.Instance.AutomatedRejection_EmailSignatureImage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, img))
			using (RecruitmentDataRegistry.Instance.SenderAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, dummyEmailAddress))
			{
				var factory = new BusinessObjectFactory();
				var application = AutomatedRejectionTestHelper.CreateApplication(factory, "Bob Ross");
				factory.Save();
				GlbCompany.CurrentCompany.CompanyName = "WiseTech Global";
				var email = new RejectionEmailHelpers().BuildRejectionEmail(application, null);

				AssertEquals(EmailContentTypes.HTML, email.ContentType);
				AssertEquals(1, email.Recipients.Count);
				AssertEquals(application.Applicant.Email, email.Recipients[0].Email);

				AssertEquals(dummyEmailAddress, email.FromAddress);
				AssertEquals("WiseTech Global Recruitment", email.FromDisplayName);

				AssertEquals("Your application with WiseTech Global", email.Subject);

				AssertEquals(1, email.Attachments.Count);
				EmailGenerationHelperTests.AssertEmailHasImage(email, img);

				AssertContains("Bob", email.Body);
				AssertContains(@"https://www.linkedin.com/company/wisetech-global", email.Body);
				AssertContains("emailSignatureImageCID", email.Body);
			}
		}

		public void TestBuildRejectionEmail_SenderAddressNotSet()
		{
			using (RecruitmentDataRegistry.Instance.SenderAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, null))
			{
				var email = new RejectionEmailHelpers().BuildRejectionEmail(null, DummyLogger(out var logs));

				AssertNull("Email should not be generated", email);
				AssertEquals("Error: 'Recruitment > Mail > Outgoing > Sender Address' must be set in the registry.", logs.ToString().Trim());
			}
		}

		ILogger DummyLogger(out StringBuilder logs)
		{
			var sb = logs = new StringBuilder();

			var logger = new DummyLogger();
			logger.OnLog += (o, e) => sb.AppendLine(e.Type + ": " + e.Message);
			return logger;
		}
	}
}
