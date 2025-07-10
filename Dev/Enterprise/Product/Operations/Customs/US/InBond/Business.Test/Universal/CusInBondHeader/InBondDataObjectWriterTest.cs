using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.US.InBond.Messaging.Interface;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Testing;
using CodeDescriptionPairForTesting = Enterprise.Customs.DataTransfer.Universal.Testing.CodeDescriptionPairForTesting;

namespace Enterprise.Customs.US.InBond.Business.Universal.Testing
{
	partial class InBondDataObjectWriterTest : DataTransfer.Universal.Testing.InBondDataObjectWriterTest
	{
		public void TestCommercialInvoiceLineCollectionWriterStrategy()
		{
			var header = Factory.New<CusInBondHeader>();
			var writer = new CusInBondHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, header),
				writerStrategy: new DataObjectWriterStrategyTestClass(s => s != nameof(CommercialInvoiceHeader.CommercialInvoiceLineCollection))));
			var shipment = writer.GetDataObject(header);
			var commercialInvoiceHeader = shipment.CommercialInfo.CommercialInvoiceCollection.Single();
			AssertNull("CommercialInvoiceLineCollection - writerStrategy not allow", commercialInvoiceHeader.CommercialInvoiceLineCollection);

			writer = new CusInBondHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, header)));
			shipment = writer.GetDataObject(header);
			commercialInvoiceHeader = shipment.CommercialInfo.CommercialInvoiceCollection.Single();
			AssertNotNull("CommercialInvoiceLineCollection - writerStrategy allow", commercialInvoiceHeader.CommercialInvoiceLineCollection);
		}

		public void TestInBondMappingsForChildCommodities()
		{
			Importer.MiscServ.OM_IMPartAttrib1Name = "VIN1";
			Importer.MiscServ.OM_IMPartAttrib2Name = "VIN2";
			Importer.MiscServ.OM_IMPartAttrib3Name = "VIN3";
			Importer.MiscServ.OM_IMUseSerialNumber = true;
			var supplier1 = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);
			var supplier2 = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			var header = Factory.New<CusInBondHeader>();
			header.BH_ImportTransportMode = TransportModeCodes.Codes.VesselContainer;
			header.BH_OA_Importer = Importer.MainAddress.PK;
			header.BH_OH_Supplier = supplier1.PK;
			header.BH_FTZMove = true;
			var bill = SetupCusInBondBill(header.Bills.AddNew(), "HB24");
			var moveHeader = header.MovementHeaders.AddNew();
			var moveHeaderDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var container = moveHeaderDetail.Containers.AddNew();
			container.BC_ContainerNum = "CONT324";
			var commodity1 = SetupCusInBondCargoDesc(container.Commodities.AddNew(), "1010102030");
			commodity1.BY_WarehouseEntryNumber = "HB24";
			var commodity2 = SetupCusInBondCargoDesc(container.Commodities.AddNew(), "2010203040");
			commodity2.BY_PartNumber = "PART1";
			commodity2.BY_PartAttrib1 = "ATT1";
			commodity2.BY_PartAttrib2 = "ATT2";
			commodity2.BY_PartAttrib3 = "ATT3";
			commodity2.BY_SerialNumber = "SN";
			commodity2.BY_WarehouseEntryNumber = "HB24";
			commodity2.BY_WarehouseEntryLineNo = 1;
			commodity2.BY_InvoiceQuantity = 2m;
			var commodity3 = SetupCusInBondCargoDesc(container.Commodities.AddNew(), "", 112, "N3", "A DESCRIPTION THAT IS MORE THAN 65 CHAR LONG AND THAT SHOULD NOT BE CUT", ZDecimal.Zero, ZDecimal.Zero, "", "MARKS BOB 3");
			commodity3.BY_WarehouseEntryNumber = "HB24";
			var commodity3Child1 = SetupCusInBondCargoDesc(commodity3.ChildCommodities.AddNew(), "3010101030", ZInt.Zero, "", "", 350m, 3.5m, "KG", "");
			commodity3.BY_WarehouseEntryNumber = "HB24";
			var commodity3Child2 = SetupCusInBondCargoDesc(commodity3.ChildCommodities.AddNew(), "", ZInt.Zero, "", "", ZDecimal.Zero, ZDecimal.Zero, "", "");
			commodity3Child2.BY_OH_Supplier = supplier2.PK;
			commodity3Child2.BY_PartNumber = "PART2";
			commodity3Child2.BY_PartAttrib1 = "PART2ATT1";
			commodity3Child2.BY_PartAttrib2 = "PART2ATT2";
			commodity3Child2.BY_PartAttrib3 = "PART2ATT3";
			commodity3Child2.BY_SerialNumber = "PART2SN";
			commodity3Child2.BY_WarehouseEntryNumber = "HB24";
			commodity3Child2.BY_WarehouseEntryLineNo = 2;
			commodity3Child2.BY_InvoiceQuantity = 3m;
			var commodity3Child2Child = SetupCusInBondCargoDesc(commodity3Child2.ChildCommodities.AddNew(), "4010203010", ZInt.Zero, "", "", 403.4m, 54.23m, "LP", "");
			var writer = new CusInBondHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, header)));
			var headerData = writer.GetDataObject(header);
			AssertEquals(2, headerData.OrganizationAddressCollection.Count);
			var importerAddressData = headerData.OrganizationAddressCollection[0];
			var supplierAddressData = headerData.OrganizationAddressCollection[1];
			if (supplierAddressData.AddressType.GetValueOrDefault() == nameof(DocAddressType.ImporterDocumentaryAddress))
			{
				importerAddressData = headerData.OrganizationAddressCollection[1];
				supplierAddressData = headerData.OrganizationAddressCollection[0];
			}

			AssertOrganizationBO_WUFSHIJNB("Importer", importerAddressData, nameof(DocAddressType.ImporterDocumentaryAddress));
			AssertOrganizationBO_INTHEMSYD("Supplier", supplierAddressData, nameof(DocAddressType.SupplierDocumentaryAddress));
			AssertEquals(1, headerData.CommercialInfo.CommercialInvoiceCollection.Count);
			var commercialInvoiceLineCollection = headerData.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection;
			AssertEquals(6, commercialInvoiceLineCollection.Count);
			var keyValuePairsWithNoValues = new[] { new KeyValuePair("VIN1", ""), new KeyValuePair("VIN2", ""), new KeyValuePair("VIN3", "") };
			var expectedKeyValuePairsWithNoValues = keyValuePairsWithNoValues.Append(new KeyValuePair("Serial Number", "")).ToArray();
			var keyValuePairsPart1 = new[] { new KeyValuePair("VIN1", "ATT1"), new KeyValuePair("VIN2", "ATT2"), new KeyValuePair("VIN3", "ATT3") };
			var expectedKeyValuePairsPart1 = keyValuePairsPart1.Append(new KeyValuePair("Serial Number", "SN")).ToArray();
			var keyValuePairsPart2 = new[] { new KeyValuePair("VIN1", "PART2ATT1"), new KeyValuePair("VIN2", "PART2ATT2"), new KeyValuePair("VIN3", "PART2ATT3") };
			var expectedKeyValuePairsPart2 = keyValuePairsPart2.Append(new KeyValuePair("Serial Number", "PART2SN")).ToArray();
			AssertCommercialInvoiceLine(commercialInvoiceLineCollection[0], 1, null, "", expectedKeyValuePairsWithNoValues, "HB24", null, 0m, null);
			AssertCommercialInvoiceLine(commercialInvoiceLineCollection[1], 2, AssertOrganizationBO_CRAHOLSYD, "PART2", expectedKeyValuePairsPart2, "HB24", 2, 3m, null);
			AssertCommercialInvoiceLine(commercialInvoiceLineCollection[2], 3, null, "", expectedKeyValuePairsWithNoValues, null, null, 0m, null);
			AssertCommercialInvoiceLine(commercialInvoiceLineCollection[3], 4, AssertOrganizationBO_INTHEMSYD, "", expectedKeyValuePairsWithNoValues, null, null, 0m, null);
			AssertCommercialInvoiceLine(commercialInvoiceLineCollection[4], 5, AssertOrganizationBO_INTHEMSYD, "", expectedKeyValuePairsWithNoValues, "HB24", null, 0m, null);
			AssertCommercialInvoiceLine(commercialInvoiceLineCollection[5], 6, AssertOrganizationBO_INTHEMSYD, "PART1", expectedKeyValuePairsPart1, "HB24", 1, 2m, null);
			AssertEquals(3, headerData.PackingLineCollection.Count);
			var packingLine1 = headerData.PackingLineCollection[0];
			AssertInBondCommodityContents(packingLine1, 1, "", 112, CodeDescriptionPairForTesting.New("N3", null), "A DESCRIPTION THAT IS MORE THAN 65 CHAR LONG AND THAT SHOULD NOT BE CUT", ZDecimal.Zero, ZDecimal.Zero, CodeDescriptionPairForTesting.New("", null), "MARKS BOB 3");
			AssertCommercialInvoiceLineLink(packingLine1, 1);
			AssertEquals(2, packingLine1.PackingLineCollection.Count);
			var packingLine1Line1 = packingLine1.PackingLineCollection[0];
			AssertInBondCommodityContents(packingLine1Line1, null, "", 0, CodeDescriptionPairForTesting.New("", null), "", ZDecimal.Zero, ZDecimal.Zero, CodeDescriptionPairForTesting.New("", null), "");
			AssertCommercialInvoiceLineLink(packingLine1Line1, 2);
			AssertEquals(1, packingLine1Line1.PackingLineCollection.Count);
			var packingLine1Line1Line = packingLine1Line1.PackingLineCollection[0];
			AssertInBondCommodityContents(packingLine1Line1Line, null, "4010203010", 0, CodeDescriptionPairForTesting.New("", null), "", 403.4m, 54.23m, CodeDescriptionPairForTesting.New("LP", null), "");
			AssertCommercialInvoiceLineLink(packingLine1Line1Line, 3);
			AssertEquals(0, packingLine1Line1Line.PackingLineCollection.Count);
			var packingLine1Line2 = packingLine1.PackingLineCollection[1];
			AssertInBondCommodityContents(packingLine1Line2, null, "3010101030", 0, CodeDescriptionPairForTesting.New("", null), "", 350m, 3.5m, CodeDescriptionPairForTesting.New("KG", "Kilograms"), "");
			AssertCommercialInvoiceLineLink(packingLine1Line2, 4);
			AssertEquals(0, packingLine1Line2.PackingLineCollection.Count);
			var packingLine2 = headerData.PackingLineCollection[1];
			AssertInBondCommodityContents(packingLine2, 1, "1010102030");
			AssertCommercialInvoiceLineLink(packingLine2, 5);
			AssertEquals(0, packingLine2.PackingLineCollection.Count);
			var packingLine3 = headerData.PackingLineCollection[2];
			AssertInBondCommodityContents(packingLine3, 1, "2010203040");
			AssertCommercialInvoiceLineLink(packingLine3, 6);
			AssertEquals(0, packingLine3.PackingLineCollection.Count);
		}

		public void TestInBondMappings()
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
			var moveHeader1 = SetupCusInBondMoveHeader(header.MovementHeaders.AddNew(), "INB323423");
			var moveHeader1Detail1 = SetupCusInBondMoveDetail(moveHeader1.MovementDetails.AddNew(bill1.PK), "PRBILL1");
			var moveHeader1Detail2 = SetupCusInBondMoveDetail(moveHeader1.MovementDetails.AddNew(bill3.PK), "PRBILL3");
			var moveHeader2 = SetupCusInBondMoveHeader(header.MovementHeaders.AddNew(), "INB864865");
			var moveHeader2Detail1 = SetupCusInBondMoveDetail(moveHeader2.MovementDetails.AddNew(bill2.PK), "PRBILL2");
			var moveHeader2Detail2 = SetupCusInBondMoveDetail(moveHeader2.MovementDetails.AddNew(bill3.PK), "PRBILL3");
			var moveHeader3 = SetupCusInBondMoveHeader(header.MovementHeaders.AddNew(), "INB568996");
			var moveHeader3Detail1 = SetupCusInBondMoveDetail(moveHeader3.MovementDetails.AddNew(bill2.PK), "PRBILL2");
			var moveHeader3Detail2 = SetupCusInBondMoveDetail(moveHeader3.MovementDetails.AddNew(bill1.PK), "PRBILL1");
			((IInBondQXHeader)moveHeader1).Bills.ToArray();
			((IInBondQXHeader)moveHeader2).Bills.ToArray();
			((IInBondQXHeader)moveHeader3).Bills.ToArray();
			Factory.SaveForTesting();
			var writer = new CusInBondHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, header)));
			var headerData = writer.GetDataObject(header);
			AssertInBondHeaderContents(headerData, true);
			AssertEquals("headerData.AdditionalBillCollection.Count", 3, headerData.AdditionalBillCollection.Count);
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
			var billData3 = headerData.AdditionalBillCollection[2];
			AssertInBondBillContents(billData3, "HB89");
			AssertEquals("billData3.OrganizationAddressCollection.Count", 3, billData3.OrganizationAddressCollection.Count);
			AssertOrganizationBO_WUFSHIJNB("ForeignShipper", billData3.OrganizationAddressCollection.FirstOrDefault(address => address.AddressType.ToString() == nameof(DocAddressType.ForeignShipperDocumentaryAddress)), nameof(DocAddressType.ForeignShipperDocumentaryAddress));
			AssertOrganizationBO_CRAHOLSYD("Consignee", billData3.OrganizationAddressCollection.FirstOrDefault(address => address.AddressType.ToString() == nameof(DocAddressType.ConsigneeAddress)), nameof(DocAddressType.ConsigneeAddress));
			AssertAddress("NotifyParty", billData3.OrganizationAddressCollection.FirstOrDefault(address => address.AddressType.ToString() == nameof(DocAddressType.NotifyParty)), nameof(DocAddressType.NotifyParty), null, "BOB THE BUILDER", ZBool.True, "ADDRESS 1", "ADDRESS 2", "CITY BOB", "STATE BOB", "39234", Core.Constants.CountryCodes.Bahamas, "", "", "", "", "");
			AssertEquals("billData3.CustomsReferenceCollection.Count", 0, billData3.CustomsReferenceCollection.Count);
			AssertNotNull("headerData.InBondMoveHeaderCollection", headerData.InBondMoveHeaderCollection);
			AssertEquals("headerData.InBondMoveHeaderCollection.Count", 3, headerData.InBondMoveHeaderCollection.Count);
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
			var moveHeader2Data = headerData.InBondMoveHeaderCollection[1];
			AssertInBondMoveHeaderContents(moveHeader2Data, "INB568996");
			AssertNotNull("moveHeader2Data.InBondMoveDetailCollection", moveHeader2Data.InBondMoveDetailCollection);
			AssertEquals("moveHeader2Data.InBondMoveDetailCollection.Count", 2, moveHeader2Data.InBondMoveDetailCollection.Count);
			AssertInBondMoveDetailContents(moveHeader2Data.InBondMoveDetailCollection[0], "0001", 1, "PRBILL1", new[] { new ContainerLink()
			{ Link = 5, ContainerNumber = "CNT121" }, new ContainerLink()
			{ Link = 6, ContainerNumber = "CNT212" } });
			AssertInBondMoveDetailContents(moveHeader2Data.InBondMoveDetailCollection[1], "0002", 3, "PRBILL2", new[] { new ContainerLink()
			{ Link = 7, ContainerNumber = "CNT121" }, new ContainerLink()
			{ Link = 8, ContainerNumber = "CNT212" } });
			var moveHeader3Data = headerData.InBondMoveHeaderCollection[2];
			AssertInBondMoveHeaderContents(moveHeader3Data, "INB864865");
			AssertNotNull("moveHeader3Data.InBondMoveDetailCollection", moveHeader3Data.InBondMoveDetailCollection);
			AssertEquals("moveHeader3Data.InBondMoveDetailCollection.Count", 2, moveHeader3Data.InBondMoveDetailCollection.Count);
			AssertInBondMoveDetailContents(moveHeader3Data.InBondMoveDetailCollection[0], "0001", 2, "PRBILL3", new[] { new ContainerLink()
			{ Link = 9, ContainerNumber = "CNT121" }, new ContainerLink()
			{ Link = 10, ContainerNumber = "CNT212" } });
			AssertInBondMoveDetailContents(moveHeader3Data.InBondMoveDetailCollection[1], "0002", 3, "PRBILL2", new[] { new ContainerLink()
			{ Link = 11, ContainerNumber = "CNT121" }, new ContainerLink()
			{ Link = 12, ContainerNumber = "CNT212" } });
			AssertNotNull("headerData.ContainerCollection", headerData.ContainerCollection);
			AssertEquals("headerData.ContainerCollection.Count", 12, headerData.ContainerCollection.Count);
			AssertInBondContainerContents(headerData.ContainerCollection[0], "CNT121", 1);
			AssertInBondContainerContents(headerData.ContainerCollection[1], "CNT212", 2);
			AssertInBondContainerContents(headerData.ContainerCollection[2], "CNT121", 3);
			AssertInBondContainerContents(headerData.ContainerCollection[3], "CNT212", 4);
			AssertInBondContainerContents(headerData.ContainerCollection[4], "CNT121", 5);
			AssertInBondContainerContents(headerData.ContainerCollection[5], "CNT212", 6);
			AssertInBondContainerContents(headerData.ContainerCollection[6], "CNT121", 7);
			AssertInBondContainerContents(headerData.ContainerCollection[7], "CNT212", 8);
			AssertInBondContainerContents(headerData.ContainerCollection[8], "CNT121", 9);
			AssertInBondContainerContents(headerData.ContainerCollection[9], "CNT212", 10);
			AssertInBondContainerContents(headerData.ContainerCollection[10], "CNT121", 11);
			AssertInBondContainerContents(headerData.ContainerCollection[11], "CNT212", 12);
			AssertNotNull("headerData.PackingLineCollection", headerData.PackingLineCollection);
			AssertEquals("headerData.PackingLineCollection.Count", 24, headerData.PackingLineCollection.Count);
			AssertInBondCommodityContents(headerData.PackingLineCollection[0], 1, "203040");
			AssertInBondCommodityContents(headerData.PackingLineCollection[1], 1, "304050");
			AssertInBondCommodityContents(headerData.PackingLineCollection[2], 2, "203040");
			AssertInBondCommodityContents(headerData.PackingLineCollection[3], 2, "304050");
			AssertInBondCommodityContents(headerData.PackingLineCollection[4], 3, "203040");
			AssertInBondCommodityContents(headerData.PackingLineCollection[5], 3, "304050");
			AssertInBondCommodityContents(headerData.PackingLineCollection[6], 4, "203040");
			AssertInBondCommodityContents(headerData.PackingLineCollection[7], 4, "304050");
			AssertInBondCommodityContents(headerData.PackingLineCollection[8], 5, "203040");
			AssertInBondCommodityContents(headerData.PackingLineCollection[9], 5, "304050");
			AssertInBondCommodityContents(headerData.PackingLineCollection[10], 6, "203040");
			AssertInBondCommodityContents(headerData.PackingLineCollection[11], 6, "304050");
			AssertInBondCommodityContents(headerData.PackingLineCollection[12], 7, "203040");
			AssertInBondCommodityContents(headerData.PackingLineCollection[13], 7, "304050");
			AssertInBondCommodityContents(headerData.PackingLineCollection[14], 8, "203040");
			AssertInBondCommodityContents(headerData.PackingLineCollection[15], 8, "304050");
			AssertInBondCommodityContents(headerData.PackingLineCollection[16], 9, "203040");
			AssertInBondCommodityContents(headerData.PackingLineCollection[17], 9, "304050");
			AssertInBondCommodityContents(headerData.PackingLineCollection[18], 10, "203040");
			AssertInBondCommodityContents(headerData.PackingLineCollection[19], 10, "304050");
			AssertInBondCommodityContents(headerData.PackingLineCollection[20], 11, "203040");
			AssertInBondCommodityContents(headerData.PackingLineCollection[21], 11, "304050");
			AssertInBondCommodityContents(headerData.PackingLineCollection[22], 12, "203040");
			AssertInBondCommodityContents(headerData.PackingLineCollection[23], 12, "304050");
		}

		protected override BusinessObject CreateBizObjWithParent(out Customs.Business.CusInBondHeader header)
		{
			var declaration = Factory.New<US.Business.JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.US.Business.JobMessageTypeList.Codes.Import;
			header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = declaration.PK;
			header.BH_ParentTableCode = declaration.TablePrefix;
			header.BH_ImportConveyanceName = "BOB VESSEL";
			Factory.SaveForTesting();
			return declaration;
		}

		protected override DataContextType GetDataContextType => DataContextType.InBond;

		CusInBondHeader SetupCusInBondHeader(CusInBondHeader header, bool setupSupplier = false)
		{
			header.BH_FTZMove = ZBool.True;
			header.BH_CarrierSCAC = CarrierCode1;
			header.BH_ImportTransportMode = TransportModeCodes.Codes.VesselContainer;
			header.BH_ImportConveyanceName = APLVessel.RV_Code;
			header.BH_ImportConveyanceCountry = APLVessel.RV_RN_NKCountryOfReg;
			header.BH_VoyageNumber = "V324";
			header.BH_LloydsNumber = APLVessel.RV_LloydsNumber;
			header.BH_PortUnladingDCode = SeaLocalPort1ScheduleD.ZZD_Code;
			header.BH_ETA = new ZDateTime(2014, 2, 10);
			header.BH_FIRMS = "F320";
			header.BH_GB = InBondBranch.PK;
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			header.BH_OA_Importer = Importer.MainAddress.PK;
			if (setupSupplier)
			{
				header.BH_OH_Supplier = Supplier.PK;
			}

			header.BH_ImportLoadPortKCode = SeaForeignPort1ScheduleK.ZZD_Code;
			header.BH_RN_NKFirstExportCountry = Core.Constants.CountryCodes.Australia;
			header.BH_FirstExportDate = new ZDateTime(2014, 1, 20);
			header.BH_SailingDate = new ZDateTime(2014, 1, 19);
			header.Notes.AddNew(ZBool.True, "DUMMY NOTE", "HELLO WORLD");
			header.Notes.AddNew(ZBool.False, "DUMMY NOTE 2", "GOODBYE WORLD");
			return header;
		}

		void AssertInBondHeaderContents(Shipment headerData, bool hasSupplier = false, bool? hasWarehouseData = false)
		{
			AssertInBondHeaderContents(headerData, CodeDescriptionPairForTesting.New(InBondBranch.GB_Code, InBondBranch.GB_BranchName),
				CodeDescriptionPairForTesting.New(US.Business.TransportTypeList.Codes.Sea, US.Business.TransportTypeList.Descriptions.Sea),
				CodeDescriptionPairForTesting.New(US.Business.ContainerModeList.Codes.Containerized, US.Business.ContainerModeList.Descriptions.Containerized), APLVessel.RV_Code,
				CodeDescriptionPairForTesting.New(Core.Constants.CountryCodes.Jamaica, "Jamaica"), APLVessel.RV_LloydsNumber, "V324",
				CodeDescriptionPairForTesting.New(SeaForeignPort1.RL_Code, SeaForeignPort1.RL_PortName),
				CodeDescriptionPairForTesting.New(SeaLocalPort1.RL_Code, SeaLocalPort1.RL_PortName), new ZDateTime(2014, 2, 10), new ZDateTime(2014, 1, 19), ZBool.True, CarrierCode1,
				SeaLocalPort1ScheduleD.ZZD_Code, "F320", InBondHeaderTypeList.Codes.FullData, SeaForeignPort1ScheduleK.ZZD_Code);
			AssertEquals("headerData.TransportLegCollection.Count", 2, headerData.TransportLegCollection.Count);
			var preCarriageLeg = headerData.TransportLegCollection[0];
			AssertEquals("preCarriageLeg.LegOrder", (ZByte)1, preCarriageLeg.LegOrder);
			AssertNotNull("preCarriageLeg.PortOfLoading", preCarriageLeg.PortOfLoading);
			AssertEquals("preCarriageLeg.PortOfLoading.Code", Core.Constants.CountryCodes.Australia, preCarriageLeg.PortOfLoading.Code);
			AssertEquals("preCarriageLeg.PortOfLoading.Name", "Australia", preCarriageLeg.PortOfLoading.Name);
			AssertEquals("preCarriageLeg.ActualDeparture", new ZDateTime(2014, 1, 20), preCarriageLeg.ActualDeparture);
			AssertTransportLeg(headerData.TransportLegCollection[1], 2, APLVessel.RV_Code, APLVessel.RV_LloydsNumber, "V324", TransportMode.Sea, CodeDescriptionPairForTesting.New(SeaForeignPort1.RL_Code, SeaForeignPort1.RL_PortName), CodeDescriptionPairForTesting.New(SeaLocalPort1.RL_Code, SeaLocalPort1.RL_PortName), new ZDateTime(2014, 1, 19), new ZDateTime(2014, 2, 10));
			var organisationCount = 1;
			var hasWarehouseDataValue = hasWarehouseData.GetValueOrDefault();
			if (hasWarehouseDataValue)
			{
				organisationCount++;
			}

			if (hasSupplier)
			{
				organisationCount++;
			}

			AssertEquals("headerData.OrganizationAddressCollection.Count", organisationCount, headerData.OrganizationAddressCollection.Count);
			var importerAddressData = headerData.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == nameof(DocAddressType.ImporterDocumentaryAddress));
			AssertOrganizationBO_WUFSHIJNB("Importer", importerAddressData, nameof(DocAddressType.ImporterDocumentaryAddress));
			if (hasSupplier)
			{
				var supplierAddressData = headerData.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == nameof(DocAddressType.SupplierDocumentaryAddress));
				AssertOrganizationBO_INTHEMSYD("Supplier", supplierAddressData, nameof(DocAddressType.SupplierDocumentaryAddress));
			}

			AssertNotNull("headerData.NoteCollection", headerData.NoteCollection);
			AssertContainNote(headerData.NoteCollection, ZBool.True, "DUMMY NOTE", "HELLO WORLD");
			AssertContainNote(headerData.NoteCollection, ZBool.False, "DUMMY NOTE 2", "GOODBYE WORLD");
			if (hasWarehouseDataValue)
			{
				var dataSourceCollection = headerData.DataContext.DataSourceCollection.ToArray();
				AssertEquals("dataSourceCollection.Length", 1, dataSourceCollection.Length);
				AssertEquals("dataSourceCollection[0].Type", nameof(DataContextType.WarehouseInBond), dataSourceCollection[0].Type);
				AssertNotNull("headerData.MessageType", headerData.MessageType);
				AssertEquals("headerData.MessageType.Code", CusInBondApplicationCodeList.Codes.InBond, headerData.MessageType.Code);
				AssertEquals("headerData.MessageType.Description", CusInBondApplicationCodeList.Descriptions.InBond, headerData.MessageType.Description);
				var warehouseAddressData = headerData.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == nameof(DocAddressType.CustomsWarehouseAddress));
				AssertOrganizationBO_INTHEMSYD("Warehouse", warehouseAddressData, nameof(DocAddressType.CustomsWarehouseAddress));
			}
			else
			{
				AssertEquals("headerData.DataContext.DataSourceCollection.Count()", 1, headerData.DataContext.DataSourceCollection.Count());
				AssertEquals("headerData.DataContext.DataSourceCollection.Type", nameof(DataContextType.InBond), headerData.DataContext.DataSourceCollection.First().Type);
				AssertNull("headerData.MessageType", headerData.MessageType);
			}
		}

		void AssertTransportLeg(TransportLeg transportLegData, ZByte? legOrder, ZString? vessel, ZString? lloyds, ZString? voyage, TransportMode transportMode, ICodeDescription portOfLoading,
			ICodeDescription portOfDischarge, ZDateTime actualDeparture, ZDateTime estimatedArrival)
		{
			AssertNotNull("Precondition: transportLegData", transportLegData);
			CombineAssertions(delegate
			{
				AssertEquals("transportLegData.LegOrder", legOrder, transportLegData.LegOrder);
				AssertEquals("transportLegData.VesselName", vessel, transportLegData.VesselName);
				AssertEquals("transportLegData.VesselLloydsIMO", lloyds, transportLegData.VesselLloydsIMO);
				AssertEquals("transportLegData.VoyageFlightNo", voyage, transportLegData.VoyageFlightNo);
				AssertEquals("transportLegData.TransportMode", transportMode, transportLegData.TransportMode);
				if (portOfLoading == null)
				{
					AssertNull("transportLegData.PortOfLoading", transportLegData.PortOfLoading);
				}
				else
				{
					AssertNotNull("transportLegData.PortOfLoading", transportLegData.PortOfLoading);
					AssertEquals("transportLegData.PortOfLoading.Code", portOfLoading.Code, transportLegData.PortOfLoading.Code);
					AssertEquals("transportLegData.PortOfLoading.Name", portOfLoading.Description, transportLegData.PortOfLoading.Name);
				}

				if (portOfDischarge == null)
				{
					AssertNull("transportLegData.PortOfDischarge", transportLegData.PortOfDischarge);
				}
				else
				{
					AssertNotNull("transportLegData.PortOfDischarge", transportLegData.PortOfDischarge);
					AssertEquals("transportLegData.PortOfDischarge.Code", portOfDischarge.Code, transportLegData.PortOfDischarge.Code);
					AssertEquals("transportLegData.PortOfDischarge.Name", portOfDischarge.Description, transportLegData.PortOfDischarge.Name);
				}

				AssertEquals("transportLegData.ActualDeparture", actualDeparture, transportLegData.ActualDeparture);
				AssertEquals("transportLegData.EstimatedArrival", estimatedArrival, transportLegData.EstimatedArrival);
			});
		}

		void AssertInBondHeaderContents(Shipment headerData, ICodeDescription branch, ICodeDescription transportMode, ICodeDescription containerMode, ZString? vessel, ICodeDescription vesselCountryOfRegistration,
			ZString? lloyds, ZString? voyage, ICodeDescription portOfLoading, ICodeDescription portOfDischarge, ZDateTime? eta, ZDateTime? sailingDate, ZBool? ftzMove, ZString? carrierSCAC,
			ZString? portUnladingDCode, ZString? fIRMS, ZString? headerType, ZString? importLoadPortKCode)
		{
			AssertNotNull("Precondition: headerData", headerData);
			CombineAssertions(delegate
			{
				AssertNull("headerData.WayBillNumber", headerData.WayBillNumber);
				AssertNull("headerData.WayBillType", headerData.WayBillType);
				AssertNotNull("headerData.Branch", headerData.Branch);
				AssertEquals("headerData.Branch.Code", branch.Code, headerData.Branch.Code);
				AssertEquals("headerData.Branch.Name", branch.Description, headerData.Branch.Name);
				AssertNotNull("headerData.TransportMode", headerData.TransportMode);
				AssertEquals("headerData.TransportMode.Code", transportMode.Code, headerData.TransportMode.Code);
				AssertEquals("headerData.TransportMode.Description", transportMode.Description, headerData.TransportMode.Description);
				if (containerMode == null)
				{
					AssertNull("headerData.CustomsContainerMode", headerData.CustomsContainerMode);
				}
				else
				{
					AssertNotNull("headerData.CustomsContainerMode", headerData.CustomsContainerMode);
					AssertEquals("headerData.CustomsContainerMode.Code", containerMode.Code, headerData.CustomsContainerMode.Code);
					AssertEquals("headerData.CustomsContainerMode.Description", containerMode.Description, headerData.CustomsContainerMode.Description);
				}

				AssertEquals("headerData.VesselName", vessel, headerData.VesselName);
				if (vesselCountryOfRegistration == null)
				{
					AssertNull("headerData.VesselCountryOfRegistration", headerData.VesselCountryOfRegistration);
				}
				else
				{
					AssertNotNull("headerData.VesselCountryOfRegistration", headerData.VesselCountryOfRegistration);
					AssertEquals("headerData.VesselCountryOfRegistration.Code", vesselCountryOfRegistration.Code, headerData.VesselCountryOfRegistration.Code);
					AssertEquals("headerData.VesselCountryOfRegistration.Name", vesselCountryOfRegistration.Description, headerData.VesselCountryOfRegistration.Name);
				}

				AssertEquals("headerData.LloydsIMO", lloyds, headerData.LloydsIMO);
				AssertEquals("headerData.VoyageFlightNo", voyage, headerData.VoyageFlightNo);
				if (portOfLoading == null)
				{
					AssertNull("headerData.PortOfLoading", headerData.PortOfLoading);
				}
				else
				{
					AssertNotNull("headerData.PortOfLoading", headerData.PortOfLoading);
					AssertEquals("headerData.PortOfLoading.Code", portOfLoading.Code, headerData.PortOfLoading.Code);
					AssertEquals("headerData.PortOfLoading.Name", portOfLoading.Description, headerData.PortOfLoading.Name);
				}

				if (portOfDischarge == null)
				{
					AssertNull("headerData.PortOfDischarge", headerData.PortOfDischarge);
				}
				else
				{
					AssertNotNull("headerData.PortOfDischarge", headerData.PortOfDischarge);
					AssertEquals("headerData.PortOfDischarge.Code", portOfDischarge.Code, headerData.PortOfDischarge.Code);
					AssertEquals("headerData.PortOfDischarge.Name", portOfDischarge.Description, headerData.PortOfDischarge.Name);
				}

				AssertNotNull("headerData.DateCollection", headerData.DateCollection);
				AssertContainDate(headerData.DateCollection, DateType.Arrival, ZBool.False, eta);
				AssertContainDate(headerData.DateCollection, DateType.Departure, ZBool.True, sailingDate);
				AssertNotNull("headerData.AddInfoCollection", headerData.AddInfoCollection);
				AssertCollectionContains(headerData.AddInfoCollection, Constants.Header.AddInfo.FTZMove, ftzMove);
				AssertCollectionContains(headerData.AddInfoCollection, Constants.Header.AddInfo.UI_NKCarrierSCAC, carrierSCAC);
				AssertCollectionContains(headerData.AddInfoCollection, Constants.Header.AddInfo.SchDArrival, portUnladingDCode);
				AssertCollectionContains(headerData.AddInfoCollection, Constants.Header.AddInfo.US_NKLocationOfGoods, fIRMS);
				AssertCollectionContains(headerData.AddInfoCollection, Constants.Header.AddInfo.InBondMode, headerType);
				AssertCollectionContains(headerData.AddInfoCollection, Constants.Header.AddInfo.SchDLoading, importLoadPortKCode);
				AssertNotNull("headerData.TransportLegCollection", headerData.TransportLegCollection);
			});
		}
	}
}
