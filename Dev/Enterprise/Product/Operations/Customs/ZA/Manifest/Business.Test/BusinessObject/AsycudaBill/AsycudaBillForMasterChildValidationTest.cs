using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;
using UniversalConstants = Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.ZA.Manifest.Business.Testing
{
	class AsycudaBillForMasterChildValidationTest : BusinessObjectValidationTestCase
	{
		public void TestABL_E_DEP()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestValidationRule, "ManifestValidationRule");
			var messageError = "An Estimated Departure Time is required";
			var validationRule = helper.CreateNewOrGetExistingCusCodeList(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, RefCusCodeListTypes.Codes.ManifestValidationRule, UniversalConstants.RefCusCodeList.ManifestValidationRuleCodes.EstimatedDepartureTime, messageError, ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(validationRule.PK, UniversalConstants.RefCusCodeList.ManifestValidationRuleCodes.Mandatory, "");
			messageError += $" for {GlbCompany.CurrentCompany.GC_RN_NKCountryCode}.";
			Factory.Save();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			header.AMA_E_DEP = ZDateTime.BrettsBirthday;
			AssertNoMessageError(header.MasterBill.ABL_E_DEPInfo, messageError);
			header.AMA_ManifestType = nameof(ManifestDocumentType.ALH);
			header.AMA_E_DEP = ZDateTime.Empty;
			AssertHasMessageError(header.MasterBill.ABL_E_DEPInfo, messageError);
			header.AMA_ManifestType = nameof(ManifestDocumentType.RFM);
			header.AMA_E_DEP = ZDateTime.Empty;
			AssertNoMessageError(header.MasterBill.ABL_E_DEPInfo, messageError);
		}
	}
}
