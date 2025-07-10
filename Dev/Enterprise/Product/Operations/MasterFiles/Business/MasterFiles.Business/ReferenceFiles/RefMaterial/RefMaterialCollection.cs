using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class RefMaterialCollection : ActiveBusinessObjectCollection<RefMaterial>, IRefMaterialCollection
	{
		public RefMaterialCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public RefMaterialCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public RefMaterialCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}
	}
}
