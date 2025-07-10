using CargoWise.EntityFramework;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocSending
{
	public sealed class DocSendingBusinessObjectCollection : NonPersistentBusinessObjectCollection<DocSendingBusinessObject>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject() => null;

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;
	}
}
