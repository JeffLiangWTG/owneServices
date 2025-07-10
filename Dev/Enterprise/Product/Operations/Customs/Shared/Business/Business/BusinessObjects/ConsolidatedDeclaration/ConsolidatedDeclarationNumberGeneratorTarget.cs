using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	sealed class ConsolidatedDeclarationNumberGeneratorTarget : NumberGeneratorTarget
	{
		public override string NumberCustomisationLocation => Res.GetString("F186B05A-17AE-4336-98B9-14445FCECF53", "Customs -> Consolidated Entry Number Customization");

		protected override int GetMaxLengthCore() => JobDeclarationSchema.JE_DeclarationReference.MaxLength;

		protected override ZString GetNameCore() => Res.GetString("619A0E36-CAA3-4F10-B52B-3C276CE47E19", "Consolidated Entry Job Number");

		protected override BillOfLadingNumberCustomisation GetNumberCustomisationCore() => Context.AccessRegistry(CustomsDataRegistry.Instance.ConsolidatedEntryNumberCustomisation);
	}
}
