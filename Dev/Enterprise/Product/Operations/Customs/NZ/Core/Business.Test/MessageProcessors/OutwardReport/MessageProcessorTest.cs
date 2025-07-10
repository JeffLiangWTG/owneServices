using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.NZ;
using Enterprise.Customs.NZ.Business.Declaration.OutwardReport;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NZ.Business.MessageProcessors.OutwardReport.Testing
{
	public class MessageProcessorTest : TestCaseWithFactory
	{
		public void TestMessageProcessingExceptionCaughtWhenThereIsNoConsolAttached()
		{
			string response = @"
UNH+TRN888005+CUSRES:D:03A:UN+SRN654301'
BGM+932+45678901'
GEI+6+847:120:143'
UNT+4+TRN888005'
";
			Declaration.NZCMessage message = GetNZCMessage(response);
			LoggingInformation logger = new LoggingInformation();
			MessageProcessor processor = new MessageProcessor(logger);
			processor.ProcessMessage(message);
			Assert("Logger should be reported that no Consol exists", logger.UserLogStrings.Count > 0);
		}

		public void TestProcessMessageWithoutUNH()
		{
			string response = @"
BGM+932+45678901'
GEI+6+847:120:143'
UNT+3+TRN888005'
";
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "SRN654301";
			consol.JK_MasterBillNum = "081-12345678";

			Declaration.NZCMessage message = GetNZCMessage(response);
			LoggingInformation logger = new LoggingInformation();
			MessageProcessor processor = new MessageProcessor(logger);
			processor.ProcessMessage(message);
			AssertEquals("No success", "ERR", message.EM_Status);
			Assert("Logger should be reported that no UNH is there", logger.UserLogStrings.Count > 0);
		}

		public void TestProcessMessageWithoutBGM()
		{
			string response = @"
UNH+TRN888005+CUSRES:D:03A:UN+SRN654301'
GEI+6+847:120:143'
UNT+3+TRN888005'
";
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "SRN654301";
			consol.JK_MasterBillNum = "081-12345678";

			Declaration.NZCMessage message = GetNZCMessage(response);
			LoggingInformation logger = new LoggingInformation();
			MessageProcessor processor = new MessageProcessor(logger);
			processor.ProcessMessage(message);
			Assert("Logger should be reported that no BGM is there", logger.UserLogStrings.Count > 0);
		}

		public void TestProcessMessageWithoutGEI()
		{
			string response = @"
UNH+TRN888005+CUSRES:D:03A:UN+SRN654301'
BGM+932+45678901'
UNT+3+TRN888005'
";
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "SRN654301";
			consol.JK_MasterBillNum = "081-12345678";

			Declaration.NZCMessage message = GetNZCMessage(response);
			LoggingInformation logger = new LoggingInformation();
			MessageProcessor processor = new MessageProcessor(logger);
			processor.ProcessMessage(message);
			Assert("Logger should be reported that no GEI is there", logger.UserLogStrings.Count > 0);
		}

		public void TestProcessMessageWithoutERC()
		{
			string rejectionResponse = @"
UNH+TRN888006+CUSRES:D:03A:UN+SRN654302'
BGM+963+45678901'
GEI+6+841:120:143'
ERP+1::80'
DOC+ERR:148:143+1'
ERP+1:730'
ERC+678::143'
UNT+8+TRN888006'
";

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "SRN654302";
			consol.JK_MasterBillNum = "08112345678";

			string messageText = @"UNH+111+CUSCAR:D:03A:UN'
BGM+833:::DEPART+SRN654304+9'
NAD+CS+40009914H:ZZZ:143'
NAD+CH++QANTAS'TDT+20++4+++++:::QF2'
LOC+5+NZWLG'
LOC+8+US'
DTM+136:20031014:102'
CNT+2:2'
CNI+1+89012345'
RFF+HWB:123456'
CNI+2+23456789'
RFF+HWB:SOC00009876'
UNT+14+111'";

			EDIMessage outgoing = consol.Messages.AddNew();
			outgoing.EM_MessageText = messageText.Replace("\r\n", "");
			outgoing.EM_ReceiveTransmit = "TRX";
			outgoing.EM_ApplicationCode = EDIInterchange.ApplicationCodes.NewZealandCustoms;
			outgoing.EM_MessageType = OutwardReportMessage.MessageTypes.OutwardReport.MessageType;

			Declaration.NZCMessage message = GetNZCMessage(rejectionResponse);
			LoggingInformation logger = new LoggingInformation();
			MessageProcessor processor = new MessageProcessor(logger);
			processor.ProcessMessage(message);
			AssertEquals("No success", "ERR", message.EM_Status);

			Assert("No ERC segment is found", logger.UserLogStrings.Count > 0);
		}

		public void TestProcessWithoutSegmentGroup14()
		{
			string rejectionResponse = @"
UNH+TRN888006+CUSRES:D:03A:UN+SRN654302'
BGM+963+45678901'
GEI+6+841:120:143'
ERP+1::80'
ERC+101::143'
DOC+ERR:148:143+1'
UNT+7+TRN888006'
";

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "SRN654302";
			consol.JK_MasterBillNum = "08112345678";

			string messageText = @"UNH+111+CUSCAR:D:03A:UN'
BGM+833:::DEPART+SRN654304+9'
NAD+CS+40009914H:ZZZ:143'
NAD+CH++QANTAS'TDT+20++4+++++:::QF2'
LOC+5+NZWLG'
LOC+8+US'
DTM+136:20031014:102'
CNT+2:2'
CNI+1+89012345'
RFF+HWB:123456'
CNI+2+23456789'
RFF+HWB:SOC00009876'
UNT+14+111'";

			EDIMessage outgoing = consol.Messages.AddNew();
			outgoing.EM_MessageText = messageText.Replace("\r\n", "");
			outgoing.EM_ReceiveTransmit = "TRX";
			outgoing.EM_ApplicationCode = EDIInterchange.ApplicationCodes.NewZealandCustoms;
			outgoing.EM_MessageType = OutwardReportMessage.MessageTypes.OutwardReport.MessageType;

			Declaration.NZCMessage message = GetNZCMessage(rejectionResponse);
			LoggingInformation logger = new LoggingInformation();
			MessageProcessor processor = new MessageProcessor(logger);
			processor.ProcessMessage(message);
			AssertEquals("No success", "ERR", message.EM_Status);
			Assert("No SG14 segment is found", logger.UserLogStrings.Count > 0);
		}

		public void TestProcessResponseWithoutERCinGroup14()
		{
			string rejectionResponse = @"
UNH+TRN888006+CUSRES:D:03A:UN+SRN654302'
BGM+963+45678901'
GEI+6+841:120:143'
ERP+1::80'
ERC+101::143'
DOC+ERR:148:143+1'
ERP+1:730'
UNT+8+TRN888006'
";
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "SRN654302";
			consol.JK_MasterBillNum = "08112345678";

			string messageText = @"UNH+111+CUSCAR:D:03A:UN'
BGM+833:::DEPART+SRN654304+9'
NAD+CS+40009914H:ZZZ:143'
NAD+CH++QANTAS'TDT+20++4+++++:::QF2'
LOC+5+NZWLG'
LOC+8+US'
DTM+136:20031014:102'
CNT+2:2'
CNI+1+89012345'
RFF+HWB:123456'
CNI+2+23456789'
RFF+HWB:SOC00009876'
UNT+14+111'";

			EDIMessage outgoing = consol.Messages.AddNew();
			outgoing.EM_MessageText = messageText.Replace("\r\n", "");
			outgoing.EM_ReceiveTransmit = "TRX";
			outgoing.EM_ApplicationCode = EDIInterchange.ApplicationCodes.NewZealandCustoms;
			outgoing.EM_MessageType = OutwardReportMessage.MessageTypes.OutwardReport.MessageType;

			Declaration.NZCMessage message = GetNZCMessage(rejectionResponse);
			LoggingInformation logger = new LoggingInformation();
			MessageProcessor processor = new MessageProcessor(logger);
			processor.ProcessMessage(message);

			AssertEquals("No success", "ERR", message.EM_Status);
			Assert("Logger should be reported that no ERC in SG14 exist", logger.UserLogStrings.Count > 0);
		}

		public void TestProcessAcceptanceResponse()
		{
			string acceptanceResponse = @"
UNH+TRN888005+CUSRES:D:03A:UN+SRN654301'
BGM+932+45678901'
GEI+6+847:120:143'
UNT+4+TRN888005'
";

			string acceptanceInterpreted = @"Consol Number		: SRN654301
Outward Report No	: 45678901
Master Bill			: 081-12345678

Rsp Message No		: TRN888005
Message Type		: Acceptance Notice 
Status				: Outward Report Accepted 
";
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "SRN654301";
			consol.JK_MasterBillNum = "081-12345678";
			Declaration.NZCMessage message = GetNZCMessage(acceptanceResponse);
			LoggingInformation logger = new LoggingInformation();
			MessageProcessor processor = new MessageProcessor(logger);
			processor.ProcessMessage(message);
			string result = processor.GetReport();
			AssertEmailReport("Processed", acceptanceInterpreted, result);
		}

		public void TestProcessRejectionResponse()
		{
			string rejectionResponse = @"
UNH+TRN888006+CUSRES:D:03A:UN+SRN654302'
BGM+963+45678901'
GEI+6+841:120:143'
ERP+1::80'
ERC+101::143'
DOC+ERR:148:143+1'
ERP+1:730'
ERC+678::143'
UNT+9+TRN888006'
";
			string rejectionInterpreted = @"Consol Number		: SRN654302
Outward Report No	: 45678901
Master Bill			: 08112345678

Rsp Message No		: TRN888006
Message Type		: Rejection Notice 
Status				: Outward Report Rejected

Message Errors
---------------------------------------------------------------------
--Error Found		: FlightNo. : Not specified

Shipment Responses
---------------------------------------------------------------------
HWB 123456
	--Error Found	: Clearance number : Not specified or invalid
";

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "SRN654302";
			consol.JK_MasterBillNum = "08112345678";

			string messageText = @"UNH+111+CUSCAR:D:03A:UN'
BGM+833:::DEPART+SRN654304+9'
NAD+CS+40009914H:ZZZ:143'
NAD+CH++QANTAS'TDT+20++4+++++:::QF2'
LOC+5+NZWLG'
LOC+8+US'
DTM+136:20031014:102'
CNT+2:2'
CNI+1+89012345'
RFF+HWB:123456'
CNI+2+23456789'
RFF+HWB:SOC00009876'
UNT+14+111'";

			EDIMessage outgoing = consol.Messages.AddNew();
			outgoing.EM_MessageText = messageText.Replace("\r\n", "");
			outgoing.EM_ReceiveTransmit = "TRX";
			outgoing.EM_ApplicationCode = EDIInterchange.ApplicationCodes.NewZealandCustoms;
			outgoing.EM_MessageType = OutwardReportMessage.MessageTypes.OutwardReport.MessageType;

			Declaration.NZCMessage message = GetNZCMessage(rejectionResponse);
			LoggingInformation logger = new LoggingInformation();
			MessageProcessor processor = new MessageProcessor(logger);
			processor.ProcessMessage(message);
			string result = processor.GetReport();

			AssertEmailReport("Processed", rejectionInterpreted, result);
		}

		public void TestProcessAdjustmentResponse()
		{
			string adjustmentResponse = @"
UNH+TRN888007+CUSRES:D:03A:UN+SRN654307'
BGM+965+45678901'
GEI+6+830:120:143'
UNT+4+TRN888007'
";

			string adjustmentInterpreted = @"Consol Number		: SRN654307
Outward Report No	: 45678901
Master Bill			: 08112345678
 
Rsp Message No		: TRN888007
Message Type		: Confirmation of Adjustment
Status				: Outward Report Accepted
";

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "SRN654307";
			consol.JK_MasterBillNum = "08112345678";

			Declaration.NZCMessage message = GetNZCMessage(adjustmentResponse);
			LoggingInformation logger = new LoggingInformation();
			MessageProcessor processor = new MessageProcessor(logger);
			processor.ProcessMessage(message);
			string result = processor.GetReport();
			AssertEmailReport("Processed", adjustmentInterpreted, result);
		}

		public void TestProcessAuditRequirementResponse()
		{
			string auditResponse = @"
UNH+TRN888008+CUSRES:D:03A:UN+SRN654304'
BGM+962+45678901'
FTX+ICN+++ITEM 1 - INVALID CLEARANCE NUMBER QUOTED'
GEI+6+805:120:143'
UNT+5+TRN888008'
";
			string auditRequiredInterpreted = @"Consol Number		: SRN654304
Outward Report No	: 45678901
Master Bill			: 08112345678
 
Rsp Message No		: TRN888008
Message Type		: Inspection/Audit Requirements
Status				: Customs Instuctions as specified
 
Customs Instructions
---------------------------------------------------------------------
HWB SOC76543WLG		: ITEM 1 - INVALID CLEARANCE NUMBER QUOTED
";

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "SRN654304";
			consol.JK_MasterBillNum = "08112345678";
			EDIMessage outgoing = consol.Messages.AddNew();
			string messageText = @"UNH+111+CUSCAR:D:03A:UN'
BGM+833:::DEPART+SRN654304+9'
NAD+CS+40009914H:ZZZ:143'
NAD+CH++QANTAS'TDT+20++4+++++:::QF2'
LOC+5+NZWLG'
LOC+8+US'
DTM+136:20031014:102'
CNT+2:2'
CNI+1+89012345'
RFF+HWB:SOC76543WLG'
CNI+2+23456789'
RFF+HWB:SOC00009876'
UNT+14+111'";

			outgoing.EM_MessageText = messageText.Replace("\r\n", "");
			outgoing.EM_ReceiveTransmit = "TRX";
			outgoing.EM_ApplicationCode = EDIInterchange.ApplicationCodes.NewZealandCustoms;
			outgoing.EM_MessageType = OutwardReportMessage.MessageTypes.OutwardReport.MessageType;

			Declaration.NZCMessage message = GetNZCMessage(auditResponse);
			LoggingInformation logger = new LoggingInformation();
			MessageProcessor processor = new MessageProcessor(logger);
			processor.ProcessMessage(message);
			string result = processor.GetReport();
			AssertEmailReport("Processed", auditRequiredInterpreted, result);
		}

		public void TestHandleResponseForCancel()
		{
			string response = @"
UNH+3027+CUSRES:D:03A:UN+SRN654304'
BGM+965+51717407'
GEI+6+830:120:143'
UNT+4+3027'
";
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "SRN654304";

			CusEntryNumber entryNum = Factory.New<CusEntryNumber>();
			entryNum.CE_EntryNum = "51717407";
			entryNum.CE_EntryType = CusEntryNumberTypeList.Codes.OutwardReportNumber;
			entryNum.CE_ParentID = consol.PK;
			entryNum.CE_EntryStatus = OutwardReportStatusList.Codes.Cleared;

			EDIMessage outgoing = consol.Messages.AddNew();
			outgoing.EM_ApplicationCode = EDIInterchange.ApplicationCodes.NewZealandCustoms;
			outgoing.EM_MessageType = OutwardReportMessage.MessageTypes.OutwardReport.MessageType;
			outgoing.EM_MessageSubType = OutwardReportMessage.MessageTypes.OutwardReport.MessageSubTypes.Cancellation;

			Declaration.NZCMessage message = GetNZCMessage(response);
			LoggingInformation logger = new LoggingInformation();
			MessageProcessor processor = new MessageProcessor(logger);
			processor.ProcessMessage(message);

			AssertEquals("Entry is cancelled", OutwardReportStatusList.Codes.Cancelled, entryNum.CE_EntryStatus);
		}

		protected void AssertEmailReport(string message, string expected, string acutal)
		{
			AssertMultilineEquals(message, expected.Replace(" ", "").Replace("\t", "").Replace("\r\n", "'"), acutal.Replace(" ", "").Replace("\t", "").Replace("\r\n", "'"), '\'');
		}

		protected Declaration.NZCMessage GetNZCMessage(ZString messageText)
		{
			Declaration.NZCMessage message = Factory.New<Declaration.NZCMessage>();
			message.EM_MessageText = messageText.Replace("\r", "").Replace("\n", "");
			return message;
		}
	}
}

