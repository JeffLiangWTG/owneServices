#if DEBUG
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Packing.Business.Testing
{
	public class DummyWithAutoPackageBreakdown : DummyWithPacking, IPackingParentWithAutoPackageBreakdown
	{
		public DummyWithAutoPackageBreakdown(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public void RunAfterAllAutoCreatedPackagesAreAdded(IEnumerable<PkgPackage> packages)
		{
			PackagesPassedIntoRunAfterAllAutoCreatedPackagesAreAdded = packages.ToArray();
		}

		public PkgPackage[] PackagesPassedIntoRunAfterAllAutoCreatedPackagesAreAdded { get; private set; }

		public IDisposable SuspendWhileAddingAutoCreatedPackages()
		{
			return new SemaphoreManager(AutoBreakdownSemaphore);
		}

		public readonly Semaphore AutoBreakdownSemaphore = new Semaphore();
	}
}
#endif
