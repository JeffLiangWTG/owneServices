using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business;
using Enterprise.Environment;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public class MessageSendingObjectParent : BaseMessageSendingObjectParent<MessageSendingObject>
	{
		public MessageSendingObjectParent(AsycudaManifestHeader header, ZString messageType) : base(header.Factory)
		{
			Header = header;
			MessageType = messageType;
			switch (messageType)
			{
				case MessageTypeList.Codes.IBC:
					getMessageSendingObjectForHeader = h => new N5135MessageSendingObject(h);
					break;
				case MessageTypeList.Codes.EBC:
					getMessageSendingObjectForHeader = h => new N5205MessageSendingObject(h);
					break;
				default:
					getMessageSendingObjectForHeader = h => null;
					break;
			}
		}

		public AsycudaManifestHeader Header { get; }

		public ZString MessageType { get; }

		public override BusinessObject TopLevelBusinessObject => Header;

		public override Security.SecurityCheckpoint SecurityCheckpointToSendWithMessageError => Env.Security.CustomsDeclarationSendWithMessageErrors;

		protected override NonPersistentBusinessObjectCollection<MessageSendingObject> GetSendingObjectsCollectionCore()
		{
			var sendingObjectsCollection = new MessageSendingObjectCollection(Factory)
			{
				getMessageSendingObjectForHeader(Header)
			};
			return sendingObjectsCollection;
		}

		readonly Func<AsycudaManifestHeader, MessageSendingObject> getMessageSendingObjectForHeader;
	}
}
