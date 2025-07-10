using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ASYCUDA.Business;
using NUnit.Framework;
using static Enterprise.Customs.US.AIM.Messaging.Constants;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.US.AIM.Messaging.Testing
{
	public class FERMessageTypeProcessorTest : MessageTypeProcessorBaseTest
	{
		[TestDate(2020, 12, 7, 11, 15, 33)]
		public void TestProcessFERMessage_Bill()
		{
			var manifestHeader = ManifestHeader;
			var bill = HouseBill;
			var arrHeader = ArrivalHeader;
			var arrLine = ArrivalLine;
			arrLine.ATL_MessageStatus = ASYCUDA.Business.MessageStatusCodeList.Codes.Sent;

			var responseMessageText = @"FER
QF101/12DEC
081-11223344-HAWB123/123456
ERR/001ERROR DESCRIPTION
ERR/002ANOTHER ERROR
";

			ProcessMessage(responseMessageText, bill);

			bill.Reload();
			AssertEquals("ABL_MessageStatus", ASYCUDA.Business.MessageStatusCodeList.Codes.Error, bill.ABL_MessageStatus);

			arrLine.Reload();
			AssertEquals("ATL_MessageStatus", ASYCUDA.Business.MessageStatusCodeList.Codes.Sent, arrLine.ATL_MessageStatus);

			var expectedPropertyTableHTML = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Property</th><th>Value</th></tr></thead><tr><td>Message Time</td><td>07-Dec-20 11:15:33</td></tr><tr><td>Air Waybill Number</td><td>08111223344</td></tr><tr><td>HAWB Number</td><td>HAWB123</td></tr><tr><td>Flight Number</td><td>QF101</td></tr><tr><td>Arrival Date</td><td>12-Dec-20</td></tr><tr><td>Package Tracking Identifier</td><td>123456</td></tr></table>";
			var expectedExtraHTML = @"<P><B>Error List</B></P><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Code</th><th>Description</th></tr></thead><tr><td>001</td><td>ERROR DESCRIPTION</td></tr><tr><td>002</td><td>ANOTHER ERROR</td></tr></table>";
			AssertEmailCreated(ExpectedEmailMessageType + " - 08111223344 - HAWB123", expectedPropertyTableHTML, expectedExtraHTML);
		}

		[TestDate(2020, 12, 7, 11, 15, 33)]
		public void TestProcessFERMessage_Bill_188Warning()
		{
			var manifestHeader = ManifestHeader;
			var bill = HouseBill;
			var arrHeader = ArrivalHeader;
			var arrLine = ArrivalLine;
			arrLine.ATL_MessageStatus = ASYCUDA.Business.MessageStatusCodeList.Codes.Sent;

			var responseMessageText = @"FER
QF101/12DEC
081-11223344-HAWB123/123456
ERR/188EXPRESS RECORD INCOMPLETE
";

			ProcessMessage(responseMessageText, bill);

			bill.Reload();
			AssertEquals("ABL_MessageStatus", ASYCUDA.Business.MessageStatusCodeList.Codes.Warning, bill.ABL_MessageStatus);

			var expectedPropertyTableHTML = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Property</th><th>Value</th></tr></thead><tr><td>Message Time</td><td>07-Dec-20 11:15:33</td></tr><tr><td>Air Waybill Number</td><td>08111223344</td></tr><tr><td>HAWB Number</td><td>HAWB123</td></tr><tr><td>Flight Number</td><td>QF101</td></tr><tr><td>Arrival Date</td><td>12-Dec-20</td></tr><tr><td>Package Tracking Identifier</td><td>123456</td></tr></table>";
			var expectedExtraHTML = @"<P><B>Error List</B></P><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Code</th><th>Description</th></tr></thead><tr><td>188</td><td>EXPRESS RECORD INCOMPLETE</td></tr></table>";
			AssertEmailCreated(ExpectedEmailMessageType + " - 08111223344 - HAWB123", expectedPropertyTableHTML, expectedExtraHTML);
		}

		[TestDate(2020, 12, 7, 11, 15, 33)]
		public void TestProcessFERMessage_MultiBill()
		{
			var manifestHeader = ManifestHeader;
			var bill = HouseBill;
			var arrHeader = ArrivalHeader;
			var arrLine = ArrivalLine;
			arrLine.ATL_MessageStatus = ASYCUDA.Business.MessageStatusCodeList.Codes.Sent;
			var outgoingMessage1 = CreateSentMessage(bill);
			outgoingMessage1.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);

			var bill2 = ManifestHeader.Bills.AddNew();
			bill2.ABL_BillNumber = "HAWB456";
			PopulateHouseBill(bill2);
			var arrivalHeader2 = ManifestHeader.ArrivalHeaders.AddNew();
			PopulateArrivalHeader(arrivalHeader2);
			var arrivalLine2 = arrivalHeader2.ArrivalDetails.AddNew();
			arrivalLine2.ATL_ABL_AsycudaBill = bill2.PK;
			var outgoingMessage2 = CreateSentMessage(bill2);
			bill2.ABL_MessageStatus = ASYCUDA.Business.MessageStatusCodeList.Codes.Sent;

			var responseMessageText = @"FER
QF101/12DEC
081-11223344-HAWB123/123456
ERR/001ERROR DESCRIPTION
ERR/002ANOTHER ERROR
";

			var inboundMessage = CreateQueuedMessage(responseMessageText);
			Factory.Save();

			ProcessMessage(inboundMessage, outgoingMessage1, bill);

			bill.Reload();
			AssertEquals("Processor sets status on correct bill.", ASYCUDA.Business.MessageStatusCodeList.Codes.Error, bill.ABL_MessageStatus);
			bill2.Reload();
			AssertEquals("bill2 is untouched.", ASYCUDA.Business.MessageStatusCodeList.Codes.Sent, bill2.ABL_MessageStatus);
			arrLine.Reload();
			AssertEquals("arrival is untouched", ASYCUDA.Business.MessageStatusCodeList.Codes.Sent, arrLine.ATL_MessageStatus);

			var expectedPropertyTableHTML = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Property</th><th>Value</th></tr></thead><tr><td>Message Time</td><td>07-Dec-20 11:15:33</td></tr><tr><td>Air Waybill Number</td><td>08111223344</td></tr><tr><td>HAWB Number</td><td>HAWB123</td></tr><tr><td>Flight Number</td><td>QF101</td></tr><tr><td>Arrival Date</td><td>12-Dec-20</td></tr><tr><td>Package Tracking Identifier</td><td>123456</td></tr></table>";
			var expectedExtraHTML = @"<P><B>Error List</B></P><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Code</th><th>Description</th></tr></thead><tr><td>001</td><td>ERROR DESCRIPTION</td></tr><tr><td>002</td><td>ANOTHER ERROR</td></tr></table>";
			AssertEmailCreated(ExpectedEmailMessageType + " - 08111223344 - HAWB123", expectedPropertyTableHTML, expectedExtraHTML);
		}

		[TestDate(2020, 12, 7)]
		public void TestProcessFERMessage_ArrivalFromManifest()
		{
			var manifestHeader = ManifestHeader;
			manifestHeader.AMA_E_ARV = new ZDateTime(2020, 12, 10);
			var bill = HouseBill;
			var arrHeader = ArrivalHeader;
			var arrLine = ArrivalLine;
			arrLine.ATL_MessageStatus = ASYCUDA.Business.MessageStatusCodeList.Codes.Sent;

			var responseMessageText = @"FER
QF101/12DEC
081-11223344-HAWB123/123456
ERR/001ERROR DESCRIPTION
ERR/002ANOTHER ERROR
";

			ProcessMessage(responseMessageText, bill);

			bill.Reload();
			AssertEquals("ABL_MessageStatus", ZString.Empty, bill.ABL_MessageStatus);

			arrLine.Reload();
			AssertEquals("ATL_MessageStatus", ASYCUDA.Business.MessageStatusCodeList.Codes.Error, arrLine.ATL_MessageStatus);
		}

		[TestDate(2020, 12, 7)]
		public void TestProcessFERMessage_ArrivalFromSentMessage()
		{
			var manifestHeader = ManifestHeader;
			var bill = HouseBill;
			var arrHeader = ArrivalHeader;
			var arrLine = ArrivalLine;
			arrLine.ATL_MessageStatus = ASYCUDA.Business.MessageStatusCodeList.Codes.Sent;

			var outgoingMessage1 = CreateSentMessage(bill);
			outgoingMessage1.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
			Factory.Save();

			var outgoingMessageText = @"FSN
/US/12345/123-456-7890";

			var responseMessageText = @"FER
QF101/12DEC
081-11223344-HAWB123/123456
ERR/001ERROR DESCRIPTION
ERR/002ANOTHER ERROR
";

			var outgoingMessage = CreateSentMessage(arrHeader, outgoingMessageText, AIMMessageSubTypes.FSN);
			var inboundMessage = CreateQueuedMessage(responseMessageText);
			Factory.Save();

			ProcessMessage(inboundMessage, outgoingMessage, arrHeader);

			bill.Reload();
			AssertEquals("ABL_MessageStatus", ZString.Empty, bill.ABL_MessageStatus);

			arrLine.Reload();
			AssertEquals("ATL_MessageStatus", ASYCUDA.Business.MessageStatusCodeList.Codes.Error, arrLine.ATL_MessageStatus);
		}

		[TestDate(2020, 12, 7)]
		public void TestProcessFERMessage_MultiArrival()
		{
			var manifestHeader = ManifestHeader;
			var bill = HouseBill;
			var arrHeader = ArrivalHeader;
			var arrLine = ArrivalLine;
			arrLine.ATL_MessageStatus = ASYCUDA.Business.MessageStatusCodeList.Codes.Sent;

			var outgoingMessageText = @"FSN
/US/12345/123-456-7890";
			var outgoingMessage = CreateSentMessage(arrHeader, outgoingMessageText, AIMMessageSubTypes.FSN);
			outgoingMessage.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);

			var bill2 = ManifestHeader.Bills.AddNew();
			bill2.ABL_BillNumber = "HAWB456";
			PopulateHouseBill(bill2);
			var arrivalHeader2 = ManifestHeader.ArrivalHeaders.AddNew();
			PopulateArrivalHeader(arrivalHeader2);
			var arrivalLine2 = arrivalHeader2.ArrivalDetails.AddNew();
			arrivalLine2.ATL_ABL_AsycudaBill = bill2.PK;

			var outgoingMessage2 = CreateSentMessage(arrivalHeader2);
			Factory.Save();

			var responseMessageText = @"FER
QF101/12DEC
081-11223344-HAWB123/123456
ERR/001ERROR DESCRIPTION
ERR/002ANOTHER ERROR
";

			var inboundMessage = CreateQueuedMessage(responseMessageText);
			Factory.Save();

			ProcessMessage(inboundMessage, outgoingMessage, arrHeader);

			bill.Reload();
			AssertEquals("bill is untouched", ZString.Empty, bill.ABL_MessageStatus);

			arrLine.Reload();
			AssertEquals("Processor sets error status on arrival of correct bill.", ASYCUDA.Business.MessageStatusCodeList.Codes.Error, arrLine.ATL_MessageStatus);
		}

		public void TestUpdatesStatus_Arrival()
		{
			var responseMessageText = $@"FER
QF101/12DEC
081-11223344-{HouseBill.ABL_BillNumber}/123456
";
			var manifestHeader = ManifestHeader;
			var previousMessage = Factory.New<AIMEDIMessage>();
			manifestHeader.Messages.Add(previousMessage);
			previousMessage.EM_LinkedObject = HouseBill;
			previousMessage.EM_MessageSubType = "FSN";
			previousMessage.EM_Status = "SNT";
			previousMessage.EM_ReceiveTransmit = "TRX";
			ArrivalLine.ATL_MessageStatus = "SNT";
			var transferHeaderOld = ArrivalHeader.TransferHeaders.AddNew();
			transferHeaderOld.ATF_TransferType = ManifestBase.TransferTypeList.Codes.Domestic;
			var transferBillOld = transferHeaderOld.TransferBills.AddNew();
			transferBillOld.ATB_ABL_Bill = HouseBill.PK;
			transferBillOld.ATB_MessageStatus = "TCD";
			var transferHeaderCurrent = ArrivalHeader.TransferHeaders.AddNew();
			transferHeaderCurrent.ATF_TransferType = ManifestBase.TransferTypeList.Codes.Domestic;
			var transferBillCurrent = transferHeaderCurrent.TransferBills.AddNew();
			transferBillCurrent.ATB_ABL_Bill = HouseBill.PK;
			var anotherArrivalHeader = ManifestHeader.ArrivalHeaders.AddNew();
			anotherArrivalHeader.ATH_VoyageFlightNo = "QF101";
			anotherArrivalHeader.ATH_ETAAtDischargePort = new ZDateTime(2020, 12, 13);
			var anotherArrivalLine = anotherArrivalHeader.ArrivalDetails.AddNew();
			anotherArrivalLine.ATL_ABL_AsycudaBill = HouseBill.PK;
			anotherArrivalLine.ATL_MessageStatus = "SNT";
			var transferHeaderForAnotherArrival = anotherArrivalHeader.TransferHeaders.AddNew();
			transferHeaderForAnotherArrival.ATF_TransferType = ManifestBase.TransferTypeList.Codes.Domestic;
			var transferBillForAnotherArrival = transferHeaderForAnotherArrival.TransferBills.AddNew();
			transferBillForAnotherArrival.ATB_ABL_Bill = HouseBill.PK;
			transferBillForAnotherArrival.ATB_SystemCreateTimeUtc = ZDateTime.Now.AddSeconds(30);
			manifestHeader.Factory.Save();
			CombineAssertions(() =>
			{
				ProcessMessage(responseMessageText, HouseBill);
				AssertEquals("Message status", "AER", transferBillCurrent.ATB_MessageStatus);
				AssertNotEquals("Old transfer bill unchanged", "AER", transferBillOld.HasChanges);
				AssertNotEquals("Transfer bill for another arrival unchanged", "AER", transferBillForAnotherArrival.ATB_MessageStatus);
				AssertEquals("Arrival line updated", "ERR", ArrivalLine.ATL_MessageStatus);
				AssertNotEquals("Another arrival line not updated", "ERR", anotherArrivalLine.ATL_MessageStatus);
			});
		}

		public void TestUpdatesStatus_Transfer()
		{
			var responseMessageText = $@"FER
QF101/12DEC
081-11223344-{HouseBill.ABL_BillNumber}/123456
ERR/113TRANSFER LINE IGNORED
";
			var manifestHeader = ManifestHeader;
			var transferHeaderOld = ArrivalHeader.TransferHeaders.AddNew();
			transferHeaderOld.ATF_TransferType = ManifestBase.TransferTypeList.Codes.Domestic;
			var transferBillOld = transferHeaderOld.TransferBills.AddNew();
			transferBillOld.ATB_ABL_Bill = HouseBill.PK;
			transferBillOld.ATB_MessageStatus = "TCD";
			var transferHeaderCurrent = ArrivalHeader.TransferHeaders.AddNew();
			transferHeaderCurrent.ATF_TransferType = ManifestBase.TransferTypeList.Codes.Domestic;
			var transferBillCurrent = transferHeaderCurrent.TransferBills.AddNew();
			transferBillCurrent.ATB_ABL_Bill = HouseBill.PK;
			var anotherArrivalHeader = ManifestHeader.ArrivalHeaders.AddNew();
			anotherArrivalHeader.ATH_VoyageFlightNo = "QF101";
			anotherArrivalHeader.ATH_ETAAtDischargePort = new ZDateTime(2020, 12, 13);
			var transferHeaderForAnotherArrival = anotherArrivalHeader.TransferHeaders.AddNew();
			transferHeaderForAnotherArrival.ATF_TransferType = ManifestBase.TransferTypeList.Codes.Domestic;
			var transferBillForAnotherArrival = transferHeaderForAnotherArrival.TransferBills.AddNew();
			transferBillForAnotherArrival.ATB_ABL_Bill = HouseBill.PK;
			transferBillForAnotherArrival.ATB_SystemCreateTimeUtc = ZDateTime.Now.AddSeconds(30);
			manifestHeader.Factory.Save();
			CombineAssertions(() =>
			{
				ProcessMessage(responseMessageText, HouseBill);
				AssertEquals("Message status", "TER", transferBillCurrent.ATB_MessageStatus);
				AssertNotEquals("Old transfer bill unchanged", "TER", transferBillOld.HasChanges);
				AssertNotEquals("Transfer bill for another arrival unchanged", "TER", transferBillForAnotherArrival.ATB_MessageStatus);
				AssertEquals("House bill updated", "ERR", HouseBill.ABL_MessageStatus);
			});
		}

		public void TestUpdatesStatus_Transfer_TER()
		{
			var manifestHeader = ManifestHeader;
			var transferHeaderOld = ArrivalHeader.TransferHeaders.AddNew();
			transferHeaderOld.ATF_TransferType = ManifestBase.TransferTypeList.Codes.Domestic;
			var transferBillOld = transferHeaderOld.TransferBills.AddNew();
			transferBillOld.ATB_ABL_Bill = HouseBill.PK;
			transferBillOld.ATB_MessageStatus = "TCD";
			var transferHeaderCurrent = ArrivalHeader.TransferHeaders.AddNew();
			transferHeaderCurrent.ATF_TransferType = ManifestBase.TransferTypeList.Codes.Domestic;
			var transferBillCurrent = transferHeaderCurrent.TransferBills.AddNew();
			transferBillCurrent.ATB_ABL_Bill = HouseBill.PK;
			var anotherArrivalHeader = ManifestHeader.ArrivalHeaders.AddNew();
			anotherArrivalHeader.ATH_VoyageFlightNo = "QF101";
			anotherArrivalHeader.ATH_ETAAtDischargePort = new ZDateTime(2020, 12, 13);
			var transferHeaderForAnotherArrival = anotherArrivalHeader.TransferHeaders.AddNew();
			transferHeaderForAnotherArrival.ATF_TransferType = ManifestBase.TransferTypeList.Codes.Domestic;
			var transferBillForAnotherArrival = transferHeaderForAnotherArrival.TransferBills.AddNew();
			transferBillForAnotherArrival.ATB_ABL_Bill = HouseBill.PK;
			transferBillForAnotherArrival.ATB_SystemCreateTimeUtc = ZDateTime.Now.AddSeconds(30);
			manifestHeader.Factory.Save();

			var responseMessageText = $@"FER
QF101/12DEC
081-11223344-{HouseBill.ABL_BillNumber}/123456
ERR/000COMPLETE TRANSACTION REJECTED
ERR/179NO ACTIVE TRANSFER ROUTING
";

			CombineAssertions(() =>
			{
				ProcessMessage(responseMessageText, HouseBill);
				AssertEquals("Message status", "TER", transferBillCurrent.ATB_MessageStatus);
				AssertNotEquals("Old transfer bill unchanged", "TER", transferBillOld.HasChanges);
				AssertNotEquals("Transfer bill for another arrival unchanged", "TER", transferBillForAnotherArrival.ATB_MessageStatus);
				AssertEquals("House bill updated", "ERR", HouseBill.ABL_MessageStatus);
			});
		}

		#region Calculated Arrival Date

		[TestDate(2021, 11, 1)]
		public void TestFERMessageCalculatedArrivalDate_EmptyEstDate()
		{
			var responseMessageText = @"FER
QF101/01JUL
081-11223344-HAWB123/123456
";

			var manifestHeader = ManifestHeader;
			AssertCalculatedScheduledArrivalDate(manifestHeader, responseMessageText, "Default to the year from EstimateScheduledArrivalDate of Arrival Wrapper.", ZDate.Empty, new ZDate(2022, 07, 01));
		}

		[TestDate(2021, 11, 1)]
		public void TestFERMessageCalculatedArrivalDate_ValidEstDate()
		{
			var responseMessageText = @"FER
QF101/01JUL
081-11223344-HAWB123/123456
";

			var manifestHeader = ManifestHeader;
			AssertCalculatedScheduledArrivalDate(manifestHeader, responseMessageText, "Should consider the year from AMA_E_ARV of manifestHeader.", new ZDate(2021, 06, 01), new ZDate(2021, 07, 01));
		}

		[TestDate(2021, 11, 1)]
		public void TestFERMessageCalculatedArrivalDate_CloseToNewYear()
		{
			var responseMessageText = @"FER
QF101/01JAN
081-11223344-HAWB123/123456
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
				.FirstOrDefault() as FERMessage;

			AssertNotNull(inboundMessage);
			AssertEquals(message, expectedArrivalDate, inboundMessage.CalculatedArrivalDate);
		}

		#endregion

		#region Implementation

		protected override IAIMMessageTypeProcessor GetNewMessageTypeProcessor() => new FERMessageTypeProcessor(new LoggingInformation());
		protected override AIMInboundMessage GetNewInboundMessage() => Factory.New<FERMessage>();
		protected override ZString ExpectedEmailMessageType => "Freight Error Report (FER)";

		protected override void PopulateManifestHeader(AsycudaManifestHeader manifestHeader)
		{
			manifestHeader.AMA_Voyage = "QF101";
			manifestHeader.AMA_E_ARV = new ZDateTime(2020, 12, 12);
		}

		protected override void PopulateHouseBill(AsycudaBill houseBill)
		{
			houseBill.ABL_SenderReference = "123456";
			houseBill.ABL_MessageStatus = ZString.Empty;
		}

		protected override void PopulateArrivalHeader(AsycudaArrivalHeader arrivalHeader)
		{
			arrivalHeader.ATH_VoyageFlightNo = "QF101";
			arrivalHeader.ATH_ETAAtDischargePort = new ZDateTime(2020, 12, 12, 6, 15, 33);
		}

		protected override ZString GetResponseMessageForGroupNotificationTesting()
		{
			return @"FER
QF101/12DEC
081-11223344-HAWB123/123456
ERR/001ERROR DESCRIPTION
ERR/002ANOTHER ERROR
";
		}

		#endregion
	}
}
