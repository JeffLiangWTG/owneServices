using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsInventoryPackageDetailCollection : NonPersistentBusinessObjectCollection<WhsInventoryPackageDetail>
	{
		#region Constructors

		public WhsInventoryPackageDetailCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#endregion

		#region CreateNonPersistentBusinessObject

		protected override BusinessObject CreateNonPersistentBusinessObject() => new WhsInventoryPackageDetail();
		
		#endregion

		#region AllowRemoveCore

		protected override bool AllowRemoveCore => false;

		#endregion

		#region AllowNewCore

		protected override bool AllowNewCore => false;

		#endregion
	}
}
