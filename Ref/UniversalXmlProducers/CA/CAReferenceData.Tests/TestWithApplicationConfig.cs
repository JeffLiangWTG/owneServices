using CargoWise.RefDbRepo.CAReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CAReferenceData.Tests
{
	public abstract class TestWithApplicationTestConfig
	{
		[SetUp]
		public virtual void SetUp()
		{
			ApplicationConfig.SetConfigFileForTest("CargoWise.RefDbRepo.CAReferenceData.Tests.config.json");
		}

		[TearDown]
		public virtual void TearDown()
		{
			ApplicationConfig.SetConfigFileForTest("CargoWise.RefDbRepo.CAReferenceData.CmdLine.config.json");
		}
	}
}
