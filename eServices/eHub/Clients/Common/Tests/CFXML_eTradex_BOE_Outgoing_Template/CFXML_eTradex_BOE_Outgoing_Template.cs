using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Core.Transforms.Helper.Testing;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using CargoWise.eHub.Clients.Common.UnitTestHelperFramework;
using CargoWise.eHub.Clients.Common.Transforms.CFXML_eTradex_BOE_Outgoing_Template;

namespace CargoWise.eHub.Clients.Common.Tests
{
    [TestClass]
    public class CFXML_eTradex_BOE_Outgoing_TemplateTests
    {
        private string path;
        private MapTester mapTester;

        private void doTest(string sourceFile, string expectedFile)
        {
            mapTester.ExecuteCompiled<CFXML_eTradex_BOE_Outgoing_Template>(path + sourceFile, path + expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestCFXML_eTradex_BOE_Outgoing_Template()
        {
            InitialiseCodeMapsTestingContext();

            mapTester = new MapTester(Assembly.GetExecutingAssembly());

            path = "CFXML_eTradex_BOE_Outgoing_Template.TestFiles.";

            doTest("01_CFXML_eTradex_BOE_Outgoing_Input.xml", "01_CFXML_eTradex_BOE_Outgoing_Output.xml");

            doTest("02_CFXML_eTradex_BOE_Outgoing_Input.xml", "02_CFXML_eTradex_BOE_Outgoing_Output.xml");
        }

        private static void InitialiseCodeMapsTestingContext()
        {
            string Sender = "XXXXXXXXXX";
            string Recipient = "XXXXXXXXXX";
            string TS_Name = "XXXXXXXXXX";

            UnitTestHelper helper = new UnitTestHelper();
            helper.SetActiveTS(Sender, Recipient, TS_Name);
            CodeSet cs;

            cs = helper.NewCodeSet("Transport Mode");
            cs.AddFields("Input Code", "Output Code");
            cs.AddRecord("%", "<PassThroughKey>");

            CodeMapsTestingContext ctx = helper.GetCodeMapsTestingContext();

            var ta = new TransformAccessor();
            ta.SetCodeMapsTestingContext(ctx);

            helper.WriteSqlToFile(@"c:\temp\output.sql");
        }
    }
}
