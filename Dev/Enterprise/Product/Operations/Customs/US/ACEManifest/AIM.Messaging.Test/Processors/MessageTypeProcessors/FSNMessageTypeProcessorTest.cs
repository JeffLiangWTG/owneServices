using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.US.AIM.Messaging.Testing
{
	public class FSNMessageTypeProcessorTest : MessageTypeProcessorBaseTest
	{
		[TestDate(2020, 12, 7, 11, 15, 33)]
		public void TestFSNMessageCreatesArrival()
		{
			var manifestHeader = ManifestHeader;
			var bill = HouseBill;
			AssertEquals("Precondition: ArrivalHeaders.Count", 0, manifestHeader.ArrivalHeaders.Count);

			var responseMessageText = @"FSN
MIAKLM
081-11223344-HAWB123/123456
ARR/KL325/14DEC-A
CSN/1C-20/14DEC1430/0158232873876
";

			ProcessMessage(responseMessageText, bill);

			manifestHeader.Reload();
			AssertEquals("ArrivalHeaders.Count", 1, manifestHeader.ArrivalHeaders.Count);

			bill.Reload();
			AssertEquals("ABL_MessageStatus", ZString.Empty, bill.ABL_MessageStatus);
			AssertEquals("ABL_BillStatus", "1C", bill.ABL_BillStatus);

			var arrHeader = manifestHeader.ArrivalHeaders[0];
			AssertEquals("ATH_VoyageFlightNo", "KL325", arrHeader.ATH_VoyageFlightNo);
			AssertEquals("ATH_ETAAtDischargePort", new ZDateTime(2020, 12, 14), arrHeader.ATH_ETAAtDischargePort);
			AssertEquals("ATH_ArrivalReference", "", arrHeader.ATH_Reference);
			AssertEquals("ArrivalDetails.Count", 1, arrHeader.ArrivalDetails.Count);

			var arrLine = arrHeader.ArrivalDetails[0];
			AssertEquals("ATL_CargoStatus", "1C", arrLine.ATL_CargoStatus);
			AssertEquals("ATL_ABL_AsycudaBill", bill.PK, arrLine.ATL_ABL_AsycudaBill);
			AssertEquals("ATL_ArrivalReference", "A", arrLine.ATL_Reference);

			var expectedPropertyTableHTML = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Property</th><th>Value</th></tr></thead><tr><td>Message Time</td><td>07-Dec-20 11:15:33</td></tr><tr><td>Air Waybill Number</td><td>08111223344</td></tr><tr><td>HAWB Number</td><td>HAWB123</td></tr><tr><td>Package Tracking Identifier</td><td>123456</td></tr><tr><td>Flight Number</td><td>KL325</td></tr><tr><td>Scheduled Arrival Date</td><td>14-Dec-20</td></tr><tr><td>Part Arrival Reference</td><td>A</td></tr><tr><td>Airport Of Arrival</td><td>MIA</td></tr><tr><td>Action Code</td><td>1C - Entry processed: CBP general examination.</td></tr><tr><td>Remarks</td><td>&nbsp;</td></tr><tr><td>Number of Pieces</td><td>20</td></tr><tr><td>Entry Type</td><td>01</td></tr><tr><td>Entry Number</td><td>58232873876</td></tr></table>";
			AssertEmailCreated(ExpectedEmailMessageType + " - 08111223344 - HAWB123", expectedPropertyTableHTML);
		}

		[TestDate(2020, 12, 7, 11, 15, 33)]
		public void TestEmailTableRows()
		{
			var manifestHeader = ManifestHeader;
			var bill = HouseBill;

			var responseMessageText = @"FSN
MIAKLM
081-11223344-HAWB123/123456
ARR/KL325/14DEC-A
CSN/1C-20/14DEC1430/0158232873876/HLD 1234
";
			ProcessMessage(responseMessageText, bill);

			manifestHeader.Reload();
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			var emailBody = email.Body;
			AssertContains("Email contains Message Time", "<tr><td>Message Time</td><td>07-Dec-20 11:15:33</td></tr>", emailBody);
			AssertContains("Email contains Air Waybill Number", "<tr><td>Air Waybill Number</td><td>08111223344</td></tr>", emailBody);
			AssertContains("Email contains HAWB Number", "<tr><td>HAWB Number</td><td>HAWB123</td></tr>", emailBody);
			AssertContains("Email contains Package Tracking Identifier", "<tr><td>Package Tracking Identifier</td><td>123456</td></tr>", emailBody);
			AssertContains("Email contains Flight Number", "<tr><td>Flight Number</td><td>KL325</td></tr>", emailBody);
			AssertContains("Email contains Scheduled Arrival Date", "<tr><td>Scheduled Arrival Date</td><td>14-Dec-20</td></tr>", emailBody);
			AssertContains("Email contains Part Arrival Reference", "<tr><td>Part Arrival Reference</td><td>A</td></tr>", emailBody);
			AssertContains("Email contains Airport Of Arrival", "<tr><td>Airport Of Arrival</td><td>MIA</td></tr>", emailBody);
			AssertContains("Email contains Action Code", "<tr><td>Action Code</td><td>1C - Entry processed: CBP general examination.</td></tr>", emailBody);
			AssertContains("Email contains Remarks", "<tr><td>Remarks</td><td>HLD 1234</td></tr>", emailBody);
			AssertContains("Email contains Number of Pieces", "<tr><td>Number of Pieces</td><td>20</td></tr>", emailBody);
			AssertContains("Email contains Entry Type", "<tr><td>Entry Type</td><td>01</td></tr>", emailBody);
			AssertContains("Email contains Entry Number", "<tr><td>Entry Number</td><td>58232873876</td></tr>", emailBody);

			AssertNotContains("Email NOT contain Cargo Terminal Operator", "Cargo Terminal Operator", emailBody);
		}

		[TestDate(2020, 12, 7, 11, 15, 33)]
		public void TestEmailTableRows_Unsolicited_NoEntryNumber()
		{
			var manifestHeader = ManifestHeader;
			var bill = HouseBill;
			var postmaster = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_LoginName, User.PostMasterUserName));
			postmaster.GS_EmailAddress = "post@master.com";

			var responseMessageText = @"FSN
MIAKLM
081-11223344-HAWB123/123456
ARR/KL325/14DEC-A
CSN/1C-20/14DEC1430//HLD 1234
";
			var inboundMessage = CreateQueuedMessage(responseMessageText);
			Factory.Save();
			ProcessMessage(inboundMessage, null, bill);

			manifestHeader.Reload();
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Sends to Post Master when an outbound message cannot be found.", "post@master.com", email.Recipients[0].Email);

			var emailBody = email.Body;
			AssertContains("Email contains Message Time", "<tr><td>Message Time</td><td>07-Dec-20 11:15:33</td></tr>", emailBody);
			AssertContains("Email contains Air Waybill Number", "<tr><td>Air Waybill Number</td><td>08111223344</td></tr>", emailBody);
			AssertContains("Email contains HAWB Number", "<tr><td>HAWB Number</td><td>HAWB123</td></tr>", emailBody);
			AssertContains("Email contains Package Tracking Identifier", "<tr><td>Package Tracking Identifier</td><td>123456</td></tr>", emailBody);
			AssertContains("Email contains Flight Number", "<tr><td>Flight Number</td><td>KL325</td></tr>", emailBody);
			AssertContains("Email contains Scheduled Arrival Date", "<tr><td>Scheduled Arrival Date</td><td>14-Dec-20</td></tr>", emailBody);
			AssertContains("Email contains Part Arrival Reference", "<tr><td>Part Arrival Reference</td><td>A</td></tr>", emailBody);
			AssertContains("Email contains Airport Of Arrival", "<tr><td>Airport Of Arrival</td><td>MIA</td></tr>", emailBody);
			AssertContains("Email contains Action Code", "<tr><td>Action Code</td><td>1C - Entry processed: CBP general examination.</td></tr>", emailBody);
			AssertContains("Email contains Remarks", "<tr><td>Remarks</td><td>HLD 1234</td></tr>", emailBody);
			AssertContains("Email contains Number of Pieces", "<tr><td>Number of Pieces</td><td>20</td></tr>", emailBody);
			AssertContains("Email contains Entry Type", "<tr><td>Entry Type</td><td>&nbsp;</td></tr>", emailBody);
			AssertContains("Email contains Entry Number", "<tr><td>Entry Number</td><td>&nbsp;</td></tr>", emailBody);

			AssertNotContains("Email NOT contain Cargo Terminal Operator", "Cargo Terminal Operator", emailBody);
		}

		[TestDate(2020, 12, 7, 11, 15, 33)]
		public void TestEmailTableRows_NoPackageTrackingIdentifier()
		{
			var manifestHeader = ManifestHeader;
			var bill = HouseBill;

			var responseMessageText = @"FSN
MIAKLM
081-11223344-HAWB123
ARR/KL325/14DEC-A
CSN/1C-20/14DEC1430/0158232873876/HLD 1234
";
			ProcessMessage(responseMessageText, bill);

			manifestHeader.Reload();
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			var emailBody = email.Body;

			AssertNotContains("Email NOT contain Package Tracking Identifier", "Package Tracking Identifier", emailBody);
		}

		[TestDate(2020, 12, 7, 11, 15, 33)]
		public void TestFSNMessageUpdatesArrival()
		{
			var manifestHeader = ManifestHeader;
			var bill = HouseBill;
			bill.ABL_BillNumber = "1234321";
			var arrHeader = ArrivalHeader;
			var arrLine = ArrivalLine;

			AssertEquals("Precondition: ABL_MessageStatus", ZString.Empty, arrLine.ATL_CargoStatus);

			var responseMessageText = @"FSN
MIAKLM
081-11223344-1234321/123456
ARR/KL325/12DEC
CSN/1C-20/12DEC1430/0158232873876
";

			ProcessMessage(responseMessageText, bill);

			bill.Reload();
			AssertEquals("ABL_BillStatus", "1C", bill.ABL_BillStatus);

			arrLine.Reload();
			AssertEquals("ABL_MessageStatus", "1C", arrLine.ATL_CargoStatus);
		}

		[TestDate(2020, 12, 7, 11, 15, 33)]
		public void TestFSNMessageUpdatesArrivalOnHyphenatedMasterBill()
		{
			var manifestHeader = ManifestHeader;
			manifestHeader.AMA_MasterBill = "081-11223344";

			var bill = HouseBill;
			var arrHeader = ArrivalHeader;
			var arrLine = ArrivalLine;

			AssertEquals("Precondition: ABL_MessageStatus", ZString.Empty, arrLine.ATL_CargoStatus);

			var responseMessageText = @"FSN
MIAKLM
081-11223344-HAWB123/123456
ARR/KL325/12DEC
CSN/1C-20/12DEC1430/0158232873876
";

			ProcessMessage(responseMessageText, bill);

			bill.Reload();
			AssertEquals("ABL_BillStatus", "1C", bill.ABL_BillStatus);

			arrLine.Reload();
			AssertEquals("ABL_MessageStatus", "1C", arrLine.ATL_CargoStatus);
		}

		[TestDate(2020, 12, 7, 11, 15, 33)]
		public void TestFSNMessageWithoutMatchingArrivalCreatesArrival()
		{
			var manifestHeader = ManifestHeader;
			var bill = HouseBill;
			var arrHeader = ArrivalHeader;
			var arrLine = ArrivalLine;

			var responseMessageText = @"FSN
MIAKLM
081-11223344-HAWB123/123456
ARR/KL325/13DEC-A
CSN/1C-20/12DEC1430/0158232873876
";

			ProcessMessage(responseMessageText, bill);

			manifestHeader.Reload();
			AssertEquals("ArrivalHeaders.Count", 2, manifestHeader.ArrivalHeaders.Count);

			bill.Reload();
			AssertEquals("ABL_BillStatus", "1C", bill.ABL_BillStatus);

			arrLine.Reload();
			AssertEquals("ABL_MessageStatus", "", arrLine.ATL_CargoStatus);

			var newArrHeader = manifestHeader.ArrivalHeaders.FirstOrDefault(o => o.PK != arrHeader.PK);
			AssertEquals("ATH_ArrivalReference", "", newArrHeader.ATH_Reference);

			var newArrLine = newArrHeader.ArrivalDetails.FirstOrDefault();
			AssertEquals("ATL_CargoStatus", "1C", newArrLine.ATL_CargoStatus);
			AssertEquals("ATL_ArrivalReference", "A", newArrLine.ATL_Reference);
		}

		[TestDate(2020, 12, 7, 11, 15, 33)]
		public void TestFSNMessageWithoutTrackingIdUpdatesCorrectHouseBillStatus()
		{
			var manifestHeader = ManifestHeader;
			var bill1 = HouseBill;
			var outgoingMessage1 = CreateSentMessage(bill1);
			outgoingMessage1.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(-2);

			var bill2 = ManifestHeader.Bills.AddNew();
			bill2.ABL_BillNumber = "HAWB456";
			PopulateHouseBill(bill2);
			var outgoingMessage2 = CreateSentMessage(bill2);
			outgoingMessage2.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);

			var bill3 = ManifestHeader.Bills.AddNew();
			bill3.ABL_BillNumber = "HAWB789";
			PopulateHouseBill(bill3);
			var outgoingMessage3 = CreateSentMessage(bill3);

			var responseMessageText = @"FSN
