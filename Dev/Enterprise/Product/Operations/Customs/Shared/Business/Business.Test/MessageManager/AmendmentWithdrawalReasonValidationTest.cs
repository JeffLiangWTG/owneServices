using CargoWise.ComponentModel;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	class AmendmentWithdrawalReasonValidationTest : TestCase
	{
		public void TestValidateReasonText()
		{
			AmendmentWithdrawalReason reason = GetAmendmentWithdrawalReason();
			reason.RunPreSaveValidation();
			AssertEquals("Should have validated", true, reason.ReasonTextInfo.HasErrors());
		}

		public void TestEmptyReasonIsAnError()
		{
			AmendmentWithdrawalReason reason = GetAmendmentWithdrawalReason();
			reason.ReasonText = "";
			AssertEquals("Should have validated", true, reason.ReasonTextInfo.HasErrors());

			reason.ReasonText = "BLAH";
			AssertEquals("Should have validated", false, reason.ReasonTextInfo.HasErrors());
		}

		protected virtual AmendmentWithdrawalReason GetAmendmentWithdrawalReason()
		{
			return new AmendmentWithdrawalReason();
		}
	}
}
