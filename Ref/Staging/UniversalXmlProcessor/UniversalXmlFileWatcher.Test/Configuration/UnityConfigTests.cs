using System.IO;
using CargoWise.RefDbRepo.Staging.Schema_New;
using CargoWise.RefDbRepo.UniversalXmlFileWatcher.Configuration;
using CargoWise.RefDbRepo.UniversalXmlFileWatcher.Interfaces;
using CargoWise.RefDbRepo.UniversalXmlParser.Interfaces;
using NUnit.Framework;
using Unity;

namespace CargoWise.RefDbRepo.UniversalXmlFileWatcher.Tests.Configuration
{
	[TestFixture]
	public class UnityConfigTests
	{
		[SetUp]
		public void Setup()
		{
			Application.ConfigCoreEnvironment(Path.GetTempPath());
		}

		[Test]
		public void TestCoreUnityContainerConfig()
		{
			var container = Application.UnityContainer;

			Assert.IsNotNull(container.Resolve<IFileProcessorConfig>());
			Assert.IsNotNull(container.Resolve<IFileProcessor>());
		}

		[Test]
		public void TestFileSpecificEnvironmentConfig()
		{
			using (var innerContainer = new UnityContainer())
			{
				Application.ConfigFileSpecificEnvironment(innerContainer, Path.GetTempPath());

				Assert.IsNotNull(innerContainer.Resolve<IParserConfig>());

				Assert.IsNotNull(innerContainer.Resolve<IUniversalXmlParser>());
				Assert.IsNotNull(innerContainer.Resolve<IUniversalXmlSchemaHandler>());
				Assert.IsNotNull(innerContainer.Resolve<IStagingRepositoryWrapper>());
				Assert.IsNotNull(innerContainer.Resolve<ISourceDataWriter>());
				Assert.IsNotNull(innerContainer.Resolve<IFileTrace>());
				Assert.IsNotNull(innerContainer.Resolve<IEntityTypeHelper>());
				Assert.IsNotNull(innerContainer.Resolve<IEntityMissingValuesProvider>());
				Assert.IsNotNull(innerContainer.Resolve<IEntityValuesProvider>());
				Assert.IsNotNull(innerContainer.Resolve<IStagingRepository>());
			}
		}

		[Test]
		public void TestBulkInsertSizeIsSet()
		{
			var container = Application.UnityContainer;

			var applicationConfig = container.Resolve<IFileProcessorConfig>();
			Assert.IsNotNull(applicationConfig);
			Assert.IsTrue(applicationConfig.BulkInsertSize > 0);
		}
	}
}
