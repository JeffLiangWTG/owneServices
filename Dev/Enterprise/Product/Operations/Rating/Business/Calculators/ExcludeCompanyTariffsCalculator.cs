using CargoWise.DataTransfer.Ratings;

namespace Enterprise.Rating.Business
{
	public class ExcludeCompanyTariffsCalculator : Calculator
	{
		public ExcludeCompanyTariffsCalculator(IRateLine master)
			: base(master)
		{
		}

		public const string Code = RatingCalculatorCodes.ExcludeCompanyTariffs;

		public override bool CanBePrintedUsing6StandardOperatorColumnHeaders => false;

		public override bool SupportsProductLineUnitFactor => true;

		public override bool IsMeasureTypeMatchApplicable => false;
	}
}

