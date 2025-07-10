namespace CargoWise.RefDbRepo.FRReferenceData.Services;

public class TAXCodeTypeGenerator : RefCusCodeTypeGenerator
{
	protected override string CodeType => "TAX";

	protected override bool ReadOnly => true;

	protected override byte MaxLength => 4;

	protected internal override string DataGrouping => "FR";

	protected internal override string Description => "French National Tax Code";
}
