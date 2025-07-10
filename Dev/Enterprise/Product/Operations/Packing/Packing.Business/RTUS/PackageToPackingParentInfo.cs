using System;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Packing.Business
{
	public sealed class PackageToPackingParentInfo
	{
		public PackageToPackingParentInfo(IPackingParent packingParent, PkgPackage package, ZString packingParentId)
		{
			Package = Argument.NotNull(package, nameof(package));
			PackingParent = Argument.NotNull(packingParent, nameof(packingParent));

			if (packingParentId.IsEmpty)
			{
				throw new ArgumentException("Packing Parent ID should not be empty.", nameof(packingParentId));
			}

			PackingParentId = packingParentId;
		}

		public PkgPackage Package { get; }
		public ZString PackingParentId { get; }
		public IPackingParent PackingParent { get; }
	}
}
