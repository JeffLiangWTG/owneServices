using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.CFS.Business
{
	[ModuleID(ModuleId.ShipmentReceival)]
	public class CFSModuleShipmentCollection : ModuleShipmentCollection
	{
		public CFSModuleShipmentCollection(BusinessObjectFactory factory)
			: base(factory)
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
