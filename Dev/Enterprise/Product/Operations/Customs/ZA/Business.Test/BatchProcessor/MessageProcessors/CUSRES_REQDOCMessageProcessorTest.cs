using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.ZA.Business.BatchProcessor;
using Enterprise.Customs.ZA.Business.MessageProcessor;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using static Enterprise.Integration.Customs.ASYCUDA;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class CUSRES_REQDOCMessageProcessorTest : TestCaseWithFactory
	{
		public void TestCusres_ReqdocMessagePreProcessing()
		{
			var logger = InboundInterchangeProcessorTest.GetNewLoggerForTesting() as LoggingInformationForTesting;
			var testMessage = Factory.NewWithValidTestData<CUSRES_REQDOCEDIMessage>();
			testMessage.EM_ApplicationCode = "ZAC";
			testMessage.EM_MessageType = "RSQ";
			testMessage.EM_MessageSubType = "XXX";
			testMessage.EM_ReceiveTransmit = "RCV";
			testMessage.EM_MessageNum = "1";
			testMessage.EM_Status = "QUE";
			testMessage.EM_MessageText = ResponseMessageText("01020304JSA20160708000064");
			Factory.Save();
			new CUSRES_REQDOCMessageProcessor(logger).PreProcessMessage(testMessage);
			Factory.Save();
			CombineAssertions("Rejected", () =>
			{
				AssertEquals("PPS", testMessage.EM_Status);
				AssertEquals(testHeader.PK, testMessage.EM_LinkUniqueID);
				AssertMultilineASCIIEquals("Log", @"Information: 	Linking CUSRES-REQDOC Message: #1/ to job: 01020304JSA20160708000064", logger.LogMessages.ToString());
			});
		}

		public void TestCusres_ReqdocMessagePreProcessingForManifestHeaderLevelMessages()
		{
			var cuscarMessage = Factory.New<CUSCAREDIMessage>();
			cuscarMessage.EM_MessageText = @"UNH+360+CUSCAR:D:16A:UN:RCG001'BGM+85:::ALM+" + EDIMessage.SystemCommonAccessReferencePkPlaceholder + @"+9'RFF+LO:MAN0000014'NAD+MS+12342342'NAD+DEG'TDT+20++++:172:20'LOC+60'CNI+1+123434:BOL:123434'RFF+BM:11111'LOC+8'LOC+9'GID+1+0'FTX+AAA++9'MEA+AAE+AAB+KGM:0'PCI+24'UNT+16+360'";
			var manifestHeaderMessageAttachee = (IEDIFACTMessageAttacheeAndEDIFACTMessageStatusCalculatorProvider)Factory.New<IAsycudaManifestHeader>();
			manifestHeaderMessageAttachee.Messages.Add(cuscarMessage);
			AssertCusres_ReqdocMessagePreProcessingForManifests(manifestHeaderMessageAttachee);
		}

		public void TestCusres_ReqdocMessagePreProcessingForManifestBillLevelMessages()
		{
			var cuscarMessage = Factory.New<CUSCAREDIMessage>();
			cuscarMessage.EM_MessageText = @"UNH+360+CUSCAR:D:16A:UN:RCG001'BGM+85:::ALM+" + EDIMessage.SystemCommonAccessReferencePkPlaceholder + @"+9'RFF+LO:MAN0000014'NAD+MS+12342342'NAD+DEG'TDT+20++++:172:20'LOC+60'CNI+1+123434:BOL:123434'RFF+BM:11111'LOC+8'LOC+9'GID+1+0'FTX+AAA++9'MEA+AAE+AAB+KGM:0'PCI+24'UNT+16+360'";
			var manifestHeader = Factory.New<IAsycudaManifestHeader>();
			var bill = Factory.New<IAsycudaBill>();
			bill.ABL_AMA = manifestHeader.PK;
			var billMessageAttachee = (IEDIFACTMessageAttacheeAndEDIFACTMessageStatusCalculatorProvider)bill;
			billMessageAttachee.Messages.Add(cuscarMessage);
			AssertCusres_ReqdocMessagePreProcessingForManifests(billMessageAttachee);
		}

		public void TestPreProcessingNotFindingParent()
		{
			var logger = InboundInterchangeProcessorTest.GetNewLoggerForTesting() as LoggingInformationForTesting;
			var testMessage = Factory.NewWithValidTestData<CUSRES_REQDOCEDIMessage>();
			testMessage.EM_ReceiveTransmit = "RCV";
			testMessage.EM_ApplicationCode = "ZAC";
			testMessage.EM_Status = "QUE";
			testMessage.EM_MessageType = "RSQ";
			testMessage.EM_MessageSubType = "RSQ";
			testMessage.EM_MessageText = ResponseMessageText("Poop LRN should not be found");
			testMessage.EM_MessageNum = "IN1";
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_InterchangeNum = "INT123";
			testMessage.EM_EI = interchange.PK;
			Factory.Save();
			new CUSRES_REQDOCMessageProcessor(logger).PreProcessMessage(testMessage);
			AssertEquals("DCD", testMessage.EM_Status);
			AssertMultilineASCIIEquals("Log", @"Information: 	Unable to find the linked job for CUSRES-REQDOC Message: #IN1/INT123", logger.LogMessages.ToString());
		}

		public void TestPreProcessingRequired()
		{
			var logger = InboundInterchangeProcessorTest.GetNewLoggerForTesting() as LoggingInformationForTesting;
			var processor = new CUSRES_REQDOCMessageProcessor(logger);
			AssertEquals(true, processor.RequiresPreProcessing);
		}

		public void TestProcessBatch()
		{
			var declaration = Factory.New<JobDeclaration>();
			var testHeader = declaration.CustomsEntryHeaders.AddNew();
			testHeader.CH_BGMReference = "TESTHeader1";
			var outgoingMessage = Factory.NewWithValidTestData<ZAMessageForTest>();
			outgoingMessage.EM_ApplicationCode = "ZAC";
			outgoingMessage.EM_MessageType = "DEC";
			outgoingMessage.EM_MessageSubType = "ORG";
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			outgoingMessage.EM_MessageNum = "911";
			outgoingMessage.EM_LinkUniqueID = testHeader.PK;
			outgoingMessage.EM_LinkTable = testHeader.TableName;
			outgoingMessage.MessageNumForTesting = "911";
			var testMessage = Factory.NewWithValidTestData<CUSRESEDIMessageForTest>();
			testMessage.EM_ApplicationCode = "ZAC";
			testMessage.EM_MessageType = "CTL";
			testMessage.EM_MessageSubType = "XXX";
			testMessage.EM_ReceiveTransmit = "RCV";
			testMessage.EM_MessageNum = "1";
			testMessage.EM_Status = "QUE";
			testMessage.EM_MessageText = "UNH+1+CONTRL:D:3:UN:CONTRL'UCI+181+00505655TST::AAAAAAAAAAAAAABB:CORAS2+SARSDECT+7'UCM+911+CUSDEC:D:96B:UN:ZZZ01+7'UNT+4+1'";
			Factory.Save();
			var logger = (LoggingInformationForTesting)InboundInterchangeProcessorTest.GetNewLoggerForTesting();
			var proc = new ZACIncomingMessageProcessor(logger);
			proc.ExecuteBatch();
			testMessage.Reload();
			AssertEquals("PRS", testMessage.EM_Status);
			AssertEquals("CusEntryHeader", testMessage.EM_LinkTable);
			AssertEquals(testHeader.PK, testMessage.EM_LinkUniqueID);
		}

		public void TestProcessMessageWithNoLinkID()
		{
			var testMessage = Factory.NewWithValidTestData<CUSRES_REQDOCEDIMessage>();
			testMessage.EM_ApplicationCode = "ZAC";
			testMessage.EM_MessageType = "RSQ";
			testMessage.EM_MessageSubType = "XXX";
			testMessage.EM_ReceiveTransmit = "RCV";
			testMessage.EM_MessageNum = "1";
			testMessage.EM_Status = "QUE";
			testMessage.EM_LinkTable = ZString.Empty;
			testMessage.EM_LinkUniqueID = ZGuid.Empty;
			testMessage.EM_MessageText = ResponseMessageText();
			Factory.Save();
			var logger = InboundInterchangeProcessorTest.GetNewLoggerForTesting() as LoggingInformationForTesting;
			new CUSRES_REQDOCMessageProcessor(logger).ProcessMessage(testMessage);
			CombineAssertions("Rejected", () =>
			{
				AssertEquals("DCD", testMessage.EM_Status);
				Assert(!testMessage.EM_LinkUniqueID.IsValid);
				AssertMultilineASCIIEquals("Log", @"Information: 	No business object linked to Message: #1/", logger.LogMessages.ToString());
			});
		}

		CusEntryHeader testHeader;
		protected override void SetUp()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsOfficeCusCodeEntry("BFN");
			var testAgent = Factory.NewWithValidTestData<OrgHeader>();
			testAgent.CustomsCodes.AddNew("CDP", "51051342", "ZA");
			testAgent.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "51051342", Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();
			var collection = new FinancialAccountNumberPortMapCollection();
			var mapping = collection.AddNew();
			mapping.OrganizationPK = testAgent.PK;
			mapping.CustomsOfficeCode = "BFN";
			mapping.FinancialAccountNumber = "3234002346";
			mapping.ImporterPays = true;
			mapping.AccountStartDay = 1;
			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = "BLT";
			declaration.JE_MessageType = "IMP";
			declaration.JE_OA_DeclarantAddress = testAgent.MainAddress.PK;
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = "11";
			var invHeader = declaration.Invoices.AddNew();
			invHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invLine1 = invHeader.JobComInvoiceLines.AddNew();
			invLine1.JI_CEI = entryInstruction.PK;
			invLine1.JI_Procedure = invLine1.EntryInstruction.CEI_Style + "00";
			Factory.Save();
			new LineMerger(declaration).DoMerge();
			AssertEquals(1, declaration.ActiveEntryHeaders.Count);
			Factory.Save();
			testHeader = declaration.ActiveEntryHeaders.FirstOrDefault() as CusEntryHeader;
			testHeader.CH_BGMReference = "01020304JSA20160708000064";
		}

		void AssertCusres_ReqdocMessagePreProcessingForManifests(IEDIFACTMessageAttacheeAndEDIFACTMessageStatusCalculatorProvider messageAttachee)
		{
			var reference = new ZString(messageAttachee.Messages[0].PK.ToString()).KeepAlphanumericCharacters().ToUpper();
			var logger = InboundInterchangeProcessorTest.GetNewLoggerForTesting() as LoggingInformationForTesting;
			var testMessage = Factory.NewWithValidTestData<CUSRES_REQDOCEDIMessage>();
			testMessage.EM_ApplicationCode = "ZAC";
			testMessage.EM_MessageType = "RSQ";
			testMessage.EM_MessageSubType = "XXX";
			testMessage.EM_ReceiveTransmit = "RCV";
			testMessage.EM_MessageNum = "1";
			testMessage.EM_Status = "QUE";
			testMessage.EM_MessageText = ResponseMessageText(reference);
			Factory.Save();
			new CUSRES_REQDOCMessageProcessor(logger).PreProcessMessage(testMessage);
			Factory.Save();
			var manifestHeader = (IAsycudaManifestHeader)messageAttachee.TopLevelBusinessObject;
			CombineAssertions("Rejected", () =>
			{
				AssertEquals("PPS", testMessage.EM_Status);
				AssertEquals(((BusinessObject)messageAttachee).PK, testMessage.EM_LinkUniqueID);
				AssertMultilineASCIIEquals("Log", @"Information: 	Linking CUSRES-REQDOC Message: #1/ to job: " + manifestHeader.AMA_MasterBill, logger.LogMessages.ToString());
			});
		}

		const string responseMessageTextTemplate = @"UNH+1+CUSRES:D:96B:UN:ZZZ01'
