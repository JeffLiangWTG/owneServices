using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	public class MessageSendingObjectParentForTestSendingObject : MessageSendingObjectParent
	{
		public MessageSendingObjectParentForTestSendingObject(AsycudaManifestHeaderForTestSendingObject manifestHeader) : base(manifestHeader)
		{
			this.manifestHeader = manifestHeader;
		}
		readonly AsycudaManifestHeaderForTestSendingObject manifestHeader;

		protected override NonPersistentBusinessObjectCollection<MessageSendingObject> GetSendingObjectsCollectionCore()
		{
			var sendingObjectsCollection = new MessageSendingObjectCollection(Factory);
			foreach (var bill in manifestHeader.Bills.Cast<AsycudaBillForTestSendingObject>())
			{
				var sendingObject = new MessageSendingObjectForTestSendingObject(bill);
				sendingObjectsCollection.Add(sendingObject);
			}
			RegisterEditableChildObject(sendingObjectsCollection);
			return sendingObjectsCollection;
		}
	}
}
