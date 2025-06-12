using CargoWise.eHub.BizTalkAdapters.WSHttpEx;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Net.Security;
using System.Xml.Linq;

namespace CargoWise.eHub.BizTalkAdapters.Tests
{
	[TestClass]
	public class WsHttpExRequestFactoryTest : TestBase
	{
		[TestMethod]
		public void CreateRequestDefaultProperties()
		{
			var message = WSHttpExPropertiesTest.CreateMessage(new Object[] { new XElement("uri", "http://SomeUri") });
			var factory = new WSHttpExRequestFactory();

			var request = (HttpWebRequest)factory.CreateRequest(message, WSHttpExPropertiesTest.CreateProperties(message));
			verifyConstantProperties(request);
			Assert.AreEqual("text/xml; charset=MyCharSet", request.ContentType);
			Assert.AreEqual(0, request.ClientCertificates.Count);
			Assert.AreEqual(1, request.Headers.Count);
			Assert.IsNotNull(request.ServerCertificateValidationCallback);
			Assert.IsTrue(request.ServerCertificateValidationCallback(null, null, null, SslPolicyErrors.None));
			Assert.IsFalse(request.ServerCertificateValidationCallback(null, null, null, SslPolicyErrors.RemoteCertificateNameMismatch));
		}

		[TestMethod]
		public void CreateRequestWithSoapAction()
		{
			var configElements = new Object[] {	new XElement("soapAction", "MyAction"),
												new XElement("uri", "http://SomeUri")};
			var message = WSHttpExPropertiesTest.CreateMessage(configElements);
			var factory = new WSHttpExRequestFactory();

			var request = (HttpWebRequest)factory.CreateRequest(message, WSHttpExPropertiesTest.CreateProperties(message));
			verifyConstantProperties(request);
			Assert.AreEqual("text/xml; charset=MyCharSet", request.ContentType);
			Assert.AreEqual(0, request.ClientCertificates.Count);
			Assert.AreEqual(2, request.Headers.Count);
            Assert.AreEqual("SOAPAction", request.Headers.GetKey(1));
            Assert.AreEqual("MyAction", request.Headers.Get(1));
		}

		[TestMethod]
		public void CreateRequestNonTextContentType()
		{
			var configElements = new Object[] {	new XElement("contentType", "Binary"),
												new XElement("uri", "http://SomeUri") };
			var message = WSHttpExPropertiesTest.CreateMessage(configElements);
			var factory = new WSHttpExRequestFactory();

			var request = (HttpWebRequest)factory.CreateRequest(message, WSHttpExPropertiesTest.CreateProperties(message));
			verifyConstantProperties(request);
			Assert.AreEqual("Binary", request.ContentType);
			Assert.AreEqual(0, request.ClientCertificates.Count);
			Assert.AreEqual(1, request.Headers.Count);
		}

		[TestMethod]
		public void CreateRequestIgnoreServerCertErrors()
		{
			var configElements = new Object[] {	new XElement("ignoreServerCertErrors", "true"),
												new XElement("uri", "http://SomeUri") };
			var message = WSHttpExPropertiesTest.CreateMessage(configElements);
			var factory = new WSHttpExRequestFactory();

			var request = (HttpWebRequest)factory.CreateRequest(message, WSHttpExPropertiesTest.CreateProperties(message));
			verifyConstantProperties(request);
			Assert.AreEqual("text/xml; charset=MyCharSet", request.ContentType);
			Assert.AreEqual(0, request.ClientCertificates.Count);
			Assert.AreEqual(1, request.Headers.Count);
			Assert.IsTrue(request.ServerCertificateValidationCallback(null, null, null, SslPolicyErrors.RemoteCertificateNameMismatch));
		}

		[TestMethod]
		public void CreateRequestWithCerificate()
		{
			var message = WSHttpExPropertiesTest.CreateMessage(new Object[] { new XElement("uri", "http://SomeUri") }, false, GetEmbeddedResourceBytes("TestFiles.tempClientcert.pfx"), "3hubRock$");
			var factory = new WSHttpExRequestFactory();

			var request = (HttpWebRequest)factory.CreateRequest(message, WSHttpExPropertiesTest.CreateProperties(message));
			verifyConstantProperties(request);
			Assert.AreEqual("text/xml; charset=MyCharSet", request.ContentType);
			Assert.AreEqual(2, request.ClientCertificates.Count);
			Assert.AreEqual(1, request.Headers.Count);
		}

        [TestMethod]
        public void CreateRequestWithAuthentication()
        {
            var message = WSHttpExPropertiesTest.CreateMessage(new Object[] { new XElement("uri", "http://SomeUri") }, false, GetEmbeddedResourceBytes("TestFiles.tempClientcert.pfx"), "3hubRock$", "Transport", "UserName", "Password");
            var factory = new WSHttpExRequestFactory();

            var request = (HttpWebRequest)factory.CreateRequest(message, WSHttpExPropertiesTest.CreateProperties(message));
            verifyConstantProperties(request);
            Assert.AreEqual("text/xml; charset=MyCharSet", request.ContentType);
            Assert.AreEqual(2, request.ClientCertificates.Count);
            Assert.AreEqual(1, request.Headers.Count);
            Assert.AreEqual("UserName", request.Credentials.GetCredential(new Uri("http://SomeUri"), "Transport").UserName);
            Assert.AreEqual("Password", request.Credentials.GetCredential(new Uri("http://SomeUri"), "Transport").Password);
        }

        [TestMethod]
        public void CreatePreAuthenticateRequest()
        {
            var message = WSHttpExPropertiesTest.CreateMessage(new Object[] { new XElement("uri", "http://SomeUri") }, false, GetEmbeddedResourceBytes("TestFiles.tempClientcert.pfx"), "3hubRock$", "Transport", "UserName", "Password");
            var factory = new WSHttpExRequestFactory();

            var request = (HttpWebRequest)factory.PreAuthenticateRequest(message, WSHttpExPropertiesTest.CreateProperties(message));
            Assert.AreEqual(true, request.PreAuthenticate);
            Assert.AreEqual("Post Test", request.UserAgent);
            Assert.AreEqual("HEAD", request.Method);
            Assert.AreEqual("UserName", request.Credentials.GetCredential(new Uri("http://SomeUri"), "Transport").UserName);
            Assert.AreEqual("Password", request.Credentials.GetCredential(new Uri("http://SomeUri"), "Transport").Password);       
        }

		void verifyConstantProperties(HttpWebRequest request)
		{
			Assert.AreEqual(true, request.AllowAutoRedirect);
			Assert.AreEqual("POST", request.Method);
			Assert.AreEqual(false, request.AllowWriteStreamBuffering);
			Assert.AreEqual(true, request.SendChunked);
			Assert.AreEqual(true, request.PreAuthenticate);
		}
	}
}
