using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.Riba.Transforms;
using Microsoft.BizTalk.TestTools.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.Riba.Tests
{
    [TestClass]
    public class EDIUniversalBatch2FlexibleFormatTests
    {
        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestEDIUniversalBatch2RIBA_Native()
        {
            AssertRibaNativeTestCase(
                GetEmbeddedResourceFile("TestFiles.UDM_TRX_XDC_BodyText_RIB.xml"),
                GetEmbeddedResourceFile("TestFiles.RIBA.txt")
            );
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestEDIUniversalBatch2RIBA_Native_ManyTransactions()
        {
            AssertRibaNativeTestCase(
                GetEmbeddedResourceFile("TestFiles.UDM_TRX_XDC_BodyText_RIB_ManyTransactions.xml"),
                GetEmbeddedResourceFile("TestFiles.RIBA_ManyTransactions.txt")
            );
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestEDIUniversalBatch2RIBA_Native_AmountSummarizing()
        {
            AssertRibaNativeTestCase(
                GetEmbeddedResourceFile("TestFiles.UDM_TRX_XDC_BodyText_RIB_AmountSummarizing.xml"),
                GetEmbeddedResourceFile("TestFiles.RIBA_AmountSummarizing.txt")
            );
        }

        void AssertRibaNativeTestCase(string sourceFile, string expectedFile)
        {
            string outputFile = Path.GetTempFileName();

            try
            {
                EDIUniversalBatch2RIBA map = new EDIUniversalBatch2RIBA();
                map.TestMap(sourceFile, InputInstanceType.Xml, outputFile, OutputInstanceType.Native);

                string expectedFileContent = File.ReadAllText(expectedFile).TrimEnd();
                string outputFileContent = File.ReadAllText(outputFile).TrimEnd(); // use trimEnd() here due to 2 extra mysterious whitespace in the output file.

                Assert.AreEqual(expectedFileContent, outputFileContent);
            }
            finally
            {
                File.Delete(sourceFile);
                File.Delete(expectedFile);
                File.Delete(outputFile);
            }
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestEDIUniversalBatch2DE_Native()
        {
            string sourceFile = GetEmbeddedResourceFile("TestFiles.UDM_TRX_XDC_BodyText_DE.xml");
            string expectedFile = GetEmbeddedResourceFile("TestFiles.DE.txt");
            string outputFile = Path.GetTempFileName();

            try
            {
                
                EDIUniversalBatch2DE map = new EDIUniversalBatch2DE();
                map.TestMap(sourceFile, InputInstanceType.Xml, outputFile, OutputInstanceType.Native);

                string expectedFileContent = File.ReadAllText(expectedFile).TrimEnd();
                string outputFileContent = File.ReadAllText(outputFile).TrimEnd(); // use trimEnd() here due to 2 extra mysterious whitespace in the output file.

                Assert.AreEqual(expectedFileContent, outputFileContent);
            }
            finally
            {
                File.Delete(sourceFile);
                File.Delete(expectedFile);
                File.Delete(outputFile);
            }
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniversalTransactionBatch2RIBA_XML()
        {
            AssertRibaXmlTestCase("TestFiles.UDM_TRX_XDC_BodyText_RIB.xml", "TestFiles.RIBA.xml");
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniversalTransactionBatch2RIBA_XML_ManyTransactions()
        {
            AssertRibaXmlTestCase("TestFiles.UDM_TRX_XDC_BodyText_RIB_ManyTransactions.xml", "TestFiles.RIBA_ManyTransactions.xml");
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniversalTransactionBatch2RIBA_XML_AmountSummarizing()
        {
            AssertRibaXmlTestCase("TestFiles.UDM_TRX_XDC_BodyText_RIB_AmountSummarizing.xml", "TestFiles.RIBA_AmountSummarizing.xml");
        }

        void AssertRibaXmlTestCase(string inputFile, string expectedFile)
        {
            var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
            mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "PAR00001042.txt"));

            var extensionObjects = new Dictionary<string, object>()
            {
                {"http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockContextAccessor}
            };

            var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
            mapTester.Execute<EDIUniversalBatch2RIBA>(inputFile, expectedFile);

            mockContextAccessor.VerifyAllExpectations();
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniversalTransactionBatch2DE_XML()
        {
            var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
            mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "PAR00001042.txt"));

            var extensionObjects = new Dictionary<string, object>()
            {
                {"http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockContextAccessor}
            };

            string inputFile = "TestFiles.UDM_TRX_XDC_BodyText_DE.xml";
            string expectedFile = "TestFiles.DE.xml";

            var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
            mapTester.Execute<EDIUniversalBatch2DE>(inputFile, expectedFile);

            mockContextAccessor.VerifyAllExpectations();
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniversalTransactionBatch2SEPA_XML()
        {
            AssertSepaNativeTestCase("TestFiles.UDM_TRX_XDC_BodyText_SEP.xml", "TestFiles.SEPA.xml");
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniversalTransactionBatch2SEPA_XML_ManyTransactions()
        {
            AssertSepaNativeTestCase("TestFiles.UDM_TRX_XDC_BodyText_SEP_ManyTransactions.xml", "TestFiles.SEPA_ManyTransactions.xml");
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniversalTransactionBatch2SEPA_XML_LargeListOfNumbers()
        {
            AssertSepaNativeTestCase("TestFiles.UDM_TRX_XDC_BodyText_SEP_LargeListOfNumbers.xml", "TestFiles.SEPA_LargeListOfNumbers.xml");
        }

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalTransactionBatch2SEPA_XML_Decimals_DebtorBank()
		{
			AssertSepaNativeTestCase("TestFiles.UDM_TRX_XDC_BodyText_SEP_Decimals_DebtorBank.xml", "TestFiles.SEPA_Decimals_DebtorBank.xml");
		}

		void AssertSepaNativeTestCase(string inputFile, string expectedFile)
        {
            var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
            mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "PAR00001042.xml"));

            var extensionObjects = new Dictionary<string, object>()
            {
                {"http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockContextAccessor}
            };

            var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
            mapTester.Execute<EDIUniversalBatch2SEPA>(inputFile, expectedFile);

            mockContextAccessor.VerifyAllExpectations();
        }

        string GetEmbeddedResourceFile(string resourceName)
        {
            string tempFileName = Path.GetTempFileName();
            string fullResourceName = Assembly.GetExecutingAssembly().GetName().Name + '.' + resourceName;
            using (Stream reader = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullResourceName))
            {
                using (Stream writer = new FileStream(tempFileName, FileMode.Create))
                {
                    reader.CopyTo(writer);
                    writer.Flush();
                }
            }
            return tempFileName;
        }
    }
}
