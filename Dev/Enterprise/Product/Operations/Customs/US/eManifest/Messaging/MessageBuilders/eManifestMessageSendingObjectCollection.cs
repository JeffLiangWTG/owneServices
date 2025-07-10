using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.eManifest.Messaging
{
	class eManifestMessageSendingObjectCollection : NonPersistentBusinessObjectCollection<eManifestMessageSendingObject>
	{
		public eManifestMessageSendingObjectCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotImplementedException();
		}
		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;
	}
}
