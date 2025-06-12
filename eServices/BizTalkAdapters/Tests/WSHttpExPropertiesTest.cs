using System;
using System.IO;
using System.Xml.Linq;
using CargoWise.eHub.BizTalkAdapters.WSHttpEx;
using Common.Logging.Simple;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.BizTalkAdapters.Tests
{
	[TestClass()]
	public class WSHttpExPropertiesTest : TestBase
	{
		public const string PropertyNamespace = "PROPERTYNAMESPACE";

		[TestMethod()]
		public void WSHttpExProperties_Defaults()
		{
			var target = CreateProperties();

			Assert.IsNull(target.Uri);
			Assert.AreEqual(120000, target.Timeout);
			Assert.AreEqual("text/xml", target.ContentType);
			Assert.AreEqual(false, target.IsTwoWay);
			Assert.AreEqual("MyPortName", target.PortName);
			Assert.IsNull(target.Certificate);
			Assert.IsNull(target.CertificatePassPhrase);
			Assert.IsNull(target.SoapAction);
			Assert.AreEqual(false, target.IgnoreServerCertErrors);
			Assert.AreEqual(InboundBodyLocations.BodyContents, target.InboundBodyLocation);
			Assert.AreEqual("/*", target.InboundBodyPathExpression);
			Assert.IsTrue(target.LoggingEnabled);
			Assert.AreEqual(@"C:\Logs\BizTalk\Interfaces\{prefix,3}", target.LogFolder);
			Assert.AreEqual(2, target.LogMaxSize);
			Assert.AreEqual(10, target.LogMaxCount);
			Assert.IsInstanceOfType(target.Logger, typeof(NoOpLogger));
		}

		[TestMethod()]
		public void WSHttpExProperties_NotDefault()
		{
			var configElements = new[] { new XElement("uri", "URI"),
										new XElement("timeout", "30000"),
										new XElement("contentType", "myContentType"),
										new XElement("soapAction", "DoSomething"),
										new XElement("ignoreServerCertErrors", "true"),
										new XElement("inboundBodyLocation", "2"),
										new XElement("inboundBodyPathExpression", "/MyRoot/*"),
										new XElement("Logging", "False"),
										new XElement("LogDir", "LOGDIR"),
										new XElement("LogMaxSize", "5"),
										new XElement("LogMaxCount", "10"),
										new XElement("responseMessageEncoding", "Mtom")};
			byte[] certificateBytes = new byte[] { 0x43, 0x07, 0xF3, 0x22 };
			var target = CreateProperties(configElements, true, certificateBytes, "MyPassWord", "Transport", "ABC", "PPP");

			Assert.AreEqual("URI", target.Uri);
			Assert.AreEqual(30000, target.Timeout);
			Assert.AreEqual("myContentType", target.ContentType);
			Assert.AreEqual(true, target.IsTwoWay);
			Assert.AreEqual("MyPortName", target.PortName);
			Assert.AreEqual("DoSomething", target.SoapAction);
			Assert.AreEqual("MyPassWord", target.CertificatePassPhrase);
			Assert.AreEqual(certificateBytes.Length, target.Certificate.Length);
			for (int i = 0; i < certificateBytes.Length; ++i)
			{
				Assert.AreEqual(certificateBytes[i], target.Certificate[i]);
			}
			Assert.AreEqual(true, target.IgnoreServerCertErrors);
			Assert.AreEqual(InboundBodyLocations.BodyPath, target.InboundBodyLocation);
			Assert.AreEqual("/MyRoot/*", target.InboundBodyPathExpression);
			Assert.IsFalse(target.LoggingEnabled);
			Assert.AreEqual("LOGDIR", target.LogFolder);
			Assert.AreEqual(5, target.LogMaxSize);
			Assert.AreEqual(10, target.LogMaxCount);
			Assert.IsInstanceOfType(target.Logger, typeof(NoOpLogger));
			Assert.AreEqual("Transport", target.SecurityMode);
			Assert.AreEqual("ABC", target.UserName);
			Assert.AreEqual("PPP", target.Password);
			Assert.AreEqual("Mtom", target.ResponseMessageEncoding);
		}

		public static WSHttpExProperties CreateProperties(IBaseMessage message)
		{
			return new WSHttpExProperties(message, PropertyNamespace);
		}

		public static WSHttpExProperties CreateProperties(object[] adapterConfig = null, bool isSolicitResponse = false, byte[] certificate = null, string certificatePassPhrase = null, string securityMode = null, string userName = null, string password = null)
		{
			return CreateProperties(CreateMessage(adapterConfig, isSolicitResponse, certificate, certificatePassPhrase, securityMode, userName, password));
		}

		public static IBaseMessage CreateMessage(object[] adapterConfig = null, bool isSolicitResponse = false, byte[] certificate = null, string certificatePassPhrase = null, string securityMode = null, string userName = null, string password = null)
		{
			var dataStream = new MemoryStream();
			var bodyXML = new XDocument();
			bodyXML.Add(new XElement("RequestElement"));
			bodyXML.Save(dataStream);
			dataStream.Position = 0;
			var stubMessagePart = MockRepository.GenerateStub<IBaseMessagePart>();
			stubMessagePart.Stub(x => x.GetOriginalDataStream()).Return(dataStream);
			stubMessagePart.Charset = "MyCharSet";

			var configXml = new XElement("Config", adapterConfig);
			var context = MockRepository.GenerateStub<IBaseMessageContext>();
			context.Expect(x => x.Read("AdapterConfig", PropertyNamespace)).Return(configXml.ToString());
			context.Expect(x => x.Read("IsSolicitResponse", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(isSolicitResponse);
			context.Expect(x => x.Read("SPName", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("MyPortName");
			context.Expect(x => x.Read("CertificatePassPhrase", PropertyNamespace)).Return(certificatePassPhrase);
			context.Expect(x => x.Read("Certificate", PropertyNamespace)).Return((certificate != null) ? Convert.ToBase64String(certificate) : null);
			context.Expect(x => x.Read("SecurityMode", "http://schemas.microsoft.com/BizTalk/2006/01/Adapters/WCF-properties")).Return(securityMode ?? null);
			context.Expect(x => x.Read("UserName", "http://schemas.microsoft.com/BizTalk/2006/01/Adapters/WCF-properties")).Return(userName ?? null);
			context.Expect(x => x.Read("Password", "http://schemas.microsoft.com/BizTalk/2006/01/Adapters/WCF-properties")).Return(password ?? null);
			var message = MockRepository.GenerateStub<IBaseMessage>();
			message.Context = context;
			message.Expect(x => x.BodyPart).Return(stubMessagePart);

			return message;
		}
	}
}
