using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.GBCustoms.Core.BT.Tests.Transforms.GBCustoms2GBCustoms
{
    [TestClass]
    public class GBCustoms2GBCustomsTests
    {
        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void GBCustoms2GBCustoms_Declaration()
        {
            AssertMapping("Transforms.GBCustoms2GBCustoms.TestFiles.Declaration.xml", "GBCustoms-Direct", "AAA.BBB.CCC");
			AssertMapping("Transforms.GBCustoms2GBCustoms.TestFiles.Declaration MCP.xml", "GBCustoms-MCP", "WTGPRD-MCP-MCPBadge");
			AssertMapping("Transforms.GBCustoms2GBCustoms.TestFiles.Declaration CNS.xml", "GBCustoms-CNS", "WTGPRD-CNS-CNSBadge");
			AssertMapping("Transforms.GBCustoms2GBCustoms.TestFiles.Declaration Pentant.xml", "GBCustoms-Pentant", "WTGPRD-Pentant-PentantBadge");
			AssertMapping("Transforms.GBCustoms2GBCustoms.TestFiles.Declaration ICSNI.xml", "GBCustoms-ICSNI", "HYEMIK.GB896458895023");
		}

        public void AssertMapping(string testFile, string destinationParty, string overrideFileName)
        {
            var mockContextAccessor = MockRepository.GenerateMock<ContextAccessor>();
            mockContextAccessor.Stub(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("GBCustoms");
            mockContextAccessor.Stub(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("WTGEKHPRD");
            var extensionObjects = new Dictionary<string, object>() {
                { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor }
            };

            var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
            mapTester.ExecuteCompiled<BT.Transforms.GBCustoms2GBCustoms>(testFile, testFile);

            mockContextAccessor.AssertWasCalled(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", destinationParty));
            mockContextAccessor.AssertWasCalled(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", overrideFileName));
        }
    }
}
