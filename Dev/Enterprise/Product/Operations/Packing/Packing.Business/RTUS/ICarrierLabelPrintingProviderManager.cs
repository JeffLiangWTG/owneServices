using System;

namespace Enterprise.Packing.Business
{
	public interface ICarrierLabelPrintingProviderManager : IDisposable
	{
		ICarrierLabelPrintingProvider GetCarrierLabelPrintingProvider(Action<Exception> action);
		bool IsParentJobValid(IPackingParent packingParent);
		string GetInvalidParentJobTypeErrorMessage(PkgPackage package);
	}
}
