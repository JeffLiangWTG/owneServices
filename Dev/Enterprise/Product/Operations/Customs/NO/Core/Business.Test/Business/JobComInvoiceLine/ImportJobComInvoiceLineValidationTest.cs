using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(ImportJobComInvoiceLineValidation))]
sealed class ImportJobComInvoiceLineValidationTest : JobComInvoiceLineValidationAbstractTest
{
	static class MessageErrors
	{
		public const string NoSupportingDocument = "supporting document of type M2 must be entered";
	}

	public void TestCheckJI_PrimaryPreference()
	{
		var tariffTestHelper = new RefCusTariffTestHelper(Factory);
		var tariffTest1 = tariffTestHelper.CreateImportTariff("77777777");
		tariffTestHelper.AddRate(tariffTest1, PrimaryPreferenceCodeList.Codes.A, "1");
		ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.JI_PrimaryPreferenceInfo, "XX", "A");
	}

	public void TestCheckJI_PrimaryPreference_PreferenceAndZeroRate()
	{
		var errorMessage = "The preference code is not valid. On tariff numbers with general customs rate 0, the only valid preference codes are N or J";

		var tariffTestHelper = new RefCusTariffTestHelper(Factory);
		var tariffTest1 = tariffTestHelper.CreateImportTariff("77777777");
		tariffTestHelper.AddRate(tariffTest1, PrimaryPreferenceCodeList.Codes.B, "0", Core.Constants.CountryCodes.Sweden);
		tariffTestHelper.AddRate(tariffTest1, PrimaryPreferenceCodeList.Codes.B, "1", Core.Constants.CountryCodes.Denmark);
		tariffTestHelper.AddGeneralRate(tariffTest1, PrimaryPreferenceCodeList.Codes.N, "0");

		var tariffTest2 = tariffTestHelper.CreateImportTariff("88888888");
		tariffTestHelper.AddRate(tariffTest2, PrimaryPreferenceCodeList.Codes.B, "0", Core.Constants.CountryCodes.Sweden);
		tariffTestHelper.AddRate(tariffTest2, PrimaryPreferenceCodeList.Codes.B, "1", Core.Constants.CountryCodes.Denmark);
		tariffTestHelper.AddGeneralRate(tariffTest2, PrimaryPreferenceCodeList.Codes.N, "5");

		CombineAssertions(() =>
		{
			invoiceLine.JI_Tariff = "77777777";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Sweden;
			invoiceLine.JI_PrimaryPreference = PrimaryPreferenceCodeList.Codes.B;
			AssertHasError("Sweden - Has zero formula", invoiceLine.JI_PrimaryPreferenceInfo, errorMessage);

			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Denmark;
			invoiceLine.JI_PrimaryPreference = PrimaryPreferenceCodeList.Codes.B;
			AssertHasError("Denmark - Has zero formula", invoiceLine.JI_PrimaryPreferenceInfo, errorMessage);

			invoiceLine.JI_CountryOfOrigin = ZString.Empty;
			invoiceLine.JI_PrimaryPreference = ZString.Empty;
			invoiceLine.JI_Tariff = "88888888";

			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Sweden;
			invoiceLine.JI_PrimaryPreference = PrimaryPreferenceCodeList.Codes.B;
			AssertNoError("Sweden - No zero formula", invoiceLine.JI_PrimaryPreferenceInfo, errorMessage);

			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Denmark;
			invoiceLine.JI_PrimaryPreference = PrimaryPreferenceCodeList.Codes.B;
			AssertNoError("Denmark - No zero formula", invoiceLine.JI_PrimaryPreferenceInfo, errorMessage);
		});
	}

	public void TestCheckJI_PrimaryPreference_ReducedRateAndPreference()
	{
		const string messageErrorText = "The selected reduced customs flag requires preference code to be N or J.";

		var tariffTestHelper = new RefCusTariffTestHelper(Factory);
		var tariffTest1 = tariffTestHelper.CreateImportTariff("77777777");
		tariffTestHelper.AddRate(tariffTest1, PrimaryPreferenceCodeList.Codes.B, "1", Enterprise.Core.Constants.CountryCodes.Denmark);
		invoiceLine.JI_Tariff = "77777777";

		CombineAssertions(() =>
		{
			invoiceLine.JI_CountryOfOrigin = Enterprise.Core.Constants.CountryCodes.Denmark;
			invoiceLine.JI_ReducedCustomsFlag = ReducedCustomsFlagList.Codes.S;
			invoiceLine.JI_PrimaryPreference = PrimaryPreferenceCodeList.Codes.B;
			AssertHasMessageError("Has ReducedFlag and Preference", invoiceLine.JI_PrimaryPreferenceInfo, messageErrorText);

			invoiceLine.JI_CountryOfOrigin = Enterprise.Core.Constants.CountryCodes.Denmark;
			invoiceLine.JI_ReducedCustomsFlag = ZString.Empty;
			invoiceLine.JI_PrimaryPreference = PrimaryPreferenceCodeList.Codes.B;
			AssertNoMessageError("No reduced flag", invoiceLine.JI_PrimaryPreferenceInfo, messageErrorText);
		});
	}

	public void TestCheckJI_PrimaryPreference_VerifyOriginCountryInTradegroup()
	{
		var errorMessage = "The preference code is not valid for this Country of origin";

		var tariffTestHelper = new RefCusTariffTestHelper(Factory);
		var tariffTest1 = tariffTestHelper.CreateImportTariff("77777777");
		tariffTestHelper.AddRate(tariffTest1, PrimaryPreferenceCodeList.Codes.B, "1", Enterprise.Core.Constants.CountryCodes.Sweden);
		tariffTestHelper.AddRate(tariffTest1, PrimaryPreferenceCodeList.Codes.C, "2", Enterprise.Core.Constants.CountryCodes.Denmark);
		invoiceLine.JI_Tariff = "77777777";

		CombineAssertions(() =>
		{
			invoiceLine.JI_CountryOfOrigin = Enterprise.Core.Constants.CountryCodes.Sweden;
			invoiceLine.JI_PrimaryPreference = PrimaryPreferenceCodeList.Codes.C;
			AssertHasMessageError("Sweden - Not in Tradegroup", invoiceLine.JI_PrimaryPreferenceInfo, errorMessage);

			invoiceLine.JI_CountryOfOrigin = Enterprise.Core.Constants.CountryCodes.Denmark;
			invoiceLine.JI_PrimaryPreference = PrimaryPreferenceCodeList.Codes.C;
			AssertNoError("Denmark - In Tradegroup", invoiceLine.JI_PrimaryPreferenceInfo, errorMessage);
		});
	}
	public void TestCheckJI_PrimaryPreference_SER_Required()
	{
		var expectedMessageError = "Preference code 'A' requires a Supporting Document of type 'SER'.";
		var expectedMessageWarning = "Preference code is set to 'A', and SER/FAKTURAERKLÆRING  is added to box [44] Supporting documents.";

		invoiceLine.JI_PrimaryPreference = "J";
		AssertNoMessageError(invoiceLine.JI_PrimaryPreferenceInfo, expectedMessageError);
		AssertNoWarning(invoiceLine.JI_PrimaryPreferenceInfo, expectedMessageWarning);

		var suppDoc = invoiceLine.SupportingDocuments.AddNew();
		suppDoc.CSI_Code = "TXT";
		suppDoc.CSI_Description = "Description";
		invoiceLine.Validation.ValidateAll();
		AssertNoMessageError(invoiceLine.JI_PrimaryPreferenceInfo, expectedMessageError);
		AssertNoWarning(invoiceLine.JI_PrimaryPreferenceInfo, expectedMessageWarning);

		invoiceLine.JI_PrimaryPreference = "A";
		AssertHasMessageError("This should trigger error", invoiceLine.JI_PrimaryPreferenceInfo, expectedMessageError);
		AssertNoWarning(invoiceLine.JI_PrimaryPreferenceInfo, expectedMessageWarning);

		suppDoc.CSI_Code = SupportingDocumentCodeList.CertificateForOrigin;
		invoiceLine.Validation.ValidateAll();
		AssertNoMessageError(invoiceLine.JI_PrimaryPreferenceInfo, expectedMessageError);
		AssertHasWarning(invoiceLine.JI_PrimaryPreferenceInfo, expectedMessageWarning);
	}

	public void TestCheckJI_Procedure()
	{
		RefCusProcedureHelper.CreateRefCusProcedureList(Factory);
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		instruction.CEI_Style = "4";
		invoiceLine.JI_Procedure = ZString.Empty;
		invoiceLine.JI_CEI = instruction.PK;
		CombineAssertions(() =>
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.JI_ProcedureInfo,
				new ZString[] { "5011", "6011", "7001", "1030" },
				new ZString[] { "4000", "4010", "4110", "4111" });
			AssertNoMessageErrors(invoiceLine.JI_ProcedureInfo);
		});
	}

	public void TestCheckJI_StateOrRegionOfOrigin()
	{
		CombineAssertions(() =>
		{
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Norway;
			invoiceLine.Validation.ValidateAll();
			AssertNoMessageErrors(invoiceLine.JI_StateOrRegionOfOriginInfo);

			invoiceLine.JI_CountryOfOrigin = ZString.Empty;
			invoiceLine.Validation.ValidateAll();
			AssertNoMessageErrors(invoiceLine.JI_StateOrRegionOfOriginInfo);
		});
	}

	public void TestCheckJI_CountryOfOrigin()
	{
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(invoiceLine.JI_CountryOfOriginInfo, "QQ", Core.Constants.CountryCodes.UnitedStates);
	}

	public void TestCheckJI_ZZF_NKTaxType()
	{
		RefCusTaxOrFeeHelper.CreateRefCusTaxOrFeeList(Factory);

		CombineAssertions(() =>
		{
			ValidationTestHelper.AssertErrorIfInvalidCode(invoiceLine.JI_ZZF_NKTaxTypeInfo, "ZZ", "MV1");
			ValidationTestHelper.AssertErrorIfNotEntered(invoiceLine.JI_ZZF_NKTaxTypeInfo, MandatoryValidation.MustBeEnteredMessage(invoiceLine.JI_ZZF_NKTaxTypeInfo.HumanReadableName));
		});
	}

	public void TestCheckJI_Tariff_UniqueRateForRateType()
	{
		var testHelper = new RefCusTariffTestHelper(Factory);
		testHelper.SetupTariffWithMultipleTariffRates();
		invoiceLine.JI_PrimaryPreference = "N";
		invoiceLine.JI_CountryOfOrigin = Enterprise.Core.Constants.CountryCodes.Latvia;

		CombineAssertions(() =>
		{
			const string errorMessage = "There is more than one applicable type of customs rate for the Tariff 21069060. Please select customs rate override to select the type that applies.";
			invoiceLine.JI_Tariff = "21069060";
			AssertHasMessageError("No override, should set MessageError", invoiceLine.JI_TariffInfo, errorMessage);
			invoiceLine.CustomsRateIsOverridden = true;
			AssertNoMessageError("Overridden, should NOT set MessageError", invoiceLine.JI_TariffInfo, errorMessage);
		});
	}

	public void TestCheckJI_ZZF_NKTaxType_ShouldRequireSupportingDocument_WhenTaxTypeMf()
	{
		var taxTypeInfo = invoiceLine.JI_ZZF_NKTaxTypeInfo;
		CombineAssertions(() =>
		{
			AssertNoMessageErrorContaining("(base-case): invoiceLine.JI_ZZF_NKTaxTypeInfo should NOT have message error", taxTypeInfo, MessageErrors.NoSupportingDocument);

			invoiceLine.JI_ZZF_NKTaxType = "MVF";
			AssertHasMessageErrorContaining("(when-mvf-set): invoiceLine.JI_ZZF_NKTaxTypeInfo should have message error", taxTypeInfo, MessageErrors.NoSupportingDocument);

			var supportingDocument = invoiceLine.SupportingDocuments.AddNew();
			supportingDocument.CSI_Code = "M2";
			invoiceLine.Validation.ValidateJI_ZZF_NKTaxType();
			AssertNoMessageErrorContaining("(when-m2-added): invoiceLine.JI_ZZF_NKTaxTypeInfo should NOT have message error", taxTypeInfo, MessageErrors.NoSupportingDocument);
		});
	}

	public void TestCheckJI_ZZF_NKTaxType_ShouldNotRequireSupportingDocument_WhenMvaImporter()
	{
		var taxTypeInfo = invoiceLine.JI_ZZF_NKTaxTypeInfo;
		CombineAssertions(() =>
		{
			AssertNoMessageErrorContaining("(base-case): invoiceLine.JI_ZZF_NKTaxTypeInfo should NOT have message error", taxTypeInfo, MessageErrors.NoSupportingDocument);

			invoiceLine.JI_ZZF_NKTaxType = "MVF";
			AssertHasMessageErrorContaining("(when-mvf-set): invoiceLine.JI_ZZF_NKTaxTypeInfo should have message error", taxTypeInfo, MessageErrors.NoSupportingDocument);

			var mvaImporter = Factory.NewWithValidTestData<OrgHeader>().AsMVARegistered();
			declaration.JE_OH_Importer = mvaImporter.PK;
			invoiceLine.Validation.ValidateJI_ZZF_NKTaxType();
			AssertNoMessageErrorContaining("(when-mva-registered): invoiceLine.JI_ZZF_NKTaxTypeInfo should NOT have message error", taxTypeInfo, MessageErrors.NoSupportingDocument);
		});
	}

	protected override string MessageType => JobMessageTypeList.Codes.Import;
}
