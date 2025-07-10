using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class DeclarationNumberGeneratorTarget : NumberGeneratorTarget
	{
		public override string NumberCustomisationLocation
		{
			get { return Res.GetString("48c002b0-46ae-4030-8777-c431a5283770", "Customs -> Declaration Number Customization"); }
		}

		protected override BillOfLadingNumberCustomisation GetNumberCustomisationCore()
		{
			return Context.AccessRegistry(CustomsDataRegistry.Instance.DeclarationNumberCustomisation);
		}

		protected override int GetMaxLengthCore()
		{
			return JobDeclarationSchema.JE_DeclarationReference.MaxLength;
		}

		protected override ZString GetNameCore()
		{
			return Res.GetString("7b2b687c-56a6-4fd3-a36d-ad85e78dd504", "Declaration number");
		}
	}
}
