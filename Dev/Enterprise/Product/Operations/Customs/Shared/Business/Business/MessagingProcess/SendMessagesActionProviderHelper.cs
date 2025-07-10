using System.Linq;

namespace Enterprise.Customs.Business.MessagingProcess
{
	public static class SendMessagesActionProviderHelper
	{
		public static ActionResult DeleteMessages(ActionResult previousResult)
		{
			if (previousResult.EDIMessages.Any())
			{
				previousResult.EDIMessages.ForEach(x => x.Delete());
			}

			return previousResult;
		}
	}
}
