using System.Collections.Generic;

namespace Enterprise.Customs.TR.Business.MessagingProcess
{
	public interface ITRCustomsMessagingProvider
	{
		IReadOnlyCollection<ITRCustomsMessenger> TRMessengers { get; }
		GlbExternalPassword_TR TRBPassword { get; }
	}
}
