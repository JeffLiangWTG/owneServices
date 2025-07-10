
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.CFS.Business
{
	public class GatePassLoadListConsol : CFSLoadListConsol
	{
		public GatePassLoadListConsol(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Related Business Objects

		#region Shipments

		protected override ConsolShipmentCollection GetNewConsolShipmentCollection()
		{
			return new GatePassShipmentCollection(this);
		}

		public new GatePassShipmentCollection Shipments
		{
			get { return (GatePassShipmentCollection)base.Shipments; }
		}

		#endregion

		#region Containers

		public new GatePassContainerCollection Containers
		{
			get { return (GatePassContainerCollection)base.Containers; }
		}

		protected override CommonContainerCollection GetNewContainerCollection()
		{
			return new GatePassContainerCollection(this, Factory);
		}

		#endregion

		#endregion

	}
}
