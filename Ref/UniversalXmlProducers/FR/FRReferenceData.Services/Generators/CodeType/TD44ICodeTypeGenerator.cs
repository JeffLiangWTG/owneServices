namespace CargoWise.RefDbRepo.FRReferenceData.Services.Generators.CodeType
{
	public class TD44ICodeTypeGenerator : RefCusCodeTypeGenerator
	{
		protected override string CodeType => "TD44I";
		internal protected override string DataGrouping => "DIE";
		internal protected override string Description => "Import Transport Document Type";
		protected override byte MaxLength => 4;
		protected override bool ReadOnly => true;
	}
}
