namespace CargoWise.RefDbRepo.ESReferenceData.Business;

public class ESEXCParser(IDateTimeProvider dateTimeProvider) : ExcisesParentParser(dateTimeProvider)
{
	protected override string applicabilityType => "ESEXC";

	protected override string dataSource => Constants.DataSources.ESEXC;

	protected override string measureCode => Constants.Excises.ESEXCCode;
}
