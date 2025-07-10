using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class DutyOtherTaxFeeWrapperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestData()
		{
			IDutyOtherTaxFee dutyOtherTaxFee = new DutyOtherTaxFeeWrapper("A", "%", 20M);

			NUnit.Framework.Assert.That(dutyOtherTaxFee.MethodCode, NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(dutyOtherTaxFee.MethodOfCalculation, NUnit.Framework.Is.EqualTo("%").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(dutyOtherTaxFee.TaxRateNumeric, NUnit.Framework.Is.EqualTo(20M).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(dutyOtherTaxFee.TypeCode, NUnit.Framework.Is.EqualTo("A").Using(CustomComparers.TypeComparison));
			dutyOtherTaxFee = new DutyOtherTaxFeeWrapper("C", "A", 20M);
			NUnit.Framework.Assert.That(dutyOtherTaxFee.MethodCode, NUnit.Framework.Is.EqualTo("2").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(dutyOtherTaxFee.MethodOfCalculation, NUnit.Framework.Is.EqualTo("A").Using(CustomComparers.TypeComparison));
		}
	}
}
