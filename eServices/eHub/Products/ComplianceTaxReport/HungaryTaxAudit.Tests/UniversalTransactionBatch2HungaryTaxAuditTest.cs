using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.ComplianceTaxReport.HungaryTaxAudit.Transformations;
using Microsoft.BizTalk.TestTools.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ComplianceTaxReport.HungaryTaxAudit.Tests
{
    [TestClass]
    public class UniversalTransactionBatch2ItalyBlackListTest
    {
        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniversalTransactionBatch2HungaryTaxAudit()
        {
            var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
            mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "BUD--HU-RAF-20151201-20151230.xml"));

            var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
            mockDateMapper.Expect(x => x.CurrentDateTimeUTC(Arg.Is("yyyy-MM-dd"))).Return("2015-12-24");

            var extensionObjects = new Dictionary<string, object>()
            {
				{"http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockContextAccessor},
				{"http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper}
			};

            string inputFile = "TestFiles.UniversalTransactionBatch_input.xml";
            string expectedFile = "TestFiles.UniversalTransactionBatch2HungaryTaxAudit_output.xml";

            var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
            mapTester.Execute<UniversalTransactionBatch2HungaryTaxAudit>(inputFile, expectedFile);

            mockContextAccessor.VerifyAllExpectations();
        }
    }
}
