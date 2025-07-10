using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class DutyTaxFeeAmountWrapperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestData()
		{
			IDutyTaxFeeAmount dutyTaxFeeAmount = new DutyTaxFeeAmountWrapper(10M);
			NUnit.Framework.Assert.That(dutyTaxFeeAmount.TaxRateNumeric, NUnit.Framework.Is.EqualTo(10M).Using(CustomComparers.TypeComparison));
		}
	}
}
