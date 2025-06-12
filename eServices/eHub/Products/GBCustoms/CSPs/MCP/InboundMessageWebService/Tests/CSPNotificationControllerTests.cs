using System.Reflection;
using System.Xml.Linq;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Products.GBCustoms.MCP.InboundMessageWebService.Controllers;
using CargoWise.eHub.Products.GBCustoms.Core.InboundMessageWebService.Handlers;
using Common.Logging;
using NUnit.Framework;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.GBCustoms.MCP.InboundMessageWebService.Tests
{
	[TestFixture]
	public class CSPNotificationControllerTests
	{
		[TestCase("MCP_TransportSuccessResponse.xml")]
		[TestCase("X-MCP-ID_TransportSuccessResponse.xml")]
		public void TestTransportResponse(string inputTestXmlFile)
		{
			XDocument body;
			var controller = new CSPNotificationControllerForTest();
			var handler = controller.ExposeHandler(XDocument.Parse(GetMessage(inputTestXmlFile)), out body);
			Assert.IsNotNull(handler);
			Assert.AreEqual(typeof(TransportAcknowledgementHandler).Name, handler.GetType().Name);
			Assert.AreEqual(GetMessage("MCP_TransportSuccessResponseBody.xml"), body.ToString());
		}

		[TestCase("MCP_TransportErrorResponseServiceUnavailable.xml")]
		public void TestTransportErrorResponse(string inputTestXmlFile)
		{
			XDocument body;
			var controller = new CSPNotificationControllerForTest();
			var handler = controller.ExposeHandler(XDocument.Parse(GetMessage(inputTestXmlFile)), out body);
			Assert.IsNotNull(handler);
			Assert.AreEqual(typeof(TransportAcknowledgementHandler).Name, handler.GetType().Name);
			Assert.AreEqual(GetMessage("MCP_TransportErrorResponseBodyServiceUnavailable.xml"), body.ToString());
		}

		[TestCase("MCP_BusinessResponse.xml")]
		[TestCase("DMS_NotificationType_BusinessResponse.xml")]
		public void TestBusinessNotification(string inputTestXmlFile)
		{
			XDocument body;
			var controller = new CSPNotificationControllerForTest();
			var handler = controller.ExposeHandler(XDocument.Parse(GetMessage(inputTestXmlFile)), out body);
			Assert.IsNotNull(handler);
			Assert.AreEqual(typeof(BusinessNotificationHandler), handler.GetType());
			Assert.AreEqual(GetMessage("MCP_BusinessResponseBody.xml"), body.ToString());
		}

		[Test]
		public void TestCSPNotification()
		{
			XDocument body;
			var controller = new CSPNotificationControllerForTest();
			var handler = controller.ExposeHandler(XDocument.Parse(GetMessage("CSP_Response.xml")), out body);
			Assert.IsNotNull(handler);
			Assert.AreEqual(typeof(CSPNotificationHandler), handler.GetType());
			Assert.AreEqual(GetMessage("CSP_SuccessResponseBody.xml"), body.ToString());
		}

		[Test]
		public void TestCSPNotification_MessageTypeDMSNoCSPID_BusinessNotificationHandler()
		{
			XDocument body;
			var controller = new CSPNotificationControllerForTest();
			var handler = controller.ExposeHandler(XDocument.Parse(GetMessage("CSP_Response_noCSPID.xml")), out body);
			Assert.IsNotNull(handler);
			Assert.AreEqual(typeof(BusinessNotificationHandler), handler.GetType());
		}

		[TestCase("MCP_BusinessResponse_Windows1252.xml")]
		public void TestBusinessNotification_Windows1252Encoding(string inputTestXmlFile)
		{
			XDocument body;
			var controller = new CSPNotificationControllerForTest();
			var handler = controller.ExposeHandler(XDocument.Parse(GetMessage(inputTestXmlFile)), out body);
			Assert.IsNotNull(handler);
			Assert.AreEqual(typeof(BusinessNotificationHandler), handler.GetType());
			Assert.AreEqual(GetMessage("MCP_BusinessResponseBody_Windows1252.xml"), body.ToString());
		}

		string GetMessage(string source)
		{
			return Assembly.GetExecutingAssembly().GetManifestResourceStream($"CargoWise.eHub.Products.GBCustoms.MCP.InboundMessageWebService.Tests.TestFiles.{source}").ReadToEnd();
		}
	}

	#region Implementation
	public class CSPNotificationControllerForTest : MCPNotificationController
	{
		public CSPNotificationControllerForTest()
		{
			InternalLog = () => MockRepository.GenerateMock<ILog>();
		}

		public IMessageHandler ExposeHandler(XDocument responseXml, out XDocument body)
		{
			return GetHandler(responseXml, out body);
		}
	}
	#endregion
}
