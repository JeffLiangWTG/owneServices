using System;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Business.MessageProcessors.Testing
{
	sealed class EmailDefBuilderTest : TestCaseWithFactory
	{
		public void TestGetJobLink()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "DECLARATION";
			var link = EmailDefBuilder.GetJobLink(declaration, declaration.JE_DeclarationReference);
			var expectedLink = string.Format(@"<a href=""{0}"">DECLARATION</a>", ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.Customs.JobDeclaration, declaration.PK.ToGuid()));
			AssertEquals("Link", expectedLink, link);

			link = EmailDefBuilder.GetJobLink(null, declaration.JE_DeclarationReference);
			AssertEquals("Link empty when object null", string.Empty, link);
		}

		public void TestGetJobLinkByController()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_FullName = "STAFF TEST";
			var link = EmailDefBuilder.GetJobLink(ControllerIDs.GlbStaff, staff.PK.ToGuid(), staff.GS_FullName);
			var expectedLink = string.Format(@"<a href=""{0}"">STAFF TEST</a>", ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.GlbStaff, staff.PK.ToGuid()));
			AssertEquals("Link", expectedLink, link);

			link = EmailDefBuilder.GetJobLink(null, staff.PK.ToGuid(), staff.GS_FullName);
			AssertEquals("Link empty when object null", string.Empty, link);

			link = EmailDefBuilder.GetJobLink(ControllerIDs.GlbStaff, Guid.Empty, staff.GS_FullName);
			AssertEquals("Link empty when object null", string.Empty, link);
		}

		public void TestToEmail()
		{
			var emailBuilder = new EmailDefBuilder("Test Subject", "Test Attachment Text", EmailDefBuilder.HtmlTemplates.ErrorResponse);
			emailBuilder.AddArgReplacementRange("ARGREPLACEMENT1", "ARGREPLACEMENT2", "ARGREPLACEMENT3");
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.FromMessageSender, "from the CBSA ");
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml1, "Text Replacement 1");
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml2, "Text Replacement 2");

			AssertEmailBodyText(emailBuilder.ToString());

			var email = emailBuilder.ToEmail();
			AssertEquals("Subject", "Test Subject", email.Subject);
			AssertEmailBodyText(email.Body);
			AssertEquals("Attachments Count", 3, email.Attachments.Count);
			AssertEquals("Attachment DisplayName", "Message.txt", email.Attachments[2].DisplayName);
			AssertEquals("Attachment Data", "Test Attachment Text", Encoding.ASCII.GetString(email.Attachments[2].Data));
		}

		void AssertEmailBodyText(string emailBodyText)
		{
			#region ExpectedHtmlText

			const string expectedHtmlText = @"<br />
<strong>Job Number : ARGREPLACEMENT1<br />
Reference Number : ARGREPLACEMENT2<br />
<br />
</strong>An ERROR response message has been received from the CBSA for an ARGREPLACEMENT3.<br />
<p class=""MsoNormal"" style=""margin: 0cm 0cm 0pt; text-align: justify; tab-stops: -72.0pt"">
    The message sent for the above mentioned job had the following errors.</p>
<br />
<br />
Text Replacement 1
<br />
<br />
Text Replacement 2
<br />
<hr />
<br />
<!--EndSection Details-->
Regards,<br />
<br />
CargoWise One Administrative Messages Sender<br />
<br />
<br />
<br />";

			#endregion

			AssertContains("Body", expectedHtmlText, emailBodyText);
		}

		public void TestFreeFormHTML()
		{
			var emailBuilder = new EmailDefBuilder("Test Subject", "Test Attachment Text", EmailDefBuilder.HtmlTemplates.FreeFormResponse);
			emailBuilder.AddArgReplacementRange("ARGREPLACEMENT1");
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.FromMessageSender, " from the ACS");
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml1, "Text Replacement 1");
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml2, "Text Replacement 2");
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml3, "Text Replacement 3");
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.HeaderSectionDetails, "Header Section");
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.EndSectionDetails, "End Section");

			const string expectedHtmlText = @"<br />
