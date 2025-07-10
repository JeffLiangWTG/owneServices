using Enterprise.Customs.Common;

namespace Enterprise.Customs.Business.MessagingProcess
{
	public interface ICustomsMessenger
	{
		IEDIMessageCollectionOwner Owner { get; }
		ICustomsMessageGenerator MessageGenerator { get; }

		bool ShouldCreateMessage(ActionResult previousResult);

		bool ProcessUpdates(ActionResult previousResult);
	}
}