BGM+962+{{JOB_REFEFENCE}}'
GIS+8:120:ZZZ'
ERP+1'
ERC+0000'
FTX+AAO+++CUSDEC-{{JOB_REFEFENCE}}, Ver=, TraderRef=224915: :CUSDEC TimeStamp = 20160704162512:DocType=830, MFC=9'
FTX+AAO+++CUSRES-{{JOB_REFEFENCE}}, Ver=0, TraderRef=224915:SARS Ref = 427902155b64c8b49node1:CUSDEC TimeStamp = 20160704162512, CUSRES Processed=20160704162521:Status=1:Recipient=MSC'
FTX+AAO+++CUSRES-{{JOB_REFEFENCE}}, Ver=0, TraderRef=224915:SARS Ref = 458078155b64c8b49node1:CUSDEC TimeStamp = 20160704162512, CUSRES Processed=20160704162522:Status=1:Recipient=51051342'
FTX+AAO+++CUSRES-{{JOB_REFEFENCE}}, Ver=0, TraderRef=224915:SARS Ref = 868696155b64c8b49node1:CUSDEC TimeStamp = 20160704162512, CUSRES Processed=20160704162522:Status=1:Recipient=87'
UNT+10+1'";

		internal static string ResponseMessageText(string jobReference = "01020304JSA20160708000064") =>
			responseMessageTextTemplate
			.Replace("\r\n", "")
			.Replace("{{JOB_REFEFENCE}}", jobReference);
	}
}
