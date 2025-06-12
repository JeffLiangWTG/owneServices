using System.Reflection;
using System.Xml.Linq;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Products.GBCustoms.CNS.InboundMessageWebService.Controllers;
using CargoWise.eHub.Products.GBCustoms.Core.InboundMessageWebService.Handlers;
using Common.Logging;
using NUnit.Framework;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.GBCustoms.CNS.InboundMessageWebService.Tests
{
	[TestFixture]
	public class CSPNotificationControllerTests
	{
		[Test]
		public void TestTransportResponse()
		{
			XDocument body;
			var controller = new CSPNotificationControllerForTest();
			var handler = controller.ExposeHandler(XDocument.Parse(GetMessage("CNS_TransportSuccessResponse.xml")), out body);
			Assert.IsNotNull(handler);
			Assert.AreEqual(typeof(TransportAcknowledgementHandler).Name, handler.GetType().Name);
			Assert.AreEqual(GetMessage("CNS_TransportSuccessResponseBody.xml"), body.ToString());
		}

		[Test]
		public void TestTransportResponseEmptyBody()
		{
			XDocument body;
			var controller = new CSPNotificationControllerForTest();
			var handler = controller.ExposeHandler(XDocument.Parse(GetMessage("CNS_TransportSuccessResponseWithEmptyBody.xml")), out body);
			Assert.IsNotNull(handler);
			Assert.AreEqual(typeof(TransportAcknowledgementHandler).Name, handler.GetType().Name);
			Assert.AreEqual(GetMessage("CNS_TransportSuccessResponseEmptyBody.xml"), body.ToString());
		}

		[Test]
		public void TestTransportErrorResponse()
		{
			XDocument body;
			var controller = new CSPNotificationControllerForTest();
			var handler = controller.ExposeHandler(XDocument.Parse(GetMessage("CNS_TransportErrorResponseServiceUnavailable.xml")), out body);
			Assert.IsNotNull(handler);
			Assert.AreEqual(typeof(TransportAcknowledgementHandler).Name, handler.GetType().Name);
			Assert.AreEqual(GetMessage("CNS_TransportErrorResponseBodyServiceUnavailable.xml"), body.ToString());
		}

		[Test]
		public void TestBusinessNotification()
		{
			XDocument body;
			var controller = new CSPNotificationControllerForTest();
			var handler = controller.ExposeHandler(XDocument.Parse(GetMessage("CNS_BusinessResponse.xml")), out body);
			Assert.IsNotNull(handler);
			Assert.AreEqual(typeof(BusinessNotificationHandler), handler.GetType());
			Assert.AreEqual(GetMessage("CNS_BusinessResponseBody.xml"), body.ToString());
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

		[Test]
		public void TestBusinessNotification_InventoryLinkingByNotificationType()
		{
			XDocument body;
			var controller = new CSPNotificationControllerForTest();
			var handler = controller.ExposeHandler(XDocument.Parse(GetMessage("CNS_BusinessResponseInventoryLinking.xml")), out body);
			Assert.IsNotNull(handler);
			Assert.AreEqual(typeof(BusinessNotificationHandler), handler.GetType());
			Assert.AreEqual(GetMessage("CNS_BusinessResponseInventoryLinkingBody.xml"), body.ToString());
		}

		[Test]
		public void TestBusinessNotification_InventoryLinkingByBody()
		{
			XDocument body;
			var controller = new CSPNotificationControllerForTest();
			var handler = controller.ExposeHandler(XDocument.Parse(GetMessage("CNS_BusinessResponseInventoryLinkingNoType.xml")), out body);
			Assert.IsNotNull(handler);
			Assert.AreEqual(typeof(BusinessNotificationHandler), handler.GetType());
			Assert.AreEqual(GetMessage("CNS_BusinessResponseInventoryLinkingBody.xml"), body.ToString());
		}

		string GetMessage(string source)
		{
			return Assembly.GetExecutingAssembly().GetManifestResourceStream($"CargoWise.eHub.Products.GBCustoms.CNS.InboundMessageWebService.Tests.TestFiles.{source}").ReadToEnd();
		}
	}

	#region Implementation
	public class CSPNotificationControllerForTest : CNSNotificationController
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
