using System;
using System.Linq;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using AMSBusiness = Enterprise.Customs.US.AMS.Business;
using CodeDescriptionPairForTesting = Enterprise.Customs.DataTransfer.Universal.Testing.CodeDescriptionPairForTesting;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.US.InBond.Business.Universal.Testing
{
	partial class InBondDataObjectWriterTest
	{
		void AssertInBondCommodityContents(PackingLine commodityData, ZInt? containerLink, ZString? harmonisedTariff)
		{
			AssertInBondCommodityContents(commodityData, containerLink, harmonisedTariff, 10, CodeDescriptionPairForTesting.New(AMSBusiness.ManifestUnitList.Codes.Bag, AMSBusiness.ManifestUnitList.Descriptions.Bag), "YUMMY GOODS", 1500m, 110m, CodeDescriptionPairForTesting.New(Core.Constants.Weight.Kilograms, "Kilograms"), "MARKS LOOK FUNNY");
		}

		void AssertInBondCommodityContents(PackingLine commodityData, ZInt? containerLink, ZString? harmonisedTariff, ZLong? pieceCount, ICodeDescription manifestUnit, ZString? description, ZDecimal? monetaryValue, ZDecimal? weight, ICodeDescription weightUnit, ZString? marksAndNumbers)
		{
			AssertNotNull("Precondition: commodityData", commodityData);
			CombineAssertions(delegate
			{
				AssertEquals("commodityData.ContainerLink", containerLink, commodityData.ContainerLink);
				AssertEquals("commodityData.HarmonisedCode", harmonisedTariff, commodityData.HarmonisedCode);
				AssertEquals("commodityData.PackQty", pieceCount, commodityData.PackQty);
				AssertNotNull("commodityData.PackType", commodityData.PackType);
				AssertEquals("commodityData.PackType.Code", manifestUnit.Code, commodityData.PackType.Code);
				AssertEquals("commodityData.PackType.Description", manifestUnit.Description, commodityData.PackType.Description);
				AssertEquals("commodityData.GoodsDescription", description, commodityData.GoodsDescription);
				AssertEquals("commodityData.LinePrice", monetaryValue, commodityData.LinePrice);
				AssertEquals("commodityData.Weight", weight, commodityData.Weight);
				AssertNotNull("commodityData.WeightUnit", commodityData.WeightUnit);
				AssertEquals("commodityData.WeightUnit.Code", weightUnit.Code, commodityData.WeightUnit.Code);
				AssertEquals("commodityData.WeightUnit.Description", weightUnit.Description, commodityData.WeightUnit.Description);
				AssertEquals("commodityData.MarksAndNos", marksAndNumbers, commodityData.MarksAndNos);
			});
		}

		CusInBondCargoDesc SetupCusInBondCargoDesc(CusInBondCargoDesc commodity, ZString harmonisedTariff)
		{
			return SetupCusInBondCargoDesc(commodity, harmonisedTariff, 10, AMSBusiness.ManifestUnitList.Codes.Bag, "YUMMY GOODS", 1500m, 110m, Core.Constants.Weight.Kilograms, "MARKS LOOK FUNNY");
		}

		CusInBondCargoDesc SetupCusInBondCargoDesc(CusInBondCargoDesc commodity, ZString harmonisedTariff, ZInt pieceCount, ZString manifestUnit, ZString description, ZDecimal monetaryValue, ZDecimal weight, ZString weightUnit, ZString marksAndNumbers)
		{
			commodity.BY_HarmonisedTariff = harmonisedTariff;
			commodity.BY_PieceCount = pieceCount;
			commodity.BY_ManifestUnitCode = manifestUnit;
			commodity.BY_Description = description;
			commodity.BY_MonetaryValue = monetaryValue;
			commodity.BY_GrossWeight = weight;
			commodity.BY_GrossWeightUnit = weightUnit;
			commodity.BY_MarksAndNumbers = marksAndNumbers;
			return commodity;
		}

		CusInBondCargoDesc SetupCusInBondCargoDescWarehouseData(CusInBondCargoDesc commodity, ZString warehouseEntryNumber, ZShort warehouseEntryLineNo)
		{
			return SetupCusInBondCargoDescWarehouseData(commodity, Supplier.PK, Part.OP_PartNum, warehouseEntryNumber, warehouseEntryLineNo, 100m);
		}

		CusInBondCargoDesc SetupCusInBondCargoDescWarehouseData2(CusInBondCargoDesc commodity, ZString warehouseEntryNumber, ZShort warehouseEntryLineNo)
		{
			return SetupCusInBondCargoDescWarehouseData(commodity, Supplier2.PK, Part2.OP_PartNum, warehouseEntryNumber, warehouseEntryLineNo, 200m);
		}

		CusInBondCargoDesc SetupCusInBondCargoDescWarehouseData(CusInBondCargoDesc commodity, ZGuid supplierPK, ZString partNumber, ZString warehouseEntryNumber, ZShort warehouseEntryLineNo, ZDecimal invoiceQuanityt)
		{
			commodity.BY_OH_Supplier = supplierPK;
			commodity.BY_PartNumber = partNumber;
			commodity.BY_WarehouseEntryNumber = warehouseEntryNumber;
			commodity.BY_WarehouseEntryLineNo = warehouseEntryLineNo;
			commodity.BY_InvoiceQuantity = invoiceQuanityt;
			return commodity;
		}

		void AssertCommercialInvoiceLine(UniversalCustoms.CommercialInvoiceLine commercialInvoiceLine, ZInt? link, Action<string, OrganizationAddress, string> assertSupplier, ZString? partNo, KeyValuePair[] attribs, ZString? warehouseEntryNumber, ZShort? warehouseEntryLineNo, ZDecimal? invoiceQuantity, ZDecimal? bondedWarehouseQuantity)
		{
			AssertEquals("commercialInvoiceLine.Link", link, commercialInvoiceLine.Link);
			if (assertSupplier == null)
			{
				AssertEquals("commercialInvoiceLine.OrganizationAddressCollection.Count", 0, commercialInvoiceLine.OrganizationAddressCollection.Count);
			}
			else
			{
				AssertEquals("commercialInvoiceLine.OrganizationAddressCollection.Count", 1, commercialInvoiceLine.OrganizationAddressCollection.Count);
				assertSupplier("Supplier", commercialInvoiceLine.OrganizationAddressCollection[0], nameof(DocAddressType.SupplierDocumentaryAddress));
			}

			AssertEquals("commercialInvoiceLine.PartNo", partNo, commercialInvoiceLine.PartNo);
			if (attribs == null)
			{
				AssertNull("commercialInvoiceLine.CustomizedFieldCollection", commercialInvoiceLine.CustomizedFieldCollection);
			}
			else
			{
				AssertEquals("commercialInvoiceLine.CustomizedFieldCollection.Count", attribs.Length, commercialInvoiceLine.CustomizedFieldCollection.Count);
				foreach (var attrib in attribs)
				{
					AssertNotNull(string.Format("commercialInvoiceLine.CustomizedFieldCollection(key='{0}', value='{1}')", attrib.Key, attrib.Value), commercialInvoiceLine.CustomizedFieldCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == attrib.Key && x.Value.GetValueOrDefault() == attrib.Value));
				}
			}

			AssertNotNull("commercialInvoiceLine.AddInfoCollection", commercialInvoiceLine.AddInfoCollection);
			AssertEquals("commercialInvoiceLine.AddInfoCollection.Key=WHSEntryLineNo", warehouseEntryLineNo, commercialInvoiceLine.AddInfoCollection.GetZShortValue(JobComInvoiceLine.Schema.US_WHSEntryLineNo.Substring(3)));
			AssertEquals("commercialInvoiceLine.AddInfoCollection.Key=WHSEntryNumber", warehouseEntryNumber, commercialInvoiceLine.AddInfoCollection.GetZStringValue(JobDeclaration.Schema.US_WHSEntryNumber.Substring(3)));
			AssertEquals("commercialInvoiceLine.InvoiceQuantity", invoiceQuantity, commercialInvoiceLine.InvoiceQuantity);
			AssertEquals("commercialInvoiceLine.BondedWarehouseQuantity", bondedWarehouseQuantity, commercialInvoiceLine.BondedWarehouseQuantity);
		}

		void AssertCommercialInvoiceLineLink(PackingLine packingLine, ZInt? link)
		{
			AssertEquals(1, packingLine.PackedItemCollection.Count);
			AssertEquals(link, packingLine.PackedItemCollection[0].CommercialInvoiceLineLink);
		}
	}
}
