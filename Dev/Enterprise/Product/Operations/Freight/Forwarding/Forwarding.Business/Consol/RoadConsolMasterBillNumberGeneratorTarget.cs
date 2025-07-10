using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class RoadConsolMasterBillNumberGeneratorTarget : NumberGeneratorTarget
	{
		public override string NumberCustomisationLocation => ((IRegistryItemInternals)FreightConfigurationRegistry.Instance.RoadConsolMasterBillNumber).Location;

		protected override BillOfLadingNumberCustomisation GetNumberCustomisationCore()
		{
			var billOfLadingNumberCustomisation = Context.AccessRegistry(FreightConfigurationRegistry.Instance.RoadConsolMasterBillNumber);

			return billOfLadingNumberCustomisation;
		}

		protected override int GetMaxLengthCore()
		{
			return JobConsolSchema.JK_MasterBillNum.MaxLength;
		}

		protected override ZString GetNameCore()
		{
			return Res.GetString("fa82cfcf-f617-4e21-86b1-5bc2fc818bae", "master bill number");
		}
	}
}
