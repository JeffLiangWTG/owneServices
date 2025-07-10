using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.DutyCalculator.Testing;

[TestsSubclassesOf(typeof(UniversalDutyCalculator<,>))]
public abstract class UniversalDutyCalculatorAbstractTest<TEntity, TUniversalRateCalcData> : TestCaseWithFactory
	where TEntity : BusinessObject
	where TUniversalRateCalcData : IUniversalRateCalcData
{
	public void TestUniversalRateCalcDataType()
	{
		var dutyCalculator = CreateDutyCalculator();
		var rateCalcData = dutyCalculator.CreateUniversalRateCalcData(Factory.New<RateView>());
		AssertNotNull("IUniversalRateCalcData", rateCalcData);
		AssertType("IUniversalRateCalcData type", ExpectedUniversalRateCalDataType, rateCalcData);
	}

	protected abstract UniversalDutyCalculator<TEntity, TUniversalRateCalcData> CreateDutyCalculator();

	protected virtual Type ExpectedUniversalRateCalDataType => typeof(TUniversalRateCalcData);
}
