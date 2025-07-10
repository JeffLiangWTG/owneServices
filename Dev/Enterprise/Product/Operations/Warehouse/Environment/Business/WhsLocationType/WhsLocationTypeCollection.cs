using CargoWise.EntityFramework;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Environment.Business
{
	[ModuleID(ModuleId.WhsConfigLocationType)]
	public class WhsLocationTypeCollection : ActiveBusinessObjectCollection<WhsLocationType>, IWhsLocationTypeCollection
	{
		public WhsLocationTypeCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WhsLocationTypeCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}
	}
}

