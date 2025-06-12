using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.ZACustoms.Orchestrations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ZACustoms.Tests
{
	[TestClass]
	public class SupportingDocumentUploadingResponse2Universal_Tests
	{
		const string filePath = "Universal2SupportingDocumentUploadingRequest.TestFiles.";
	   
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversal2SupportingDocumentUploading()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			var input = filePath + "Test1_UniversalEvent_input.xml";
			var expectedOutput = filePath + "Test1_ZACustomsSoapRequest_output.xml";

			mapTester.Execute<Universal2ZACSoapRequest>(input, expectedOutput);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversal2UniversalDoc()
		{
			var mockContextAccessor = MockRepository.GenerateMock<ContextAccessor>();
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("ZACustoms");
			mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "ZACustomsDoc"));
			var extensionObjects = new Dictionary<string, object>() { 
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockContextAccessor },
			};
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

			string sourceFile, expectedFile;
			sourceFile = "Universal2SupportingDocumentUploadingRequest.TestFiles.Test2_UniversalInterchange_Input.xml";
			expectedFile = "Universal2SupportingDocumentUploadingRequest.TestFiles.Test2_UniversalInterchange_Output.xml";

			mapTester.Execute<Universal2UniversalDoc>(sourceFile, expectedFile);

			mockContextAccessor.VerifyAllExpectations();

		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestSoapResponse2UniversalSuccessful()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			var input = filePath + "Test3_SoapResponse_Successful_input.xml";
			var expectedOutput = filePath + "Test3_UniversalEvent_Successful_output.xml";

			mapTester.Execute<SoapResponse2Universal>(input, expectedOutput);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestSoapResponse2UniversalWithError()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			var input = filePath + "Test4_SoapResponse_Error_input.xml";
			var expectedOutput = filePath + "Test4_UniversalEvent_Error_output.xml";

			mapTester.Execute<SoapResponse2Universal>(input, expectedOutput);
		}
	}
}
