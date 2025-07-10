using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business.Testing
{
	public class JobDeclarationMessageSendingObjectParentForTest : JobDeclarationMessageSendingObjectParent
	{
		public JobDeclarationMessageSendingObjectParentForTest(JobDeclaration declaration, ZString messageType) : base(declaration, messageType)
		{
		}

		protected override NonPersistentBusinessObjectCollection<MessageSendingObject> GetSendingObjectsCollectionCore()
		{
			if (sendingObjectsCollection == null)
			{
				sendingObjectsCollection = new MessageSendingObjectCollection(Factory);
				foreach (var header in ParentDeclaration.ActiveEntryHeaders.Cast<CusEntryHeader>())
				{
					var sendingObject = new MessageSendingObjectForTesting(header);
					if (sendingObject != null)
					{
						sendingObjectsCollection.Add(sendingObject);
					}
				}
			}

			RegisterEditableChildObject(sendingObjectsCollection);
			return sendingObjectsCollection;
		}

		MessageSendingObjectCollection sendingObjectsCollection;
	}
}
