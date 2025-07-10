using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.eHubMessaging.Business.DownloadHandler;

namespace Enterprise.Services.ServiceHost
{
	public class MessageNumberResult : IMessageHandlerResult
	{
		public ZString InterchangeNumber { get; }

		public ZGuid TrackingID { get; }

		public IEnumerable<ZString> MessageNumbers { get; set; }

		public ZString FailureReason { get; }

		public IEnumerable<ZString> Warnings { get; }

		public IEnumerable<ZString> ExternalReferenceNumbers { get; set; }
	}
}
