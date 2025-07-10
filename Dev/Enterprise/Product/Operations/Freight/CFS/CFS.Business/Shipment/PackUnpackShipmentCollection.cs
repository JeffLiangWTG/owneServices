using System;
using CargoWise.Types;

namespace Enterprise.Freight.CFS.Business
{
	public class PackUnpackShipmentCollection : CFSShipmentCollection
	{
		public PackUnpackShipmentCollection(PackUnpackLoadListConsol master) : base(master)
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

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(PackUnpackShipment);
		}
	}
}
