using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business.N5301;
using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.Customs.TW.Business
{
	public class TranshipmentMessageSendingObjectParent : BaseMessageSendingObjectParent<TranshipmentMessageSendingObject>
	{
		public TranshipmentMessageSendingObjectParent(CusInBondHeader entry, ZString messageType)
		: base(entry.Factory)
		{
			Header = entry;
			this.messageType = messageType;
		}
		readonly ZString messageType;

		public CusInBondHeader Header { get; }

		public override BusinessObject TopLevelBusinessObject => Header;

		protected override NonPersistentBusinessObjectCollection<TranshipmentMessageSendingObject> GetSendingObjectsCollectionCore()
		{
			var sendingObjectsCollection = new TranshipmentMessageSendingObjectCollection(Factory);
			switch (messageType)
			{
				case MessageTypeList.Codes.TRA:
					sendingObjectsCollection.Add(new N5301MessageSendingObject(Header));
					break;
			}
			return sendingObjectsCollection;
		}
		public override SecurityCheckpoint SecurityCheckpointToSendWithMessageError => Env.Security.CustomsDeclarationSendWithMessageErrors;
	}
}
