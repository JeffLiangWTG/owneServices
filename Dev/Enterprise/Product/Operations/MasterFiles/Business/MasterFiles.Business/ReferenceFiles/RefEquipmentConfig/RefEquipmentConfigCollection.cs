using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class RefEquipmentConfigCollection : ActiveBusinessObjectCollection<RefEquipment>
	{
		public RefEquipmentConfigCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
	}
}
