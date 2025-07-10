using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class WhsInventoryWrapperCollection : NonPersistentBusinessObjectCollection<WhsInventoryWrapper>
	{
		public WhsInventoryWrapperCollection(InventorySelectionHeader header)
			: base(header.Factory)
		{
		}

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return null;
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}
		#endregion
	}
}
