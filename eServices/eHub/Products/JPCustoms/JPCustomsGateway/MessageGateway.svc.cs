using System;
using System.IO;
using System.ServiceModel;
using System.Text;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Products.JPCustoms.Client;
using CargoWise.eHub.Products.JPCustoms.Common;
using CargoWise.eHub.Products.JPCustoms.Common.Extensions;
using Common.Logging;

namespace CargoWise.eHub.Products.JPCustoms.Gateway
{
	public class MessageGateway : IMessageGateway
	{
		readonly IJPCustomsSendClient sendClient;
		readonly ILog logger;
		readonly IAuditLogger auditLogger;

		public MessageGateway()
			: this(LogManager.GetLogger(typeof(MessageGateway)), new AuditLogger(new AuditLoggerConfiguration(new DateTimeProvider())), new JPCustomsSendClient(new SmtpMailClientConfiguration(LogManager.GetLogger(typeof(MessageGateway)))))
		{
		}

		public MessageGateway(ILog logger, IAuditLogger auditLogger, IJPCustomsSendClient sendClient)
		{
			if (logger == null) throw new ArgumentNullException("logger");
			this.logger = logger;

			if (auditLogger == null) throw new ArgumentNullException("auditLogger");
			this.auditLogger = auditLogger;

			if (sendClient == null) throw new ArgumentNullException("sendClient");
			this.sendClient = sendClient;
		}

		public void SendLodgement(string message, string audit)
		{
			logger.InfoFormat("{1}: Sending message '{0}'", message, audit);

			if (string.IsNullOrEmpty(message)) throw new FaultException("message is null or empty");
			if (string.IsNullOrEmpty(audit)) throw new FaultException("audit is null or empty");

			try
			{
				using (var encodedStream = new MemoryStream(Encoding.UTF8.GetBytes(message)))
				{
					using (var stream = encodedStream.DecodeAndDecompress())
					{
						stream.SeekBegin();
						sendClient.Send(stream);
						stream.SeekBegin();
						var streamMsg = stream.ReadToEnd();
						auditLogger.MessageSubmitted(streamMsg, audit);
						logger.InfoFormat("{1}: Messsage '{0}' was sent", streamMsg, audit);
					}
				}
			}
			catch (Exception ex)
			{
				logger.ErrorFormat("Error during message sending. Message details: '{0}'", ex, message);
				throw new FaultException(ex.Message);
			}
		}

	}
}
