using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Universal.Testing
{
	class ZZRefCusConfigurationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZZC_CustomsValueCodeForExport_Entered()
		{
			CombineAssertions(() =>
			{
				config.ZZC_CustomsValueCodeForExport = ZString.Empty;
				AssertHasErrorContaining("Empty", config.ZZC_CustomsValueCodeForExportInfo, MandatoryValidation.MustBeEntered);
				config.ZZC_CustomsValueCodeForExport = CustomsValueCodeList.Codes.CIF;
				AssertNoErrorContaining("Entered", config.ZZC_CustomsValueCodeForExportInfo, MandatoryValidation.MustBeEntered);
			});
		}

		public void TestCheckZZC_CustomsValueCodeForExport_ValidCode()
		{
			CombineAssertions(() =>
			{
				config.ZZC_CustomsValueCodeForExport = "X";
				AssertHasErrorContaining("Invalid", config.ZZC_CustomsValueCodeForExportInfo, ListValidation.InvalidCodeError);
				config.ZZC_CustomsValueCodeForExport = CustomsValueCodeList.Codes.CIF;
				AssertNoErrorContaining("Valid", config.ZZC_CustomsValueCodeForExportInfo, ListValidation.InvalidCodeError);
			});
		}

		public void TestCheckZZC_DefaultExportValuationDate()
		{
			CombineAssertions(() =>
			{
				config.ZZC_DefaultExportValuationDate = ZString.Empty;
				AssertNoErrorContaining("Empty", config.ZZC_DefaultExportValuationDateInfo, ListValidation.InvalidCodeError);
				config.ZZC_DefaultExportValuationDate = "X";
				AssertHasErrorContaining("Invalid", config.ZZC_DefaultExportValuationDateInfo, ListValidation.InvalidCodeError);
				config.ZZC_DefaultExportValuationDate = ValuationDateExportDefaultTypeList.Codes.DOE;
				AssertNoErrorContaining("Valid", config.ZZC_DefaultExportValuationDateInfo, ListValidation.InvalidCodeError);
			});
		}

		public void TestCheckZZC_DefaultImportValuationDate()
		{
			CombineAssertions(() =>
			{
				config.ZZC_DefaultImportValuationDate = ZString.Empty;
				AssertNoErrorContaining("Empty", config.ZZC_DefaultImportValuationDateInfo, ListValidation.InvalidCodeError);
				config.ZZC_DefaultImportValuationDate = "X";
				AssertHasErrorContaining("Invalid", config.ZZC_DefaultImportValuationDateInfo, ListValidation.InvalidCodeError);
				config.ZZC_DefaultImportValuationDate = ValuationDateDefaultTypeList.Codes.DOA;
				AssertNoErrorContaining("Valid", config.ZZC_DefaultImportValuationDateInfo, ListValidation.InvalidCodeError);
			});
		}

		public void TestCheckZZC_IsReciprocalExchangeRate()
		{
			CombineAssertions(() =>
			{
				config.ZZC_IsReciprocalExchangeRate = IsReciprocalExchangeRateList.Codes.Yes;
				AssertNoErrorContaining("'Y' is a valid option", config.ZZC_IsReciprocalExchangeRateInfo, ListValidation.InvalidCodeError);
				config.ZZC_IsReciprocalExchangeRate = IsReciprocalExchangeRateList.Codes.No;
				AssertNoErrorContaining("'N' is a valid option", config.ZZC_IsReciprocalExchangeRateInfo, ListValidation.InvalidCodeError);
				config.ZZC_IsReciprocalExchangeRate = IsReciprocalExchangeRateList.Codes.Blank;
				AssertNoErrorContaining("Blank is a valid option", config.ZZC_IsReciprocalExchangeRateInfo, ListValidation.InvalidCodeError);
				config.ZZC_IsReciprocalExchangeRate = "T";
				AssertHasErrorContaining("Test invalid option", config.ZZC_IsReciprocalExchangeRateInfo, ListValidation.InvalidCodeError);
			});
		}

		public void TestCheckZZC_CustomsValueCode()
		{
			const string customsValueError = "When the VAT Value Code is FOB then Customs Value Code must be FOB";
			CombineAssertions(() =>
			{
				config.ZZC_CustomsValueCode = CustomsValueCodeList.Codes.FOB;
				AssertNoErrorContaining("'FOB' is a valid option", config.ZZC_CustomsValueCodeInfo, ListValidation.InvalidCodeError);
				config.ZZC_CustomsValueCode = CustomsValueCodeList.Codes.CIF;
				AssertNoErrorContaining("'CIF' is a valid option", config.ZZC_CustomsValueCodeInfo, ListValidation.InvalidCodeError);
				config.ZZC_CustomsValueCode = ZString.Empty;
				AssertHasErrorContaining("Blank is invalid", config.ZZC_CustomsValueCodeInfo, MandatoryValidation.MustBeEntered);
				config.ZZC_CustomsValueCode = "CFR";
				AssertHasErrorContaining("'CFR' is an invalid option", config.ZZC_CustomsValueCodeInfo, ListValidation.InvalidCodeError);
				config.ZZC_VATValueCode = CustomsValueCodeList.Codes.FOB;
				config.ZZC_CustomsValueCode = CustomsValueCodeList.Codes.CIF;
				AssertHasErrorContaining("Customs Value code is not FOB", config.ZZC_CustomsValueCodeInfo, customsValueError);
				config.ZZC_CustomsValueCode = CustomsValueCodeList.Codes.FOB;
				AssertNoErrorContaining("Customs Value code must be FOB", config.ZZC_CustomsValueCodeInfo, customsValueError);
			});
		}

		public void TestCheckZZC_VATValueCode()
		{
			CombineAssertions(() =>
			{
				config.ZZC_VATValueCode = VATValueCodeList.Codes.FOB;
				AssertNoErrorContaining("'FOB' is a valid option", config.ZZC_VATValueCodeInfo, ListValidation.InvalidCodeError);
				config.ZZC_VATValueCode = VATValueCodeList.Codes.CIF;
				AssertNoErrorContaining("'CIF' is a valid option", config.ZZC_VATValueCodeInfo, ListValidation.InvalidCodeError);
				config.ZZC_VATValueCode = ZString.Empty;
				AssertHasErrorContaining("Blank is invalid", config.ZZC_VATValueCodeInfo, MandatoryValidation.MustBeEntered);
				config.ZZC_VATValueCode = "CFR";
				AssertHasErrorContaining("'CFR' is an invalid option", config.ZZC_VATValueCodeInfo, ListValidation.InvalidCodeError);
			});
		}

		protected override void SetUp()
		{
			config = Factory.New<ZZRefCusConfiguration>();
			base.SetUp();
		}

		ZZRefCusConfiguration config;
	}
}
