using CargoWise.RefDbRepo.Common.Customization.Fody;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.ApplicationConfig.Test
{
	[TestFixture]
	public class RepoConfigurationBuilderFixture
	{
		[Test]
		public void Build()
		{
			var config = new RepoConfigurationBuilder()
				.AddJsonFile("CargoWise.RefDbRepo.Staging.ApplicationConfig.Test.config.json")
				.Build();
			Assert.AreEqual("7", config["IssueReportingExitCode"]);
			Assert.AreEqual("ConfigValueFromRepoForTest", config["ConfigFromRepoForTest"]);
		}
	}
}
