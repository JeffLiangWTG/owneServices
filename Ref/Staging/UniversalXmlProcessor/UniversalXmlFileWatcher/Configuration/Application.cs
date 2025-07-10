using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Staging.Schema_New;
using CargoWise.RefDbRepo.UniversalXmlFileWatcher.Interfaces;
using CargoWise.RefDbRepo.UniversalXmlFileWatcher.Utilities;
using CargoWise.RefDbRepo.UniversalXmlParser;
using CargoWise.RefDbRepo.UniversalXmlParser.Interfaces;
using CargoWise.RefDbRepo.UniversalXmlParser.Providers;
using CargoWise.RefDbRepo.UniversalXmlParser.Repository;
using CargoWise.RefDbRepo.UniversalXmlParser.Utilities;
using Unity;
using Unity.Injection;
using Unity.Lifetime;

namespace CargoWise.RefDbRepo.UniversalXmlFileWatcher.Configuration
{
	public static class Application
	{
		const int DefaultBulkInsertSize = 100;

		public static readonly UnityContainer UnityContainer = new UnityContainer();

		public static void ConfigCoreEnvironment(string baseDirectory)
		{
			Argument.NotNullOrEmpty(baseDirectory, nameof(baseDirectory));

			ConfigCoreUnityContainer();
			ConfigSettings(baseDirectory);
			ConfigFileWatcher();
			ConfigFileProcessor(baseDirectory);
			ConfigBulkInsertSize();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "dispose by ContainerControlledLifetimeManager")]
		static void ConfigCoreUnityContainer()
		{
			UnityContainer.RegisterType<IFileProcessorConfig, FileProcessorConfig>(new ContainerControlledLifetimeManager());
			UnityContainer.RegisterType<IFileProcessor, FileProcessor>(new ContainerControlledLifetimeManager());
			UnityContainer.RegisterType<IFileCompressor, FileCompressor>(new ContainerControlledLifetimeManager());
			UnityContainer.RegisterType<IContainerFactory, ContainerFactory>(new ContainerControlledLifetimeManager());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "dispose by ContainerControlledLifetimeManager")]
		public static void ConfigFileSpecificEnvironment(IUnityContainer container, string fileName)
		{
			container.RegisterInstance<IParserConfig>(UnityContainer.Resolve<IFileProcessorConfig>());

			container.RegisterType<IUniversalXmlParser, UniversalXmlParser.UniversalXmlParser>();
			container.RegisterType<IUniversalXmlSchemaHandler, UniversalXmlSchemaHandler>();
			container.RegisterType<IStagingRepositoryWrapper, StagingRepositoryWrapper>();
			container.RegisterType<ISourceDataWriter, SourceDataWriter>();
			container.RegisterType<IFileTrace, FileTrace>(new InjectionConstructor(fileName));
			container.RegisterType<IEntityTypeHelper, EntityTypeHelper>();
			container.RegisterType<IObsoletePropertiesLookup, DummyObsoletePropertiesLookup>();
			container.RegisterType<IEntityMissingValuesProvider, EntityMissingValuesProvider>();
			container.RegisterInstance<IEntityValuesProvider>(new EntityValuesProvider(AppConfig.RefDbRepoStagingConnString));
			container.RegisterInstance<IStagingRepository>(new StagingRepository(AppConfig.RefDbRepoStagingConnString));
		}

		static void ConfigSettings(string baseDirectory)
		{
			Argument.NotNullOrEmpty(baseDirectory, nameof(baseDirectory));

			var incomingXmlFilesDir = "..\\XmlFiles";
			var archiveXmlFilesDir = incomingXmlFilesDir + "\\Archive";
			var errorXmlFilesDir = incomingXmlFilesDir + "\\Error";
			var incomingXmlFilesForceParseDir = incomingXmlFilesDir + "\\ForceParse";

			incomingXmlFilesDir = AppConfig.Configuration["IncomingXmlFilesDir"] ?? Path.Combine(baseDirectory, incomingXmlFilesDir);

			archiveXmlFilesDir = AppConfig.Configuration["ArchiveXmlFilesDir"] ?? Path.Combine(baseDirectory, archiveXmlFilesDir);

			errorXmlFilesDir = AppConfig.Configuration["ErrorXmlFilesDir"] ?? Path.Combine(baseDirectory, errorXmlFilesDir);

			incomingXmlFilesForceParseDir = AppConfig.Configuration["IncomingXmlFilesForceParseDir"] ?? Path.Combine(baseDirectory, incomingXmlFilesForceParseDir);

			var incomingXmlFileExtension = AppConfig.Configuration["IncomingXmlFileExtension"] ?? "*.xml";

			if (!int.TryParse(AppConfig.Configuration["FileWatchTimerIntervalInSeconds"], out var fileWatchTimerIntervalInSeconds))
			{
				fileWatchTimerIntervalInSeconds = 30;
			}

			var fileProcessorConfig = UnityContainer.Resolve<IFileProcessorConfig>();

			fileProcessorConfig.IncomingXmlFilesDir = Path.GetFullPath(incomingXmlFilesDir);
			fileProcessorConfig.ArchiveXmlFilesDir = Path.GetFullPath(archiveXmlFilesDir);
			fileProcessorConfig.ErrorXmlFilesDir = Path.GetFullPath(errorXmlFilesDir);
			fileProcessorConfig.IncomingXmlFilesForceParseDir = Path.GetFullPath(incomingXmlFilesForceParseDir);
			fileProcessorConfig.UniversalXmlFileExtensionName = incomingXmlFileExtension;
			fileProcessorConfig.FileWatchTimerIntervalInSeconds = fileWatchTimerIntervalInSeconds;
		}

		static void ConfigBulkInsertSize()
		{
			var fileWatcherConfig = UnityContainer.Resolve<IFileProcessorConfig>();
			fileWatcherConfig.BulkInsertSize = DefaultBulkInsertSize;

			if (int.TryParse(AppConfig.Configuration["BulkInsertSize"], out var bulkInsertSize))
			{
				fileWatcherConfig.BulkInsertSize = bulkInsertSize;
			}
		}

		static void ConfigFileWatcher()
		{
			var fileWatcherConfig = UnityContainer.Resolve<IFileProcessorConfig>();
			var fileProcessor = UnityContainer.Resolve<IFileProcessor>();

			fileProcessor.FoldersToScan = [fileWatcherConfig.IncomingXmlFilesForceParseDir, fileWatcherConfig.IncomingXmlFilesDir];
			fileProcessor.FileExtensionToMonitor = fileWatcherConfig.UniversalXmlFileExtensionName;
		}

		static void ConfigFileProcessor(string baseDirectory)
		{
			Argument.NotNullOrEmpty(baseDirectory, nameof(baseDirectory));
			var fileWatcherConfig = UnityContainer.Resolve<IFileProcessorConfig>();
			var fileProcessor = UnityContainer.Resolve<IFileProcessor>();

			_ = int.TryParse(AppConfig.Configuration["OverDueMonths"], out var months);
			months = months > 0 ? months : 12;
			var lastDeleteFile = AppConfig.Configuration["LastRunTimeFile"] ?? "FileWatcher_LastRunTime.cfg";
			fileProcessor.OverdueMonths = months;
			fileProcessor.LastRunTimeFile = Path.Combine(baseDirectory, lastDeleteFile);
			fileProcessor.ArchiveFolder = fileWatcherConfig.ArchiveXmlFilesDir;
			fileProcessor.ErrorFolder = fileWatcherConfig.ErrorXmlFilesDir;
		}
	}
}
