
using CargoWise.EntityFramework;

namespace Enterprise.Freight.CFS.Business
{
	public class PackUnpackShipmentList : CFSShipmentList
	{
		public PackUnpackShipmentList(BusinessObjectFactory factory) : base(factory)
		{
		}

		public PackUnpackShipmentList(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public new PackUnpackShipment this[int index]
		{
			get { return (PackUnpackShipment)Elements[index]; }
		}

		public new PackUnpackShipment AddNew()
		{
			return (PackUnpackShipment)base.AddNew();
		}
	}
}