<strong>
Header Section
<br />
</strong>An ARGREPLACEMENT1 message has been received from the ACS.<br />
<br />
Text Replacement 1
<br />
Text Replacement 2
<br />
Text Replacement 3
<hr />
<br />
<strong><large>
End Section</strong></large>
Regards,<br />
<br />
CargoWise One Administrative Messages Sender<br />
<br />
<br />
<br />";
			AssertContains("Free Form Body", expectedHtmlText, emailBuilder.ToString());
		}

		public void TestStatusUpdateHTML()
		{
			var emailBuilder = new EmailDefBuilder("Test Subject", "Test Attachment Text", EmailDefBuilder.HtmlTemplates.StatusUpdate);
			emailBuilder.AddArgReplacementRange("ARGREPLACEMENT1", "ARGREPLACEMENT2", "ARGREPLACEMENT3");
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.FromMessageSender, "from the ACS ");
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml1, "Text Replacement 1");
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.AdditionalComments, "Comments");
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.EndSectionDetails, "End Section");

			const string expectedHtmlText = @"<br />
<strong>Job Number : ARGREPLACEMENT1<br />
Reference Number : ARGREPLACEMENT2<br />
<br />
</strong>A 'Status Update' message has been received from the ACS for an ARGREPLACEMENT3.<br />
Please see message details below.<br />
<br />Comments
<br />
Text Replacement 1
<br />
<hr />
<br />
<br />
End Section
Regards,<br />
<br />
CargoWise One Administrative Messages Sender<br />
<br />
<br />
<br />";
			AssertContains("Status Update Body", expectedHtmlText, emailBuilder.ToString());
		}

		public void TestAddDynamicHtmlReplacement()
		{
			CombineAssertions(() =>
			{
				var emailBuilder = new EmailDefBuilder("Test Subject", "Test Attachment Text", EmailDefBuilder.HtmlTemplates.EmptyWithDynamicHtml5);
				AssertContains("<!--DynamicHtml1--><!--DynamicHtml2--><!--DynamicHtml3--><!--DynamicHtml4--><!--DynamicHtml5-->", emailBuilder.ToString());
				emailBuilder.AddDynamicHtmlReplacement("HELLO");
				AssertContains("HELLO<br /><!--DynamicHtml2--><!--DynamicHtml3--><!--DynamicHtml4--><!--DynamicHtml5-->", emailBuilder.ToString());
				emailBuilder.AddDynamicHtmlReplacement("BYE");
				AssertContains("HELLO<br />BYE<br /><!--DynamicHtml3--><!--DynamicHtml4--><!--DynamicHtml5-->", emailBuilder.ToString());
				emailBuilder.AddDynamicHtmlReplacement("HI");
				AssertContains("HELLO<br />BYE<br />HI<br /><!--DynamicHtml4--><!--DynamicHtml5-->", emailBuilder.ToString());
				emailBuilder.AddDynamicHtmlReplacement("SEE YA");
				AssertContains("HELLO<br />BYE<br />HI<br />SEE YA<br /><!--DynamicHtml5-->", emailBuilder.ToString());
				emailBuilder.AddDynamicHtmlReplacement("GDAY");
				AssertContains("HELLO<br />BYE<br />HI<br />SEE YA<br />GDAY<br />", emailBuilder.ToString());
				emailBuilder.AddDynamicHtmlReplacement("HOWDY");
				AssertContains("HELLO<br />BYE<br />HI<br />SEE YA<br />GDAY<br />", emailBuilder.ToString());
				emailBuilder = new EmailDefBuilder("Test Subject", "Test Attachment Text", EmailDefBuilder.HtmlTemplates.EmptyWithDynamicHtml5);
				emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml1, "HI");
				emailBuilder.AddDynamicHtmlReplacement("HELLO");
				AssertContains("HIHELLO<br /><!--DynamicHtml3--><!--DynamicHtml4--><!--DynamicHtml5-->", emailBuilder.ToString());
			});
		}
	}
}
