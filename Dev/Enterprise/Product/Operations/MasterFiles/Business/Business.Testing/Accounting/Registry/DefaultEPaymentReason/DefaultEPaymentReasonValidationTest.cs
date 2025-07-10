using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class DefaultEPaymentReasonValidationTest : TestCaseWithFactory
	{
		public void TestValidateReasonCode()
		{
			var reasons = new DefaultEPaymentReasonCollection();
			var reason = reasons.AddNew();
			reason.ProviderCode = EPaymentProviderCodes.Codes.OFX;
			AssertEquals(ZString.Empty, reason.ReasonCode);
			reason.RunPreSaveValidation();
			AssertNull("User is allowed to leave reason code empty.", reason.ReasonCodeInfo.Notifications.FirstOrDefault(n => n.Message == "Please enter a Code."));
			reason.ReasonCode = "AAA";
			AssertHasError(reason.ReasonCodeInfo, "Enter a valid Code.");
			reason.ReasonCode = "SVT";
			AssertNoError(reason.ReasonCodeInfo, "Enter a valid Code.");
		}

		public void TestValidateProviderCode()
		{
			var reasons = new DefaultEPaymentReasonCollection();
			var reason = reasons.AddNew();
			reason.ProviderCode = ZString.Empty;
			AssertHasError(reason.ProviderCodeInfo, "Please enter a Provider.");
			reason.ProviderCode = "AAA";
			AssertHasError(reason.ProviderCodeInfo, "Enter a valid Provider.");
			reason.ProviderCode = EPaymentProviderCodes.Codes.OFX;
			AssertNoError(reason.ProviderCodeInfo, "Enter a valid Provider.");
			var duplicateReason = reasons.AddNew();
			duplicateReason.ProviderCode = EPaymentProviderCodes.Codes.OFX;
			AssertHasRowError(duplicateReason, "Only one default payment reason is allowed for a Provider.");
			duplicateReason.ProviderCode = "AAA";
			AssertNoRowError(duplicateReason, "Only one default payment reason is allowed for a Provider.");
		}
	}
}
