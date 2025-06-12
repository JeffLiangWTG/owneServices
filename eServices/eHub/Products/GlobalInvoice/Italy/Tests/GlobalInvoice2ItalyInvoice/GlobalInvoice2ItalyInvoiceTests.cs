using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.GlobalInvoice.Common.Transforms.Helper;
using CargoWise.eHub.Products.GlobalInvoice.Italy.Transforms.GlobalInvoice2ItalyInvoice;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.GlobalInvoice.Italy.Tests
{
	[TestClass]
	public class GlobalInvoice2ItalyInvoiceTests
	{
		const string filePath = "GlobalInvoice2ItalyInvoice.TestFiles.";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestGlobalInvoice2ItalyInvoice()
		{
			var input = filePath + "Test1_input.xml";
			var expectedOutput = filePath + "Test1_output.xml";

			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockCertificateHelper = MockRepository.GenerateStrictMock<CertificateHelper>();
			var mockFilenameHelper = MockRepository.GenerateStrictMock<FilenameHelper>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER__1");
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("GEI_ITALY");
			mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "IT1234567_AAA.xml"));

			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GEIMSG", "GEI_ITALY", "TESTSENDER__1", "00003", "AAA"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GEIMSG", "GEI_ITALY", "TESTSENDER__1", "AAA", "00003"));

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.GlobalInvoice.Transforms.Italy.Accoglienza", "@maxlength", "10")).Return("3");
			mockCodeMapper.Expect(x => x.GetRecipientCode("GLB_ELEC_INVOICING", "GLB_ELEC_INVOICING", "Global Electronic Invoicing Configuration", "Agent VAT", "Agent Code", "GEI_ITALY")).Return("1234567");

			mockFilenameHelper.Expect(x => x.Base36Encoding(3, 5)).Return("AAA");

			var extensionObjects = new Dictionary<string, object>() { 
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockFilenameHelper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockDataModelAccessor },
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<GlobalInvoice2ItalyInvoice>(input, expectedOutput);


			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
			mockCertificateHelper.VerifyAllExpectations();
			mockFilenameHelper.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestResetCounter()
		{
			var input = filePath + "Test1_input.xml";
			var expectedOutput = filePath + "Test1_output.xml";

			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockCertificateHelper = MockRepository.GenerateStrictMock<CertificateHelper>();
			var mockFilenameHelper = MockRepository.GenerateStrictMock<FilenameHelper>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER__1");
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("GEI_ITALY");
			mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "IT1234567_AAA.xml"));

			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GEIMSG", "GEI_ITALY", "TESTSENDER__1", "00003", "AAA"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GEIMSG", "GEI_ITALY", "TESTSENDER__1", "AAA", "00003"));

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.GlobalInvoice.Transforms.Italy.Accoglienza", "@maxlength", "10")).Return("916132831");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.GlobalInvoice.Transforms.Italy.Accoglienza", "@maxlength", "1")).Return("1");
			mockCodeMapper.Expect(x => x.GetRecipientCode("GLB_ELEC_INVOICING", "GLB_ELEC_INVOICING", "Global Electronic Invoicing Configuration", "Agent VAT", "Agent Code", "GEI_ITALY")).Return("1234567");

			mockFilenameHelper.Expect(x => x.Base36Encoding(1, 5)).Return("AAA");

			var extensionObjects = new Dictionary<string, object>() { 
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockFilenameHelper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockDataModelAccessor },
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<GlobalInvoice2ItalyInvoice>(input, expectedOutput);


			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
			mockCertificateHelper.VerifyAllExpectations();
			mockFilenameHelper.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestTestSystem()
		{
			var input = filePath + "Test1_input.xml";
			var expectedOutput = filePath + "Test1_output.xml";

			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockCertificateHelper = MockRepository.GenerateStrictMock<CertificateHelper>();
			var mockFilenameHelper = MockRepository.GenerateStrictMock<FilenameHelper>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER__1");
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("GEI_ITALYTest");
			mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "IT1234567_AAA.xml"));

			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GEIMSG", "GEI_ITALYTest", "TESTSENDER__1", "00003", "AAA"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GEIMSG", "GEI_ITALYTest", "TESTSENDER__1", "AAA","00003"));

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.GlobalInvoice.Transforms.Italy.Accoglienza.Test", "@maxlength", "10")).Return("916132831");
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.GlobalInvoice.Transforms.Italy.Accoglienza.Test", "@maxlength", "1")).Return("1");
			mockCodeMapper.Expect(x => x.GetRecipientCode("GLB_ELEC_INVOICING", "GLB_ELEC_INVOICING", "Global Electronic Invoicing Configuration", "Agent VAT", "Agent Code", "GEI_ITALYTest")).Return("1234567");

			mockFilenameHelper.Expect(x => x.Base36Encoding(1, 5)).Return("AAA");

			var extensionObjects = new Dictionary<string, object>() { 
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockFilenameHelper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockDataModelAccessor },
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<GlobalInvoice2ItalyInvoice>(input, expectedOutput);

			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
			mockCertificateHelper.VerifyAllExpectations();
			mockFilenameHelper.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
		}

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestGlobalInvoice2ItalyInvoiceEscapedCData()
        {
            var input = filePath + "TestEscapedCData_input.xml";
            var expectedOutput = filePath + "TestCData_output.xml";

            var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
            var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
            var mockCertificateHelper = MockRepository.GenerateStrictMock<CertificateHelper>();
            var mockFilenameHelper = MockRepository.GenerateStrictMock<FilenameHelper>();
            var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

            mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER__1");
            mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("GEI_ITALY");
            mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "IT1234567_AAA.xml"));

            mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GEIMSG", "GEI_ITALY", "TESTSENDER__1", "00003", "AAA"));
            mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GEIMSG", "GEI_ITALY", "TESTSENDER__1", "AAA", "00003"));

            mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.GlobalInvoice.Transforms.Italy.Accoglienza", "@maxlength", "10")).Return("3");
            mockCodeMapper.Expect(x => x.GetRecipientCode("GLB_ELEC_INVOICING", "GLB_ELEC_INVOICING", "Global Electronic Invoicing Configuration", "Agent VAT", "Agent Code", "GEI_ITALY")).Return("1234567");

            mockFilenameHelper.Expect(x => x.Base36Encoding(3, 5)).Return("AAA");

            var extensionObjects = new Dictionary<string, object>() { 
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockFilenameHelper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockDataModelAccessor },
			};

            var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
            mapTester.Execute<GlobalInvoice2ItalyInvoice>(input, expectedOutput);


            mockCodeMapper.VerifyAllExpectations();
            mockContextAccessor.VerifyAllExpectations();
            mockCertificateHelper.VerifyAllExpectations();
            mockFilenameHelper.VerifyAllExpectations();
            mockDataModelAccessor.VerifyAllExpectations();
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestGlobalInvoice2ItalyInvoiceUnescapedCData()
        {
            var input = filePath + "TestUnescapedCData_input.xml";
            var expectedOutput = filePath + "TestCData_output.xml";

            var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
            var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
            var mockCertificateHelper = MockRepository.GenerateStrictMock<CertificateHelper>();
            var mockFilenameHelper = MockRepository.GenerateStrictMock<FilenameHelper>();
            var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

            mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER__1");
            mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("GEI_ITALY");
            mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "IT1234567_AAA.xml"));

            mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GEIMSG", "GEI_ITALY", "TESTSENDER__1", "00003", "AAA"));
            mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GEIMSG", "GEI_ITALY", "TESTSENDER__1", "AAA", "00003"));

            mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.GlobalInvoice.Transforms.Italy.Accoglienza", "@maxlength", "10")).Return("3");
            mockCodeMapper.Expect(x => x.GetRecipientCode("GLB_ELEC_INVOICING", "GLB_ELEC_INVOICING", "Global Electronic Invoicing Configuration", "Agent VAT", "Agent Code", "GEI_ITALY")).Return("1234567");

            mockFilenameHelper.Expect(x => x.Base36Encoding(3, 5)).Return("AAA");

            var extensionObjects = new Dictionary<string, object>() { 
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockFilenameHelper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockDataModelAccessor },
			};

            var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
            mapTester.Execute<GlobalInvoice2ItalyInvoice>(input, expectedOutput);


            mockCodeMapper.VerifyAllExpectations();
            mockContextAccessor.VerifyAllExpectations();
            mockCertificateHelper.VerifyAllExpectations();
            mockFilenameHelper.VerifyAllExpectations();
            mockDataModelAccessor.VerifyAllExpectations();
        }

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestGlobalInvoice2ItalyInvoiceNewCDataFormatWhichIsStillWrong()
		{
			var input = filePath + "TestNewCDATA_input.xml";
			var expectedOutput = filePath + "TestNewCDATA_output.xml";

			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockCertificateHelper = MockRepository.GenerateStrictMock<CertificateHelper>();
			var mockFilenameHelper = MockRepository.GenerateStrictMock<FilenameHelper>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER__1");
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("GEI_ITALY");
			mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "IT1234567_AAA.xml"));

			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GEIMSG", "GEI_ITALY", "TESTSENDER__1", "4002", "AAA"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GEIMSG", "GEI_ITALY", "TESTSENDER__1", "AAA", "4002"));

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.GlobalInvoice.Transforms.Italy.Accoglienza", "@maxlength", "10")).Return("3");
			mockCodeMapper.Expect(x => x.GetRecipientCode("GLB_ELEC_INVOICING", "GLB_ELEC_INVOICING", "Global Electronic Invoicing Configuration", "Agent VAT", "Agent Code", "GEI_ITALY")).Return("1234567");

			mockFilenameHelper.Expect(x => x.Base36Encoding(3, 5)).Return("AAA");

			var extensionObjects = new Dictionary<string, object>() { 
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockFilenameHelper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockDataModelAccessor },
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<GlobalInvoice2ItalyInvoice>(input, expectedOutput);


			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
			mockCertificateHelper.VerifyAllExpectations();
			mockFilenameHelper.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestGlobalInvoice2ItalyInvoiceReplacePLACEHOLDE()
		{
			var input = filePath + "Test_input_PLACEHOLDE.xml";
			var expectedOutput = filePath + "Test_output_PLACEHOLDE.xml";

			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockCertificateHelper = MockRepository.GenerateStrictMock<CertificateHelper>();
			var mockFilenameHelper = MockRepository.GenerateStrictMock<FilenameHelper>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER__1");
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("GEI_ITALY");
			mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "IT1234567_AAA.xml"));

			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GEIMSG", "GEI_ITALY", "TESTSENDER__1", "00003", "AAA"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GEIMSG", "GEI_ITALY", "TESTSENDER__1", "AAA", "00003"));

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.GlobalInvoice.Transforms.Italy.Accoglienza", "@maxlength", "10")).Return("3");
			mockCodeMapper.Expect(x => x.GetRecipientCode("GLB_ELEC_INVOICING", "GLB_ELEC_INVOICING", "Global Electronic Invoicing Configuration", "Agent VAT", "Agent Code", "GEI_ITALY")).Return("1234567");

			mockFilenameHelper.Expect(x => x.Base36Encoding(3, 5)).Return("AAA");

			var extensionObjects = new Dictionary<string, object>() {
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockFilenameHelper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockDataModelAccessor },
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<GlobalInvoice2ItalyInvoice>(input, expectedOutput);


			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
			mockCertificateHelper.VerifyAllExpectations();
			mockFilenameHelper.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
		}

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestGlobalInvoice2ItalyInvoiceFileNameUseIssuerRegistrationNumber()
        {
            var input = filePath + "TestIssuerRegistrationNumbe_input.xml";
            var expectedOutput = filePath + "TestIssuerRegistrationNumbe_output.xml";

            var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
            var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
            var mockCertificateHelper = MockRepository.GenerateStrictMock<CertificateHelper>();
            var mockFilenameHelper = MockRepository.GenerateStrictMock<FilenameHelper>();
            var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

            mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER__1");
            mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("GEI_ITALY");
            mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "IT01923170490CWEAR_AAA.xml"));

            mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GEIMSG", "GEI_ITALY", "TESTSENDER__1", "00003", "AAA"));
            mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("GEIMSG", "GEI_ITALY", "TESTSENDER__1", "AAA", "00003"));

            mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.GlobalInvoice.Transforms.Italy.Accoglienza", "@maxlength", "10")).Return("3");

            mockFilenameHelper.Expect(x => x.Base36Encoding(3, 5)).Return("AAA");

            var extensionObjects = new Dictionary<string, object>() {
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockCodeMapper },
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockContextAccessor },
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockFilenameHelper },
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", mockDataModelAccessor },
            };

            var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
            mapTester.Execute<GlobalInvoice2ItalyInvoice>(input, expectedOutput);


            mockCodeMapper.VerifyAllExpectations();
            mockContextAccessor.VerifyAllExpectations();
            mockCertificateHelper.VerifyAllExpectations();
            mockFilenameHelper.VerifyAllExpectations();
            mockDataModelAccessor.VerifyAllExpectations();
        }
    }
}
