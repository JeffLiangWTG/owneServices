using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class RefDamageCollection : ActiveBusinessObjectCollection<RefDamage>, IRefDamageCollection
	{
		public RefDamageCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public RefDamageCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public RefDamageCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}
	}
}
