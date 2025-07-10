namespace CargoWise.RefDbRepo.FRReferenceData.Services.Generators.CodeType
{
	public class AHCONCodeTypeGenerator : RefCusCodeTypeGenerator
	{
		protected override string CodeType => "AHCON";
		internal protected override string DataGrouping => UniversalDataHelper.Constants.DeltaIE;
		internal protected override string Description => "Legal Conditions List for AdHoc Authorisations";
		protected override byte MaxLength => 2;
		protected override bool ReadOnly => true;
	}
}
