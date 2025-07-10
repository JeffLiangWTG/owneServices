using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.Warehouse.Transit.Document.Common;

namespace Enterprise.Warehouse.Transit.Document.Testing
{
	public class CIN750NotificationValidationTest : TestCaseWithFactory
	{
		#region TestGetInNotificationValidationMessage

		public void TestGetInNotificationValidationMessage_HistoryTypeNotMatch_AWB() => TestGetInNotificationValidationMessage("|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=5|PTP=REF|RFN=EDIDATRC0000001|WGT=8.8",
			"Cannot Send CIN 750 In Notification with Reference Type Master Air Waybill because it does not match the history Notifications.",
			createMasterBill: true);

		public void TestGetInNotificationValidationMessage_HistoryTypeNotMatch_REF() => TestGetInNotificationValidationMessage("|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=5|PTP=AWB|RFN=MasterBill001|WGT=8.8",
			"Cannot Send CIN 750 In Notification with Reference Type Reference because it does not match the history Notifications.");

		public void TestGetInNotificationValidationMessage_HistoryCodeNotMatch() => TestGetInNotificationValidationMessage("|HBL=-|JOB=RC0000002|MBL=-|MST=CIN750InNotification|OTY=5|PTP=REF|RFN=EDIDATRC0000002|WGT=8.8",
			"Cannot Send CIN 750 In Notification with Reference Code EDIDATRC0000001 because it does not match the history Notifications.");

		public void TestGetInNotificationValidationMessage_HistoryCodeMatch_AWB() => TestGetInNotificationValidationMessage("|HBL=-|JOB=RC0000002|MBL=MB-1234|MST=CIN750InNotification|OTY=3|PTP=AWB|RFN=EDIDATRC0000002|WGT=3.8", string.Empty, true);

		public void TestGetInNotificationValidationMessage_ReceivedPackageQuantityTotallyReported() => TestGetInNotificationValidationMessage("|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=5|PTP=REF|RFN=EDIDATRC0000001|WGT=10",
			"Cannot Send CIN 750 In Notification because the reported Packages Quantity (5) is equal to the Quantity of Received Packages.",
			quantityOverridden: 5);

		public void TestGetInNotificationValidationMessage_ReceivedPackageQuantityOverReported() => TestGetInNotificationValidationMessage("|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=8|PTP=REF|RFN=EDIDATRC0000001|WGT=10",
			"Cannot Send CIN 750 In Notification because the reported Packages Quantity (8) is greater than the Quantity of Received Packages (5).");

		public void TestGetInNotificationValidationMessage_NotificationPackageQuantityOverLimit() => TestGetInNotificationValidationMessage("|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=3|PTP=REF|RFN=EDIDATRC0000001|WGT=10",
			"Cannot Send CIN 750 In Notification because Amount Quantity (5) is greater than the Quantity of Received Packages that haven't been reported (2).",
			quantityOverridden: 5);

		public void TestGetInNotificationValidationMessage_ReceivedPackageWeightTotallyReported() => TestGetInNotificationValidationMessage("|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=3|PTP=REF|RFN=EDIDATRC0000001|WGT=10",
			"Cannot Send CIN 750 In Notification because the reported Packages Weight (10.000) is equal to the Weight of Received Packages.");

		public void TestGetInNotificationValidationMessage_ReceivedPackageWeightOverReported() => TestGetInNotificationValidationMessage("|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=3|PTP=REF|RFN=EDIDATRC0000001|WGT=15",
			"Cannot Send CIN 750 In Notification because the reported Packages Weight (15.000) is greater than the Weight of Received Packages (10.000).");

		public void TestGetInNotificationValidationMessage_NotificationPackageWeightOverLimit() => TestGetInNotificationValidationMessage("|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=3|PTP=REF|RFN=EDIDATRC0000001|WGT=6",
			"Cannot Send CIN 750 In Notification because Amount Weight (10.000) is greater than the Weight of Received Packages that haven't been reported (4.000).",
			weightOverridden: 10);

		void TestGetInNotificationValidationMessage(string historyReference, string expectedMessage, bool createMasterBill = false, int quantityOverridden = 0, int weightOverridden = 0)
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch("WH1");
			warehouse.WarehouseAddress.Address1 = "WH1Address";
			Helper.AddOrgCode(warehouse.WarehouseAddress, OrgCusCode.FranceCodeTypes.CIN, "C001");

			var rcn = Helper.CreateReceiveConsignment("RC0000001", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", rcn.Warehouse.PK, rcn.Warehouse.DefaultInboundDockDoorLocation.PK);

			if (createMasterBill)
			{
				Helper.CreateAdditionalReference(rcn, "MB-1234", AdditionalReferenceTypes.Codes.MasterBill);
			}

			var ctoOrg = Factory.NewWithValidTestData<OrgHeader>();
			var ctoAddress = Factory.NewWithValidTestData<OrgAddress>();
			ctoAddress.Address1 = "CTOAddress";
			var ctoJobDocAddress = Helper.CreateJobDocAddressFromAddress(rcn, DocAddressTypes.Codes.ArrivalCTOAddress, ctoAddress);

			Helper.AddOrgCode(ctoJobDocAddress.Address, OrgCusCode.FranceCodeTypes.CIN, "C002");

			var cen = Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "CEN1");
			Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration, "TST1");

			var packageState1 = Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P1", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P2", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P3", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P4", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P5", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			packageState1.Package.KP_GoodsDescription = "Test Description";

			rcn.WRC_Direction = TransitWarehouseConsignmentDirections.Codes.Domestic;

			Helper.CreateStmALog(rcn, EventCodes.MessageSent, historyReference);

			Factory.Save();

			var notification = new CIN750InNotificationBuilder(rcn).Build();
			if (quantityOverridden != 0)
			{
				notification.Goods.Single().AmountQuantity = quantityOverridden;
			}
			if (weightOverridden != 0)
			{
				notification.Goods.Single().AmountWeight = weightOverridden;
			}

			var validationResult = CIN750NotificationValidation.GetInNotificationValidationMessage(notification);

