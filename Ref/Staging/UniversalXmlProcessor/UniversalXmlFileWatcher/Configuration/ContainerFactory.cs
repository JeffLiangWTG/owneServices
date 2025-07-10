using CargoWise.RefDbRepo.UniversalXmlFileWatcher.Interfaces;
using Unity;

namespace CargoWise.RefDbRepo.UniversalXmlFileWatcher.Configuration
{
	class ContainerFactory : IContainerFactory
	{
		public UnityContainer Create(string fileName)
		{
			var result = new UnityContainer();
			Application.ConfigFileSpecificEnvironment(result, fileName);
			return result;
		}
	}
}
