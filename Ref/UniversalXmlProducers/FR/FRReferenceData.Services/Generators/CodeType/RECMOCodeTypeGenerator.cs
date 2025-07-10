namespace CargoWise.RefDbRepo.FRReferenceData.Services.Generators.CodeType
{
	public class RECMOCodeTypeGenerator : RefCusCodeTypeGenerator
	{
		protected override string CodeType => "RECMO";
		internal protected override string DataGrouping => "DIE";
		internal protected override string Description => "Motivation for Rectification Request";
		protected override byte MaxLength => 5;
		protected override bool ReadOnly => true;
	}
}
