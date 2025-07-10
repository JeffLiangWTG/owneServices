using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class BIRDFileOutputHelperTest : TestCaseWithFactory
	{
		public void TestBuildAMessageAndWriteToAFileWhenUsersOK()
		{
			var declaration = new DeclarationTestHelper().GetMergedDutiableDeclaration(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.SetExternalBrokerForTesting();
			new BIRDFileOutputHelper().BuildAMessage(declaration, new BuildMessage(new BIRDEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry).PopulateMessage), "7501");
			AssertEquals("One message should have been created", 1, declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages.Count);
			var message = declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages[0];
			AssertEquals(ApplicationIdentifierCodeList.Codes.BIRDTransaction, message.EM_MessageType);
			AssertEquals(Enterprise.Customs.US.Business.EM_MessageSubTypeList.Codes.BIRDEntrySummary, message.EM_MessageSubType);
			AssertEquals(MQEDIMessage.Status.Pending, message.EM_Status);
			//Messages should be saved before it is written to a file
			AssertNotContains(MQEDIMessage.USEntryNumberPlaceHolder, message.EM_MessageText);
		}

		public void TestBuildAMessageAndWriteToAFileWhenUsersCancel()
		{
			JobDeclaration declaration = new DeclarationTestHelper().GetMergedDutiableDeclaration(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			new BIRDFileOutputHelper().BuildAMessage(declaration, new BuildMessage(new BIRDEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry).PopulateMessage), "7501");
			AssertEquals(typeof(SaveFileDialog), ZFormModaliser.LastCommonDialogShownDialogForTest.GetType());
			AssertEquals("No message should have been created", 0, declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages.Count);
		}

		public void TestBuildAMessageAndWriteToAFileWhenUsersOKWhenNoExternalBroker()
		{
			using (var tempFile = TempFile.New())
			{
				var declaration = new DeclarationTestHelper().GetMergedDutiableDeclaration(Factory);
				declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
				Assert("PreCondition", !declaration.HasBIRDCommunicationMode());
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.FileNameToSelectInShowCommonDialog = tempFile.Filename;
				new BIRDFileOutputHelper().BuildAMessage(declaration, new BuildMessage(new BIRDEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry).PopulateMessage), "7501");
				AssertEquals("One message should have been created", 1, declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages.Count);
				var message = declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages[0];
				AssertEquals(ApplicationIdentifierCodeList.Codes.BIRDTransaction, message.EM_MessageType);
				AssertEquals(Enterprise.Customs.US.Business.EM_MessageSubTypeList.Codes.BIRDEntrySummary, message.EM_MessageSubType);
				AssertEquals(MQEDIMessage.Status.Acknowledged, message.EM_Status);
				//Messages should be saved before it is written to a file
				AssertNotContains(MQEDIMessage.USEntryNumberPlaceHolder, message.EM_MessageText);
			}
		}
	}
}
