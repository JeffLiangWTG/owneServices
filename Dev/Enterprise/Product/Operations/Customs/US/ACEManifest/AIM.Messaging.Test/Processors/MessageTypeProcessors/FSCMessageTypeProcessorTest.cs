using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using NUnit.Framework;
using static Enterprise.Customs.US.AIM.Messaging.Constants;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.US.AIM.Messaging.Testing
{
	public class FSCMessageTypeProcessorTest : MessageTypeProcessorBaseTest
	{
		public void TestUpdateTACStatusForTransfer()
		{
			var manifestHeader = Factory.NewWithValidTestData<ACEManifest.Business.AsycudaManifestHeader>();
			manifestHeader.AMA_RN_NKCountry = "US";
			manifestHeader.AMA_TransportMode = "AIR";
			manifestHeader.AMA_MasterBill = "05259805126";
			manifestHeader.AMA_SystemCreateTimeUtc = ZDateTime.UtcNow;
			var bill = manifestHeader.Bills.AddNew();
			bill.ABL_BillNumber = "2025050801";
			var arrival = manifestHeader.ArrivalHeaders.AddNew();
			arrival.ATH_VoyageFlightNo = "99528";
			arrival.ATH_ETAAtDischargePort = new ZDate(2025, 10, 26);
			var transfer = arrival.TransferHeaders.AddNew();
			transfer.ATF_TransferType = "D";
			var transferDetail = transfer.TransferBills.AddNew();
			transferDetail.ATB_ABL_Bill = bill.PK;
			transferDetail.ATB_BillNumber = "05259805126";
			AssertEquals(ZString.Empty, transferDetail.ATB_MessageStatus);

			var fscMessage = @"FSC
JFKAA
052-59805126-002025050801
ARR/99528/26OCT
FSC/10
WBL/HKG/T1/K12/PLASTIC
ARR/99528/26OCT";

			ProcessMessage(fscMessage, manifestHeader);
			AssertEquals(AIMTransferStatusCodes.Codes.TransferAccepted, transferDetail.ATB_MessageStatus);
		}
		public void TestUpdateARVStatusForTransfer()
		{
			var fsnMessage = @"FSN
LAXZI
052-59805127-002025050802
ARR/99529/26OCT
ASN4
";

			var fscMessage = @"FSC
JFKAA
052-59805127-002025050802
ARR/99529/26OCT
FSC/10
WBL/HKG/T1/K12/PLASTIC
ARR/99529/26OCT";

			var manifestHeader = Factory.NewWithValidTestData<ACEManifest.Business.AsycudaManifestHeader>();
			manifestHeader.AMA_RN_NKCountry = "US";
			manifestHeader.AMA_TransportMode = "AIR";
			manifestHeader.AMA_MasterBill = "05259805127";
			manifestHeader.AMA_SystemCreateTimeUtc = ZDateTime.UtcNow;
			var bill = manifestHeader.Bills.AddNew();
			bill.ABL_BillNumber = "2025050802";
			var arrival = manifestHeader.ArrivalHeaders.AddNew();
			arrival.ATH_VoyageFlightNo = "99529";
			arrival.ATH_ETAAtDischargePort = new ZDate(2025, 10, 26);
			var transfer = arrival.TransferHeaders.AddNew();
			transfer.ATF_TransferType = "D";
			var transferDetail = transfer.TransferBills.AddNew();
			transferDetail.ATB_ABL_Bill = bill.PK;
			transferDetail.ATB_BillNumber = "05259805127";

			var outgoingMessage = CreateSentMessage(manifestHeader, fsnMessage, AIMMessageSubTypes.FSN);
			outgoingMessage.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
			var inboundMessage = CreateQueuedMessage(fscMessage);
			Factory.Save();

			ProcessMessage(inboundMessage, outgoingMessage, manifestHeader);
			bill.Reload();
			AssertEquals(AIMTransferStatusCodes.Codes.Arrived, transferDetail.ATB_MessageStatus);
		}

		public void TestNoExceptionThrownWhenAirWayBillArrivalIsNull()
		{
			var manifestHeader = Factory.NewWithValidTestData<ACEManifest.Business.AsycudaManifestHeader>();
			manifestHeader.AMA_RN_NKCountry = "US";
			manifestHeader.AMA_TransportMode = "AIR";
			manifestHeader.AMA_MasterBill = "05259805126";
			manifestHeader.AMA_SystemCreateTimeUtc = ZDateTime.UtcNow;
			var bill = manifestHeader.Bills.AddNew();
			bill.ABL_BillNumber = "2025050801";
			var existingArrival = manifestHeader.ArrivalHeaders.AddNew();
			existingArrival.ATH_VoyageFlightNo = "ZI004";
			existingArrival.ATH_ETAAtDischargePort = new ZDate(2021, 10, 20);
			AssertEquals("No ArrivalLine", 0, existingArrival.ArrivalDetails.Count);

			var fscMessage = @"FSC
JFKAA
052-59805126-002025050801
FSC/10
WBL/HKG/T1/K12/PLASTIC
";
			AssertNoExceptionThrown(() => ProcessMessage(fscMessage, manifestHeader));
			AssertEquals("Should use existing ArrivalHeader", 1, manifestHeader.ArrivalHeaders.Count);
			Assert("Should use existing ArrivalHeader.", manifestHeader.ArrivalHeaders[0] == existingArrival);
			AssertEquals("Should not have created ArrivalLine.", 0, existingArrival.ArrivalDetails.Count);
		}

		[TestDate(2020, 12, 7, 11, 15, 33)]
		public void TestProcessStatus_00_HAWB()
		{
			_ = ManifestHeader;
			var bill = HouseBill;

			var responseMessage = @"FSC
MIAKLM
081-11223344-HAWB123
FSC/00";
			AssertProcessFSC00(responseMessage, bill);

			var expectedPropertyTableHTML = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Property</th><th>Value</th></tr></thead><tr>" +
				@"<td>Message Time</td><td>07-Dec-20 11:15:33</td></tr><tr><td>Air Waybill Number</td><td>08111223344</td></tr><tr><td>HAWB Number</td><td>HAWB123</td></tr><tr><td>Package Tracking Identifier</td><td>&nbsp;</td></tr>" +
				@"<tr><td>Flight Number</td><td>&nbsp;</td></tr><tr><td>Part Arrival Reference</td><td>&nbsp;</td></tr><tr><td>Scheduled Arrival Date</td><td>&nbsp;</td></tr><tr><td>Status Answer Code</td><td>00 - Record not on file.</td></tr><tr><td>Information</td><td>&nbsp;</td></tr></table>";
			AssertEmailCreated(ExpectedEmailMessageType + " - 08111223344 - HAWB123", expectedPropertyTableHTML);

			responseMessage = @"FSC
MIAKLM
081-11223344-HAWB123
FSC/00";
			AssertProcessFSC00(responseMessage, bill);
		}

		[TestDate(2020, 12, 12)]
		public void TestProcessStatus_00_MAWB()
		{
			var manifestHeader = ManifestHeader;
			var bill = HouseBill;

			var responseMessage = @"FSC
MIAKLM
081-11223344
FSC/00";
			AssertProcessFSC00(responseMessage, manifestHeader.MasterBill);
		}

		[TestDate(2020, 12, 12)]
		public void TestProcessStatus_00_MAWB_Simple()
		{
			var manifestHeader = ManifestHeader;
			var bill = HouseBill;

			var responseMessage = @"FSC
081-11223344
FSC/00";
			AssertProcessFSC00(responseMessage, manifestHeader.MasterBill);
		}

		[TestDate(2020, 12, 12)]
		public void TestDoNotShowEmptyDestPort()
		{
			var manifestHeader = ManifestHeader;
			manifestHeader.AMA_Voyage = "KL888";
			manifestHeader.AMA_E_ARV = new ZDateTime(2020, 12, 12, 08, 15, 00);
			manifestHeader.AMA_RL_NKPortOfLoading = "AUSYD";
			manifestHeader.AMA_RL_NKPortOfDischarge = "USLAX";
			var bill = HouseBill;
			bill.ABL_BillNumber = "HAWB123";

			// Below message example should not show the Destination Port column.
			var responseMessage = @"FSC
MIAKLM
081-11223344
ARR/KL888/14DEC
FSC/10
WBL/KHH/T50/K680.0/SHOES/24OCT
/SOCKS
ARR/ZZ0100/24OCT B10/K50.0
TRN/ORD-D//97000006
TRN/ORD-D//J999
";
			AssertProcessFSC10(responseMessage, ManifestHeader.MasterBill, "KL888", new ZDateTime(2020, 12, 14), "AUSYD", "USLAX");
			var expectedPropertyTableHTML = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Property</th><th>Value</th></tr></thead><tr><td>Message Time</td><td>12-Dec-20 00:00:00</td></tr><tr><td>Air Waybill Number</td><td>08111223344</td></tr><tr><td>HAWB Number</td><td>&nbsp;</td></tr><tr><td>Package Tracking Identifier</td><td>&nbsp;</td></tr><tr><td>Flight Number</td><td>KL888</td></tr><tr><td>Part Arrival Reference</td><td>&nbsp;</td></tr><tr><td>Scheduled Arrival Date</td><td>14-Dec-20</td></tr><tr><td>Status Answer Code</td><td>10 - Current Air Waybill information on file follows.</td></tr><tr><td>Information</td><td>&nbsp;</td></tr></table>";
			var expectedExtraHTML = @"<P><B>FSC Code 10 - Air Waybill</B></P><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Port of Origin</th></tr></thead><tr><td>KHH</td></tr></table>";
			AssertEmailCreated(ExpectedEmailMessageType + " - 08111223344", expectedPropertyTableHTML, expectedExtraHTML);
		}

		[TestDate(2021, 08, 12)]
		public void TestShortFlightNumber()
		{
			var manifestHeader = ManifestHeader;
			manifestHeader.AMA_Voyage = "UA1";
			manifestHeader.AMA_E_ARV = ZDateTime.Today;
			manifestHeader.AMA_RL_NKPortOfLoading = "AUSYD";
			manifestHeader.AMA_RL_NKPortOfDischarge = "USLAX";
			var bill = HouseBill;
			bill.ABL_BillNumber = "HAWB123";

			var responseMessage = @"FSC
MIAKLM
081-11223344
ARR/UA1/12AUG
FSC/10
WBL/KHH/T50/K680.0/SHOES/24OCT
/SOCKS
ARR/UA1/12AUG B10/K50.0
TRN/ORD-D//97000006
TRN/ORD-D//J999
";
			AssertProcessFSC10(responseMessage, ManifestHeader.MasterBill, "UA1", new ZDateTime(2021, 08, 12), "AUSYD", "USLAX");
		}

		void AssertProcessFSC00(ZString responseMessage, AsycudaBill bill)
		{
			bill.ABL_BillStatus = "1C";
			bill.ABL_MessageStatus = MessageStatusCodeList.Codes.Sent;

			BusinessObject messageParent = bill == ManifestHeader.MasterBill ? ManifestHeader : bill;
			ProcessMessage(responseMessage, messageParent);

			bill.Reload();
			AssertEquals("ABL_BillStatus", ZString.Empty, bill.ABL_BillStatus);
			AssertEquals("ABL_MessageStatus", ZString.Empty, bill.ABL_MessageStatus);
		}

		[TestDate(2020, 12, 7, 11, 15, 33)]
		public void TestProcessStatus_02_HAWB()
		{
			var manifestHeader = ManifestHeader;
			var bill = HouseBill;
			var arrivalA = manifestHeader.ArrivalHeaders.AddNew();
			arrivalA.ATH_Reference = "";
			arrivalA.ATH_VoyageFlightNo = "QF001";
			arrivalA.ATH_ETAAtDischargePort = new ZDateTime(2020, 12, 22);
			var arrivalLineA = arrivalA.ArrivalDetails.AddNew();
			arrivalLineA.ATL_ABL_AsycudaBill = bill.PK;
			arrivalLineA.ATL_MessageStatus = ZString.Empty;

			var responseMessage = @"FSC
MIAKLM
081-11223344-HAWB123
ARR/KL325/12DEC
FSC/02
TXT/BILL IS SPLIT
/A-KL325/23DEC B1
/B-PA014/23DEC B5
/C-KL325/24DEC B15";

			ProcessMessage(responseMessage, bill);

			manifestHeader.Reload();
			AssertEquals("ArrivalHeaders Count", 4, manifestHeader.ArrivalHeaders.Count);
			AssertArrivalEntry("QF001", new ZDateTime(2020, 12, 22), "", bill.PK, 0, "");
			AssertArrivalEntry("KL325", new ZDateTime(2020, 12, 23), "", bill.PK, 1, "A");
			AssertArrivalEntry("PA014", new ZDateTime(2020, 12, 23), "", bill.PK, 5, "B");
			AssertArrivalEntry("KL325", new ZDateTime(2020, 12, 24), "", bill.PK, 15, "C");

			var expectedPropertyTableHTML = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Property</th><th>Value</th></tr></thead><tr>" +
				@"<td>Message Time</td><td>07-Dec-20 11:15:33</td></tr><tr><td>Air Waybill Number</td><td>08111223344</td></tr><tr><td>HAWB Number</td><td>HAWB123</td></tr><tr><td>Package Tracking Identifier</td><td>&nbsp;</td></tr>" +
				@"<tr><td>Flight Number</td><td>KL325</td></tr><tr><td>Part Arrival Reference</td><td>&nbsp;</td></tr><tr><td>Scheduled Arrival Date</td><td>12-Dec-20</td></tr><tr><td>Status Answer Code</td><td>02 - Bill is split - see list below.</td></tr><tr><td>Information</td><td>BILL IS SPLIT</td></tr></table>";
			var expectedExtraHTML = @"<P><B>FSC Code 02 - Split Bill</B></P><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Part Arrival Reference</th><th>Flight Number</th><th>Scheduled Arrival Date</th><th>Boarded Pieces</th></tr></thead><tr><td>A</td><td>KL325</td><td>23-Dec-20</td><td>1</td></tr><tr><td>B</td><td>PA014</td><td>23-Dec-20</td><td>5</td></tr><tr><td>C</td><td>KL325</td><td>24-Dec-20</td><td>15</td></tr></table>";
			AssertEmailCreated(ExpectedEmailMessageType + " - 08111223344 - HAWB123", expectedPropertyTableHTML, expectedExtraHTML);
		}

		[TestDate(2020, 12, 7, 11, 15, 33)]
		public void TestProcessStatus_02_MAWB()
		{
			var manifestHeader = ManifestHeader;
			var bill = HouseBill;

			var responseMessage = @"FSC
MIAKLM
081-11223344
ARR/KL325/12DEC
FSC/02
TXT/BILL IS SPLIT
/A-KL325/23DEC
/B-PA014/23DEC
/C-KL325/24DEC
";

			ProcessMessage(responseMessage, manifestHeader);

			manifestHeader.Reload();
			AssertEquals("ArrivalHeaders Count", 3, manifestHeader.ArrivalHeaders.Count);
			var masterBill = manifestHeader.MasterBill;
			AssertArrivalEntry("KL325", new ZDateTime(2020, 12, 23), "A", masterBill.PK, 0, "A");
			AssertArrivalEntry("PA014", new ZDateTime(2020, 12, 23), "B", masterBill.PK, 0, "B");
			AssertArrivalEntry("KL325", new ZDateTime(2020, 12, 24), "C", masterBill.PK, 0, "C");

			var expectedPropertyTableHTML = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Property</th><th>Value</th></tr></thead><tr>" +
				@"<td>Message Time</td><td>07-Dec-20 11:15:33</td></tr><tr><td>Air Waybill Number</td><td>08111223344</td></tr><tr><td>HAWB Number</td><td>&nbsp;</td></tr><tr><td>Package Tracking Identifier</td><td>&nbsp;</td></tr>" +
				@"<tr><td>Flight Number</td><td>KL325</td></tr><tr><td>Part Arrival Reference</td><td>&nbsp;</td></tr><tr><td>Scheduled Arrival Date</td><td>12-Dec-20</td></tr><tr><td>Status Answer Code</td><td>02 - Bill is split - see list below.</td></tr><tr><td>Information</td><td>BILL IS SPLIT</td></tr></table>";
			var expectedExtraHTML = @"<P><B>FSC Code 02 - Split Bill</B></P><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Part Arrival Reference</th><th>Flight Number</th><th>Scheduled Arrival Date</th><th>Boarded Pieces</th></tr></thead><tr><td>A</td><td>KL325</td><td>23-Dec-20</td><td>0</td></tr><tr><td>B</td><td>PA014</td><td>23-Dec-20</td><td>0</td></tr><tr><td>C</td><td>KL325</td><td>24-Dec-20</td><td>0</td></tr></table>";
			AssertEmailCreated(ExpectedEmailMessageType + " - 08111223344", expectedPropertyTableHTML, expectedExtraHTML);
		}

		[TestDate(2020, 12, 7, 11, 15, 33)]
		public void TestProcessStatus_07()
		{
			var manifestHeader = ManifestHeader;
			var bill = HouseBill;

			var responseMessage = @"FSC
MIAKLM
081-11223344
ARR/KL888/14DEC
FSC/07
TXT/BILL STATUS INFORMATION
/NO ENTRY FILED
/ENTRY XXX-XXXXXXXX FILED
/ENTRY FOR LESS THAN WBL QTY (not operational)
/MULTIPLE ENTRIES ON FILE";
			ProcessMessage(responseMessage, manifestHeader);

			ManifestHeader.Reload();
			AssertEquals("AMA_Voyage", "QF101", ManifestHeader.AMA_Voyage);
			AssertEquals("AMA_E_ARV", new ZDateTime(2020, 12, 12), ManifestHeader.AMA_E_ARV);

			var expectedPropertyTableHTML = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Property</th><th>Value</th></tr></thead><tr>" +
				@"<td>Message Time</td><td>07-Dec-20 11:15:33</td></tr><tr><td>Air Waybill Number</td><td>08111223344</td></tr><tr><td>HAWB Number</td><td>&nbsp;</td></tr><tr><td>Package Tracking Identifier</td><td>&nbsp;</td></tr>" +
				@"<tr><td>Flight Number</td><td>KL888</td></tr><tr><td>Part Arrival Reference</td><td>&nbsp;</td></tr><tr><td>Scheduled Arrival Date</td><td>14-Dec-20</td></tr><tr><td>Status Answer Code</td><td>07 - Bill status information follows.</td></tr><tr><td>Information</td><td>BILL STATUS INFORMATION</td></tr></table>";
			var expectedExtraHTML = @"<P><B>FSC Code 07 - Bill Status Information</B></P><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Conditions</th></tr></thead><tr><td>BILL STATUS INFORMATION</td></tr><tr><td>NO ENTRY FILED</td></tr><tr><td>ENTRY XXX-XXXXXXXX FILED</td></tr><tr><td>ENTRY FOR LESS THAN WBL QTY (not operational)</td></tr><tr><td>MULTIPLE ENTRIES ON FILE</td></tr></table>";
			AssertEmailCreated(ExpectedEmailMessageType + " - 08111223344", expectedPropertyTableHTML, expectedExtraHTML);
		}

		[TestDate(2020, 12, 7, 11, 15, 33)]
		public void TestConditions()
		{
			var manifestHeader = ManifestHeader;
			var bill = HouseBill;
			var responseMessage = @"FSC
LAXZI
081-11223344
ARR/ZI002/06JUL
FSC/07
TXT/NO ENTRY FILED
TXT/MASTER AWB";
			ProcessMessage(responseMessage, manifestHeader);

			ManifestHeader.Reload();
			var expectedPropertyTableHTML = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Property</th><th>Value</th></tr></thead><tr><td>Message Time</td><td>07-Dec-20 11:15:33</td></tr><tr><td>Air Waybill Number</td><td>08111223344</td></tr><tr><td>HAWB Number</td><td>&nbsp;</td></tr><tr><td>Package Tracking Identifier</td><td>&nbsp;</td></tr><tr><td>Flight Number</td><td>ZI002</td></tr><tr><td>Part Arrival Reference</td><td>&nbsp;</td></tr><tr><td>Scheduled Arrival Date</td><td>06-Jul-21</td></tr><tr><td>Status Answer Code</td><td>07 - Bill status information follows.</td></tr><tr><td>Information</td><td>NO ENTRY FILED</td></tr></table>";
			var expectedExtraHTML = @"<P><B>FSC Code 07 - Bill Status Information</B></P><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Conditions</th></tr></thead><tr><td>NO ENTRY FILED</td></tr><tr><td>MASTER AWB</td></tr></table>";
			AssertEmailCreated(ExpectedEmailMessageType + " - 08111223344", expectedPropertyTableHTML, expectedExtraHTML);
		}

		[TestDate(2020, 12, 7, 11, 15, 33)]
		public void TestProcessStatus_08_Full()
		{
			var manifestHeader = ManifestHeader;
			manifestHeader.AMA_Voyage = "QF101";
			manifestHeader.AMA_E_ARV = new ZDateTime(2020, 12, 12);
			manifestHeader.AMA_RL_NKPortOfFirstArrival = "USLAX";
			manifestHeader.AMA_RL_NKPortOfDischarge = "USCHI";
			var bill = HouseBill;

			var responseMessage = @"FSC
MIAKLM
081-11223344
ARR/KL888/14DEC
FSC/08
TXT/ARR-ANC PTP-ORD INB-ORDJFK-A";
			AssertProcessFSC08(responseMessage, "KL888", new ZDateTime(2020, 12, 14), "USANC", "USORD");

			var expectedPropertyTableHTML = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Property</th><th>Value</th></tr></thead><tr>" +
				@"<td>Message Time</td><td>07-Dec-20 11:15:33</td></tr><tr><td>Air Waybill Number</td><td>08111223344</td></tr><tr><td>HAWB Number</td><td>&nbsp;</td></tr><tr><td>Package Tracking Identifier</td><td>&nbsp;</td></tr>" +
				@"<tr><td>Flight Number</td><td>KL888</td></tr><tr><td>Part Arrival Reference</td><td>&nbsp;</td></tr><tr><td>Scheduled Arrival Date</td><td>14-Dec-20</td></tr><tr><td>Status Answer Code</td><td>08 - Routing information follows.</td></tr><tr><td>Information</td><td>ARR-ANC PTP-ORD INB-ORDJFK-A</td></tr></table>";

			// Status_08 (Routing Information) should not display any Destination Port (WI00404083)
			var expectedExtraHTML = @"<P><B>FSC Code 08 - Routing Information</B></P><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Arrival Port</th></tr></thead>" +
				@"<tr><td>ANC</td></tr></table>";

			AssertEmailCreated(ExpectedEmailMessageType + " - 08111223344", expectedPropertyTableHTML, expectedExtraHTML);
		}

		[TestDate(2020, 12, 12)]
		public void TestProcessStatus_08_WithPartReference()
		{
			var manifestHeader = ManifestHeader;
			manifestHeader.AMA_Voyage = "QF101";
			manifestHeader.AMA_E_ARV = new ZDateTime(2020, 12, 12);
			manifestHeader.AMA_RL_NKPortOfFirstArrival = "USLAX";
			manifestHeader.AMA_RL_NKPortOfDischarge = "USCHI";
			var bill = HouseBill;

			var responseMessage = @"FSC
MIAKLM
081-11223344
ARR/KL888/14DEC-A
FSC/08
TXT/ARR-ANC PTP-ORD";
			AssertProcessFSC08(responseMessage, "QF101", new ZDateTime(2020, 12, 12), "USLAX", "USCHI");

			AssertEquals("ArrivalHeaders Count", 1, manifestHeader.ArrivalHeaders.Count);
			AssertArrivalEntry("KL888", new ZDateTime(2020, 12, 14), "A", manifestHeader.MasterBill.PK, 0, "A");
		}

		[TestDate(2020, 12, 12)]
		public void TestProcessStatus_08_WithoutPartReference()
		{
			var manifestHeader = ManifestHeader;
			manifestHeader.AMA_Voyage = "QF101";
			manifestHeader.AMA_E_ARV = new ZDateTime(2020, 12, 12);
			manifestHeader.AMA_RL_NKPortOfFirstArrival = "USLAX";
			manifestHeader.AMA_RL_NKPortOfDischarge = "USCHI";

			_ = HouseBill;

			var responseMessage = @"FSC
MIAKLM
081-11223344
ARR/KL888/14DEC
FSC/08
TXT/ARR-ANC PTP-ORD
";
			AssertProcessFSC08(responseMessage, "KL888", new ZDateTime(2020, 12, 14), "USANC", "USORD");
		}

		[TestDate(2020, 12, 12)]
		public void TestProcessStatus_08_WithoutDischargePort()
		{
			var manifestHeader = ManifestHeader;
			manifestHeader.AMA_Voyage = "QF101";
			manifestHeader.AMA_E_ARV = new ZDateTime(2020, 12, 12);
			manifestHeader.AMA_RL_NKPortOfLoading = "AUSYD";
			manifestHeader.AMA_RL_NKPortOfFirstArrival = "";
			manifestHeader.AMA_RL_NKPortOfDischarge = "USLAX";

			_ = HouseBill;

			var responseMessage = @"FSC
MIAKLM
081-11223344
ARR/KL888/14DEC
FSC/08
TXT/ARR-ANC INB-ORDJFK T1
";
			AssertProcessFSC08(responseMessage, "KL888", new ZDateTime(2020, 12, 14), "", "USANC");
		}

		[TestDate(2020, 12, 12)]
		public void TestProcessStatus_08_Short()
		{
			var manifestHeader = ManifestHeader;
			manifestHeader.AMA_Voyage = "QF101";
			manifestHeader.AMA_E_ARV = new ZDateTime(2020, 12, 14, 08, 15, 00);
			manifestHeader.AMA_RL_NKPortOfFirstArrival = "USLAX";
			manifestHeader.AMA_RL_NKPortOfDischarge = "USCHI";

			_ = HouseBill;

			var responseMessage = @"FSC
MIAKLM
081-11223344
ARR/KL888/14DEC
FSC/08
TXT/ARR-ANC
";
			AssertProcessFSC08(responseMessage, "KL888", new ZDateTime(2020, 12, 14), "USANC", "USANC");
		}

		[TestDate(2020, 12, 12)]
		public void TestProcessStatus_08_Short_ArrivalTimePreserved()
		{
			var manifestHeader = ManifestHeader;
			manifestHeader.AMA_Voyage = "KL888";
			manifestHeader.AMA_E_ARV = new ZDateTime(2020, 12, 14, 08, 15, 00);
			manifestHeader.AMA_RL_NKPortOfFirstArrival = "";
			manifestHeader.AMA_RL_NKPortOfDischarge = "USCHI";

			_ = HouseBill;

			var responseMessage = @"FSC
MIAKLM
081-11223344
ARR/KL888/14DEC
FSC/08
TXT/ARR-ANC
";
			AssertProcessFSC08(responseMessage, "KL888", new ZDateTime(2020, 12, 14, 08, 15, 00), "", "USANC");
		}

		[TestDate(2020, 12, 12)]
		public void TestProcessStatus_08_WithoutFlightDetails()
		{
			var manifestHeader = ManifestHeader;
			manifestHeader.AMA_Voyage = "KL888";
			manifestHeader.AMA_E_ARV = new ZDateTime(2020, 12, 14, 08, 15, 00);
			manifestHeader.AMA_RL_NKPortOfFirstArrival = "";
			manifestHeader.AMA_RL_NKPortOfDischarge = "USCHI";

			_ = HouseBill;

			var responseMessage = @"FSC
MIAKLM
081-11223344
FSC/08
TXT/ARR-ANC
";
			AssertProcessFSC08(responseMessage, "KL888", new ZDateTime(2020, 12, 14, 08, 15, 00), "", "USANC");
		}

		void AssertProcessFSC08(ZString responseMessage, ZString flight, ZDateTime arrivalTime, ZString arrivalPort, ZString dischargePort)
		{
			ProcessMessage(responseMessage, ManifestHeader);

			ManifestHeader.Reload();

			AssertEquals("AMA_Voyage", flight, ManifestHeader.AMA_Voyage);
			AssertEquals("AMA_E_ARV", arrivalTime, ManifestHeader.AMA_E_ARV);
			AssertEquals("AMA_RL_NKPortOfFirstArrival", arrivalPort, ManifestHeader.AMA_RL_NKPortOfFirstArrival);
			AssertEquals("AMA_RL_NKPortOfDischarge", dischargePort, ManifestHeader.AMA_RL_NKPortOfDischarge);
		}

		[TestDate(2020, 12, 7, 11, 15, 33)]
		public void TestProcessStatus_10_HAWB()
		{
			var manifestHeader = ManifestHeader;
			manifestHeader.AMA_Voyage = "QF101";
			manifestHeader.AMA_E_ARV = new ZDateTime(2020, 12, 14, 08, 15, 00);
			manifestHeader.AMA_RL_NKPortOfLoading = "AUSYD";
			manifestHeader.AMA_RL_NKPortOfDischarge = "USLAX";
			var bill = HouseBill;

			var responseMessage = @"FSC
MIAKLM
081-11223344-HAWB123
ARR/KL888/14DEC
FSC/10
WBL/KHHSFO/T50/K680.0/SHOES/24OCT
/SOCKS
ARR/ZZ0100/24OCT B10/K50.0
TRN/ORD-D//97000006
TRN/ORD-D//J999
";
			AssertProcessFSC10(responseMessage, bill, "KL888", new ZDateTime(2020, 12, 14), "AUSYD", "USSFO");

			var expectedPropertyTableHTML = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Property</th><th>Value</th></tr></thead><tr>" +
				@"<td>Message Time</td><td>07-Dec-20 11:15:33</td></tr><tr><td>Air Waybill Number</td><td>08111223344</td></tr><tr><td>HAWB Number</td><td>HAWB123</td></tr><tr><td>Package Tracking Identifier</td><td>&nbsp;</td></tr>" +
				@"<tr><td>Flight Number</td><td>KL888</td></tr><tr><td>Part Arrival Reference</td><td>&nbsp;</td></tr><tr><td>Scheduled Arrival Date</td><td>14-Dec-20</td></tr><tr><td>Status Answer Code</td><td>10 - Current Air Waybill information on file follows.</td></tr><tr><td>Information</td><td>&nbsp;</td></tr></table>";
			var expectedExtraHTML = @"<P><B>FSC Code 10 - Air Waybill</B></P><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Port of Origin</th><th>Destination Port</th></tr></thead>" +
				@"<tr><td>KHH</td><td>SFO</td></tr></table>";
			AssertEmailCreated(ExpectedEmailMessageType + " - 08111223344 - HAWB123", expectedPropertyTableHTML, expectedExtraHTML);
		}

		[TestDate(2020, 12, 7, 11, 15, 33)]
		public void TestProcessStatus_10_HAWB_WithPartReference()
		{
			var manifestHeader = ManifestHeader;
			manifestHeader.AMA_Voyage = "QF101";
			manifestHeader.AMA_E_ARV = new ZDateTime(2020, 12, 12);
			manifestHeader.AMA_RL_NKPortOfLoading = "AUSYD";
			manifestHeader.AMA_RL_NKPortOfDischarge = "USLAX";
			var bill = HouseBill;
			bill.ABL_BillNumber = "HAWB123";

			var responseMessage = @"FSC
MIAKLM
081-11223344-HAWB123
ARR/KL888/14DEC-A
FSC/10
WBL/KHHSFO/T50/K680.0/SHOES/24OCT
/SOCKS
ARR/ZZ0100/24OCT-A/B10/K50.0
TRN/ORD-D//97000006
TRN/ORD-D//J999
";
			AssertProcessFSC10(responseMessage, bill, "QF101", new ZDateTime(2020, 12, 12), "AUSYD", "USLAX");

			AssertEquals("ArrivalHeaders Count", 1, manifestHeader.ArrivalHeaders.Count);
			AssertArrivalEntry("ZZ0100", new ZDateTime(2020, 10, 24), "", bill.PK, 10, "A");

			var expectedPropertyTableHTML = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Property</th><th>Value</th></tr></thead><tr>" +
				@"<td>Message Time</td><td>07-Dec-20 11:15:33</td></tr><tr><td>Air Waybill Number</td><td>08111223344</td></tr><tr><td>HAWB Number</td><td>HAWB123</td></tr><tr><td>Package Tracking Identifier</td><td>&nbsp;</td></tr>" +
				@"<tr><td>Flight Number</td><td>KL888</td></tr><tr><td>Part Arrival Reference</td><td>A</td></tr><tr><td>Scheduled Arrival Date</td><td>14-Dec-20</td></tr><tr><td>Status Answer Code</td><td>10 - Current Air Waybill information on file follows.</td></tr><tr><td>Information</td><td>&nbsp;</td></tr></table>";
			var expectedExtraHTML = @"<P><B>FSC Code 10 - Air Waybill</B></P><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Port of Origin</th><th>Destination Port</th></tr></thead>" +
				@"<tr><td>KHH</td><td>SFO</td></tr></table>";
			AssertEmailCreated(ExpectedEmailMessageType + " - 08111223344 - HAWB123", expectedPropertyTableHTML, expectedExtraHTML);
		}

		[TestDate(2020, 12, 12)]
		public void TestProcessStatus_10_MAWB()
		{
			var manifestHeader = ManifestHeader;
			manifestHeader.AMA_Voyage = "QF101";
			manifestHeader.AMA_E_ARV = new ZDateTime(2020, 12, 12, 08, 15, 00);
			manifestHeader.AMA_RL_NKPortOfLoading = "AUSYD";
			manifestHeader.AMA_RL_NKPortOfDischarge = "USLAX";
			var bill = HouseBill;

			var responseMessage = @"FSC
MIAKLM
081-11223344
ARR/KL888/14DEC
FSC/10
WBL/KHHSFO/T50/K680.0/SHOES/24OCT
/SOCKS
ARR/ZZ0100/24OCT B10/K50.0
TRN/ORD-D//97000006
TRN/ORD-D//J999
";
			AssertProcessFSC10(responseMessage, ManifestHeader.MasterBill, "KL888", new ZDateTime(2020, 12, 14), "AUSYD", "USSFO");
		}

		void AssertProcessFSC10(ZString responseMessage, AsycudaBill bill, ZString flight, ZDateTime arrivalTime, ZString loadPort, ZString dischargePort)
		{
			BusinessObject messageParent = bill == ManifestHeader.MasterBill ? ManifestHeader : bill;
			ProcessMessage(responseMessage, messageParent);

			ManifestHeader.Reload();
			AssertEquals("AMA_Voyage", flight, ManifestHeader.AMA_Voyage);
			AssertEquals("AMA_E_ARV", arrivalTime, ManifestHeader.AMA_E_ARV);
			AssertEquals("AMA_RL_NKPortOfLoading", loadPort, ManifestHeader.AMA_RL_NKPortOfLoading);
			AssertEquals("AMA_RL_NKPortOfDischarge", dischargePort, ManifestHeader.AMA_RL_NKPortOfDischarge);
			AssertEquals("ABL_MessageStatus", MessageStatusCodeList.Codes.Registered, bill.ABL_MessageStatus);
		}

		void AssertArrivalEntry(ZString voyageFlight, ZDateTime eta, ZString arrivalReference, ZGuid billPk, ZDecimal boardedPieces, ZString arrivalLineReference)
		{
			var arrivalName = $"Arrival {voyageFlight} {eta}";

			var manifestHeader = ManifestHeader;
			var arrival = manifestHeader.ArrivalHeaders.FirstOrDefault(a => a.ATH_VoyageFlightNo == voyageFlight && a.ATH_ETAAtDischargePort == eta);
			AssertNotNull(arrivalName, arrival);
			AssertEquals(arrivalName + " ATH_Reference", arrivalReference, arrival.ATH_Reference);

			AssertEquals(arrivalName + " ArrivalDetails Count", 1, arrival.ArrivalDetails.Count);
			var arrivalDetail = arrival.ArrivalDetails[0];
			AssertEquals(arrivalName + " line ATL_ABL_AsycudaBill", billPk, arrivalDetail.ATL_ABL_AsycudaBill);
			AssertEquals(arrivalName + " line ATL_Quantity", boardedPieces, (ZDecimal)arrivalDetail.ATL_Quantity);
			AssertEquals(arrivalName + " line ATL_Reference", arrivalLineReference, arrivalDetail.ATL_Reference);
		}

		#region Arrivals & Arrival Line

		[TestDate(2021, 9, 7)]
		public void TestProcessStatus_10_ArrivalCreating()
		{
			var manifestHeader = ManifestHeader;
			var bill = HouseBill;
			AssertEquals("Should not have created ArrivalHeaders yet.", 0, manifestHeader.ArrivalHeaders.Count);
			var fsc10messageEmptyReference = @"FSC
JFKZI
081-11223344-HAWB123
ARR/ZI003/19OCT
FSC/10
WBL/SYD/T3/K3/CONSOLIDATION
ARR/ZI004/20OCT
TRN/ORD-D/UAL/990000000
";
			ProcessMessage(fsc10messageEmptyReference, bill);
			AssertEquals("Should have created ArrivalHeader from message", 1, manifestHeader.ArrivalHeaders.Count);
			var createdArrival = manifestHeader.ArrivalHeaders[0];
			AssertEquals("Populate FlightNo from message.", "ZI004", createdArrival.ATH_VoyageFlightNo);
			AssertEquals("Populate Empty Reference.", ZString.Empty, createdArrival.ATH_Reference);
			AssertEquals("Populate ETA from message.", new ZDate(2021, 10, 20), createdArrival.ATH_ETAAtDischargePort);

			var newArrLine = createdArrival.ArrivalDetails.FirstOrDefault();
			AssertEquals("ATL_ABL_AsycudaBill", bill.PK, newArrLine.ATL_ABL_AsycudaBill);
			AssertEquals("ATH_ArrivalReference", "", newArrLine.ATL_Reference);
		}

		[TestDate(2021, 9, 7)]
		public void TestProcessStatus_10_ArrivalCreating_MAWBReference()
		{
			var manifestHeader = ManifestHeader;
			AssertEquals("Should not have created ArrivalHeaders yet.", 0, manifestHeader.ArrivalHeaders.Count);
			var fsc10messageWithReference = @"FSC
JFKZI
081-11223344
ARR/ZI003/19OCT
FSC/10
WBL/SYD/T3/K3/CONSOLIDATION
ARR/ZI004/20OCT-B/B2/K2
";
			ProcessMessage(fsc10messageWithReference, manifestHeader);
			AssertEquals("Should have created ArrivalHeader from message", 1, manifestHeader.ArrivalHeaders.Count);
			var createdArrival = manifestHeader.ArrivalHeaders[0];
			AssertEquals("Populate FlightNo from message.", "ZI004", createdArrival.ATH_VoyageFlightNo);
			AssertEquals("Populate ETA from message.", new ZDate(2021, 10, 20), createdArrival.ATH_ETAAtDischargePort);
			AssertEquals("Populate Header Reference from Arrival", "B", createdArrival.ATH_Reference);

			var newArrLine = createdArrival.ArrivalDetails.FirstOrDefault();
			AssertEquals("ATL_ABL_AsycudaBill is masterbill", manifestHeader.MasterBill.PK, newArrLine.ATL_ABL_AsycudaBill);
			AssertEquals("ATH_ArrivalReference from Arrival", "B", newArrLine.ATL_Reference);
		}

		[TestDate(2021, 9, 7)]
		public void TestProcessStatus_10_ArrivalCreating_HAWBReference()
		{
			var manifestHeader = ManifestHeader;
			var bill = HouseBill;
			AssertEquals("Should not have created ArrivalHeaders yet.", 0, manifestHeader.ArrivalHeaders.Count);
			var fsc10messageWithReference = @"FSC
JFKZI
081-11223344-HAWB123-B
ARR/ZI003/19OCT
FSC/10
WBL/SYD/T3/K3/CONSOLIDATION
ARR/ZI004/20OCT-B/B2/K2
";
			ProcessMessage(fsc10messageWithReference, bill);
			AssertEquals("Should have created ArrivalHeader from message", 1, manifestHeader.ArrivalHeaders.Count);
			var createdArrival = manifestHeader.ArrivalHeaders[0];
			AssertEquals("Populate FlightNo from message.", "ZI004", createdArrival.ATH_VoyageFlightNo);
			AssertEquals("Populate ETA from message.", new ZDate(2021, 10, 20), createdArrival.ATH_ETAAtDischargePort);
			AssertEquals("Header Reference is Empty", ZString.Empty, createdArrival.ATH_Reference);

			var newArrLine = createdArrival.ArrivalDetails.FirstOrDefault();
			AssertEquals("ATL_ABL_AsycudaBill", bill.PK, newArrLine.ATL_ABL_AsycudaBill);
			AssertEquals("ATH_ArrivalReference from Arrival when provided", "B", newArrLine.ATL_Reference);
		}

		[TestDate(2021, 9, 7)]
		public void TestProcessStatus_10_ArrivalCreating_NoConflictingArrivalDate()
		{
			var manifestHeader = ManifestHeader;
			var existingArrival = manifestHeader.ArrivalHeaders.AddNew();
			existingArrival.ATH_VoyageFlightNo = "ZI004";
			existingArrival.ATH_ETAAtDischargePort = new ZDate(2021, 10, 19);
			AssertEquals("1 existing ArrivalHeader with empty reference.", 1, manifestHeader.ArrivalHeaders.Count);
			var fsc10messageWithReference = @"FSC
JFKZI
081-11223344-HAWB123-B
ARR/ZI003/19OCT
FSC/10
WBL/SYD/T3/K3/CONSOLIDATION
ARR/ZI004/20OCT/B2/K2
";
			ProcessMessage(fsc10messageWithReference, ManifestHeader);
			AssertEquals("Should have created ArrivalHeader from message", 2, manifestHeader.ArrivalHeaders.Count);
			var createdArrival = manifestHeader.ArrivalHeaders.FirstOrDefault(arrival => arrival != existingArrival);
			AssertEquals("Populate FlightNo from message.", "ZI004", createdArrival.ATH_VoyageFlightNo);
			AssertEquals("Populate ETA from message.", new ZDate(2021, 10, 20), createdArrival.ATH_ETAAtDischargePort);
		}

		[TestDate(2021, 9, 7)]
		public void TestProcessStatus_10_ArrivalCreating_NoConflictingArrivalFlight()
		{
			var manifestHeader = ManifestHeader;
			var existingArrival = manifestHeader.ArrivalHeaders.AddNew();
			existingArrival.ATH_VoyageFlightNo = "ZI003";
			existingArrival.ATH_ETAAtDischargePort = new ZDate(2021, 10, 20);
			AssertEquals("1 existing ArrivalHeader with empty reference.", 1, manifestHeader.ArrivalHeaders.Count);
			var fsc10messageWithReference = @"FSC
JFKZI
081-11223344-HAWB123-B
ARR/ZI003/19OCT
FSC/10
WBL/SYD/T3/K3/CONSOLIDATION
ARR/ZI004/20OCT/B2/K2
";
			ProcessMessage(fsc10messageWithReference, ManifestHeader);
			AssertEquals("Should have created ArrivalHeader from message", 2, manifestHeader.ArrivalHeaders.Count);
			var createdArrival = manifestHeader.ArrivalHeaders.FirstOrDefault(arrival => arrival != existingArrival);
			AssertEquals("Populate FlightNo from message.", "ZI004", createdArrival.ATH_VoyageFlightNo);
			AssertEquals("Populate ETA from message.", new ZDateTime(2021, 10, 20), createdArrival.ATH_ETAAtDischargePort);
		}

		[TestDate(2021, 9, 7)]
		public void TestProcessStatus_10_ArrivalExisting_EmptyReference()
		{
			var manifestHeader = ManifestHeader;
			var existingArrival = manifestHeader.ArrivalHeaders.AddNew();
			existingArrival.ATH_VoyageFlightNo = "ZI004";
			existingArrival.ATH_ETAAtDischargePort = new ZDate(2021, 10, 20);
			AssertEquals("Should have 1 existing ArrivalHeader.", 1, manifestHeader.ArrivalHeaders.Count);

			var fsc10messageWithoutReference = @"FSC
JFKZI
081-11223344-HAWB123
ARR/ZI003/19OCT
FSC/10
WBL/SYD/T3/K3/CONSOLIDATION
ARR/ZI004/20OCT
TRN/ORD-D/UAL/990000000
";

			ProcessMessage(fsc10messageWithoutReference, HouseBill);
			AssertEquals("Should use existing ArrivalHeader.", 1, manifestHeader.ArrivalHeaders.Count);
			AssertSame(existingArrival, manifestHeader.ArrivalHeaders[0]);
		}

		[TestDate(2021, 9, 7)]
		public void TestProcessStatus_10_ArrivalExisting_WithReference()
		{
			var manifestHeader = ManifestHeader;
			var existingArrivalWithReference = manifestHeader.ArrivalHeaders.AddNew();
			existingArrivalWithReference.ATH_VoyageFlightNo = "ZI004";
			existingArrivalWithReference.ATH_ETAAtDischargePort = new ZDate(2021, 10, 20);
			existingArrivalWithReference.ATH_Reference = "B";

			var fsc10messageWithReference = @"FSC
JFKZI
081-11223344-HAWB123-B
ARR/ZI003/19OCT
FSC/10
WBL/SYD/T3/K3/CONSOLIDATION
ARR/ZI004/20OCT/B2/K2
";
			ProcessMessage(fsc10messageWithReference, ManifestHeader);
			AssertEquals("Should use existing ArrivalHeader.", 1, manifestHeader.ArrivalHeaders.Count);
			AssertSame(existingArrivalWithReference, manifestHeader.ArrivalHeaders[0]);
		}

		[TestDate(2021, 9, 7)]
		public void TestProcessStatus_10_ArrivalLine_EmptyHAWBNumber()
		{
			var manifestHeader = ManifestHeader;
			var existingArrival = manifestHeader.ArrivalHeaders.AddNew();
			existingArrival.ATH_VoyageFlightNo = "ZI004";
			existingArrival.ATH_ETAAtDischargePort = new ZDate(2021, 10, 20);
			AssertEquals("No ArrivalLine", 0, existingArrival.ArrivalDetails.Count);

			var fsc10messageEmptyHAWBNumber = @"FSC
JFKZI
081-11223344
ARR/ZI003/19OCT
FSC/10
WBL/SYD/T3/K3/CONSOLIDATION
ARR/ZI004/20OCT
TRN/ORD-D/UAL/990000000
";

			ProcessMessage(fsc10messageEmptyHAWBNumber, manifestHeader);
			Assert("Should use existing ArrivalHeader.", manifestHeader.ArrivalHeaders.Count == 1 && manifestHeader.ArrivalHeaders[0] == existingArrival);
			AssertEquals("Should have created ArrivalLine.", 1, existingArrival.ArrivalDetails.Count);
			var createdArrivalLine = existingArrival.ArrivalDetails[0];
			AssertEquals("ATL_ABL_AsycudaBill is MasterBill", manifestHeader.MasterBill.PK, createdArrivalLine.ATL_ABL_AsycudaBill);
			AssertEquals("Populate ATL_Quantity from Total Qty in message.", 3, createdArrivalLine.ATL_Quantity);
		}

		[TestDate(2021, 9, 7)]
		public void TestProcessStatus_10_ArrivalLine_TotalQty()
		{
			var manifestHeader = ManifestHeader;
			var bill = HouseBill;
			var existingArrival = manifestHeader.ArrivalHeaders.AddNew();
			existingArrival.ATH_VoyageFlightNo = "ZI004";
			existingArrival.ATH_ETAAtDischargePort = new ZDate(2021, 10, 20);
			AssertEquals("No ArrivalLine", 0, existingArrival.ArrivalDetails.Count);

			var fsc10messageWithTotalQty = @"FSC
JFKZI
081-11223344-HAWB123-B
ARR/ZI003/19OCT
FSC/10
WBL/SYD/T3/K3/CONSOLIDATION
ARR/ZI004/20OCT
TRN/ORD-D/UAL/990000000
";
			ProcessMessage(fsc10messageWithTotalQty, bill);
			AssertEquals("Should use existing ArrivalHeader", 1, manifestHeader.ArrivalHeaders.Count);
			Assert("Should use existing ArrivalHeader.", manifestHeader.ArrivalHeaders[0] == existingArrival);
			AssertEquals("Should have created ArrivalLine.", 1, existingArrival.ArrivalDetails.Count);
			var createdArrivalLine = existingArrival.ArrivalDetails[0];
			AssertEquals("Populate ATL_Quantity from Total Qty in message.", 3, createdArrivalLine.ATL_Quantity);
		}

		public void TestProcessStatus_10_ArrivalLine_BoardedQtyAndReference()
		{
			var manifestHeader = ManifestHeader;
			var bill = HouseBill;

			var fsc10messageWithBoardedQty = @"FSC
JFKZI
081-11223344-HAWB123-B
ARR/ZI003/19OCT
FSC/10
WBL/SYD/T3/K3/CONSOLIDATION
ARR/ZI004/20OCT-B/B2/K2
";
			ProcessMessage(fsc10messageWithBoardedQty, bill);
			AssertEquals("Should created 1 ArrivalHeader", 1, manifestHeader.ArrivalHeaders.Count);
			AssertEquals("Should created 1 ArrivalHeader and 1 ArrivalLine.", 1, manifestHeader.ArrivalHeaders[0].ArrivalDetails.Count);
			var createdArrivalLine = ManifestHeader.ArrivalHeaders[0].ArrivalDetails[0];
			AssertEquals("Populate ATL_Quantity from Boarded Qty in message.", 2, createdArrivalLine.ATL_Quantity);
			AssertEquals("Populate ATL_Reference from House Bill Reference.", "B", createdArrivalLine.ATL_Reference);
		}

		public void TestQtyIsAlwaysUpdated()
		{
			var manifestHeader = ManifestHeader;
			manifestHeader.AMA_MasterBill = "43951223001";
			manifestHeader.AMA_Voyage = "ZI001";
			manifestHeader.AMA_E_ARV = new ZDateTime(ZDateTime.Now.Year, 12, 12);

			var bill3 = HouseBill;
			bill3.ABL_BillNumber = "BKG220309HB3";

			var fsc10Bill3ResponseMessage1 = @"FSC
JFKZI
439-51223001-BKG220309HB3
ARR/ZI001/08MAR
FSC/10
WBL/SYD/T3/K3/OTHER ITEMS
ARR/ZI001/08MAR";
			ProcessMessage(fsc10Bill3ResponseMessage1, bill3);
			AssertEquals("Should create 1 ArrivalHeader", 1, manifestHeader.ArrivalHeaders.Count);
			AssertEquals("Should create 1 ArrivalHeader and 1 ArrivalLine.", 1, manifestHeader.ArrivalHeaders[0].ArrivalDetails.Count);
			var createdArrivalLine = ManifestHeader.ArrivalHeaders[0].ArrivalDetails[0];
			AssertEquals("Ensure ATL_Quantity is initially updated from Total Qty in WBL message being processed.", 3, createdArrivalLine.ATL_Quantity);

			var fsc10Bill3ResponseMessage2 = @"FSC
JFKZI
439-51223001-BKG220309HB3
ARR/ZI001/08MAR
FSC/08
TXT/ARR-JFK T4";
			ProcessMessage(fsc10Bill3ResponseMessage2, bill3);

			var fsc10Bill3ResponseMessage3 = @"FSC
JFKZI
439-51223001-BKG220309HB3
ARR/ZI001/08MAR
FSC/07
TXT/NO ENTRY FILED";
			ProcessMessage(fsc10Bill3ResponseMessage3, bill3);

			var fsc10Bill3ResponseMessage4 = @"FSC
JFKZI
439-51223001-BKG220309HB3
ARR/ZI001/08MAR
FSC/09
TXT/AGT - NONE";
			ProcessMessage(fsc10Bill3ResponseMessage4, bill3);

			var fsc10Bill3ResponseMessage5 = @"FSC
JFKZI
439-51223001-BKG220309HB3
ARR/ZI001/08MAR
FSC/10
WBL/SYD/T4/K4/OTHER ITEMS
ARR/ZI001/08MAR";
			ProcessMessage(fsc10Bill3ResponseMessage5, bill3);
			AssertEquals("Should update the ArrivalHeader", 1, manifestHeader.ArrivalHeaders.Count);
			AssertEquals("Should update the ArrivalHeader and 1 ArrivalLine.", 1, manifestHeader.ArrivalHeaders[0].ArrivalDetails.Count);
			var updatedArrivalLine = ManifestHeader.ArrivalHeaders[0].ArrivalDetails[0];
			AssertEquals("Ensure ATL_Quantity is updated from Total Qty in WBL message being processed.", 4, updatedArrivalLine.ATL_Quantity);
		}

		#endregion

		#region Calculated Arrival Date

		[TestDate(2021, 11, 1)]
		public void TestFSCMessageCalculatedArrivalDate_EmptyEstDate()
		{
			var responseMessageText = @"FSC
JFKZI
081-11223344-HAWB123
ARR/ZI003/01JUL
";

			var manifestHeader = ManifestHeader;
			AssertCalculatedScheduledArrivalDate(manifestHeader, responseMessageText, "Default to the year from EstimateScheduledArrivalDate of Arrival Wrapper.", ZDate.Empty, new ZDate(2022, 07, 01));
		}

		[TestDate(2021, 11, 1)]
		public void TestFSCMessageCalculatedArrivalDate_ValidEstDate()
		{
			var responseMessageText = @"FSC
JFKZI
081-11223344-HAWB123
ARR/ZI003/01JUL
";

			var manifestHeader = ManifestHeader;
			AssertCalculatedScheduledArrivalDate(manifestHeader, responseMessageText, "Should consider the year from AMA_E_ARV of manifestHeader.", new ZDate(2021, 06, 01), new ZDate(2021, 07, 01));
		}

		[TestDate(2021, 11, 1)]
		public void TestFSCMessageCalculatedArrivalDate_CloseToNewYear()
		{
			var responseMessageText = @"FSC
JFKZI
081-11223344-HAWB123
ARR/ZI003/01JAN
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
				.FirstOrDefault() as FSCMessage;

			AssertNotNull(inboundMessage);
			AssertEquals(message, expectedArrivalDate, inboundMessage.CalculatedScheduledArrivalDate);
		}

		#endregion

		#region Implementation

		protected override IAIMMessageTypeProcessor GetNewMessageTypeProcessor() => new FSCMessageTypeProcessor(new BatchProcessor.LoggingInformation());
		protected override AIMInboundMessage GetNewInboundMessage() => Factory.New<FSCMessage>();
		protected override ZString ExpectedEmailMessageType => "Freight Status Condition (FSC)";

		protected override void PopulateManifestHeader(AsycudaManifestHeader manifestHeader)
		{
			manifestHeader.AMA_Voyage = "QF101";
			manifestHeader.AMA_E_ARV = new ZDateTime(ZDateTime.Now.Year, 12, 12);
		}

		protected override ZString GetResponseMessageForGroupNotificationTesting()
		{
			return @"FSC
MIAKLM
081-11223344-HAWB123
FSC/00";
		}

		#endregion
	}
}
