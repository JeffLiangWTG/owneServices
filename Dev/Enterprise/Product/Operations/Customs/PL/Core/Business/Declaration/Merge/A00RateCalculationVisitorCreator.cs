using CargoWise.Common;
using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.PL.Business.Declaration;

public class A00RateCalculationVisitorCreator : IRateCalculationVisitorCreator
{
	public A00RateCalculationVisitorCreator(IRateCalculationVisitorCreator baseCreator)
	{
		this.baseCreator = Argument.NotNull(baseCreator, nameof(baseCreator));
	}

	readonly IRateCalculationVisitorCreator baseCreator;

	public IRateCalculationVisitor NewVisitor(IUniversalRateCalcData rateCalcData, FormulaErrorListener errorListener)
	{
		return new A00RateCalculationDecorator(baseCreator.NewVisitor(rateCalcData, errorListener), rateCalcData);
	}
}
