using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public interface IMessageSendingActionBase
	{
		IDISDocumentBase DisDocument { get; }
		ZBool Send { get; set; }
		ZBool SendWithdrawal { get; set; }
		ZBool HasAnyDocumentsToSend { get; }
		ZString MessageType { get; }
		ZString StatusDescription { get; }
		ZString DocumentDescription { get; }
	}
}
