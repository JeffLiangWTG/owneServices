using System;
using System.Drawing;
using System.IO;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.AutomatedRejection;
using Enterprise.Recruiter.Business.Testing;
using Enterprise.Recruitment.Registry;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Recruiter.Testing.AutomatedRejection
{
	sealed class EmailGenerationHelperTests : TransactionedTestCase
	{
		#region CreatePreviewSample

		public void TestCreatePreviewSample()
		{
			using (RecruitmentDataRegistry.Instance.AutomatedRejection_LinkedInURL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://www.likedin.com/abc"))
			{
				var lookup = new RejectionEmailGenerator().CreatePreviewSample() as EmailGenerationLookup;

				AssertNotNull(lookup);

				AssertEquals(GlbCompany.CurrentCompany.CompanyName, lookup.CompanyName);
				AssertEquals("John", lookup.FirstName);
				AssertEquals("http://www.likedin.com/abc", lookup.LinkedInURL);
				AssertEquals("", lookup.CompanySignatureLogoHtml);
			}
		}

		#endregion

		#region BuildEmail

		public void TestBuildEmail()
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
				var generator = new RejectionEmailGenerator(new DocRecruitmentAutoRejectionEmailParser(application.Factory));

				var email = generator.BuildEmail(application.Applicant.Email, new NotificationEmailTemplate()
				{
					EmailSubject = "Email From (*CompanyName*)",
					EmailBody = "(*FirstName*)-(*CompanyName*)-(*LinkedInURL*)-(*CompanySignatureLogoHtml*)",
				}, generator.CreateLookup(application));

				AssertEquals(EmailContentTypes.HTML, email.ContentType);
				AssertEquals(1, email.Recipients.Count);
				AssertEquals(application.Applicant.Email, email.Recipients[0].Email);

				AssertEquals(dummyEmailAddress, email.FromAddress);
				AssertEquals("WiseTech Global Recruitment", email.FromDisplayName);

				AssertEquals("Email From WiseTech Global", email.Subject);

				AssertEquals(1, email.Attachments.Count);
				AssertEmailHasImage(email, img);

				AssertEquals("Bob-WiseTech Global-https://www.linkedin.com/company/wisetech-global/-<img src=\"cid:emailSignatureImageCID\" width=\"1\" height=\"1\" />", email.Body);
			}
		}

		#endregion

		#region AddImage

		public void TestAddImageToEmail()
		{
			var email = new EmailDef();
			var img = new Bitmap(1, 1);
			img.SetPixel(0, 0, Color.Chartreuse);

			var result = RejectionEmailGenerator.AddImageToEmail(email, img);

			AssertEquals(true, result);
			AssertEmailHasImage(email, img);
		}

		public void TestAddImageToEmailNullParams()
		{
			var email = new EmailDef();
			var img = new Bitmap(1, 1);
			img.SetPixel(0, 0, Color.Chartreuse);

			AssertEquals(false, RejectionEmailGenerator.AddImageToEmail(null, img));
			AssertEquals(false, RejectionEmailGenerator.AddImageToEmail(email, null));
			AssertEquals(false, RejectionEmailGenerator.AddImageToEmail(null, null));
		}

		public static void AssertEmailHasImage(EmailDef email, Image expected)
		{
			using (var ms = new MemoryStream(email.Attachments[0].Data))
			{
				var actual = new Bitmap(ms);
				AssertImageEquals("Image colours didn't match", expected, actual, 1);
			}
		}

		#endregion

		#region FirstName

		public void TestFirstName()
		{
			var factory = new BusinessObjectFactory();
			var application = AutomatedRejectionTestHelper.CreateApplication(factory, "Bob Ross");
			var result = RejectionEmailGenerator.FirstName(application);
			AssertEquals("Bob", result);
		}

		public void TestFirstNameHasNoSpace()
		{
			var factory = new BusinessObjectFactory();
			var application = AutomatedRejectionTestHelper.CreateApplication(factory, "BobRoss");
			var result = RejectionEmailGenerator.FirstName(application);
			AssertEquals("BobRoss", result);
		}

		public void TestFirstNameIsEmpty()
		{
			var factory = new BusinessObjectFactory();
			var application = AutomatedRejectionTestHelper.CreateApplication(factory, string.Empty);
			var result = RejectionEmailGenerator.FirstName(application);
			AssertEquals(string.Empty, result);
		}

		public void TestFirstNameCandidateIsNull()
		{
			var result = RejectionEmailGenerator.FirstName(null);
			AssertEquals(RejectionEmailGenerator.NullString, result);
		}

		#endregion
	}
}
