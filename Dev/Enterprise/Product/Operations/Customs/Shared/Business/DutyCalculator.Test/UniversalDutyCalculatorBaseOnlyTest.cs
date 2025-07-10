using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.DutyCalculator.Testing;

[TestedType(typeof(UniversalDutyCalculator<,>))]
sealed class UniversalDutyCalculatorBaseOnlyTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When entity is null",
			() => new UniversalDutyCalculatorForTest(entity: null,
				rateCalculationVisitorMode: RateCalculationVisitorMode.Default,
				createCalcDataFactory: (e, r) => new EntryLineUniversalRateCalcData(e, r)));

		AssertExceptionThrown<ArgumentNullException>("When createCalcDataFactory is null",
			() => new UniversalDutyCalculatorForTest(entity: Factory.New<CusEntryLine>(),
				rateCalculationVisitorMode: RateCalculationVisitorMode.Default,
				createCalcDataFactory: null));
	}

	class UniversalDutyCalculatorForTest : UniversalDutyCalculator<CusEntryLine, EntryLineUniversalRateCalcData>
	{
		public UniversalDutyCalculatorForTest(CusEntryLine entity, RateCalculationVisitorMode rateCalculationVisitorMode, Func<CusEntryLine, RateView, IUniversalRateCalcData> createCalcDataFactory)
			: base(entity, rateCalculationVisitorMode, createCalcDataFactory)
		{
		}
	}
}
