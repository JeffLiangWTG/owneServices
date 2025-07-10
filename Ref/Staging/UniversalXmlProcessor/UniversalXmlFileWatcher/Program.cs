using System.Globalization;
using CargoWise.RefDbRepo.DataProcessingExplanation;
using CargoWise.RefDbRepo.UniversalXmlFileWatcher.Configuration;
using CargoWise.RefDbRepo.UniversalXmlFileWatcher.Interfaces;
using CargoWise.RefDbRepo.XmlProducer.Common;
using Unity;

namespace CargoWise.RefDbRepo.UniversalXmlFileWatcher
{
	class Program
	{
		static int Main()
		{
			CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
			Application.ConfigCoreEnvironment(AppDomain.CurrentDomain.BaseDirectory);
			ErrorConstants.AddJsonFile(AppConfig.JsonConfigFile);

			return (int)Execute();
		}

		static ProducerStatus Execute()
		{
			var fileProcessor = Application.UnityContainer.Resolve<IFileProcessor>();

			var task = fileProcessor.ScanFolders();

			task.GetAwaiter().GetResult();
			fileProcessor.DeleteOverdueFiles();

			return ProducerStatus.Success;
		}
	}
}
