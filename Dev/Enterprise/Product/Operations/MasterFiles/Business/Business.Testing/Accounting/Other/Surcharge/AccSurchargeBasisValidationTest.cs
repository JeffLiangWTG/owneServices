using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccSurchargeBasisValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckASB_AC_ChargeCode()
		{
			var surchargeConfig = Factory.NewWithValidTestData<AccSurchargeConfiguration>();
			var surchargeBasis1 = surchargeConfig.AccSurchargeBasises.AddNew();
			var charge = Factory.NewWithValidTestData<AccChargeCode>();
			charge.AC_GC = surchargeConfig.ASC_GC_Company;

			var validation = new AccSurchargeBasisValidation(surchargeBasis1);
			validation.ValidateASB_AC_ChargeCode();

			AssertHasError("ASB_AC_ChargeCode has error message 'Please enter a value.'", surchargeBasis1.ASB_AC_ChargeCodeInfo, "Please enter a value.");

			surchargeBasis1.ASB_ChargeGroup = "FRT";
			surchargeBasis1.ASB_AC_ChargeCode = charge.PK;

			validation = new AccSurchargeBasisValidation(surchargeBasis1);
			validation.ValidateASB_AC_ChargeCode();

			AssertHasError("ASB_AC_ChargeCode has error message 'For each row, please specify a charge group or charge code only.'", surchargeBasis1.ASB_AC_ChargeCodeInfo, "For each row, please specify a charge group or charge code only.");

			surchargeBasis1.ASB_ChargeGroup = ZString.Empty;

			var surchargeBasis2 = surchargeConfig.AccSurchargeBasises.AddNew();
			surchargeBasis2.ASB_AC_ChargeCode = charge.PK;

			validation.ValidateASB_AC_ChargeCode();

			AssertHasError("ASB_AC_ChargeCode has error message 'Charge Code must be unique within a single Surcharge.'", surchargeBasis1.ASB_AC_ChargeCodeInfo, "Charge Code must be unique within a single Surcharge.");

			surchargeBasis2.ASB_AC_ChargeCode = ZGuid.Empty;
			surchargeBasis2.ASB_ChargeGroup = "TST";

			validation.ValidateASB_AC_ChargeCode();

			AssertNoErrors(surchargeBasis1.ASB_AC_ChargeCodeInfo);
		}

		public void TestCheckASB_AC_ChargeCode_ChargeCodeNotBelongToCurrentEditCompany()
		{
			var chargeNotInEditingCompany = Factory.NewWithValidTestData<AccChargeCode>();
			Factory.Save();

			var nonCurrentCompany = Factory.NewWithValidTestData<GlbCompany>();
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_GC = nonCurrentCompany.PK;

			var config = nonCurrentCompany.AccSurchargeConfigurations.AddNew();
			var basis = config.AccSurchargeBasises.AddNew();
			basis.ASB_ChargeGroup = "FRT";
			config.ASC_GC_Company = nonCurrentCompany.PK;

			basis.ASB_AC_ChargeCode = chargeNotInEditingCompany.PK;
			AssertHasError(basis.ASB_AC_ChargeCodeInfo, "Enter a valid selection.");

			basis.ASB_AC_ChargeCode = chargeCode.PK;
			AssertNoError(basis.ASB_AC_ChargeCodeInfo, "Enter a valid selection.");
		}

		public void TestCheckASB_ChargeGroup()
		{
			var charge = Factory.NewWithValidTestData<AccChargeCode>();
			var surchargeConfig = Factory.NewWithValidTestData<AccSurchargeConfiguration>();
			var surchargeBasis1 = surchargeConfig.AccSurchargeBasises.AddNew();

			var validation = new AccSurchargeBasisValidation(surchargeBasis1);
			validation.ValidateASB_ChargeGroup();

			AssertHasError("ASB_AC_ChargeCode has error message 'Please enter a value.'", surchargeBasis1.ASB_ChargeGroupInfo, "Please enter a value.");

			surchargeBasis1.ASB_ChargeGroup = "FRT";
			surchargeBasis1.ASB_AC_ChargeCode = charge.PK;

			validation = new AccSurchargeBasisValidation(surchargeBasis1);
			validation.ValidateASB_ChargeGroup();

			AssertHasError("ASB_ChargeGroup has error message 'For each row, please specify a charge group or charge code only.'", surchargeBasis1.ASB_ChargeGroupInfo, "For each row, please specify a charge group or charge code only.");

			surchargeBasis1.ASB_AC_ChargeCode = ZGuid.Empty;

			var surchargeBasis2 = surchargeConfig.AccSurchargeBasises.AddNew();
			surchargeBasis2.ASB_ChargeGroup = "FRT";

			validation.ValidateASB_ChargeGroup();

			AssertHasError("ASB_ChargeGroup has error message 'Charge Group must be unique within a single Surcharge.'", surchargeBasis1.ASB_ChargeGroupInfo, "Charge Group must be unique within a single Surcharge.");

			surchargeBasis2.ASB_ChargeGroup = "TST";

			validation.ValidateASB_ChargeGroup();

			AssertNoErrors(surchargeBasis1.ASB_ChargeGroupInfo);
		}
	}
}
