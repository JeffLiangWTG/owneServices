using System;
using System.IO;
using System.ServiceModel;
using System.Text;
using CargoWise.eHub.Products.JPCustoms.Client;
using CargoWise.eHub.Products.JPCustoms.Common;
using CargoWise.eHub.Products.JPCustoms.Gateway;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace CargoWise.eHub.Products.JPCustoms.Tests
{
	[TestClass]
	public class MessageGatewayTests : TestBase
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void MessageGateway()
		{
			var auditLogger = new Mock<IAuditLogger>();
			var sendClient = new Mock<IJPCustomsSendClient>();

			string actualMessage = string.Empty;
			sendClient.Setup(c => c.Send(It.IsAny<Stream>())).Callback((Stream stream) =>
			{
				actualMessage = new StreamReader(stream).ReadToEnd();
			});

			var service = new MessageGateway(Logger, auditLogger.Object, sendClient.Object);
			service.SendLodgement("H4sIAAAAAAAEAO29B2AcSZYlJi9tynt/SvVK1+B0oQiAYBMk2JBAEOzBiM3mkuwdaUcjKasqgcplVmVdZhZAzO2dvPfee++999577733ujudTif33/8/XGZkAWz2zkrayZ4hgKrIHz9+fB8/It7kTZt+5+XJummrRZMu8qbJLvL/ByEtBxsWAAAA", "168.192.1.45");

			auditLogger.Verify(a => a.MessageSubmitted("Test JPCustoms message", "168.192.1.45"));
			Assert.AreEqual("Test JPCustoms message", actualMessage);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		[ExpectedException(typeof(FaultException), "Decoding exception")]
		public void MessageGateway_Message_DecodingException()
		{
			var auditLogger = new Mock<IAuditLogger>();
			var sendClient = new Mock<IJPCustomsSendClient>();

			var service = new MessageGateway(Logger, auditLogger.Object, sendClient.Object);
			service.SendLodgement("Bad message", "168.192.1.45");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		[ExpectedException(typeof(FaultException), "Some error during audit")]
		public void MessageGateway_AuditLogger_Exception()
		{
			using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes("Test JPCustoms message")))
			{
				var auditLogger = new Mock<IAuditLogger>();
				var sendClient = new Mock<IJPCustomsSendClient>();

				auditLogger.Setup(a => a.MessageSubmitted("Test JPCustoms message", "168.192.1.45"))
					.Throws(new Exception("Some error during audit"));
				;
				sendClient.Setup(c => c.Send(messageStream));

				var service = new MessageGateway(Logger, auditLogger.Object, sendClient.Object);
				service.SendLodgement(
					"H4sIAAAAAAAEAO29B2AcSZYlJi9tynt/SvVK1+B0oQiAYBMk2JBAEOzBiM3mkuwdaUcjKasqgcplVmVdZhZAzO2dvPfee++999577733ujudTif33/8/XGZkAWz2zkrayZ4hgKrIHz9+fB8/It7kTZt+5+XJummrRZMu8qbJLvL/ByEtBxsWAAAA",
					"168.192.1.45");
			}
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		[ExpectedException(typeof(FaultException), "Some error during send")]
		public void MessageGateway_SendClient_Exception()
		{
			var auditLogger = new Mock<IAuditLogger>();
			var sendClient = new Mock<IJPCustomsSendClient>();

			auditLogger.Setup(a => a.MessageSubmitted("Test JPCustoms message", "168.192.1.45"));
			sendClient.Setup(c => c.Send(It.IsAny<Stream>())).Throws(new Exception("Some error during send"));

			var service = new MessageGateway(Logger, auditLogger.Object, sendClient.Object);
			service.SendLodgement(
				"H4sIAAAAAAAEAO29B2AcSZYlJi9tynt/SvVK1+B0oQiAYBMk2JBAEOzBiM3mkuwdaUcjKasqgcplVmVdZhZAzO2dvPfee++999577733ujudTif33/8/XGZkAWz2zkrayZ4hgKrIHz9+fB8/It7kTZt+5+XJummrRZMu8qbJLvL/ByEtBxsWAAAA",
				"168.192.1.45");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		[ExpectedException(typeof(FaultException), "Message is null")]
		public void MessageGateway_Message_NullException()
		{
			var auditLogger = new AuditLogger(AuditLoggerConfiguration);
			var sendClient = new JPCustomsSendClient(SmtpMailClientConfiguration);
			var service = new MessageGateway(Logger, auditLogger, sendClient);

			service.SendLodgement(null, "168.192.1.45");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		[ExpectedException(typeof(FaultException), "Message is empty")]
		public void MessageGateway_Message_EmptyException()
		{
			var auditLogger = new AuditLogger(AuditLoggerConfiguration);
			var sendClient = new JPCustomsSendClient(SmtpMailClientConfiguration);
			var service = new MessageGateway(Logger, auditLogger, sendClient);

			service.SendLodgement("", "168.192.1.45");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		[ExpectedException(typeof(FaultException), "Audit is null")]
		public void MessageGateway_Audit_NullException()
		{
			var auditLogger = new AuditLogger(AuditLoggerConfiguration);
			var sendClient = new JPCustomsSendClient(SmtpMailClientConfiguration);
			var service = new MessageGateway(Logger, auditLogger, sendClient);

			service.SendLodgement("H4sIAAAAAAAEAO29B2AcSZYlJi9tynt/SvVK1+B0oQiAYBMk2JBAEOzBiM3mkuwdaUcjKasqgcplVmVdZhZAzO2dvPfee++999577733ujudTif33/8/XGZkAWz2zkrayZ4hgKrIHz9+fB8/It7kTZt+5+XJummrRZMu8qbJLvL/ByEtBxsWAAAA", null);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		[ExpectedException(typeof(FaultException), "Audit is empty")]
		public void MessageGateway_Audit_EmptyException()
		{
			var auditLogger = new AuditLogger(AuditLoggerConfiguration);
			var sendClient = new JPCustomsSendClient(SmtpMailClientConfiguration);
			var service = new MessageGateway(Logger, auditLogger, sendClient);

			service.SendLodgement("H4sIAAAAAAAEAO29B2AcSZYlJi9tynt/SvVK1+B0oQiAYBMk2JBAEOzBiM3mkuwdaUcjKasqgcplVmVdZhZAzO2dvPfee++999577733ujudTif33/8/XGZkAWz2zkrayZ4hgKrIHz9+fB8/It7kTZt+5+XJummrRZMu8qbJLvL/ByEtBxsWAAAA", "");
		}
	}
}
