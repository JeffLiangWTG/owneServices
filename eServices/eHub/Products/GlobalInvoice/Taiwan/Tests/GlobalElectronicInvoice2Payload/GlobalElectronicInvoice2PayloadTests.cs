using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.GlobalInvoice.Taiwan.Transforms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.GlobalInvoice.Taiwan.Tests
{
	[TestClass]
	public class GlobalElectronicInvoice2PayloadTests
	{
		private const string filePath = "GlobalElectronicInvoice2Payload.TestFiles.";

		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestGlobalElectronicInvoice2Payload()
		{
			AssertMapping("Test1_input.xml", "Test1_output.xml", "12656354-InvoiceMD-20160725-155833.txt");
		}

		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestGlobalElectronicInvoice2Payload_FileNameMissing()
		{
			try
			{
				AssertMapping("Test2_input.xml", "Test1_output.xml", "");
				Assert.Fail("Expected exception- FileName is mandatory but is missing.");
			}
			catch (Exception ex)
			{
				var expectedMessage = "GlobalElectronicInvoicing/Header/ElectronicInvoiceBatchRequest/FileName is mandatory but is missing.";
				var actualException = ex.InnerException ?? ex;
				Assert.AreEqual(expectedMessage, actualException.Message);
			}
		}

		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestGlobalElectronicInvoice2Payload_BatchNumberMissing()
		{
			try
			{
				AssertMapping("Test3_input.xml", "Test1_output.xml", "12656354-InvoiceMD-20160725-155833.txt");
				Assert.Fail("Expected exception- BatchNumber is mandatory but is missing.");
			}
			catch (Exception ex)
			{
				var expectedMessage = "GlobalElectronicInvoicing/Header/ElectronicInvoiceBatchRequest/BatchNumber is mandatory but is missing.";
				var actualException = ex.InnerException ?? ex;
				Assert.AreEqual(expectedMessage, actualException.Message);
			}
		}

		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestGlobalElectronicInvoice2Payload_PayloadMissing()
		{
			try
			{
				AssertMapping("Test4_input.xml", "Test1_output.xml", "12656354-InvoiceMD-20160725-155833.txt");
				Assert.Fail("Expected exception- Payload is mandatory but is missing.");
			}
			catch (Exception ex)
			{
				var expectedMessage = "GlobalElectronicInvoicing/Payload is mandatory but is missing.";
				var actualException = ex.InnerException ?? ex;
				Assert.AreEqual(expectedMessage, actualException.Message);
			}
		}

		private void AssertMapping(string inputFile, string expectedOutputFile, string fileName)
		{
			var input = filePath + inputFile;
			var expectedOutput = filePath + expectedOutputFile;

			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

			mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "12656354-InvoiceMD-20160725-155833.txt"));
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("GEI_TAIWANTest");
			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("HYEDCNUAT");
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GEIMSG", "GEI_TAIWANTest", "HYEDCNUAT", "12656354-InvoiceMD-20160725-155833", "134"));

			var extensionObjects = new Dictionary<string, object>() 
			{ 
				{ "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor }
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<GlobalElectronicInvoice2Payload>(input, expectedOutput);

			mockContextAccessor.VerifyAllExpectations();
		}
	}
}
