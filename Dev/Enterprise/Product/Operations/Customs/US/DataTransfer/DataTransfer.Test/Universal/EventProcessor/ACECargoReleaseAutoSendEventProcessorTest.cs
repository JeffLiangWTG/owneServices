using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.DataTransfer.Universal.Testing
{
	sealed class ACECargoReleaseAutoSendEventProcessorTest : TestCaseWithFactory
	{
		public void TestAutoSendCargoReleaseMessage()
		{
			CreateTestMessage();
			var declaration = CreateJobDeclarationForSendMessage();
			var simplifiedEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var sendMessageAction = new AutoSendMessageCusAddInfo.Loader(declaration.Factory).Load(simplifiedEntry);
			if (sendMessageAction == null)
			{
				sendMessageAction = Factory.New<AutoSendMessageCusAddInfo>();
				sendMessageAction.B7_ParentID = simplifiedEntry.PK;
				sendMessageAction.B7_ParentTableCode = "CH";
			}

			sendMessageAction.B7_AddInfoData = "<EntryHeaderMessageSendingAction><US_AcknowledgeAndSign>N</US_AcknowledgeAndSign><US_CertifyCargoRelease>N</US_CertifyCargoRelease><US_CertifyTIB>N</US_CertifyTIB><US_CertifyTSCA>N</US_CertifyTSCA><US_IsCustomsRequested>N</US_IsCustomsRequested><US_JobReadyForPosting>N</US_JobReadyForPosting><US_RequestBillOfLadingResult>N</US_RequestBillOfLadingResult><US_SendMessage>Y</US_SendMessage><US_SE_ContactName>CargoWiseOneSupport</US_SE_ContactName><US_SE_DISIndicator>N</US_SE_DISIndicator></EntryHeaderMessageSendingAction>";
			simplifiedEntry.Logs.AddNew(Events.Authorised, eBondLogger.eBondAutoSendMessageReference + ":ORG");
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			declaration.Reload();
			simplifiedEntry.Reload();
			AssertEquals("Send a message.", 1, simplifiedEntry.Messages.Count);
			simplifiedEntry.Logs.GetAllLogs().Reload(true);
			AssertNull("ATH event was cancelled", eBondLogger.GetAutoSendEvent(simplifiedEntry.Logs));
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Recipients.Contains(declaration.CusAgent.GS_EmailAddress));
			Assert(email.Body.Contains("ACE Cargo Release has been sent successfully"));
			var newFactory = new BusinessObjectFactory();
			var simplifiedEntry2 = newFactory.Load<CusEntryHeader>(simplifiedEntry.PK);
			AssertNull("Deleted", new AutoSendMessageCusAddInfo.Loader(newFactory).Load(simplifiedEntry2));
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
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "Z8";
			staff.GS_EmailAddress = "dummy@email.com";
			GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates);
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = "07";
			declaration.US_EntryFilerCode = "XXX";
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.DecEntryNumber = "C1234578";
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondDesignationCode = BondDesignationCodeList.Codes.BasicBond;
			declaration.US_BondDispositionCode = BondDispositionCodeList.Codes.CVB;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.InvoiceLines.AddNew();
			var invoiceHeader2 = declaration.Invoices.AddNew();
			invoiceHeader2.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			declaration.JE_GS_NKCusAgent = "Z8";
			Factory.Save();
			return declaration;
		}
	}
}
