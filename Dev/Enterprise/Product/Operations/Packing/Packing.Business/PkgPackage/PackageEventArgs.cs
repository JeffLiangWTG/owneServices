using System;
using CargoWise.Common;

namespace Enterprise.Packing.Business
{
	public class PackageEventArgs : EventArgs
	{
		public PackageEventArgs(PkgPackage package)
		{
			Package = Argument.NotNull(package, "PkgPackage package");
		}

		public readonly PkgPackage Package;
	}
}

