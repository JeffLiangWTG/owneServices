namespace CargoWise.RefDbRepo.ESReferenceData.Business;

public class CANEXCParser(IDateTimeProvider dateTimeProvider) : ExcisesParentParser(dateTimeProvider)
{
	protected override string applicabilityType => "CANEX";

	protected override string dataSource => Constants.DataSources.CANEXC;

	protected override string measureCode => Constants.Excises.CANEXCCode;
}
