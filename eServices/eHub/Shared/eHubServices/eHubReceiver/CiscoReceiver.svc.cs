using System;
using System.Configuration;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;
using Common.Logging;

namespace CargoWise.eHub.Share.eHubServices.eHubReceiver
{
	public class CiscoReceiver : ICiscoReceiver
	{
		readonly ILog logger;

		static readonly ILog StaticLogger = LogManager.GetLogger(typeof(CiscoReceiver));

		public CiscoReceiver()
			: this(StaticLogger)
		{

		}

		public CiscoReceiver(ILog logger)
		{
			if (logger == null) throw new ArgumentNullException("logger");
			this.logger = logger;
		}


		public string Receive3B14(string sMsgIdentifier, string sMsgType, int iPriority, string sRequestXML, string sSender, string sReceiver)
		{
			if (sRequestXML == null)
			{
				string errorText = string.Format(@"<responseforsoap xmlns=""http://www.tibco.com/soapresponse.xsd""><status>ERROR</status><timestamp>{0}</timestamp><errorcode>503</errorcode><errordescription>Input string is null</errordescription></responseforsoap>", DateTimeNowString);
				logger.Error(errorText);
				return errorText;
			}

			if (sRequestXML.Length == 0)
			{
				string errorText = string.Format(@"<responseforsoap xmlns=""http://www.tibco.com/soapresponse.xsd""><status>ERROR</status><timestamp>{0}</timestamp><errorcode>503</errorcode><errordescription>Input string is empty</errordescription></responseforsoap>", DateTimeNowString);
				logger.Error(errorText);
				return errorText;
			}

			using (Stream originalStream = sRequestXML.ToStream())
			{
				Stream messageStream = originalStream;

				if (IsPayloadCompressed)
				{
					messageStream = originalStream.DecodeAndDecompressPayload();
				}

				string validationErrors = MessageValidator.Validate(messageStream, DateTimeNowString);

				if (string.IsNullOrEmpty(validationErrors))
				{
					Stream outputMessage = null;
					try
					{
						var builder = new MessageBuilder(EndpointAddress);
						outputMessage = builder.CreateMessage(messageStream);
					}
					catch (Exception ex)
					{
						string errorText = string.Format(@"<responseforsoap xmlns=""http://www.tibco.com/soapresponse.xsd""><status>ERROR</status><timestamp>{0}</timestamp><errorcode>503</errorcode><errordescription>{1}</errordescription></responseforsoap>", DateTimeNowString, ex.Message);
						logger.Error(errorText);
						return errorText;
					}
					finally
					{
						messageStream.Dispose();
					}

					if (outputMessage == null)
					{
						string errorText = string.Format(@"<responseforsoap xmlns=""http://www.tibco.com/soapresponse.xsd""><status>ERROR</status><timestamp>{0}</timestamp><errorcode>503</errorcode><errordescription>{1}</errordescription></responseforsoap>", DateTimeNowString, "outputMessage is null");
						logger.Error(errorText);
						return errorText;
					}

					try
					{
						using (var message = Message.CreateMessage(MessageVersion.Default, "SendMessage", new XmlTextReader(outputMessage)))
						{
							SendToBiztalk(message);
						}

						string returnText = string.Format(@"<responseforsoap xmlns=""http://www.tibco.com/soapresponse.xsd""><status>SUCCESS</status><timestamp>{0}</timestamp><errorcode></errorcode><errordescription></errordescription></responseforsoap>", DateTimeNowString); //string.Format(@"<responseforsoap xmlns=""http://www.tibco.com/soapresponse.xsd""><status>SUCCESS</status><timestamp>{0}</timestamp><errorcode></errorcode><errordescription></errordescription></responseforsoap>", DateTimeNowString);
						logger.Info(returnText);
						return returnText;
					}
					catch (Exception ex)
					{
						string errorText = string.Format(@"<responseforsoap xmlns=""http://www.tibco.com/soapresponse.xsd""><status>ERROR</status><timestamp>{0}</timestamp><errorcode>503</errorcode><errordescription>Unable to send message to eHub. Details: {1}</errordescription></responseforsoap>", DateTimeNowString, ex.Message);
						logger.Error(errorText);
						return errorText;
					}
					finally
					{
						outputMessage.Dispose();
					}
				}

				logger.Error(validationErrors);
				return validationErrors;
			}
		}

		protected virtual Uri EndpointAddress
		{
			get
			{
				return OperationContext.Current.EndpointDispatcher.EndpointAddress.Uri;
			}
		}

		protected virtual void SendToBiztalk(Message message)
		{
			using (var channelFactory = new ChannelFactory<ISendToBiztalk>("SendToBiztalkService"))
			{
				channelFactory.Endpoint.Contract.SessionMode = SessionMode.Allowed;
				var channel = channelFactory.CreateChannel();
				channel.SendMessage(message);
			}
		}

		protected virtual bool IsPayloadCompressed
		{
			get
			{
				try
				{
					return bool.Parse(ConfigurationManager.AppSettings["PayloadCompressed"]);
				}
				catch
				{
					return false;
				}
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
