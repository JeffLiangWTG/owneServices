using System.Collections.Generic;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.Business.MessagingProcess
{
	public interface ISupportMessageSigning
	{
		string SignMessages(IReadOnlyCollection<EDIMessage> messages, ActionResult previousResult);
	}
}
