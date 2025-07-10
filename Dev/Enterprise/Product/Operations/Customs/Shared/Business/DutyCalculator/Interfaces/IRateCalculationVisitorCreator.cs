using Enterprise.Customs.Universal;

namespace Enterprise.Customs.DutyCalculator;

public interface IRateCalculationVisitorCreator
{
	IRateCalculationVisitor NewVisitor(IUniversalRateCalcData rateCalcData, FormulaErrorListener errorListener);
}
