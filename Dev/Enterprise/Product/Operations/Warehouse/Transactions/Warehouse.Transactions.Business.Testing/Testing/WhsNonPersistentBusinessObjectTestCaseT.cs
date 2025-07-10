using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public abstract class WhsNonPersistentBusinessObjectTestCase<T> : WhsNonPersistentBusinessObjectTestCase
			where T : NonPersistentBusinessObject
	{
		#region Implementation

		protected new T GetNewBusinessObject()
		{
			return (T)base.GetNewBusinessObject();
		}

		#endregion
	}
}
