using CargoWise.EntityFramework;

namespace Enterprise.Freight.Agency.Business
{
	public class BulkDetentionChildCollection : NonPersistentBusinessObjectCollection<BulkDetentionChild>
	{
		public BulkDetentionChildCollection(BusinessObjectFactory factory)
			: base(factory) { }

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new BulkDetentionChild(Factory);
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


