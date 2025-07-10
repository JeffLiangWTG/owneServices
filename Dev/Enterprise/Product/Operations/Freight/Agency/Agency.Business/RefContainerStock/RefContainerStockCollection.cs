using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Agency.Business
{
	[ModuleID(ModuleId.AgencyContainerManager)]
	public class RefContainerStockCollection : ActiveBusinessObjectCollection<RefContainerStock>
	{
		public RefContainerStockCollection(BusinessObjectFactory factory)
			: base(factory) { }

		public RefContainerStockCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship) { }
	}
}
