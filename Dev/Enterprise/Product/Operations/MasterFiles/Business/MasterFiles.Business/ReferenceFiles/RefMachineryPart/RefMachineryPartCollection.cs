using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class RefMachineryPartCollection : ActiveBusinessObjectCollection<RefMachineryPart>, IRefMachineryPartCollection
	{
		public RefMachineryPartCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public RefMachineryPartCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public RefMachineryPartCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}
	}
}
