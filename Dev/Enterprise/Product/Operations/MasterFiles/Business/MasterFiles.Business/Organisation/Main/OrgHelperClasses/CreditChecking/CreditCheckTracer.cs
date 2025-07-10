using System;
using CargoWise.Application;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core.Diagnostics;

namespace Enterprise.MasterFiles.CreditControl.Business
{
	public class CreditCheckTracer
	{
		public CreditCheckTracer()
		{
			Tracer = ObjectFactory.Get<ITracer>();
		}

		ITracer Tracer { get; }

		public void TraceInformation(Func<string> buildMessage)
		{
			Tracer.TraceInformation(AccountingTraceSourceCodes.CLC, buildMessage);
		}

		public bool IsEnabled()
		{
			return Tracer.IsEnabled(AccountingTraceSourceCodes.CLC);
		}
	}
}