			AssertEquals("Error Message", expectedMessage, validationResult);
		}

		public void TestGetInNotificationMessage_WeightPrecision()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch("WH1");
			warehouse.WarehouseAddress.Address1 = "WH1Address";
			Helper.AddOrgCode(warehouse.WarehouseAddress, OrgCusCode.FranceCodeTypes.CIN, "C001");

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			var rcn1 = Helper.CreateReceiveConsignment("RC0000001", warehouse.PK);
			// 35 LB = 15.875 733 KG, round to 15.876 KG
			var packageState1 = Helper.CreatePackageState(rcn1, 1, PackType.Freight.PKG, "P1", TransitWarehouseStatuses.Codes.Arrived, weight: 35, weightUQ: "LB", receiveUnit: rtu);
			packageState1.Package.KP_GoodsDescription = "Test Description";

			var rcn2 = Helper.CreateReceiveConsignment("RC0000002", warehouse.PK);
			// 16 LB = 7.257 478 KG, round to 7.257 KG
			var packageState2 = Helper.CreatePackageState(rcn2, 1, PackType.Freight.BOX, "P2", TransitWarehouseStatuses.Codes.Arrived, weight: 16, weightUQ: "LB", receiveUnit: rtu);
			packageState2.Package.KP_GoodsDescription = "Test Description 2";
			Factory.Save();

			var notification = new CIN750InNotificationBuilder(rcn1).Build();
			var validationResult = CIN750NotificationValidation.GetInNotificationValidationMessage(notification);
			AssertNullOrEmpty(validationResult);
			AssertEquals(notification.Goods.First().AmountWeight, 15.876m);

			notification = new CIN750InNotificationBuilder(rcn2).Build();
			validationResult = CIN750NotificationValidation.GetInNotificationValidationMessage(notification);
			AssertNullOrEmpty(validationResult);
			AssertEquals(notification.Goods.First().AmountWeight, 7.257m);

			Helper.CreateStmALog(rcn2, EventCodes.MessageSent, "|HBL=-|JOB=RC0000002|MBL=-|MST=CIN750InNotification|OTY=1|PTP=REF|RFN=EDIDATRC0000002|WGT=7.257");

			// 16 LB + 16 LB = 15.514 956 KG, round to 15.515 KG = 7.257 KG + 7.258 KG
			var packageState3 = Helper.CreatePackageState(rcn2, 1, PackType.Freight.BOX, "P2", TransitWarehouseStatuses.Codes.Arrived, weight: 16, weightUQ: "LB", receiveUnit: rtu);
			packageState3.Package.KP_GoodsDescription = "Test Description 2";

			Factory.Save();

			notification = new CIN750InNotificationBuilder(rcn2).Build();
			validationResult = CIN750NotificationValidation.GetInNotificationValidationMessage(notification);
			AssertNullOrEmpty(validationResult);
			AssertEquals(notification.Goods.First().AmountWeight, 7.258m);
		}

		#endregion

		#region TestGetDeconsNotificationValidationMessage

		public void TestGetDeconsNotificationValidationMessage_NoGoodsDetails()
		{
			var data = SetupDataForDeconsNotificationValidation();
			Factory.Save();

			TestGetDeconsNotificationValidationMessage(data.dcn, "Cannot Send CIN 750 Deconsolidation Notification without Goods Detail.");
		}

		public void TestGetDeconsNotificationValidationMessage_FromQuantityIsGreaterThanToQuantity()
		{
			var data = SetupDataForDeconsNotificationValidation();
			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=MAB-1|MST=CIN750InNotification|OTY=2|PTP=AWB|RFN=EDIDATRC0000001|WGT=6");
			var packageState1 = Helper.CreatePackageState(data.rcn, 1, PackType.Freight.PKG, "P1", TransitWarehouseStatuses.Codes.Departed, weight: 2, receiveUnit: data.rtu, dispatchConsignment: data.dcn, dispatchLoadList: data.dll, dispatchUnit: data.dtu);
			packageState1.Package.KP_GoodsDescription = "BOOKS";
			Helper.CreatePackageState(data.rcn, 1, PackType.Freight.PKG, "P2", TransitWarehouseStatuses.Codes.Departed, weight: 4, receiveUnit: data.rtu, dispatchConsignment: data.dcn, dispatchLoadList: data.dll, dispatchUnit: data.dtu);

			Factory.Save();

			TestGetDeconsNotificationValidationMessage(data.dcn, "Cannot Send CIN 750 Deconsolidation Notification because from goods amount (2) is greater than to goods amount (1).", toQuantity: 1);
		}

		public void TestGetDeconsNotificationValidationMessage_FromQuantityIsGreaterThanReportedInQuantity()
		{
			var data = SetupDataForDeconsNotificationValidation();
			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=MAB-1|MST=CIN750InNotification|OTY=2|PTP=AWB|RFN=EDIDATRC0000001|WGT=2");
			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=MAB-1|MST=CIN750CorNotification|OTY=-1|PTP=AWB|RFN=EDIDATRC0000001|WGT=-1");
			var packageState1 = Helper.CreatePackageState(data.rcn, 1, PackType.Freight.PKG, "P1", TransitWarehouseStatuses.Codes.Departed, weight: 2, receiveUnit: data.rtu, dispatchConsignment: data.dcn, dispatchLoadList: data.dll, dispatchUnit: data.dtu);
			packageState1.Package.KP_GoodsDescription = "BOOKS";
			Helper.CreatePackageState(data.rcn, 1, PackType.Freight.PKG, "P2", TransitWarehouseStatuses.Codes.Departed, weight: 4, receiveUnit: data.rtu, dispatchConsignment: data.dcn, dispatchLoadList: data.dll, dispatchUnit: data.dtu);
			Helper.CreatePackageState(data.rcn, 1, "PKG", "P3", TransitWarehouseStatuses.Codes.AdjustedOut, weight: 1m, receiveUnit: data.rtu);
			Factory.Save();

			TestGetDeconsNotificationValidationMessage(data.dcn, "Cannot Send CIN 750 Deconsolidation Notification because the reported Package Quantity (2) is greater than reported In Packages that haven't been reported (1).", fromQuantity: 2);
		}

		public void TestGetDeconsNotificationValidationMessage_FromWeightIsGreaterThanReportedInWeight()
		{
			var data = SetupDataForDeconsNotificationValidation();
			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=MAB-1|MST=CIN750InNotification|OTY=2|PTP=AWB|RFN=EDIDATRC0000001|WGT=4");
			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=MAB-1|MST=CIN750CorNotification|OTY=0|PTP=AWB|RFN=EDIDATRC0000001|WGT=2");
			var packageState1 = Helper.CreatePackageState(data.rcn, 1, PackType.Freight.PKG, "P1", TransitWarehouseStatuses.Codes.Departed, weight: 4, receiveUnit: data.rtu, dispatchConsignment: data.dcn, dispatchLoadList: data.dll, dispatchUnit: data.dtu);
			packageState1.Package.KP_GoodsDescription = "BOOKS";
			Helper.CreatePackageState(data.rcn, 1, PackType.Freight.PKG, "P2", TransitWarehouseStatuses.Codes.Departed, weight: 4, receiveUnit: data.rtu, dispatchConsignment: data.dcn, dispatchLoadList: data.dll, dispatchUnit: data.dtu);

			Factory.Save();

			TestGetDeconsNotificationValidationMessage(data.dcn, "Cannot Send CIN 750 Deconsolidation Notification because the reported Package Weight (7.000) is greater than reported In Packages that haven't been reported (6.000).", fromWeight: 7);
		}

		(WhsItemReceiveConsignment rcn, WhsItemReceiveTransportationUnit rtu, WhsItemDispatchConsignment dcn, WhsItemDispatchTransportationUnit dtu, WhsItemDispatchLoadList dll) SetupDataForDeconsNotificationValidation()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch("WH1");
			warehouse.WarehouseAddress.Address1 = "WH1Address";
			var location = Helper.CreateLocation(warehouse);
			Helper.AddOrgCode(warehouse.WarehouseAddress, OrgCusCode.FranceCodeTypes.CIN, "C001");

			var rcn = Helper.CreateReceiveConsignment("RC0000001", warehouse.PK);
			rcn.WRC_Direction = TransitWarehouseConsignmentDirections.Codes.Domestic;
			Helper.CreateAdditionalReference(rcn, "MAB-1", AdditionalReferenceTypes.Codes.MasterBill);

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK, location);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);

			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			dcn.WDC_HouseBillNumber = "HSB1";

			return (rcn, rtu, dcn, dtu, dll);
		}

		void TestGetDeconsNotificationValidationMessage(WhsItemDispatchConsignment dcn, string expectedMessage, int fromQuantity = 0, int toQuantity = 0, decimal fromWeight = 0, decimal toWeight = 0)
		{
			var notification = new CIN750DeconsNotificationBuilder(dcn).Build();

			if (fromQuantity != 0 || toQuantity != 0 || fromWeight > 0 || toWeight > 0)
			{
				var fromToGoods = notification.GoodsPairs.FirstOrDefault();
				if (fromQuantity != 0)
				{
					fromToGoods.Item1.AmountQuantity = fromQuantity;
				}
				if (toQuantity != 0)
				{
					fromToGoods.Item2.AmountQuantity = toQuantity;
				}
				if (fromWeight > 0)
				{
					fromToGoods.Item1.AmountWeight = fromWeight;
				}
				if (toWeight > 0)
				{
					fromToGoods.Item2.AmountWeight = toWeight;
				}
			}

			var validationResult = CIN750NotificationValidation.GetDeconsNotificationValidationMessage(notification);
			AssertEquals("Error Message", expectedMessage, validationResult);
		}

		#endregion

		#region TestGetOutNotificationValidationMessage

		(WhsItemDispatchConsignment dcn, WhsWarehouse warehouse) SetupDataForOutNotificationValidation(bool createCustomsDocuments = true)
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch("WH1");
			warehouse.WarehouseAddress.Address1 = "WH1Address";
			var location = Helper.CreateLocation(warehouse);
			Helper.AddOrgCode(warehouse.WarehouseAddress, OrgCusCode.FranceCodeTypes.CIN, "C001");

			var rcn = Helper.CreateReceiveConsignment("RC0000001", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK, location);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DC0000001", warehouse.PK);

			rcn.WRC_HouseBillNumber = "HSB1";
			dcn.WDC_HouseBillNumber = "HSB1";
			
			if (createCustomsDocuments)
			{
				Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration, "TSD1");
				var crn = Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "CRN1");
				crn.PopulateAddOnValue("SourceType", "STR", "T1");
			}

			Helper.CreateStmALog(rcn, EventCodes.MessageSent, "|HBL=HSB1|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=1|PTP=REF|RFN=EDIDATRC0000001|WGT=2");

			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P1", TransitWarehouseStatuses.Codes.Departed, weight: 2, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			Factory.Save();

			return (dcn, warehouse);
		}

		public void TestGetOutNotificationValidationMessage_WithoutGoodsDetails()
		{
			var data = SetupDataForOutNotificationValidation();

			var additionalData = new OutNotificationAdditionalData()
			{
				PackageQuantityReadyToOut = 1,
				PackageWeightReadyToOut = 2m,
			};

			var notification = new CIN750OutNotificationBuilder(data.dcn, additionalData).Build();
			notification.Goods = null;

			var validationMessage = CIN750NotificationValidation.GetOutNotificationValidationMessage(notification);
			AssertEquals("Cannot send CIN 750 Out Notification without Goods Detail or multiple Goods Detail.", validationMessage.ToString());
		}

		public void TestGetOutNotificationValidationMessage_MaxQuantityIsZero()
		{
			var data = SetupDataForOutNotificationValidation();

			var additionalData = new OutNotificationAdditionalData()
			{
				PackageQuantityReadyToOut = 1,
				PackageWeightReadyToOut = 2m,
			};

			var notification = new CIN750OutNotificationBuilder(data.dcn, additionalData).Build();
			notification.Goods.First().AmountQuantity = 2;

			var validationMessage = CIN750NotificationValidation.GetOutNotificationValidationMessage(notification);
			AssertEquals("Cannot send CIN 750 Out Notification because Amount Quantity (2) is greater than the Quantity of Departed Packages that haven't been reported (1).", validationMessage.ToString());
		}

		public void TestGetOutNotificationValidationMessage_MaxWeightIsZero()
		{
			var data = SetupDataForOutNotificationValidation();

			var additionalData = new OutNotificationAdditionalData()
			{
				PackageQuantityReadyToOut = 1,
				PackageWeightReadyToOut = 2m
			};

			var notification = new CIN750OutNotificationBuilder(data.dcn, additionalData).Build();
			notification.Goods.First().AmountWeight = 10m;

			var validationMessage = CIN750NotificationValidation.GetOutNotificationValidationMessage(notification);
			AssertEquals("Cannot send CIN 750 Out Notification because Amount Weight (10.000) is greater than the Weight of Departed Packages that haven't been reported (2.000).", validationMessage.ToString());
		}

		public void TestGetOutNotificationValidationMessage_WithoutCustomsDocuments()
		{
			var data = SetupDataForOutNotificationValidation(false);

			var historyManager = new DispatchConsignmentCIN750NotificationHistoryManager(data.dcn);
			var result = historyManager.GetNextMessageTypeCore(CIN750NotificationMessageTypes.CIN750OutNotification);
			var notification = new CIN750OutNotificationBuilder(data.dcn, historyManager.AdditionalDataForOut).Build();
			var validationMessage = CIN750NotificationValidation.GetOutNotificationValidationMessage(notification);
			AssertEquals("Customs Documents is required.", validationMessage.ToString());
		}

		#endregion

		#region TestGetCorNotificationValidationMessage
		public void TestGetCorNotificationValidationMessage_InNotificationHasNotSent() => TestGetCorNotificationValidationMessage(
			"Cannot Send CIN 750 Cor Notification because it has not been sent In Notifications.",
			historyReference: "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750CorNotification|OTY=-5|PTP=REF|RFN=EDIDATRC0000001|WGT=-8.8");
		public void TestGetCorNotificationValidationMessage_PartyOfPackageHasNotSent() => TestGetCorNotificationValidationMessage(
			"", false, 0, 2, false,
			"|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=4|PTP=REF|RFN=EDIDATRC0000001|WGT=8.8",
			"|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750CorNotification|OTY=-3|PTP=REF|RFN=EDIDATRC0000001|WGT=-4.8");

		public void TestGetCorNotificationValidationMessage_HistoryTypeNotMatch_AWB() => TestGetCorNotificationValidationMessage(
			"Cannot Send CIN 750 Cor Notification with Reference Type Master Air Waybill because it does not match the history Notifications.",
			createMasterBill: true, 0, 0, false,
			"|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=5|PTP=REF|RFN=EDIDATRC0000001|WGT=8.8",
			"|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750CorNotification|OTY=-3|PTP=REF|RFN=EDIDATRC0000001|WGT=-4.8");

		public void TestGetCorNotificationValidationMessage_HistoryTypeNotMatch_REFWhenCreateMAB() => TestGetCorNotificationValidationMessage(
			"Cannot Send CIN 750 Cor Notification with Reference Code MB1234 because it does not match the history Notifications.",
			createMasterBill: true, 0, 0, false,
			"|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=5|PTP=REF|RFN=EDIDATRC0000001|WGT=8.8",
			"|HBL=-|JOB=RC0000001|MBL=MB-2|MST=CIN750CorNotification|OTY=-3|PTP=AWB|RFN=EDIDATRC0000001|WGT=-4.8");

		public void TestGetCorNotificationValidationMessage_HistoryTypeNotMatch_REF() => TestGetCorNotificationValidationMessage(
			"Cannot Send CIN 750 Cor Notification with Reference Type Reference because it does not match the history Notifications.",
			false, 0, 0, false,
			"|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=5|PTP=REF|RFN=EDIDATRC0000001|WGT=8.8",
			"|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750CorNotification|OTY=-3|PTP=AWB|RFN=MasterBill001|WGT=-4.8");

		public void TestGetCorNotificationValidationMessage_HistoryCodeNotMatch() => TestGetCorNotificationValidationMessage(
			"Cannot Send CIN 750 Cor Notification with Reference Code EDIDATRC0000001 because it does not match the history Notifications.",
			false, 0, 0, false,
			"|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=5|PTP=REF|RFN=EDIDATRC0000001|WGT=8.8",
			"|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750CorNotification|OTY=-3|PTP=REF|RFN=EDIDATRC0000002|WGT=-4.8");

		public void TestGetCorNotificationValidationMessage_CorQuantityGreaterThanIn() => TestGetCorNotificationValidationMessage(
			"Cannot Send CIN 750 Cor Notification because the reported Packages Quantity adjustment (-4) exceeds the Quantity of In Notification (3).",
			false, -2, 0, false,
			"|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=3|PTP=REF|RFN=EDIDATRC0000001|WGT=4.8",
			"|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750CorNotification|OTY=-2|PTP=REF|RFN=EDIDATRC0000001|WGT=-2.8");

		public void TestGetCorNotificationValidationMessage_CorWeightGreaterThanIn() => TestGetCorNotificationValidationMessage(
			"Cannot Send CIN 750 Cor Notification because the reported Packages Weight adjustment (-10.800) exceeds the Weight of In Notification (8.800).",
			false, 0, -8, false,
			"|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=5|PTP=REF|RFN=EDIDATRC0000001|WGT=8.800",
			"|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750CorNotification|OTY=-2|PTP=REF|RFN=EDIDATRC0000001|WGT=-2.800");

		public void TestGetCorNotificationValidationMessage_CorSendsPositiveWeightManually() => TestGetCorNotificationValidationMessage(
			"", false, 0, 3.800m, false,
			"|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=5|PTP=REF|RFN=EDIDATRC0000001|WGT=8.800",
			"|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750CorNotification|OTY=-2|PTP=REF|RFN=EDIDATRC0000001|WGT=-2.800");

		public void TestGetCorNotificationValidationMessage_CorSendsQuantityIsEmpty() => TestGetCorNotificationValidationMessage(
			"",
			false, 0, 2m, true,
			"|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=5|PTP=REF|RFN=EDIDATRC0000001|WGT=8.800",
			"|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750CorNotification|OTY=-2|PTP=REF|RFN=EDIDATRC0000001|WGT=-2.800");

		public void TestGetCorNotificationValidationMessage_CorSendsQuantityAndWeightAreEmpty() => TestGetCorNotificationValidationMessage(
			"Cannot Send CIN 750 Cor Notification because the reported Packages Quantity and Weight are 0.",
			false, 0, 0m, true,
			"|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=5|PTP=REF|RFN=EDIDATRC0000001|WGT=8.800",
			"|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750CorNotification|OTY=-2|PTP=REF|RFN=EDIDATRC0000001|WGT=-2.800");

		void TestGetCorNotificationValidationMessage(string expectedMessage, bool createMasterBill = false, int quantityOverridden = 0, decimal weightOverridden = 0, bool forceOverridden = false, params string[] historyReference)
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch("WH1");
			warehouse.WarehouseAddress.Address1 = "WH1Address";
			Helper.AddOrgCode(warehouse.WarehouseAddress, OrgCusCode.FranceCodeTypes.CIN, "C001");

			var rcn = Helper.CreateReceiveConsignment("RC0000001", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", rcn.Warehouse.PK, rcn.Warehouse.DefaultInboundDockDoorLocation.PK);

			if (createMasterBill)
			{
				Helper.CreateAdditionalReference(rcn, "MB-1234", AdditionalReferenceTypes.Codes.MasterBill);
			}

			var cen = Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "CEN1");
			Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration, "TST1");

			var packageState1 = Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P1", TransitWarehouseStatuses.Codes.AdjustedOut, weight: 2, weightUQ: "KG", receiveUnit: rtu, adjustedOut: "OTH");
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P2", TransitWarehouseStatuses.Codes.AdjustedOut, weight: 800, weightUQ: "G", receiveUnit: rtu, adjustedOut: "OTH");
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P3", TransitWarehouseStatuses.Codes.AdjustedOut, weight: 2, weightUQ: "KG", receiveUnit: rtu, adjustedOut: "OTH");
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P4", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P5", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			packageState1.Package.KP_GoodsDescription = "Test Description";

			rcn.WRC_Direction = TransitWarehouseConsignmentDirections.Codes.Domestic;

			foreach (var history in historyReference)
			{
				helper.CreateStmALog(rcn, EventCodes.MessageSent, history);
			}

			Factory.Save();

			var notification = new CIN750CorNotificationBuilder(rcn).Build();
			if (quantityOverridden != 0 || forceOverridden)
			{
				notification.Goods.Single().AmountQuantity = quantityOverridden;
			}
			if (weightOverridden != 0 || forceOverridden)
			{
				notification.Goods.Single().AmountWeight = weightOverridden;
			}

			var validationResult = CIN750NotificationValidation.GetCorNotificationValidationMessage(notification);

			AssertEquals("Error Message", expectedMessage, validationResult);
		}

		#endregion

		#region TestGetConsNotificationValidationMessage

		public void TestGetConsNotificationValidationMessage_ToRefTypeNotEqualToConsNotificationType()
		{
			var data = SetupDataForConsNotificationValidation();
			data.dcn.WDC_HouseBillNumber = "HSB1";
			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=1|PTP=REF|RFN=EDIDATRC0000001|WGT=2");
			Helper.CreatePackageState(data.rcn, 1, PackType.Freight.PKG, "P1", TransitWarehouseStatuses.Codes.Departed, weight: 2, receiveUnit: data.rtu, dispatchConsignment: data.dcn, dispatchLoadList: data.dll, dispatchUnit: data.dtu);
			Factory.Save();

			TestGetConsNotificationValidationMessage(data.dcn, "Cannot Send CIN 750 Consolidation Notification as reported Ref Type should as same as To Goods Ref Type.", reportRefType: CIN750RefTypes.Codes.Reference);
		}

		public void TestGetConsNotificationValidationMessage_RCNHasId_DCNHasHSB() => TestGetConsNotificationValidationMessage_NoErrorCore(dcnHasHSB: true);

		public void TestGetConsNotificationValidationMessage_RCNHasId_DCNHasHSB_HasCor() => TestGetConsNotificationValidationMessage_NoErrorCore(dcnHasHSB: true, hasCor: true);

		public void TestGetConsNotificationValidationMessage_RCNHasId_DCNHasHSB_DCNHasMAB() => TestGetConsNotificationValidationMessage_NoErrorCore(dcnHasHSB: true, dcnHasMAB: true, needAWBCons: true);

		public void TestGetConsNotificationValidationMessage_RCNHasHSB_DCNHasHSB_HSBDifferent() => TestGetConsNotificationValidationMessage_NoErrorCore(rcnHasHSB: true, dcnHasHSB: true);

		public void TestGetConsNotificationValidationMessage_RCNHasHSB_DCNHasHSB_DCNHasMAB_HSBDifferent() => TestGetConsNotificationValidationMessage_NoErrorCore(rcnHasHSB: true, dcnHasHSB: true, dcnHasMAB: true);

		public void TestGetConsNotificationValidationMessage_RCNHasHSB_DCNHasHSB_DCNHasMAB_HSBSame() => TestGetConsNotificationValidationMessage_NoErrorCore(rcnHasHSB: true, dcnHasHSB: true, dcnHasMAB: true, hsbIsSame: true);

		public void TestGetConsNotificationValidationMessage_RCNHasHSB_RCNHasMAB_DCNHasHSB_DCNHasMAB_HSBSame_MABDifferent() => TestGetConsNotificationValidationMessage_NoErrorCore(rcnHasHSB: true, rcnHasMAB: true, dcnHasHSB: true, dcnHasMAB: true, hsbIsSame: true, hasDecons: true);

		public void TestGetConsNotificationValidationMessage_RCNHasId_DCNHasID_DCNHasOVP() => TestGetConsNotificationValidationMessage_NoErrorCore(dcnHasOVP: true);

		public void TestGetConsNotificationValidationMessage_RCNHasHSB_RCNHasMAB_DCNHasHSB_HSBDifferent_DCNHasOVP() => TestGetConsNotificationValidationMessage_NoErrorCore(rcnHasHSB: true, rcnHasMAB: true, dcnHasHSB: true, hasDecons: true, dcnHasOVP: true);

		void TestGetConsNotificationValidationMessage_NoErrorCore(bool rcnHasHSB = false, bool rcnHasMAB = false, bool dcnHasHSB = false, bool dcnHasMAB = false, bool hsbIsSame = false, bool needAWBCons = false, bool hasDecons = false, bool dcnHasOVP = false, bool hasCor = false)
		{
			var data = SetupDataForConsNotificationValidation();
			var p1 = Helper.CreatePackageState(data.rcn, 1, PackType.Freight.PKG, "P1", TransitWarehouseStatuses.Codes.Departed, weight: 500, receiveUnit: data.rtu, dispatchConsignment: data.dcn, dispatchLoadList: data.dll, dispatchUnit: data.dtu);
			var p2 = Helper.CreatePackageState(data.rcn, 1, PackType.Freight.PKG, "P2", TransitWarehouseStatuses.Codes.Departed, weight: 500, receiveUnit: data.rtu, dispatchConsignment: data.dcn, dispatchLoadList: data.dll, dispatchUnit: data.dtu);
			if (dcnHasOVP)
			{
				var ovpPackage = Helper.CreateOverpackPackage("OVP", data.rcn, receiveUnit: null, rcn: data.rcn, dcn: data.dcn, dtu: data.dtu, status: TransitWarehouseStatuses.Codes.Departed, dll: data.dll);
				Helper.PackPackageIntoHandlingUnit(ovpPackage, p1, ZDateTimeOffset.Now, "LDN", ovpPackage);
				Helper.PackPackageIntoHandlingUnit(ovpPackage, p2, ZDateTimeOffset.Now, "LDN", ovpPackage);
			}
			var rcnRefType = string.Empty;
			var inRefCode = string.Empty;
			var rcnAWB = "-";
			var rcnHWB = "-";
			if (rcnHasHSB)
			{
				data.rcn.WRC_HouseBillNumber = "HSB1";
				rcnRefType = CIN750RefTypes.Codes.HouseAirWaybill;
				rcnHWB = "HSB1";
			}
			else if (rcnHasMAB)
			{
				Helper.CreateAdditionalReference(data.rcn, "MAB-1", AdditionalReferenceTypes.Codes.MasterBill);
				rcnRefType = CIN750RefTypes.Codes.MasterAirWaybill;
				rcnAWB = "MAB-1";
			}
			else
			{
				rcnRefType = CIN750RefTypes.Codes.Reference;
			}
			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, $"|HBL={rcnHWB}|JOB=RC0000001|MBL={rcnAWB}|MST=CIN750InNotification|OTY=2|PTP={rcnRefType}|RFN=EDIDATRC0000001|WGT=1000");

			var dcnAWB = "-";
			var dcnHWB = "-";
			if (dcnHasHSB)
			{
				if (hsbIsSame)
				{
					data.dcn.WDC_HouseBillNumber = "HSB1";
					dcnHWB = "HSB1";
				}
				else
				{
					data.dcn.WDC_HouseBillNumber = "HSB2";
					dcnHWB = "HSB2";
				}
			}
			if (dcnHasMAB)
			{
				Helper.CreateAdditionalReference(data.dcn, "MAB-2", AdditionalReferenceTypes.Codes.MasterBill);
				dcnAWB = "MAB-2";
			}

			if (hasDecons)
			{
				Helper.CreateStmALog(data.dcn, EventCodes.MessageSent, $"|HBL={rcnHWB}|JOB=RC0000001|MBL={rcnAWB}|MST=CIN750DeconsNotification_From|OTY=2|PTP={rcnRefType}|RFN=EDIDATRC0000001|WGT=1000");
				Helper.CreateStmALog(data.dcn, EventCodes.MessageSent, $"|HBL={dcnHWB}|JOB=RC0000001|MBL={dcnAWB}|MST=CIN750DeconsNotification_To|OTY=2|PTP=HWB|RFN=EDIDATDC0000001|WGT=1000");
			}

			if (needAWBCons)
			{
				Helper.CreateStmALog(data.dcn, EventCodes.MessageSent, $"|HBL={rcnHWB}|JOB=RC0000001|MBL={rcnAWB}|MST=CIN750ConsNotification_From|OTY=2|PTP={rcnRefType}|RFN=EDIDATRC0000001|WGT=1000");
				Helper.CreateStmALog(data.dcn, EventCodes.MessageSent, $"|HBL={dcnHWB}|JOB=DC0000001|MBL={dcnAWB}|MST=CIN750ConsNotification_To|OTY=2|PTP=HWB|RFN=EDIDATDC0000001|WGT=1000");
			}

			if (hasCor)
			{
				Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, $"|HBL={rcnHWB}|JOB=RC0000001|MBL={rcnAWB}|MST=CIN750CorNotification|OTY=0|PTP={rcnRefType}|RFN=EDIDATRC0000001|WGT=+1");
				p2.Package.KP_Weight += 1;
			}
			Factory.Save();

			var historyManager = new DispatchConsignmentCIN750NotificationHistoryManager(data.dcn);
			var result = historyManager.GetNextMessageTypeCore(CIN750NotificationMessageTypes.CIN750ConsNotification);

			var notification = new CIN750ConsNotificationBuilder(data.dcn, historyManager.AdditionalDataForCons).Build();
			var validationResult = CIN750NotificationValidation.GetConsNotificationValidationMessage(notification);
			AssertEquals("no validation error", string.Empty, validationResult);
		}

		public void TestGetConsNotificationValidationMessage_DCNHasAWB_NotConsHWB()
		{
			var data = SetupDataForConsNotificationValidation();
			data.dcn.WDC_HouseBillNumber = "HSB1";
			Helper.CreatePackageState(data.rcn, 1, PackType.Freight.PKG, "P1", TransitWarehouseStatuses.Codes.Departed, weight: 500, receiveUnit: data.rtu, dispatchConsignment: data.dcn, dispatchLoadList: data.dll, dispatchUnit: data.dtu);
			Helper.CreatePackageState(data.rcn, 1, PackType.Freight.PKG, "P2", TransitWarehouseStatuses.Codes.Departed, weight: 500, receiveUnit: data.rtu, dispatchConsignment: data.dcn, dispatchLoadList: data.dll, dispatchUnit: data.dtu);
			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=2|PTP=REF|RFN=EDIDATRC0000001|WGT=1000");
			Factory.Save();

			TestGetConsNotificationValidationMessage(data.dcn, "Unable to send CIN 750 Consolidation Notification for Master Bill as House Bill has not been consolidated.", toRefType: CIN750RefTypes.Codes.MasterAirWaybill);
		}

		public void TestGetConsNotificationValidationMessage_FromGoodsQuantityLessThanToGoodsQuantity()
		{
			var data = SetupDataForConsNotificationValidation();
			data.dcn.WDC_HouseBillNumber = "HSB1";
			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=1|PTP=REF|RFN=EDIDATRC0000001|WGT=2");
			Helper.CreatePackageState(data.rcn, 1, PackType.Freight.PKG, "P1", TransitWarehouseStatuses.Codes.Departed, weight: 2, receiveUnit: data.rtu, dispatchConsignment: data.dcn, dispatchLoadList: data.dll, dispatchUnit: data.dtu);
			Factory.Save();

			TestGetConsNotificationValidationMessage(data.dcn, "Cannot Send CIN 750 Consolidation Notification because From Goods quantity cannot less than To Goods quantity.", toQuantity: 2);
		}

		public void TestGetConsNotificationValidationMessage_FromGoodsWeightLessThanToGoodsWeight()
		{
			var data = SetupDataForConsNotificationValidation();
			data.dcn.WDC_HouseBillNumber = "HSB1";
			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=1|PTP=REF|RFN=EDIDATRC0000001|WGT=2");
			Helper.CreatePackageState(data.rcn, 1, PackType.Freight.PKG, "P1", TransitWarehouseStatuses.Codes.Departed, weight: 2, receiveUnit: data.rtu, dispatchConsignment: data.dcn, dispatchLoadList: data.dll, dispatchUnit: data.dtu);
			Factory.Save();

			TestGetConsNotificationValidationMessage(data.dcn, "Cannot Send CIN 750 Consolidation Notification because From Goods weight cannot less than To Goods weight.", toWeight: 10);
		}

		public void TestGetConsNotificationValidationMessage_GoodsQTYOrWeightWrong_MAB()
		{
			var data = SetupDataForConsNotificationValidation();
			data.rcn.WRC_HouseBillNumber = "HSB1";
			data.dcn.WDC_HouseBillNumber = "HSB1";
			Helper.CreateAdditionalReference(data.dcn, "MAB-1", AdditionalReferenceTypes.Codes.MasterBill);
			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=HSB1|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=1|PTP=HWB|RFN=EDIDATRC0000001|WGT=2");
			Helper.CreatePackageState(data.rcn, 1, PackType.Freight.PKG, "P1", TransitWarehouseStatuses.Codes.Departed, weight: 2, receiveUnit: data.rtu, dispatchConsignment: data.dcn, dispatchLoadList: data.dll, dispatchUnit: data.dtu);
			Factory.Save();

			TestGetConsNotificationValidationMessage(data.dcn, "Master Bill quantity or weight cannot be greater than DCN quantity or weight.", fromQuantity: 10);
			TestGetConsNotificationValidationMessage(data.dcn, "Master Bill quantity or weight cannot be greater than DCN quantity or weight.", fromWeight: 10);
		}

		public void TestGetConsNotificationValidationMessage_GoodsQTYOrWeightWrong_HWB()
		{
			var data = SetupDataForConsNotificationValidation();
			data.dcn.WDC_HouseBillNumber = "HSB1";
			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=1|PTP=REF|RFN=EDIDATRC0000001|WGT=2");
			Helper.CreatePackageState(data.rcn, 1, PackType.Freight.PKG, "P1", TransitWarehouseStatuses.Codes.Departed, weight: 2, receiveUnit: data.rtu, dispatchConsignment: data.dcn, dispatchLoadList: data.dll, dispatchUnit: data.dtu);
			Factory.Save();

			TestGetConsNotificationValidationMessage(data.dcn, "House Bill or Reference quantity or weight cannot be greater than DCN quantity or weight.", fromQuantity: 10);
			TestGetConsNotificationValidationMessage(data.dcn, "House Bill or Reference quantity or weight cannot be greater than DCN quantity or weight.", fromWeight: 10);
		}

		public void TestGetConsNotificationValidationMessage_GoodsQTYOrWeightOverHWB_AWB()
		{
			var data = SetupDataForConsNotificationValidation();
			data.dcn.WDC_HouseBillNumber = "HSB1";
			Helper.CreateAdditionalReference(data.dcn, "MAB-1", AdditionalReferenceTypes.Codes.MasterBill);
			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=HSB1|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=1|PTP=HWB|RFN=EDIDATRC0000001|WGT=2");
			Helper.CreateStmALog(data.dcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750ConsNotification_From|OTY=1|PTP=REF|RFN=EDIDATRRC0000001|WGT=2");
			Helper.CreateStmALog(data.dcn, EventCodes.MessageSent, "|HBL=HSB1|JOB=DC0000001|MBL=MAB-1|MST=CIN750ConsNotification_To|OTY=1|PTP=REF|RFN=EDIDATRDC0000001|WGT=2");
			Helper.CreatePackageState(data.rcn, 1, PackType.Freight.PKG, "P1", TransitWarehouseStatuses.Codes.Departed, weight: 2, receiveUnit: data.rtu, dispatchConsignment: data.dcn, dispatchLoadList: data.dll, dispatchUnit: data.dtu);
			Factory.Save();

			TestGetConsNotificationValidationMessage(data.dcn, "Master Bill quantity or weight cannot be greater than DCN quantity or weight.", fromQuantity: 10);
			TestGetConsNotificationValidationMessage(data.dcn, "Master Bill quantity or weight cannot be greater than DCN quantity or weight.", fromWeight: 10);
		}

		public void TestGetConsNotificationValidationMessage_GoodsQTYOrWeightOverDecons()
		{
			var data = SetupDataForConsNotificationValidation();
			data.rcn.WRC_HouseBillNumber = "HSB1";
			Helper.CreateAdditionalReference(data.rcn, "MAB-1", AdditionalReferenceTypes.Codes.MasterBill);
			data.dcn.WDC_HouseBillNumber = "HWB2";
			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=HSB1|JOB=RC0000001|MBL=MAB-1|MST=CIN750InNotification|OTY=1|PTP=HWB|RFN=EDIDATRC0000001|WGT=2");
			Helper.CreateStmALog(data.dcn, EventCodes.MessageSent, "|HBL=HWB2|JOB=RC0000001|MBL=-|MST=CIN750DeconsNotification_From|OTY=1|PTP=HWB|RFN=EDIDATRC0000001|WGT=2");
			Helper.CreateStmALog(data.dcn, EventCodes.MessageSent, "|HBL=HWB2|JOB=RC0000001|MBL=-|MST=CIN750DeconsNotification_To|OTY=1|PTP=HWB|RFN=EDIDATDC0000001|WGT=2");
			Helper.CreatePackageState(data.rcn, 1, PackType.Freight.PKG, "P1", TransitWarehouseStatuses.Codes.Departed, weight: 2, receiveUnit: data.rtu, dispatchConsignment: data.dcn, dispatchLoadList: data.dll, dispatchUnit: data.dtu);
			Factory.Save();

			TestGetConsNotificationValidationMessage(data.dcn, "Cannot Send CIN 750 Consolidation Notification as From Goods quantity or weight cannot be greater than Deconsolidation.", fromQuantity: 10);
			TestGetConsNotificationValidationMessage(data.dcn, "Cannot Send CIN 750 Consolidation Notification as From Goods quantity or weight cannot be greater than Deconsolidation.", fromWeight: 10);
		}

		public void TestGetConsNotificationValidationMessage_WrongRefType_HWB()
		{
			var data = SetupDataForConsNotificationValidation();
			data.dcn.WDC_HouseBillNumber = "HSB1";
			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=1|PTP=REF|RFN=EDIDATRC0000001|WGT=2");
			Helper.CreatePackageState(data.rcn, 1, PackType.Freight.PKG, "P1", TransitWarehouseStatuses.Codes.Departed, weight: 2, receiveUnit: data.rtu, dispatchConsignment: data.dcn, dispatchLoadList: data.dll, dispatchUnit: data.dtu);
			Factory.Save();

			TestGetConsNotificationValidationMessage(data.dcn, "Unable to Send CIN 750 Consolidation Notification as reported Ref Type is not HWB.", fromRefType: CIN750RefTypes.Codes.HouseAirWaybill);
		}

		public void TestGetConsNotificationValidationMessage_WrongRefType_AWB_HWBNotCompleted()
		{
			var data = SetupDataForConsNotificationValidation();
			data.dcn.WDC_HouseBillNumber = "HSB1";
			Helper.CreateAdditionalReference(data.dcn, "MAB-1", AdditionalReferenceTypes.Codes.MasterBill);
			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=1|PTP=REF|RFN=EDIDATRC0000001|WGT=2");
			Helper.CreatePackageState(data.rcn, 1, PackType.Freight.PKG, "P1", TransitWarehouseStatuses.Codes.Departed, weight: 2, receiveUnit: data.rtu, dispatchConsignment: data.dcn, dispatchLoadList: data.dll, dispatchUnit: data.dtu);
			Factory.Save();

			TestGetConsNotificationValidationMessage(data.dcn, "Unable to send CIN 750 Consolidation Notification for Master Bill as House Bill has not been consolidated.", toRefType: CIN750RefTypes.Codes.MasterAirWaybill);
		}

		public void TestGetConsNotificationValidationMessage_WrongRefType_AWB_HWBCompleted()
		{
			var data = SetupDataForConsNotificationValidation();
			data.dcn.WDC_HouseBillNumber = "HSB1";
			Helper.CreateAdditionalReference(data.dcn, "MAB-1", AdditionalReferenceTypes.Codes.MasterBill);
			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=1|PTP=REF|RFN=EDIDATRC0000001|WGT=2");
			Helper.CreateStmALog(data.dcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750ConsNotification_From|OTY=1|PTP=REF|RFN=EDIDATRC0000001|WGT=2");
			Helper.CreateStmALog(data.dcn, EventCodes.MessageSent, "|HBL=HSB1|JOB=DC0000001|MBL=MAB-1|MST=CIN750ConsNotification_To|OTY=1|PTP=HWB|RFN=EDIDATDC0000001|WGT=2");
			Helper.CreatePackageState(data.rcn, 1, PackType.Freight.PKG, "P1", TransitWarehouseStatuses.Codes.Departed, weight: 2, receiveUnit: data.rtu, dispatchConsignment: data.dcn, dispatchLoadList: data.dll, dispatchUnit: data.dtu);
			Factory.Save();

			TestGetConsNotificationValidationMessage(data.dcn, "CIN 750 Consolidation Notification Ref Type should be Master Bill as House Bill has been consolidated.", fromRefType: CIN750RefTypes.Codes.Reference);
		}

		public void TestGetConsNotificationValidationMessage_DCNAndRCNHasSameHSB_DCNHasAWB()
		{
			var data = SetupDataForConsNotificationValidation();
			Helper.CreateAdditionalReference(data.dcn, "MAB-1", AdditionalReferenceTypes.Codes.MasterBill);
			data.dcn.WDC_HouseBillNumber = "HSB1";
			data.rcn.WRC_HouseBillNumber = "HSB1";
			Helper.CreateStmALog(data.rcn, EventCodes.MessageSent, "|HBL=HSB1|JOB=RC0000001|MBL=MAB-1|MST=CIN750InNotification|OTY=1|PTP=HWB|RFN=EDIDATRC0000001|WGT=2");
			Helper.CreatePackageState(data.rcn, 1, PackType.Freight.PKG, "P1", TransitWarehouseStatuses.Codes.Departed, weight: 2, receiveUnit: data.rtu, dispatchConsignment: data.dcn, dispatchLoadList: data.dll, dispatchUnit: data.dtu);
			Factory.Save();

			TestGetConsNotificationValidationMessage(data.dcn, "Unable to send CIN 750 Consolidation Notification as reported Ref Type should be Master Bill.", fromRefType: CIN750RefTypes.Codes.Reference);
		}

		(WhsItemReceiveConsignment rcn, WhsItemReceiveTransportationUnit rtu, WhsItemDispatchConsignment dcn, WhsItemDispatchTransportationUnit dtu, WhsItemDispatchLoadList dll) SetupDataForConsNotificationValidation()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch("WH1");
			warehouse.WarehouseAddress.Address1 = "WH1Address";
			var location = Helper.CreateLocation(warehouse);
			Helper.AddOrgCode(warehouse.WarehouseAddress, OrgCusCode.FranceCodeTypes.CIN, "C001");

			var rcn = Helper.CreateReceiveConsignment("RC0000001", warehouse.PK);

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK, location);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);

			var dcn = Helper.CreateDispatchConsignment("DC0000001", warehouse.PK);

			return (rcn, rtu, dcn, dtu, dll);
		}

		void TestGetConsNotificationValidationMessage(WhsItemDispatchConsignment dcn, string expectedMessage, int fromQuantity = 0, int toQuantity = 0, decimal fromWeight = 0, decimal toWeight = 0, string fromRefType = "", string toRefType = "", string reportRefType = "")
		{
			var historyManager = new DispatchConsignmentCIN750NotificationHistoryManager(dcn);
			var result = historyManager.GetNextMessageTypeCore(CIN750NotificationMessageTypes.CIN750ConsNotification);
			var notification = new CIN750ConsNotificationBuilder(dcn, historyManager.AdditionalDataForCons).Build();

			if (fromQuantity != 0 || toQuantity != 0 || fromWeight > 0 || toWeight > 0 || !string.IsNullOrEmpty(fromRefType) || !string.IsNullOrEmpty(toRefType) || !string.IsNullOrEmpty(reportRefType))
			{
				var fromGoods = notification.FromGoods.FirstOrDefault();
				if (fromQuantity != 0)
				{
					fromGoods.AmountQuantity = fromQuantity;
				}
				if (fromWeight != 0)
				{
					fromGoods.AmountWeight = fromWeight;
				}
				if (!string.IsNullOrEmpty(fromRefType))
				{
					fromGoods.RefType.Code = fromRefType;
				}
				if (!string.IsNullOrEmpty(toRefType))
				{
					notification.RefType.Code = toRefType;
					notification.ToGoods.RefType.Code = toRefType;
				}
				if (!string.IsNullOrEmpty(reportRefType))
				{
					notification.RefType.Code = reportRefType;
				}
				if (toQuantity != 0)
				{
					notification.ToGoods.AmountQuantity = toQuantity;
				}
				if (toWeight != 0)
				{
					notification.ToGoods.AmountWeight = toWeight;
				}
			}

			var validationResult = CIN750NotificationValidation.GetConsNotificationValidationMessage(notification);
			AssertEquals("Error Message", expectedMessage, validationResult);
		}

		#endregion

		#region Implement

		protected WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper helper;

		#endregion
	}
}
