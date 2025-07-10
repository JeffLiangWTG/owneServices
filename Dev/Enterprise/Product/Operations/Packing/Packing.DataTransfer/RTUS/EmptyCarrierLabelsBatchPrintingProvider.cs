using System;
using System.Threading;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.Core;
using WTG.RTUS.Interface;

namespace Enterprise.Packing.DataTransfer
{
	class EmptyCarrierLabelsBatchPrintingProvider : ICarrierLabelsBatchPrintingProvider
	{
		public EmptyCarrierLabelsBatchPrintingProvider(string remotePrintingServer, string remotePrintingUserName, string remotePrintingPassword)
		{
			RemotePrintingServer = remotePrintingServer;
			RemotePrintingUserName = remotePrintingUserName;
			RemotePrintingPassword = remotePrintingPassword;
		}

		string RemotePrintingServer { get; }
		string RemotePrintingUserName { get; }
		string RemotePrintingPassword { get; }

		public bool IsRemotePrintingConnectionDetailsProvided => false;

		public ReturnResult PrintCarrierLabels(ICarrierLabelsBatchPrintingInfo carrierLabelsBatchPrintingInfo, RTUSCBA type, Uri url, ZGuid printerPK, CancellationToken cancellationToken)
		{
			return new ReturnResult { Message = RTUSRemotePrintingHelper.GetRemotePrintServerConfigErrorDetails(RemotePrintingServer, RemotePrintingUserName, RemotePrintingPassword) };
		}

		public void Dispose()
		{
		}
	}
}
