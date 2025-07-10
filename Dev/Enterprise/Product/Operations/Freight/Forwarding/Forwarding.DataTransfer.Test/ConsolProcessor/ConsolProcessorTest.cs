using System;
using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class ConsolProcessorTest : TestCaseWithFactory
	{
		public void TestMessageFilter()
		{
			using (TempDirectory tempDir = new TempDirectory())
			{
				SystemDataRegistry.Instance.ConsolsDataImportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tempDir.DirectoryName);
				var helper = new MessageFilterTestHelper<ConsolProcessor, MailItem>();
				Assert(helper.Process(ImportMailItem));
				AssertEquals("Attachment should exist in the temp directory", true, File.Exists(Path.Combine(tempDir.DirectoryName, ImportMailItem.MailAttachments[0].MA_FileName)));
			}
		}

		public void TestMessageFilterWhenCantSaveAttachment()
		{
			string incorrectDirectory = @"u:\thisdircantexist\becauseivemadeitup\andshouldnoteverbecreated\andhas|@#%!/\illegal[chars]\\\";
			SystemDataRegistry.Instance.ConsolsDataImportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, incorrectDirectory);
			var helper = new MessageFilterTestHelper<ConsolProcessor, MailItem>();
			Assert(!helper.Process(ImportMailItem));
			AssertEquals(1, helper.Log.Count);
			Assert(helper.Log[0].StartsWith("Error|ediEnterprise Consol XML File Import - Fail to process email message"));
		}

		public void TestEmailSentToPostmastersWhenCantSaveAttachment()
		{
			GlbGroup group = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			group.Staff[0].GS_EmailAddress = "test@test.com";
			Factory.Save();

			string incorrectDirectory = @"u:\thisdircantexist\becauseivemadeitup\andshouldnoteverbecreated\andhas|@#%!/\illegal[chars]\\\";
			SystemDataRegistry.Instance.ConsolsDataImportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, incorrectDirectory);

			var helper = new MessageFilterTestHelper<ConsolProcessor, MailItem>();
			helper.Process(ImportMailItem);

			Predicate<EmailDef> emailMatch = (mailToMatch => mailToMatch.Subject.Contains(ConsolProcessor.ConsolSubject));
			EmailDef email = Env.OutgoingMailManager.EmailsCreated.Find(emailMatch);
			AssertNotNull("1 email should have been created", email);
			AssertEquals("Email should contain postmasters as recipients", true, email.Recipients.Contains("test@test.com"));
			Assert("Should have notifications", helper.Log.Count > 0);
		}

		public void TestAttachmentGetsWrittenToCorrectDirectory()
		{
			using (TempDirectory tempDir = new TempDirectory())
			{
				SystemDataRegistry.Instance.ConsolsDataImportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tempDir.DirectoryName);

				var helper = new MessageFilterTestHelper<ConsolProcessor, MailItem>();
				helper.Process(ImportMailItem);

				AssertEquals("Attachment should exist in the temp directory", true, File.Exists(Path.Combine(tempDir.DirectoryName, ImportMailItem.MailAttachments[0].MA_FileName)));
			}
		}

		#region Implementation

		MailItem ImportMailItem;

		protected override void SetUp()
		{
			base.SetUp();

			TestCaseHelper.ClearTable(MailDBItemsSchema.Constants.TableName);
			TestCaseHelper.ClearTable(JobConsolSchema.Constants.TableName);

			ImportMailItem = Factory.New<MailItem>();
			ImportMailItem.MI_Status = MailStatus.Queued;
			ImportMailItem.MI_Direction = MailDirection.Receive;
			ImportMailItem.MI_SendDateTime = ZDateTime.Now;
			ImportMailItem.MI_ReceivedDateTime = ZDateTime.Now;
			ImportMailItem.MI_Subject = ConsolProcessor.ConsolSubject;

			MailAttachment attachment = ImportMailItem.MailAttachments.AddNew();
			attachment.MA_FileName = "Consol_C00002217___MasterBill_08198393939.xml";

			EmbeddedResourceRetriever resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			string embeddedResourcePath = $"Enterprise.Freight.Forwarding.DataTransfer.Test.ConsolProcessor.TestFiles.Consol_C00002217___MasterBill_08198393939.xml";
			attachment.MA_Data = resourceRetriever.GetBytes(embeddedResourcePath);

			Factory.Save();
		}

		#endregion
	}
}
