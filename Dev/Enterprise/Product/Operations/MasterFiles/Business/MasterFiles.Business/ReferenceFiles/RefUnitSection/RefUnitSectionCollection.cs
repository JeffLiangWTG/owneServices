using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class RefUnitSectionCollection : ActiveBusinessObjectCollection<RefUnitSection>, IRefUnitSectionCollection
	{
		public RefUnitSectionCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public RefUnitSectionCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public RefUnitSectionCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}
	}
}
