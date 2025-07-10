using System.Collections.Generic;

namespace Enterprise.Customs.Business.MessagingProcess
{
	public interface ISupportPreSendValidation
	{
		IReadOnlyCollection<MessageSendingNotification> RunPreSendValidation(ActionResult previousResult);
	}
}
