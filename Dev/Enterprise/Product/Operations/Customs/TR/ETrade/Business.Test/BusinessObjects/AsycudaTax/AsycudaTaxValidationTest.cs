using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.TR.ETrade.Business.Testing
{
	public class AsycudaTaxValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAET_ChargeType()
		{
			var asycudaTax = SetupTestEnvironmentForAsycudaTax();

			asycudaTax.AET_ChargeType = "9";
			asycudaTax.Validation.ValidateAET_ChargeType();
			AssertHasMessageErrorContaining(asycudaTax.AET_ChargeTypeInfo, ListValidation.InvalidCodeMessageError);
			asycudaTax.AET_ChargeType = TaxCodeList.Codes.CustomsDuty;
			asycudaTax.Validation.ValidateAET_ChargeType();
			AssertNoMessageErrors(asycudaTax.AET_ChargeTypeInfo);
		}

		public void TestCheckAET_MethodOfPayment()
		{
			var asycudaTax = SetupTestEnvironmentForAsycudaTax();

			asycudaTax.AET_MethodOfPayment = "ABC";
			asycudaTax.Validation.ValidateAET_MethodOfPayment();
			AssertHasMessageErrorContaining(asycudaTax.AET_MethodOfPaymentInfo, ListValidation.InvalidCodeMessageError);
			asycudaTax.AET_MethodOfPayment = MethodOfPaymentList.Codes.Cash;
			asycudaTax.Validation.ValidateAET_ChargeType();
			AssertNoMessageErrors(asycudaTax.AET_MethodOfPaymentInfo);
		}

		public void TestCheckAET_RateOverrideReasonCode()
		{
			var asycudaTax = SetupTestEnvironmentForAsycudaTax();

			asycudaTax.AET_RateOverrideReasonCode = ZString.Empty;
			AssertNoNotifications(asycudaTax.AET_RateOverrideReasonCodeInfo);

			asycudaTax.AET_RateOverrideReasonCode = RateOverrideReasonCodeList.Codes.Additional;
			AssertNoNotifications(asycudaTax.AET_RateOverrideReasonCodeInfo);

			asycudaTax.AET_RateOverrideReasonCode = "ABC";
			AssertHasMessageErrorContaining(asycudaTax.AET_RateOverrideReasonCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		AsycudaTax SetupTestEnvironmentForAsycudaTax()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			return bill.AsycudaTaxes.AddNew();
		}
	}
}
