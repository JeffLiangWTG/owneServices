using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(CBP7512DocumentLineCollection))]
	sealed class CBP7512DocumentLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CBP7512DocumentLineCollection>
	{
		public void TestFTZBOLShownInDescriptionAndQtyOfMerchandise()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_ImportTransportMode = TransportModeCodes.Codes.AirNonContainer;
			header.BH_FTZMove = true;
			var bill = header.Bills.AddNew();
			bill.B0_IssuerCode = "ISSU";
			bill.B0_MasterBillNumber = "MB1";
			bill.B0_HouseBillNumber = "HB1";
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var warehouseDetail = moveDetail.WarehouseDetails.AddNew();
			warehouseDetail.US_WarehouseNumber = "MB112";
			warehouseDetail.US_WarehouseBondedQuantity = 1000.00m;
			warehouseDetail.US_WarehouseWithdrawQuantity = 100.00m;
			var entryDetail = "ENTRY: MB112\r\nBONDED:1000;WITHDRAW:100;BAL:900\r\n" + CBP7512DocumentLineCollection.DottedLine;
			var helper = new WhsDataTestHelper(Factory);
			var warehouse = Factory.New<OrgHeader>();
			warehouse.OH_Code = "W#@33";
			var address2 = warehouse.Addresses.AddNew();
			var whsWarehouse = helper.GetNewWhsWarehouse(address2.PK, true, "W#@", Warehouse.Integration.CodeLists.WarehouseTypes.Codes.FreeTradeZone);
			moveHeader.BM_OA_WarehouseAddress = warehouse.MainAddress.PK;
			var collection = new CBP7512DocumentLineCollection(moveHeader);
			AssertEquals(false, collection.OfType<CBP7512DocumentLine>().Any(x => x.DescriptionAndQtyOfMerchandise == entryDetail));
		}

		public void TestBOLShownInDescriptionAndQtyOfMerchandise()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_ImportTransportMode = TransportModeCodes.Codes.AirNonContainer;
			header.BH_FTZMove = true;
			var bill = header.Bills.AddNew();
			bill.B0_IssuerCode = "ISSU";
			bill.B0_MasterBillNumber = "MB1";
			bill.B0_HouseBillNumber = "HB1";
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var warehouseDetail = moveDetail.WarehouseDetails.AddNew();
			warehouseDetail.US_WarehouseNumber = "MB112";
			warehouseDetail.US_WarehouseBondedQuantity = 1000.00m;
			warehouseDetail.US_WarehouseWithdrawQuantity = 100.00m;
			var airBillDetail = "BOL: ISSU MB1 (HB1)";
			var seaBillDetail = "BOL: ISSU MB1";
			var entryDetail = "ENTRY: MB112\r\nBONDED:1000;WITHDRAW:100;BAL:900\r\n" + CBP7512DocumentLineCollection.DottedLine;
			var lineItem = moveDetail.CBP7512Lines.AddNew();
			var collection = new CBP7512DocumentLineCollection(moveHeader);
			Assert(collection.OfType<CBP7512DocumentLine>().Any(x => x.DescriptionAndQtyOfMerchandise == airBillDetail));
			AssertEquals(false, collection.OfType<CBP7512DocumentLine>().Any(x => x.DescriptionAndQtyOfMerchandise == seaBillDetail));
			AssertEquals(false, collection.OfType<CBP7512DocumentLine>().Any(x => x.DescriptionAndQtyOfMerchandise == entryDetail));
			header.BH_ImportTransportMode = TransportModeCodes.Codes.VesselContainer;
			collection = new CBP7512DocumentLineCollection(moveHeader);
			AssertEquals(false, collection.OfType<CBP7512DocumentLine>().Any(x => x.DescriptionAndQtyOfMerchandise == airBillDetail));
			AssertEquals(false, collection.OfType<CBP7512DocumentLine>().Any(x => x.DescriptionAndQtyOfMerchandise == seaBillDetail));
			Assert(collection.OfType<CBP7512DocumentLine>().Any(x => x.DescriptionAndQtyOfMerchandise == entryDetail));
			var helper = new WhsDataTestHelper(Factory);
			var whsWarehouse = helper.GetNewWhsWarehouse(helper.Warehouse.MainAddress.PK, true, "HO2", Warehouse.Integration.CodeLists.WarehouseTypes.Codes.FreeTradeZone);
			moveHeader.BM_OA_WarehouseAddress = helper.Warehouse.MainAddress.PK;
			collection = new CBP7512DocumentLineCollection(moveHeader);
			AssertEquals(false, collection.OfType<CBP7512DocumentLine>().Any(x => x.DescriptionAndQtyOfMerchandise == airBillDetail));
			AssertEquals(false, collection.OfType<CBP7512DocumentLine>().Any(x => x.DescriptionAndQtyOfMerchandise == seaBillDetail));
			AssertEquals(false, collection.OfType<CBP7512DocumentLine>().Any(x => x.DescriptionAndQtyOfMerchandise == entryDetail));
		}

		public void TestPopulateLines()
		{
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "1!10";
			subs.DG_Variant = "c";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			subs.DG_Class = "2.1";
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_AdditionalText = "OPEN TEXT";
			var bill1 = header.Bills.AddNew();
			bill1.B0_IssuerCode = "ABJD";
			bill1.B0_MasterBillNumber = "MB1";
			var bill2 = header.Bills.AddNew();
			bill2.B0_IssuerCode = "ABJD";
			bill2.B0_MasterBillNumber = "MB2";
			var billAddRef1 = bill2.AdditionalReferences.AddNew();
			billAddRef1.BR_Qualifier = ReferenceQualifierList.Codes.CR;
			billAddRef1.BR_ReferenceNum = "REF1";
			var billAddRef2 = bill2.AdditionalReferences.AddNew();
			billAddRef2.BR_Qualifier = ReferenceQualifierList.Codes.GR;
			billAddRef2.BR_ReferenceNum = "REF2";
			var bill3 = header.Bills.AddNew();
			bill3.B0_IssuerCode = "ABJD";
			bill3.B0_MasterBillNumber = "MB3";
			var billAddRef3 = bill3.AdditionalReferences.AddNew();
			billAddRef3.BR_Qualifier = ReferenceQualifierList.Codes.OM;
			billAddRef3.BR_ReferenceNum = "REF3";
			var moveDetail1 = moveHeader.MovementDetails.AddNew();
			moveDetail1.B9_B0 = bill2.PK;
			moveDetail1.B9_SeqNo = "2";
			var moveDetail2 = moveHeader.MovementDetails.AddNew();
			moveDetail2.B9_B0 = bill1.PK;
			moveDetail2.B9_SeqNo = "2";
			var moveDetail3 = moveHeader.MovementDetails.AddNew();
			moveDetail3.B9_B0 = bill3.PK;
			moveDetail3.B9_SeqNo = "1";
			var lineItem1 = moveDetail1.CBP7512Lines.AddNew();
			lineItem1.BI_Description = "moveDetail1 line 1";
			var lineItem2 = moveDetail1.CBP7512Lines.AddNew();
			lineItem2.BI_Description = "moveDetail1 line 2";
			var container1 = moveDetail1.Containers.AddNew();
			container1.BC_ContainerNum = "CONT1";
			container1.BC_Seal1 = "SL1";
			var container2 = moveDetail1.Containers.AddNew();
			container2.BC_ContainerNum = "CONT2";
			container2.BC_Seal1 = "SL1";
			var lineItem3 = moveDetail2.CBP7512Lines.AddNew();
			lineItem3.BI_Description = "moveDetail2 line 1";
			var lineItem4 = moveDetail2.CBP7512Lines.AddNew();
			lineItem4.BI_Description = "moveDetail2 line 2";
			var container3 = moveDetail2.Containers.AddNew();
			container3.BC_ContainerNum = "CONT2";
			container3.BC_Seal1 = "SL1";
			var container4 = moveDetail2.Containers.AddNew();
			container4.BC_ContainerNum = "CONT3";
			container4.BC_Seal1 = "SL1";
			var undg1 = container4.UNDGs.AddNew();
			undg1.LinkDefault(subs);
			var lineItem5 = moveDetail3.CBP7512Lines.AddNew();
			lineItem5.BI_Description = "moveDetail3 line 1";
			var lineItem6 = moveDetail3.CBP7512Lines.AddNew();
			lineItem6.BI_Description = "moveDetail3 line 2";
			var container5 = moveDetail3.Containers.AddNew();
			container5.BC_ContainerNum = "CONT2";
			container5.BC_Seal1 = "SL2";
			var container6 = moveDetail3.Containers.AddNew();
			container6.BC_ContainerNum = "CONT1";
			container6.BC_Seal1 = "SL2";
			var collection = new CBP7512DocumentLineCollection(moveHeader);
			AssertEquals(12, collection.Count);
			AssertDocumentLine(collection[0], "moveDetail3 line 1");
			AssertDocumentLine(collection[1], "moveDetail3 line 2");
			AssertDocumentLine(collection[2], "BOL: ABJD MB3");
			AssertDocumentLine(collection[3], string.Format("ADDITIONAL REFERENCE(S):{0}OM - {1}: REF3", System.Environment.NewLine, ReferenceQualifierList.Descriptions.OM));
			AssertDocumentLine(collection[4], "moveDetail2 line 1");
			AssertDocumentLine(collection[5], "moveDetail2 line 2");
			AssertDocumentLine(collection[6], "BOL: ABJD MB1");
			AssertDocumentLine(collection[7], "moveDetail1 line 1");
			AssertDocumentLine(collection[8], "moveDetail1 line 2");
			AssertDocumentLine(collection[9], "BOL: ABJD MB2");
			AssertDocumentLine(collection[10], string.Format("ADDITIONAL REFERENCE(S):{0}CR - {1}: REF1{0}GR - {2}: REF2", System.Environment.NewLine, ReferenceQualifierList.Descriptions.CR, ReferenceQualifierList.Descriptions.GR));
			AssertDocumentLine(collection[11], @"
CTNR # CONT2   SEAL # SL2
CTNR # CONT1   SEAL # SL2
CTNR # CONT2   SEAL # SL1
CTNR # CONT3   SEAL # SL1
CTNR # CONT1   SEAL # SL1
UNUN1!10, class 2.1 I

OPEN TEXT");
		}

		public void TestPopulateLinesAirNonContainerFullDataMode()
		{
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "1!10";
			subs.DG_Variant = "c";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			subs.DG_Class = "2.1";
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			var bill1 = header.Bills.AddNew();
			bill1.B0_IssuerCode = "ABJD";
			bill1.B0_MasterBillNumber = "MB1";
			var bill2 = header.Bills.AddNew();
			bill2.B0_IssuerCode = "ABJD";
			bill2.B0_MasterBillNumber = "MB2";
			var billAddRef1 = bill2.AdditionalReferences.AddNew();
			billAddRef1.BR_Qualifier = ReferenceQualifierList.Codes.CR;
			billAddRef1.BR_ReferenceNum = "REF1";
			var billAddRef2 = bill2.AdditionalReferences.AddNew();
			billAddRef2.BR_Qualifier = ReferenceQualifierList.Codes.GR;
			billAddRef2.BR_ReferenceNum = "REF2";
			var bill3 = header.Bills.AddNew();
			bill3.B0_IssuerCode = "ABJD";
			bill3.B0_MasterBillNumber = "MB3";
			var billAddRef3 = bill3.AdditionalReferences.AddNew();
			billAddRef3.BR_Qualifier = ReferenceQualifierList.Codes.OM;
			billAddRef3.BR_ReferenceNum = "REF3";
			var moveDetail1 = moveHeader.MovementDetails.AddNew();
			moveDetail1.B9_B0 = bill2.PK;
			moveDetail1.B9_SeqNo = "2";
			var moveDetail2 = moveHeader.MovementDetails.AddNew();
			moveDetail2.B9_B0 = bill1.PK;
			moveDetail2.B9_SeqNo = "2";
			var moveDetail3 = moveHeader.MovementDetails.AddNew();
			moveDetail3.B9_B0 = bill3.PK;
			moveDetail3.B9_SeqNo = "1";
			var lineItem1 = moveDetail1.CBP7512Lines.AddNew();
			lineItem1.BI_Description = "moveDetail1 line 1";
			var lineItem2 = moveDetail1.CBP7512Lines.AddNew();
			lineItem2.BI_Description = "moveDetail1 line 2";
			var container1 = moveDetail1.Containers.AddNew();
			container1.BC_ContainerNum = "CONT1";
			container1.BC_Seal1 = "SL1";
			var container2 = moveDetail1.Containers.AddNew();
			container2.BC_ContainerNum = "CONT2";
			container2.BC_Seal1 = "SL1";
			var lineItem3 = moveDetail2.CBP7512Lines.AddNew();
			lineItem3.BI_Description = "moveDetail2 line 1";
			var lineItem4 = moveDetail2.CBP7512Lines.AddNew();
			lineItem4.BI_Description = "moveDetail2 line 2";
			var container3 = moveDetail2.Containers.AddNew();
			container3.BC_ContainerNum = "CONT2";
			container3.BC_Seal1 = "SL1";
			var container4 = moveDetail2.Containers.AddNew();
			container4.BC_ContainerNum = "CONT3";
			container4.BC_Seal1 = "SL1";
			var undg1 = container4.UNDGs.AddNew();
			undg1.LinkDefault(subs);
			var lineItem5 = moveDetail3.CBP7512Lines.AddNew();
			lineItem5.BI_Description = "moveDetail3 line 1";
			var lineItem6 = moveDetail3.CBP7512Lines.AddNew();
			lineItem6.BI_Description = "moveDetail3 line 2";
			var container5 = moveDetail3.Containers.AddNew();
			container5.BC_ContainerNum = "CONT2";
			container5.BC_Seal1 = "SL2";
			var container6 = moveDetail3.Containers.AddNew();
			container6.BC_ContainerNum = "CONT1";
			container6.BC_Seal1 = "SL2";
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			header.BH_ImportTransportMode = TransportModeCodes.Codes.AirNonContainer;
			AssertEquals(0, moveDetail1.Containers.Count);
			AssertEquals(0, moveDetail2.Containers.Count);
			AssertEquals(0, moveDetail3.Containers.Count);
			var collection = new CBP7512DocumentLineCollection(moveHeader);
			AssertEquals(11, collection.Count);
			AssertDocumentLine(collection[0], "moveDetail3 line 1");
			AssertDocumentLine(collection[1], "moveDetail3 line 2");
			AssertDocumentLine(collection[2], "BOL: ABJD MB3 ()");
			AssertDocumentLine(collection[3], string.Format("ADDITIONAL REFERENCE(S):{0}OM - {1}: REF3", System.Environment.NewLine, ReferenceQualifierList.Descriptions.OM));
			AssertDocumentLine(collection[4], "moveDetail2 line 1");
			AssertDocumentLine(collection[5], "moveDetail2 line 2");
			AssertDocumentLine(collection[6], "BOL: ABJD MB1 ()");
			AssertDocumentLine(collection[7], "moveDetail1 line 1");
			AssertDocumentLine(collection[8], "moveDetail1 line 2");
			AssertDocumentLine(collection[9], "BOL: ABJD MB2 ()");
			AssertDocumentLine(collection[10], string.Format("ADDITIONAL REFERENCE(S):{0}CR - {1}: REF1{0}GR - {2}: REF2", System.Environment.NewLine, ReferenceQualifierList.Descriptions.CR, ReferenceQualifierList.Descriptions.GR));
		}

		public void TestPopulateLinesOnShipmentPluggedInInBond()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_PackingMode = "LSE";
			shipment.JS_HouseBill = "HB0394842JFK";
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "1!10";
			subs.DG_Variant = "c";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			subs.DG_Class = "2.1";
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = shipment.PK;
			header.BH_ParentTableCode = shipment.TablePrefix;
			header.BH_ImportTransportMode = TransportModeCodes.Codes.AirNonContainer;
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_AdditionalText = "OPEN TEXT";
			var bill1 = header.Bills.AddNew();
			bill1.B0_IssuerCode = "ABJD";
			bill1.B0_MasterBillNumber = "MB1";
			var bill2 = header.Bills.AddNew();
			bill2.B0_IssuerCode = "ABJD";
			bill2.B0_MasterBillNumber = "MB2";
			var billAddRef1 = bill2.AdditionalReferences.AddNew();
			billAddRef1.BR_Qualifier = ReferenceQualifierList.Codes.CR;
			billAddRef1.BR_ReferenceNum = "REF1";
			var billAddRef2 = bill2.AdditionalReferences.AddNew();
			billAddRef2.BR_Qualifier = ReferenceQualifierList.Codes.GR;
			billAddRef2.BR_ReferenceNum = "REF2";
			var bill3 = header.Bills.AddNew();
			bill3.B0_IssuerCode = "ABJD";
			bill3.B0_MasterBillNumber = "MB3";
			var billAddRef3 = bill3.AdditionalReferences.AddNew();
			billAddRef3.BR_Qualifier = ReferenceQualifierList.Codes.OM;
			billAddRef3.BR_ReferenceNum = "REF3";
			var moveDetail1 = moveHeader.MovementDetails.AddNew();
			moveDetail1.B9_B0 = bill2.PK;
			moveDetail1.B9_SeqNo = "2";
			var moveDetail2 = moveHeader.MovementDetails.AddNew();
			moveDetail2.B9_B0 = bill1.PK;
			moveDetail2.B9_SeqNo = "2";
			var moveDetail3 = moveHeader.MovementDetails.AddNew();
			moveDetail3.B9_B0 = bill3.PK;
			moveDetail3.B9_SeqNo = "1";
			var lineItem1 = moveDetail1.CBP7512Lines.AddNew();
			lineItem1.BI_Description = "moveDetail1 line 1";
			var lineItem2 = moveDetail1.CBP7512Lines.AddNew();
			lineItem2.BI_Description = "moveDetail1 line 2";
			var container1 = moveDetail1.Containers.AddNew();
			container1.BC_ContainerNum = "CONT1";
			container1.BC_Seal1 = "SL1";
			var container2 = moveDetail1.Containers.AddNew();
			container2.BC_ContainerNum = "CONT2";
			container2.BC_Seal1 = "SL1";
			var lineItem3 = moveDetail2.CBP7512Lines.AddNew();
			lineItem3.BI_Description = "moveDetail2 line 1";
			var lineItem4 = moveDetail2.CBP7512Lines.AddNew();
			lineItem4.BI_Description = "moveDetail2 line 2";
			var container3 = moveDetail2.Containers.AddNew();
			container3.BC_ContainerNum = "CONT2";
			container3.BC_Seal1 = "SL1";
			var container4 = moveDetail2.Containers.AddNew();
			container4.BC_ContainerNum = "CONT3";
			container4.BC_Seal1 = "SL1";
			var undg1 = container4.UNDGs.AddNew();
			undg1.LinkDefault(subs);
			var lineItem5 = moveDetail3.CBP7512Lines.AddNew();
			lineItem5.BI_Description = "moveDetail3 line 1";
			var lineItem6 = moveDetail3.CBP7512Lines.AddNew();
			lineItem6.BI_Description = "moveDetail3 line 2";
			var container5 = moveDetail3.Containers.AddNew();
			container5.BC_ContainerNum = "CONT2";
			container5.BC_Seal1 = "SL2";
			var container6 = moveDetail3.Containers.AddNew();
			container6.BC_ContainerNum = "CONT1";
			container6.BC_Seal1 = "SL2";
			var collection = new CBP7512DocumentLineCollection(moveHeader);
			AssertEquals(12, collection.Count);
			AssertDocumentLine(collection[0], "moveDetail3 line 1");
			AssertDocumentLine(collection[1], "moveDetail3 line 2");
			AssertDocumentLine(collection[2], "BOL: ABJD MB3 ()");
			AssertDocumentLine(collection[3], string.Format("ADDITIONAL REFERENCE(S):{0}OM - {1}: REF3", System.Environment.NewLine, ReferenceQualifierList.Descriptions.OM));
			AssertDocumentLine(collection[4], "moveDetail2 line 1");
			AssertDocumentLine(collection[5], "moveDetail2 line 2");
			AssertDocumentLine(collection[6], "BOL: ABJD MB1 ()");
			AssertDocumentLine(collection[7], "moveDetail1 line 1");
			AssertDocumentLine(collection[8], "moveDetail1 line 2");
			AssertDocumentLine(collection[9], "BOL: ABJD MB2 ()");
			AssertDocumentLine(collection[10], string.Format("ADDITIONAL REFERENCE(S):{0}CR - {1}: REF1{0}GR - {2}: REF2", System.Environment.NewLine, ReferenceQualifierList.Descriptions.CR, ReferenceQualifierList.Descriptions.GR));
			AssertDocumentLine(collection[11], @"
CTNR # CONT2   SEAL # SL2
CTNR # CONT1   SEAL # SL2
CTNR # CONT2   SEAL # SL1
CTNR # CONT3   SEAL # SL1
CTNR # CONT1   SEAL # SL1
UNUN1!10, class 2.1 I

OPEN TEXT

HAWB: HB0394842JFK");
		}

		public void TestPopulateLinesOnSeaShipmentWithHouseBillIssuingParty()
		{
			Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy).CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "XXXX", Core.Constants.CountryCodes.UnitedStates);
			Factory.Save();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "LSE";
			shipment.JS_HouseBill = "HB0394842JFK";
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = shipment.PK;
			header.BH_ParentTableCode = shipment.TablePrefix;
			header.BH_ImportTransportMode = TransportModeCodes.Codes.VesselContainer;
			var cusInBondMoveHeader = header.MovementHeaders.AddNew();
			cusInBondMoveHeader.BM_AdditionalText = "OPEN TEXT";
			var bill = header.Bills.AddNew();
			bill.B0_IssuerCode = "ABJD";
			bill.B0_MasterBillNumber = "MB1";
			var cusInBondMoveDetail = cusInBondMoveHeader.MovementDetails.AddNew();
			cusInBondMoveDetail.B9_B0 = bill.PK;
			cusInBondMoveDetail.B9_SeqNo = "2";
			var collection = new CBP7512DocumentLineCollection(cusInBondMoveHeader);
			AssertEquals(2, collection.Count);
			AssertDocumentLine(collection[0], "BOL: ABJD MB1");
			AssertDocumentLine(collection[1], @"
OPEN TEXT

HBOL: XXXX HB0394842JFK");
			var houseBillIssuingParty = Factory.New<OrgHeader>();
			houseBillIssuingParty.OH_Code = "TEST1";
			houseBillIssuingParty.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "ABCD", Core.Constants.CountryCodes.UnitedStates);
			shipment.HouseBillIssuingPartyDocumentaryAddress.OrganisationPK = houseBillIssuingParty.PK;
			collection = new CBP7512DocumentLineCollection(cusInBondMoveHeader);
			AssertEquals(2, collection.Count);
			AssertDocumentLine(collection[0], "BOL: ABJD MB1");
			AssertDocumentLine(collection[1], @"
OPEN TEXT

HBOL: ABCD HB0394842JFK");
		}

		public void TestPopulateLinesOnSeaShipmentWithHouseBillNumber()
		{
			Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy).CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "XXXX", Core.Constants.CountryCodes.UnitedStates);
			Factory.Save();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_PackingMode = "LSE";
			shipment.JS_HouseBill = "XXXX94842JFK";
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = shipment.PK;
			header.BH_ParentTableCode = shipment.TablePrefix;
			header.BH_ImportTransportMode = TransportModeCodes.Codes.VesselContainer;
			var cusInBondMoveHeader = header.MovementHeaders.AddNew();
			cusInBondMoveHeader.BM_AdditionalText = "OPEN TEXT";
			var bill = header.Bills.AddNew();
			bill.B0_IssuerCode = "ABJD";
			bill.B0_MasterBillNumber = "MB1";
			var cusInBondMoveDetail = cusInBondMoveHeader.MovementDetails.AddNew();
			cusInBondMoveDetail.B9_B0 = bill.PK;
			cusInBondMoveDetail.B9_SeqNo = "2";
			var collection = new CBP7512DocumentLineCollection(cusInBondMoveHeader);
			AssertEquals(2, collection.Count);
			AssertDocumentLine(collection[0], "BOL: ABJD MB1");
			AssertDocumentLine(collection[1], @"
OPEN TEXT

HBOL: XXXX94842JFK");
			var houseBillIssuingParty = Factory.New<OrgHeader>();
			houseBillIssuingParty.OH_Code = "TEST1";
			houseBillIssuingParty.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "ABCD", Core.Constants.CountryCodes.UnitedStates);
			shipment.HouseBillIssuingPartyDocumentaryAddress.OrganisationPK = houseBillIssuingParty.PK;
			collection = new CBP7512DocumentLineCollection(cusInBondMoveHeader);
			AssertEquals(2, collection.Count);
			AssertDocumentLine(collection[0], "BOL: ABJD MB1");
			AssertDocumentLine(collection[1], @"
OPEN TEXT

HBOL: XXXX94842JFK");

			shipment.JS_HouseBill = "XXXY94842JFK";
			collection = new CBP7512DocumentLineCollection(cusInBondMoveHeader);
			AssertEquals(2, collection.Count);
			AssertDocumentLine(collection[0], "BOL: ABJD MB1");
			AssertDocumentLine(collection[1], @"
OPEN TEXT

HBOL: ABCD XXXY94842JFK");

			houseBillIssuingParty.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.TruckCarrierCode, "XXXY", Core.Constants.CountryCodes.UnitedStates);
			collection = new CBP7512DocumentLineCollection(cusInBondMoveHeader);
			AssertEquals(2, collection.Count);
			AssertDocumentLine(collection[0], "BOL: ABJD MB1");
			AssertDocumentLine(collection[1], @"
OPEN TEXT

HBOL: ABCD XXXY94842JFK");
		}

		public void TestPopulateLinesOnRoadShipmentWithHouseBillNumber()
		{
			Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy).CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "XXXX", Core.Constants.CountryCodes.UnitedStates);
			Factory.Save();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Road;
			shipment.JS_PackingMode = "FCL";
			shipment.JS_HouseBill = "XXXX94842JFK";
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = shipment.PK;
			header.BH_ParentTableCode = shipment.TablePrefix;
			header.BH_ImportTransportMode = TransportModeCodes.Codes.TruckContainer;
			var cusInBondMoveHeader = header.MovementHeaders.AddNew();
			cusInBondMoveHeader.BM_AdditionalText = "OPEN TEXT";
			var bill = header.Bills.AddNew();
			bill.B0_IssuerCode = "ABJD";
			bill.B0_MasterBillNumber = "MB1";
			var cusInBondMoveDetail = cusInBondMoveHeader.MovementDetails.AddNew();
			cusInBondMoveDetail.B9_B0 = bill.PK;
			cusInBondMoveDetail.B9_SeqNo = "2";
			var collection = new CBP7512DocumentLineCollection(cusInBondMoveHeader);
			AssertEquals(2, collection.Count);
			AssertDocumentLine(collection[0], "BOL: ABJD MB1");
			AssertDocumentLine(collection[1], @"
OPEN TEXT

HBOL: XXXX94842JFK");
			var houseBillIssuingParty = Factory.New<OrgHeader>();
			houseBillIssuingParty.OH_Code = "TEST1";
			houseBillIssuingParty.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "ABCD", Core.Constants.CountryCodes.UnitedStates);
			shipment.HouseBillIssuingPartyDocumentaryAddress.OrganisationPK = houseBillIssuingParty.PK;
			collection = new CBP7512DocumentLineCollection(cusInBondMoveHeader);
			AssertEquals(2, collection.Count);
			AssertDocumentLine(collection[0], "BOL: ABJD MB1");
			AssertDocumentLine(collection[1], @"
OPEN TEXT

HBOL: XXXX94842JFK");

			shipment.JS_HouseBill = "XXXY94842JFK";
			collection = new CBP7512DocumentLineCollection(cusInBondMoveHeader);
			AssertEquals(2, collection.Count);
			AssertDocumentLine(collection[0], "BOL: ABJD MB1");
			AssertDocumentLine(collection[1], @"
OPEN TEXT

HBOL: ABCD XXXY94842JFK");

			houseBillIssuingParty.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.TruckCarrierCode, "XXXY", Core.Constants.CountryCodes.UnitedStates);
			collection = new CBP7512DocumentLineCollection(cusInBondMoveHeader);
			AssertEquals(2, collection.Count);
			AssertDocumentLine(collection[0], "BOL: ABJD MB1");
			AssertDocumentLine(collection[1], @"
OPEN TEXT

HBOL: XXXY94842JFK");
		}

		public void TestPopulateLinesOnDeclarationWithIssuerSCAC()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_TransportMode = "SEA";
			declaration.JE_HouseBill = "HB039484";
			declaration.JE_HouseBillIssuerSCAC = "OOLU";
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = declaration.PK;
			header.BH_ParentTableCode = declaration.TablePrefix;
			header.BH_ImportTransportMode = TransportModeCodes.Codes.VesselContainer;
			var cusInBondMoveHeader = header.MovementHeaders.AddNew();
			cusInBondMoveHeader.BM_AdditionalText = "OPEN TEXT";
			var bill = header.Bills.AddNew();
			bill.B0_IssuerCode = "ABJD";
			bill.B0_MasterBillNumber = "MB1";
			var cusInBondMoveDetail = cusInBondMoveHeader.MovementDetails.AddNew();
			cusInBondMoveDetail.B9_B0 = bill.PK;
			cusInBondMoveDetail.B9_SeqNo = "2";
			var collection = new CBP7512DocumentLineCollection(cusInBondMoveHeader);
			AssertEquals(2, collection.Count);
			AssertDocumentLine(collection[0], "BOL: ABJD MB1");
			AssertDocumentLine(collection[1], @"
OPEN TEXT

HBOL: OOLU HB039484");
			header.BH_ImportTransportMode = TransportModeCodes.Codes.AirNonContainer;
			collection = new CBP7512DocumentLineCollection(cusInBondMoveHeader);
			AssertEquals(2, collection.Count);
			AssertDocumentLine(collection[1], @"
OPEN TEXT

HAWB: HB039484");
		}

		protected override CBP7512DocumentLineCollection GetCollectionToTest()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			return new CBP7512DocumentLineCollection(moveHeader);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => new CBP7512DocumentLine();

		void AssertDocumentLine(CBP7512DocumentLine line, ZString description)
		{
			AssertEquals(description, line.DescriptionAndQtyOfMerchandise);
		}
	}
}
