using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class EPaymentReasonValidationTest : TestCaseWithFactory
	{
		public void TestValidateProviderCode()
		{
			var reason = new EPaymentReason();
			AssertEquals(ZString.Empty, reason.ProviderCode);
			reason.RunPreSaveValidation();
			AssertHasError(reason.ProviderCodeInfo, "Please enter a Provider.");
			reason.ProviderCode = EPaymentProviderCodes.Codes.OFX;
			AssertNoErrors(reason.ProviderCodeInfo);
			reason.ProviderCode = "AAA";
			AssertHasError(reason.ProviderCodeInfo, "Enter a valid Provider.");
		}

		public void TestValidateReasonCode()
		{
			var reasons = new EPaymentReasonCollection();
			var reason1 = reasons.AddNew();
			reason1.ProviderCode = EPaymentProviderCodes.Codes.OFX;
			AssertEquals(ZString.Empty, reason1.ReasonCode);
			reason1.RunPreSaveValidation();
			AssertHasError(reason1.ReasonCodeInfo, "Please enter a Code.");
			reason1.ReasonCode = "AAA";
			AssertNoErrors(reason1.ReasonCodeInfo);

			var reason2 = reasons.AddNew();
			reason2.ProviderCode = EPaymentProviderCodes.Codes.OFX;
			reason2.ReasonCode = "AAA";
			AssertHasRowError(reason2, "At least one more payment reason already exist with the same Code.");
			reason2.ReasonCode = "PPP";
			AssertNoRowError(reason2, "At least one more payment reason already exist with the same Code.");
		}

		public void TestValidateReasonDescription()
		{
			var reason = new EPaymentReason();
			AssertEquals(ZString.Empty, reason.ReasonDescription);
			reason.RunPreSaveValidation();
			AssertHasError(reason.ReasonDescriptionInfo, "Please enter a Description.");
			reason.ReasonDescription = "Employee Salary";
			AssertNoErrors(reason.ReasonDescriptionInfo);
		}
	}
}
