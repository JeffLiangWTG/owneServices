using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccSurchargeConfigurationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckASC_Code()
		{
			var surchargeConfiguration1 = GlbCompany.CurrentCompany.AccSurchargeConfigurations.AddNew();
			surchargeConfiguration1.ASC_Code = "TST";

			var surchargeConfiguration2 = GlbCompany.CurrentCompany.AccSurchargeConfigurations.AddNew();
			surchargeConfiguration2.ASC_Code = "TST";

			var validation = new AccSurchargeConfigurationValidation(surchargeConfiguration1);
			validation.ValidateASC_Code();

			AssertHasError("ASC_Code has error message 'Code must be unique within a single company'", surchargeConfiguration1.ASC_CodeInfo, "Code must be unique within a single company");

			var surchargeConfiguration3 = Factory.NewWithValidTestData<AccSurchargeConfiguration>();
			surchargeConfiguration3.ASC_Code = "NON";

			validation = new AccSurchargeConfigurationValidation(surchargeConfiguration3);
			validation.ValidateASC_Code();

			AssertHasError("ASC_Code has error message 'Please specify a code other than 'ALL' and 'NON' which is reserved by system to identify 'All surcharges to be applied' or 'No surcharges to be applied'.'", surchargeConfiguration3.ASC_CodeInfo, "Please specify a code other than 'ALL' and 'NON' which is reserved by system to identify 'All surcharges to be applied' or 'No surcharges to be applied'.");

			var surchargeConfiguration4 = Factory.NewWithValidTestData<AccSurchargeConfiguration>();
			surchargeConfiguration4.ASC_Code = "ALL";

			validation = new AccSurchargeConfigurationValidation(surchargeConfiguration4);
			validation.ValidateASC_Code();

			AssertHasError("ASC_Code has error message 'Please specify a code other than 'ALL' and 'NON' which is reserved by system to identify 'All surcharges to be applied' or 'No surcharges to be applied'.'", surchargeConfiguration4.ASC_CodeInfo, "Please specify a code other than 'ALL' and 'NON' which is reserved by system to identify 'All surcharges to be applied' or 'No surcharges to be applied'.");

			var surchargeConfiguration5 = Factory.NewWithValidTestData<AccSurchargeConfiguration>();
			surchargeConfiguration5.ASC_Code = "TS2";

			validation = new AccSurchargeConfigurationValidation(surchargeConfiguration5);
			validation.ValidateASC_Code();

			AssertNoErrors(surchargeConfiguration5.ASC_CodeInfo);
		}

		public void TestCheckASC_Description()
		{
			var surchargeConfiguration = Factory.NewWithValidTestData<AccSurchargeConfiguration>();
			surchargeConfiguration.ASC_Description = string.Empty;

			var validation = new AccSurchargeConfigurationValidation(surchargeConfiguration);
			validation.ValidateASC_Description();

			AssertHasError("ASC_Description has error message 'Please enter a value.'", surchargeConfiguration.ASC_DescriptionInfo, "Please enter a value.");

			surchargeConfiguration.ASC_Description = "Test";

			validation.ValidateASC_Description();

			AssertNoErrors(surchargeConfiguration.ASC_DescriptionInfo);
		}

		public void TestCheckASC_Rate()
		{
			var surchargeConfiguration = Factory.NewWithValidTestData<AccSurchargeConfiguration>();
			surchargeConfiguration.ASC_Rate = 0;

			var validation = new AccSurchargeConfigurationValidation(surchargeConfiguration);
			validation.ValidateASC_Rate();

			AssertHasError("ASC_Rate has error message 'Surcharge Percentage should be greater than 0.'", surchargeConfiguration.ASC_RateInfo, "Surcharge Percentage should be greater than 0.");

			surchargeConfiguration.ASC_Rate = -1;
			validation.ValidateASC_Rate();

			AssertHasError("ASC_Rate has error message 'Surcharge Percentage should be greater than 0.'", surchargeConfiguration.ASC_RateInfo, "Surcharge Percentage should be greater than 0.");

			surchargeConfiguration.ASC_Rate = 1;
			validation.ValidateASC_Rate();

			AssertNoErrors(surchargeConfiguration.ASC_RateInfo);
		}

		public void TestCheckASC_BasisType()
		{
			var surchargeConfiguration = Factory.NewWithValidTestData<AccSurchargeConfiguration>();
			surchargeConfiguration.ASC_BasisType = string.Empty;

			var validation = new AccSurchargeConfigurationValidation(surchargeConfiguration);
			validation.ValidateASC_BasisType();

			AssertHasError("ASC_BasisType has error message 'Please enter a value.'", surchargeConfiguration.ASC_BasisTypeInfo, "Please enter a value.");

			surchargeConfiguration.ASC_BasisType = SurchargeBasisTypeList.Codes.EXC;
			validation.ValidateASC_BasisType();

			AssertHasError("ASC_BasisType has error message 'Please specify at least a charge group or a charge code.'", surchargeConfiguration.ASC_BasisTypeInfo, "Please specify at least a charge group or a charge code.");

			var surchargeBasis = surchargeConfiguration.AccSurchargeBasises.AddNew();
			surchargeBasis.ASB_ChargeGroup = "FRT";

			validation.ValidateASC_BasisType();

			AssertNoErrors(surchargeConfiguration.ASC_BasisTypeInfo);
		}

		public void TestCheckASC_AC_ChargeCode()
		{
			var chargeCodeNotInCurrentEditCompany = Factory.NewWithValidTestData<AccChargeCode>();
			Factory.Save();

			var nonCurrentCompany = Factory.NewWithValidTestData<GlbCompany>();
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_GC = nonCurrentCompany.PK;

			var config = nonCurrentCompany.AccSurchargeConfigurations.AddNew();
			config.ASC_GC_Company = nonCurrentCompany.PK;

			config.ASC_AC_ChargeCode = chargeCodeNotInCurrentEditCompany.PK;
			AssertHasError(config.ASC_AC_ChargeCodeInfo, "Enter a valid selection.");

			config.ASC_AC_ChargeCode = chargeCode.PK;
			AssertNoError(config.ASC_AC_ChargeCodeInfo, "Enter a valid selection.");
		}
	}
}
