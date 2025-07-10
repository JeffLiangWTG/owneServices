using System;
using System.Drawing;
using System.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(HtmlEmailWithAttachment))]
	sealed class HtmlEmailWithAttachmentTest : EmailWithAttachmentTestCase<HtmlEmailWithAttachment>
	{
		public void TestICustomFieldProvider()
		{
			ISendEmailSource source = Factory.New<OrgHeader>();
			ICustomFieldProvider email = new HtmlEmailWithAttachment(source);
			AssertEquals(((ICustomFieldProvider)source).GetCustomBusinessObject(), email.GetCustomBusinessObject());
		}

		public override void TestCheckIsReadyToSendEmail()
		{
			var emailWithAttachment = GetEmailWithAttachment();
			emailWithAttachment.Attachment = "!@#";
			emailWithAttachment.Cc = "!@#";
			emailWithAttachment.Bcc = "!@#";
			emailWithAttachment.FromEmailAddress = "!@#";
			emailWithAttachment.Body = "";
			emailWithAttachment.ToEmailAddress = "!@#";

			AssertEquals("IsReadyToSendEmail", false, emailWithAttachment.CheckIsReadyToSendEmail());
			AssertHasErrors(emailWithAttachment.AttachmentInfo);
			AssertHasErrors(emailWithAttachment.CcInfo);
			AssertHasErrors(emailWithAttachment.BccInfo);
			AssertHasErrors(emailWithAttachment.FromEmailAddressInfo);
			AssertHasErrors(emailWithAttachment.BodyInfo);
			AssertHasErrors(emailWithAttachment.ToEmailAddressInfo);

			emailWithAttachment.Attachment = "";
			emailWithAttachment.Cc = "cc@cc.com";
			emailWithAttachment.Bcc = "bcc@bcc.com";
			emailWithAttachment.FromEmailAddress = "from@from.com";
			emailWithAttachment.Body = "Some body I used to know";
			emailWithAttachment.ToEmailAddress = "to@to.com";

			AssertEquals("IsReadyToSendEmail", true, emailWithAttachment.CheckIsReadyToSendEmail());
			AssertNoErrors(emailWithAttachment);
		}

		public void TestBannerAndFooterAreCompanySpecific()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			var branch1 = company1.Branches.AddNew();
			branch1.GB_Code = "AAA";
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			var branch2 = company2.Branches.AddNew();
			branch2.GB_Code = "BBB";
			Factory.Save();
			SystemDataRegistry.Instance.HtmlEmailBannerImage.SetValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, new Bitmap(1, 1));
			SystemDataRegistry.Instance.HtmlEmailFooterImage.SetValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, new Bitmap(2, 2));
			SystemDataRegistry.Instance.HtmlEmailBannerImage.SetValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, new Bitmap(3, 3));
			SystemDataRegistry.Instance.HtmlEmailFooterImage.SetValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, new Bitmap(9, 9));

			ISendEmailSource source = Factory.New<OrgHeader>();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branch1.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				EmailDef email = new HtmlEmailWithAttachment(source).GetEmail();
				AssertNotNull("Email should be created", email);
				AssertEquals("Attachments", 2, email.Attachments.Count);
				AssertEquals("Banner.jpg", email.Attachments[0].DisplayName);
				AssertEquals("Footer.jpg", email.Attachments[1].DisplayName);
				AssertAttachment(email.Attachments[0], new Size(1, 1));
				AssertAttachment(email.Attachments[1], new Size(2, 2));
			}

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branch2.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				EmailDef email = new HtmlEmailWithAttachment(source).GetEmail();
				AssertNotNull("Email should be created", email);
				AssertEquals("Attachments", 2, email.Attachments.Count);
				AssertEquals("Banner.jpg", email.Attachments[0].DisplayName);
				AssertEquals("Footer.jpg", email.Attachments[1].DisplayName);
				AssertAttachment(email.Attachments[0], new Size(3, 3));
				AssertAttachment(email.Attachments[1], new Size(9, 9));
			}
		}

		void AssertAttachment(AttachmentDef attachment, Size expectedSize)
		{
			using (MemoryStream stream = new MemoryStream(attachment.Data))
			using (Image image = Image.FromStream(stream))
			{
				AssertEquals(attachment.DisplayName + " Size", expectedSize, image.Size);
			}
		}

		public void TestNormaliseWhitespaceCharactersForHtml()
		{
			ISendEmailSource source = Factory.New<OrgHeader>();
			var email = new HtmlEmailWithAttachmentForTest(source);
			ZString input = "Line one\n\tLine two\r\t\tLine three\r\nLine four";
			ZString expectedOutput = "Line one<br />&nbsp;&nbsp;&nbsp;&nbsp;Line two<br />&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Line three<br />Line four";
			AssertEquals(expectedOutput, email.NormaliseWhitespaceCharactersForHtml_Exposed(input));
		}

		public void TestNormaliseNewLine()
		{
			ISendEmailSource source = Factory.New<OrgHeader>();
			var email = new HtmlEmailWithAttachmentForTest(source);
			ZString input = "Line one\nLine two\rLine three\r\nLine four\r\n\r\nLine five\r\r\nLine six\r\n\nLine seven\r\n\rLine eight\n\r\nLine nine\n\n\r";
			ZString expectedOutput = "Line one\r\nLine two\r\nLine three\r\nLine four\r\n\r\nLine five\r\n\r\nLine six\r\n\r\nLine seven\r\n\r\nLine eight\r\n\r\nLine nine\r\n\r\n\r\n";
			AssertEquals(expectedOutput, email.NormaliseNewLine_Exposed(input));
		}

		protected override HtmlEmailWithAttachment GetEmailWithAttachment()
		{
			ISendEmailSource source = Factory.NewWithValidTestData<OrgHeader>();
			return new HtmlEmailWithAttachment(source);
		}

		protected override string DefaultFromEmailAddress { get { return Env.Registry.SMTPDefaultReturnEmailAddress; } }
		protected override string DefaultFromDisplayName { get { return GlbStaff.CurrentUser.GS_FullName; } }

		public override void TestAllRecipientsCommaDelimited()
		{
			var emailWithAttachment = GetEmailWithAttachment();
			AssertEquals("AllRecipientsCommaDelimited", "", emailWithAttachment.AllRecipientsCommaDelimited);
			emailWithAttachment.ToEmailAddress = "a@a.com;b@b.com;";
			AssertEquals("AllRecipientsCommaDelimited", "a@a.com, b@b.com", emailWithAttachment.AllRecipientsCommaDelimited);
			emailWithAttachment.Cc = "c@c.com;d@d.com;";
			AssertEquals("AllRecipientsCommaDelimited", "a@a.com, b@b.com, c@c.com, d@d.com", emailWithAttachment.AllRecipientsCommaDelimited);
			emailWithAttachment.ToEmailAddress = "";
			AssertEquals("AllRecipientsCommaDelimited", "c@c.com, d@d.com", emailWithAttachment.AllRecipientsCommaDelimited);
			emailWithAttachment.Bcc = "e@e.com;f@f.com;";
			AssertEquals("AllRecipientsCommaDelimited", "c@c.com, d@d.com, e@e.com, f@f.com", emailWithAttachment.AllRecipientsCommaDelimited);
		}
	}
}
