using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.InBond.Messaging.Interface;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using CodeDescriptionPairForTesting = Enterprise.Customs.DataTransfer.Universal.Testing.CodeDescriptionPairForTesting;
using WarehouseTransactionStatusList = Enterprise.Customs.Business.WarehouseTransactionStatusList;

namespace Enterprise.Customs.US.InBond.Business.Universal.Testing
{
	partial class InBondDataObjectWriterTest
	{
		public void TestInBondMappingsForWarehouse()
		{
			var foreignShipper = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			var consignee = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			var header = SetupCusInBondHeader(Factory.New<CusInBondHeader>(), true);
			var bill1 = SetupCusInBondBill(header.Bills.AddNew(), "HB24");
			bill1.ForeignShipper.E2_OA_Address = foreignShipper.MainAddress.PK;
			bill1.Consignee.E2_OA_Address = consignee.MainAddress.PK;
			bill1.NotifyParty.E2_AddressOverride = ZBool.True;
			bill1.NotifyParty.E2_CompanyName = "BOB THE BUILDER";
			bill1.NotifyParty.E2_Address1 = "ADDRESS 1";
			bill1.NotifyParty.E2_Address2 = "ADDRESS 2";
			bill1.NotifyParty.E2_City = "CITY BOB";
			bill1.NotifyParty.E2_State = "STATE BOB";
			bill1.NotifyParty.E2_Postcode = "39234";
			bill1.NotifyParty.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Bahamas;
			var ref1 = bill1.AdditionalReferences.AddNew();
			ref1.BR_Qualifier = ReferenceQualifierList.Codes.IN;
			ref1.BR_ReferenceNum = "I986";
			var ref2 = bill1.AdditionalReferences.AddNew();
			ref2.BR_Qualifier = ReferenceQualifierList.Codes.IN;
			ref2.BR_ReferenceNum = "I498";
			var ref3 = bill1.AdditionalReferences.AddNew();
			ref3.BR_Qualifier = ReferenceQualifierList.Codes.CG;
			ref3.BR_ReferenceNum = "C3234";
			var bill2 = SetupCusInBondBill(header.Bills.AddNew(), "HB89");
			var bill3 = SetupCusInBondBill(header.Bills.AddNew(), "HB43");
			var moveHeader1 = SetupWarehouse(SetupCusInBondMoveHeader(header.MovementHeaders.AddNew(), "INB323423"));
			var moveHeader1Detail1 = SetupCusInBondMoveDetail(moveHeader1.MovementDetails.AddNew(bill1.PK), "PRBILL1");
			var moveHeader1Detail2 = SetupCusInBondMoveDetail(moveHeader1.MovementDetails.AddNew(bill3.PK), "PRBILL3", "HB43");
			var moveHeader2 = SetupCusInBondMoveHeader(header.MovementHeaders.AddNew(), "INB864865");
			var moveHeader2Detail1 = SetupCusInBondMoveDetail(moveHeader2.MovementDetails.AddNew(bill2.PK), "PRBILL2", "HB89");
			var moveHeader2Detail2 = SetupCusInBondMoveDetail(moveHeader2.MovementDetails.AddNew(bill3.PK), "PRBILL3");
			var moveHeader3 = SetupCusInBondMoveHeader(header.MovementHeaders.AddNew(), "INB568996");
			var moveHeader3Detail1 = SetupCusInBondMoveDetail(moveHeader3.MovementDetails.AddNew(bill2.PK), "PRBILL2");
			var moveHeader3Detail2 = SetupCusInBondMoveDetail(moveHeader3.MovementDetails.AddNew(bill1.PK), "PRBILL1");
			((IInBondQXHeader)moveHeader1).Bills.ToArray();
			((IInBondQXHeader)moveHeader2).Bills.ToArray();
			((IInBondQXHeader)moveHeader3).Bills.ToArray();
			Factory.SaveForTesting();
			var writer = new WarehouseInBondDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, header)));
			var headerData = writer.GetDataObject(moveHeader1);
			AssertInBondHeaderContents(headerData, true, true);
			AssertEquals("headerData.AdditionalBillCollection.Count", 2, headerData.AdditionalBillCollection.Count);
			var billData = headerData.AdditionalBillCollection[0];
			AssertInBondBillContents(billData, "HB24");
			var organizationAddressCollection = billData.OrganizationAddressCollection;
			AssertEquals("billData.OrganizationAddressCollection.Count", 3, organizationAddressCollection.Count);
			AssertOrganizationBO_WUFSHIJNB("ForeignShipper", organizationAddressCollection.FirstOrDefault(address => address.AddressType.ToString() == nameof(DocAddressType.ForeignShipperDocumentaryAddress)), nameof(DocAddressType.ForeignShipperDocumentaryAddress));
			AssertOrganizationBO_CRAHOLSYD("Consignee", organizationAddressCollection.FirstOrDefault(address => address.AddressType.ToString() == nameof(DocAddressType.ConsigneeAddress)), nameof(DocAddressType.ConsigneeAddress));
			AssertAddress("NotifyParty", organizationAddressCollection.FirstOrDefault(address => address.AddressType.ToString() == nameof(DocAddressType.NotifyParty)), nameof(DocAddressType.NotifyParty), null, "BOB THE BUILDER", ZBool.True, "ADDRESS 1", "ADDRESS 2", "CITY BOB", "STATE BOB", "39234", Core.Constants.CountryCodes.Bahamas, "", "", "", "", "");
			AssertNotNull("billData.CustomsReferenceCollection", billData.CustomsReferenceCollection);
			AssertEquals("billData.CustomsReferenceCollection.Count", 3, billData.CustomsReferenceCollection.Count);
			AssertCustomsReferenceContents(billData.CustomsReferenceCollection[0], Constants.AdditionalReference.Type, ReferenceQualifierList.Codes.CG, "C3234");
			AssertCustomsReferenceContents(billData.CustomsReferenceCollection[1], Constants.AdditionalReference.Type, ReferenceQualifierList.Codes.IN, "I498");
			AssertCustomsReferenceContents(billData.CustomsReferenceCollection[2], Constants.AdditionalReference.Type, ReferenceQualifierList.Codes.IN, "I986");
			var billData2 = headerData.AdditionalBillCollection[1];
			AssertInBondBillContents(billData2, "HB43");
			AssertEquals("billData2.OrganizationAddressCollection.Count", 3, billData2.OrganizationAddressCollection.Count);
			AssertOrganizationBO_WUFSHIJNB("ForeignShipper", billData2.OrganizationAddressCollection.FirstOrDefault(address => address.AddressType.ToString() == nameof(DocAddressType.ForeignShipperDocumentaryAddress)), nameof(DocAddressType.ForeignShipperDocumentaryAddress));
			AssertOrganizationBO_CRAHOLSYD("Consignee", billData2.OrganizationAddressCollection.FirstOrDefault(address => address.AddressType.ToString() == nameof(DocAddressType.ConsigneeAddress)), nameof(DocAddressType.ConsigneeAddress));
			AssertAddress("NotifyParty", billData2.OrganizationAddressCollection.FirstOrDefault(address => address.AddressType.ToString() == nameof(DocAddressType.NotifyParty)), nameof(DocAddressType.NotifyParty), null, "BOB THE BUILDER", ZBool.True, "ADDRESS 1", "ADDRESS 2", "CITY BOB", "STATE BOB", "39234", Core.Constants.CountryCodes.Bahamas, "", "", "", "", "");
			AssertNotNull("headerData.InBondMoveHeaderCollection", headerData.InBondMoveHeaderCollection);
			AssertEquals("headerData.InBondMoveHeaderCollection.Count", 1, headerData.InBondMoveHeaderCollection.Count);
			var moveHeader1Data = headerData.InBondMoveHeaderCollection[0];
			AssertInBondMoveHeaderContents(moveHeader1Data, "INB323423");
			AssertNotNull("moveHeader1Data.InBondMoveDetailCollection", moveHeader1Data.InBondMoveDetailCollection);
			AssertEquals("moveHeader1Data.InBondMoveDetailCollection.Count", 2, moveHeader1Data.InBondMoveDetailCollection.Count);
			AssertInBondMoveDetailContents(moveHeader1Data.InBondMoveDetailCollection[0], "0001", 1, "PRBILL1", new[] { new ContainerLink()
			{ Link = 1, ContainerNumber = "CNT121" }, new ContainerLink()
			{ Link = 2, ContainerNumber = "CNT212" } });
			AssertInBondMoveDetailContents(moveHeader1Data.InBondMoveDetailCollection[1], "0002", 2, "PRBILL3", new[] { new ContainerLink()
			{ Link = 3, ContainerNumber = "CNT121" }, new ContainerLink()
			{ Link = 4, ContainerNumber = "CNT212" } });
			AssertNotNull("headerData.ContainerCollection", headerData.ContainerCollection);
			AssertEquals("headerData.ContainerCollection.Count", 4, headerData.ContainerCollection.Count);
			AssertInBondContainerContents(headerData.ContainerCollection[0], "CNT121", 1);
			AssertInBondContainerContents(headerData.ContainerCollection[1], "CNT212", 2);
			AssertInBondContainerContents(headerData.ContainerCollection[2], "CNT121", 3);
			AssertInBondContainerContents(headerData.ContainerCollection[3], "CNT212", 4);
			AssertNotNull("headerData.PackingLineCollection", headerData.PackingLineCollection);
			AssertEquals("headerData.PackingLineCollection.Count", 8, headerData.PackingLineCollection.Count);
			AssertInBondCommodityContents(headerData.PackingLineCollection[0], 1, "203040");
			AssertInBondCommodityContents(headerData.PackingLineCollection[1], 1, "304050");
			AssertInBondCommodityContents(headerData.PackingLineCollection[2], 2, "203040");
			AssertInBondCommodityContents(headerData.PackingLineCollection[3], 2, "304050");
			AssertInBondCommodityContents(headerData.PackingLineCollection[4], 3, Classification.CC_TariffNum, 10, CodeDescriptionPairForTesting.New(AMS.Business.ManifestUnitList.Codes.Bag, AMS.Business.ManifestUnitList.Descriptions.Bag), Part.OP_Desc, 1500m, 150m, CodeDescriptionPairForTesting.New(Core.Constants.Weight.Pounds, "Pounds"), "MARKS LOOK FUNNY");
			AssertInBondCommodityContents(headerData.PackingLineCollection[5], 3, Pivot2.CI_TariffNum, 10, CodeDescriptionPairForTesting.New(AMS.Business.ManifestUnitList.Codes.Bag, AMS.Business.ManifestUnitList.Descriptions.Bag), Part2.OP_Desc, 1500m, 250m, CodeDescriptionPairForTesting.New(Core.Constants.Weight.Kilograms, "Kilograms"), "MARKS LOOK FUNNY");
			AssertInBondCommodityContents(headerData.PackingLineCollection[6], 4, Classification.CC_TariffNum, 10, CodeDescriptionPairForTesting.New(AMS.Business.ManifestUnitList.Codes.Bag, AMS.Business.ManifestUnitList.Descriptions.Bag), Part.OP_Desc, 1500m, 150m, CodeDescriptionPairForTesting.New(Core.Constants.Weight.Pounds, "Pounds"), "MARKS LOOK FUNNY");
			AssertInBondCommodityContents(headerData.PackingLineCollection[7], 4, Pivot2.CI_TariffNum, 10, CodeDescriptionPairForTesting.New(AMS.Business.ManifestUnitList.Codes.Bag, AMS.Business.ManifestUnitList.Descriptions.Bag), Part2.OP_Desc, 1500m, 250m, CodeDescriptionPairForTesting.New(Core.Constants.Weight.Kilograms, "Kilograms"), "MARKS LOOK FUNNY");
			AssertEquals("headerData.CommercialInfo.CommercialInvoiceCollection.Count", 1, headerData.CommercialInfo.CommercialInvoiceCollection.Count);
			var invoiceData = headerData.CommercialInfo.CommercialInvoiceCollection[0];
			AssertEquals("invoiceData.CommercialInvoiceLineCollection.Count", 8, invoiceData.CommercialInvoiceLineCollection.Count);
			AssertCommercialInvoiceLine(invoiceData.CommercialInvoiceLineCollection[0], 1, AssertOrganizationBO_INTHEMSYD, "", null, null, null, 0m, null);
			AssertCommercialInvoiceLine(invoiceData.CommercialInvoiceLineCollection[1], 2, AssertOrganizationBO_INTHEMSYD, "", null, null, null, 0m, null);
			AssertCommercialInvoiceLine(invoiceData.CommercialInvoiceLineCollection[2], 3, AssertOrganizationBO_INTHEMSYD, "", null, null, null, 0m, null);
			AssertCommercialInvoiceLine(invoiceData.CommercialInvoiceLineCollection[3], 4, AssertOrganizationBO_INTHEMSYD, "", null, null, null, 0m, null);
			AssertCommercialInvoiceLine(invoiceData.CommercialInvoiceLineCollection[4], 5, AssertOrganizationBO_INTHEMSYD, Part.OP_PartNum, null, "HB43", 1, 100m, 100m);
			AssertCommercialInvoiceLine(invoiceData.CommercialInvoiceLineCollection[5], 6, AssertOrganizationBO_CRAHOLSYD, Part2.OP_PartNum, null, "HB43", 2, 200m, 200m);
			AssertCommercialInvoiceLine(invoiceData.CommercialInvoiceLineCollection[6], 7, AssertOrganizationBO_INTHEMSYD, Part.OP_PartNum, null, "HB43", 1, 100m, 100m);
			AssertCommercialInvoiceLine(invoiceData.CommercialInvoiceLineCollection[7], 8, AssertOrganizationBO_CRAHOLSYD, Part2.OP_PartNum, null, "HB43", 2, 200m, 200m);
		}

		CusInBondMoveHeader SetupWarehouse(CusInBondMoveHeader moveHeader)
		{
			moveHeader.BM_OA_WarehouseAddress = Warehouse.MainAddress.PK;
			moveHeader.BM_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCreated;
			return moveHeader;
		}
	}
}
