using System.IO;
using System.Xml.Schema;
using Microsoft.XLANGs.BaseTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.eHub.Products.SGCustoms.MHAccess.Orchestrations.Helpers;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Tests
{
	[TestClass]
	public class SchemaValidatorTests : TestBase
	{
		private const string filePath = "SendOrchestrationsHelper.TestFiles.SchemaValidator.";

		[TestMethod]
		public void TestValidateSchema_Exception_AIRAED()
		{
			var bodyStream = GetEmbeddedResource(filePath + "SendMessage_AIRAED.xml");
			mockPart.Expect(x => x.RetrieveAs(typeof(Stream))).Return(bodyStream);
			var execeptionMessage = "Message validation failed\r\n'SER2' - 'NTT'";
			AssertException(() => SchemaValidator.Validate(mockMessage, "EFACT_31_AIRAED"), typeof(XmlSchemaValidationException), execeptionMessage);
		}

		[TestMethod]
		public void TestValidateSchema_Success_AIRAED_EmptyHSCode()
		{
			var bodyStream = GetEmbeddedResource(filePath + "SendMessage_AIRAED_EmptyHSCode.xml");
			mockPart.Expect(x => x.RetrieveAs(typeof(Stream))).Return(bodyStream);
			Assert.AreEqual(true, SchemaValidator.Validate(mockMessage, "EFACT_31_AIRAED"));
		}

		[TestMethod]
		public void TestValidateSchema_Exception_AIRAEU()
		{
			var bodyStream = GetEmbeddedResource(filePath + "SendMessage_AIRAEU.xml");
			mockPart.Expect(x => x.RetrieveAs(typeof(Stream))).Return(bodyStream);
			var execeptionMessage = "Message validation failed\r\n'CST1' - '3213344'";
			AssertException(() => SchemaValidator.Validate(mockMessage, "EFACT_31_AIRAEU"), typeof(XmlSchemaValidationException), execeptionMessage);
		}

		[TestMethod]
		public void TestValidateSchema_Exception_AIRPCM()
		{
			var bodyStream = GetEmbeddedResource(filePath + "SendMessage_AIRPCM.xml");
			mockPart.Expect(x => x.RetrieveAs(typeof(Stream))).Return(bodyStream);
			var execeptionMessage = "Message validation failed\r\n'MEA2' - '1234567891234567'";
			AssertException(() => SchemaValidator.Validate(mockMessage, "EFACT_31_AIRPCM"), typeof(XmlSchemaValidationException), execeptionMessage);
		}

		[TestMethod]
		public void TestValidateSchema_Exception_AIRPCU()
		{
			var bodyStream = GetEmbeddedResource(filePath + "SendMessage_AIRPCU.xml");
			mockPart.Expect(x => x.RetrieveAs(typeof(Stream))).Return(bodyStream);
			var execeptionMessage = "Message validation failed\r\n'CST1' - '4561522'";
			AssertException(() => SchemaValidator.Validate(mockMessage, "EFACT_31_AIRPCU"), typeof(XmlSchemaValidationException), execeptionMessage);
		}

		[TestInitialize]
		public void Initialize()
		{
			mockMessage = MockRepository.GenerateMock<XLANGMessage>();
			mockPart = MockRepository.GenerateMock<XLANGPart>();
			mockMessage.Expect(x => x[0]).Return(mockPart);
		}

		private XLANGMessage mockMessage;
		private XLANGPart mockPart;
	}
}
