namespace CargoWise.RefDbRepo.FRReferenceData.Services.Generators.CodeType
{
	public class CL214IMCodeTypeGenerator : RefCusCodeTypeGenerator
	{
		protected override string CodeType => "214IM";

		internal protected override string DataGrouping => "FR";

		internal protected override string Description => "Previous Document Type (UCC6 Import)";

		protected override byte MaxLength => 4;

		protected override bool ReadOnly => true;
	}
}
