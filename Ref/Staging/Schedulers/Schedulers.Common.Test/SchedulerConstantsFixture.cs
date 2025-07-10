using System.IO;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.Schedulers.Common.Test
{
	[TestFixture]
	class SchedulerConstantsFixture
	{
		[Test]
		public void UXMLProducerConfigPath()
		{
			Assert.That(SchedulerConstants.UXMLProducerConfigPaths, Has.Some.Matches<string>(x => File.Exists(Path.Combine(x, "CargoWise.RefDbRepo.AUReferenceData.CmdLine.config.json"))));
		}
	}
}
