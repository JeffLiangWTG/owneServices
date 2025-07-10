using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Packing.Business
{
	public interface ICarrierLabelCancellation
	{
		IEnumerable<CancellationResponse> CancelPackages(IEnumerable<PkgPackage> packagesToCancel);
		ReturnResult SubscribePackageForCancellation(PkgPackage package);
	}
}
