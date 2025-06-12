using System;
using OcmPoc.Tests.Scenario.Configuration;

namespace OcmPoc.Tests.Scenario.Helpers
{
	static class ReceiverFactory
    {
		public static IReceiver Create(string providerName, int count)
		{
			switch (providerName)
			{
				case Names.ProviderA:
					return new FileSystemReceiver(TestConfig.ProviderA.ReceivePath, count);
				case Names.ProviderB:
					return new FileSystemReceiver(TestConfig.ProviderB.ReceivePath, count);
				default:
					throw new NotSupportedException(providerName);
			}
		}
    }
}
