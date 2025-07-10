using CargoWise.Types;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business
{
	public class ZAOutturnGateInOutJobNumberGeneratorTarget : NumberGeneratorTarget
	{
		#region Overrides of NumberGeneratorTarget

		protected override BillOfLadingNumberCustomisation GetNumberCustomisationCore()
		{
			return Context.AccessRegistry(ZACustomsRegistry.Instance.ZAOutturnGateInOutJobNumberCustomization);
		}

		protected override int GetMaxLengthCore()
		{
			return AsycudaManifestHeaderSchema.AMA_JobReference.MaxLength;
		}

		protected override ZString GetNameCore()
		{
			return "Outturn & Gate In/Out job number";
		}

		public override string NumberCustomisationLocation => ((IRegistryItemInternals)ZACustomsRegistry.Instance.ZAOutturnGateInOutJobNumberCustomization).Location;

		#endregion
	}
}
