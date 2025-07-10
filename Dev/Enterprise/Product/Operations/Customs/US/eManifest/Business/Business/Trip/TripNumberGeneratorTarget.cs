using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.eManifest.Business
{
	class TripNumberGeneratorTarget : NumberGeneratorTarget
	{
		#region Overrides of NumberGeneratorTarget

		protected override BillOfLadingNumberCustomisation GetNumberCustomisationCore()
		{
			return Context.AccessRegistry(USeManifestDataRegistry.Instance.NumberCustomisation);
		}

		protected override int GetMaxLengthCore()
		{
			return CusInBondHeaderSchema.BH_JobReference.MaxLength;
		}

		protected override ZString GetNameCore()
		{
			return "e-Manifest number";
		}

		public override string NumberCustomisationLocation
		{
			get { return ((IRegistryItemInternals)USeManifestDataRegistry.Instance.NumberCustomisation).Location; }
		}

		#endregion
	}
}
