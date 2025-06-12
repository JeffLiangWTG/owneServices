using System;
using System.IO;
using System.Linq;
using OcmPoc.Tests.Scenario.Configuration;

namespace OcmPoc.Tests.Scenario.Helpers
{
	class ProviderBGenerator : FileSystemGenerator, IGenerator
	{
		public ProviderBGenerator(string recipient, int quantity)
			: base(recipient, quantity)
		{
		}

		public override IGenerator PrepareMessage(string line, int lineCount)
		{
			FileContent = new[]
			{
				$"From: Provider-B",
				$"To: {Recipient}",
				$"Tracking Id: {Guid.NewGuid()}",
			}
			.Concat(Enumerable.Repeat($"Msg: {line}", lineCount))
			.ToList();

			FileNames = Enumerable
				.Range(1, Quantity)
				.Select(serial => $"Message_{serial}.txt")
				.Select(name => Path.Combine(TestConfig.ProviderB.SendPath, name))
				.ToList();

			return this;
		}
	}
}
