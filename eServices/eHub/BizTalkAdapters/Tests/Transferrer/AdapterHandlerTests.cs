using System;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter;
using Common.Logging;
using Common.Logging.Simple;
using Microsoft.BizTalk.Component.Interop;
using Moq;
using NUnit.Framework;

namespace CargoWise.eHub.BizTalkAdapters.Tests.Transferrer
{
	public class AdapterHandlerTests
	{
		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TransferrerCore_AdapterHandler_CreateTerminate()
		{
			var loggerFactory = new Mock<ILoggerFactory>();
			loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>(), It.IsAny<XmlDocument>()))
				.Returns(new ConsoleOutLoggerFactoryAdapter().GetLogger(nameof(AdapterHandlerTests)));
			var handlerConfigXml = new XElement("Config",
				new XElement("InterfacesLogDir", "X:\\InterfacesLogDir"),
				new XElement("StructuredLogDir", "X:\\StructuredLogDir"),
				new XElement("TerminateWaitLimit", "5")
			);

			var handlerPropertyBag = new Mock<IPropertyBag>();
			object configVal = handlerConfigXml.ToString();
			handlerPropertyBag.Setup(x => x.Read("AdapterConfig", out configVal, 0));

			var stubEndpoint = new Mock<IDisposable>();
			stubEndpoint.Setup(x => x.Dispose()).Callback(Task.Delay(1100).Wait);

			var adapterHandler = new AdapterHandler(AdapterHandler.Direction.Send, "AdapterType", loggerFactory.Object);
			adapterHandler.Configure(handlerPropertyBag.Object);
			adapterHandler.RegisterEndpoint(stubEndpoint.Object);
			adapterHandler.Terminate();

			Assert.That(adapterHandler.InterfacesLogDir, Is.EqualTo("X:\\InterfacesLogDir"));
			Assert.That(adapterHandler.StructuredLogDir,Is.EqualTo("X:\\StructuredLogDir"));
			Assert.That(adapterHandler.terminateWaitLimit, Is.EqualTo(5));
			stubEndpoint.Verify(x => x.Dispose());
		}

		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TransferrerCore_AdapterHandler_LogError()
		{
			var logger = new Mock<ILog>();
			var testLogger = new ConsoleOutLoggerFactoryAdapter().GetLogger(nameof(AdapterHandlerTests));
			logger.Setup(x => x.ErrorFormat(It.IsAny<string>(), It.IsAny<Exception>(), It.IsAny<object[]>()))
				.Callback(new Action<string, Exception, object[]>((fmt, ex, args) => testLogger.ErrorFormat(fmt, ex, args)));
			logger.SetupGet(x => x.IsErrorEnabled).Returns(true);
			var loggerFactory = new Mock<ILoggerFactory>();
			loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>(), It.IsAny<XmlDocument>())).Returns(logger.Object);

			var exception = new Exception("TEST ERROR");
			const string format = "DETAILS {0} + {1}";
			const string arg1 = "P1";
			const int arg2 = 999;

			var adapterHandler = new AdapterHandler(AdapterHandler.Direction.Send, "AdapterType", loggerFactory.Object);
			adapterHandler.LogInterfaceError(logger.Object, "1", format, exception, arg1, arg2);

			logger.Verify(x => x.ErrorFormat($"(1) {format}", exception, arg1, arg2));
		}
	}
}
