using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.RefEquipment)]
	public class RefEquipmentCollection : ActiveBusinessObjectCollection<RefEquipment>
	{
		public RefEquipmentCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public RefEquipmentCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public RefEquipmentCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}
	}
}
