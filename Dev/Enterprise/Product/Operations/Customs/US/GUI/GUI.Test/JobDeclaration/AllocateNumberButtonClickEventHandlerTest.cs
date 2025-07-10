using System;
using System.Data;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class AllocateNumberButtonClickEventHandlerTest : TestCaseWithFactory
	{
		public void TestJobDeclarationAllocate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK; //for allocate form
				form.FindSingle<ZButton>("AllocateImportEntryNumberButton").PerformClick();
				AssertNotEquals("Entry Number is allocated, and yet it was " + declaration.ImportEntryNumber, ZString.Empty, declaration.ImportEntryNumber);
				var entryNumberAllocated = declaration.ImportEntryNumber;
				var factory2 = new BusinessObjectFactory();
				var declarationLoaded = factory2.Load<JobDeclaration>(declaration.PK);
				AssertEquals(entryNumberAllocated, declaration.ImportEntryNumber);
			}
		}

		public void TestJobDeclarationAllocateWhenStopped()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			Factory.Save();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var factory2 = new BusinessObjectFactory();
				factory2.RefreshEnabled = false;
				var declarationInFactory2 = factory2.Load<JobDeclaration>(declaration.PK);
				AssertEquals(true, declarationInFactory2.LockImportEntryNumberAllocationMutex);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.FindSingle<ZButton>("AllocateImportEntryNumberButton").PerformClick();
				AssertEquals("Entry Number is not allocated", "", declaration.ImportEntryNumber);
				AssertEquals(declaration.GetImportEntryNumberAllocationMutexLockInfo() + " is in the process of allocating Entry Number for this job.\r\nPlease re-open the job later.", UnitTestUserNotification.Instance.LastMessage.Text);
				declarationInFactory2.Invoices.AddNew();
				declarationInFactory2.InvoiceLines.AddNew();
				declarationInFactory2.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				declarationInFactory2.ActiveEntryHeaders.EntrySummaryEntry.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal;
				factory2.Save();
				var entryNumber = declarationInFactory2.ImportEntryNumber;
				var warningMessage = string.Format("There is Entry Number ({0}) already allocated for this job. Are you sure you wish to continue?", entryNumber);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.FindSingle<ZButton>("AllocateImportEntryNumberButton").PerformClick();
				AssertEquals("Entry Number is not allocated", entryNumber, declaration.ImportEntryNumber);
				AssertEquals(warningMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestReconDeclarationAllocateNoExceptionThrownWhenAddInfoPropertiesNotInRecon()
		{
			var declaration = Factory.New<JobDeclaration>();
			var reconDec = new ReconDeclaration(declaration);
			declaration.US_EntryFilerCode = "XJ5";
			Factory.Save();

			UpdateDeclarationAdditionalInfo(declaration.PK.ToGuid(), "EntryFilerCode=XJ5*ImportEntrySource=1*BondType=8*BondProducerAccNo=990900083");

			declaration.US_Comment = "Test";
			using (var form = new ReconDeclarationForm(reconDec))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AssertNoExceptionThrown(() => form.FindSingle<ZButton>("AllocateButton").PerformClick());
			}

			void UpdateDeclarationAdditionalInfo(Guid declarationPK, string additionalInfo)
			{
				var updateSQL = @"
UPDATE dbo.JobDeclaration
SET
	JE_AddInfo = @additionalInfo,
	JE_SystemLastEditTimeUtc = GETUTCDATE(),
	JE_SystemLastEditUser = '~BP'
WHERE
	JE_PK = @declarationPK";

				using (var command = Db.Connection.Command(updateSQL))
				{
					command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
					command.AddParameter("@additionalInfo", SqlDbType.VarChar, additionalInfo);
					command.ExecuteNonQuery();
				}
			}
		}
	}
}
