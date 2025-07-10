using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business.Testing;

namespace Enterprise.Customs.US.Business.LicenseManager.Testing
{
	sealed class ECCNValidationTest : TestCaseWithFactory
	{
		public void TestGetECCNErrorWhenNoLiceseType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var refCusCodeS94 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECCNNumber, USAESLicenseCode.Codes.S94, USAESLicenseCode.Codes.S94, new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			var refCusCodeECCN3C992 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECCNNumber, "3C992", "3C992", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCodeECCN3C992.PK, Universal.RefCusCodeListAttributeTypes.Codes.LicenseType, USAESLicenseCode.Codes.C35);
			Factory.Save();
			AssertEquals("GetECCNError", "", LicenseValidationHelper.GetECCNError("", "", Factory, ZDateTime.Today));
			AssertEquals("GetECCNError", string.Format(ECCNValidation.CodeIsInvalid, "3A667", ""), LicenseValidationHelper.GetECCNError("", "3A667", Factory, ZDateTime.Today));
		}

		public void TestGetECCNError_ForECCN600SeriesDotYNotEligible()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var eccnNumberNotEligible = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECCNNumber, "0A606", "0A606", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeListAttribute(eccnNumberNotEligible.PK, Universal.RefCusCodeListAttributeTypes.Codes.AllowECCNNLR, Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.NotEligible);

			var eccnNumberNotEligible2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECCNNumber, "5A606", "5A606", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeListAttribute(eccnNumberNotEligible2.PK, Universal.RefCusCodeListAttributeTypes.Codes.LicenseType, "C59");

			var eccnNumberEligibleForAUGB = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECCNNumber, "3A611", "3A611", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeListAttribute(eccnNumberEligibleForAUGB.PK, Universal.RefCusCodeListAttributeTypes.Codes.AllowECCNNLR, Core.Constants.CountryCodes.Australia);
			helper.CreateNewOrGetExistingCusCodeListAttribute(eccnNumberEligibleForAUGB.PK, Universal.RefCusCodeListAttributeTypes.Codes.AllowECCNNLR, Core.Constants.CountryCodes.UnitedKingdom);

			Factory.Save();

			AssertEquals(ZString.Empty, LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C33, ZString.Empty, Factory, ZDateTime.Today, destinationCountry: ZString.Empty));
			AssertEquals(ZString.Empty, LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C33, "5A606", Factory, ZDateTime.Today, destinationCountry: ZString.Empty));

			AssertEquals(ECCNValidation.ECCN600SeriesDotYNotEligible, LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C32, "0A606", Factory, ZDateTime.Today, destinationCountry: ZString.Empty));
			AssertEquals(ZString.Empty, LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C32, "5A606", Factory, ZDateTime.Today, destinationCountry: ZString.Empty));
			AssertEquals(ECCNValidation.ECCN600SeriesDotYNotEligible, LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C32, "3A611", Factory, ZDateTime.Today, destinationCountry: ZString.Empty));

			AssertEquals(ZString.Empty, LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C32, "3A611", Factory, ZDateTime.Today, destinationCountry: Core.Constants.CountryCodes.Australia));
			AssertEquals(ZString.Empty, LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C32, "3A611", Factory, ZDateTime.Today, destinationCountry: Core.Constants.CountryCodes.UnitedKingdom));
			AssertEquals(ZString.Empty, LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C32, "5A606", Factory, ZDateTime.Today, destinationCountry: Core.Constants.CountryCodes.UnitedKingdom));

			AssertEquals(ECCNValidation.ECCN600SeriesDotYNotEligible, LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C33, "0A606", Factory, ZDateTime.Today, destinationCountry: ZString.Empty));
			AssertEquals(ECCNValidation.ECCN600SeriesDotYNotEligible, LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C33, "3A611", Factory, ZDateTime.Today, destinationCountry: ZString.Empty));
			AssertEquals(ZString.Empty, LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C33, "3A611", Factory, ZDateTime.Today, destinationCountry: Core.Constants.CountryCodes.Australia));
			AssertEquals(ZString.Empty, LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C33, "3A611", Factory, ZDateTime.Today, destinationCountry: Core.Constants.CountryCodes.UnitedKingdom));
		}

		public void TestGetECCNErrorForC35C45()
		{
			AssertEquals("GetECCNError", "ECCN must be formatted NANNN.", LicenseValidationHelper.GetECCNError("", "!@#$", Factory, ZDateTime.Today));
			AssertEquals("GetECCNError", "", LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C35, "0A501", Factory, ZDateTime.Today));
			AssertEquals("GetECCNError", "", LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C35, "0A502", Factory, ZDateTime.Today));
			AssertEquals("GetECCNError", "", LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C35, "0A504", Factory, ZDateTime.Today));
			AssertEquals("GetECCNError", "", LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C35, "0A505", Factory, ZDateTime.Today));
			AssertEquals("GetECCNError", "", LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C35, "0B501", Factory, ZDateTime.Today));
			AssertEquals("GetECCNError", "", LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C35, "0B505", Factory, ZDateTime.Today));
			AssertEquals("GetECCNError", "", LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C35, "0A602", Factory, ZDateTime.Today));
			AssertEquals("GetECCNError", "", LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C35, "0B602", Factory, ZDateTime.Today));

			AssertEquals("GetECCNError", "", LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C45, "0A602", Factory, ZDateTime.Today));
			AssertEquals("GetECCNError", "", LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C45, "0B602", Factory, ZDateTime.Today));
			AssertEquals("GetECCNError", "", LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C45, "0D602", Factory, ZDateTime.Today));
			AssertEquals("GetECCNError", "", LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C45, "1A613", Factory, ZDateTime.Today));
			AssertEquals("GetECCNError", "", LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C45, "0E602", Factory, ZDateTime.Today));
		}

		public void TestGetECCNError()
		{
			AssertEquals("GetECCNError", "", LicenseValidationHelper.GetECCNError("ZZ", "", Factory, ZDateTime.Today));

			AssertEquals("GetECCNError", string.Format(ECCNValidation.ECCNIsRequiredMessage, USAESLicenseCode.Codes.C31), LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C31, "", Factory, ZDateTime.Today));
			AssertEquals("GetECCNError", "", LicenseValidationHelper.GetECCNError("ZZ", "5A992", Factory, ZDateTime.Today));

			AssertEquals("GetECCNError", string.Format(ECCNValidation.ECCNIsRequiredMessage, USAESLicenseCode.Codes.C54), LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C54, "", Factory, ZDateTime.Today));
			AssertEquals("GetECCNError", "", LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C54, "1C988", Factory, ZDateTime.Today));

			AssertEquals("GetECCNError", string.Format(ECCNValidation.ECCNIsRequiredMessage, USAESLicenseCode.Codes.C53), LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C53, "", Factory, ZDateTime.Today));
			AssertEquals("GetECCNError", ECCNValidation.ECCNIsRequiredMessageForSpecialCountry, LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C42, "", Factory, ZDateTime.Today, countryCode: Core.Constants.CountryCodes.China));
			AssertEquals("GetECCNError", ECCNValidation.ECCNIsRequiredMessageForSpecialCountry, LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C33, "", Factory, ZDateTime.Today, countryCode: Core.Constants.CountryCodes.China));
			AssertEquals("GetECCNError", ECCNValidation.ECCNIsRequiredMessageForSpecialCountry, LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C42, "", Factory, ZDateTime.Today, countryCode: Core.Constants.CountryCodes.Russia));
			AssertEquals("GetECCNError", ECCNValidation.ECCNIsRequiredMessageForSpecialCountry, LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C33, "", Factory, ZDateTime.Today, countryCode: Core.Constants.CountryCodes.Russia));
			AssertEquals("GetECCNError", ECCNValidation.ECCNIsRequiredMessageForSpecialCountry, LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C42, "", Factory, ZDateTime.Today, countryCode: Core.Constants.CountryCodes.Venezuela));
			AssertEquals("GetECCNError", ECCNValidation.ECCNIsRequiredMessageForSpecialCountry, LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C33, "", Factory, ZDateTime.Today, countryCode: Core.Constants.CountryCodes.Venezuela));
			AssertEquals("GetECCNError", "", LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C53, "4A003", Factory, ZDateTime.Today));
			AssertEquals("GetECCNError", "", LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C53, "4D001", Factory, ZDateTime.Today));

			AssertEquals("GetECCNError - invalid format", ECCNValidation.ECCNIsInvalidFormat, LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C30, "12345", Factory, ZDateTime.Today));
			AssertEquals("GetECCNError - invalid format", ECCNValidation.ECCNIsInvalidFormat, LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C30, "ABCDE", Factory, ZDateTime.Today));
			AssertEquals("GetECCNError - invalid format", ECCNValidation.ECCNIsInvalidFormat, LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C30, "12D34", Factory, ZDateTime.Today));

			AssertEquals(string.Format(ECCNValidation.ECCNNotAllowed, "1C351", USAESLicenseCode.Codes.C33), LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C33, "1C351", Factory, ZDateTime.Today));

			AssertEquals(string.Empty, LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C30, "1A614", Factory, ZDateTime.Today));

			AssertEquals(ZString.Empty, LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C35, "0A018", Factory, ZDateTime.Today));
			AssertEquals(ZString.Empty, LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C35, "0A614", Factory, ZDateTime.Today));
			AssertEquals(ZString.Empty, LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C35, "9A515", Factory, ZDateTime.Today));
			AssertEquals("GetECCNError", "", LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C35, "3A611", Factory, ZDateTime.Today));
			AssertEquals("GetECCNError", "", LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C35, "3B611", Factory, ZDateTime.Today));
			AssertEquals("GetECCNError", "", LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C35, "9A620", Factory, ZDateTime.Today));
			AssertEquals("GetECCNError", "", LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C35, "9B620", Factory, ZDateTime.Today));
			AssertEquals("GetECCNError", "", LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C35, "1B607", Factory, ZDateTime.Today));
			AssertEquals("GetECCNError", "", LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C35, "6B619", Factory, ZDateTime.Today));
			AssertEquals("GetECCNError", "", LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C35, "7A611", Factory, ZDateTime.Today));
			AssertEquals("GetECCNError", "", LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C35, "7B611", Factory, ZDateTime.Today));

			AssertEquals(ZString.Empty, LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C30, "0A614", Factory, ZDateTime.Today));
			AssertEquals(ZString.Empty, LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C30, "9A515", Factory, ZDateTime.Today));

			AssertEquals(ZString.Empty, LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C60, "9A610", Factory, ZDateTime.Today));
			AssertEquals(ZString.Empty, LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C60, "8E609", Factory, ZDateTime.Today));

			AssertEquals(ZString.Empty, LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C38, "4D001", Factory, ZDateTime.Today));
			AssertEquals(ZString.Empty, LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C38, "8E002", Factory, ZDateTime.Today));

			AssertEquals(ECCNValidation.Additional600ECCNSNotAllowed, LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C33, "0A614", Factory, ZDateTime.Today));

			AssertEquals(ZString.Empty, LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C59, "1A607", Factory, ZDateTime.Today));
			AssertEquals(ZString.Empty, LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C59, "1B607", Factory, ZDateTime.Today));
			AssertEquals(ZString.Empty, LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C59, "1C607", Factory, ZDateTime.Today));
			AssertEquals(ZString.Empty, LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C59, "1D607", Factory, ZDateTime.Today));
			AssertEquals(ZString.Empty, LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C59, "1E607", Factory, ZDateTime.Today));
			AssertEquals(ZString.Empty, LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C59, "6B619", Factory, ZDateTime.Today));
			AssertEquals(ZString.Empty, LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C59, "6D619", Factory, ZDateTime.Today));
			AssertEquals(ZString.Empty, LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C59, "6E619", Factory, ZDateTime.Today));
			AssertEquals(ZString.Empty, LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C59, "7A611", Factory, ZDateTime.Today));
			AssertEquals(ZString.Empty, LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C59, "7B611", Factory, ZDateTime.Today));
			AssertEquals(ZString.Empty, LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C59, "7D611", Factory, ZDateTime.Today));
			AssertEquals(ZString.Empty, LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C59, "7E611", Factory, ZDateTime.Today));

			AssertEquals(ZString.Empty, LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C60, "1A613", Factory, ZDateTime.Today));
			AssertEquals(ZString.Empty, LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C60, "9E619", Factory, ZDateTime.Today));
			AssertEquals(ECCNValidation.Additional600ECCNSNotAllowed, LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.SAG, "0D614", Factory, ZDateTime.Today));

			AssertEquals(ZString.Empty, LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C60, "7A611", Factory, ZDateTime.Today));
			AssertEquals(ZString.Empty, LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C60, "7D611", Factory, ZDateTime.Today));
			AssertEquals(ZString.Empty, LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C60, "7E611", Factory, ZDateTime.Today));
			AssertEquals(ZString.Empty, LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C60, "9A515", Factory, ZDateTime.Today));
			AssertEquals(ZString.Empty, LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C60, "9D515", Factory, ZDateTime.Today));
			AssertEquals(ZString.Empty, LicenseValidationHelper.GetECCNError(USAESLicenseCode.Codes.C60, "9E515", Factory, ZDateTime.Today));
		}

		protected override void SetUp()
		{
			base.SetUp();
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(Factory, new ZString[] {
				USAESLicenseCode.Codes.C30, USAESLicenseCode.Codes.C31, USAESLicenseCode.Codes.C32, USAESLicenseCode.Codes.C33, USAESLicenseCode.Codes.C35, USAESLicenseCode.Codes.C38, USAESLicenseCode.Codes.C41, USAESLicenseCode.Codes.C45,
				USAESLicenseCode.Codes.C46, USAESLicenseCode.Codes.C53, USAESLicenseCode.Codes.C54, USAESLicenseCode.Codes.C57, USAESLicenseCode.Codes.C58, USAESLicenseCode.Codes.C59, USAESLicenseCode.Codes.C60, USAESLicenseCode.Codes.C63,
				USAESLicenseCode.Codes.E01, USAESLicenseCode.Codes.SAG, USAESLicenseCode.Codes.SAU, USAESLicenseCode.Codes.SCA, USAESLicenseCode.Codes.SGB, USAESLicenseCode.Codes.S00, USAESLicenseCode.Codes.S05, USAESLicenseCode.Codes.S61,
				USAESLicenseCode.Codes.S73, USAESLicenseCode.Codes.S85, USAESLicenseCode.Codes.S94, USAESLicenseCode.Codes.VDS });
		}
	}
}
