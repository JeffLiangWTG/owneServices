using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml;
using System.Xml.Linq;
using CargoWise.eHub.BizTalkAdapters.HttpEx;
using CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter;
using Common.Logging.Simple;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.TransportProxy.Interop;
using Microsoft.Samples.BizTalk.Adapter.Common;
using Microsoft.Test.BizTalk.PipelineObjects;
using NUnit.Framework;
using Rhino.Mocks;

namespace CargoWise.eHub.BizTalkAdapters.Tests.HttpEx
{
	public class TransmitterEndpointTests
	{
		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void HttpEx_TransmitterEndpoint_RequestResponse_Success()
		{
			var requestMessageText = Encoding.UTF8.GetString(GetEmbeddedResourceBytes("TestFiles.BigUpload.txt"));
			var responseMessageText = "RESPONSE MESSAGE TEXT";
			var transmitMessage = CreateTransmitMessage(new MemoryStream(Encoding.UTF8.GetBytes(requestMessageText)));
			transmitMessage.Context.Write("UserHttpHeaders", "http://schemas.microsoft.com/BizTalk/2003/http-properties", "X-ExtraHeaders: More data" + Environment.NewLine + "UserName-Agent: Vendor=WTG; Version=1.0");
			transmitMessage.Context.Write("CustomHeaderNamesWithoutValidation", HttpExTransmitter.HttpExPropertyNamespace, "UserName-Agent");
			transmitMessage.Context.Write("DestinationUrl", HttpExTransmitter.HttpExPropertyNamespace, "http://override-url/encoded%2Fpath");
			var mockHttpExClient = MockRepository.GenerateMock<IHttpExClient>();
			var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
			httpResponseMessage.Content = new StringContent(responseMessageText);
			httpResponseMessage.Headers.Add("X-CustomResponse", new[] { "CUSTOM RESPONSE PART 1", "CUSTOM RESPONSE PART 2" });
			mockHttpExClient.Expect(x => x.SendAsync(Arg<HttpRequestMessage>.Matches(r =>
				r.Method == HttpMethod.Post &&
				r.RequestUri.ToString() == "http://override-url/encoded%2Fpath" &&
				string.Join(Environment.NewLine, r.Headers.Select(h => h.Key + ": " + string.Join(" ", h.Value))) ==
					"X-CustomHeader: CUSTOM_VALUE" + Environment.NewLine +
					"X-ExtraHeaders: More data" + Environment.NewLine +
					"UserName-Agent: Vendor=WTG; Version=1.0" &&
				r.Content.ReadAsStringAsync().Result == requestMessageText
			))).Return(Task.FromResult(httpResponseMessage));

			var mockHandlerFactory = MockRepository.GenerateMock<IHttpExClientHandlerFactory>();
			var clientHandler = new WebRequestHandler();
			mockHandlerFactory.Expect(x => x.CreateHandler()).Return(clientHandler);
			var mockClientFactory = MockRepository.GenerateMock<IHttpExClientFactory>();
			mockClientFactory.Expect(x => x.CreateHttpExClient(clientHandler)).Return(mockHttpExClient);

			var httpExTransmitterEndpoint = GetHttpExTransmitterEndpoint(mockHandlerFactory, mockClientFactory);

			var result = httpExTransmitterEndpoint.ProcessMessage(transmitMessage);

			mockHttpExClient.VerifyAllExpectations();
			Assert.IsNotNull(result);
			Assert.AreEqual(200,
				(int)result.Context.Read("ResponseStatusCode", "http://schemas.microsoft.com/BizTalk/2003/http-properties"));
			Assert.AreEqual(@"X-CustomResponse: CUSTOM RESPONSE PART 1, CUSTOM RESPONSE PART 2
Content-Type: text/plain; charset=utf-8
", (string)result.Context.Read("InboundHttpHeaders", "http://schemas.microsoft.com/BizTalk/2003/http-properties"));
			using (var sr = new StreamReader(result.BodyPart.GetOriginalDataStream()))
				Assert.AreEqual(responseMessageText, sr.ReadToEnd());
		}

		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void HttpEx_TransmitterEndpoint_RequestResponse_Terminated()
		{
			var responseMessageText = "RESPONSE MESSAGE TEXT";
			var transmitMessage = CreateTransmitMessage();
			var mockHttpExClient = MockRepository.GenerateMock<IHttpExClient>();
			var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
			httpResponseMessage.Content = new StringContent(responseMessageText);
			mockHttpExClient.Expect(x => x.SendAsync(Arg<HttpRequestMessage>.Is.Anything))
				.Return(Task.FromResult(httpResponseMessage));

			var mockHandlerFactory = MockRepository.GenerateMock<IHttpExClientHandlerFactory>();
			var clientHandler = new WebRequestHandler();
			mockHandlerFactory.Expect(x => x.CreateHandler()).Return(clientHandler);
			var mockClientFactory = MockRepository.GenerateMock<IHttpExClientFactory>();
			mockClientFactory.Expect(x => x.CreateHttpExClient(clientHandler)).Return(mockHttpExClient);

			var httpExTransmitterEndpoint = GetHttpExTransmitterEndpoint(mockHandlerFactory, mockClientFactory);
			var result = httpExTransmitterEndpoint.ProcessMessage(transmitMessage);

			mockHttpExClient.VerifyAllExpectations();
			Assert.IsNotNull(result);
			Assert.AreEqual(400,
				(int)result.Context.Read("ResponseStatusCode", "http://schemas.microsoft.com/BizTalk/2003/http-properties"));
			Assert.AreEqual(@"Content-Type: text/plain; charset=utf-8
", (string)result.Context.Read("InboundHttpHeaders", "http://schemas.microsoft.com/BizTalk/2003/http-properties"));
			using (var sr = new StreamReader(result.BodyPart.GetOriginalDataStream()))
				Assert.AreEqual(responseMessageText, sr.ReadToEnd());
		}

		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void HttpEx_TransmitterEndpoint_OneWay_Success()
		{
			var transmitMessage = CreateTransmitMessage(isTwoWay: false);

			var mockHttpExClient = MockRepository.GenerateMock<IHttpExClient>();
			var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
			mockHttpExClient.Expect(x => x.SendAsync(Arg<HttpRequestMessage>.Is.Anything))
				.Return(Task.FromResult(httpResponseMessage));
			var mockHandlerFactory = MockRepository.GenerateMock<IHttpExClientHandlerFactory>();
			var clientHandler = new WebRequestHandler();
			mockHandlerFactory.Expect(x => x.CreateHandler()).Return(clientHandler);
			var mockClientFactory = MockRepository.GenerateMock<IHttpExClientFactory>();
			mockClientFactory.Expect(x => x.CreateHttpExClient(clientHandler)).Return(mockHttpExClient);

			var httpExTransmitterEndpoint = GetHttpExTransmitterEndpoint(mockHandlerFactory, mockClientFactory);
			var result = httpExTransmitterEndpoint.ProcessMessage(transmitMessage);

			mockHttpExClient.VerifyAllExpectations();
			mockHandlerFactory.VerifyAllExpectations();
			mockClientFactory.VerifyAllExpectations();

			Assert.IsNull(result);
		}

		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void HttpEx_TransmitterEndpoint_OneWay_Terminated()
		{
			var transmitMessage = CreateTransmitMessage(isTwoWay: false);
			var mockHttpExClient = MockRepository.GenerateMock<IHttpExClient>();
			var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
			mockHttpExClient.Expect(x => x.SendAsync(Arg<HttpRequestMessage>.Is.Anything))
				.Return(Task.FromResult(httpResponseMessage));

			var mockHandlerFactory = MockRepository.GenerateMock<IHttpExClientHandlerFactory>();
			var clientHandler = new WebRequestHandler();
			mockHandlerFactory.Expect(x => x.CreateHandler()).Return(clientHandler);
			var mockClientFactory = MockRepository.GenerateMock<IHttpExClientFactory>();
			mockClientFactory.Expect(x => x.CreateHttpExClient(clientHandler)).Return(mockHttpExClient);

			var httpExTransmitterEndpoint = GetHttpExTransmitterEndpoint(mockHandlerFactory, mockClientFactory);

			var ex = Assert.Throws<TransferrerException>(() => httpExTransmitterEndpoint.ProcessMessage(transmitMessage));
			Assert.That(ex.Message, Is.EqualTo("HTTP 'POST' request to http://destination-url/ failed with status (400) BadRequest"));
		}

		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void HttpEx_TransmitterEndpoint_TransientFailure()
		{
			var transmitMessage = CreateTransmitMessage();
			var stubHttpExClient = MockRepository.GenerateStub<IHttpExClient>();
			stubHttpExClient.Stub(x => x.SendAsync(Arg<HttpRequestMessage>.Is.Anything))
				.Throw(new TimeoutException());

			var mockHandlerFactory = MockRepository.GenerateMock<IHttpExClientHandlerFactory>();
			var clientHandler = new WebRequestHandler();
			mockHandlerFactory.Expect(x => x.CreateHandler()).Return(clientHandler);
			var mockClientFactory = MockRepository.GenerateMock<IHttpExClientFactory>();
			mockClientFactory.Expect(x => x.CreateHttpExClient(clientHandler)).Return(stubHttpExClient);

			var httpExTransmitterEndpoint = GetHttpExTransmitterEndpoint(mockHandlerFactory, mockClientFactory);

			var ex = Assert.Throws<TransferrerException>(() => httpExTransmitterEndpoint.ProcessMessage(transmitMessage));
			Assert.That(ex.Message, Is.EqualTo("HTTP 'POST' request to http://destination-url/ failed with error: (System.TimeoutException) The operation has timed out."));
		}

		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void HttpEx_TransmitterEndpoint_TransientFailureTransactionException()
		{
			var transmitMessage = CreateTransmitMessage();
			var stubHttpExClient = MockRepository.GenerateStub<IHttpExClient>();
			stubHttpExClient.Stub(x => x.SendAsync(Arg<HttpRequestMessage>.Is.Anything))
				.Throw(new TransactionException("Test Exception Message"));

			var mockHandlerFactory = MockRepository.GenerateMock<IHttpExClientHandlerFactory>();
			var clientHandler = new WebRequestHandler();
			mockHandlerFactory.Expect(x => x.CreateHandler()).Return(clientHandler);
			var mockClientFactory = MockRepository.GenerateMock<IHttpExClientFactory>();
			mockClientFactory.Expect(x => x.CreateHttpExClient(clientHandler)).Return(stubHttpExClient);

			var httpExTransmitterEndpoint = GetHttpExTransmitterEndpoint(mockHandlerFactory, mockClientFactory);

			var ex = Assert.Throws<TransferrerException>(() => httpExTransmitterEndpoint.ProcessMessage(transmitMessage));
			Assert.That(ex.Message, Is.EqualTo("HTTP 'POST' request to http://destination-url/ failed with error: (System.Transactions.TransactionException) Test Exception Message"));
		}

		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void HttpEx_TransmitterEndpoint_TwoWay_Error()
		{
			var transmitMessage = CreateTransmitMessage(isTwoWay: true);
			var mockHttpExClient = MockRepository.GenerateMock<IHttpExClient>();
			var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.MovedPermanently);
			mockHttpExClient.Expect(x => x.SendAsync(Arg<HttpRequestMessage>.Is.Anything))
				.Return(Task.FromResult(httpResponseMessage));

			var mockHandlerFactory = MockRepository.GenerateMock<IHttpExClientHandlerFactory>();
			var clientHandler = new WebRequestHandler();
			mockHandlerFactory.Expect(x => x.CreateHandler()).Return(clientHandler);
			var mockClientFactory = MockRepository.GenerateMock<IHttpExClientFactory>();
			mockClientFactory.Expect(x => x.CreateHttpExClient(clientHandler)).Return(mockHttpExClient);

			var httpExTransmitterEndpoint = GetHttpExTransmitterEndpoint(mockHandlerFactory, mockClientFactory);

			var ex = Assert.Throws<TransferrerException>(() => httpExTransmitterEndpoint.ProcessMessage(transmitMessage));
			Assert.That(ex.Message, Is.EqualTo("HTTP 'POST' request to http://destination-url/ failed with status (301) MovedPermanently"));
		}

		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void HttpEx_TransmitterEndpoint_OneWay_SendWithClientCertificate()
		{
			var transmitMessage = CreateTransmitMessage(isTwoWay: false);
			var certificateBytes = GetEmbeddedResourceBytes("TestFiles.tempClientcert.pfx");
			transmitMessage.Context.Write("Certificate", "http://cargowise.com/ehub/biztalkadapters/httpex-properties", Convert.ToBase64String(certificateBytes));
			transmitMessage.Context.Write("CertificatePassphrase", "http://cargowise.com/ehub/biztalkadapters/httpex-properties", "3hubRock$");

			var mockHttpExClient = MockRepository.GenerateMock<IHttpExClient>();
			var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
			mockHttpExClient.Expect(x => x.SendAsync(Arg<HttpRequestMessage>.Is.Anything))
				.Return(Task.FromResult(httpResponseMessage));
			var mockHandlerFactory = MockRepository.GenerateMock<IHttpExClientHandlerFactory>();
			var clientHandler = new WebRequestHandler();
			mockHandlerFactory.Expect(x => x.CreateHandler()).Return(clientHandler);
			var mockClientFactory = MockRepository.GenerateMock<IHttpExClientFactory>();
			mockClientFactory.Expect(x => x.CreateHttpExClient(clientHandler)).Return(mockHttpExClient);

			var httpExTransmitterEndpoint = GetHttpExTransmitterEndpoint(mockHandlerFactory, mockClientFactory);
			var result = httpExTransmitterEndpoint.ProcessMessage(transmitMessage);

			mockHttpExClient.VerifyAllExpectations();
			mockHandlerFactory.VerifyAllExpectations();
			mockClientFactory.VerifyAllExpectations();

			Assert.AreEqual(2, clientHandler.ClientCertificates.Count);
			Assert.AreEqual("37A049B9A5B2DEAF44921E8620A7F293", clientHandler.ClientCertificates[0].GetSerialNumberString());
			Assert.AreEqual("EB828E0B3FAD4D854187274B030EC9E8", clientHandler.ClientCertificates[1].GetSerialNumberString());
			Assert.IsNull(result);
		}

		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void HttpEx_TransmitterEndpoint_OneWay_DoesNotReuseCertificate()
		{
			var transmitMessage = CreateTransmitMessage(isTwoWay: false);
			var certificateBytes = GetEmbeddedResourceBytes("TestFiles.tempClientcert.pfx");
			transmitMessage.Context.Write("Certificate", "http://cargowise.com/ehub/biztalkadapters/httpex-properties", Convert.ToBase64String(certificateBytes));
			transmitMessage.Context.Write("CertificatePassphrase", "http://cargowise.com/ehub/biztalkadapters/httpex-properties", "3hubRock$");

			var mockHttpExClient = MockRepository.GenerateMock<IHttpExClient>();
			var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
			mockHttpExClient.Expect(x => x.SendAsync(Arg<HttpRequestMessage>.Is.Anything))
				.Return(Task.FromResult(httpResponseMessage));
			var mockHandlerFactory = MockRepository.GenerateMock<IHttpExClientHandlerFactory>();
			var clientHandler = new WebRequestHandler();
			mockHandlerFactory.Expect(x => x.CreateHandler()).Return(clientHandler).Repeat.Once();
			var mockClientFactory = MockRepository.GenerateMock<IHttpExClientFactory>();
			mockClientFactory.Expect(x => x.CreateHttpExClient(clientHandler)).Return(mockHttpExClient).Repeat.Once();

			var mockHttpExClient2 = MockRepository.GenerateMock<IHttpExClient>();
			var httpResponseMessage2 = new HttpResponseMessage(HttpStatusCode.OK);
			mockHttpExClient2.Expect(x => x.SendAsync(Arg<HttpRequestMessage>.Is.Anything))
				.Return(Task.FromResult(httpResponseMessage2));
			var clientHandler2 = new WebRequestHandler();
			mockHandlerFactory.Expect(x => x.CreateHandler()).Return(clientHandler2).Repeat.Once();
			mockClientFactory.Expect(x => x.CreateHttpExClient(clientHandler2)).Return(mockHttpExClient2).Repeat.Once();

			var httpExTransmitterEndpoint = GetHttpExTransmitterEndpoint(mockHandlerFactory, mockClientFactory);
			var result = httpExTransmitterEndpoint.ProcessMessage(transmitMessage);

			Assert.AreEqual(2, clientHandler.ClientCertificates.Count);
			Assert.AreEqual("37A049B9A5B2DEAF44921E8620A7F293", clientHandler.ClientCertificates[0].GetSerialNumberString());
			Assert.AreEqual("EB828E0B3FAD4D854187274B030EC9E8", clientHandler.ClientCertificates[1].GetSerialNumberString());
			Assert.IsNull(result);

			var transmitMessage2 = CreateTransmitMessage(isTwoWay: false);

			var result2 = httpExTransmitterEndpoint.ProcessMessage(transmitMessage2);

			mockHttpExClient.VerifyAllExpectations();
			mockHttpExClient2.VerifyAllExpectations();
			mockHandlerFactory.VerifyAllExpectations();
			mockClientFactory.VerifyAllExpectations();

			Assert.AreEqual(0, clientHandler2.ClientCertificates.Count);
			Assert.IsNull(result2);
		}

		[Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		[Test(Description = "When HTTP method matches item in SuppressMessageBodyForHttpVerbs configuration setting, HTTP request must be sent without a body.")]
		public void HttpEx_TransmitterEndpoint_SuppressMessageBodyForHttpVerbs()
		{
			var requestMessageText = "REQUEST MESSAGE TEXT";
			var transmitMessage = CreateTransmitMessage(new MemoryStream(Encoding.UTF8.GetBytes(requestMessageText)), true, "GET");
			var mockHttpExClient = MockRepository.GenerateMock<IHttpExClient>();
			var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("RESPONSE") };
			mockHttpExClient.Stub(x => x.SendAsync(Arg<HttpRequestMessage>.Is.Anything))
				.Return(Task.FromResult(httpResponseMessage));
			var mockHandlerFactory = MockRepository.GenerateMock<IHttpExClientHandlerFactory>();
			var clientHandler = new WebRequestHandler();
			mockHandlerFactory.Stub(x => x.CreateHandler()).Return(clientHandler);
			var mockClientFactory = MockRepository.GenerateMock<IHttpExClientFactory>();
			mockClientFactory.Stub(x => x.CreateHttpExClient(clientHandler)).Return(mockHttpExClient);

			var httpExTransmitterEndpoint = GetHttpExTransmitterEndpoint(mockHandlerFactory, mockClientFactory);
			var result = httpExTransmitterEndpoint.ProcessMessage(transmitMessage);

			mockHttpExClient.AssertWasCalled(x => x.SendAsync(Arg<HttpRequestMessage>.Matches(r => r.Method == HttpMethod.Get && r.Content == null)));
			Assert.IsNotNull(result);
			Assert.AreEqual(200, (int)result.Context.Read("ResponseStatusCode", "http://schemas.microsoft.com/BizTalk/2003/http-properties"));
		}

		public Stream GetEmbeddedResourceStream(string name) => Assembly.GetExecutingAssembly().GetManifestResourceStream($"{Assembly.GetExecutingAssembly().GetName().Name}.{name}");
		public byte[] GetEmbeddedResourceBytes(string name)
		{
			using (var res = GetEmbeddedResourceStream(name))
			using (var mem = new MemoryStream((int)res.Length))
			{
				res.CopyTo(mem);
				return mem.ToArray();
			}
		}

		private static IBaseMessage CreateTransmitMessage(Stream data = null, bool isTwoWay = true, string method = "POST")
		{
			var testMessageFactory = new MessageFactory();
			var transmitMessage = testMessageFactory.CreateMessage();
			transmitMessage.Context = testMessageFactory.CreateMessageContext();
			transmitMessage.AddPart("Body", testMessageFactory.CreateMessagePart(), true);
			transmitMessage.BodyPart.Data = data ?? new MemoryStream();
			var configXml = new XElement("Config",
				new XElement("UniqueName", "UNIQUENAME"),
				new XElement("DestinationUrl", "http://destination-url/"),
				new XElement("Timeout", "90000"),
				new XElement("Method", method),
				new XElement("CustomHeaders", "X-CustomHeader: CUSTOM_VALUE"),
				new XElement("Success", "200-299"),
				new XElement("Terminate", "400-499,500-599"),
				new XElement("LogMaxSize", "5"),
				new XElement("LogMaxCount", "10"),
				new XElement("LogLevel", "Debug"),
				new XElement("LogFormat", "Flat")
			);
			transmitMessage.Context.Write("AdapterConfig", HttpExTransmitter.HttpExPropertyNamespace, configXml.ToString());
			transmitMessage.Context.Write("SPName", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "PORT_NAME");
			transmitMessage.Context.Write("IsSolicitResponse", "http://schemas.microsoft.com/BizTalk/2003/system-properties",
				isTwoWay);
			transmitMessage.Context.Write("MessageType", "http://schemas.microsoft.com/BizTalk/2003/system-properties",
				"http://cargowise.com/ehub/biztalkadapters/polling#PollingRequest");
			return transmitMessage;
		}

		private static HttpExTransmitterEndpoint GetHttpExTransmitterEndpoint(IHttpExClientHandlerFactory mockHandlerFactory, IHttpExClientFactory mockClientFactory)
		{
			var loggerFactory = new Moq.Mock<ILoggerFactory>();
			loggerFactory.Setup(x => x.CreateLogger(Moq.It.IsAny<string>(), Moq.It.IsAny<XmlDocument>()))
				.Returns(new ConsoleOutLoggerFactoryAdapter().GetLogger(nameof(TransmitterEndpointTests)));
			loggerFactory.Setup(x => x.CreateLogger(Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<XmlDocument>()))
				.Returns(new ConsoleOutLoggerFactoryAdapter().GetLogger(nameof(TransmitterEndpointTests)));
			var transportProxy = new Moq.Mock<IBTTransportProxy>() { CallBase = true };
			var testMessageFactory = new MessageFactory();
			transportProxy.Setup(x => x.GetMessageFactory()).Returns(testMessageFactory);
			object handlerConfigValue = new XElement("Config", new XElement("InterfacesLogDir", "X:\\InterfacesLogDir"), new XElement("TerminateWaitLimit", "60000")).ToString();
			var handlerProperties = new Moq.Mock<IPropertyBag>();
			handlerProperties.Setup(x => x.Read("AdapterConfig", out handlerConfigValue, Moq.It.IsAny<int>()));
			var transmitHandler = new Moq.Mock<Transmitter>("HTTPEx Transmitter", "1.0", "HTTPEx Adapter", "HTTPEx", Guid.Empty, HttpExTransmitter.HttpExPropertyNamespace, null, 1, loggerFactory.Object) { CallBase = true };
			transmitHandler.Object.Initialize(transportProxy.Object);
			transmitHandler.Object.Load(handlerProperties.Object, 0);
			var endpoint = new Moq.Mock<HttpExTransmitterEndpoint>(transmitHandler.Object, mockClientFactory, mockHandlerFactory, loggerFactory.Object) { CallBase = true };
			var transmitterEndpointParameters = new TransmitterEndpointParameters("winscp://port-uri", "PORT_NAME");
			endpoint.Object.Open(transmitterEndpointParameters, null, HttpExTransmitter.HttpExPropertyNamespace);
			return endpoint.Object;
		}
	}
}
