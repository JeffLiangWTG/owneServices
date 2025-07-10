using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class NettingSystemCollection : ActiveBusinessObjectCollection<NettingSystem>
	{
		public NettingSystemCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public NettingSystemCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public NettingSystemCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}
	}
}
