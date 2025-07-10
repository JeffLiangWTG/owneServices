using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsDocumentInventoryOptions : NonPersistentBusinessObject
	{
		public WhsDocumentInventoryOptions(WhsInventoryViewCollection inventories)
		{
			this.inventories = inventories;
		}

		readonly WhsInventoryViewCollection inventories;

		#region DocumentInventories

		[ChildEditable()]
		public WhsDocumentInventoryCollection DocumentInventories
		{
			get
			{
				if (documentInventories == null)
				{
					documentInventories = WhsDocumentInventoryCollection.New(inventories);
					RegisterEditableChildObject(documentInventories);
				}
				return documentInventories;
			}
		}

		WhsDocumentInventoryCollection documentInventories;

		#endregion
	}
}
