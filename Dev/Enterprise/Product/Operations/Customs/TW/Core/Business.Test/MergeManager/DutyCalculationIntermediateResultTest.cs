using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class DutyCalculationIntermediateResultTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestData()
		{
			var result = new DutyCalculationIntermediateResult();
			NUnit.Framework.Assert.That(result.Amount, NUnit.Framework.Is.EqualTo(0m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(result.Rate, NUnit.Framework.Is.EqualTo(0m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(result.RateCode, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(result.PaymentMethod, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(result.UnitOfCalculation, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(result.TypeCode, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
		}
	}
}
