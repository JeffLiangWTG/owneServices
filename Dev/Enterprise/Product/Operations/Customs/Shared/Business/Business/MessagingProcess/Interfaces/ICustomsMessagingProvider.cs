using System.Collections.Generic;

namespace Enterprise.Customs.Business.MessagingProcess
{
	public interface ICustomsMessagingProvider
	{
		bool IsInTestMode { get; }
		bool EnableTestModeValidation { get; }

		IReadOnlyCollection<ICustomsMessenger> GetMessengers();
	}
}
