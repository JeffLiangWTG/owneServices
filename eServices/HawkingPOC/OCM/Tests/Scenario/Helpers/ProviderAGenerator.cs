using System.IO;
using System.Linq;
using OcmPoc.Tests.Scenario.Configuration;

namespace OcmPoc.Tests.Scenario.Helpers
{
	class ProviderAGenerator : FileSystemGenerator, IGenerator
	{
		public ProviderAGenerator(string recipient, int quantity)
			: base(recipient, quantity)
		{
		}

		public override IGenerator PrepareMessage(string line, int lineCount)
		{
			FileContent = Enumerable.Repeat(line, lineCount).ToList();
			FileNames = Enumerable
				.Range(1, Quantity)
				.Select(serial => $"To_{Recipient}_{serial}.txt")
				.Select(name => Path.Combine(TestConfig.ProviderA.SendPath, name))
				.ToList();

			return this;
		}
	}
}
