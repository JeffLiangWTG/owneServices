using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.Warehouse.Transit.Business.Testing;

namespace Enterprise.Warehouse.Transit.Document.Testing
{
	sealed class CIN750InNotificationDocPackingLineBuilderTest : TestCaseWithFactory
	{
		public void TestBuild_PackageNotArrived() => TestBuild_InvalidPackage(TransitWarehouseStatuses.Codes.Booked);

		public void TestBuild_PackageAdjustedOut() => TestBuild_InvalidPackage(TransitWarehouseStatuses.Codes.AdjustedOut);

		void TestBuild_InvalidPackage(string status)
		{
			var rcn = CreateGeneralRCN();
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", rcn.Warehouse.PK, rcn.Warehouse.DefaultInboundDockDoorLocation.PK);
			var notification = new CIN750InNotificationBuilder(rcn).Build();

			Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "CEN1");

			if (status == TransitWarehouseStatuses.Codes.Booked)
			{
				Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P", status, weight: 2, weightUQ: "KG");
			}
			else
			{
				Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P", status, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			}

			Factory.Save();

			var packline = new CIN750InNotificationPackingLineBuilder(notification).Build();
			AssertNotNull(packline);

			AssertEquals("Count", 0, packline.AmountQuantity);
		}

		public void TestBuild_RCNWithMultipleCENNumbers()
		{
			var rcn = CreateGeneralRCN();
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", rcn.Warehouse.PK, rcn.Warehouse.DefaultInboundDockDoorLocation.PK);

			Helper.CreateAdditionalReference(rcn, "MAB-1", AdditionalReferenceTypes.Codes.MasterBill);

			var cen1 = Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "CEN1");
			cen1.PopulateAddOnValue("SourceType", "STR", "T1");
			var cen2 = Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "CEN2");
			cen2.PopulateAddOnValue("SourceType", "STR", "T2L");
			var cen3 = Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "CEN3");
			cen3.PopulateAddOnValue("SourceType", "STR", "T2");
			var cen4 = Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "CEN4");
			Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration, "TST1");

			var packageState1 = Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P1", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P2", TransitWarehouseStatuses.Codes.Arrived, weight: 800, weightUQ: "G", receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P3", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P4", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P5", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			packageState1.Package.KP_GoodsDescription = "Test Description";

			Factory.Save();

			var notification = new CIN750InNotificationBuilder(rcn).Build();
			var packline = new CIN750InNotificationPackingLineBuilder(notification).Build();
			AssertNotNull(packline);

			AssertEquals("Amount", 5, packline.AmountQuantity);
			AssertEquals("Weight", 8.8m, packline.AmountWeight);
			AssertEquals("Description", "Test Description", packline.Description);
			AssertEquals("Accompany Document Ref", "CEN1, CEN3, CEN2", packline.AccompanyDocumentRef);
			AssertEquals("Accompany Document Ref", "T1, T2, T2L", packline.AccompanyDocumentType);
			AssertEquals("Temporary Storage Declaration", "TST1", packline.TemporaryStorageDeclaration);
		}

		public void TestBuild_RCNWithoutCENAndTMPSNumber()
		{
			var rcn = CreateGeneralRCN();
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", rcn.Warehouse.PK, rcn.Warehouse.DefaultInboundDockDoorLocation.PK);

			Helper.CreateAdditionalReference(rcn, "MAB-1", AdditionalReferenceTypes.Codes.MasterBill);

			var packageState1 = Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P1", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P2", TransitWarehouseStatuses.Codes.Arrived, weight: 800, weightUQ: "G", receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P3", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P4", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P5", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			packageState1.Package.KP_GoodsDescription = "Test Description";

			Factory.Save();

			var notification = new CIN750InNotificationBuilder(rcn).Build();
			var packline = new CIN750InNotificationPackingLineBuilder(notification).Build();
			AssertNotNull(packline);

			AssertEquals("Amount", 5, packline.AmountQuantity);
			AssertEquals("Weight", 8.8m, packline.AmountWeight);
			AssertEquals("Description", "Test Description", packline.Description);
			AssertEquals("Accompany Document Ref", "", packline.AccompanyDocumentRef);
			AssertEquals("Accompany Document Type", "", packline.AccompanyDocumentType);
			AssertEquals("Temporary Storage Declaration", "", packline.TemporaryStorageDeclaration);
		}

		public void TestBuild_PackagePartialArrived()
		{
			var rcn = CreateGeneralRCN();
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", rcn.Warehouse.PK, rcn.Warehouse.DefaultInboundDockDoorLocation.PK);

			Helper.CreateAdditionalReference(rcn, "MAB-1", AdditionalReferenceTypes.Codes.MasterBill);

			var cen = Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "CEN1");
			cen.PopulateAddOnValue("SourceType", "STR", "T1");
			Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration, "TST1");

			var packageState1 = Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P1", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			var packageState2 = Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P2", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P3", TransitWarehouseStatuses.Codes.Booked, weight: 2, weightUQ: "KG");
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P4", TransitWarehouseStatuses.Codes.Booked, weight: 2, weightUQ: "KG");
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P5", TransitWarehouseStatuses.Codes.Booked, weight: 2, weightUQ: "KG");
			packageState2.Package.KP_GoodsDescription = "Test Description";

			Factory.Save();

			var notification = new CIN750InNotificationBuilder(rcn).Build();
			var packline = new CIN750InNotificationPackingLineBuilder(notification).Build();
			AssertNotNull(packline);

			AssertEquals("Amount", 2, packline.AmountQuantity);
			AssertEquals("Weight", 4m, packline.AmountWeight);
			AssertEquals("Description", "Test Description", packline.Description);
			AssertEquals("Accompany Document Ref", "CEN1", packline.AccompanyDocumentRef);
			AssertEquals("Accompany Document Ref", "T1", packline.AccompanyDocumentType);
			AssertEquals("Temporary Storage Declaration", "TST1", packline.TemporaryStorageDeclaration);
		}

		public void TestBuild_PackagePackedIntoHU()
		{
			var rcn = CreateGeneralRCN();
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", rcn.Warehouse.PK, rcn.Warehouse.DefaultInboundDockDoorLocation.PK);

			Helper.CreateAdditionalReference(rcn, "MAB-1", AdditionalReferenceTypes.Codes.MasterBill);

			var cen = Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "CEN1");
			cen.PopulateAddOnValue("SourceType", "STR", "T1");
			Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration, "TST1");

			var packageState1 = Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P1", TransitWarehouseStatuses.Codes.Putaway, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			var packageState2 = Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P2", TransitWarehouseStatuses.Codes.Putaway, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			var packageState3 = Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P3", TransitWarehouseStatuses.Codes.Putaway, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P4", TransitWarehouseStatuses.Codes.Putaway, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P5", TransitWarehouseStatuses.Codes.Putaway, weight: 2, weightUQ: "KG", receiveUnit: rtu);

			var handlingUnit = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage = Helper.CreateHandlingUnitPackage("HU1", handlingUnit, receiveUnit: rtu, entryNum: "JOB1");

			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, packageState1, ZDateTimeOffset.Now, "ABC", handlingUnitPackage);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, packageState2, ZDateTimeOffset.Now, "ABC", handlingUnitPackage);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, packageState3, ZDateTimeOffset.Now, "ABC", handlingUnitPackage);

			Factory.Save();

			var notification = new CIN750InNotificationBuilder(rcn).Build();
			var packline = new CIN750InNotificationPackingLineBuilder(notification).Build();
			AssertNotNull(packline);

			AssertEquals("Amount", 5, packline.AmountQuantity);
			AssertEquals("Weight", 10m, packline.AmountWeight);
			AssertEquals("Accompany Document Ref", "CEN1", packline.AccompanyDocumentRef);
			AssertEquals("Accompany Document Ref", "T1", packline.AccompanyDocumentType);
			AssertEquals("Temporary Storage Declaration", "TST1", packline.TemporaryStorageDeclaration);
		}

		public void TestBuild_AdjustInPackage()
		{
			var rcn = CreateGeneralRCN();
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", rcn.Warehouse.PK, rcn.Warehouse.DefaultInboundDockDoorLocation.PK);

			Helper.CreateAdditionalReference(rcn, "MAB-1", AdditionalReferenceTypes.Codes.MasterBill);

			var cen = Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "CEN1");
			cen.PopulateAddOnValue("SourceType", "STR", "T1");

			Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration, "TST1");

			Helper.CreateStmALog(rcn, "MSN", "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=3|PTP=REF|RFN=EDIDATRC0000001|WGT=6");
			Helper.CreateStmALog(rcn, "MSN", "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750CorNotification|OTY=-2|PTP=REF|RFN=EDIDATRC0000001|WGT=-4");
			var packageState1 = Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P1", TransitWarehouseStatuses.Codes.Putaway, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			var packageState2 = Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P2", TransitWarehouseStatuses.Codes.Putaway, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			var packageState3 = Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P3", TransitWarehouseStatuses.Codes.AdjustedOut, weight: 2, weightUQ: "KG", receiveUnit: rtu, adjustedOut: "OTH");

			Factory.Save();

			var notification = new CIN750InNotificationBuilder(rcn).Build();
			var packline = new CIN750InNotificationPackingLineBuilder(notification).Build();
			AssertNotNull(packline);

			AssertEquals("Amount", 1, packline.AmountQuantity);
			AssertEquals("Weight", 2m, packline.AmountWeight);
			AssertEquals("Accompany Document Ref", "CEN1", packline.AccompanyDocumentRef);
			AssertEquals("Accompany Document Ref", "T1", packline.AccompanyDocumentType);
			AssertEquals("Temporary Storage Declaration", "TST1", packline.TemporaryStorageDeclaration);
		}

		public void TestBuild_HistoryLessThanReceivedPackages()
		{
			var rcn = CreateGeneralRCN();
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", rcn.Warehouse.PK, rcn.Warehouse.DefaultInboundDockDoorLocation.PK);

			Helper.CreateAdditionalReference(rcn, "MAB-1", AdditionalReferenceTypes.Codes.MasterBill);

			var cen = Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "CEN1");
			cen.PopulateAddOnValue("SourceType", "STR", "T1");
			Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration, "TST1");

			Helper.CreateStmALog(rcn, "MSN", "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=2|PTP=REF|RFN=EDIDATRC0000001|WGT=4");
			var packageState1 = Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P1", TransitWarehouseStatuses.Codes.Putaway, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			var packageState2 = Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P2", TransitWarehouseStatuses.Codes.Putaway, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			var packageState3 = Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P3", TransitWarehouseStatuses.Codes.Putaway, weight: 2, weightUQ: "KG", receiveUnit: rtu);

			Factory.Save();

			var notification = new CIN750InNotificationBuilder(rcn).Build();
			var packline = new CIN750InNotificationPackingLineBuilder(notification).Build();
			AssertNotNull(packline);

			AssertEquals("Amount", 1, packline.AmountQuantity);
			AssertEquals("Weight", 2m, packline.AmountWeight);
			AssertEquals("Accompany Document Ref", "CEN1", packline.AccompanyDocumentRef);
			AssertEquals("Accompany Document Ref", "T1", packline.AccompanyDocumentType);
			AssertEquals("Temporary Storage Declaration", "TST1", packline.TemporaryStorageDeclaration);
		}

		public void TestBuild_PackagePackedIntoNestedHU()
		{
			var rcn = CreateGeneralRCN();
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", rcn.Warehouse.PK, rcn.Warehouse.DefaultInboundDockDoorLocation.PK);

			Helper.CreateAdditionalReference(rcn, "MAB-1", AdditionalReferenceTypes.Codes.MasterBill);

			var cen = Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "CEN1");
			cen.PopulateAddOnValue("SourceType", "STR", "T1");
			Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration, "TST1");

			var packageState1 = Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P1", TransitWarehouseStatuses.Codes.Putaway, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			var packageState2 = Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P2", TransitWarehouseStatuses.Codes.Putaway, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			var packageState3 = Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P3", TransitWarehouseStatuses.Codes.Putaway, weight: 2, weightUQ: "KG", receiveUnit: rtu);

			var ovp2 = helper.CreateOverpackPackage("OVP2", rcn, rtu, TransitWarehouseStatuses.Codes.Putaway, rcn: rcn);
			ovp2.Package.KP_Weight = 8m;
			ovp2.Package.KP_WeightUQ = "KG";

			var ovp1 = helper.CreateOverpackPackage("OVP1", rcn, rtu, TransitWarehouseStatuses.Codes.Putaway, rcn: rcn);
			ovp1.Package.KP_Weight = 10m;
			ovp1.Package.KP_WeightUQ = "KG";

			var handlingUnit = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage = Helper.CreateHandlingUnitPackage("HU1", handlingUnit, receiveUnit: rtu, entryNum: "JOB1");
			handlingUnitPackage.Package.KP_Weight = 12m;
			handlingUnitPackage.Package.KP_WeightUQ = "KG";

			Helper.PackPackageIntoHandlingUnit(ovp2, packageState1, ZDateTimeOffset.Now, "ABC", handlingUnitPackage);
			Helper.PackPackageIntoHandlingUnit(ovp2, packageState2, ZDateTimeOffset.Now, "ABC", handlingUnitPackage);
			Helper.PackPackageIntoHandlingUnit(ovp2, packageState3, ZDateTimeOffset.Now, "ABC", handlingUnitPackage);
			Helper.PackPackageIntoHandlingUnit(ovp1, ovp2, ZDateTimeOffset.Now, "ABC", handlingUnitPackage);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, ovp1, ZDateTimeOffset.Now, "ABC", handlingUnitPackage);

			Factory.Save();

			var notification = new CIN750InNotificationBuilder(rcn).Build();
			var packline = new CIN750InNotificationPackingLineBuilder(notification).Build();
			AssertNotNull(packline);

			AssertEquals("Amount", 1, packline.AmountQuantity);
			AssertEquals("Weight", 10m, packline.AmountWeight);
			AssertEquals("Accompany Document Ref", "CEN1", packline.AccompanyDocumentRef);
			AssertEquals("Accompany Document Ref", "T1", packline.AccompanyDocumentType);
			AssertEquals("Temporary Storage Declaration", "TST1", packline.TemporaryStorageDeclaration);
		}

		public void TestBuild_RCNHasShipmentDescription()
		{
			var rcn = CreateGeneralRCN();
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", rcn.Warehouse.PK, rcn.Warehouse.DefaultInboundDockDoorLocation.PK);

			Helper.CreateAdditionalReference(rcn, "MAB-1", AdditionalReferenceTypes.Codes.MasterBill);
			Helper.CreateAdditionalReference(rcn, "shipment description", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentDescription);

			var cen1 = Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "CEN1");
			var cen2 = Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "CEN2");
			var cen3 = Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "CEN3");
			Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration, "TST1");

			Factory.Save();

			var packageState1 = Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P1", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P2", TransitWarehouseStatuses.Codes.Arrived, weight: 800, weightUQ: "G", receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P3", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P4", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P5", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			packageState1.Package.KP_GoodsDescription = "Test Description";

			Factory.Save();

			var notification = new CIN750InNotificationBuilder(rcn).Build();
			var packline = new CIN750InNotificationPackingLineBuilder(notification).Build();
			AssertNotNull(packline);
			AssertEquals("goods description should be shipment description", "shipment description", packline.Description);
		}

		public void TestBuild_PackageBreakDown()
		{
			var rcn = CreateGeneralRCN();
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", rcn.Warehouse.PK, rcn.Warehouse.DefaultInboundDockDoorLocation.PK);

			Helper.CreateAdditionalReference(rcn, "MAB-1", AdditionalReferenceTypes.Codes.MasterBill);
			Helper.CreateAdditionalReference(rcn, "shipment description", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentDescription);
			var cen = Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber, "CEN1");
			cen.PopulateAddOnValue("SourceType", "STR", "T1");
			Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration, "TST1");

			var ovpPackageState = Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P-OVP", TransitWarehouseStatuses.Codes.AdjustedOut, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			var innerPackageState1 = Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P1", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			var innerPackageState2 = Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P2", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			var innerPackageState3 = Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P3", TransitWarehouseStatuses.Codes.Arrived, weight: 2, weightUQ: "KG", receiveUnit: rtu);
			Helper.PackPackageIntoHandlingUnit(ovpPackageState, innerPackageState1, ZDateTimeOffset.Now, "TST", ZDateTimeOffset.Now);
			Helper.PackPackageIntoHandlingUnit(ovpPackageState, innerPackageState2, ZDateTimeOffset.Now, "TST", ZDateTimeOffset.Now);
			Helper.PackPackageIntoHandlingUnit(ovpPackageState, innerPackageState3, ZDateTimeOffset.Now, "TST", ZDateTimeOffset.Now);

			Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P4", TransitWarehouseStatuses.Codes.Arrived, weight: 3, weightUQ: "KG", receiveUnit: rtu);

			Factory.Save();

			var notification = new CIN750InNotificationBuilder(rcn).Build();
			var packline = new CIN750InNotificationPackingLineBuilder(notification).Build();
			AssertNotNull(packline);

			AssertEquals("Amount", 2, packline.AmountQuantity);
			AssertEquals("Weight", 5m, packline.AmountWeight);
			AssertEquals("Accompany Document Ref", "CEN1", packline.AccompanyDocumentRef);
			AssertEquals("Accompany Document Ref", "T1", packline.AccompanyDocumentType);
			AssertEquals("Temporary Storage Declaration", "TST1", packline.TemporaryStorageDeclaration);
		}

		WhsItemReceiveConsignment CreateGeneralRCN()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch("WH1");
			warehouse.WarehouseAddress.Address1 = "WH1Address";
			Helper.AddOrgCode(warehouse.WarehouseAddress, OrgCusCode.FranceCodeTypes.CIN, "C001");

			var rcn = Helper.CreateReceiveConsignment("RC0000001", warehouse.PK);

			var ctoAddress = Helper.CreateClient().MainAddress;
			ctoAddress.Address1 = "CTOAddress";
			var ctoJobDocAddress = Helper.CreateJobDocAddressFromAddress(rcn, DocAddressTypes.Codes.ArrivalCTOAddress, ctoAddress);
			Helper.AddOrgCode(ctoJobDocAddress.Address, OrgCusCode.FranceCodeTypes.CIN, "C002");

			return rcn;
		}

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper helper;
	}
}
