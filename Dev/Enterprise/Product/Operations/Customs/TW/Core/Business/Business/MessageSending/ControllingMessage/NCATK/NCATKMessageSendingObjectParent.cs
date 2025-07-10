using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public class NCATKMessageSendingObjectParent : ControllingMessageSendingObjectParent
	{
		public NCATKMessageSendingObjectParent(JobDeclaration declaration, ZString messageType) : base(declaration, messageType)
		{
		}

		public NCATKMessageSendingObjectParent(JobDeclaration declaration, ZString messageType, ZString menuCaption) : base(declaration, messageType, menuCaption)
		{
		}

		protected override ControllingMessageSendingObject GetNewSendingObject(CusTWControllingMessageHeader messageHeader) => new NCATKMessageSendingObject(messageHeader);
	}
}
