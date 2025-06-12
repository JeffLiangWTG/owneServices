using System;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter;
using Common.Logging;
using Common.Logging.Simple;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.BizTalkAdapters.Tests.Transferrer.Adapter
{
	[TestClass]
	public class AdapterHandlerTests
	{
		[TestMethod]
		public void TransferrerCore_AdapterHandler_CreateTerminate()
		{
			Func<string, XmlDocument, ILog> loggerFactory = (name, config) => new TraceLoggerFactoryAdapter().GetLogger(typeof(AdapterHandlerTests).Name);
			var handlerConfigXml = new XElement("Config",
				new XElement("InterfacesLogDir", "X:\\InterfacesLogDir"),
				new XElement("TerminateWaitLimit", "5")
			);

			var handlerPropertyBag = MockRepository.GenerateStub<IPropertyBag>();
			handlerPropertyBag.Stub(x => x.Read(Arg.Is("AdapterConfig"), out Arg<object>.Out(handlerConfigXml.ToString()).Dummy, Arg.Is(0)));

			var stubEndpoint = MockRepository.GenerateStub<IDisposable>();
			stubEndpoint.Stub(x => x.Dispose()).Do(new Action(Task.Delay(1100).Wait));

			var adapterHandler = new AdapterHandler(AdapterHandler.Direction.Send, "AdapterType", loggerFactory);
			adapterHandler.Configure(handlerPropertyBag);
			adapterHandler.RegisterEndpoint(stubEndpoint);
			adapterHandler.Terminate();

			Assert.AreEqual("X:\\InterfacesLogDir", adapterHandler.InterfacesLogDir);
			Assert.AreEqual(5, adapterHandler.terminateWaitLimit);
			stubEndpoint.AssertWasCalled(x => x.Dispose());
		}

		[TestMethod]
		public void TransferrerCore_AdapterHandler_LogError()
		{
			var logger = MockRepository.GenerateStub<ILog>();
			var traceLogger = new TraceLoggerFactoryAdapter().GetLogger(typeof(AdapterHandlerTests).Name);
			logger.Stub(x => x.ErrorFormat(Arg<string>.Is.Anything, Arg<Exception>.Is.Anything, Arg<object[]>.Is.Anything))
				.Do(new Action<string, Exception, object[]>((fmt, ex, args) => traceLogger.ErrorFormat(fmt, ex, args)));

			var exception = new Exception("TEST ERROR");
			const string format = "DETAILS {0} + {1}";
			const string arg1 = "P1";
			const int arg2 = 999;

			var adapterHandler = new AdapterHandler(AdapterHandler.Direction.Send, "AdapterType", (name, config) => logger);
			adapterHandler.LogInterfaceError(logger, format, exception, arg1, arg2);

			logger.AssertWasCalled(x => x.ErrorFormat(format, exception, arg1, arg2));
		}
	}
}
