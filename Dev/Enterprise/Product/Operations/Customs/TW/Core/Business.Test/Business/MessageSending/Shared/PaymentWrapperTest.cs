using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(PaymentWrapper))]
	sealed class PaymentWrapperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestPaymentWrapper()
		{
			IPayment payment = new PaymentWrapper("7", "123");
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(payment.MethodCode, NUnit.Framework.Is.EqualTo("7").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(payment.ReferenceID, NUnit.Framework.Is.EqualTo("123").Using(CustomComparers.TypeComparison));
			});
		}
	}
}
