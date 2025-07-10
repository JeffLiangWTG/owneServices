using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class NX101InvoiceLineTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestProperties()
		{
			IInvoiceLine line = new NX101InvoiceLine("USD", 100m);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(line.CurrencyTypeCode, NUnit.Framework.Is.EqualTo("USD").Using(CustomComparers.TypeComparison), "CurrencyTypeCode");
				NUnit.Framework.Assert.That(line.ItemChargeAmount, NUnit.Framework.Is.EqualTo(100m).Using(CustomComparers.TypeComparison), "ItemChargeAmount");
			});
		}
	}
}
