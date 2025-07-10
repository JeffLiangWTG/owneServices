using CargoWise.RefDbRepo.Staging.Schema_New;
using CargoWise.RefDbRepo.UniversalXmlParser;
using CargoWise.RefDbRepo.UniversalXmlParser.Interfaces;
using CargoWise.RefDbRepo.UniversalXmlParser.Providers;
using CargoWise.RefDbRepo.UniversalXmlParser.Repository;
using CargoWise.RefDbRepo.UniversalXmlParser.Utilities;
using CargoWise.RefDbRepo.UniversalXmlProcessor.Configuration;
using Unity;
using Unity.Injection;
using Unity.Lifetime;

namespace CargoWise.RefDbRepo.UniversalXmlProcessor
{
	public static class Application
	{
		static UnityContainer unityContainer;
		public static UnityContainer UnityContainer
		{
			get
			{
				if (unityContainer == null)
				{
					unityContainer = new UnityContainer();
				}
				return unityContainer;
			}
		}

		public static void ConfigEnvironment(string filePath, string connectionString = "")
		{
			unityContainer = null;
			connectionString = string.IsNullOrEmpty(connectionString) ? ApplicationConfig.RefDbRepoStagingConnString : connectionString;
			ConfigUnityContainer(filePath, connectionString);
			ConfigBulkInsertSize();
		}

		static void ConfigBulkInsertSize()
		{
			var applicationConfig = UnityContainer.Resolve<IParserConfig>();
			applicationConfig.BulkInsertSize = ApplicationConfig.BulkInsertSize;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "dispose by ContainerControlledLifetimeManager")]
		static void ConfigUnityContainer(string filePath, string connectionString)
		{
			UnityContainer.RegisterType<IUniversalXmlParser, UniversalXmlParser.UniversalXmlParser>();
			UnityContainer.RegisterType<IUniversalXmlSchemaHandler, UniversalXmlSchemaHandler>();
			UnityContainer.RegisterType<IStagingRepositoryWrapper, StagingRepositoryWrapper>();
			UnityContainer.RegisterType<ISourceDataWriter, SourceDataWriter>();
			UnityContainer.RegisterType<IFileTrace, FileTrace>(new ContainerControlledLifetimeManager(), new InjectionConstructor(filePath));
			UnityContainer.RegisterType<IParserConfig, ParserConfig>(new ContainerControlledLifetimeManager());
			UnityContainer.RegisterType<IEntityTypeHelper, EntityTypeHelper>(new ContainerControlledLifetimeManager());
			UnityContainer.RegisterType<IObsoletePropertiesLookup, DummyObsoletePropertiesLookup>(new ContainerControlledLifetimeManager());
			UnityContainer.RegisterType<IEntityMissingValuesProvider, EntityMissingValuesProvider>(new ContainerControlledLifetimeManager());
			UnityContainer.RegisterInstance<IEntityValuesProvider>(new EntityValuesProvider(connectionString));
			UnityContainer.RegisterInstance<IStagingRepository>(new StagingRepository(connectionString));
		}
	}
}
