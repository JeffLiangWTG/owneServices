using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsHoldOrderLineCollection : NonPersistentBusinessObjectCollection<WhsHoldOrderLine>
	{
		public WhsHoldOrderLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new WhsHoldOrderLine(Factory);
		}

		#endregion	
	}
}
