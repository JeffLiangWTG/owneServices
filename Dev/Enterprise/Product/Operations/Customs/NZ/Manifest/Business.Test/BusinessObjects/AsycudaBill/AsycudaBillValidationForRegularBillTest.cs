using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.NZ.Manifest.Business.Testing
{
	sealed class AsycudaBillValidationForRegularBillTest : CargoWise.EntityFramework.Testing.BusinessObjectValidationTestCase
	{
		public void TestBillsValidationNoNotifications()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_ManifestUQ = ZString.Empty;
			bill.Validation.ValidateABL_ManifestUQ();
			AssertNoMessageErrors(bill.ABL_ManifestUQInfo);
			bill.ABL_ManifestUQ = "PK";
			bill.Validation.ValidateABL_ManifestUQ();
			AssertNoMessageErrors(bill.ABL_ManifestUQInfo);
			bill.ABL_OA_Consignee = ZGuid.Empty;
			bill.Validation.ValidateABL_OA_Consignee();
			AssertNoMessageErrors(bill.ABL_OA_ConsigneeInfo);
			bill.ABL_ConsigneeName = ZString.Empty;
			bill.Validation.ValidateABL_ConsigneeName();
			AssertNoMessageErrors(bill.ABL_ConsigneeNameInfo);
			bill.ABL_ConsigneeStreet1 = ZString.Empty;
			bill.Validation.ValidateABL_ConsigneeStreet1();
			AssertNoMessageErrors(bill.ABL_ConsigneeStreet1Info);
			bill.ABL_ConsigneeStreet2 = ZString.Empty;
			bill.Validation.ValidateABL_ConsigneeStreet2();
			AssertNoMessageErrors(bill.ABL_ConsigneeStreet2Info);
			bill.ABL_ConsigneeCity = ZString.Empty;
			bill.Validation.ValidateABL_ConsigneeCity();
			AssertNoMessageErrors(bill.ABL_ConsigneeCityInfo);
			bill.ABL_RN_NKConsigneeCountry = ZString.Empty;
			bill.Validation.ValidateABL_RN_NKConsigneeCountry();
			AssertNoMessageErrors(bill.ABL_RN_NKConsigneeCountryInfo);
			bill.ABL_ConsigneePostcode = ZString.Empty;
			bill.Validation.ValidateABL_ConsigneePostcode();
			AssertNoMessageErrors(bill.ABL_ConsigneePostcodeInfo);
			bill.ABL_GrossWeight = 0.0m;
			bill.Validation.ValidateABL_GrossWeight();
			AssertNoNotifications(bill.ABL_GrossWeightInfo);
			bill.ABL_GrossWeightUQ = ZString.Empty;
			bill.Validation.ValidateABL_GrossWeightUQ();
			AssertNoNotifications(bill.ABL_GrossWeightUQInfo);
			bill.ABL_GrossWeightUQ = "PK";
			bill.Validation.ValidateABL_GrossWeightUQ();
			AssertNoNotifications(bill.ABL_GrossWeightUQInfo);
		}

		public void TestCheckCustomsEntryNumber()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsEntryNumberTypes, "FRM", "Formal Entry No.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.NewZealand, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsEntryNumberTypes, "ORG", "Original Entry No.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.NewZealand, NZManifestTypes.Codes.OCR, ApplicationCodeTypeList.Codes.ShippingLine);
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.NewZealand;
			var bill = header.Bills.AddNew();
			bill.CustomsEntryNumberType = "FRM";
			bill.CustomsEntryNumber = "01234567890";
			bill.Validation.ValidateCustomsEntryNumber();
			AssertNoMessageErrors(bill.CustomsEntryNumberInfo);
			bill.CustomsEntryNumber = ZString.Empty;
			bill.Validation.ValidateCustomsEntryNumber();
			AssertHasMessageError(bill.CustomsEntryNumberInfo, "You have not entered an Export Delivery Order Number.");
		}
	}
}
