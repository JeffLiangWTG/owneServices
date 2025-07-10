using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class DutyTaxFeeQuantityWrapperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestData()
		{
			IDutyTaxFeeQuantity dutyTaxFeeQuantityWrapper = new DutyTaxFeeQuantityWrapper("A", 20M);
			NUnit.Framework.Assert.That(dutyTaxFeeQuantityWrapper.DutyUnitCode, NUnit.Framework.Is.EqualTo("A").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(dutyTaxFeeQuantityWrapper.TaxRateNumeric, NUnit.Framework.Is.EqualTo(20M).Using(CustomComparers.TypeComparison));
		}
	}
}
