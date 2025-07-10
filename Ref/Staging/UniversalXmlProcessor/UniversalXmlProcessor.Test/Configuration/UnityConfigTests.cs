using CargoWise.RefDbRepo.Staging.Schema_New;
using CargoWise.RefDbRepo.UniversalXmlParser.Interfaces;
using NUnit.Framework;
using Unity;

namespace CargoWise.RefDbRepo.UniversalXmlProcessor.Test
{
	[TestFixture]
	public class UnityConfigTests
	{
		[SetUp]
		public void Setup()
		{
			Application.ConfigEnvironment("test.txt");
		}

		[Test]
		public void TestUnityConfig()
		{
			var container = Application.UnityContainer;

			Assert.IsNotNull(container.Resolve<IUniversalXmlParser>());
			Assert.IsNotNull(container.Resolve<IUniversalXmlSchemaHandler>());
			Assert.IsNotNull(container.Resolve<IStagingRepositoryWrapper>());
			Assert.IsNotNull(container.Resolve<ISourceDataWriter>());
			Assert.IsNotNull(container.Resolve<IFileTrace>());
			Assert.IsNotNull(container.Resolve<IParserConfig>());
			Assert.IsNotNull(container.Resolve<IEntityTypeHelper>());
			Assert.IsNotNull(container.Resolve<IEntityMissingValuesProvider>());
			Assert.IsNotNull(container.Resolve<IEntityValuesProvider>());
			Assert.IsNotNull(container.Resolve<IStagingRepository>());
		}

		[Test]
		public void TestBulkInsertSizeIsSet()
		{
			var container = Application.UnityContainer;

			var parserConfig = container.Resolve<IParserConfig>();
			Assert.IsNotNull(parserConfig);
			Assert.IsTrue(parserConfig.BulkInsertSize > 0);
		}
	}
}
