using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace Enterprise.Services.ServiceHost
{
	public class eAdaptorStreamServiceErrorHandler : IErrorHandler
	{
		public bool HandleError(Exception exception)
		{
			return true;
		}

		public void ProvideFault(Exception exception, MessageVersion version, ref Message fault)
		{
			var newEx = new FaultException(exception.Message);
			var msgFault = newEx.CreateMessageFault();
			fault = Message.CreateMessage(version, msgFault, newEx.Action);
		}
	}
}
