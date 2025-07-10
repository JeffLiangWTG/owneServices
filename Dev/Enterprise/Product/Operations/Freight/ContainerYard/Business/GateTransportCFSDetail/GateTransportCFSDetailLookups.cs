
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Freight.ContainerYard.Business
{
	public class GateTransportCFSDetailLookups : AutoGateTransportCFSDetailLookups
	{
		public GateTransportCFSDetailLookups(AutoGateTransportCFSDetail parent)
			: base(parent)
		{
		}

		#region Warehouses

		public virtual WhsWarehouseCollection Warehouses => new WhsWarehouseCollectionWithSecurityCheck(Factory);

		#endregion
	}
}
