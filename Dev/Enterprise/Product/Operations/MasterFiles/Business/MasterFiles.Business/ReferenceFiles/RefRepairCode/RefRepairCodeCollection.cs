using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class RefRepairCodeCollection : ActiveBusinessObjectCollection<RefRepairCode>, IRefRepairCodeCollection
	{
		public RefRepairCodeCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public RefRepairCodeCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public RefRepairCodeCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}
	}
}
