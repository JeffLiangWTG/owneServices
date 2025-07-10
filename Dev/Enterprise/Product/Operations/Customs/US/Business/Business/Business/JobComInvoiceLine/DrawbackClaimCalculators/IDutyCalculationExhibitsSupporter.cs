using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public interface IDutyCalculationExhibitsSupporter : ICalculationExhibitsSupporter
	{
		ZDecimal ExportValue { get; }
		ZDecimal DutyRate { get; }
	}
}
