using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestsSubclassesOf(typeof(EmailWithAttachment), ExcludePrivate = true)]
	public abstract class EmailWithAttachmentTestCase<T> : NonPersistentBusinessObjectTestCase where T : EmailWithAttachment
	{
		public void TestRecipientsAncCcRecipients()
		{
			T emailWithAttachment = GetEmailWithAttachment();
			AssertEquals("ToEmailAddressInfo.MaxLength", 8000, emailWithAttachment.ToEmailAddressInfo.MaxLength);
			emailWithAttachment.ToEmailAddress = "1@1.com";
			emailWithAttachment.Cc = "3@3.com";

			AssertEquals("Recipients.Length", 1, emailWithAttachment.Recipients.Length);
			AssertEquals("Recipients[0]", "1@1.com", emailWithAttachment.Recipients[0]);

			AssertEquals("CcRecipients.Length", 1, emailWithAttachment.CcRecipients.Length);
			AssertEquals("CcRecipients[0]", "3@3.com", emailWithAttachment.CcRecipients[0]);

			emailWithAttachment.ToEmailAddress = "1@1.com;2@2.com";
			emailWithAttachment.Cc = "3@3.com;4@4.com";

			AssertEquals("Recipients.Length", 2, emailWithAttachment.Recipients.Length);
			AssertEquals("Recipients[0]", "1@1.com", emailWithAttachment.Recipients[0]);
			AssertEquals("Recipients[1]", "2@2.com", emailWithAttachment.Recipients[1]);

			AssertEquals("CcRecipients.Length", 2, emailWithAttachment.CcRecipients.Length);
			AssertEquals("CcRecipients[0]", "3@3.com", emailWithAttachment.CcRecipients[0]);
			AssertEquals("CcRecipients[1]", "4@4.com", emailWithAttachment.CcRecipients[1]);

			emailWithAttachment.ToEmailAddress = "1@1.com;2@2.com;";
			emailWithAttachment.Cc = "3@3.com;4@4.com;";

			AssertEquals("Recipients.Length", 2, emailWithAttachment.Recipients.Length);
			AssertEquals("Recipients[0]", "1@1.com", emailWithAttachment.Recipients[0]);
			AssertEquals("Recipients[1]", "2@2.com", emailWithAttachment.Recipients[1]);

			AssertEquals("CcRecipients.Length", 2, emailWithAttachment.CcRecipients.Length);
			AssertEquals("CcRecipients[0]", "3@3.com", emailWithAttachment.CcRecipients[0]);
			AssertEquals("CcRecipients[1]", "4@4.com", emailWithAttachment.CcRecipients[1]);

			emailWithAttachment.ToEmailAddress = "";
			emailWithAttachment.Cc = "";

			AssertEquals("Recipients.Length", 0, emailWithAttachment.Recipients.Length);
			AssertEquals("CcRecipients.Length", 0, emailWithAttachment.CcRecipients.Length);

			emailWithAttachment.ToEmailAddress = "test@test.com;;Test@Test.com";
			emailWithAttachment.Cc = "test@test.com;;Test@Test.com";

			AssertEquals("Recipients.Length", 2, emailWithAttachment.Recipients.Length);
			AssertEquals("CcRecipients.Length", 2, emailWithAttachment.CcRecipients.Length);
		}

		public virtual void TestAllRecipientsCommaDelimited()
		{
			T emailWithAttachment = GetEmailWithAttachment();
			emailWithAttachment.ToEmailAddress = "";
			AssertEquals("AllRecipientsCommaDelimited", "", emailWithAttachment.AllRecipientsCommaDelimited);
			emailWithAttachment.ToEmailAddress = "a@a.com;b@b.com;";
			AssertEquals("AllRecipientsCommaDelimited", "a@a.com, b@b.com", emailWithAttachment.AllRecipientsCommaDelimited);
			emailWithAttachment.Cc = "c@c.com;d@d.com;";
			AssertEquals("AllRecipientsCommaDelimited", "a@a.com, b@b.com, c@c.com, d@d.com", emailWithAttachment.AllRecipientsCommaDelimited);
			emailWithAttachment.ToEmailAddress = "";
			AssertEquals("AllRecipientsCommaDelimited", "c@c.com, d@d.com", emailWithAttachment.AllRecipientsCommaDelimited);
		}

		#region Validation

		public void TestValidation()
		{
			AssertNotNull("Validation object should not be null", GetEmailWithAttachment().Validation);
		}

		#endregion

		#region Bound Properties

		public void TestBodyMaxLength()
		{
			AssertEquals("BodyInfo.MaxLength", -1, GetEmailWithAttachment().BodyInfo.MaxLength);
		}

		public void TestCcMaxLength()
		{
			AssertEquals("CcInfo.MaxLength", 512, GetEmailWithAttachment().CcInfo.MaxLength);
		}

		public void TestDisplayNames()
		{
			T emailWithAttachment = GetEmailWithAttachment();
			emailWithAttachment.ToDisplayName = "Bob";
			emailWithAttachment.FromDisplayName = "Sandra";

			AssertEquals("To display name should be", "Bob", emailWithAttachment.ToDisplayName);
			AssertEquals("From display name should be", "Sandra", emailWithAttachment.FromDisplayName);
			AssertEquals("ToDisplayNameInfo.MaxLength", 8000, emailWithAttachment.ToDisplayNameInfo.MaxLength);
		}

		public void TestPriorityList()
		{
			T emailWithAttachment = GetEmailWithAttachment();
			AssertEquals("PriorityList.Count", 3, emailWithAttachment.PriorityList.Count);
			AssertEquals("PriorityList.GetDescriptionFromCode(\"HI\")", "High", emailWithAttachment.PriorityList.GetDescriptionFromCode("HI"));
			AssertEquals("PriorityList.GetDescriptionFromCode(\"MED\")", "Medium", emailWithAttachment.PriorityList.GetDescriptionFromCode("MED"));
			AssertEquals("PriorityList.GetDescriptionFromCode(\"LOW\")", "Low", emailWithAttachment.PriorityList.GetDescriptionFromCode("LOW"));
		}

		public void TestSetEmailAddressesTrimsValue()
		{
			T emailWithAttachment = GetEmailWithAttachment();
			emailWithAttachment.FromEmailAddress = " test@test.com ";
			emailWithAttachment.ToEmailAddress = " test@test.com; test@test.com ";
			emailWithAttachment.Cc = " test@test.com; test@test.com ";

			AssertEquals("FromEmailAddress", "test@test.com", emailWithAttachment.FromEmailAddress);
			AssertEquals("To", "test@test.com;test@test.com", emailWithAttachment.ToEmailAddress);
			AssertEquals("Cc", "test@test.com;test@test.com", emailWithAttachment.Cc);

			emailWithAttachment.FromEmailAddress = "test@test.com";
			emailWithAttachment.ToEmailAddress = "test@test.com;test@test.com";
			emailWithAttachment.Cc = "test@test.com;test@test.com";

			AssertEquals("FromEmailAddress", "test@test.com", emailWithAttachment.FromEmailAddress);
			AssertEquals("To", "test@test.com;test@test.com", emailWithAttachment.ToEmailAddress);
			AssertEquals("Cc", "test@test.com;test@test.com", emailWithAttachment.Cc);

			emailWithAttachment.FromEmailAddress = " test@test.com";
			emailWithAttachment.ToEmailAddress = "test@test.com ";
			emailWithAttachment.Cc = " test@test.com;";

			AssertEquals("FromEmailAddress", "test@test.com", emailWithAttachment.FromEmailAddress);
			AssertEquals("To", "test@test.com", emailWithAttachment.ToEmailAddress);
			AssertEquals("Cc", "test@test.com;", emailWithAttachment.Cc);

			emailWithAttachment.FromEmailAddress = " test@te st.com";
			emailWithAttachment.ToEmailAddress = "tes t@test.com ";
			emailWithAttachment.Cc = " test@test.com ;";

			AssertEquals("FromEmailAddress", "test@test.com", emailWithAttachment.FromEmailAddress);
			AssertEquals("To", "test@test.com", emailWithAttachment.ToEmailAddress);
			AssertEquals("Cc", "test@test.com;", emailWithAttachment.Cc);
		}

		#endregion

		#region Add/Remove Attachments

		public void TestAddAndRemoveAttachments()
		{
			T emailWithAttachment = GetEmailWithAttachment();
			string tempFile1 = Temp.GetTempFileName();
			string tempFile2 = Temp.GetTempFileName();
			string tempFile3 = Temp.GetTempFileName();

			string code1 = Path.GetFileName(tempFile1);
			string code2 = Path.GetFileName(tempFile2);
			string code3 = Path.GetFileName(tempFile3);

			string description1 = tempFile1 + " [Size: 1KB]";
			string description2 = tempFile2 + " [Size: 2KB]";
			string description3 = tempFile3 + " [Size: 3KB]";

			using (FileStream fileStream = File.OpenWrite(tempFile1))
			{
				fileStream.Write(new byte[1024], 0, 1024);
			}

			using (FileStream fileStream = File.OpenWrite(tempFile2))
			{
				fileStream.Write(new byte[2048], 0, 2048);
			}

			using (FileStream fileStream = File.OpenWrite(tempFile3))
			{
				fileStream.Write(new byte[3072], 0, 3072);
			}

			try
			{
				AssertEquals("Precondition: Attachment should be empty.", "", emailWithAttachment.Attachment);
				AssertEquals("Precondition: AttachmentList should be empty.", 0, emailWithAttachment.AttachmentList.Count);
				AssertEquals("NumberOfAttachmentsMessage", "There are 0 attachments.", emailWithAttachment.NumberOfAttachmentsMessage);

				emailWithAttachment.AddAttachment(@"X:\Attachment1", "", @"X:\Attachment2");
				AssertEquals("AttachmentList.Count", 0, emailWithAttachment.AttachmentList.Count);

				emailWithAttachment.AddAttachment(tempFile1);
				AssertEquals("Attachment", code1, emailWithAttachment.Attachment);
				AssertEquals("AttachmentList.Count", 1, emailWithAttachment.AttachmentList.Count);
				AssertEquals("AttachmentList[0].Code", code1, emailWithAttachment.AttachmentList[0].Code);
				AssertEquals("AttachmentList[0].Description", tempFile1 + " [Size: 1KB]", emailWithAttachment.AttachmentList[0].Description);
				AssertEquals("NumberOfAttachmentsMessage", "There is 1 attachment.", emailWithAttachment.NumberOfAttachmentsMessage);

				emailWithAttachment.AddAttachment(tempFile1);
				AssertEquals("Attachment", code1, emailWithAttachment.Attachment);
				AssertEquals("AttachmentList.Count", 1, emailWithAttachment.AttachmentList.Count);
				AssertEquals("AttachmentList[0].Code", code1, emailWithAttachment.AttachmentList[0].Code);
				AssertEquals("AttachmentList[0].Description", description1, emailWithAttachment.AttachmentList[0].Description);
				AssertEquals("NumberOfAttachmentsMessage", "There is 1 attachment.", emailWithAttachment.NumberOfAttachmentsMessage);

				emailWithAttachment.AddAttachment(tempFile2, tempFile3);
				AssertEquals("Attachment", code3, emailWithAttachment.Attachment);
				AssertEquals("AttachmentList.Count", 3, emailWithAttachment.AttachmentList.Count);
				AssertEquals("AttachmentList[0].Code", code1, emailWithAttachment.AttachmentList[0].Code);
				AssertEquals("AttachmentList[0].Description", description1, emailWithAttachment.AttachmentList[0].Description);
				AssertEquals("AttachmentList[1].Code", code2, emailWithAttachment.AttachmentList[1].Code);
				AssertEquals("AttachmentList[1].Description", description2, emailWithAttachment.AttachmentList[1].Description);
				AssertEquals("AttachmentList[2].Code", code3, emailWithAttachment.AttachmentList[2].Code);
				AssertEquals("AttachmentList[2].Description", description3, emailWithAttachment.AttachmentList[2].Description);
				AssertEquals("NumberOfAttachmentsMessage", "There are 3 attachments.", emailWithAttachment.NumberOfAttachmentsMessage);

				emailWithAttachment.RemoveAttachment();
				AssertEquals("Attachment", code2, emailWithAttachment.Attachment);
				AssertEquals("AttachmentList.Count", 2, emailWithAttachment.AttachmentList.Count);
				AssertEquals("AttachmentList[0].Code", code1, emailWithAttachment.AttachmentList[0].Code);
				AssertEquals("AttachmentList[0].Description", description1, emailWithAttachment.AttachmentList[0].Description);
				AssertEquals("AttachmentList[1].Code", code2, emailWithAttachment.AttachmentList[1].Code);
				AssertEquals("AttachmentList[1].Description", description2, emailWithAttachment.AttachmentList[1].Description);
				AssertEquals("NumberOfAttachmentsMessage", "There are 2 attachments.", emailWithAttachment.NumberOfAttachmentsMessage);

				emailWithAttachment.Attachment = code1;
				emailWithAttachment.RemoveAttachment();
				AssertEquals("Attachment", code2, emailWithAttachment.Attachment);
				AssertEquals("AttachmentList.Count", 1, emailWithAttachment.AttachmentList.Count);
				AssertEquals("AttachmentList[0].Code", code2, emailWithAttachment.AttachmentList[0].Code);
				AssertEquals("AttachmentList[0].Description", description2, emailWithAttachment.AttachmentList[0].Description);
				AssertEquals("NumberOfAttachmentsMessage", "There is 1 attachment.", emailWithAttachment.NumberOfAttachmentsMessage);

				emailWithAttachment.RemoveAttachment();
				AssertEquals("Attachment", "", emailWithAttachment.Attachment);
				AssertEquals("AttachmentList.Count", 0, emailWithAttachment.AttachmentList.Count);
				AssertEquals("NumberOfAttachmentsMessage", "There are 0 attachments.", emailWithAttachment.NumberOfAttachmentsMessage);
			}
			finally
			{
				File.Delete(tempFile1);
				File.Delete(tempFile2);
				File.Delete(tempFile3);
			}
		}

		public void TestAttachmentReadOnlyWhenListIsEmpty()
		{
			T emailWithAttachment = GetEmailWithAttachment();
			AssertEquals("AttachmentList.Count", 0, emailWithAttachment.AttachmentList.Count);
			AssertEquals("AttachmentInfo.ReadOnly", true, emailWithAttachment.AttachmentInfo.ReadOnly);

			emailWithAttachment.AttachmentList.AddPair("Attachment", "");
			AssertEquals("AttachmentInfo.ReadOnly", false, emailWithAttachment.AttachmentInfo.ReadOnly);

			emailWithAttachment.AttachmentList.RemoveCode("Attachment");
			AssertEquals("AttachmentInfo.ReadOnly", true, emailWithAttachment.AttachmentInfo.ReadOnly);
		}

		public void TestGetFileNameFromAttachmentListDescription()
		{
			T emailWithAttachment = GetEmailWithAttachment();
			AssertEquals("File name should be ", "Blah Blah", emailWithAttachment.GetFileNameFromAttachmentListDescription("Blah Blah [Size: 1KB]"));
			AssertEquals("File name should be ", "Size not found", emailWithAttachment.GetFileNameFromAttachmentListDescription("Size not found"));
		}

		#endregion

		public virtual void TestCheckIsReadyToSendEmail()
		{
			T emailWithAttachment = GetEmailWithAttachment();
			emailWithAttachment.Attachment = "!@#";
			emailWithAttachment.Cc = "!@#";
			emailWithAttachment.FromEmailAddress = "!@#";
			emailWithAttachment.Priority = "!@#";
			emailWithAttachment.ToEmailAddress = "!@#";

			AssertEquals("IsReadyToSendEmail", false, emailWithAttachment.CheckIsReadyToSendEmail());
			AssertHasErrors(emailWithAttachment.AttachmentInfo);
			AssertHasErrors(emailWithAttachment.CcInfo);
			AssertHasErrors(emailWithAttachment.FromEmailAddressInfo);
			AssertHasErrors(emailWithAttachment.PriorityInfo);
			AssertHasErrors(emailWithAttachment.ToEmailAddressInfo);

			emailWithAttachment.Attachment = "";
			emailWithAttachment.Cc = "cc@cc.com";
			emailWithAttachment.FromEmailAddress = "from@from.com";
			emailWithAttachment.Priority = "MED";
			emailWithAttachment.ToEmailAddress = "to@to.com";

			AssertEquals("IsReadyToSendEmail", true, emailWithAttachment.CheckIsReadyToSendEmail());
			AssertNoErrors(emailWithAttachment);
		}

		#region Default Email Address / Display Name

		public virtual void TestSetupDefaultFromAddressCore()
		{
			Env.Registry.AllowEmailsToBeSentFromUsersAddress = false;
			T emailWithAttachment = GetEmailWithAttachment();
			AssertEquals("UseCurrentUsersNameAndTitle", true, emailWithAttachment.UseCurrentUsersNameAndTitle);
			AssertEquals("UseCurrentUsersEmailAddress", false, emailWithAttachment.UseCurrentUsersEmailAddress);
			AssertEquals("FromDisplayName", GlbStaff.CurrentUser.GS_FullName, emailWithAttachment.FromDisplayName);
			AssertEquals("FromEmailAddress", DefaultFromEmailAddress, emailWithAttachment.FromEmailAddress);
			AssertEquals("FromDisplayName readonly", true, emailWithAttachment.FromDisplayNameInfo.ReadOnly);
			AssertEquals("FromEmailAddress readonly", false, emailWithAttachment.FromEmailAddressInfo.ReadOnly);

			Env.Registry.AllowEmailsToBeSentFromUsersAddress = true;
			emailWithAttachment = (T)GetNewBusinessObject();
			AssertEquals("UseCurrentUsersEmailAddress", true, emailWithAttachment.UseCurrentUsersEmailAddress);
		}

		public void TestUseCurrentUsersNameAndTitle()
		{
			T emailWithAttachment = GetEmailWithAttachment();
			emailWithAttachment.UseCurrentUsersNameAndTitle = true;
			AssertEquals("If UseCurrentUsersNameAndTitle in FromDisplayName we should use CurrentUser", GlbStaff.CurrentUser.GS_FullName, emailWithAttachment.FromDisplayName);
			Assert("If UseCurrentUsersNameAndTitle FromDisplayName should be readonly", emailWithAttachment.FromDisplayNameInfo.ReadOnly);

			emailWithAttachment.UseCurrentUsersNameAndTitle = false;
			AssertEquals("If UseCurrentUsersNameAndTitle = fasle in FromDisplayName we should use DefaultFromDisplayName", DefaultFromDisplayName, emailWithAttachment.FromDisplayName);
			Assert("If UseCurrentUsersNameAndTitle = fasle FromDisplayName should not be readonly", !emailWithAttachment.FromDisplayNameInfo.ReadOnly);

			emailWithAttachment.FromDisplayName = "Ted Burhan";
			AssertEquals("Ted Burhan", emailWithAttachment.FromDisplayName);
			Assert("If UseCurrentUsersNameAndTitle = fasle FromDisplayName should not be readonly", !emailWithAttachment.FromDisplayNameInfo.ReadOnly);

			emailWithAttachment.UseCurrentUsersNameAndTitle = true;
			AssertEquals("If UseCurrentUsersNameAndTitle in FromDisplayName we should use CurrentUser", GlbStaff.CurrentUser.GS_FullName, emailWithAttachment.FromDisplayName);
			Assert("If UseCurrentUsersNameAndTitle FromDisplayName should be readonly", emailWithAttachment.FromDisplayNameInfo.ReadOnly);
		}

		public void TestUseCurrentUsersEmailAddress()
		{
			Env.Registry.AllowEmailsToBeSentFromUsersAddress = false;
			T emailWithAttachment = GetEmailWithAttachment();
			AssertEquals("Precondition: UseCurrentUsersEmailAddress", false, emailWithAttachment.UseCurrentUsersEmailAddress);
			AssertEquals("If UseCurrentUsersEmailAddress = false in FromEmailAdress we should use DefaultFromEmailAddress", DefaultFromEmailAddress, emailWithAttachment.FromEmailAddress);
			Assert("If UseCurrentUsersEmailAddress = false FromEmailAdress should be not readonly", !emailWithAttachment.FromEmailAddressInfo.ReadOnly);

			emailWithAttachment.FromEmailAddress = "ted.burhan@cargowise.com";
			AssertEquals("ted.burhan@cargowise.com", emailWithAttachment.FromEmailAddress);
			Assert("If UseCurrentUsersEmailAddress = false FromEmailAdress should not be readonly", !emailWithAttachment.FromEmailAddressInfo.ReadOnly);

			emailWithAttachment.UseCurrentUsersEmailAddress = true;
			AssertEquals("If UseCurrentUsersEmailAddress = true in FromEmailAdress we should use current user email", GlbStaff.CurrentUser.GS_EmailAddress, emailWithAttachment.FromEmailAddress);
			Assert("If UseCurrentUsersEmailAddress = true FromDisplayName should be readonly", emailWithAttachment.FromEmailAddressInfo.ReadOnly);

			emailWithAttachment.UseCurrentUsersEmailAddress = false;
			AssertEquals("If UseCurrentUsersEmailAddress = false in FromEmailAdress we should use DefaultFromEmailAddress", DefaultFromEmailAddress, emailWithAttachment.FromEmailAddress);
			Assert("If UseCurrentUsersEmailAddress = flase FromEmailAdress should be not readonly", !emailWithAttachment.FromEmailAddressInfo.ReadOnly);
		}

		protected abstract string DefaultFromEmailAddress { get; }
		protected abstract string DefaultFromDisplayName { get; }

		#endregion Default Email Address / Display Name

		#region Implementation

		protected virtual void PopulateEmailContactObject(T emailContactObject)
		{
			emailContactObject.FromDisplayName = "From Me";
			emailContactObject.FromEmailAddress = "From@Me.com";
			emailContactObject.ToEmailAddress = "1@1.com; 2@2.com";
			emailContactObject.Cc = "3@3.com;4@4.com";
			emailContactObject.Subject = "This is the subject.";
			emailContactObject.Body = "This is the body.";
		}

		protected abstract T GetEmailWithAttachment();

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetEmailWithAttachment();
		}

		protected override void SetUp()
		{
			base.SetUp();
			OriginalUserName = GlbStaff.CurrentUser.GS_FullName;
			OriginalUserEmail = GlbStaff.CurrentUser.GS_EmailAddress;
			GlbStaff.CurrentUser.GS_FullName = "Sergey Gordok";
			GlbStaff.CurrentUser.GS_EmailAddress = "sergey.gordok@cargowise.com";
		}
		string OriginalUserName;
		string OriginalUserEmail;

		protected void ResetCurrentUserSetup()
		{
			GlbStaff.CurrentUser.GS_FullName = OriginalUserName;
			GlbStaff.CurrentUser.GS_EmailAddress = OriginalUserEmail;
		}

		#endregion
	}
}
