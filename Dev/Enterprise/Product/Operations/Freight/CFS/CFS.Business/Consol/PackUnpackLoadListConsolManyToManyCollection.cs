using System;
using CargoWise.Types;

namespace Enterprise.Freight.CFS.Business
{
	public class PackUnpackLoadListConsolManyToManyCollection : CFSLoadListConsolManyToManyCollection
	{
		public PackUnpackLoadListConsolManyToManyCollection(PackUnpackShipment shipment) : base(shipment)
		{
		}

		public new PackUnpackLoadListConsol this[int index]
		{
			get { return (PackUnpackLoadListConsol)(Elements[index]); }
		}

		public new PackUnpackLoadListConsol AddNew()
		{
			return (PackUnpackLoadListConsol)base.AddNew();
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(PackUnpackLoadListConsol);
		}
	}
}
