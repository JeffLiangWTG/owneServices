using Enterprise.Freight.Integration;

namespace Enterprise.Freight.CarbonEmissions.Business
{
	public interface ICO2eRecalculationChecker
	{
		bool ShouldRecalculate(ICO2eCalculationSupporter supporter);
	}
}
