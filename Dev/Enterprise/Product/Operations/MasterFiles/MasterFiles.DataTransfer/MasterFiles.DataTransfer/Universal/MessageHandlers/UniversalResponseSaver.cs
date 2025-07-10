using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.eHubMessaging.Business.DownloadHandler;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.MasterFiles.DataTransfer
{
	public class UniversalResponseSaver : IHttpXmlMessageSaver
	{
		readonly IXmlSessionTracker xmlSessionTracker;

		public UniversalResponseSaver(IHttpXmlRequestResponse requestMessage, IHttpXmlRequestResponse responseMessage, IXmlSessionTracker xmlSessionTracker)
		{
			RequestMessage = requestMessage;
			ResponseMessage = responseMessage;
			this.xmlSessionTracker = xmlSessionTracker;
			Factory = ((IHttpXmlEDIMessage)ResponseMessage).Factory;
		}

		public BusinessObjectFactory Factory { get; }

		public IHttpXmlRequestResponse RequestMessage { get; }

		public IHttpXmlRequestResponse ResponseMessage { get; }

		public bool IsFinalProcessingAttempt { get; set; } = true;

		public void Save(IHttpXmlProcessingResult processingResult)
		{
			var universalResponseStream = UniversalResponseWriter.CreateResponse(processingResult?.Status ?? EDIMessageStatusList.Codes.Error, processingResult.ResponseMessageText, new StringReader(xmlSessionTracker.ToString()), new SingleMessageResult(((IEDIMessage)RequestMessage).EM_MessageNum, ((IEDIMessage)RequestMessage).EM_ExternalReferenceNumber), xmlSessionTracker.ValidationRuleCollection);
			using (var stream = (SubStreamableStream)new UnclosableStreamWrapper(universalResponseStream))
			{
				((HttpXmlProcessingResult)processingResult).FullResponseMessageText = universalResponseStream;
				ResponseMessage.SetMessageTextSource(stream);
				using (ProcessTask.Loader.SuppressTemplateApplication())
				{
					Factory.Save();
				}
			}
		}
	}

	#region SingleMessageResult

	public class SingleMessageResult : IMessageHandlerResult
	{
		public SingleMessageResult(ZString messageNumber, ZString externalReferenceNumber)
		{
			MessageNumber = messageNumber;
			ExternalReferenceNumber = externalReferenceNumber;
		}

		public ZString MessageNumber { get; }

		public ZString ExternalReferenceNumber { get; }

		IEnumerable<ZString> IMessageHandlerResult.MessageNumbers => new[] { MessageNumber };

		ZString IMessageHandlerResult.InterchangeNumber => ZString.Empty;

		ZGuid IMessageHandlerResult.TrackingID => ZGuid.Empty;

		ZString IMessageHandlerResult.FailureReason => ZString.Empty;

		IEnumerable<ZString> IMessageHandlerResult.ExternalReferenceNumbers => new[] { ExternalReferenceNumber };

		public IEnumerable<ZString> Warnings
		{
			get
			{
				return Enumerable.Empty<ZString>();
			}
		}
	}

	#endregion
}
