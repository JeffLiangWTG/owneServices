namespace CargoWise.RefDbRepo.FRReferenceData.Services.Generators.CodeType
{
	public class AHIPCCodeTypeGenerator : RefCusCodeTypeGenerator
	{
		protected override string CodeType => "AHIPC";
		internal protected override string DataGrouping => UniversalDataHelper.Constants.DeltaIE;
		internal protected override string Description => "Inward Processing Conditions List for AdHoc Authorisations";
		protected override byte MaxLength => 2;
		protected override bool ReadOnly => true;
	}
}
