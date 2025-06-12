using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.DataAccess.Integration;
using CargoWise.eHub.Products.Core.Tests;

using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Core.Transforms.Helper.Testing;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using Microsoft.BizTalk.TestTools.Mapper;
using Microsoft.BizTalk.TestTools.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using CargoWise.eHub.Products.ComplianceTaxReport.Transformations;
using CargoWise.eHub.Core.PropertySchemas;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.Test.BizTalk.PipelineObjects;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ComplianceTaxReport.Tests
{
    /// <summary>
    /// Summary description for ComplianceTaxReportWrapper2UniversalEventTest
    /// </summary>
    [TestClass]
    public class ComplianceTaxReportWrapper2UniversalEventTest
    {
        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestComplianceTaxReportWrapper2UniversalEvent_ItalyBlackList()
        {
            var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
            mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("SenderID");
            mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("RecipientID");
            mockContextAccessor.Expect(x => x.SetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "RecipientID"));
            mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "SenderID"));
            mockContextAccessor.Expect(x => x.GetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06")).Return("IT1--IT-SUB-20150301-20150430.txt");

            var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
            mockDateMapper.Expect(x => x.CurrentDateTimeUTC(Arg.Is("yyyy-MM-ddTHH:mm:ss.fff"))).Return("2015-06-15T10:08:33.000");

            var extensionObjects = new Dictionary<string, object>()
            {
				{"http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockContextAccessor},
				{"http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper}
			};

            string inputFile = "TestFiles.ItalyBlackListWrapper.xml";
            string expectedFile = "TestFiles.ItalyBlackListUniversalEvent.xml";
            var comparer = new ExcludingComparer(new List<string>() { "//*[local-name()='EventTime']" });

            var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
            mapTester.Execute<ComplianceTaxReportWrapper2UniversalEvent>(inputFile, expectedFile);

            mockDateMapper.VerifyAllExpectations();
            mockContextAccessor.VerifyAllExpectations();
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestComplianceTaxReportWrapper2UniversalEvent_HungaryTaxAudit()
        {
            var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
            mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("SenderID");
            mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("RecipientID");
            mockContextAccessor.Expect(x => x.SetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "RecipientID"));
            mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "SenderID"));
            mockContextAccessor.Expect(x => x.GetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06")).Return("BUD--HU-RAF-20151201-20151230.xml");

            var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
            mockDateMapper.Expect(x => x.CurrentDateTimeUTC(Arg.Is("yyyy-MM-ddTHH:mm:ss.fff"))).Return("2016-01-20T02:21:13.563");

            var extensionObjects = new Dictionary<string, object>()
            {
				{"http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockContextAccessor},
				{"http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockDateMapper}
			};

            string inputFile = "TestFiles.HungaryTaxAuditWrapper.xml";
            string expectedFile = "TestFiles.HungaryTaxAuditUniversalEvent.xml";
            var comparer = new ExcludingComparer(new List<string>() { "//*[local-name()='EventTime']" });

            var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
            mapTester.Execute<ComplianceTaxReportWrapper2UniversalEvent>(inputFile, expectedFile);

            mockDateMapper.VerifyAllExpectations();
            mockContextAccessor.VerifyAllExpectations();
        }
    }
}
