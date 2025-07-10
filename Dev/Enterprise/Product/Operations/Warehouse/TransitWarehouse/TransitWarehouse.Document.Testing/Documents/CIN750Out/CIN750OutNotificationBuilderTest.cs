using System.Linq;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.Warehouse.Transit.Document.Common;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;

namespace Enterprise.Warehouse.Transit.Document.Testing
{
	public sealed class CIN750OutNotificationBuilderTest : CIN750NotificationBuilderTest<CIN750OutNotificationBuilder, WhsItemDispatchConsignment, CIN750OutNotification>
	{
		public void TestBuild()
		{
			var warehouse = CreateWhsWarehouse("WH1", "WH1Address", "C001");

			var rcn = Helper.CreateReceiveConsignment("RC0000001", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.WW_DefaultInboundDockDoor);
			Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration, "TSD1");
			var crn = Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "CRN1");
			crn.PopulateAddOnValue("SourceType", "STR", "T1");

			var dcn = CreateGeneralDCN(warehouse);
			Helper.CreateAdditionalReference(dcn, "MAB-1", AdditionalReferenceTypes.Codes.MasterBill);
			var dll = Helper.CreateDispatchLoadList("DLL001", warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU001", warehouse.PK);
			dcn.WDC_HouseBillNumber = "HB1";
			var packageState = Helper.CreatePackageState(rcn, 1, PackageStateUnitType.Codes.Package, "P1", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, weight: 1, weightUQ: Core.Constants.Weight.Kilograms, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			Helper.CreatePackageState(rcn, 1, PackageStateUnitType.Codes.Package, "P2", TransitWarehouseStatuses.Codes.Departed, rtu, weight: 1, weightUQ: Core.Constants.Weight.Kilograms, dispatchConsignment: dcn, dispatchLoadList: dll);
			packageState.Package.KP_GoodsDescription = "ppp";

			var additionalData = new OutNotificationAdditionalData()
			{
				PackageQuantityReadyToOut = 1,
				PackageWeightReadyToOut = 1
			};
			var outNotification = new CIN750OutNotificationBuilder(dcn, additionalData).Build();
			AssertNotNull(outNotification);

			AssertNotEquals("MovementTime should not be Empty", ZDateTime.Empty, outNotification.MovementTime);
			AssertEquals("DeclaredInWarehouse should be WH1Address", "WH1Address", outNotification.DeclaredInWarehouse.AddressLine1);
			AssertEquals("DeclaredInWarehouseCIN should be C001", "C001", outNotification.DeclaredInWarehouseCIN);
			AssertEquals("ToCTO should be CTOAddress", "CTOAddress", outNotification.ToCTO.AddressLine1);
			AssertEquals("ToCTOCIN should be C002", "C002", outNotification.ToCTOCIN);
			AssertEquals("RefType should be " + CIN750RefTypes.Codes.MasterAirWaybill, CIN750RefTypes.Codes.MasterAirWaybill, outNotification.RefType.Code);
			AssertEquals("RefCode should be MAB-1", "MAB1", outNotification.RefCode);

			var customsDocuments = outNotification.CustomsDocuments;
			AssertEquals("CustomsDocutmentType should have only 1 line", 1, customsDocuments.Count);
			var fristCustomsDocument = customsDocuments.First();
			AssertEquals("customsDocutment.Type should be T1", "T1", fristCustomsDocument.RefType);
			AssertEquals("customsDocutment.Ref should be CRN1", "CRN1", fristCustomsDocument.RefCode);

			var goods = outNotification.Goods;
			AssertEquals("Goods should have only 1 line", 1, goods.Count);
			var line = goods.First();
			AssertNotNull(line);
			AssertEquals("line.AmountQuantity should be 1", 1, line.AmountQuantity);
			AssertEquals("line.AmountWeight should be 1", 1m, line.AmountWeight);
			AssertEquals("line.Description should be ppp", "ppp", line.Description);
			AssertNullOrEmpty("line.TemporaryStorageDeclaration should be blank", line.TemporaryStorageDeclaration);
		}

		public void TestBuild_HouseBill()
		{
			var warehouse = CreateWhsWarehouse("WH1", "WH1Address", "C001");

			var rcn = Helper.CreateReceiveConsignment("RC0000001", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.WW_DefaultInboundDockDoor);
			Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration, "TSD1");
			Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "CRN1");

