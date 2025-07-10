using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class RefMachineryMakeCollection : ActiveBusinessObjectCollection<RefMachineryMake>, IRefMachineryMakeCollection
	{
		public RefMachineryMakeCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public RefMachineryMakeCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public RefMachineryMakeCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}
	}
}
