using System;
using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class EmailWithAttachmentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateAll()
		{
			using (email.GetValidationSuspender())
			{
				email.FromEmailAddress = "!@#";
				email.ToEmailAddress = "!@#";
				email.Cc = "!@#";
				email.Attachment = "!@#";

				AssertNoErrors("Validation suspended, should not have errors", email.FromEmailAddressInfo);
				AssertNoErrors("Validation suspended, should not have errors", email.ToEmailAddressInfo);
				AssertNoErrors("Validation suspended, should not have errors", email.CcInfo);
				AssertNoErrors("Validation suspended, should not have errors", email.AttachmentInfo);
			}
			email.Validation.ValidateAll();

			AssertHasErrors("Validate All called, should have errors", email.FromEmailAddressInfo);
			AssertHasErrors("Validate All called, should have errors", email.ToEmailAddressInfo);
			AssertHasErrors("Validate All called, should have errors", email.CcInfo);
			AssertHasErrors("Validate All called, should have errors", email.AttachmentInfo);
		}

		public void TestValidateFromDisplayName()
		{
			using (Env.Registry.RawRegistry.MailboxDisplayName.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "Jerry Company Test"))
			{
				GlbStaff.CurrentUser.GS_FullName = "Jerry Test Staff";

				var email = new EmailWithAttachment("test@test.com", "Jerry Test");
				email.ShouldValidateFrom = true;

				email.FromDisplayName = "Jerry Test";
				AssertNoErrors(email.FromDisplayNameInfo);

				email.FromDisplayName = "Jerry Test Staff";
				AssertNoErrors(email.FromDisplayNameInfo);

				email.FromDisplayName = "Jerry Company Test";
				AssertNoErrors(email.FromDisplayNameInfo);

				email.FromDisplayName = "Other Name";
				AssertHasError(email.FromDisplayNameInfo, "Enter a valid selection.");
			}
		}

		public void TestValidateFromEmailAddress()
		{
			var typeList = new CodeDescriptionPairList();
			typeList.AddPair("IMP", "Import");

			using (SystemDataRegistry.Instance.StaffEmailTypeList.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, typeList))
			{
				var emailAddress = GlbStaff.CurrentUser.EmailAddresses.AddNew();
				emailAddress.GSE_GC_Company = GlbCompany.CurrentCompany.PK;
				emailAddress.GSE_EmailAddress = "import@test.com";
				emailAddress.GSE_Type = "IMP";

				var email = new EmailWithAttachment("test@test.com", "Jerry Test");
				email.ShouldValidateFrom = true;

				email.FromEmailAddress = "!@#";
				AssertHasError(email.FromEmailAddressInfo, "Enter a valid selection.");

				email.FromEmailAddress = "test@test.com;TESt@test.com";
				AssertHasError(email.FromEmailAddressInfo, "Enter a valid selection.");

				email.FromEmailAddress = "test@test.com";
				AssertNoErrors(email.FromEmailAddressInfo);

				email.FromEmailAddress = "";
				AssertHasError(email.FromEmailAddressInfo, "Please enter a value.");

				email.FromEmailAddress = "test@test.com;;TESt@test.com";
				AssertHasError(email.FromEmailAddressInfo, "Enter a valid selection.");

				GlbStaff.CurrentUser.GS_EmailAddress = "main@test.com";

				email = new EmailWithAttachment("!@#", "Jerry Test");
				email.ShouldValidateFrom = true;
				email.FromEmailAddress = "!@#";
				AssertHasError(email.FromEmailAddressInfo, @"The email address ""!@#"" is invalid.");

				email.FromEmailAddress = "main@test.com";
				AssertNoErrors(email.FromEmailAddressInfo);

				email.FromEmailAddress = "import@test.com";
				AssertNoErrors(email.FromEmailAddressInfo);
			}
		}

		public void TestValidateTo()
		{
			email.ToEmailAddress = "";
			AssertMandatoryValidationError(email.ToEmailAddressInfo, true);

			email.ToEmailAddress = "!@#";
			AssertHasError(email.ToEmailAddressInfo, @"The email address ""!@#"" is invalid.");

			email.ToEmailAddress = "test@test.com";
			AssertNoErrors(email.ToEmailAddressInfo);

			email.ToEmailAddress = "";
			AssertMandatoryValidationError(email.ToEmailAddressInfo, true);

			email.ToEmailAddress = "Test@test.com;test@test2.cvom@.com";
			AssertHasError(email.ToEmailAddressInfo, @"The email address ""test@test2.cvom@.com"" is invalid.");

			email.ToEmailAddress = "Test@test.com;test@test2.cvom";
			AssertNoErrors(email.ToEmailAddressInfo);

			email.ToEmailAddress = "Test@test.com;;test@test2.cvom";
			AssertNoErrors(email.ToEmailAddressInfo);

			var emailAddress = GlbEmailAddress.LoadOrNew(Factory, "Test@test.com");
			emailAddress.GI_DeliveryStatus = EmailDeliveryReportStatus.Codes.NonDeliveryReport;
			emailAddress.GI_DeliveryReportTimeUtc = new ZDateTime(2002, 2, 2);
			Factory.Save();

			email.Validation.ValidateToEmailAddress();
			AssertHasWarning(email.ToEmailAddressInfo, @"The last email sent to ""Test@test.com"" received a Non-Delivery Receipt (at 02-Feb-02 00:00:00).");
		}

		public void TestValidateCc()
		{
			AssertNoErrors("Precondition: Cc should not have errors.", email.CcInfo);

			email.Cc = "!@#";
			AssertHasError(email.CcInfo, @"The email address ""!@#"" is invalid.");

			email.Cc = "test@test.com";
			AssertNoErrors(email.CcInfo);

			email.Cc = "";
			AssertNoErrors(email.CcInfo);

			email.Cc = "Test@test.com;test@test2.cvom@.com";
			AssertHasError(email.CcInfo, @"The email address ""test@test2.cvom@.com"" is invalid.");

			email.Cc = "Test@test.com;test@test2.cvom";
			AssertNoErrors(email.CcInfo);

			email.Cc = "Test@test.com;;test@test2.cvom";
			AssertNoErrors(email.CcInfo);

			var emailAddress = GlbEmailAddress.LoadOrNew(Factory, "Test@test.com");
			emailAddress.GI_DeliveryStatus = EmailDeliveryReportStatus.Codes.NonDeliveryReport;
			emailAddress.GI_DeliveryReportTimeUtc = new ZDateTime(2002, 2, 2);
			Factory.Save();

			email.Validation.ValidateCc();
			AssertHasWarning(email.CcInfo, @"The last email sent to ""Test@test.com"" received a Non-Delivery Receipt (at 02-Feb-02 00:00:00).");
		}

		public void TestValidateAttachment()
		{
			string tempFile = Temp.GetTempFileName();

			using (FileStream fileStream = File.OpenWrite(tempFile))
			{
				fileStream.Write(new byte[1], 0, 1);
			}

			try
			{
				AssertNoErrors("Precondition: Attachment should not have errors.", email.AttachmentInfo);

				email.Attachment = "!@#";
				AssertHasError(email.AttachmentInfo, "Enter a valid selection.");

				email.AttachmentList.AddPair("Attachment", @"X:\Attachment");
				email.Attachment = "Attachment";
				AssertHasError(email.AttachmentInfo, @"The file X:\Attachment does not exist.");

				email.AttachmentList.Clear();
				email.AddAttachment(tempFile);
				email.Attachment = Path.GetFileName(tempFile);
				AssertNoErrors(email.AttachmentInfo);

				email.Attachment = "";
				AssertNoErrors(email.AttachmentInfo);
			}
			finally
			{
				File.Delete(tempFile);
			}
		}

		public void TestMaxSizeOfAttachments()
		{
			string tempFile1 = Temp.GetTempFileName();
			string tempFile2 = Temp.GetTempFileName();
			string tempFile3 = Temp.GetTempFileName();
			int fileLen1 = (5 * 1024 + 1) * 1024 - 1;
			int fileLen2 = 1;
			int fileLen3 = 15 * 1024 * 1024;

			using (FileStream fileStream = File.OpenWrite(tempFile1))
			{
				fileStream.Write(new byte[fileLen1], 0, fileLen1);
			}

			using (FileStream fileStream = File.OpenWrite(tempFile2))
			{
				fileStream.Write(new byte[fileLen2], 0, fileLen2);
			}

			using (FileStream fileStream = File.OpenWrite(tempFile3))
			{
				fileStream.Write(new byte[fileLen3], 0, fileLen3);
			}

			try
			{
				SystemDataRegistry.Instance.EmailAttachmentSizeLimitInMB.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 5);

				AssertNoErrors("Precondition: Attachment should not have errors.", email.AttachmentInfo);

				email.AddAttachment(tempFile1);
				AssertNoErrors(email.AttachmentInfo);

				email.AddAttachment(tempFile2);
				AssertHasError(email.AttachmentInfo, "The total attachment size is currently 5121KB but the maximum is 5120KB. Please reduce the size or number of attachments.");

				SystemDataRegistry.Instance.EmailAttachmentSizeLimitInMB.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 20);

				email.Validation.ValidateAttachment();
				AssertNoErrors(email.AttachmentInfo);

				email.AddAttachment(tempFile3);
				AssertHasError(email.AttachmentInfo, "The total attachment size is currently 20481KB but the maximum is 20480KB. Please reduce the size or number of attachments.");

				email.RemoveAttachment();
				AssertNoErrors(email.AttachmentInfo);
			}
			finally
			{
				File.Delete(tempFile1);
				File.Delete(tempFile2);
				File.Delete(tempFile3);
			}
		}

		public void TestMaxTotalAttachmentSizeInKB()
		{
			//999 999 999 MB when converted to Bytes will overflow int.MaxValue. This makes sure we cater for the max value this registry supports
			using (SystemDataRegistry.Instance.EmailAttachmentSizeLimitInMB.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 999_999_999))
			{
				AssertEquals(999_999_999L * 1024, email.Validation.MaxTotalAttachmentSizeInKB);
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			email = new EmailWithAttachment();
		}
		EmailWithAttachment email;

		#endregion
	}
}
