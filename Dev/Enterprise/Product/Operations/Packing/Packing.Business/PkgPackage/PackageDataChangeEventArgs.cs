using CargoWise.Common;

namespace Enterprise.Packing.Business
{
	public class PackageDataChangeEventArgs : PackageEventArgs
	{
		public PackageDataChangeEventArgs(PkgPackage package, PackageDataChangeType dataChangeType)
			: base(package)
		{
			DataChangeType = Argument.NotNull(dataChangeType, "dataChangeType");
		}

		public readonly PackageDataChangeType DataChangeType;
	}

	public enum PackageDataChangeType
	{
		PackageContent,
		Weight,
		Volume
	}
}

