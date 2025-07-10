using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class ConsolNumberGeneratorTarget : NumberGeneratorTarget
	{
		public override string NumberCustomisationLocation
		{
			get { return Res.GetString("4508dcb5-5ccf-4e33-a0e0-40e1988155e7", "Freight -> Consolidations -> Consol Number"); }
		}

		protected override BillOfLadingNumberCustomisation GetNumberCustomisationCore()
		{
			return Context.AccessRegistry(FreightConfigurationRegistry.Instance.ConsolNumberCustomisation);
		}

		protected override int GetMaxLengthCore()
		{
			return JobConsolSchema.JK_UniqueConsignRef.MaxLength;
		}

		protected override ZString GetNameCore()
		{
			return Res.GetString("2e5d0065-b00b-4dc1-9738-c878715b99a3", "consol number");
		}
	}
}
