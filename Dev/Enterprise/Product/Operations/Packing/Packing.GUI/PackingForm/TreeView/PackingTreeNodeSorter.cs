using System;
using System.Collections;
using System.Globalization;
using CargoWise.Common;
using Enterprise.Packing.Business;

namespace Enterprise.Packing.GUI
{
	class PackingTreeNodeSorter : ComparerWithCacheableSortProperties<PackingTreeNode>, IComparer
	{
		PkgPackageComparer PackageComparer { get; } = new PkgPackageComparer();
		CaseInsensitiveComparer CaseInsensitiveComparer { get; } = new CaseInsensitiveComparer(CultureInfo.InvariantCulture);

		protected override int CompareCore(PackingTreeNode x, PackingTreeNode y)
		{
			int result;
			if (x.IsPackage && !y.IsPackage) // only x is a package
			{
				result = -1;
			}
			else if (!x.IsPackage && y.IsPackage) // only y is a package
			{
				result = 1;
			}
			else if (x.IsPackage && y.IsPackage) // both packages
			{
				result = PackageComparer.Compare(x.Package, y.Package);
			}
			else if (x.IsPackageJob && y.IsPackageJob) // both package jobs
			{
				result = 1;
			}
			else // both packed items
			{
				result = Compare(x, y, nameof(PkgPackageItemDivotsWrapper.CodeWithDescription), node => (string)node.PackedItem.CodeWithDescription, CaseInsensitiveComparer.Compare);
			}

			return result;
		}

		public IDisposable CacheSortingProperties()
		{
			return new DisposableList(new[] { CacheSortProperties(), PackageComparer.CacheSortingProperties() });
		}

		int IComparer.Compare(object x, object y) => Compare((PackingTreeNode)x, (PackingTreeNode)y);
	}
}
