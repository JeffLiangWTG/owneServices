using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USDASugarCertificateValidatorTest : PermitValidatorTest<USDASugarCertificateValidator>
	{
		public void TestUSDASugarCertificate_ACS()
		{
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			InvoiceLine.AddInfoValidation.ValidateUS_MiscPermitNo();
			AssertEquals(false, InvoiceLine.US_MiscPermitNoInfo.HasNotifications());
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_DateFrom = ZDateTime.Now.AddDays(-2);
			tariff.UE_DateTo = ZDateTime.Now.AddDays(2);
			InvoiceLine.JI_Tariff = tariff.UE_Tariff;
			InvoiceLine.AddInfoValidation.ValidateUS_MiscPermitNo();
			AssertEquals(false, InvoiceLine.US_MiscPermitNoInfo.HasNotifications());
			tariff.UE_PermitLicenseIndicator = LicencePermitTypeList.Codes._21;
			InvoiceLine.AddInfoValidation.ValidateUS_MiscPermitNo();
			Assert(InvoiceLine.US_MiscPermitNoInfo.GetMessageErrors().GetFirstMessage().Contains("The USDA Sugar Certificate number is required."));
			InvoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Mexico;
			InvoiceLine.AddInfoValidation.ValidateUS_MiscPermitNo();
			AssertEquals(false, InvoiceLine.US_MiscPermitNoInfo.HasMessageErrors());
			InvoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.SouthAfrica;
			Declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			InvoiceLine.AddInfoValidation.ValidateUS_MiscPermitNo();
			AssertEquals(false, InvoiceLine.US_MiscPermitNoInfo.HasMessageErrors());
			Declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			InvoiceLine.AddInfoValidation.ValidateUS_MiscPermitNo();
			AssertEquals(false, InvoiceLine.US_MiscPermitNoInfo.HasMessageErrors());
			Declaration.US_EntryType = EntryTypeList.Codes.ReWarehouse;
			InvoiceLine.AddInfoValidation.ValidateUS_MiscPermitNo();
			AssertEquals(false, InvoiceLine.US_MiscPermitNoInfo.HasMessageErrors());
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			InvoiceLine.US_SupTariff = "98220515";
			InvoiceLine.AddInfoValidation.ValidateUS_MiscPermitNo();
			AssertEquals(false, InvoiceLine.US_MiscPermitNoInfo.HasMessageErrors());
			InvoiceLine.US_SupTariff = "98220520";
			InvoiceLine.AddInfoValidation.ValidateUS_MiscPermitNo();
			AssertEquals(false, InvoiceLine.US_MiscPermitNoInfo.HasMessageErrors());
			InvoiceLine.US_SupTariff = "";
			InvoiceLine.AddInfoValidation.ValidateUS_MiscPermitNo();
			Assert(InvoiceLine.US_MiscPermitNoInfo.GetMessageErrors().GetFirstMessage().Contains("The USDA Sugar Certificate number is required."));
			InvoiceLine.US_MiscPermitNo = "123456789";
			InvoiceLine.AddInfoValidation.ValidateUS_MiscPermitNo();
			AssertEquals(false, InvoiceLine.US_MiscPermitNoInfo.HasMessageErrors());
		}

		public void TestUSDASugarCertificate_ACE()
		{
			var expectedMessageError = "The USDA Sugar Certificate number is required.";
			var expectedWarning = string.Format(LicenceValidator.PermitPossiblyRequiredForSelectedTariff, "USDA Sugar Certificate");
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			InvoiceLine.Validation.ValidateJI_Tariff();
			AssertEquals(false, InvoiceLine.US_MiscPermitNoInfo.HasMessageError(expectedMessageError));
			AssertEquals(false, InvoiceLine.US_MiscPermitNoInfo.HasWarning(expectedWarning));
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_DateFrom = ZDateTime.Now.AddDays(-2);
			tariff.UE_DateTo = ZDateTime.Now.AddDays(2);
			InvoiceLine.JI_Tariff = tariff.UE_Tariff;
			InvoiceLine.Validation.ValidateJI_Tariff();
			AssertEquals(false, InvoiceLine.US_MiscPermitNoInfo.HasMessageError(expectedMessageError));
			AssertEquals(false, InvoiceLine.US_MiscPermitNoInfo.HasWarning(expectedWarning));
			tariff.UE_PermitLicenseIndicator = LicencePermitTypeList.Codes._21;
			InvoiceLine.Validation.ValidateJI_Tariff();
			AssertEquals(true, InvoiceLine.JI_TariffInfo.HasMessageError(expectedMessageError));
			AssertEquals(false, InvoiceLine.JI_TariffInfo.HasWarning(expectedWarning));
			InvoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Mexico;
			InvoiceLine.Validation.ValidateJI_Tariff();
			AssertEquals(false, InvoiceLine.JI_TariffInfo.HasMessageError(expectedMessageError));
			AssertEquals(true, InvoiceLine.JI_TariffInfo.HasWarning(expectedWarning));
			InvoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.SouthAfrica;
			Declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			InvoiceLine.Validation.ValidateJI_Tariff();
			AssertEquals(false, InvoiceLine.JI_TariffInfo.HasMessageError(expectedMessageError));
			AssertEquals(true, InvoiceLine.JI_TariffInfo.HasWarning(expectedWarning));
			Declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			InvoiceLine.Validation.ValidateJI_Tariff();
			AssertEquals(false, InvoiceLine.JI_TariffInfo.HasMessageError(expectedMessageError));
			AssertEquals(true, InvoiceLine.JI_TariffInfo.HasWarning(expectedWarning));
			Declaration.US_EntryType = EntryTypeList.Codes.ReWarehouse;
			InvoiceLine.Validation.ValidateJI_Tariff();
			AssertEquals(false, InvoiceLine.JI_TariffInfo.HasMessageError(expectedMessageError));
			AssertEquals(true, InvoiceLine.JI_TariffInfo.HasWarning(expectedWarning));
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			InvoiceLine.US_SupTariff = "98220515";
			InvoiceLine.AddInfoValidation.ValidateUS_MiscPermitNo();
			AssertEquals(false, InvoiceLine.JI_TariffInfo.HasMessageError(expectedMessageError));
			AssertEquals(true, InvoiceLine.JI_TariffInfo.HasWarning(expectedWarning));
			InvoiceLine.US_SupTariff = "98220520";
			InvoiceLine.AddInfoValidation.ValidateUS_MiscPermitNo();
			AssertEquals(false, InvoiceLine.JI_TariffInfo.HasMessageError(expectedMessageError));
			AssertEquals(true, InvoiceLine.JI_TariffInfo.HasWarning(expectedWarning));
			InvoiceLine.US_SupTariff = "";
			InvoiceLine.Validation.ValidateJI_Tariff();
			AssertEquals(true, InvoiceLine.JI_TariffInfo.HasMessageError(expectedMessageError));
			AssertEquals(false, InvoiceLine.JI_TariffInfo.HasWarning(expectedWarning));
			var sugarCert = InvoiceLine.LicenceAndPermits.AddNew();
			sugarCert.CY_Code = LicencePermitTypeList.Codes._21;
			sugarCert.CY_Data = "123456789";
			InvoiceLine.Validation.ValidateJI_Tariff();
			AssertEquals(false, InvoiceLine.JI_TariffInfo.HasMessageError(expectedMessageError));
			AssertEquals(false, InvoiceLine.JI_TariffInfo.HasWarning(expectedWarning));
			InvoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Mexico;
			InvoiceLine.Validation.ValidateJI_Tariff();
			AssertEquals(false, InvoiceLine.JI_TariffInfo.HasMessageError(expectedMessageError));
			AssertEquals(false, InvoiceLine.JI_TariffInfo.HasWarning(expectedWarning));
		}

		protected override string[] ValidNumbers => new string[] { "123456789", "AAAAAAAA9" };

		protected override string[] InvalidNumbers => new string[] { "12345 789", "12345678", "AAAAAAAA" };

		protected override string LicenseTypeCode => LicencePermitTypeList.Codes._21;

		protected override string LicenseTypeDescription => "USDA Sugar Certificate";

		protected override string FormatMask => @"\w{9}";

		protected override string FormatErrorText => "9 alpha-numeric characters";

		JobDeclaration Declaration => InvoiceLine.Declaration;
	}
}
