
namespace Enterprise.eManifest.Business
{
	public class FlattenedSupplierBookingLine : AutoFlattenedSupplierBookingLine
	{
		protected override int ConsigneeReference_MaxLength { get { return AutoSupplierBookingLine.Schema.DL_ConsigneeReferenceMaxLength; } }

		protected override int GoodsValueCurrency_MaxLength { get { return AutoSupplierBookingLine.Schema.DL_RX_NKGoodsValueCurrencyMaxLength; } }

		protected override int GrossWeightUQ_MaxLength { get { return AutoSupplierBookingLine.Schema.DL_GrossWeightUQMaxLength; } }

		protected override int CubicUQ_MaxLength { get { return AutoSupplierBookingLine.Schema.DL_CubicUQMaxLength; } }

		protected override int GoodsDescription_MaxLength { get { return AutoSupplierBookingLine.Schema.DL_GoodsDescriptionMaxLength; } }

		protected override int OrderTrackingNumber_MaxLength { get { return AutoSupplierBookingLine.Schema.DL_OrderTrackingNumberMaxLength; } }

		protected override int ServiceLevel_MaxLength { get { return AutoSupplierBookingLine.Schema.DL_RS_NKServiceLevelMaxLength; } }

		protected override int MarksAndNumbers_MaxLength { get { return AutoSupplierBookingLine.Schema.DL_MarksAndNumbersMaxLength; } }

		protected override int ContactName_MaxLength { get { return AutoSupplierBookingLine.Schema.DL_ConsigneeContactMaxLength; } }

		protected override int ConsigneeAddressLine1_MaxLength { get { return AutoSupplierBookingLine.Schema.DL_ConsigneeAddress1MaxLength; } }

		protected override int ConsigneeAddressLine2_MaxLength { get { return AutoSupplierBookingLine.Schema.DL_ConsigneeAddress2MaxLength; } }

		protected override int ConsigneeCity_MaxLength { get { return AutoSupplierBookingLine.Schema.DL_ConsigneeCityMaxLength; } }

		protected override int ConsigneePostCode_MaxLength { get { return AutoSupplierBookingLine.Schema.DL_ConsigneePostCodeMaxLength; } }

		protected override int ConsigneeStateProvince_MaxLength { get { return AutoSupplierBookingLine.Schema.DL_ConsigneeStateMaxLength; } }

		protected override int ConsigneeCountry_MaxLength { get { return AutoSupplierBookingLine.Schema.DL_RN_NKConsigneeCountryCodeMaxLength; } }

		protected override int ContactPhone_MaxLength { get { return AutoSupplierBookingLine.Schema.DL_ConsigneePhoneMaxLength; } }

		protected override int ConsignorName_MaxLength { get { return AutoSupplierBookingLine.Schema.DL_ConsignorNameMaxLength; } }

		protected override int ConsignorAddressLine1_MaxLength { get { return AutoSupplierBookingLine.Schema.DL_ConsignorAddress1MaxLength; } }

		protected override int ConsignorAddressLine2_MaxLength { get { return AutoSupplierBookingLine.Schema.DL_ConsignorAddress2MaxLength; } }

		protected override int ConsignorCity_MaxLength { get { return AutoSupplierBookingLine.Schema.DL_ConsignorCityMaxLength; } }

		protected override int ConsignorPostCode_MaxLength { get { return AutoSupplierBookingLine.Schema.DL_ConsignorPostCodeMaxLength; } }

		protected override int ConsignorStateProvince_MaxLength { get { return AutoSupplierBookingLine.Schema.DL_ConsignorStateMaxLength; } }

		protected override int ConsignorCountry_MaxLength { get { return AutoSupplierBookingLine.Schema.DL_RN_NKConsignorCountryCodeMaxLength; } }

		protected override int VendorID_MaxLength { get { return AutoSupplierBookingLine.Schema.DL_VendorIdentifierMaxLength; } }
	}
}
