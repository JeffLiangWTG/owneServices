using System;
using CargoWise.Common.ErrorManagement;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[Serializable]
	public abstract class ExceptionHandledWithPopup : Exception, IExceptionReporterExtender, IErrorReporterExtender
	{
		protected ExceptionHandledWithPopup(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected ExceptionHandledWithPopup(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		bool IExceptionReporterExtender.HandleException()
		{
			if (Globals.IsUserInteractive)
			{
				Globals.Message.ShowError(Message);
			}
			return true;
		}

		bool IErrorReporterExtender.ShouldReportAlwaysInReportOnce => true;
	}
}
