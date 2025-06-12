using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.ForwardingPortMessaging.FR.APPLUS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.FR.Tests
{
	[TestClass]
	public class UInterchange2UShipment_Tests
	{
		const string filePath = "APPLUS.UInterchange2UShipment.TestFiles.";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUS2UI()
		{
			AssertMapping("Test1_input_DOS_UInterchange.xml", "Test1_output_DOS_UShipment.xml");
			AssertMapping("Test2_input_AMQ_UInterchange.xml", "Test2_output_AMQ_UShipment.xml");
			AssertMapping("Test3_input_CRESA_UInterchange.xml", "Test3_output_CRESA_UShipment.xml");
			AssertMapping("Test4_input_withInvalidText.xml", "Test4_output_withInvalidText.xml");
		}

		void AssertMapping(string inputFile, string expectedOutputFile)
		{
			var input = filePath + inputFile;
			var expectedOutput = filePath + expectedOutputFile;

			var mockContextAccessor = MockRepository.GenerateMock<ContextAccessor>();
			var mockCodeMapper = MockRepository.GenerateMock<CodeMapper>();
			mockContextAccessor.Stub(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("SENDER");
			mockContextAccessor.Stub(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("RECIPIENT");
			mockCodeMapper.Stub(x => x.CallActionProcedureHelper("GetEdiProdLicenceType", "@LicenceType", "@ClientID", "SENDER")).Return("PRD");
			var extensionObjects = new Dictionary<string, object>() {
				{ "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper }
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<UInterchange2UShipment>(input, expectedOutput);

			mockContextAccessor.AssertWasNotCalled(x => x.SetContextProperty(Arg.Is("DestinationParty"), Arg.Is("http://schemas.microsoft.com/BizTalk/2003/system-properties"), Arg<string>.Is.Anything));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUS2UI_Licence()
		{
			AssertLicence("MGI_DOS1", "PRD", "MGI_DOS1");
			AssertLicence("MGI_DOS1", "TST", "MGI_DOS1_TST");
			AssertLicence("MGI_DOS1", "TRN", "MGI_DOS1_TST");
			AssertLicence("SOGET_TRC2", "PRD", "SOGET_TRC2");
			AssertLicence("SOGET_TRC2", "TRN", "SOGET_TRC2_TST");
		}

		public void AssertLicence(string recipient, string licenceType, string expectedRecipient)
		{
			var input = filePath + "Test1_input_DOS_UInterchange.xml";
			var expectedOutput = filePath + "Test1_output_DOS_UShipment.xml";

			var mockContextAccessor = MockRepository.GenerateMock<ContextAccessor>();
			var mockCodeMapper = MockRepository.GenerateMock<CodeMapper>();
			mockContextAccessor.Stub(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("SENDER");
			mockContextAccessor.Stub(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(recipient);
			mockCodeMapper.Stub(x => x.CallActionProcedureHelper("GetEdiProdLicenceType", "@LicenceType", "@ClientID", "SENDER")).Return(licenceType);
			var extensionObjects = new Dictionary<string, object>() {
				{ "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper }
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<UInterchange2UShipment>(input, expectedOutput);

			if (recipient == expectedRecipient)
			{
				mockContextAccessor.AssertWasNotCalled(x => x.SetContextProperty(Arg.Is("DestinationParty"), Arg.Is("http://schemas.microsoft.com/BizTalk/2003/system-properties"), Arg<string>.Is.Anything));
			}
			else
			{
				mockContextAccessor.AssertWasCalled(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", expectedRecipient), x => x.Repeat.Once());
			}
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUS2UI_NoDataSourceKey()
		{
			var isThrownException = false;
			try
			{
				AssertMappingException("Test5_input_NoDataSourceKey.xml");
			}
			catch (ArgumentException ex)
			{
				isThrownException = true;
				Assert.AreEqual("Could not find data source key, message rejected.", ex.Message.Trim());
			}
			Assert.IsTrue(isThrownException);
		}

		void AssertMappingException(string inputFile)
		{
			var input = filePath + inputFile;

			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

			var extensionObjects = new Dictionary<string, object>() {
				{ "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper }
			};

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("SENDER");
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("RECIPIENT");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetEdiProdLicenceType", "@LicenceType", "@ClientID", "SENDER")).Return("PRD");

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<UInterchange2UShipment>(input, input);
		}
	}
}
