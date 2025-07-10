using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Packing.Business
{
	public interface IPackingParentSupportsPackageExtensions : IBusiness
	{
		string TablePrefix { get; }
		ZGuid PK { get; }
		PkgPackageExtension PackageExtension { get; }
	}
}
