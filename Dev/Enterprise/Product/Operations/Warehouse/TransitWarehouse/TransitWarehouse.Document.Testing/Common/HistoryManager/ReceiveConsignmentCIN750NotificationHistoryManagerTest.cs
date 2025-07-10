using CargoWise.Definitions;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;

namespace Enterprise.Warehouse.Transit.Document.Testing
{
	public class ReceiveConsignmentCIN750NotificationHistoryManagerTest : CIN750NotificationHistoryManagerTest<WhsItemReceiveConsignment>
	{
		public void TestGetNextMessageType_NoMessagesHasBeenSent() => TestGetNextMessageType_NoMessagesHasBeenSentCore(null);

		public void TestGetNextMessageType_NoMessagesHasBeenSent_WantToSendInMessage() => TestGetNextMessageType_NoMessagesHasBeenSentCore(CIN750NotificationMessageTypes.CIN750InNotification);

		public void TestGetNextMessageType_NoMessagesHasBeenSent_WantToSendCorMessage() => TestGetNextMessageType_NoMessagesHasBeenSentCore(CIN750NotificationMessageTypes.CIN750CorNotification);

		void TestGetNextMessageType_NoMessagesHasBeenSentCore(CIN750NotificationMessageTypes? expectedMessageType)
		{
			var (rcn, rtu) = CreateRCNAndRTU();
			var packageState1 = Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P1", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			packageState1.Package.KP_GoodsDescription = "Test Description";
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P2", TransitWarehouseStatuses.Codes.Arrived, weight: 800, weightUQ: "G", receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P3", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P4", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P5", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);

			Factory.Save();

			var historyManager = new ReceiveConsignmentCIN750NotificationHistoryManager(rcn);
			var (messageType, errorMessage) = historyManager.GetNextMessageTypeCore(expectedMessageType);

			if (CIN750NotificationMessageTypes.CIN750InNotification == expectedMessageType)
			{
				AssertNotNull(messageType);
				AssertEquals(CIN750NotificationMessageTypes.CIN750InNotification, messageType.Value);
				AssertEquals(string.Empty, errorMessage);
			}
			else if (CIN750NotificationMessageTypes.CIN750CorNotification == expectedMessageType)
			{
				AssertNotNull(messageType);
				AssertEquals(CIN750NotificationMessageTypes.CIN750InNotification, messageType.Value);
				AssertEquals(null, errorMessage);
			}
			else
			{
				AssertNotNull(messageType);
				AssertEquals(CIN750NotificationMessageTypes.CIN750InNotification, messageType.Value);
				AssertEquals(string.Empty, errorMessage);
			}
		}

		public void TestGetNextMessageType_ReportedInIsLessThanReceived() => TestGetNextMessageType_ReportedInIsLessThanReceivedCore(null);

		public void TestGetNextMessageType_ReportedInIsLessThanReceived_WantToSendInMessage() => TestGetNextMessageType_ReportedInIsLessThanReceivedCore(CIN750NotificationMessageTypes.CIN750InNotification);

		public void TestGetNextMessageType_ReportedInIsLessThanReceived_WantToSendCorMessage() => TestGetNextMessageType_ReportedInIsLessThanReceivedCore(CIN750NotificationMessageTypes.CIN750CorNotification);

		void TestGetNextMessageType_ReportedInIsLessThanReceivedCore(CIN750NotificationMessageTypes? expectedMessageType)
		{
			var (rcn, rtu) = CreateRCNAndRTU();
			var packageState1 = Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P1", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			packageState1.Package.KP_GoodsDescription = "Test Description";
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P2", TransitWarehouseStatuses.Codes.Arrived, weight: 800, weightUQ: "G", receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P3", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P4", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P5", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);

			Helper.CreateStmALog(rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=MB-1234|MST=CIN750InNotification|OTY=4|PTP=REF|RFN=EDIDATRC0000001|WGT=8.000");
			Factory.Save();

			var historyManager = new ReceiveConsignmentCIN750NotificationHistoryManager(rcn);
			var (messageType, errorMessage) = historyManager.GetNextMessageTypeCore(expectedMessageType);

			if (CIN750NotificationMessageTypes.CIN750InNotification == expectedMessageType)
			{
				AssertNotNull(messageType);
				AssertEquals(CIN750NotificationMessageTypes.CIN750InNotification, messageType.Value);
				AssertEquals(string.Empty, errorMessage);
			}
			else if (CIN750NotificationMessageTypes.CIN750CorNotification == expectedMessageType)
			{
				AssertNotNull(messageType);
				AssertEquals(CIN750NotificationMessageTypes.CIN750InNotification, messageType.Value);
				AssertEquals(null, errorMessage);
			}
			else
			{
				AssertNotNull(messageType);
				AssertEquals(CIN750NotificationMessageTypes.CIN750InNotification, messageType.Value);
				AssertEquals(string.Empty, errorMessage);
			}
		}

