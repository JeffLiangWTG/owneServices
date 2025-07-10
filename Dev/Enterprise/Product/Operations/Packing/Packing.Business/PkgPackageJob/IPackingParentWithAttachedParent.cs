using CargoWise.Types;

namespace Enterprise.Packing.Business
{
	public interface IPackingParentWithAttachedParent
	{
		ZString GetAttachedJobNumber(PkgPackage package);
	}
}
