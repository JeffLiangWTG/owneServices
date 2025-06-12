using System;

namespace OcmPoc.Tests.Scenario.Helpers
{
	static class GeneratorFactory
	{
		public static IGenerator Create(string providerName, string recipient, int quantity)
		{
			switch (providerName)
			{
				case Names.ProviderA:
					return new ProviderAGenerator(recipient, quantity);
				case Names.ProviderB:
					return new ProviderBGenerator(recipient, quantity);
				case Names.CW1:
					return new CW1Generator(recipient, quantity);
				default:
					throw new NotSupportedException(providerName);
			}
		}
    }
}