		public void TestGetNextMessageType_ReportedInIsEqualToReceived() => TestGetNextMessageType_ReportedInIsEqualToReceivedCore(null);

		public void TestGetNextMessageType_ReportedInIsEqualToReceived_WantToSendInMessage() => TestGetNextMessageType_ReportedInIsEqualToReceivedCore(CIN750NotificationMessageTypes.CIN750InNotification);

		public void TestGetNextMessageType_ReportedInIsEqualToReceived_WantToSendCorMessage() => TestGetNextMessageType_ReportedInIsEqualToReceivedCore(CIN750NotificationMessageTypes.CIN750CorNotification);

		void TestGetNextMessageType_ReportedInIsEqualToReceivedCore(CIN750NotificationMessageTypes? expectedMessageType)
		{
			var (rcn, rtu) = CreateRCNAndRTU();
			var packageState1 = Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P1", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			packageState1.Package.KP_GoodsDescription = "Test Description";
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P2", TransitWarehouseStatuses.Codes.Arrived, weight: 800, weightUQ: "G", receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P3", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P4", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P5", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);

			Helper.CreateStmALog(rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=MB-1234|MST=CIN750InNotification|OTY=5|PTP=REF|RFN=EDIDATRC0000001|WGT=8.800");
			Factory.Save();

			var historyManager = new ReceiveConsignmentCIN750NotificationHistoryManager(rcn);
			var (messageType, errorMessage) = historyManager.GetNextMessageTypeCore(expectedMessageType);

			if (CIN750NotificationMessageTypes.CIN750InNotification == expectedMessageType)
			{
				AssertNull(messageType);
				AssertEquals("Cannot send CIN 750 In Notification because the Packages on the Receive Consignment are reported completely.", errorMessage);
			}
			else if (CIN750NotificationMessageTypes.CIN750CorNotification == expectedMessageType)
			{
				AssertNull(messageType);
				AssertEquals("Cannot send CIN 750 Cor Notification because the Packages on the Receive Consignment are reported completely.", errorMessage);
			}
			else
			{
				AssertNull(messageType);
				AssertEquals("Cannot send CIN 750 Notification because the Packages on the Receive Consignment are reported completely.", errorMessage);
			}
		}

		public void TestGetNextMessageType_ReportedInIsEqualToReceivedThenAdjustOut() => TestGetNextMessageType_ReportedInIsEqualToReceivedThenAdjustOutCore(null);

		public void TestGetNextMessageType_ReportedInIsEqualToReceivedThenAdjustOut_WantToSendInMessage() => TestGetNextMessageType_ReportedInIsEqualToReceivedThenAdjustOutCore(CIN750NotificationMessageTypes.CIN750InNotification);

		public void TestGetNextMessageType_ReportedInIsEqualToReceivedThenAdjustOut_WantToSendCorMessage() => TestGetNextMessageType_ReportedInIsEqualToReceivedThenAdjustOutCore(CIN750NotificationMessageTypes.CIN750CorNotification);

		void TestGetNextMessageType_ReportedInIsEqualToReceivedThenAdjustOutCore(CIN750NotificationMessageTypes? expectedMessageType)
		{
			var (rcn, rtu) = CreateRCNAndRTU();
			var packageState1 = Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P1", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			packageState1.Package.KP_GoodsDescription = "Test Description";
			var packageState2 = Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P2", TransitWarehouseStatuses.Codes.Arrived, weight: 800, weightUQ: "G", receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P3", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P4", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P5", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);

			Helper.CreateStmALog(rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=MB-1234|MST=CIN750InNotification|OTY=5|PTP=REF|RFN=EDIDATRC0000001|WGT=8.800");
			packageState2.WPS_Status = TransitWarehouseStatuses.Codes.AdjustedOut;
			packageState2.WPS_AdjustedOut = "OTH";

			Factory.Save();

			var historyManager = new ReceiveConsignmentCIN750NotificationHistoryManager(rcn);
			var (messageType, errorMessage) = historyManager.GetNextMessageTypeCore(expectedMessageType);

			if (CIN750NotificationMessageTypes.CIN750InNotification == expectedMessageType)
			{
				AssertNotNull(messageType);
				AssertEquals(CIN750NotificationMessageTypes.CIN750CorNotification, messageType.Value);
				AssertEquals("Cannot send CIN 750 In Notification because Reported In Quantity is greater than the Arrived Packages Quantity. Please send CIN 750 Cor Notification.", errorMessage);
			}
			else if (CIN750NotificationMessageTypes.CIN750CorNotification == expectedMessageType)
			{
				AssertNotNull(messageType);
				AssertEquals(CIN750NotificationMessageTypes.CIN750CorNotification, messageType.Value);
				AssertEquals(string.Empty, errorMessage);
			}
			else
			{
				AssertNotNull(messageType);
				AssertEquals(CIN750NotificationMessageTypes.CIN750CorNotification, messageType.Value);
				AssertEquals(string.Empty, errorMessage);
			}
		}

