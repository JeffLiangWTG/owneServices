using System;
using Enterprise.Integration.DocumentEngine;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.Core;
using WTG.RTUS.Interface;

namespace Enterprise.Packing.DataTransfer
{
	class EmptyCarrierLabelPrintingProvider : ICarrierLabelPrintingProvider
	{
		public EmptyCarrierLabelPrintingProvider(string remotePrintingServer, string remotePrintingUserName, string remotePrintingPassword)
		{
			RemotePrintingServer = remotePrintingServer;
			RemotePrintingUserName = remotePrintingUserName;
			RemotePrintingPassword = remotePrintingPassword;
		}

		string RemotePrintingServer { get; }
		string RemotePrintingUserName { get; }
		string RemotePrintingPassword { get; }

		public bool IsRemotePrintingConnectionDetailsProvided => false;

		public ReturnResult PrintCarrierLabel(PkgPackage package, RTUSCBA type, Uri url, IStmPrintQueue printer)
		{
			return new ReturnResult { Message = RTUSRemotePrintingHelper.GetRemotePrintServerConfigErrorDetails(RemotePrintingServer, RemotePrintingUserName, RemotePrintingPassword) };
		}

		public bool Print(FileType fileType, byte[] binaryData, IStmPrintQueue printer)
		{
			throw new InvalidOperationException();
		}

		public void Dispose()
		{
		}
	}
}
