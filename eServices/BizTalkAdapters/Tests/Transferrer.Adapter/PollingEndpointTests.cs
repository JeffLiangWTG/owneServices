using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter;
using CargoWise.eHub.BizTalkAdapters.Transferrer.Core;
using Common.Logging;
using Common.Logging.Simple;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.TransportProxy.Interop;
using Microsoft.Samples.BizTalk.Adapter.Common;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.BizTalkAdapters.Tests.Transferrer.Adapter
{
	[TestClass]
	public class PollingEndpointTests
	{
		[TestMethod]
		public void TransferrerCore_PollingEndpoint_NoFiles()
		{
			Func<string, XmlDocument, ILog> loggerFactory = (name, config) => new TraceLoggerFactoryAdapter().GetLogger(name);
			var propertyNamespace = "http://test-properties";
			var uri = "Polling://USER@SERVER:PORT";
			var configXml = new XElement("Config",
				new XElement("Port", "1000"),
				new XElement("MoveAfterDownload", "MOVE-PATH")
			);
			Transmitter transmitHandler;
			var message = SetupTransmit(uri, propertyNamespace, loggerFactory, out transmitHandler);
			message.Context.Write("AdapterConfig", propertyNamespace, configXml.ToString());

			var receiveHandler = MockRepository.GenerateStub<IReceiveHandler>();
			receiveHandler.Stub(x => x.OpenAsync(Arg<XmlDocument>.Is.Anything, Arg<ILog>.Is.Anything, Arg<CancellationToken>.Is.Anything)).Return(Task.Delay(0));
			receiveHandler.Stub(x => x.ListServerFilesAsync(Arg<ILog>.Is.Anything, Arg<CancellationToken>.Is.Anything))
				.Return(Task.FromResult(new List<TransferrerFileInfo>()));
			receiveHandler.Stub(x => x.CloseAsync(Arg<ILog>.Is.Anything, Arg<CancellationToken>.Is.Anything)).Return(Task.Delay(0));

			var endpoint = new PollingEndpoint(transmitHandler, "http://cargowise.com/ehub/biztalkadapters/polling#PollingRequest", config => receiveHandler, loggerFactory);
			endpoint.Open(MockRepository.GenerateStub<EndpointParameters>(uri), null, propertyNamespace);
			var result = endpoint.ProcessMessage(message);

			Assert.IsNotNull(result);
			Assert.AreEqual("<?xml version=\"1.0\" encoding=\"utf-8\"?><ns0:PollingResponse Uri=\"Polling://USER@SERVER:PORT\" xmlns:ns0=\"http://cargowise.com/ehub/biztalkadapters/polling\" />", new StreamReader(result.BodyPart.Data).ReadToEnd());
		}

		[TestMethod]
		public void TransferrerCore_PollingEndpoint_ReceiveFiles()
		{
			Func<string, XmlDocument, ILog> loggerFactory = (name, config) => new TraceLoggerFactoryAdapter().GetLogger(name);
			var propertyNamespace = "http://test-properties";
			var uri = "Polling://USER@SERVER:PORT";
			Transmitter transmitHandler;
			var message = SetupTransmit(uri, propertyNamespace, loggerFactory, out transmitHandler);
			message.Context.Write("LogLevel", "http://cargowise.com/ehub/biztalkadapters/logging-properties", "Trace");

			var files = new List<TransferrerFileInfo>
			{
				new TransferrerFileInfo { Folder = "", Name = "NAME1", Size = 10, Timestamp = new DateTime(2018, 1, 1) },
				new TransferrerFileInfo { Folder = "", Name = "NAME2", Size = 20, Timestamp = new DateTime(2018, 1, 2) }
			};
			var receiveHandler = MockRepository.GenerateStub<IReceiveHandler>();
			receiveHandler.Stub(x => x.OpenAsync(Arg<XmlDocument>.Is.Anything, Arg<ILog>.Is.Anything, Arg<CancellationToken>.Is.Anything)).Return(Task.Delay(0));
			receiveHandler.Stub(x => x.ListServerFilesAsync(Arg<ILog>.Is.Anything, Arg<CancellationToken>.Is.Anything))
				.Return(Task.FromResult(files));
			receiveHandler.Stub(x => x.DownloadAsync(Arg<TransferrerFileInfo>.Is.Anything, Arg<ILog>.Is.Anything, Arg<CancellationToken>.Is.Anything))
				.Do(new Func<TransferrerFileInfo, ILog, CancellationToken, Task<Stream>>((f, l, c) => Task.FromResult((Stream)new MemoryStream(Encoding.UTF8.GetBytes("ABCDEFGHIJKLMNOP")))));
			receiveHandler.Stub(x => x.PostDownloadProcessingAsync(Arg<TransferrerFileInfo>.Is.Anything, Arg<ILog>.Is.Anything)).Return(Task.Delay(0));
			receiveHandler.Stub(x => x.CloseAsync(Arg<ILog>.Is.Anything, Arg<CancellationToken>.Is.Anything)).Return(Task.Delay(0));

			var endpoint = new PollingEndpoint(transmitHandler, "http://cargowise.com/ehub/biztalkadapters/polling#PollingRequest", config => receiveHandler, loggerFactory);
			endpoint.Open(MockRepository.GenerateStub<EndpointParameters>(uri), null, propertyNamespace);
			var result = endpoint.ProcessMessage(message);

			var expectedMessage = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ns0:PollingResponse Uri=\"Polling://USER@SERVER:PORT\" xmlns:ns0=\"http://cargowise.com/ehub/biztalkadapters/polling\"><File Name=\"NAME1\">QUJDREVGR0hJSktMTU5PUA==</File><File Name=\"NAME2\">QUJDREVGR0hJSktMTU5PUA==</File></ns0:PollingResponse>";
			Assert.AreEqual(expectedMessage, new StreamReader(result.BodyPart.Data).ReadToEnd());
			receiveHandler.AssertWasCalled(x => x.PostDownloadProcessingAsync(Arg.Is(files[0]), Arg<ILog>.Is.Anything));
			receiveHandler.AssertWasCalled(x => x.PostDownloadProcessingAsync(Arg.Is(files[1]), Arg<ILog>.Is.Anything));
		}

		[TestMethod]
		public void TransferrerCore_PollingEndpoint_Errors()
		{
			Func<string, XmlDocument, ILog> loggerFactory = (name, config) => new TraceLoggerFactoryAdapter().GetLogger(name);
			var propertyNamespace = "http://test-properties";
			var uri = "Polling://USER@SERVER:PORT";
			Transmitter transmitHandler;
			var message = SetupTransmit(uri, propertyNamespace, loggerFactory, out transmitHandler);

			var receiveHandler = MockRepository.GenerateStub<IReceiveHandler>();
			var endpoint = new PollingEndpoint(transmitHandler, "http://cargowise.com/ehub/biztalkadapters/polling#PollingRequest", config => receiveHandler, loggerFactory);

			receiveHandler.Stub(x => x.OpenAsync(Arg<XmlDocument>.Is.Anything, Arg<ILog>.Is.Anything,
					Arg<CancellationToken>.Is.Anything))
				.Do(new Func<XmlDocument, ILog, CancellationToken, Task>((cfg, log, can) =>
				{
					endpoint.HandlerCancelTokenSource.Cancel();
					return Task.Delay(0);
				}));
			receiveHandler.Stub(x => x.ListServerFilesAsync(Arg<ILog>.Is.Anything, Arg<CancellationToken>.Is.Anything))
				.Return(Task.FromResult(new List<TransferrerFileInfo>()));

			IBaseMessage result;

			message.Context.Write("MessageType", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "http://error#error");
			try
			{
				result = endpoint.ProcessMessage(message);
				Assert.Fail("Expected exception not thrown.");
			}
			catch (Exception ex)
			{
				Assert.IsInstanceOfType(ex, typeof(AggregateException), "Expected exception not thrown.");
				Assert.IsInstanceOfType(ex.InnerException, typeof(TransferrerException), "Expected exception not thrown.");
			}
			message.Context.Write("MessageType", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "http://cargowise.com/ehub/biztalkadapters/polling#PollingRequest");

			endpoint.Open(MockRepository.GenerateStub<EndpointParameters>(uri), null, propertyNamespace);
			result = endpoint.ProcessMessage(message);
			Assert.IsNull(result);
		}


		[TestMethod]
		public void TransferrerCore_PollingEndpoint_CancelReceive()
		{
			Func<string, XmlDocument, ILog> loggerFactory = (name, config) => new TraceLoggerFactoryAdapter().GetLogger(name);
			var propertyNamespace = "http://test-properties";
			var uri = "Polling://USER@SERVER:PORT";
			Transmitter transmitHandler;
			var message = SetupTransmit(uri, propertyNamespace, loggerFactory, out transmitHandler);
			message.Context.Write("LogLevel", "http://cargowise.com/ehub/biztalkadapters/logging-properties", "Trace");

			var files = new List<TransferrerFileInfo>
			{
				new TransferrerFileInfo { Folder = "", Name = "NAME1", Size = 10, Timestamp = new DateTime(2018, 1, 1) },
				new TransferrerFileInfo { Folder = "", Name = "NAME2", Size = 20, Timestamp = new DateTime(2018, 1, 2) }
			};
			var receiveHandler = MockRepository.GenerateStub<IReceiveHandler>();
			var endpoint = new PollingEndpoint(transmitHandler, "http://cargowise.com/ehub/biztalkadapters/polling#PollingRequest", config => receiveHandler, loggerFactory);
			receiveHandler.Stub(x => x.OpenAsync(Arg<XmlDocument>.Is.Anything, Arg<ILog>.Is.Anything, Arg<CancellationToken>.Is.Anything)).Return(Task.Delay(0));
			receiveHandler.Stub(x => x.ListServerFilesAsync(Arg<ILog>.Is.Anything, Arg<CancellationToken>.Is.Anything))
				.Return(Task.FromResult(files));
			receiveHandler.Stub(x => x.DownloadAsync(Arg<TransferrerFileInfo>.Is.Anything, Arg<ILog>.Is.Anything, Arg<CancellationToken>.Is.Anything))
				.Do(new Func<TransferrerFileInfo, ILog, CancellationToken, Task<Stream>>((f, l, c) =>
				{
					if (f.Name == "NAME1")
						return Task.FromResult((Stream) new MemoryStream(Encoding.UTF8.GetBytes("ABCDEFGHIJKLMNOP")));
					else
					{
						endpoint.HandlerCancelTokenSource.Cancel();
						c.ThrowIfCancellationRequested();
						return Task.FromResult((Stream)new MemoryStream());
					}
				}));
			receiveHandler.Stub(x => x.PostDownloadProcessingAsync(Arg<TransferrerFileInfo>.Is.Anything, Arg<ILog>.Is.Anything)).Return(Task.Delay(0));
			receiveHandler.Stub(x => x.CloseAsync(Arg<ILog>.Is.Anything, Arg<CancellationToken>.Is.Anything)).Return(Task.Delay(0));

			endpoint.Open(MockRepository.GenerateStub<EndpointParameters>(uri), null, propertyNamespace);
			var result = endpoint.ProcessMessage(message);

			var expectedMessage = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ns0:PollingResponse Uri=\"Polling://USER@SERVER:PORT\" xmlns:ns0=\"http://cargowise.com/ehub/biztalkadapters/polling\"><File Name=\"NAME1\">QUJDREVGR0hJSktMTU5PUA==</File></ns0:PollingResponse>";
			Assert.AreEqual(expectedMessage, new StreamReader(result.BodyPart.Data).ReadToEnd());
			receiveHandler.AssertWasCalled(x => x.PostDownloadProcessingAsync(Arg.Is(files[0]), Arg<ILog>.Is.Anything));
			receiveHandler.AssertWasNotCalled(x => x.PostDownloadProcessingAsync(Arg.Is(files[1]), Arg<ILog>.Is.Anything));
		}

		static IBaseMessage SetupTransmit(string uri, string propertyNamespace, Func<string, XmlDocument, ILog> loggerFactory, out Transmitter transmitHandler)
		{
			var testMessageFactory = new MessageFactory();
			var transmitMessage = testMessageFactory.CreateMessage();
			XNamespace pollingNs = "http://cargowise.com/ehub/biztalkadapters/polling";
			var pollingRequestMessage = new XElement(pollingNs + "PollingRequest",
				new XElement("Uri", uri),
				new XElement("RenameAfterDownload", "RENAME")
			);
			var messageData = new MemoryStream(Encoding.UTF8.GetBytes(pollingRequestMessage.ToString()));
			transmitMessage.Context = testMessageFactory.CreateMessageContext();
			transmitMessage.AddPart("Body", testMessageFactory.CreateMessagePart(), true);

			transmitMessage.BodyPart.Data = messageData;
			transmitMessage.Context.Write("SPName", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "PORT-NAME");
			transmitMessage.Context.Write("MessageType", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "http://cargowise.com/ehub/biztalkadapters/polling#PollingRequest");

			var responseMessage = MockRepository.GenerateStub<IBaseMessage>();
			var responseMessagePart = MockRepository.GenerateStub<IBaseMessagePart>();
			responseMessage.Stub(x => x.BodyPart).Return(responseMessagePart);

			var handlerConfigXml = new XElement("Config",
				new XElement("InterfacesLogDir", "X:\\InterfacesLogDir"),
				new XElement("TerminateWaitLimit", "5")
			);
			var transportProxy = MockRepository.GenerateStub<IBTTransportProxy>();
			var messageFactory = MockRepository.GenerateStub<IBaseMessageFactory>();
			var handlerProperties = MockRepository.GenerateStub<IPropertyBag>();
			transportProxy.Stub(x => x.GetMessageFactory()).Return(messageFactory);
			messageFactory.Stub(x => x.CreateMessage()).Return(responseMessage);
			handlerProperties.Stub(x => x.Read(Arg.Is("AdapterConfig"), out Arg<object>.Out(handlerConfigXml.ToString()).Dummy,
				Arg.Is(0)));

			transmitHandler = MockRepository.GeneratePartialMock<Transmitter>("Polling Transmitter", "1.0", "Polling Adapter",
				"Polling", Guid.Empty, propertyNamespace, null, 1, loggerFactory);
			transmitHandler.Initialize(transportProxy);
			transmitHandler.Load(handlerProperties, 0);
			return transmitMessage;
		}
	}
}