			var dcn = CreateGeneralDCN(warehouse);
			var dll = Helper.CreateDispatchLoadList("DLL001", warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU001", warehouse.PK);
			dcn.WDC_HouseBillNumber = "HB1";
			Helper.CreatePackageState(rcn, 1, PackageStateUnitType.Codes.Package, "P1", TransitWarehouseStatuses.Codes.Departed, rtu, weight: 1, weightUQ: Core.Constants.Weight.Kilograms, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);

			var additionalData = new OutNotificationAdditionalData();
			var outNotification = new CIN750OutNotificationBuilder(dcn, additionalData).Build();
			AssertNotNull(outNotification);

			AssertEquals("RefType should be " + CIN750RefTypes.Codes.HouseAirWaybill, CIN750RefTypes.Codes.HouseAirWaybill, outNotification.RefType.Code);
			AssertEquals("RefCode should be HB1", "HB1", outNotification.RefCode);
			AssertEquals("line.TemporaryStorageDeclaration should be TSD1", "TSD1", outNotification.Goods.First().TemporaryStorageDeclaration);
		}

		public void TestBuild_NoAirWayBill()
		{
			var warehouse = CreateWhsWarehouse("WH1", "WH1Address", "C001");

			var rcn = Helper.CreateReceiveConsignment("RC0000001", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.WW_DefaultInboundDockDoor);
			Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration, "TSD1");
			Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "CRN1");

			var dcn = CreateGeneralDCN(warehouse);
			var dll = Helper.CreateDispatchLoadList("DLL001", warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU001", warehouse.PK);
			Helper.CreatePackageState(rcn, 1, PackageStateUnitType.Codes.Package, "P1", TransitWarehouseStatuses.Codes.Departed, rtu, weight: 1, weightUQ: Core.Constants.Weight.Kilograms, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);

			var additionalData = new OutNotificationAdditionalData();
			var outNotification = new CIN750OutNotificationBuilder(dcn, additionalData).Build();
			AssertNotNull(outNotification);

			AssertEquals("RefType should be " + CIN750RefTypes.Codes.Reference, CIN750RefTypes.Codes.Reference, outNotification.RefType.Code);
			AssertEquals("RefCode should be EDIDATRC0000001", "EDIDATRC0000001", outNotification.RefCode);
			AssertEquals("line.TemporaryStorageDeclaration should be TSD1", "TSD1", outNotification.Goods.First().TemporaryStorageDeclaration);
		}

		#region TestBuild_CustomsStatus

		public void TestBuild_CustomesStatus_Export()
		{
			var warehouse = CreateWhsWarehouse("WH1", "WH1Address", "C001");
			var dcn = CreateGeneralDCN(warehouse);
			dcn.WDC_Direction = TransitWarehouseConsignmentDirections.Codes.Export;

			var additionalData = new OutNotificationAdditionalData();
			var outNotification = new CIN750OutNotificationBuilder(dcn, additionalData).Build();
			AssertNotNull(outNotification);
			AssertEquals("CustomesStatus.Code should not be " + CIN750CustomsStatus.Codes.Export, CIN750CustomsStatus.Codes.Export, outNotification.CustomsStatus.Code);
			AssertEquals("CustomesStatus.Description should not be " + CIN750CustomsStatus.Descriptions.Export, CIN750CustomsStatus.Descriptions.Export, outNotification.CustomsStatus.Description);
		}

		public void TestBuild_CustomesStatus_Import()
		{
			var warehouse = CreateWhsWarehouse("WH1", "WH1Address", "C001");
			var dcn = CreateGeneralDCN(warehouse);
			dcn.WDC_Direction = TransitWarehouseConsignmentDirections.Codes.Import;

			var additionalData = new OutNotificationAdditionalData();
			var outNotification = new CIN750OutNotificationBuilder(dcn, additionalData).Build();
			AssertNotNull(outNotification);
			AssertEquals("CustomesStatus should not be " + CIN750CustomsStatus.Codes.Import, CIN750CustomsStatus.Codes.Import, outNotification.CustomsStatus.Code);
			AssertEquals("CustomesStatus.Description should not be " + CIN750CustomsStatus.Descriptions.Import, CIN750CustomsStatus.Descriptions.Import, outNotification.CustomsStatus.Description);
		}

		public void TestBuild_CustomesStatus_Domestic()
		{
			var warehouse = CreateWhsWarehouse("WH1", "WH1Address", "C001");
			var dcn = CreateGeneralDCN(warehouse);
			dcn.WDC_Direction = TransitWarehouseConsignmentDirections.Codes.Domestic;

			var additionalData = new OutNotificationAdditionalData();
			var outNotification = new CIN750OutNotificationBuilder(dcn, additionalData).Build();
			AssertNotNull(outNotification);
			AssertEquals("CustomesStatus.Code should not be " + CIN750CustomsStatus.Codes.Domestic, CIN750CustomsStatus.Codes.Domestic, outNotification.CustomsStatus.Code);
			AssertEquals("CustomesStatus.Description should not be " + CIN750CustomsStatus.Descriptions.Domestic, CIN750CustomsStatus.Descriptions.Domestic, outNotification.CustomsStatus.Description);
		}

		public void TestBuild_CustomesStatus_None()
		{
			var warehouse = CreateWhsWarehouse("WH1", "WH1Address", "C001");
			var dcn = CreateGeneralDCN(warehouse);
			var additionalData = new OutNotificationAdditionalData();
			var outNotification = new CIN750OutNotificationBuilder(dcn, additionalData).Build();
			AssertNotNull(outNotification);
			AssertNullOrEmpty("CustomesStatus.Code should not be empty", outNotification.CustomsStatus.Code);
		}

		#endregion

		public void TestValidations()
		{
			var outNotification = GetGeneralNotification();

			AssertNoErrors(outNotification.ToCTOCINInfo);
			AssertNotNull(outNotification.CustomsDocuments);
			AssertEquals(1, outNotification.CustomsDocuments.Count);

			var customsDocuments = outNotification.CustomsDocuments.First();
			AssertNullOrEmptyOrWhitespace(customsDocuments.RefType);
			AssertNullOrEmptyOrWhitespace(customsDocuments.RefCode);
			AssertHasMessageError(customsDocuments.RefTypeInfo, "Customs Documents is required.");
			AssertHasMessageError(customsDocuments.RefCodeInfo, "Customs Documents is required.");
		}

		public void TestBuild_DCNHasShipmentDescription()
		{
			var warehouse = CreateWhsWarehouse("WH1", "WH1Address", "C001");

			var rcn = Helper.CreateReceiveConsignment("RC0000001", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.WW_DefaultInboundDockDoor);
			Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration, "TSD1");
			Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "CRN1");

			var dcn = CreateGeneralDCN(warehouse);
			Helper.CreateAdditionalReference(dcn, "MAB-1", AdditionalReferenceTypes.Codes.MasterBill);
			Helper.CreateAdditionalReference(dcn, "shipment description", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentDescription);
			var dll = Helper.CreateDispatchLoadList("DLL001", warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU001", warehouse.PK);
			dcn.WDC_HouseBillNumber = "HB1";
			var packageState = Helper.CreatePackageState(rcn, 1, PackageStateUnitType.Codes.Package, "P1", TransitWarehouseStatuses.Codes.Departed, rtu, weight: 1, weightUQ: Core.Constants.Weight.Kilograms, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			Helper.CreatePackageState(rcn, 1, PackageStateUnitType.Codes.Package, "P2", TransitWarehouseStatuses.Codes.Departed, rtu, weight: 1, weightUQ: Core.Constants.Weight.Kilograms, dispatchConsignment: dcn, dispatchLoadList: dll);
			packageState.Package.KP_GoodsDescription = "ppp";

			var additionalData = new OutNotificationAdditionalData();
			var outNotification = new CIN750OutNotificationBuilder(dcn, additionalData).Build();
			AssertNotNull(outNotification);
			AssertEquals("goods description should be shipment description", "shipment description", outNotification.Goods.First().Description);
		}

		protected override void TestRemoveHypenCore()
		{
			var dcn = CreateGeneralDCN();
			Factory.Save();

			dcn.WDC_HouseBillNumber = "HB-1";

			var outNotification = new CIN750OutNotificationBuilder(dcn, null).Build();
			AssertNotNull(outNotification);

			AssertEquals("RefType", "HWB", outNotification.RefType.Code);
			AssertEquals("RefCode", "HB1", outNotification.RefCode);
		}

		public void TestBuild_AccompanyingDocumentType_MultipleRCNWithMultipleReferences()
		{
			var warehouse = CreateWhsWarehouse("WH1", "WH1Address", "C001");

			var rcn = Helper.CreateReceiveConsignment("RC0000001", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.WW_DefaultInboundDockDoor);
			Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration, "TSD1");
			var crn1 = Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "CRN1");
			crn1.PopulateAddOnValue("SourceType", "STR", "T1");
			var crn2 = Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "CRN2");
			crn2.PopulateAddOnValue("SourceType", "STR", "T2");

			var rcn2 = Helper.CreateReceiveConsignment("RC0000002", warehouse.PK);
			var rtu2 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.WW_DefaultInboundDockDoor);
			Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration, "TSD2");
			var crn3 = Helper.CreateCustomsAdditionalReference(rcn2, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "CRN3");
			crn3.PopulateAddOnValue("SourceType", "STR", "T1");
			var crn4 = Helper.CreateCustomsAdditionalReference(rcn2, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "CRN4");
			crn4.PopulateAddOnValue("SourceType", "STR", "T2L");
			var crn5 = Helper.CreateCustomsAdditionalReference(rcn2, TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber, "CRN5");

			var dcn = CreateGeneralDCN(warehouse);
			Helper.CreateAdditionalReference(dcn, "MAB-1", AdditionalReferenceTypes.Codes.MasterBill);
			var dll = Helper.CreateDispatchLoadList("DLL001", warehouse.PK);
			dcn.WDC_HouseBillNumber = "HB1";
			var dtu = Helper.CreateDispatchTransportationUnit("DTU001", warehouse.PK);

			Helper.CreatePackageState(rcn, 1, PackageStateUnitType.Codes.Package, "P1", TransitWarehouseStatuses.Codes.Departed, rtu, weight: 1, weightUQ: Core.Constants.Weight.Kilograms, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			Helper.CreatePackageState(rcn2, 1, PackageStateUnitType.Codes.Package, "P2", TransitWarehouseStatuses.Codes.Departed, rtu, weight: 1, weightUQ: Core.Constants.Weight.Kilograms, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);

			var additionalData = new OutNotificationAdditionalData()
			{
				PackageQuantityReadyToOut = 2,
				PackageWeightReadyToOut = 2
			};
			var outNotification = new CIN750OutNotificationBuilder(dcn, additionalData).Build();
			AssertNotNull(outNotification);

			var customsDocuments = outNotification.CustomsDocuments;
			AssertEquals("CustomsDocutmentType should have 2 lines", 2, customsDocuments.Count);
			var fristCustomsDocument = customsDocuments.First();
			AssertEquals("customsDocutment.Type should be T1, T2", "T1, T2", fristCustomsDocument.RefType);
			AssertEquals("customsDocutment.Ref should be CRN1, CRN2", "CRN1, CRN2", fristCustomsDocument.RefCode);

			var secondCustomsDocument = customsDocuments.Last();
			AssertEquals("customsDocutment.Type should be T1, T2L", "T1, T2L", secondCustomsDocument.RefType);
			AssertEquals("customsDocutment.Ref should be CRN3, CRN4", "CRN3, CRN4", secondCustomsDocument.RefCode);
		}

		WhsItemDispatchConsignment CreateGeneralDCN(WhsWarehouse warehouse)
		{
			var ctoAddress = Helper.CreateClient().MainAddress;
			ctoAddress.Address1 = "CTOAddress";
			var dcn = Helper.CreateDispatchConsignment("DC0000001", warehouse.PK);
			var ctoJobDocAddress = Helper.CreateJobDocAddressFromAddress(dcn, DocAddressTypes.Codes.DepartureCTOAddress, ctoAddress);
			Helper.AddOrgCode(ctoJobDocAddress.Address, OrgCusCode.FranceCodeTypes.CIN, "C002");

			return dcn;
		}

		WhsWarehouse CreateWhsWarehouse(string warehouseName, string address, string cinCode)
		{
			var warehouse = Helper.CreateWarehouse(warehouseName);
			warehouse.WarehouseAddress.Address1 = address;
			Helper.AddOrgCode(warehouse.WarehouseAddress, OrgCusCode.FranceCodeTypes.CIN, cinCode);
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);

			var inLocation = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			warehouse.WW_DefaultInboundDockDoor = inLocation.PK;

			var outLocation = row.Locations.First(l => l.ToLocationString() == "Dock-1-2");
			warehouse.WW_DefaultOutboundDockDoor = outLocation.PK;
			return warehouse;
		}

		protected override CIN750OutNotification GetGeneralNotification()
		{
			var dcn = CreateGeneralDCN(Helper.CreateWarehouse("WH1"));
			var additionalData = new OutNotificationAdditionalData();
			return new CIN750OutNotificationBuilder(dcn, additionalData).Build();
		}
	}
}
