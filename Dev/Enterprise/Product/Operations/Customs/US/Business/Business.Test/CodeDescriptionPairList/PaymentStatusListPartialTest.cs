using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class PaymentStatusListTest : TestCase
	{
		public void TestGetCodeDescriptionPairList()
		{
			DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider list = new PaymentStatusList();
			AssertEquals(list, list.GetCodeDescriptionPairList());

			AssertEquals(true, PaymentStatusList.IsPaidOrPaymentInProgress(PaymentStatusList.Codes.PaymentAuthorizationAccepted));
			AssertEquals(true, PaymentStatusList.IsPaidOrPaymentInProgress(PaymentStatusList.Codes.PaymentInProgress));
			AssertEquals(false, PaymentStatusList.IsPaidOrPaymentInProgress(PaymentStatusList.Codes.PaymentFailed));
		}
	}
}
