using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Freight.ContainerYard.Business
{
	public class GateTransportLookups : AutoGateTransportLookups
	{
		public GateTransportLookups(AutoGateTransport parent)
			: base(parent)
		{
		}

		#region Warehouses

		public virtual WhsWarehouseCollection Warehouses
		{
			get
			{
				return new WhsWarehouseCollectionWithSecurityCheck(Factory);
			}
		}

		#endregion
	}
}
