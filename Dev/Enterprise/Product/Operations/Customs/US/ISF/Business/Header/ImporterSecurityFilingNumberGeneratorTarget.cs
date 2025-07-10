using CargoWise.Types;
using Enterprise.Customs.US.ISF.DataRegistry.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.ISF.Business
{
	public class ImporterSecurityFilingNumberGeneratorTarget : NumberGeneratorTarget
	{
		public override string NumberCustomisationLocation
		{
			get { return ((IRegistryItemInternals)ISFRegistry.Instance.ImporterSecurityFilingNumberCustomisation).Location; }
		}

		protected override BillOfLadingNumberCustomisation GetNumberCustomisationCore()
		{
			return Context.AccessRegistry(ISFRegistry.Instance.ImporterSecurityFilingNumberCustomisation);
		}

		protected override int GetMaxLengthCore()
		{
			return CusISFHeaderSchema.BF_JobReference.MaxLength;
		}

		protected override ZString GetNameCore()
		{
			return "Importer Security Filing number";
		}
	}
}
