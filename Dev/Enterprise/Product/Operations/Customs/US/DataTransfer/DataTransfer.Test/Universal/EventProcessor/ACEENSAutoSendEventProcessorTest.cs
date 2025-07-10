using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using JobMessageTypeList = Enterprise.Customs.US.Business.JobMessageTypeList;

namespace Enterprise.Customs.US.DataTransfer.Universal.Testing
{
	sealed class ACEENSAutoSendEventProcessorTest : TestCaseWithFactory
	{
		public void TestAutoSendEntrySummaryMessage()
		{
			CreateTestMessage();
			var declaration = CreateJobDeclarationForSendMessage();
			var nesEntryHeader = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var sendMessageAction = new AutoSendMessageCusAddInfo.Loader(declaration.Factory).Load(nesEntryHeader);
			if (sendMessageAction == null)
			{
				sendMessageAction = Factory.New<AutoSendMessageCusAddInfo>();
				sendMessageAction.B7_ParentID = nesEntryHeader.PK;
				sendMessageAction.B7_ParentTableCode = "CH";
			}

			sendMessageAction.B7_AddInfoData = "<EntryHeaderMessageSendingAction><US_AcknowledgeAndSign>Y</US_AcknowledgeAndSign><US_CertifyCargoRelease>Y</US_CertifyCargoRelease><US_CertifyTIB>N</US_CertifyTIB><US_CertifyTSCA>N</US_CertifyTSCA><US_DateOfDeclaration>2016-04-07T00:00:00</US_DateOfDeclaration><US_IsCustomsRequested>N</US_IsCustomsRequested><US_JobReadyForPosting>N</US_JobReadyForPosting><US_RequestBillOfLadingResult>N</US_RequestBillOfLadingResult><US_SendMessage>Y</US_SendMessage><US_SE_ContactName>CargoWiseOneSupport</US_SE_ContactName><US_SE_DISIndicator>N</US_SE_DISIndicator></EntryHeaderMessageSendingAction>";
			nesEntryHeader.Logs.AddNew(Events.Authorised, eBondLogger.eBondAutoSendMessageReference + ":ORG");
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			declaration.Reload();
			nesEntryHeader.Reload();
			nesEntryHeader.Messages.Reload(true);
			AssertEquals("Send a message.", 1, nesEntryHeader.Messages.Count);
			nesEntryHeader.Logs.GetAllLogs().Reload(true);
			AssertNull("ATH event was cancelled", eBondLogger.GetAutoSendEvent(nesEntryHeader.Logs));
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Recipients.Contains(declaration.CusAgent.GS_EmailAddress));
			Assert(email.Body.Contains("Entry Summary has been sent successfully"));
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var nesEntryHeader2 = newFactory.Load<CusEntryHeader>(nesEntryHeader.PK);
			AssertNull("Deleted", new AutoSendMessageCusAddInfo.Loader(newFactory).Load(nesEntryHeader2));
		}

		public void TestCreateDocPrintingDataForEntrySummary()
		{
			CreateTestMessage();
			var declaration = CreateJobDeclarationForSendMessage();
			var nesEntryHeader = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			nesEntryHeader.CH_Status = "CEO";
			var sendMessageAction = new AutoSendMessageCusAddInfo.Loader(declaration.Factory).Load(nesEntryHeader);
			if (sendMessageAction == null)
			{
				sendMessageAction = Factory.New<AutoSendMessageCusAddInfo>();
				sendMessageAction.B7_ParentID = nesEntryHeader.PK;
				sendMessageAction.B7_ParentTableCode = "CH";
			}

			sendMessageAction.B7_AddInfoData = "<EntryHeaderMessageSendingAction><US_AcknowledgeAndSign>Y</US_AcknowledgeAndSign><US_CertifyCargoRelease>Y</US_CertifyCargoRelease><US_CertifyTIB>N</US_CertifyTIB><US_CertifyTSCA>N</US_CertifyTSCA><US_DateOfDeclaration>2016-04-07T00:00:00</US_DateOfDeclaration><US_IsCustomsRequested>N</US_IsCustomsRequested><US_JobReadyForPosting>N</US_JobReadyForPosting><US_RequestBillOfLadingResult>N</US_RequestBillOfLadingResult><US_SendMessage>Y</US_SendMessage><US_SE_ContactName>CargoWiseOneSupport</US_SE_ContactName><US_SE_DISIndicator>N</US_SE_DISIndicator></EntryHeaderMessageSendingAction>";
			nesEntryHeader.Logs.AddNew(Events.Authorised, eBondLogger.eBondAutoSendMessageReference + ":REP");
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			declaration.Reload();
			nesEntryHeader.Reload();
			nesEntryHeader.Messages.Reload(true);
			AssertEquals("Send a message.", 1, nesEntryHeader.Messages.Count);
			IEnumerable<US7501DocPrinting> docPrintings = nesEntryHeader.US7501DocPrintingData.Find(x => x.US_MsgPK == nesEntryHeader.Messages[0].PK);
			var docPrintingData = new List<US7501DocPrinting>(new TypedEnumerable<US7501DocPrinting>(docPrintings));
			AssertEquals(1, docPrintingData.Count);
		}

