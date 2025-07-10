using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class RefRefrigerantTypeCollection : ActiveBusinessObjectCollection<RefRefrigerantType>, IRefRefrigerantTypeCollection
	{
		public RefRefrigerantTypeCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public RefRefrigerantTypeCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public RefRefrigerantTypeCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}
	}
}
