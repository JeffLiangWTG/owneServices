using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.ComplianceTaxReport.ItalyBlackList.Transformations;
using Microsoft.BizTalk.TestTools.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ComplianceTaxReport.ItalyBackList.Tests
{
    [TestClass]
    public class UniversalTransactionBatch2ItalyBlackListTest
    {

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniversalTransactionBatch2ItalyBlackList_OverrideFilename()
        {
            var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
            mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "IT1--IT-BL0-20141217-20141217.txt"));

            var extensionObjects = new Dictionary<string, object>()
            {
				{"http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockContextAccessor}
			};

            string inputFile = "TestFiles.UniversalTransactionBatch_input.xml";
            string expectedFile = "TestFiles.ItalyBlackList_output.xml";

            var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
            mapTester.ExecuteCompiled<UniversalTransactionBatch2ItalyBlackList>(inputFile, expectedFile);

            mockContextAccessor.VerifyAllExpectations();
        }
    }
}
