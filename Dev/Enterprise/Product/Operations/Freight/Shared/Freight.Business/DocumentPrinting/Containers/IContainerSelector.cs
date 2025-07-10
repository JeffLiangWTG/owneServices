using CargoWise.Macros;

namespace Enterprise.Freight.Business
{
	public interface IContainerSelector
	{
		Either<string, CommonContainer[]> SelectContainers(CommonContainer[] containers, ContainerSelectorMode mode = ContainerSelectorMode.Print);
	}
}
