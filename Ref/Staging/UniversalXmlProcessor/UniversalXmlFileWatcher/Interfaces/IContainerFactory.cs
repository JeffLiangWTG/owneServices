using Unity;

namespace CargoWise.RefDbRepo.UniversalXmlFileWatcher.Interfaces
{
	public interface IContainerFactory
	{
		UnityContainer Create(string fileName);
	}
}
