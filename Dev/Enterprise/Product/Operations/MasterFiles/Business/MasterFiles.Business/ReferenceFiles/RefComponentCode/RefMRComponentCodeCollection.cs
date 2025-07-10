using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class RefMRComponentCodeCollection : ActiveBusinessObjectCollection<RefMRComponentCode>, IRefMRComponentCodeCollection
	{
		public RefMRComponentCodeCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public RefMRComponentCodeCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public RefMRComponentCodeCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}
	}
}
