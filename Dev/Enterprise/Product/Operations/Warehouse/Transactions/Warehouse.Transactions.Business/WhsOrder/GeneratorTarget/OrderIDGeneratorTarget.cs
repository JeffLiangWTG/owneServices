using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	class OrderIDGeneratorTarget : NumberGeneratorTarget
	{
		public OrderIDGeneratorTarget(WhsOrder order)
		{
			this.order = order;
		}

		readonly WhsOrder order;

		public override string NumberCustomisationLocation => ((IRegistryItemInternals)WarehouseDataRegistry.Instance.WarehouseNumberCustomisation_Order).Location;

		protected override int GetMaxLengthCore() => WhsDocketSchema.WD_DocketID.MaxLength;

		protected override ZString GetNameCore() => (NoResString)"Order ID";

		protected override BillOfLadingNumberCustomisation GetNumberCustomisationCore()
		{
			var orderNumberCustomisationsByServiceLevel = Context.AccessRegistryByServiceLevel(WarehouseDataRegistry.Instance.WarehouseNumberCustomisation_Order);
			return orderNumberCustomisationsByServiceLevel.BillOfLadingNumberCustomisations[order.WD_RS_NKServiceLevel]
				?? orderNumberCustomisationsByServiceLevel.BillOfLadingNumberCustomisations[OrgCarrierServiceLevel.AllCode];
		}
	}
}
