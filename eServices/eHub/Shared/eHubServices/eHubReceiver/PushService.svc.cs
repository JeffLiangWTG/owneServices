using System;
using System.IO;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using System.Xml;
using Common.Logging;
using Newtonsoft.Json;

namespace CargoWise.eHub.Share.eHubServices.eHubReceiver
{
	public class PushService : IPushService
	{
		readonly ILog logger;
		readonly ILog messageLogger;

		static readonly ILog StaticLogger = LogManager.GetLogger("Log");
		static readonly ILog MessageStaticLogger = LogManager.GetLogger("Messages");

		public PushService()
			: this(StaticLogger, MessageStaticLogger)
		{

		}

		public PushService(ILog logger, ILog messageLogger)
		{
			if (logger == null) throw new ArgumentNullException("logger");
			if (messageLogger == null) throw new ArgumentNullException("messageLogger");
			this.logger = logger;
			this.messageLogger = messageLogger;
		}

		public void PostContainerEvent(Stream dataStream)
		{
			string messageToLog = string.Empty;
			try
			{
				var jsonText = new StreamReader(dataStream).ReadToEnd();
				messageLogger.Info(jsonText);
				messageToLog = jsonText.TruncateForLogging();

				var doc = JsonConvert.DeserializeXmlNode("{\"root\":" + jsonText + "}", "root");
				var memoryStream = new MemoryStream();
				doc.Save(memoryStream);

				var builder = new MessageBuilder(EndpointAddress);
				using (var outputMessage = builder.CreateMessage(memoryStream))
				{

					if (outputMessage == null)
					{
						logger.Error("PostContainerEvent outputMessage is null.");
						throw new ArgumentException("PostContainerEvent outputMessage is null.");
					}

					SendToBiztalk(outputMessage);
					logger.InfoFormat("Message received.  {0}", messageToLog);
				}

				if (WebOperationContext.Current == null) return;
				WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.OK;
			}
			catch (Exception ex)
			{
				logger.ErrorFormat("PushService error. {0}", ex, messageToLog);
				if (WebOperationContext.Current == null) return;
				WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.ServiceUnavailable;
				WebOperationContext.Current.OutgoingResponse.StatusDescription = ex.Message;
			}
		}

		protected virtual Uri EndpointAddress
		{
			get
			{
				return OperationContext.Current.EndpointDispatcher.EndpointAddress.Uri;
			}
		}

		protected virtual void SendToBiztalk(Stream outputMessage)
		{
			using (var message = Message.CreateMessage(MessageVersion.Default, "SendMessage", new XmlTextReader(outputMessage)))
			using (var channelFactory = new ChannelFactory<ISendToBiztalk>("SendToBiztalkService"))
			{
				channelFactory.Endpoint.Contract.SessionMode = SessionMode.Allowed;
				var channel = channelFactory.CreateChannel();
				channel.SendMessage(message);
			}
		}

		protected virtual string DateTimeNowString
		{
			get
			{
				return DateTimeOffset.Now.ToString("yyyy-MM-ddTHH:mm:sszzzz");
			}
		}
	}
}
