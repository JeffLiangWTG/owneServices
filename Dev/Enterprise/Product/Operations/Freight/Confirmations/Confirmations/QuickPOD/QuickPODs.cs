using CargoWise.EntityFramework;

namespace Enterprise.Freight.Confirmations.Business
{
	public class QuickPODs : NonPersistentBusinessObject, IObsoleteValidation
	{
		public QuickPODs(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Collections

		public QuickPODCollection QuickPODsCollection
		{
			get
			{
				if (quickPODsCollection == null)
				{
					quickPODsCollection = new QuickPODCollection(this);
					RegisterEditableChildObject(quickPODsCollection);
				}
				return quickPODsCollection;
			}
		}
		QuickPODCollection quickPODsCollection;

		#endregion

		#region Events

		public event QuickPODMultipleShipmentsEventHandler HouseBillMatchesOnMultipleShipments;

		public void RaiseQuickPODMultipleShipments(QuickPODMultipleShipmentsEventArgs e)
		{
			if (HouseBillMatchesOnMultipleShipments != null)
			{
				HouseBillMatchesOnMultipleShipments(this, e);
			}
		}

		#endregion
	}
}
