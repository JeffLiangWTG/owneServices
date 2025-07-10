using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.Data.Universal
{
	public class ETailCusHAWBDataObjectReader : CusHAWBDataObjectReader
	{
		public ETailCusHAWBDataObjectReader(Shipment shipmentDataObject, Shipment mawbDataObject, IXmlImportLogger logger, DataTransfer.Universal.AirManifest.AirManifestDataObjectReaderHelper helper, CusMAWB mawb, CusHAWB masterHouse, ZGuid shipmentPK, bool singleHAWBCheck = false)
			: base(shipmentDataObject, mawbDataObject, logger, helper, mawb, masterHouse, true, singleHAWBCheck)
		{
			this.shipmentPK = shipmentPK;
		}

		protected override bool CheckUpdateHAWBDataIsAllowed(CusHAWB hawb)
		{
			return !IsMessagingActive(hawb);
		}

		protected override bool ShouldCheckMessageBeforeUpdate => false;

		readonly ZGuid shipmentPK;

		protected override void FillFirstPackingLine(CusHAWB hawb)
		{
			var firstPackingLine = dataObject.PackingLineCollection?.FirstOrDefault();
			var firstPackedItem = firstPackingLine?.PackedItemCollection?.FirstOrDefault();
			var commercialInvoiceLine = firstPackedItem.FindMatchingCommercialInvoiceLine(dataObject);
			FillHouseBillFieldsWithFallback(hawb, commercialInvoiceLine, firstPackingLine, forHAWB: true);
		}

		protected override void FillSubSequentPackingLines(CusHAWB hawb)
		{
			hawb.CusHAWBItemsCollection.DeleteAll();
			var flattened = dataObject.PackingLineCollection.SelectMany(
				p =>
				{
					if (p.PackedItemCollection == null || p.PackedItemCollection.Count == 0)
					{
						return new List<PackedItem>() { null };
					}
					else
					{
						return p.PackedItemCollection;
					}
				},
				(packingLine, packedItem) => new
				{
					PackingLine = packingLine,
					CommercialInvoiceLine = packedItem.FindMatchingCommercialInvoiceLine(dataObject)
				});
			flattened = flattened.Skip(1);
			foreach (var tuple in flattened)
			{
				FillHouseBillFieldsWithFallback(hawb, tuple.CommercialInvoiceLine, tuple.PackingLine);
			}
		}

		protected override void FillCountrySpecificDetails(CusHAWB hawb)
		{
			base.FillCountrySpecificDetails(hawb);

			var hawbRow = GetColumnIndexer(hawb);
			SetValue(hawbRow, CusHAWBSchema.CS_MasterHouseBill, mawbDataObject.WayBillNumber);
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void FillHouseBillFieldsWithFallback(CusHAWB hawb, CommercialInvoiceLine commercialInvoiceLine, PackingLine packingLine, bool forHAWB = false)
		{
			var columnIndex = forHAWB ? GetColumnIndexer(hawb) : GetColumnIndexer(hawb.CusHAWBItemsCollection.AddNew());
			var customsSupportingInfo = commercialInvoiceLine?.CustomsSupportingInformationCollection?.FirstOrDefault();

			FillFields(
				columnIndex,
				GetFallbackValue(commercialInvoiceLine?.Description, packingLine.GetCleanSingleLineGoodsDescription(), dataObject.GoodsDescription),
				GetFallbackValue(customsSupportingInfo?.Country, commercialInvoiceLine?.CountryOfOrigin, dataObject.OrganizationAddressCollection?.FirstOrDefault(org => org.AddressType.Value == nameof(DocAddressType.ConsignorDocumentaryAddress))?.Country),
				GetFallbackValue(commercialInvoiceLine?.CustomsValue, dataObject.GoodsValue),
				dataObject.GoodsValueCurrency,
				GetPackQty(commercialInvoiceLine?.CustomsQuantity),
				packingLine?.PackType,
				GetFallbackValue(commercialInvoiceLine?.Weight, commercialInvoiceLine?.NetWeight, packingLine?.Weight),
				GetFallbackValue(commercialInvoiceLine?.WeightUnit, packingLine?.WeightUnit),
				mawb.EntryType == LowValueEntryTypeList.Codes.Import ? commercialInvoiceLine?.HarmonisedCode : customsSupportingInfo?.Tariff,
				forHAWB);

			static TValue GetFallbackValue<TValue>(params TValue[] values)
			{
				return values.FirstOrDefault(v =>
					v is ZString stringValue && !string.IsNullOrEmpty(stringValue)
					|| v is ICodeDataObject codeDataObject && !string.IsNullOrEmpty(codeDataObject?.Code)
					|| v is ZDecimal decimalValue && decimalValue > ZDecimal.Zero);
			}
		}

		ZInt GetPackQty(ZDecimal? customsQuantity)
		{
			var result = dataObject.PackingLineCollection.Count;
			if (customsQuantity.HasValue)
			{
				if (customsQuantity > int.MaxValue)
				{
					throw new DataObjectReadFailureException(Res.GetString("0DA9C015-A82C-4E67-9FC6-030E54016C03", "Value '{0}' of 'Customs Quantity' is too large. It cannot be greater than {1}", customsQuantity, int.MaxValue));
				}
				else
				{
					result = customsQuantity.Value.ToZInt();
				}
			}

			return result;
		}

		void FillFields(IColumnIndexer columnIndex, ZString? goodsDesc, Country countryOfOringin, ZDecimal? goodsValue, Currency goodsValueCurrency, ZInt packQty, PackageType packageType, ZDecimal? weight, UnitOfWeight weightUQ, ZString? harmonisedCode, bool forHAWB = false)
		{
			if (forHAWB)
			{
				SetValue(columnIndex, CusHAWBSchema.CS_GoodsDescription, goodsDesc);
				SetValue(columnIndex, CusHAWBSchema.CS_RN_NKGoodsOrigin, countryOfOringin);
				SetValue(columnIndex, CusHAWBSchema.CS_GoodsValue, goodsValue);
				SetValue(columnIndex, CusHAWBSchema.CS_RX_NKGoodsCurrency, goodsValueCurrency);
				SetValue(columnIndex, CusHAWBSchema.CS_PiecesManifested, packQty);
				SetValue(columnIndex, CusHAWBSchema.CS_PackType, PackageTypeConverter.GetCustomsPackageType(packageType?.Code ?? ZString.Empty));
				SetValue(columnIndex, CusHAWBSchema.CS_Weight, weight);
				SetValue(columnIndex, CusHAWBSchema.CS_WeightUQ, weightUQ);
				SetValue(columnIndex, CusHAWBSchema.CS_HarmonisedTariffNums, harmonisedCode);
				SetValue(columnIndex, CusHAWBSchema.CS_JS, shipmentPK);
			}
			else
			{
				SetValue(columnIndex, CusHAWBItemsSchema.CHI_GoodsDescription, goodsDesc);
				SetValue(columnIndex, CusHAWBItemsSchema.CHI_RN_NKGoodsOrigin, countryOfOringin);
				SetValue(columnIndex, CusHAWBItemsSchema.CHI_GoodsValue, goodsValue);
				SetValue(columnIndex, CusHAWBItemsSchema.CHI_RX_NKGoodsCurrency, goodsValueCurrency);
				SetValue(columnIndex, CusHAWBItemsSchema.CHI_PieceCount, packQty);
				SetValue(columnIndex, CusHAWBItemsSchema.CHI_PackType, PackageTypeConverter.GetCustomsPackageType(packageType?.Code ?? ZString.Empty));
				SetValue(columnIndex, CusHAWBItemsSchema.CHI_Weight, weight);
				SetValue(columnIndex, CusHAWBItemsSchema.CHI_WeightUQ, weightUQ);
				SetValue(columnIndex, CusHAWBItemsSchema.CHI_HarmonisedTariffNums, harmonisedCode);
			}
		}
	}
}
