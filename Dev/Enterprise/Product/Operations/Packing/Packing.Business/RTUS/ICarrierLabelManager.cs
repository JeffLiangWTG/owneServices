using System;
using Enterprise.UniversalDataBuss.Integration;
using WTG.RTUS.Interface;

namespace Enterprise.Packing.Business
{
	public interface ICarrierLabelManager : IDisposable
	{
		T PushCarrierLabelRequest<T>(RequestType<T> requestType, ITopLevelDataObject packageUniversalShipment, RTUSCBA type, Uri url) where T : IRTUSResponse;
		T PushCarrierLabelRequest<T>(RequestType<T> requestType, PkgPackage package, RTUSCBA type, Uri url) where T : IRTUSResponse;
	}
}
