using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.Data.Universal
{
	public class ETailCusSCAPackingLineDataObjectReader : CusSCAPivotDataObjectReader
	{
		public ETailCusSCAPackingLineDataObjectReader(ZInt lineNo, IColumnIndexer houseBill, HVLVShipmentDataObjectWrapper hvlvConsolidatorShipmentWrapper, Shipment parentShipment, PackingLine data, PackedItem packedItemLine, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(lineNo, houseBill, hvlvConsolidatorShipmentWrapper, data, logger, factory)
		{
			shipmentDataObject = parentShipment;
			packingLine = data;
			packedItem = packedItemLine;
			if (packedItem != null)
			{
				commercialInvoiceLine = packedItem.FindMatchingCommercialInvoiceLine(shipmentDataObject);
			}
		}

		bool HasCommercialInvoiceLine => commercialInvoiceLine != null;

		readonly Shipment shipmentDataObject;
		readonly PackingLine packingLine;
		readonly PackedItem packedItem;
		readonly CommercialInvoiceLine commercialInvoiceLine;

		protected override void PopulateCountrySpecificData(CusSCAPackingLine targetBO)
		{
			base.PopulateCountrySpecificData(targetBO);
			var packageBO = GetColumnIndexer(targetBO);
			SetValue(packageBO, CusSCAPivotSchema.CV_GoodsDescription, GetGoodsDescription());
			SetValue(packageBO, CusSCAPivotSchema.CV_HarmonisedTariffNums, GetHarmonisedTariffNums(targetBO));
			SetValue(packageBO, CusSCAPivotSchema.CV_GoodsValue, GetGoodsValue());
			SetValue(packageBO, CusSCAPivotSchema.CV_RX_NKGoodsCurrency, GetGoodsCurrency());

			SetValue(packageBO, CusSCAPivotSchema.CV_PackageType, GetPackType());
			SetValue(packageBO, CusSCAPivotSchema.CV_Weight, GetWeight());
			SetValue(packageBO, CusSCAPivotSchema.CV_PackageCount, GetPackageCount());
			SetValue(packageBO, CusSCAPivotSchema.CV_CN, GetContainerNumber(targetBO));
		}

		ZString? GetGoodsDescription() => GetFallbackValue(
			packedItem?.Description,
			packingLine?.GetCleanSingleLineGoodsDescription(),
			shipmentDataObject?.GoodsDescription)?.Left(CusSCAPackingLine.Schema.CV_GoodsDescriptionMaxLength);

		ZString? GetHarmonisedTariffNums(CusSCAPackingLine targetBO) => HasCommercialInvoiceLine ?
			targetBO.OceanBill.IsImport ?
				commercialInvoiceLine?.HarmonisedCode :
				commercialInvoiceLine?.CustomsSupportingInformationCollection
					?.Select(info => info.Tariff)
					.FirstOrDefault(tariff => !string.IsNullOrEmpty(tariff)) :
			null;

		ZDecimal? GetGoodsValue() => HasCommercialInvoiceLine ?
			commercialInvoiceLine?.CustomsValue :
			shipmentDataObject.GoodsValue;

		ZString? GetGoodsCurrency() => shipmentDataObject?.GoodsValueCurrency?.Code;

		ZString? GetPackType() => ConvertCustomsPackageType(packingLine.PackType.Code ?? ZString.Empty);

		ZDecimal? GetWeight() => HasCommercialInvoiceLine ?
			GetFallbackValue(
				commercialInvoiceLine?.Weight,
				commercialInvoiceLine?.NetWeight) :
			packingLine?.ManifestedWeight;

		ZInt? GetPackageCount() => GetFallbackValue(
			commercialInvoiceLine?.CustomsQuantity?.ToZInt(),
			1);

		ZGuid? GetContainerNumber(CusSCAPackingLine targetBO) => targetBO?.OceanBill?.Containers
			?.Cast<CusSCAContainer>()
			.FirstOrDefault(container => container.CN_ContainerNumber.Equals(packingLine?.ContainerNumber))
			?.PK;

		[SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification = "Casts here are not a perfomance concern and there's no need to cache")]
		TValue GetFallbackValue<TValue>(params TValue[] values)
		{
			return values.FirstOrDefault(v =>
				v is ZString stringValue && !string.IsNullOrEmpty(stringValue)
				|| v is ICodeDataObject codeDataObject && !string.IsNullOrEmpty(codeDataObject?.Code)
				|| v is ZDecimal decimalValue && decimalValue > ZDecimal.Zero
				|| v is ZInt intValue && intValue > ZInt.Zero);
		}

		protected override bool RequireContainer => false;
	}
}
