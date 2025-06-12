using System;
using System.IO;
using System.ServiceModel.Channels;
using CargoWise.eHub.Share.eHubServices.eHubReceiver;
using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Share.eHubServices.Tests.eHubReceiver
{
	[TestClass]
	public class CiscoReceiverTests : BaseTest
	{
		[TestMethod]
		public void CiscoReceiveNullInput()
		{
			var receiver = new CiscoReceiverTest();
			string result = receiver.Receive3B14("Identifier", "Type", 2, null, "Sender", "Receiver");
			Assert.AreEqual("<responseforsoap xmlns=\"http://www.tibco.com/soapresponse.xsd\"><status>ERROR</status><timestamp>2011-12-02T09:12:22-07:00</timestamp><errorcode>503</errorcode><errordescription>Input string is null</errordescription></responseforsoap>", result);

		}

		[TestMethod]
		public void CiscoReceiveEmptyStringInput()
		{
			var receiver = new CiscoReceiverTest();
			string result = receiver.Receive3B14("Identifier", "Type", 2, "", "Sender", "Receiver");
			Assert.AreEqual("<responseforsoap xmlns=\"http://www.tibco.com/soapresponse.xsd\"><status>ERROR</status><timestamp>2011-12-02T09:12:22-07:00</timestamp><errorcode>503</errorcode><errordescription>Input string is empty</errordescription></responseforsoap>", result);
		}

        [TestMethod]
		public void CiscoReceiveTextInput()
		{
			var receiver = new CiscoReceiverTest();
			string result = receiver.Receive3B14("Identifier", "Type", 2, "Bla Bla", "Sender", "Receiver");
			Assert.AreEqual("<responseforsoap xmlns=\"http://www.tibco.com/soapresponse.xsd\"><status>ERROR</status><timestamp>2011-12-02T09:12:22-07:00</timestamp><errorcode>503</errorcode><errordescription>Not valid XML</errordescription></responseforsoap>", result);
		}

        [TestMethod]
		public void CiscoReceiveXmlNotComplyToSchema()
		{
			var receiver = new CiscoReceiverTest();
			string result = receiver.Receive3B14("Identifier", "Type", 2, "<responseforsoap xmlns=\"http://www.tibco.com/soapresponse.xsd\"><status>ERROR</status><timestamp>2011-12-02T09:12:22-07:00</timestamp><errorcode>503</errorcode><errordescription>Not valid XML</errordescription></responseforsoap>", "Sender", "Receiver");
			Assert.AreEqual("<responseforsoap xmlns=\"http://www.tibco.com/soapresponse.xsd\"><status>ERROR</status><timestamp>2011-12-02T09:12:22-07:00</timestamp><errorcode>502</errorcode><errordescription>Could not find schema information for the element 'http://www.tibco.com/soapresponse.xsd:responseforsoap'.\r\nCould not find schema information for the element 'http://www.tibco.com/soapresponse.xsd:status'.\r\nCould not find schema information for the element 'http://www.tibco.com/soapresponse.xsd:timestamp'.\r\nCould not find schema information for the element 'http://www.tibco.com/soapresponse.xsd:errorcode'.\r\nCould not find schema information for the element 'http://www.tibco.com/soapresponse.xsd:errordescription'.\r\n</errordescription></responseforsoap>", result);
		}

        [TestMethod]
		public void CiscoReceiveValidXmlUnableToSendToEHub()
		{
			var receiver = new CiscoReceiverTest();
			receiver.SendToBiztalkThrownException = true;
			string messageString = new StreamReader(GetEmbeddedResource("eHubReceiver.Cisco.TestFiles.ShippingDocumentationNotification.xml")).ReadToEnd();
			string result = receiver.Receive3B14("Identifier", "Type", 2, messageString, "Sender", "Receiver");
			
			Assert.AreEqual("<responseforsoap xmlns=\"http://www.tibco.com/soapresponse.xsd\"><status>ERROR</status><timestamp>2011-12-02T09:12:22-07:00</timestamp><errorcode>503</errorcode><errordescription>Unable to send message to eHub. Details: Error during sending to eHub</errordescription></responseforsoap>", result);

		}

        [TestMethod]
		public void CiscoReceiveValidXmlSent()
		{
			var receiver = new CiscoReceiverTest();
			receiver.SendToBiztalkThrownException = false;
			string messageString = new StreamReader(GetEmbeddedResource("eHubReceiver.Cisco.TestFiles.ShippingDocumentationNotification.xml")).ReadToEnd();
			string result = receiver.Receive3B14("Identifier", "Type", 2, messageString, "Sender", "Receiver");
			Assert.AreEqual("<responseforsoap xmlns=\"http://www.tibco.com/soapresponse.xsd\"><status>SUCCESS</status><timestamp>2011-12-02T09:12:22-07:00</timestamp><errorcode></errorcode><errordescription></errordescription></responseforsoap>", result);
		}

        [TestMethod]
		public void CiscoReceiveInvalidCompressedXml()
		{
			var receiver = new CiscoReceiverCompressedTest();
			receiver.SendToBiztalkThrownException = false;
			string messageString = new StreamReader(GetEmbeddedResource("eHubReceiver.Cisco.TestFiles.InvalidCompressedPayload.txt")).ReadToEnd();
			string result = receiver.Receive3B14("Identifier", "Type", 2, messageString, "Sender", "Receiver");
			Assert.AreEqual("<responseforsoap xmlns=\"http://www.tibco.com/soapresponse.xsd\"><status>ERROR</status><timestamp>2011-12-02T09:12:22-07:00</timestamp><errorcode>502</errorcode><errordescription>The 'urn:rosettanet:specification:universal:CountrySubdivision:xsd:codelist:01.02:CountrySubdivision' element is invalid - The value ' ' is invalid according to its datatype 'urn:rosettanet:specification:universal:CountrySubdivision:xsd:codelist:01.02:CountrySubdivisionType' - line-feed (#xA) or tab (#x9) characters, leading or trailing spaces and sequences of one or more spaces (#x20) are not allowed in 'xs:token'.\r\nThe 'urn:rosettanet:specification:universal:CountrySubdivision:xsd:codelist:01.02:CountrySubdivision' element is invalid - The value ' ' is invalid according to its datatype 'urn:rosettanet:specification:universal:CountrySubdivision:xsd:codelist:01.02:CountrySubdivisionType' - line-feed (#xA) or tab (#x9) characters, leading or trailing spaces and sequences of one or more spaces (#x20) are not allowed in 'xs:token'.\r\n</errordescription></responseforsoap>", result);
		}

        [TestMethod]
		public void CiscoReceiveValidXmlInvalidUrl()
		{
			var receiver = new CiscoReceiverTest();
			receiver.InvalidUri = true;
			receiver.SendToBiztalkThrownException = false;
			string messageString = new StreamReader(GetEmbeddedResource("eHubReceiver.Cisco.TestFiles.ShippingDocumentationNotification.xml")).ReadToEnd();
			string result = receiver.Receive3B14("Identifier", "Type", 2, messageString, "Sender", "Receiver");
			Assert.AreEqual("<responseforsoap xmlns=\"http://www.tibco.com/soapresponse.xsd\"><status>ERROR</status><timestamp>2011-12-02T09:12:22-07:00</timestamp><errorcode>503</errorcode><errordescription>Can't find recipient or sender for the endpoint url</errordescription></responseforsoap>", result);
		}
	}

	public class CiscoReceiverTest : CiscoReceiver
	{
		public CiscoReceiverTest()
			: base(MockRepository.GenerateMock<ILog>())
		{

		}

		public bool SendToBiztalkThrownException { get; set; }
		public Message Message { get; set; }
		public bool InvalidUri { get; set; }


		protected override string DateTimeNowString
		{
			get
			{
				return "2011-12-02T09:12:22-07:00";
			}
		}

		protected override void SendToBiztalk(Message message)
		{
			if (SendToBiztalkThrownException) throw new Exception("Error during sending to eHub");
		}

		protected override Uri EndpointAddress
		{
			get
			{
				if (InvalidUri)
				{
					return new Uri("http://syd-wikb-2.corporate.cargowise.com/Bla/");
				}
				else
				{
					return new Uri("http://syd-wikb-2.corporate.cargowise.com/CiscoReceiverTest/CiscoReceiver.svc");
				}

			}
		}
	}

	public class CiscoReceiverCompressedTest : CiscoReceiverTest
	{
		protected override bool IsPayloadCompressed
		{
			get
			{
				return true;
			}
		}
	}
}
