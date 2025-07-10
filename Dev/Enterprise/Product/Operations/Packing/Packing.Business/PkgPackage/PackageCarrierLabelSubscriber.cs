using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Packing.Business
{
	public static class PackageCarrierLabelSubscriber
	{
		internal static ReturnResult SubscribePackageForCancellation(PkgPackage package)
		{
			return GetService(package.Factory).SubscribePackage(package);
		}

		internal static PkgPackage.PackageCarrierLabelCancellation GetService(BusinessObjectFactory factory)
		{
			var packageCancellation = factory.ServiceContainer.GetAfterOnSavingService<PkgPackage.PackageCarrierLabelCancellation>();
			if (packageCancellation == null)
			{
				packageCancellation = new PkgPackage.PackageCarrierLabelCancellation(factory);
				factory.ServiceContainer.AddAfterOnSavingService(packageCancellation);
			}

			return packageCancellation;
		}
	}
}