		public void TestGetNextMessageType_ReportedInIsEqualToReceivedThenAdjustOutAndReportCor() => TestGetNextMessageType_ReportedInIsEqualToReceivedThenAdjustOutAndReportCorMessageCore(null);

		public void TestGetNextMessageType_ReportedInIsEqualToReceivedThenAdjustOutAndReportCor_WantToSendInMessage() => TestGetNextMessageType_ReportedInIsEqualToReceivedThenAdjustOutAndReportCorMessageCore(CIN750NotificationMessageTypes.CIN750InNotification);

		public void TestGetNextMessageType_ReportedInIsEqualToReceivedThenAdjustOutAndReportCor_WantToSendCorMessage() => TestGetNextMessageType_ReportedInIsEqualToReceivedThenAdjustOutAndReportCorMessageCore(CIN750NotificationMessageTypes.CIN750CorNotification);

		void TestGetNextMessageType_ReportedInIsEqualToReceivedThenAdjustOutAndReportCorMessageCore(CIN750NotificationMessageTypes? expectedMessageType)
		{
			var (rcn, rtu) = CreateRCNAndRTU();
			var packageState1 = Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P1", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			packageState1.Package.KP_GoodsDescription = "Test Description";
			var packageState2 = Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P2", TransitWarehouseStatuses.Codes.Arrived, weight: 800, weightUQ: "G", receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P3", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P4", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P5", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			Helper.CreateStmALog(rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=MB-1234|MST=CIN750InNotification|OTY=5|PTP=REF|RFN=EDIDATRC0000001|WGT=8.800");

			packageState2.WPS_Status = TransitWarehouseStatuses.Codes.AdjustedOut;
			packageState2.WPS_AdjustedOut = "OTH";
			Helper.CreateStmALog(rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750CorNotification|OTY=-1|PTP=REF|RFN=EDIDATRC0000001|WGT=-0.800");

			Factory.Save();

			var historyManager = new ReceiveConsignmentCIN750NotificationHistoryManager(rcn);
			var (messageType, errorMessage) = historyManager.GetNextMessageTypeCore(expectedMessageType);

			if (CIN750NotificationMessageTypes.CIN750InNotification == expectedMessageType)
			{
				AssertNull(messageType);
				AssertEquals("Cannot send CIN 750 In Notification because the Packages on the Receive Consignment are reported completely.", errorMessage);
			}
			else if (CIN750NotificationMessageTypes.CIN750CorNotification == expectedMessageType)
			{
				AssertNull(messageType);
				AssertEquals("Cannot send CIN 750 Cor Notification because the Packages on the Receive Consignment are reported completely.", errorMessage);
			}
			else
			{
				AssertNull(messageType);
				AssertEquals("Cannot send CIN 750 Notification because the Packages on the Receive Consignment are reported completely.", errorMessage);
			}
		}

		public void TestGetNextMessageType_ReportedWeightIsEqualToArrived()
		{
			var (rcn, rtu) = CreateRCNAndRTU();
			var packageState1 = Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P1", TransitWarehouseStatuses.Codes.Arrived, weight: 44, weightUQ: "LB", receiveUnit: rtu);
			packageState1.Package.KP_GoodsDescription = "Test Description";

			var reportedWeight = Core.Constants.Weight.Convert(packageState1.Package.KP_Weight, packageState1.Package.KP_WeightUQ, Core.Constants.Weight.Kilograms).RoundTo3Digits();
			Helper.CreateStmALog(rcn, EventCodes.MessageSent, $"|HBL=-|JOB=RC0000001|MBL=MB-1234|MST=CIN750InNotification|OTY=1|PTP=REF|RFN=EDIDATRC0000001|WGT={reportedWeight}");

			Factory.Save();

			var historyManager = new ReceiveConsignmentCIN750NotificationHistoryManager(rcn);
			var messageType = historyManager.GetNextMessageType();
			AssertNull(messageType);
		}

		public void TestGetNextMessageType_ReportedWeightGreaterThanArrived() => TestGetNextMessageType_ReportedWeightDifferentFromArrivedCore(true, null);
		public void TestGetNextMessageType_ReportedWeightLessThanArrived() => TestGetNextMessageType_ReportedWeightDifferentFromArrivedCore(false, null);
		public void TestGetNextMessageType_ReportedWeightGreaterThanArrived_ExpectIn() => TestGetNextMessageType_ReportedWeightDifferentFromArrivedCore(true, CIN750NotificationMessageTypes.CIN750InNotification);
		public void TestGetNextMessageType_ReportedWeightLessThanArrived_ExpectIn() => TestGetNextMessageType_ReportedWeightDifferentFromArrivedCore(false, CIN750NotificationMessageTypes.CIN750InNotification);
		public void TestGetNextMessageType_ReportedWeightGreaterThanArrived_ExpectCor() => TestGetNextMessageType_ReportedWeightDifferentFromArrivedCore(true, CIN750NotificationMessageTypes.CIN750CorNotification);
		public void TestGetNextMessageType_ReportedWeightLessThanArrived_ExpectCor() => TestGetNextMessageType_ReportedWeightDifferentFromArrivedCore(false, CIN750NotificationMessageTypes.CIN750CorNotification);

		void TestGetNextMessageType_ReportedWeightDifferentFromArrivedCore(bool isReportedWeightGreater, CIN750NotificationMessageTypes? expectedMessageType)
		{
			var (rcn, rtu) = CreateRCNAndRTU();
			var packageState1 = Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P1", TransitWarehouseStatuses.Codes.Arrived, weight: 5, weightUQ: "KG", receiveUnit: rtu);
			packageState1.Package.KP_GoodsDescription = "Test Description";

			if (isReportedWeightGreater)
			{
				Helper.CreateStmALog(rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=MB-1234|MST=CIN750InNotification|OTY=1|PTP=REF|RFN=EDIDATRC0000001|WGT=6.000");
			}
			else
			{
				Helper.CreateStmALog(rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=MB-1234|MST=CIN750InNotification|OTY=1|PTP=REF|RFN=EDIDATRC0000001|WGT=4.000");
			}
			Factory.Save();

			var historyManager = new ReceiveConsignmentCIN750NotificationHistoryManager(rcn);
			var (messageType, errorMessage) = historyManager.GetNextMessageTypeCore(expectedMessageType);

			AssertNotNull(messageType);
			if (expectedMessageType == null)
			{
				AssertEquals(CIN750NotificationMessageTypes.CIN750CorNotification, messageType.Value);
				AssertEquals(string.Empty, errorMessage);
			}
			else if (expectedMessageType == CIN750NotificationMessageTypes.CIN750CorNotification)
			{
				AssertEquals(CIN750NotificationMessageTypes.CIN750CorNotification, messageType.Value);
				AssertEquals(string.Empty, errorMessage);
			}
			else if (expectedMessageType == CIN750NotificationMessageTypes.CIN750InNotification)
			{
				AssertEquals(CIN750NotificationMessageTypes.CIN750CorNotification, messageType.Value);
				if (isReportedWeightGreater)
				{
					AssertEquals("Cannot send CIN 750 In Notification because Reported In Weight is greater than the Arrived Packages Weight. Please send CIN 750 Cor Notification.", errorMessage);
				}
				else
				{
					AssertEquals("Cannot send CIN 750 In Notification because Reported In Weight is less than the Arrived Packages Weight. Please send CIN 750 Cor Notification.", errorMessage);
				}
			}
		}

		(WhsItemReceiveConsignment rcn, WhsItemReceiveTransportationUnit rtu) CreateRCNAndRTU()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch("WH1");
			warehouse.WarehouseAddress.Address1 = "WH1Address";
			Helper.AddOrgCode(warehouse.WarehouseAddress, OrgCusCode.FranceCodeTypes.CIN, "C001");

			var rcn = Helper.CreateReceiveConsignment("RC0000001", warehouse.PK);
			Helper.CreateAdditionalReference(rcn, "MB-1234", AdditionalReferenceTypes.Codes.MasterBill);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", rcn.Warehouse.PK, rcn.Warehouse.DefaultInboundDockDoorLocation.PK);
			var ctoAddress = Helper.CreateClient().MainAddress;
			ctoAddress.Address1 = "CTOAddress";
			var ctoJobDocAddress = Helper.CreateJobDocAddressFromAddress(rcn, DocAddressTypes.Codes.ArrivalCTOAddress, ctoAddress);

			Helper.AddOrgCode(ctoJobDocAddress.Address, OrgCusCode.FranceCodeTypes.CIN, "C002");
			Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "CEN1");
			rcn.WRC_Direction = TransitWarehouseConsignmentDirections.Codes.Domestic;

			return (rcn, rtu);
		}

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper helper;
	}
}
