using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class WhsDocketFlattenedCollection : NonPersistentBusinessObjectCollection<WhsDocketFlattened>
	{
		public WhsDocketFlattenedCollection()
			: base(new BusinessObjectFactory()) // should not reuse module factory as that may have BizOs which may violate DB contraints.
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new WhsDocketFlattened();
		}
	}
}