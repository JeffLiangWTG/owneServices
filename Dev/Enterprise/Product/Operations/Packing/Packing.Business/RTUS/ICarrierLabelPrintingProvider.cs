using System;
using Enterprise.Integration.DocumentEngine;
using Enterprise.ZArchitecture.Core;
using WTG.RTUS.Interface;

namespace Enterprise.Packing.Business
{
	public interface ICarrierLabelPrintingProvider : IDisposable
	{
		bool IsRemotePrintingConnectionDetailsProvided { get; }
		ReturnResult PrintCarrierLabel(PkgPackage package, RTUSCBA type, Uri url, IStmPrintQueue printer);
		bool Print(FileType fileType, byte[] binaryData, IStmPrintQueue printer);
	}
}