MIAKLM
081-11223344-HAWB456/
ARR/KL325/12DEC
CSN/1C-20/12DEC1430/0158232873876
";

			var inboundMessage = CreateQueuedMessage(responseMessageText);
			Factory.Save();

			ProcessMessage(inboundMessage, outgoingMessage2, bill2);

			bill2.Reload();
			AssertEquals("ABL_BillStatus", "1C", bill2.ABL_BillStatus);

			manifestHeader.Reload();
			AssertEquals("ArrivalHeaders.Count", 1, manifestHeader.ArrivalHeaders.Count);

			var newArrHeader = manifestHeader.ArrivalHeaders.FirstOrDefault();
			AssertEquals("ATH_ArrivalReference", "", newArrHeader.ATH_Reference);

			var newArrLine = newArrHeader.ArrivalDetails.FirstOrDefault();
			AssertEquals("ATL_CargoStatus", "1C", newArrLine.ATL_CargoStatus);
		}

		[TestDate(2020, 12, 7, 11, 15, 33)]
		public void TestFSNMessageWithoutHouseBillUpdatesMasterBillStatus()
		{
			var manifestHeader = ManifestHeader;

			var responseMessageText = @"FSN
MIAKLM
081-11223344-/
ARR/KL325/12DEC
CSN/1C-20/12DEC1430/0158232873876
";

			ProcessMessage(responseMessageText, manifestHeader);

			manifestHeader.Reload();
			AssertEquals("ArrivalHeaders.Count", 1, manifestHeader.ArrivalHeaders.Count);

			var masterBill = manifestHeader.MasterBill;
			AssertEquals("ABL_BillStatus", "1C", masterBill.ABL_BillStatus);

			var newArrHeader = manifestHeader.ArrivalHeaders.FirstOrDefault();
			AssertEquals("ATH_VoyageFlightNo", "KL325", newArrHeader.ATH_VoyageFlightNo);
			AssertEquals("ATH_ETAAtDischargePort", new ZDateTime(2020, 12, 12), newArrHeader.ATH_ETAAtDischargePort);
			AssertEquals("ATH_ArrivalReference", "", newArrHeader.ATH_Reference);

			var newArrLine = newArrHeader.ArrivalDetails.FirstOrDefault();
			AssertEquals("ATL_CargoStatus", "1C", newArrLine.ATL_CargoStatus);
			AssertEquals("ATL_ABL_AsycudaBill", masterBill.PK, newArrLine.ATL_ABL_AsycudaBill);
		}

		[TestDate(2020, 12, 7, 11, 15, 33)]
		public void TestFSNMessageWithoutHouseBillUpdatesArrivalHeaderReference()
		{
			var manifestHeader = ManifestHeader;
			var bill = HouseBill;
			var arrHeader = ArrivalHeader;
			var arrLine = ArrivalLine;

			var responseMessageText = @"FSN
MIAKLM
081-11223344-/
ARR/KL325/12DEC-A
CSN/1C-20/12DEC1430/0158232873876
";

			ProcessMessage(responseMessageText, manifestHeader);

			manifestHeader.Reload();
			var masterBill = manifestHeader.MasterBill;
			AssertEquals("ABL_BillStatus", "1C", masterBill.ABL_BillStatus);

			AssertEquals("ArrivalHeaders.Count", 1, manifestHeader.ArrivalHeaders.Count);
			var newArrHeader = manifestHeader.ArrivalHeaders.FirstOrDefault();
			AssertEquals("ATH_VoyageFlightNo", "KL325", newArrHeader.ATH_VoyageFlightNo);
			AssertEquals("ATH_ETAAtDischargePort", new ZDateTime(2020, 12, 12), newArrHeader.ATH_ETAAtDischargePort);
			AssertEquals("ATH_ArrivalReference", "A", newArrHeader.ATH_Reference);

			AssertEquals("ArrivalDetails.Count", 2, newArrHeader.ArrivalDetails.Count);
			var newArrLine = newArrHeader.ArrivalDetails.FirstOrDefault(d => d.ATL_ABL_AsycudaBill == masterBill.PK);
			AssertEquals("ATL_CargoStatus", "1C", newArrLine.ATL_CargoStatus);
			AssertEquals("ATL_ArrivalReference", "A", newArrLine.ATL_Reference);
			AssertEquals("ATL_Quantity", 20, newArrLine.ATL_Quantity);
		}

		[TestDate(2020, 12, 7, 11, 15, 33)]
		public void TestFSNMessageWithRecycledMasterBillUpdatesMasterBillStatus()
		{
			var olderManifestHeader = Factory.NewWithValidTestData<ACEManifest.Business.AsycudaManifestHeader>();
			olderManifestHeader.AMA_RN_NKCountry = "US";
			olderManifestHeader.AMA_TransportMode = "AIR";
			olderManifestHeader.AMA_MasterBill = "08111223344";
			olderManifestHeader.AMA_Voyage = "KLM325";
			olderManifestHeader.AMA_E_ARV = new ZDateTime(2020, 12, 12);
			olderManifestHeader.AMA_RL_NKPortOfFirstArrival = "USMIA";
			olderManifestHeader.AMA_CarrierCode = "KLM";

			olderManifestHeader.AMA_JobReference = "C135792";
			olderManifestHeader.AMA_SystemCreateTimeUtc = ZDateTime.Now.AddYears(-1);

			var manifestHeader = ManifestHeader;
			manifestHeader.AMA_SystemCreateTimeUtc = ZDateTime.Now;

			var responseMessageText = @"FSN
MIAKLM
081-11223344-/
ARR/KL325/12DEC
CSN/1C-20/12DEC1430/0158232873876
";

			ProcessMessage(responseMessageText, manifestHeader);

			manifestHeader.Reload();
			AssertEquals("ArrivalHeaders.Count", 1, manifestHeader.ArrivalHeaders.Count);

			var newArrHeader = manifestHeader.ArrivalHeaders.FirstOrDefault();
			AssertEquals("ATH_ArrivalReference", "", newArrHeader.ATH_Reference);

			var newArrLine = newArrHeader.ArrivalDetails.FirstOrDefault();
			AssertEquals("ATL_CargoStatus", "1C", newArrLine.ATL_CargoStatus);

			var masterBill = manifestHeader.MasterBill;
		}

		[TestDate(2021, 11, 1)]
		public void TestDoNotFilterLegacyDataByMAWB()
		{
			var responseMessageText = @"FSN
MIAKLM
081-11223344-/
ARR/KL325/01JUL-A
";

			var manifestHeader = ManifestHeader;
			manifestHeader.MasterBill.ABL_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-15);
			manifestHeader.Factory.Save();

			CreateSentMessage(manifestHeader);
			var inboundMessage = CreateQueuedMessage(responseMessageText);
			Factory.Save();

			var logger = new LoggingInformation();

			var processor = new FSNMessageTypeProcessor(logger);
			var processedResult = processor.Process(inboundMessage);

			Assert("Should not process successful as there is not a valid manifest header which created time is in 1 year range.", !processedResult);
			AssertCollectionContains("A Manifest record was not found for MasterBill = 081-11223344, HouseBill = .", logger.Logs.Select(c => c.Message));
		}

		#region Calculated Arrival Date

		[TestDate(2021, 11, 1)]
		public void TestFSNMessageCalculatedArrivalDate_EmptyEstDate()
		{
			var responseMessageText = @"FSN
MIAKLM
081-11223344-/
ARR/KL325/01JUL-A
";

			var manifestHeader = ManifestHeader;
			AssertCalculatedScheduledArrivalDate(manifestHeader, responseMessageText, "Default to the year from EstimateScheduledArrivalDate of Arrival Wrapper.", ZDate.Empty, new ZDate(2022, 07, 01));
		}

		[TestDate(2021, 11, 1)]
		public void TestFSNMessageCalculatedArrivalDate_ValidEstDate()
		{
			var responseMessageText = @"FSN
MIAKLM
081-11223344-/
ARR/KL325/01JUL-A
";

			var manifestHeader = ManifestHeader;
			AssertCalculatedScheduledArrivalDate(manifestHeader, responseMessageText, "Should consider the year from AMA_E_ARV of manifestHeader.", new ZDate(2021, 06, 01), new ZDate(2021, 07, 01));
		}

		[TestDate(2021, 11, 1)]
		public void TestFSNMessageCalculatedArrivalDate_CloseToNewYear()
		{
			var responseMessageText = @"FSN
MIAKLM
081-11223344-/
ARR/KL325/01JAN-A
";

			var manifestHeader = ManifestHeader;
			AssertCalculatedScheduledArrivalDate(manifestHeader, responseMessageText, "Should consider the year from AMA_E_ARV of manifestHeader.", new ZDate(2021, 12, 31), new ZDate(2022, 01, 01));
		}

		[TestDate(2021, 04, 02)]
		public void TestFSNMessageCalculatedArrivalDate_BeforeThreeMonths()
		{
			var responseMessageText = @"FSN
MIAKLM
081-11223344-/
ARR/KL325/01JAN-A
";

			var manifestHeader = ManifestHeader;
			AssertCalculatedScheduledArrivalDate(manifestHeader, responseMessageText, "Should consider the year from AMA_E_ARV of manifestHeader.", new ZDate(2020, 12, 30), new ZDate(2021, 01, 01));
		}

		[TestDate(2021, 04, 02)]
		public void TestFSNMessageCalculatedArrivalDate_AfterThreeMonths()
		{
			var responseMessageText = @"FSN
MIAKLM
081-11223344-/
ARR/KL325/30DEC-A
";

			var manifestHeader = ManifestHeader;
			AssertCalculatedScheduledArrivalDate(manifestHeader, responseMessageText, "Should consider the year from AMA_E_ARV of manifestHeader.", new ZDate(2021, 01, 01), new ZDate(2020, 12, 30));
		}

		public void TestUpdatesStatusByActionCode()
		{
			var responseMessageTemplate = $@"FSN
MIAKLM
081-11223344-{HouseBill.ABL_BillNumber}
ARR/KL325/12DEC-A
CSN/{{0}}-2/13DEC0051/E038
";
			var mappingsActionCode2MessageStatus = new (string actionCode, string messageStatus)[]
			{
				("1D", "TAC"),
				("1E", "TER"),
				("1F", "TAC"),
				("1G", "TER"),
				("83", "TCD"),
				("95", "TCD"),
				("11", "ARV"),
				("12", "ARV"),
				("P3", "ARV"),
			};

			CombineAssertions(() =>
			{
				foreach (var mapping in mappingsActionCode2MessageStatus)
				{
					var manifestHeader = ManifestHeader;
					var transferHeader = ArrivalHeader.TransferHeaders.AddNew();
					transferHeader.ATF_TransferType = ManifestBase.TransferTypeList.Codes.Domestic;
					var transferBill = transferHeader.TransferBills.AddNew();
					transferBill.ATB_ABL_Bill = HouseBill.PK;
					manifestHeader.Factory.Save();
					ProcessMessage(string.Format(responseMessageTemplate, mapping.actionCode), HouseBill);
					AssertEquals("Message status", mapping.messageStatus, transferBill.ATB_MessageStatus);
					AssertEquals("Customs status", mapping.actionCode, transferBill.ATB_CustomsStatus);
				}
			});
		}

		public void TestUpdatesStatusByActionCode_FindingATB()
		{
			var responseMessage = $@"FSN
MIAKLM
081-11223344-{HouseBill.ABL_BillNumber}
ARR/KL325/12DEC-A
CSN/1F-2/13DEC0051/E038
";

			var manifestHeader = ManifestHeader;
			var currentTransferHeader = ArrivalHeader.TransferHeaders.AddNew();
			currentTransferHeader.ATF_TransferType = ManifestBase.TransferTypeList.Codes.Domestic;
			var transferBillOld = currentTransferHeader.TransferBills.AddNew();
			transferBillOld.ATB_ABL_Bill = HouseBill.PK;
			transferBillOld.ATB_MessageStatus = "TCD";
			var transferBillCurrent = currentTransferHeader.TransferBills.AddNew();
			transferBillCurrent.ATB_ABL_Bill = HouseBill.PK;
			var transferBillForAnotherHouseBill = currentTransferHeader.TransferBills.AddNew();
			var anotherHouseBill = ManifestHeader.Bills.AddNew();
			anotherHouseBill.ABL_BillNumber = "HAWB456";
			transferBillForAnotherHouseBill.ATB_ABL_Bill = anotherHouseBill.PK;
			var anotherArrivalHeader = ManifestHeader.ArrivalHeaders.AddNew();
			anotherArrivalHeader.ATH_VoyageFlightNo = "KL325";
			anotherArrivalHeader.ATH_ETAAtDischargePort = new ZDateTime(2020, 12, 13);
			var transferHeaderForAnotherArrival = anotherArrivalHeader.TransferHeaders.AddNew();
			transferHeaderForAnotherArrival.ATF_TransferType = ManifestBase.TransferTypeList.Codes.Domestic;
			var transferBillForAnotherArrival = transferHeaderForAnotherArrival.TransferBills.AddNew();
			transferBillForAnotherArrival.ATB_ABL_Bill = HouseBill.PK;
			transferBillForAnotherArrival.ATB_SystemCreateTimeUtc = ZDateTime.Now.AddSeconds(30);
			manifestHeader.Factory.Save();
			CombineAssertions(() =>
			{
				ProcessMessage(responseMessage, HouseBill);
				AssertEquals("Message status for current transfer bill set", "TAC", transferBillCurrent.ATB_MessageStatus);
				AssertEquals("Customs status for current transfer bill set", "1F", transferBillCurrent.ATB_CustomsStatus);
				AssertNotEquals("Old transfer bill unchanged", "TAC", transferBillOld.ATB_MessageStatus);
				AssertNotEquals("Transfer bill for another house bill unchanged", "TAC", transferBillForAnotherHouseBill.ATB_MessageStatus);
				AssertNotEquals("Transfer bill for another arrival unchanged", "TAC", transferBillForAnotherArrival.ATB_MessageStatus);
			});
		}

		void AssertCalculatedScheduledArrivalDate(AsycudaManifestHeader manifestHeader, string responseMessageText, string message, ZDate arrivalDate, ZDate expectedArrivalDate)
		{
			manifestHeader.AMA_E_ARV = arrivalDate;
			manifestHeader.Factory.Save();

			ProcessMessage(responseMessageText, manifestHeader);

			manifestHeader.Messages.Reload(true);

			var inboundMessage = manifestHeader.Messages
				.Find(c => c.EM_ReceiveTransmit == Direction.Receive)
				.FirstOrDefault() as FSNMessage;

			AssertNotNull(inboundMessage);
			AssertEquals(message, expectedArrivalDate, inboundMessage.CalculatedScheduledArrivalDate);
		}

		#endregion

		#region Implementation

		protected override IAIMMessageTypeProcessor GetNewMessageTypeProcessor() => new FSNMessageTypeProcessor(new LoggingInformation());
		protected override AIMInboundMessage GetNewInboundMessage() => Factory.New<FSNMessage>();
		protected override ZString ExpectedEmailMessageType => "Freight Status Notification (FSN)";

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
			return @"FSN
MIAKLM
081-11223344-HAWB123/123456
ARR/KL325/14DEC-A
CSN/1C-20/14DEC1430/0158232873876
";
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.UnitedStates);
			var codeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AMSAirDispositionCode, "AMSAD", dataGrouping.ZZZ_DataGrouping);
			var date = ZDateTime.UtcNow;
			var startDate = date.AddDays(-10);
			var endDate = date.AddDays(10);
			var code1 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "1C", "Entry processed: CBP general examination.", startDate, endDate);
			Factory.Save();
		}

		#endregion
	}
}
