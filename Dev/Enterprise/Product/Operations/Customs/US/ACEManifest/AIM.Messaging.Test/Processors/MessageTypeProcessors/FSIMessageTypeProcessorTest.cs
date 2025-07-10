using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.US.AIM.Messaging.Testing
{
	public class FSIMessageTypeProcessorTest : MessageTypeProcessorBaseTest
	{
		[TestDate(2020, 12, 7, 11, 15, 33)]
		public void TestFSIMessageLinksToBill()
		{
			var manifestHeader = ManifestHeader;
			var bill = HouseBill;

			var responseMessageText = @"FSI
MIAKLM
081-11223344-HAWB123/123456
ARR/KL325/12DEC
CSN/1C-20/12DEC1430/0158232873876
";

			ProcessMessage(responseMessageText, bill);

			manifestHeader.Reload();
			AssertEquals("ArrivalHeaders.Count", 0, manifestHeader.ArrivalHeaders.Count);

			bill.Reload();
			AssertEquals("ABL_MessageStatus", ZString.Empty, bill.ABL_MessageStatus);
			AssertEquals("ABL_BillStatus", ZString.Empty, bill.ABL_BillStatus);

			var expectedPropertyTableHTML = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Property</th><th>Value</th></tr></thead><tr><td>Message Time</td><td>07-Dec-20 11:15:33</td></tr><tr><td>Air Waybill Number</td><td>08111223344</td></tr><tr><td>HAWB Number</td><td>HAWB123</td></tr><tr><td>Package Tracking Identifier</td><td>123456</td></tr><tr><td>Flight Number</td><td>KL325</td></tr><tr><td>Scheduled Arrival Date</td><td>12-Dec-20</td></tr><tr><td>Part Arrival Reference</td><td>&nbsp;</td></tr><tr><td>Airport Of Arrival</td><td>MIA</td></tr><tr><td>Cargo Terminal Operator</td><td>KLM</td></tr><tr><td>Action Code</td><td>1C</td></tr><tr><td>Remarks</td><td>&nbsp;</td></tr></table>";
			AssertEmailCreated(ExpectedEmailMessageType + " - 08111223344 - HAWB123", expectedPropertyTableHTML);
		}

		[TestDate(2020, 12, 7)]
		public void TestFSIMessage_UnsolicitedMessageLinksToBill()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;
			var messageBranch = Factory.NewWithValidTestData<GlbBranch>();
			messageBranch.GB_GC = company.PK;

			var manifestHeader = ManifestHeader;
			var bill = HouseBill;

			var responseMessageText = @"FSI
MIAKLM
081-11223344-HAWB123/
ARR/KL325/12DEC
CSN/1C-20/12DEC1430/0158232873876
";

			var inboundMessage = CreateQueuedMessage(responseMessageText);
			inboundMessage.EM_GB = messageBranch.PK;
			Factory.Save();

			var processor = GetNewMessageTypeProcessor();
			processor.Process(inboundMessage);
			Factory.Save();

			inboundMessage.Reload();
			AssertEquals("Unsolicited message is on Bill. EM_LinkTable", AsycudaBillSchema.Constants.TableName, inboundMessage.EM_LinkTable);
			AssertEquals("Unsolicited message is on Bill. EM_LinkUniqueID", bill.PK, inboundMessage.EM_LinkUniqueID);
			AssertEquals("Unsolicited message is on Manifest branch. EM_GB", manifestHeader.AMA_GB, inboundMessage.EM_GB);
		}

		[TestDate(2020, 12, 7, 11, 15, 33)]
		public void TestFSIMessageWithoutPackageTrackingIdentifierLinksToBill()
		{
			var manifestHeader = ManifestHeader;
			var bill = HouseBill;

			var responseMessageText = @"FSI
MIAKLM
081-11223344-HAWB123/
ARR/KL325/12DEC
CSN/1C-20/12DEC1430/0158232873876
";

			ProcessMessage(responseMessageText, bill);

			bill.Reload();
			AssertEquals("ABL_MessageStatus", ZString.Empty, bill.ABL_MessageStatus);
			AssertEquals("ABL_BillStatus", ZString.Empty, bill.ABL_BillStatus);

			var expectedPropertyTableHTML = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Property</th><th>Value</th></tr></thead><tr><td>Message Time</td><td>07-Dec-20 11:15:33</td></tr><tr><td>Air Waybill Number</td><td>08111223344</td></tr><tr><td>HAWB Number</td><td>HAWB123</td></tr><tr><td>Package Tracking Identifier</td><td>&nbsp;</td></tr><tr><td>Flight Number</td><td>KL325</td></tr><tr><td>Scheduled Arrival Date</td><td>12-Dec-20</td></tr><tr><td>Part Arrival Reference</td><td>&nbsp;</td></tr><tr><td>Airport Of Arrival</td><td>MIA</td></tr><tr><td>Cargo Terminal Operator</td><td>KLM</td></tr><tr><td>Action Code</td><td>1C</td></tr><tr><td>Remarks</td><td>&nbsp;</td></tr></table>";
			AssertEmailCreated(ExpectedEmailMessageType + " - 08111223344 - HAWB123", expectedPropertyTableHTML);
		}

		[TestDate(2020, 12, 7)]
		public void TestFSIMessage_MawbHawbNotFoundIsLogged()
		{
			var manifestHeader = ManifestHeader;
			manifestHeader.AMA_MasterBill = "08112345678";
			var bill = HouseBill;
			bill.ABL_BillNumber = "HAWB999";

			var responseMessageText = @"FSI
MIAKLM
081-11223344-HAWB123/
ARR/KLM325/12DEC
CSN/1C-20/12DEC1430/0158232873876
";

			var outgoingMessage = CreateSentMessage(bill);
			var inboundMessage = CreateQueuedMessage(responseMessageText);
			Factory.Save();

			var logger = new LoggingInformation();
			var processor = new FSIMessageTypeProcessor(logger);
			AssertEquals("processor.Process() result", false, processor.Process(inboundMessage));
			Factory.Save();

			inboundMessage.Reload();
			AssertEquals("EM_LinkUniqueID", ZGuid.Empty, inboundMessage.EM_LinkUniqueID);
			AssertEquals("EM_LinkTable", ZString.Empty, inboundMessage.EM_LinkTable);

			var notes = inboundMessage.Notes.GetAllNotes();
			var note = notes.Cast<StmNote>().First(n => n.ST_Description == "AIM Message Processing");
			AssertEquals("A Manifest record was not found for MasterBill = 081-11223344, HouseBill = HAWB123.", note.ST_NoteText);

			var log = logger.Logs.First(l => l.Message.StartsWith("A Manifest record was not found"));
			AssertEquals("A Manifest record was not found for MasterBill = 081-11223344, HouseBill = HAWB123.", log.Message);

			bill.Reload();
			AssertEquals("ABL_MessageStatus", ZString.Empty, bill.ABL_MessageStatus);
			AssertEquals("ABL_BillStatus", ZString.Empty, bill.ABL_BillStatus);
		}

		#region Calculated Arrival Date

		[TestDate(2021, 11, 1)]
		public void TestFSIMessageCalculatedArrivalDate_EmptyEstDate()
		{
			var responseMessageText = @"FSI
MIAKLM
081-11223344-HAWB123/
ARR/KL325/01JUL
CSN/1C-20/12DEC1430/0158232873876
";

			var manifestHeader = ManifestHeader;
			AssertCalculatedScheduledArrivalDate(manifestHeader, responseMessageText, "Default to the year from EstimateScheduledArrivalDate of Arrival Wrapper.", ZDate.Empty, new ZDate(2022, 07, 01));
		}

		[TestDate(2021, 11, 1)]
		public void TestFSIMessageCalculatedArrivalDate_ValidEstDate()
		{
			var responseMessageText = @"FSI
MIAKLM
081-11223344-HAWB123/
ARR/KL325/01JUL
CSN/1C-20/12DEC1430/0158232873876
";

			var manifestHeader = ManifestHeader;
			AssertCalculatedScheduledArrivalDate(manifestHeader, responseMessageText, "Should consider the year from AMA_E_ARV of manifestHeader.", new ZDate(2021, 06, 01), new ZDate(2021, 07, 01));
		}

		[TestDate(2021, 11, 1)]
		public void TestFSIMessageCalculatedArrivalDate_CloseToNewYear()
		{
			var responseMessageText = @"FSI
MIAKLM
081-11223344-HAWB123/
ARR/KL325/01JAN
CSN/1C-20/12DEC1430/0158232873876
";

			var manifestHeader = ManifestHeader;
			AssertCalculatedScheduledArrivalDate(manifestHeader, responseMessageText, "Should consider the year from AMA_E_ARV of manifestHeader.", new ZDate(2021, 12, 31), new ZDate(2022, 01, 01));
		}

		void AssertCalculatedScheduledArrivalDate(AsycudaManifestHeader manifestHeader, string responseMessageText, string message, ZDate arrivalDate, ZDate expectedArrivalDate)
		{
			manifestHeader.AMA_E_ARV = arrivalDate;
			manifestHeader.Factory.Save();

			ProcessMessage(responseMessageText, manifestHeader);

			manifestHeader.Messages.Reload(true);

			var inboundMessage = manifestHeader.Messages
				.Find(c => c.EM_ReceiveTransmit == Direction.Receive)
				.FirstOrDefault() as FSIMessage;

			AssertNotNull(inboundMessage);
			AssertEquals(message, expectedArrivalDate, inboundMessage.CalculatedScheduledArrivalDate);
		}

		#endregion

		#region Implementation

		protected override IAIMMessageTypeProcessor GetNewMessageTypeProcessor() => new FSIMessageTypeProcessor(new LoggingInformation());
		protected override AIMInboundMessage GetNewInboundMessage() => Factory.New<FSIMessage>();
		protected override ZString ExpectedEmailMessageType => "Freight Status Information (FSI)";

		protected override void PopulateManifestHeader(AsycudaManifestHeader manifestHeader)
		{
			manifestHeader.AMA_Voyage = "KL325";
			manifestHeader.AMA_E_ARV = new ZDateTime(2020, 12, 12);
			manifestHeader.AMA_RL_NKPortOfFirstArrival = "USMIA";
			manifestHeader.AMA_CarrierCode = "KLM";
		}

		protected override void PopulateHouseBill(AsycudaBill houseBill)
		{
			houseBill.ABL_SenderReference = "123456";
			houseBill.ABL_MessageStatus = ZString.Empty;
		}

		protected override void PopulateArrivalHeader(AsycudaArrivalHeader arrivalHeader)
		{
			arrivalHeader.ATH_VoyageFlightNo = "KL325";
			arrivalHeader.ATH_ETAAtDischargePort = new ZDateTime(2020, 12, 12);
		}

		protected override ZString GetResponseMessageForGroupNotificationTesting()
		{
			return @"FSI
MIAKLM
081-11223344-HAWB123/123456
ARR/KL325/12DEC
CSN/1C-20/12DEC1430/0158232873876
";
		}

		#endregion
	}
}
