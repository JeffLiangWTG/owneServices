using System;
using System.IO;
using System.Text;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(EmailWithAttachment))]
	sealed class EmailWithAttachmentTest : EmailWithAttachmentTestCase<EmailWithAttachment>
	{
		protected override EmailWithAttachment GetEmailWithAttachment()
		{
			return new EmailWithAttachment();
		}

		protected override string DefaultFromEmailAddress { get { return Env.Registry.SMTPDefaultReturnEmailAddress; } }
		protected override string DefaultFromDisplayName { get { return string.Empty; } }

		public void TestSetupDefaultFromAddressCore_DefaultFromEmailAddressSameAsCurrentUserEmailAddress()
		{
			var emailWithAttachment = new EmailWithAttachment(GlbStaff.CurrentUser.GS_EmailAddress, DefaultFromDisplayName);
			Assert("UseCurrentUsersNameAndTitle should be true by default", emailWithAttachment.UseCurrentUsersNameAndTitle);
			Assert("UseCurrentUsersEmailAddress should be true", emailWithAttachment.UseCurrentUsersEmailAddress);
			AssertEquals("FromDisplayName", GlbStaff.CurrentUser.GS_FullName, emailWithAttachment.FromDisplayName);
			AssertEquals("FromEmailAddress", GlbStaff.CurrentUser.GS_EmailAddress, emailWithAttachment.FromEmailAddress);
			Assert("FromDisplayName readonly", emailWithAttachment.FromDisplayNameInfo.ReadOnly);
			Assert("FromEmailAddress readonly", emailWithAttachment.FromEmailAddressInfo.ReadOnly);
		}

		public void TestSetupPreviewFile_NonHTML()
		{
			EmailWithAttachmentForTest emailContactObject = new EmailWithAttachmentForTest(EmailContentTypes.PlainText);
			emailContactObject.Body = "this is a text based email\r\nnewline character inserted here";
			AssertNull(emailContactObject.PreviewFilePath);

			using (emailContactObject.SetupPreviewFile())
			{
				Assert("Should be generated as a txt file", emailContactObject.PreviewFilePath.EndsWith("txt"));
				Assert("File should exist", File.Exists(emailContactObject.PreviewFilePath));
				AssertEquals("this is a text based email\r\nnewline character inserted here", File.ReadAllText(emailContactObject.PreviewFilePath));

				string tempDir = Path.GetDirectoryName(emailContactObject.PreviewFilePath);
				AssertEquals("There shouldn't be any other files", 1, Directory.GetFiles(tempDir).Length);
			}
		}

		public void TestSetupPreviewFile_HTML()
		{
			string tempFile1 = Env.GetTempFileName();
			string tempFile2 = Env.GetTempFileName();
			try
			{
				File.WriteAllText(tempFile1, "meh meh Meh");
				File.WriteAllText(tempFile2, "second file");

				EmailWithAttachmentForTest emailContactObject = new EmailWithAttachmentForTest(EmailContentTypes.HTML);
				emailContactObject.Body = "<B> IN BOLD </b> <a href='http://www.cargowise.com'> hyperlink </A> <br/> this is a html based email";
				emailContactObject.AttachmentList.Add(new CodeDescriptionPair(Path.GetFileName(tempFile1), tempFile1));
				emailContactObject.AttachmentList.Add(new CodeDescriptionPair(Path.GetFileName(tempFile2), tempFile2));

				AssertNull(emailContactObject.PreviewFilePath);
				using (emailContactObject.SetupPreviewFile())
				{
					Assert("Should be generated as a html file", emailContactObject.PreviewFilePath.EndsWith("html"));
					Assert("File should exist", File.Exists(emailContactObject.PreviewFilePath));
					AssertEquals("<B> IN BOLD </b> <a href='http://www.cargowise.com'> hyperlink </A> <br/> this is a html based email", File.ReadAllText(emailContactObject.PreviewFilePath));

					string tempDir = Path.GetDirectoryName(emailContactObject.PreviewFilePath);
					string attachment1 = Path.Combine(tempDir, Path.GetFileName(tempFile1));
					string attachment2 = Path.Combine(tempDir, Path.GetFileName(tempFile2));
					AssertEquals("meh meh Meh", File.ReadAllText(attachment1));
					AssertEquals("second file", File.ReadAllText(attachment2));
				}
			}
			finally
			{
				File.Delete(tempFile1);
				File.Delete(tempFile2);
			}
		}

		public void TestPreviewFileHasUtf8Bom()
		{
			var emailContactObject = new EmailWithAttachmentForTest(EmailContentTypes.HTML);
			emailContactObject.Body = "<h1>привет</h1>";

			using (emailContactObject.SetupPreviewFile())
			{
				Assert("Should be generated as a html file", emailContactObject.PreviewFilePath.EndsWith("html"));
				Assert("File should exist", File.Exists(emailContactObject.PreviewFilePath));
				var bom = new byte[3];
				Array.Copy(File.ReadAllBytes(emailContactObject.PreviewFilePath), bom, 3);
				AssertArrayEqualsByElements("File should have UTF-8 BOM so that the preview window can correctly detect the file as UTF-8", new byte[] { 0xEF, 0xBB, 0xBF }, bom);
				AssertEquals("<h1>привет</h1>", File.ReadAllText(emailContactObject.PreviewFilePath, new UTF8Encoding(true)));
			}
		}
	}
}
