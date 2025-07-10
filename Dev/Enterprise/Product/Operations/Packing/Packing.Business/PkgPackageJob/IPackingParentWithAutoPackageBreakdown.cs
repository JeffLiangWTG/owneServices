using System;
using System.Collections.Generic;

namespace Enterprise.Packing.Business
{
	public interface IPackingParentWithAutoPackageBreakdown : IPackingParent
	{
		IDisposable SuspendWhileAddingAutoCreatedPackages();
		void RunAfterAllAutoCreatedPackagesAreAdded(IEnumerable<PkgPackage> packages);
	}
}