		void CreateTestMessage()
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.eBondStatusNotification;
			message.EM_MessageNum = "~15000";
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_GE = GlbDepartment.CurrentDepartment.PK;
			message.EM_MessageText = "B018888XJ5BS                                               286                  " +
				"B1123456789 ENB NEW BOND HAS BEEN ADDED IN ACE       END 2 1127141230Y          " +
				"B2THIS IS A TEST                                                                " +
				"10B93A30000012345112614123456789112014121514000000009 NY                        " +
				"10B93A30000012345112614123456789112014121514000000009 NY                        " +
				"10B93A30000012345112614123456789112014121514000000009 NY                        " +
				"10B93A30000012345112614123456789112014121514000000009 NY                        " +
				"12NNNNXXXNNAAAABBBCCAAAABBBDDAAAABBBFF                                          " +
				"12NNNNXXXNNAAAABBBCCAAAABBBDDAAAABBBFF                                          " +
				"20107XJ5C1234578                                                                " +
				"3034 NN-NNNNNNNXXPrincipal Name                                                 " +
				"35EI YYDDPP-NNNNNCo-principal Name1                                             " +
				"35EI YYDDPP-NNNNNCo-principal Name2                                             " +
				"35EI YYDDPP-NNNNNCo-principal Name3                                             " +
				"36ANI111-NN-NNNN Bond User Name                          D112714121014          " +
				"36ANI222-NN-NNNN Bond User Name                          D112714121014          " +
				"36ANI333-NN-NNNN Bond User Name                          D112714121014          " +
				"36ANI444-NN-NNNN Bond User Name                          D112714121014          " +
				"36ANI555-NN-NNNN Bond User Name                          D112714121014          " +
				"40123777-88-9999Surety Name                              0000000005             " +
				"45111111-88-9999Surety Name                              0000000005             " +
				"45222222-88-9999Surety Name                              0000000005             " +
				"45333333-88-9999Surety Name                              0000000005             " +
				"Y  8888XJ5WR00005";
			Factory.Save();
		}

		JobDeclaration CreateJobDeclarationForSendMessage()
		{
			DeclarationTestHelper.SetupCompanySpecificFormalEntryNumber("XJ5");
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "Z8";
			staff.GS_EmailAddress = "dummy@email.com";
			GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates);
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = "07";
			declaration.US_EntryFilerCode = "XXX";
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.DecEntryNumber = "C1234578";
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondDesignationCode = BondDesignationCodeList.Codes.BasicBond;
			declaration.US_BondDispositionCode = BondDispositionCodeList.Codes.CAB;
			declaration.US_BondDispositionCode2 = BondDispositionCodeList.Codes.CAB;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.US_AMSInd = "D";
			var amsLine = invoiceLine.AMSLines.AddNew();
			amsLine.US_CommercialDescription = "PGA AMS T";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			declaration.JE_GS_NKCusAgent = "Z8";
			Factory.Save();
			return declaration;
		}
	}
}
