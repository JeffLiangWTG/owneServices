using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.US.AMS.Business.Universal
{
	public class HVLVCusInBondCargeDescDataObjectReader : DataObjectReader<PackingLine, CusInBondCargoDesc>
	{
		public HVLVCusInBondCargeDescDataObjectReader(PackingLine packingLineDataObject, Shipment consignmentDataObject, CusInBondContainer container, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(packingLineDataObject, logger, factory)
		{
			this.packingLineDataObject = packingLineDataObject;
			this.consignmentDataObject = consignmentDataObject;
			this.container = container;
		}

		readonly PackingLine packingLineDataObject;
		readonly Shipment consignmentDataObject;
		readonly CusInBondContainer container;

		protected override CusInBondCargoDesc GetExistingBusinessObject() => container.Commodities.FirstOrDefault();

		protected override CusInBondCargoDesc GetNewBusinessObject()
		{
			var result = container.Commodities.AddNew();
			result.BY_GrossWeight = ZDecimal.Zero;
			result.BY_PieceCount = ZInt.Zero;
			return result;
		}

		protected override void PopulateBusinessObject(CusInBondCargoDesc targetBO)
		{
			var itemLines = packingLineDataObject.PackedItemCollection;
			if (itemLines != null)
			{
				var commodityRow = GetColumnIndexer(targetBO);
				var commercialInvoiceLine = GetCommercialInvoiceLine(itemLines.FirstOrDefault().CommercialInvoiceLineLink);

				var monetaryValue = commodityRow.GetValue(CusInBondCargoDescSchema.BY_MonetaryValue);
				var grossWeight = commodityRow.GetValue(CusInBondCargoDescSchema.BY_GrossWeight);
				var pieceCount = commodityRow.GetValue(CusInBondCargoDescSchema.BY_PieceCount);
				var description = commodityRow.GetValue(CusInBondCargoDescSchema.BY_Description);
				var marksAndNumbers = commodityRow.GetValue(CusInBondCargoDescSchema.BY_MarksAndNumbers);
				var harmonisedTariff = commodityRow.GetValue(CusInBondCargoDescSchema.BY_HarmonisedTariff);

				SetValue(commodityRow, CusInBondCargoDescSchema.BY_ParentID, container.PK);
				SetValue(commodityRow, CusInBondCargoDescSchema.BY_ParentTableCode, CusInBondContainerSchema.Constants.Prefix);
				SetValue(commodityRow, CusInBondCargoDescSchema.BY_MonetaryValue, RoundToWholeValue(itemLines.Sum(l => l.CIFValue.Value) + monetaryValue));
				SetValue(commodityRow, CusInBondCargoDescSchema.BY_GrossWeight, RoundToWholeValue(itemLines.Sum(l => l.GrossWeight.Value) + grossWeight));
				SetValue(commodityRow, CusInBondCargoDescSchema.BY_GrossWeightUnit, itemLines[0].GrossWeightUnit?.Code);
				SetValue(commodityRow, CusInBondCargoDescSchema.BY_PieceCount, (ZInt)(pieceCount + 1));
				SetValue(commodityRow, CusInBondCargoDescSchema.BY_ManifestUnitCode, new PackageTypeMapping().GetPackageType(PkgUnit.Piece));
				SetValue(commodityRow, CusInBondCargoDescSchema.BY_Description, description + GetDescription(itemLines, commercialInvoiceLine));
				SetValue(commodityRow, CusInBondCargoDescSchema.BY_RN_NKCountryOfOrigin, commercialInvoiceLine.CountryOfOrigin?.Code.Value);

				if (string.IsNullOrEmpty(marksAndNumbers))
				{
					SetValue(commodityRow, CusInBondCargoDescSchema.BY_MarksAndNumbers, packingLineDataObject.Barcode?.ToUpper());
				}
				else
				{
					SetValue(commodityRow, CusInBondCargoDescSchema.BY_MarksAndNumbers, ZString.Format("{0},{1}", marksAndNumbers, packingLineDataObject.Barcode?.ToUpper()));
				}

				if (string.IsNullOrEmpty(harmonisedTariff))
				{
					SetValue(commodityRow, CusInBondCargoDescSchema.BY_HarmonisedTariff, TariffFormatterDecider.GetByCountryCode(commercialInvoiceLine?.CountryOfOrigin?.Code.Value).DisplayFormat(commercialInvoiceLine?.HarmonisedCode ?? ZString.Empty));
				}
			}
		}

		ZString GetDescription(List<PackedItem> itemLines, CommercialInvoiceLine commercialInvoiceLine)
		{
			var description = itemLines.Select(l => l.Description ?? ZString.Empty).ToList();
			var formattedHarmonisedTariff = TariffFormatterDecider.GetByCountryCode(commercialInvoiceLine?.CountryOfOrigin?.Code.Value).DisplayFormat(commercialInvoiceLine?.HarmonisedCode ?? ZString.Empty);
			description.AddRange(Enumerable.Repeat(formattedHarmonisedTariff, itemLines.Count));

			return new ZString(string.Join(",", description.Where(d => !d.IsEmpty))).SubstringSafe(0, CusInBondCargoDescSchema.BY_Description.MaxLength);
		}

		CommercialInvoiceLine GetCommercialInvoiceLine(ZInt? commercialInvoiceLineLink)
		{
			var commercialInvoiceLineCollection = consignmentDataObject
				.CommercialInfo?
				.CommercialInvoiceCollection?
				.FirstOrDefault()?
				.CommercialInvoiceLineCollection;

			return commercialInvoiceLineCollection?.FirstOrDefault(c => c.Link == commercialInvoiceLineLink);
		}

		ZDecimal RoundToWholeValue(ZDecimal valueToRound)
		{
			var result = valueToRound;

			if (result > ZDecimal.Zero)
			{
				result = Math.Max(result.Round(0), 1m);
			}

			return result;
		}
	}
}
