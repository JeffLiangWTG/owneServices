using System;
using System.Threading;
using System.Xml.Linq;
using Common.Logging;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.TransportProxy.Interop;
using Microsoft.Samples.BizTalk.Adapter.Common;
using Moq;
using NUnit.Framework;
using Receiver = CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter.Receiver;
using ReceiverEndpoint = CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter.ReceiverEndpoint;

namespace CargoWise.eHub.BizTalkAdapters.Tests.Transferrer
{
	public class ReceiverEndpointTests
	{
		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void ReceiverEndpoint_Open_Update_Close()
		{
			var transportType = nameof(ReceiverEndpoint_Open_Update_Close);
			var handlerConfigXml = new XElement("Config",
				new XElement("InterfacesLogDir", "X:\\InterfacesLogDir"),
				new XElement("StructuredLogDir", "X:\\StructuredLogDir"),
				new XElement("TerminateWaitLimit", "5")
			);
			var handlerPropertyBag = new Mock<IPropertyBag>();
			object configVal = handlerConfigXml.ToString();
			handlerPropertyBag.Setup(x => x.Read("AdapterConfig", out configVal, 0));
			var transportProxy = new Mock<IBTTransportProxy>();
			var receiver = new Mock<Receiver>("", "", "", transportType, Guid.Empty, "", typeof(ReceiverEndpoint), true) { CallBase = true };
			receiver.Object.Load(handlerPropertyBag.Object, 0);

			var target = new Mock<ReceiverEndpoint>() { CallBase = true };
			target.Setup(x => x.TryGetPortName(transportType, "uri1")).Returns("portName");

			target.Object.Open("uri1", handlerPropertyBag.Object, null, null, transportProxy.Object, transportType, "ns",
				new ControlledTermination());

			target.Object.Update(handlerPropertyBag.Object, null, null);

			target.Object.Dispose();

			target.Verify(x => x.EndpointTask(It.IsAny<CancellationToken>()), Times.Exactly(2));
		}

		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void ReceiverEndpoint_InterfaceError()
		{
			var callbackInvoked = false;
			var exception = new AdapterException("Test exception.");
			var transportType = nameof(ReceiverEndpoint_InterfaceError);
			var handlerConfigXml = new XElement("Config",
				new XElement("InterfacesLogDir", "X:\\InterfacesLogDir"),
				new XElement("StructuredLogDir", "X:\\StructuredLogDir"),
				new XElement("TerminateWaitLimit", "5")
			);
			var logger = new Mock<ILog>();
			logger.Setup(x => x.IsErrorEnabled).Returns(true);
			logger.Setup(x => x.ErrorFormat("(00000001) Error in receive location '{0}'", exception, "portName"))
				.Callback<string, Exception, object[]>((_, _, _) => callbackInvoked = true);
			var handlerPropertyBag = new Mock<IPropertyBag>();
			object configVal = handlerConfigXml.ToString();
			handlerPropertyBag.Setup(x => x.Read("AdapterConfig", out configVal, 0));
			var transportProxy = new Mock<IBTTransportProxy>();
			var receiver = new Mock<Receiver>("", "", "", transportType, Guid.Empty, "", typeof(ReceiverEndpoint), true) { CallBase = true };
			receiver.Object.Load(handlerPropertyBag.Object, 0);

			var target = new Mock<ReceiverEndpoint>() { CallBase = true };
			target.Setup(x => x.Logger).Returns(logger.Object);
			target.Setup(x => x.TryGetPortName(transportType, "uri")).Returns("portName");
			target.Setup(x => x.EndpointTask(It.IsAny<CancellationToken>())).Throws(exception);

			target.Object.Open("uri", handlerPropertyBag.Object, null, null, transportProxy.Object, transportType, "ns", new ControlledTermination());
			var isComplete = SpinWait.SpinUntil(() => callbackInvoked, TimeSpan.FromSeconds(5));

			Assert.That(isComplete, Is.True);
			logger.Verify(x => x.ErrorFormat("(00000001) Error in receive location '{0}'", exception, "portName"), Times.Once());
			target.Verify(x => x.EndpointTask(It.IsAny<CancellationToken>()), Times.Once);
		}
	}
}
