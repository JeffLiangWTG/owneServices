using Enterprise.Integration.Packing;

namespace Enterprise.Warehouse.Integration
{
	public interface IWhsPackageStrategy
	{
		IPackageActionStrategy GetPackageActionStrategy(IWhsOrder order, IPkgPackage package);
		void OnPackageDelete(IWhsOrder order, IPkgPackage package);
	}
}
