using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using Common.Logging;

namespace CargoWise.eServices.Billing.WcfService
{
	public class GlobalErrorHandler : IErrorHandler
	{
		public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
		{
			if (!(error is FaultException))
			{
				var faultException = new FaultException(error.Message);
				var messageFault = faultException.CreateMessageFault();
				fault = Message.CreateMessage(version, messageFault, faultException.Action);
			}
		}

		public bool HandleError(Exception error)
		{
			if (!(error is FaultException))
			{
				Logger.Error("Unknown error", error);
			}
			return true;
		}

		static readonly ILog Logger = LogManager.GetLogger(typeof (GlobalErrorHandler));
	}
}