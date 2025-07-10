using System.Collections.Generic;
using Enterprise.eHubMessaging.Business.DownloadHandler;
using Enterprise.Messaging.Integration;

namespace Enterprise.Services.ServiceHost.Common
{
	public static class CustomHeadersHelper
	{
		public static Dictionary<string, string> CreateCustomHeadersForSyncRequest(IEDIMessage requestEdiMessage, string clientName)
		{
			var customHeaders = new Dictionary<string, string>();
			if (requestEdiMessage != null)
			{
				customHeaders.Add("eAdaptor-ReceiveEDIMessageNumber", requestEdiMessage.EM_MessageNum);
				customHeaders.Add("eAdaptor-MessageType", requestEdiMessage.EM_MessageSubType);
			}
			if (!string.IsNullOrEmpty(clientName))
			{
				customHeaders.Add("eAdaptor-EDIClientName", clientName);
			}
			return customHeaders;
		}

		public static Dictionary<string, string> CreateCustomHeadersForSyncRequest(IEDIMessage requestEdiMessage, string clientName, IEDIMessage responseEdiMessage)
		{
			var customHeaders = CreateCustomHeadersForSyncRequest(requestEdiMessage, clientName);
			if (responseEdiMessage != null)
			{
				customHeaders.Add("eAdaptor-TransmitEDIMessageNumber", responseEdiMessage.EM_MessageNum);
			}
			return customHeaders;
		}

		public static Dictionary<string, string> CreateCustomHeadersForAsyncRequest(IMessageHandlerResult messageHandlerResult)
		{
			var customHeaders = new Dictionary<string, string>();
			if (messageHandlerResult != null)
			{
				customHeaders.Add("eAdaptor-InterchangeNumber", messageHandlerResult.InterchangeNumber);
			}
			return customHeaders;
		}
	}
}
