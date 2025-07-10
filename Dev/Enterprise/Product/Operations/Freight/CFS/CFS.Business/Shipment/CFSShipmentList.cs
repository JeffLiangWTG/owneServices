
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.CFS.Business
{
	/// <summary>
	/// Summary description for CFSShipmentList.
	/// </summary>
	public class CFSShipmentList : ShipmentCollection
	{
		public CFSShipmentList(BusinessObjectFactory factory) : base(factory)
		{
		}

		public CFSShipmentList(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public new CFSShipment this[int index]
		{
			get { return (CFSShipment)Elements[index]; }
		}

		public new CFSShipment AddNew()
		{
			return (CFSShipment)base.AddNew();
		}
	}
}
