using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using static Enterprise.Customs.ZA.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ZA.Business.Testing.UCMP
{
	static class TestMessageFactory
	{
		public static (EDIMessage IncomingMessage, EDIMessage OutgoingMessageReplyingTo, CusEntryHeader CusEntryHeader, JobDeclaration Declaration) Get_CONTRL_WithLinkedCusResEntryHeaderOutgoingMessage(BusinessObjectFactory factory, string messageNum = "123", string headerReference = "TestHeader123")
		{
			var testDeclaration = factory.New<JobDeclaration>();
			var testHeader = testDeclaration.CustomsEntryHeaders.AddNew();
			testHeader.CH_BGMReference = headerReference;

			var outgoingMessage = factory.New<ZAMessageForTest>();
			outgoingMessage.EM_MessageType = "DEC";
			outgoingMessage.EM_MessageSubType = "ORG";
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			outgoingMessage.EM_MessageNum = messageNum;
			outgoingMessage.EM_LinkUniqueID = testHeader.PK;
			outgoingMessage.EM_LinkTable = testHeader.TableName;
			outgoingMessage.MessageNumForTesting = messageNum;

			var incomingMessage = GetIncomingCONTRLEDIMessage(factory, messageNum);
			factory.Save();

			return (incomingMessage, outgoingMessage, testHeader, testDeclaration);
		}

		public static CONTRLEDIMessage GetIncomingCONTRLEDIMessage(BusinessObjectFactory factory, ZString messageNum)
		{
			var incomingMessage = GetIncomingMessage<CONTRLEDIMessage>(factory, $"UNH+1+CONTRL:D:3:UN:CONTRL'UCI+181+00505655TST::AAAAAAAAAAAAAABB:CORAS2+SARSDECT+7'UCM+{messageNum}+CUSDEC:D:96B:UN:ZZZ01+7'UNT+4+1'", messageNum);
			incomingMessage.EM_MessageSubType = "XXX";
			return incomingMessage;
		}

		public static (EDIMessage IncomingMessage, AsycudaManifestHeader ManifestHeader) Get_CUSCAR_WithLinkedAsycudaManifestHeader(BusinessObjectFactory factory)
		{
			// values matching CUSCARMessageProcessorTest.TestD16BMessage1Bill2Containers
			const string voyage = "S123";
			const string billNumber = "098475";
			var billIssueDate = new ZDate(2018, 02, 06);

			var manifestHeader = factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_AgentType = Core.Constants.AgentType.Agent;
			manifestHeader.AMA_IsActive = true;
			manifestHeader.AMA_Voyage = voyage;
			manifestHeader.AMA_RadioCallSign = "2CDEF";
			manifestHeader.AMA_VesselName = "ARGONAUT";
			manifestHeader.AMA_ManifestType = nameof(ManifestDocumentType.COH);

			var masterBill = manifestHeader.MasterBill;
			masterBill.ABL_BolType = "BOL";
			masterBill.ABL_BillNumber = billNumber;
			masterBill.ABL_BillIssueDate = billIssueDate;
			masterBill.ABL_IsActive = true;

			var container = manifestHeader.Containers.AddNew();
			container.ACN_ContainerNumber = "MSCU2443581";
			container.ACN_IsActive = true;
			container.ACN_GoodsWeight = 110m;
			container.ACN_GoodsWeightUQ = "L";
			container.ACN_Seal1 = "Original Seal";

			var houseBill = manifestHeader.Bills.AddNew();
			houseBill.ABL_BolType = "HWB";
			houseBill.ABL_BillNumber = $"HB{billNumber}";
			houseBill.ABL_BillIssueDate = billIssueDate;
			houseBill.ABL_IsActive = true;
			houseBill.ABL_GoodsDescription = "Stuff to be shipped";
			houseBill.ABL_ManifestQty = 4;
			houseBill.ABL_ManifestUQ = "BX";
			houseBill.ABL_GoodsLocation = "ZZZ";

			var pack = houseBill.Packs.AddNew();
			pack.APA_Weight = 99m;
			pack.APA_WeightUQ = "L";
			pack.APA_GoodsDescription = "Cola";
			pack.APA_MarksAndNumbers = "BillAPackA";
			pack.APA_VINNumber = "";
			pack.APA_LineNo = 1;
			pack.ContainerPK = container.PK;

			var message = Get_CUSCAR_WithNoLinkedObject(factory);
			factory.Save();

			return (message, manifestHeader);
		}

		public static CUSCAREDIMessage Get_CUSCAR_WithNoLinkedObject(BusinessObjectFactory factory) => GetIncomingMessage<CUSCAREDIMessage>(factory, string.Format(Testing.CUSCARMessageProcessorTest.TestD16BMessage1Bill2Containers.Replace("\r\n", ""), "AND", "9"));

		public static (EDIMessage IncomingMessage, CusEntryHeader CusEntryHeader, JobDeclaration declaration) Get_CUSRES_WithLinkedCusEntryHeader(BusinessObjectFactory factory, string headerReference = "123456789")
		{
			var testDeclaration = factory.New<JobDeclaration>();
			var testHeader = testDeclaration.CustomsEntryHeaders.AddNew();
			testHeader.CH_BGMReference = headerReference;

			GetOutgoingCUSRESEDIMessage(testHeader);
			var message = GetIncomingCUSRESEDIMessage(factory);
			factory.Save();

			return (message, testHeader, testDeclaration);
		}

		public static EDIMessage Get_CUSRES_WithLinkedAsycudaManifestHeader(ManifestBase.AsycudaManifestHeader manifestHeader, string masterBillNumber = "MB01234567")
		{
			var factory = manifestHeader.Factory;
			var bill = manifestHeader.Bills.AddNew();
			bill.ABL_BillNumber = $"HB-{masterBillNumber}";

			GetOutgoingCUSRESEDIMessage(manifestHeader);
			var incomingMessage = GetIncomingCUSRESEDIMessage(factory);
			factory.Save();
			return incomingMessage;
		}

		public static CUSRESEDIMessage GetIncomingCUSRESEDIMessage(BusinessObjectFactory factory) => GetIncomingCUSRESEDIMessage(factory, Testing.CUSRESMessageProcessorTest
				.GetTestMessageResNo(CustomsStatus.SupportingDocsRequired)
				.Replace("RFF+ACD:202'", "RFF+ACD:202'RFF+AAV:TESTCASE1'"));

		public static CUSRESEDIMessage GetIncomingCUSRESEDIMessage(BusinessObjectFactory factory, ZString messageText) => GetIncomingMessage<CUSRESEDIMessage>(factory, messageText);

		public static (EDIMessage IncomingMessage, CusEntryHeader CusEntryHeader, JobDeclaration Declaration) Get_CUSRES_REQDOC(BusinessObjectFactory factory, string jobReference)
		{
			var testDeclaration = factory.New<JobDeclaration>();
			var testHeader = testDeclaration.CustomsEntryHeaders.AddNew();
			testHeader.CH_BGMReference = jobReference;

			GetOutgoingCUSRESEDIMessage(testHeader);
			var incomingMessage = GetIncomingMessage<CUSRES_REQDOCEDIMessage>(factory, CUSRES_REQDOCMessageProcessorTest.ResponseMessageText(jobReference));
			factory.Save();

			return (incomingMessage, testHeader, testDeclaration);
		}

		public static CUSRESEDIMessage GetOutgoingCUSRESEDIMessage(BusinessObject parent, string messageNum = "202")
		{
			var messageText = Testing.CUSRESMessageProcessorTest.TestOutGoingMessage.Replace("\r\n", "");

			var outgoingMessage = parent.Factory.New<CUSRESEDIMessageForTest>();
			outgoingMessage.EM_MessageType = "DEC";
			outgoingMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			outgoingMessage.EM_LinkUniqueID = parent.PK;
			outgoingMessage.EM_LinkTable = parent.TableName;
			outgoingMessage.EM_MessageText = messageText;
			outgoingMessage.MessageNumForTesting = messageNum;
			return outgoingMessage;
		}

		public static GENRALMessage Get_GENRAL(BusinessObjectFactory factory) => GetIncomingMessage<GENRALMessage>(factory, @"UNH+00000000155033+GENRAL:D:16A:UN:ZZZ01'BGM+719:::+4:TST001+55'DTM+706:20211018161937:204'NAD+MR+20507309WTG'FTX+AAI+++Message 1:Message 2:Message 3:Message 4:Message 5'UNT+19+00000000155033'");

		public static STATACEDIMessage Get_STATAC(BusinessObjectFactory factory) => GetIncomingMessage<STATACEDIMessage>(factory, Testing.STATACMessageProcessorTest.STATAC_DAILY_Message.Replace("\r\n", ""));

		public static TMessage GetIncomingMessage<TMessage>(BusinessObjectFactory factory, ZString messageText, string messageNum = "IN0")
			where TMessage : ZAMessage
		{
			var message = factory.New<TMessage>();
			message.EM_ReceiveTransmit = STATACEDIMessage.Status.Queued;
			message.EM_Status = STATACEDIMessage.Status.Queued;
			message.EM_MessageText = messageText;
			message.EM_MessageNum = messageNum;

			return message;
		}
	}
}
