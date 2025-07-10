using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class DutyCalculationExhibitDocLine : CalculationExhibitDocLine
	{
		public DutyCalculationExhibitDocLine(IDutyCalculationExhibitsSupporter supporter)
			: base(supporter)
		{
			this.ExportValue = supporter.ExportValue;
			this.DutyRateInPercentage = supporter.DutyRate * 100;
		}

		public ZDecimal ExportValue { get; private set; }
		public ZDecimal DutyRateInPercentage { get; private set; }
	}
}
