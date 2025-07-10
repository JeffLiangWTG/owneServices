using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.NZ.Manifest.Business.Testing
{
	sealed class AsycudaManifestHeaderValidationTest : TestCaseWithFactory
	{
		public void TestCheckAMA_OA_Carrier()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.NewZealand;
			header.AMA_ManifestType = NZManifestTypes.Codes.ICR;
			header.AMA_OA_Carrier = ZGuid.Empty;
			header.Validation.ValidateAMA_OA_Carrier();
			AssertHasWarningContaining(header.AMA_OA_CarrierInfo, MandatoryValidation.YouHaveNotEntered);
			header.AMA_ManifestType = NZManifestTypes.Codes.OCR;
			header.Validation.ValidateAMA_OA_Carrier();
			AssertHasMessageErrorContaining(header.AMA_OA_CarrierInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckAMA_CustomsOffice()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.NewZealand;
			header.AMA_ManifestType = NZManifestTypes.Codes.OCR;
			header.Validation.ValidateAMA_CustomsOffice();
			AssertNoNotifications("Customs office field is not enabled for NZ - should not be erroring and stoping sending", header.AMA_CustomsOfficeInfo);
			AssertNoWarnings("Customs office field is not enabled for NZ - should not be erroring and stoping sending", header.AMA_CustomsOfficeInfo);
			AssertNoErrors("Customs office field is not enabled for NZ - should not be erroring and stoping sending", header.AMA_CustomsOfficeInfo);
			AssertNoMessageErrors("Customs office field is not enabled for NZ - should not be erroring and stoping sending", header.AMA_CustomsOfficeInfo);
		}
	}
}
