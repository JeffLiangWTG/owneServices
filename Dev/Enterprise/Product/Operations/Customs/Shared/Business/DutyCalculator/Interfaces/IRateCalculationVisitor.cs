using Enterprise.Customs.Universal;

namespace Enterprise.Customs.DutyCalculator;

public interface IRateCalculationVisitor
{
	RateFormulaResult VisitFullFormulaExpression(RateFormulaParser.ExpressionContext context);
}
