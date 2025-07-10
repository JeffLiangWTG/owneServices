using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public class AdditionalDocumentMessageSendingObjectParent : JobDeclarationMessageSendingObjectParent
	{
		public AdditionalDocumentMessageSendingObjectParent(JobDeclaration declaration, ZString messageType) : this(declaration, messageType, false)
		{
		}

		public AdditionalDocumentMessageSendingObjectParent(JobDeclaration declaration, ZString messageType, ZBool includeControllingMessageInformation) : base(declaration, messageType, includeControllingMessageInformation)
		{
		}

		public new NonPersistentBusinessObjectCollection<AdditionalDocumentMessageSendingObject> SendingObjectsCollection
		{
			get
			{
				if (sendingObjectsCollection == null)
				{
					sendingObjectsCollection = new AdditionalDocumentMessageSendingObjectCollection(Factory);
					var baseCollection = base.SendingObjectsCollection;
					foreach (var sendingObject in baseCollection.Cast<MessageSendingObject>())
					{
						sendingObjectsCollection.Add(sendingObject);
					}
				}
				return sendingObjectsCollection;
			}
		}
		AdditionalDocumentMessageSendingObjectCollection sendingObjectsCollection;
	}
}
