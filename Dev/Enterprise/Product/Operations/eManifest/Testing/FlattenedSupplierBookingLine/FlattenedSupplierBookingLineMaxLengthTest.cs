using System.Diagnostics.CodeAnalysis;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.eManifest.Business.Testing
{
	internal class FlattenedSupplierBookingLineMaxLengthTest : TestCase
	{
		[SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplification hides desired base class")]
		public void TestMaxLengthInFlattenedSupplierBookingLineIsTheSameAsInImportedObjects()
		{
			var line = new FlattenedSupplierBookingLine();
			AssertEquals(SupplierBookingLine.Schema.DL_ConsigneeReferenceMaxLength, line.ConsigneeReferenceInfo.MaxLength);
			AssertEquals(SupplierBookingLine.Schema.DL_RX_NKGoodsValueCurrencyMaxLength, line.GoodsValueCurrencyInfo.MaxLength);
			AssertEquals(SupplierBookingLine.Schema.DL_GrossWeightUQMaxLength, line.GrossWeightUQInfo.MaxLength);
			AssertEquals(SupplierBookingLine.Schema.DL_CubicUQMaxLength, line.CubicUQInfo.MaxLength);
			AssertEquals(SupplierBookingLine.Schema.DL_GoodsDescriptionMaxLength, line.GoodsDescriptionInfo.MaxLength);
			AssertEquals(SupplierBookingLine.Schema.DL_MarksAndNumbersMaxLength, line.MarksAndNumbersInfo.MaxLength);
			AssertEquals(JobDocAddress.Schema.E2_ContactMaxLength, line.ContactNameInfo.MaxLength);
			AssertEquals(JobDocAddress.Schema.E2_Address1MaxLength, line.ConsigneeAddressLine1Info.MaxLength);
			AssertEquals(JobDocAddress.Schema.E2_Address2MaxLength, line.ConsigneeAddressLine2Info.MaxLength);
			AssertEquals(JobDocAddress.Schema.E2_CityMaxLength, line.ConsigneeCityInfo.MaxLength);
			AssertEquals(JobDocAddress.Schema.E2_PostcodeMaxLength, line.ConsigneePostCodeInfo.MaxLength);
			AssertEquals(JobDocAddress.Schema.E2_StateMaxLength, line.ConsigneeStateProvinceInfo.MaxLength);
			AssertEquals(JobDocAddress.Schema.E2_RN_NKCountryCodeMaxLength, line.ConsigneeCountryInfo.MaxLength);
			AssertEquals(JobDocAddress.Schema.E2_PhoneMaxLength, line.ContactPhoneInfo.MaxLength);
			AssertEquals(SupplierBookingLine.Schema.DL_ConsignorNameMaxLength, line.ConsignorNameInfo.MaxLength);
			AssertEquals(JobDocAddress.Schema.E2_Address1MaxLength, line.ConsignorAddressLine1Info.MaxLength);
			AssertEquals(JobDocAddress.Schema.E2_Address2MaxLength, line.ConsignorAddressLine2Info.MaxLength);
			AssertEquals(JobDocAddress.Schema.E2_CityMaxLength, line.ConsignorCityInfo.MaxLength);
			AssertEquals(JobDocAddress.Schema.E2_PostcodeMaxLength, line.ConsignorPostCodeInfo.MaxLength);
			AssertEquals(JobDocAddress.Schema.E2_StateMaxLength, line.ConsignorStateProvinceInfo.MaxLength);
			AssertEquals(JobDocAddress.Schema.E2_RN_NKCountryCodeMaxLength, line.ConsignorCountryInfo.MaxLength);
			AssertEquals(SupplierBookingLine.Schema.DL_OrderTrackingNumberMaxLength, line.OrderTrackingNumberInfo.MaxLength);
			AssertEquals(SupplierBookingLine.Schema.DL_RS_NKServiceLevelMaxLength, line.ServiceLevelInfo.MaxLength);
			AssertEquals(SupplierBookingLine.Schema.DL_VendorIdentifierMaxLength, line.VendorIDInfo.MaxLength);
		}
	}
}
