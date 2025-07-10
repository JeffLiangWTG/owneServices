using CargoWise.RefDbRepo.DataProcessingExplanation;
using CargoWise.RefDbRepo.Staging.Schema_New;
using CargoWise.RefDbRepo.UniversalXmlParser;
using CargoWise.RefDbRepo.UniversalXmlParser.Providers;
using CargoWise.RefDbRepo.UniversalXmlParser.Repository;
using CargoWise.RefDbRepo.UniversalXmlParser.Utilities;
using CargoWise.RefDbRepo.XmlProducer.Common;

namespace CargoWise.RefDbRepo.SourceDataBinaryScanner
{
	class Program
	{
		static int Main(string[] args)
		{
			var uxmlSchemaHandler = new UniversalXmlSchemaHandler();
			var configProvider = new ConfigProvider();
			ErrorConstants.AddJsonFile(ConfigProvider.JsonConfigFile);
			using var stagingRepository = new StagingRepository(configProvider.ConnectionString);

			var obsoleteProperties = new DummyObsoletePropertiesLookup();
			var entityTypeHelper = new EntityTypeHelper(obsoleteProperties);

			using var entityValuesProvider = new EntityValuesProvider(configProvider.ConnectionString);
			var entityMissingValuesProvider = new EntityMissingValuesProvider(entityTypeHelper, entityValuesProvider);
			var parserConfig = new FileParserConfig();
			var stagingRepositoryWrapper = new StagingRepositoryWrapper(stagingRepository, entityMissingValuesProvider, entityTypeHelper, parserConfig);
			var fileTrace = new DummyFileTrace();

			var scanner = new Scanner(new ContentParser(), stagingRepository, uxmlSchemaHandler, fileTrace, stagingRepositoryWrapper, configProvider);
			scanner.Scan()?.Wait();
			return (int)ProducerStatus.Success;
		}
	}
}
