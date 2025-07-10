using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.ComponentModel;

namespace Enterprise.Freight.Forwarding.Business
{
	[ModuleID(Enterprise.ZArchitecture.Modules.ModuleId.JobShipment)]
	public class TopLevelForwardingShipmentCollection : TopLevelShipmentCollection
	{
		public TopLevelForwardingShipmentCollection(BusinessObjectCollection collectionToFilter, CommonConsol parentConsol)
			: base(collectionToFilter, parentConsol)
		{
		}

		public new ForwardingShipment this[int i]
		{
			get { return (ForwardingShipment)base[i]; }
		}

		public new ForwardingShipment AddNew()
		{
			return (ForwardingShipment)base.AddNew();
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(ForwardingShipment);
		}
	}
}
