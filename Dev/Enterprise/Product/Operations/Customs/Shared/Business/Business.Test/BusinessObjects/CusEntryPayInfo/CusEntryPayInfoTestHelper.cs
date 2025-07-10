using CargoWise.Types;
using static NUnit.Framework.Assertion;

namespace Enterprise.Customs.Business.Testing
{
	public static class CusEntryPayInfoTestHelper
	{
		public static void AssertEntryPayInfo(this CusEntryPayInfo entryPayInfo, ZString incomingPayResponseNo, ZString paymentParty, ZDecimal paymentAmount, ZString declarationRegistry, ZDateTime expirationDate, ZString paymentStatus)
		{
			AssertEquals("C9_IncomingPayResponseNo", incomingPayResponseNo, entryPayInfo.C9_IncomingPayResponseNo);
			AssertEquals("C9_PaymentParty", paymentParty, entryPayInfo.C9_PaymentParty);
			AssertEquals("C9_PaymentAmount", paymentAmount, entryPayInfo.C9_PaymentAmount);
			AssertEquals("C9_TransactionType", declarationRegistry, entryPayInfo.C9_TransactionType);
			AssertEquals("C9_PaymentDate", expirationDate, entryPayInfo.C9_PaymentDate);
			AssertEquals("C9_PaymentStatus", paymentStatus, entryPayInfo.C9_PaymentStatus);
		}
	}
}
