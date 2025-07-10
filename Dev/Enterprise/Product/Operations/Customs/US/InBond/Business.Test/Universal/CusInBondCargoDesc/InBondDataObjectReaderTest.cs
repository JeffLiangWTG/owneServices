using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.US.InBond.Business.Universal.Testing
{
	partial class InBondDataObjectReaderTest
	{
		public void TestImportingInBondCommodityData()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var container = moveDetail.Containers.AddNew();
			var commodityData = SetupPackingLine("101010", 1);
			var reader = new CusInBondCargoDescDataObjectReader(commodityData, logger, Helper, container.PK);
			var commodityBO = reader.ReadIntoBusinessObject();
			AssertNotNull(commodityBO);
			CombineAssertions(delegate
			{
				AssertCusInBondCargoDescContents(commodityBO, "101010");
				AssertEquals("commodityBO.BY_ParentID", container.PK, commodityBO.BY_ParentID);
				AssertEquals("commodityBO.BY_ParentTableCode", container.TablePrefix, commodityBO.BY_ParentTableCode);
				AssertMultilineASCIIEquals("logger.Logs", @" 
Information - No matching CusInBondCargoDesc found, creating new CusInBondCargoDesc.
Information - Populating CusInBondCargoDesc...".Trim(), logger.Logs);
			});
		}

		public void TestImportingInBondCommodityDataWithChildCommodies()
		{
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Importer.MiscServ.OM_IMPartAttrib1Name = "VIN1";
			Importer.MiscServ.OM_IMPartAttrib2Name = "VIN2";
			Importer.MiscServ.OM_IMPartAttrib3Name = "VIN3";
			Importer.MiscServ.OM_IMUseSerialNumber = true;
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "PART1";
			var relOrg = product.RelatedOrganisations.AddOwner(Importer);
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = US.Business.ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "9101000010";
			var attr1 = pivot.Attributes1.AddNew();
			attr1.BG_AttributeValue1 = "PART1ATT1";
			var attr2 = pivot.Attributes2.AddNew();
			attr2.BG_AttributeValue1 = "PART1ATT2";
			var attr3 = pivot.Attributes3.AddNew();
			attr3.BG_AttributeValue1 = "PART1ATT3";
			var header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = Importer.MainAddress.PK;
			var bill = header.Bills.AddNew();
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var container = moveDetail.Containers.AddNew();
			var packingLine1 = SetupPackingLine(100, "NO", null, ZDecimal.Zero, "BOB THE BUILDER", ZDecimal.Zero, "", "MARKS 1", 1);
			packingLine1.SetPackedItemCollection(() => new List<PackedItem>(new[] { new PackedItem()
			{ CommercialInvoiceLineLink = 1 } }));
			var helper = new InBondDataObjectReaderHelper(Factory);
			var invoiceLine1Data = new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Link = 1,
				LineNo = 1,
				PartNo = "PART1",
				OrganizationAddressCollection = new List<OrganizationAddress>(),
				AddInfoCollection = Enterprise.Customs.DataTransfer.Universal.AddInfoCollectionCreator.CreateCollection(string.Format("{0}=ENT23*{1}=1", JobDeclaration.Schema.US_WHSEntryNumber.Substring(3), JobComInvoiceLine.Schema.US_WHSEntryLineNo.Substring(3))),
				CustomizedFieldCollection = new List<CustomizedField>(new[] { new CustomizedField()
			{ Key = "VIN2", Value = "PART1ATT2", DataType = Enterprise.UniversalDataBuss.DataObjects.DataType.String }, new CustomizedField()
			{ Key = "VIN3", Value = "PART1ATT3", DataType = Enterprise.UniversalDataBuss.DataObjects.DataType.String }, new CustomizedField()
			{ Key = "VIN1", Value = "PART1ATT1", DataType = Enterprise.UniversalDataBuss.DataObjects.DataType.String }, new CustomizedField()
			{ Key = "Serial Number", Value = "SERIALNUM", DataType = Enterprise.UniversalDataBuss.DataObjects.DataType.String } }),
				InvoiceQuantity = 10m
			};
			var writeManager = new DataWritingManager(new ActionInfo(null, header));
			invoiceLine1Data.AddOrgAddress(writeManager, Supplier, DocAddressType.SupplierDocumentaryAddress);
			var invoiceHeaderData = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
				.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() =>
					new DataObjectList<UniversalCustoms.CommercialInvoiceLine>(new[] { invoiceLine1Data })));
			var headerData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				CommercialInfo = new UniversalCustoms.CommercialInfo()
				{ CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>(new[] { invoiceHeaderData }) }
			};
			headerData.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packingLine1 })
			{ Content = CollectionContent.Complete });
			helper.SetCusInBondCargoDescCustomLabelsProvider(header);
			helper.CollectContainerPackingLineAndCommercialInvoiceLineDetails(headerData);
			var reader = new CusInBondCargoDescDataObjectReader(packingLine1, logger, helper, container.PK);
			var commodityBO = reader.ReadIntoBusinessObject();
			AssertNotNull(commodityBO);
			CombineAssertions(delegate
			{
				AssertCusInBondCargoDescContents(commodityBO, 100, "NO", "9101000010", ZDecimal.Zero, "BOB THE BUILDER", ZDecimal.Zero, "", "MARKS 1");
				AssertCusInBondCargoDescContents(commodityBO, container.PK, container.TablePrefix, Supplier.PK, "PART1", product.PK, "PART1ATT1", "PART1ATT2", "PART1ATT3", "SERIALNUM", 10m, "ENT23", (short)1);
				commodityBO.ChildCommodities.Load();
				AssertEquals(0, commodityBO.ChildCommodities.Count);
				AssertMultilineASCIIEquals("logger.Logs", @" 
Information - No matching CusInBondCargoDesc found, creating new CusInBondCargoDesc.
Information - Populating CusInBondCargoDesc...
Information - Matching 'SupplierDocumentaryAddress':- Matched to 'INTHEMSYD' by code, address 'THEMOMENT' (only address).".Trim(), logger.Logs);
			});
			packingLine1.HarmonisedCode = "808023023";
			reader = new CusInBondCargoDescDataObjectReader(packingLine1, logger, helper, container.PK);
			commodityBO = reader.ReadIntoBusinessObject();
			AssertNotNull(commodityBO);
			CombineAssertions(delegate
			{
				AssertCusInBondCargoDescContents(commodityBO, 100, "NO", "808023023", ZDecimal.Zero, "BOB THE BUILDER", ZDecimal.Zero, "", "MARKS 1");
				AssertCusInBondCargoDescContents(commodityBO, container.PK, container.TablePrefix, Supplier.PK, "PART1", product.PK, "PART1ATT1", "PART1ATT2", "PART1ATT3", "SERIALNUM", 10m, "ENT23", (short)1);
				commodityBO.ChildCommodities.Load();
				AssertEquals(0, commodityBO.ChildCommodities.Count);
			});
			container.Commodities.DeleteAll();
			var invoiceLine2Data = new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Link = 2,
				LineNo = 2,
				PartNo = "PART1",
				OrganizationAddressCollection = new List<OrganizationAddress>(),
				AddInfoCollection = Enterprise.Customs.DataTransfer.Universal.AddInfoCollectionCreator.CreateCollection(string.Format("{0}=ENT48*{1}=2", JobDeclaration.Schema.US_WHSEntryNumber.Substring(3), JobComInvoiceLine.Schema.US_WHSEntryLineNo.Substring(3))),
				CustomizedFieldCollection = new List<CustomizedField>(new[] { new CustomizedField()
			{ Key = "VIN2", Value = "PART1ATT2", DataType = Enterprise.UniversalDataBuss.DataObjects.DataType.String }, new CustomizedField()
			{ Key = "VIN3", Value = "PART1ATT3", DataType = Enterprise.UniversalDataBuss.DataObjects.DataType.String }, new CustomizedField()
			{ Key = "VIN1", Value = "PART1ATT1", DataType = Enterprise.UniversalDataBuss.DataObjects.DataType.String }, new CustomizedField()
			{ Key = "Serial Number", Value = "SERIALNUM", DataType = Enterprise.UniversalDataBuss.DataObjects.DataType.String } }),
				InvoiceQuantity = 20m
			};
			invoiceLine2Data.AddOrgAddress(writeManager, Supplier, DocAddressType.SupplierDocumentaryAddress);
			var packingLine2 = SetupPackingLine(20, ShippingOrPackingingUnitList.Codes.Bag, "8010131221", 1600m, "YUMMY GOODS 2", 120m, Core.Constants.Weight.Kilograms, "MARKS LOOK FUNNY 2", 1);
			packingLine2.SetPackedItemCollection(() => new List<PackedItem>(new[] { new PackedItem()
			{ CommercialInvoiceLineLink = 2 } }));
			var invoiceLine3Data = new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Link = 3,
				LineNo = 3,
				PartNo = "PART1",
				OrganizationAddressCollection = new List<OrganizationAddress>(),
				AddInfoCollection = Enterprise.Customs.DataTransfer.Universal.AddInfoCollectionCreator.CreateCollection(string.Format("{0}=ENT48*{1}=3", JobDeclaration.Schema.US_WHSEntryNumber.Substring(3), JobComInvoiceLine.Schema.US_WHSEntryLineNo.Substring(3))),
				CustomizedFieldCollection = new List<CustomizedField>(new[] { new CustomizedField()
			{ Key = "VIN2", Value = "PART1ATT2", DataType = Enterprise.UniversalDataBuss.DataObjects.DataType.String }, new CustomizedField()
			{ Key = "VIN3", Value = "PART1ATT3", DataType = Enterprise.UniversalDataBuss.DataObjects.DataType.String }, new CustomizedField()
			{ Key = "VIN1", Value = "PART1ATT1", DataType = Enterprise.UniversalDataBuss.DataObjects.DataType.String }, new CustomizedField()
			{ Key = "Serial Number", Value = "SERIALNUM", DataType = Enterprise.UniversalDataBuss.DataObjects.DataType.String } }),
				InvoiceQuantity = 30m
			};
			invoiceLine3Data.AddOrgAddress(writeManager, Supplier, DocAddressType.SupplierDocumentaryAddress);
			var packingLine3 = SetupPackingLine(30, ShippingOrPackingingUnitList.Codes.Bag, "9101000010", 1700m, "YUMMY GOODS 3", 130m, Core.Constants.Weight.Kilograms, "MARKS LOOK FUNNY 3", 1);
			packingLine3.SetPackedItemCollection(() => new List<PackedItem>(new[] { new PackedItem()
			{ CommercialInvoiceLineLink = 3 } }));
			var invoiceLine4Data = new UniversalCustoms.CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Link = 4,
				LineNo = 4,
				PartNo = "PART1",
				OrganizationAddressCollection = new List<OrganizationAddress>(),
				AddInfoCollection = Enterprise.Customs.DataTransfer.Universal.AddInfoCollectionCreator.CreateCollection(string.Format("{0}=ENT48*{1}=4", JobDeclaration.Schema.US_WHSEntryNumber.Substring(3), JobComInvoiceLine.Schema.US_WHSEntryLineNo.Substring(3))),
				CustomizedFieldCollection = new List<CustomizedField>(new[] { new CustomizedField()
			{ Key = "VIN2", Value = "PART1ATT2", DataType = Enterprise.UniversalDataBuss.DataObjects.DataType.String }, new CustomizedField()
			{ Key = "VIN3", Value = "PART1ATT3", DataType = Enterprise.UniversalDataBuss.DataObjects.DataType.String }, new CustomizedField()
			{ Key = "VIN1", Value = "PART1ATT1", DataType = Enterprise.UniversalDataBuss.DataObjects.DataType.String }, new CustomizedField()
			{ Key = "Serial Number", Value = "SERIALNUM", DataType = Enterprise.UniversalDataBuss.DataObjects.DataType.String } }),
				InvoiceQuantity = 40m
			};
			invoiceLine4Data.AddOrgAddress(writeManager, Supplier, DocAddressType.SupplierDocumentaryAddress);
			var packingLine4 = SetupPackingLine(40, ShippingOrPackingingUnitList.Codes.Bag, "9101000010", 1800m, "YUMMY GOODS 4", 140m, Core.Constants.Weight.Kilograms, "MARKS LOOK FUNNY 4", 1);
			packingLine4.SetPackedItemCollection(() => new List<PackedItem>(new[] { new PackedItem()
			{ CommercialInvoiceLineLink = 4 } }));
			packingLine3.SetPackingLineCollection(() => new List<PackingLine>(new[] { packingLine4 }));
			packingLine1.SetPackingLineCollection(() => new List<PackingLine>(new[] { packingLine2, packingLine3 }));
			invoiceHeaderData.CommercialInvoiceLineCollection.Add(invoiceLine2Data);
			invoiceHeaderData.CommercialInvoiceLineCollection.Add(invoiceLine3Data);
			invoiceHeaderData.CommercialInvoiceLineCollection.Add(invoiceLine4Data);
			helper.CollectContainerPackingLineAndCommercialInvoiceLineDetails(headerData);
			reader = new CusInBondCargoDescDataObjectReader(packingLine1, logger, helper, container.PK);
			commodityBO = reader.ReadIntoBusinessObject();
			AssertNotNull(commodityBO);
			CombineAssertions(delegate
			{
				AssertCusInBondCargoDescContents(commodityBO, 100, "NO", "", ZDecimal.Zero, "BOB THE BUILDER", ZDecimal.Zero, "", "MARKS 1");
				AssertCusInBondCargoDescContents(commodityBO, container.PK, container.TablePrefix, Supplier.PK, "PART1", product.PK, "PART1ATT1", "PART1ATT2", "PART1ATT3", "SERIALNUM", 10m, "ENT23", (short)1);
				commodityBO.ChildCommodities.Load();
				AssertEquals(2, commodityBO.ChildCommodities.Count);
				var commodityChild1BO = commodityBO.ChildCommodities[0];
				var commodityChild2BO = commodityBO.ChildCommodities[1];
				if (commodityChild2BO.BY_HarmonisedTariff == "8010131221")
				{
					commodityChild1BO = commodityBO.ChildCommodities[1];
					commodityChild2BO = commodityBO.ChildCommodities[0];
				}

				AssertCusInBondCargoDescContents(commodityChild1BO, 0, "", "8010131221", 1600m, "", 120m, Core.Constants.Weight.Kilograms, "");
				AssertCusInBondCargoDescContents(commodityChild1BO, commodityBO.PK, commodityBO.TablePrefix, ZGuid.Empty, "", ZGuid.Empty, "", "", "", "", 0m, "", (short)0);
				commodityChild1BO.ChildCommodities.Load();
				AssertEquals(0, commodityChild1BO.ChildCommodities.Count);
				AssertCusInBondCargoDescContents(commodityChild2BO, 0, "", "9101000010", 1700m, "", 130m, Core.Constants.Weight.Kilograms, "");
				AssertCusInBondCargoDescContents(commodityChild2BO, commodityBO.PK, commodityBO.TablePrefix, ZGuid.Empty, "", ZGuid.Empty, "", "", "", "", 0m, "", (short)0);
				commodityChild2BO.ChildCommodities.Load();
				AssertEquals(0, commodityChild2BO.ChildCommodities.Count);
			});
			container.Commodities.DeleteAll();
			invoiceLine1Data.PartNo = ZString.Empty;
			reader = new CusInBondCargoDescDataObjectReader(packingLine1, logger, helper, container.PK);
			commodityBO = reader.ReadIntoBusinessObject();
			AssertNotNull(commodityBO);
			CombineAssertions(delegate
			{
				AssertCusInBondCargoDescContents(commodityBO, 100, "NO", "", ZDecimal.Zero, "BOB THE BUILDER", ZDecimal.Zero, "", "MARKS 1");
				AssertCusInBondCargoDescContents(commodityBO, container.PK, container.TablePrefix, ZGuid.Empty, "", ZGuid.Empty, "", "", "", "", 0m, "ENT23", (short)1);
				commodityBO.ChildCommodities.Load();
				AssertEquals(2, commodityBO.ChildCommodities.Count);
				var commodityChild1BO = commodityBO.ChildCommodities[0];
				var commodityChild2BO = commodityBO.ChildCommodities[1];
				if (commodityChild2BO.BY_WarehouseEntryLineNo == (short)2)
				{
					commodityChild1BO = commodityBO.ChildCommodities[1];
					commodityChild2BO = commodityBO.ChildCommodities[0];
				}

				AssertCusInBondCargoDescContents(commodityChild1BO, 0, "", "8010131221", 1600m, "", 120m, Core.Constants.Weight.Kilograms, "");
				AssertCusInBondCargoDescContents(commodityChild1BO, commodityBO.PK, commodityBO.TablePrefix, Supplier.PK, "PART1", product.PK, "PART1ATT1", "PART1ATT2", "PART1ATT3", "SERIALNUM", 20m, "ENT48", (short)2);
				commodityChild1BO.ChildCommodities.Load();
				AssertEquals(0, commodityChild1BO.ChildCommodities.Count);
				AssertCusInBondCargoDescContents(commodityChild2BO, 0, "", "", 0m, "", 0m, "", "");
				AssertCusInBondCargoDescContents(commodityChild2BO, commodityBO.PK, commodityBO.TablePrefix, Supplier.PK, "PART1", product.PK, "PART1ATT1", "PART1ATT2", "PART1ATT3", "SERIALNUM", 30m, "ENT48", (short)3);
				commodityChild2BO.ChildCommodities.Load();
				AssertEquals(1, commodityChild2BO.ChildCommodities.Count);
				var commodityChild2ChildBO = commodityChild2BO.ChildCommodities[0];
				AssertCusInBondCargoDescContents(commodityChild2ChildBO, 0, "", "9101000010", 1800m, "", 140m, Core.Constants.Weight.Kilograms, "");
				AssertCusInBondCargoDescContents(commodityChild2ChildBO, commodityChild2BO.PK, commodityChild2BO.TablePrefix, ZGuid.Empty, "", ZGuid.Empty, "", "", "", "", 0m, "", (short)0);
				commodityChild2ChildBO.ChildCommodities.Load();
				AssertEquals(0, commodityChild2ChildBO.ChildCommodities.Count);
			});
		}

		PackingLine SetupPackingLine(ZString? harmonisedTariff, ZInt containerLink)
		{
			return SetupPackingLine(10, ShippingOrPackingingUnitList.Codes.Bag, harmonisedTariff, 1500m, "YUMMY GOODS", 110m, Core.Constants.Weight.Kilograms, "MARKS LOOK FUNNY", containerLink);
		}

		void AssertCusInBondCargoDescContents(CusInBondCargoDesc commodityBO, ZGuid parentPK, ZString tablePrefix, ZGuid supplierPK, ZString partNumber, ZGuid partPK, ZString attrib1, ZString attrib2, ZString attrib3, ZString serialNumber, ZDecimal invoiceQuantity, ZString warehouseEntryNumber, ZShort warehouseEntryLineNo)
		{
			AssertEquals("commodityBO.BY_ParentID", parentPK, commodityBO.BY_ParentID);
			AssertEquals("commodityBO.BY_ParentTableCode", tablePrefix, commodityBO.BY_ParentTableCode);
			AssertEquals("commodityBO.BY_OH_Supplier", supplierPK, commodityBO.BY_OH_Supplier);
			AssertEquals("commodityBO.BY_PartNumber", partNumber, commodityBO.BY_PartNumber);
			AssertEquals("commodityBO.BY_OP_Part", partPK, commodityBO.BY_OP_Part);
			AssertEquals("commodityBO.BY_PartAttrib1", attrib1, commodityBO.BY_PartAttrib1);
			AssertEquals("commodityBO.BY_PartAttrib2", attrib2, commodityBO.BY_PartAttrib2);
			AssertEquals("commodityBO.BY_PartAttrib3", attrib3, commodityBO.BY_PartAttrib3);
			AssertEquals("commodityBO.BY_SerialNumber", serialNumber, commodityBO.BY_SerialNumber);
			AssertEquals("commodityBO.BY_InvoiceQuantity", invoiceQuantity, commodityBO.BY_InvoiceQuantity);
			AssertEquals("commodityBO.BY_WarehouseEntryNumber", warehouseEntryNumber, commodityBO.BY_WarehouseEntryNumber);
			AssertEquals("commodityBO.BY_WarehouseEntryLineNo", warehouseEntryLineNo, commodityBO.BY_WarehouseEntryLineNo);
		}

		void AssertCusInBondCargoDescContents(CusInBondCargoDesc commodityBO, ZString harmonisedTariff)
		{
			AssertCusInBondCargoDescContents(commodityBO, 10, ShippingOrPackingingUnitList.Codes.Bag, harmonisedTariff, 1500m, "YUMMY GOODS", 110m, Core.Constants.Weight.Kilograms, "MARKS LOOK FUNNY");
		}

		void AssertCusInBondCargoDescContents(CusInBondCargoDesc commodityBO, ZInt pieceCount, ZString manifestUnitCode, ZString harmonisedTariff, ZDecimal monetaryValue, ZString description, ZDecimal weight, ZString weightUnit, ZString marksAndNumbers)
		{
			AssertEquals("commodityBO.BY_PieceCount", pieceCount, commodityBO.BY_PieceCount);
			AssertEquals("commodityBO.BY_ManifestUnitCode", manifestUnitCode, commodityBO.BY_ManifestUnitCode);
			AssertEquals("commodityBO.BY_HarmonisedTariff", harmonisedTariff, commodityBO.BY_HarmonisedTariff);
			AssertEquals("commodityBO.BY_MonetaryValue", monetaryValue, commodityBO.BY_MonetaryValue);
			AssertEquals("commodityBO.BY_Description", description, commodityBO.BY_Description);
			AssertEquals("commodityBO.BY_GrossWeight", weight, commodityBO.BY_GrossWeight);
			AssertEquals("commodityBO.BY_GrossWeightUnit", weightUnit, commodityBO.BY_GrossWeightUnit);
			AssertEquals("commodityBO.BY_MarksAndNumbers", marksAndNumbers, commodityBO.BY_MarksAndNumbers);
		}
	}
}
