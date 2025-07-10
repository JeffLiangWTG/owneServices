using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.CFS.Business
{
	/// <summary>
	/// A collection of GatePassPackLine objects.
	/// </summary>
	public class GatePassPackLineCollection : CFSPackLineCollection
	{
		public GatePassPackLineCollection(GatePassShipment shipment, BusinessObjectFactory factory) : base(shipment, factory)
		{
		}

		public new GatePassPackLine this[int index]
		{
			get { return (GatePassPackLine)Elements[index]; }
		}

		public new GatePassPackLine AddNew()
		{
			return (GatePassPackLine)base.AddNew();
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(GatePassPackLine);
		}
	}
}
