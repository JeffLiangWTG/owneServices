using System;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business
{
	public class DocumentPickupDeliveryConfirmCollection : NonPersistentBusinessObjectCollection<DocumentPickupDeliveryConfirm>
	{
		public DocumentPickupDeliveryConfirmCollection(BusinessObjectFactory factory)
			: base(factory)
		{ }

		public DocumentPickupDeliveryConfirmCollection(CommonPickupDeliveryConfirmCollection confirms)
			: base(confirms.Factory)
		{
			foreach (CommonPickupDeliveryConfirm confirm in confirms)
			{
				Add(new DocumentPickupDeliveryConfirm(confirm));
			}
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException();
		}
	}
}
