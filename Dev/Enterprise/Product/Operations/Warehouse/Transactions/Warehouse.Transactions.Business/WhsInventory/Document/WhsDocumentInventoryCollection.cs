using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsDocumentInventoryCollection : NonPersistentBusinessObjectCollection<WhsDocumentInventory>
	{
		WhsDocumentInventoryCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public static WhsDocumentInventoryCollection New(WhsInventoryViewCollection inventories)
		{
			var result = new WhsDocumentInventoryCollection(inventories.Factory);
			result.AddRange(inventories.Cast<WhsInventoryView>().Select(i => new WhsDocumentInventory(i)));
			return result;
		}

		#region Overrides

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new WhsDocumentInventory();
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		#endregion
	}
}
