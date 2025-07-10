using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	class ReceiveIDGeneratorTarget : NumberGeneratorTarget
	{
		public ReceiveIDGeneratorTarget(WhsReceive receive)
		{
			this.receive = receive;
		}

		readonly WhsReceive receive;

		public override string NumberCustomisationLocation => ((IRegistryItemInternals)WarehouseDataRegistry.Instance.WarehouseNumberCustomisation_Receive).Location;

		protected override int GetMaxLengthCore() => WhsDocketSchema.WD_DocketID.MaxLength;

		protected override ZString GetNameCore() => (NoResString)"Receive ID";

		protected override BillOfLadingNumberCustomisation GetNumberCustomisationCore()
		{
			var receiveNumberCustomisationsByServiceLevel = Context.AccessRegistryByServiceLevel(WarehouseDataRegistry.Instance.WarehouseNumberCustomisation_Receive);
			return receiveNumberCustomisationsByServiceLevel.BillOfLadingNumberCustomisations[receive.WD_RS_NKServiceLevel]
				?? receiveNumberCustomisationsByServiceLevel.BillOfLadingNumberCustomisations[OrgCarrierServiceLevel.AllCode];
		}
	}
}
