using System;
using System.Threading;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using WTG.RTUS.Interface;

namespace Enterprise.Packing.Business
{
	public interface ICarrierLabelsBatchPrintingProvider : IDisposable
	{
		bool IsRemotePrintingConnectionDetailsProvided { get; }
		ReturnResult PrintCarrierLabels(ICarrierLabelsBatchPrintingInfo carrierLabelsBatchPrintingInfo, RTUSCBA type, Uri url, ZGuid printerPK, CancellationToken cancellationToken);
	}
}
