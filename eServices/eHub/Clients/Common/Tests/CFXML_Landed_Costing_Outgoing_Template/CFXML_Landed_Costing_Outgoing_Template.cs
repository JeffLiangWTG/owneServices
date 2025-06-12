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
using CargoWise.eHub.Clients.Common.Transforms.CFXML_Landed_Costing_Outgoing_Template;

namespace CargoWise.eHub.Clients.Common.Tests
{
    [TestClass]
    public class CFXML_Landed_Costing_Outgoing_TemplateTests
    {
        private string path;
        private MapTester mapTester;

        private void doTest(string sourceFile, string expectedFile)
        {
            mapTester.ExecuteCompiled<CFXML_Landed_Costing_Outgoing_Template>(path + sourceFile, path + expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestCFXML_Landed_Costing_Outgoing_Template()
        {
            InitialiseCodeMapsTestingContext();

            mapTester = new MapTester(Assembly.GetExecutingAssembly());

            path = "CFXML_Landed_Costing_Outgoing_Template.TestFiles.";

            doTest("01_CFXML_Landed_Costing_Outgoing_Template_Input.xml", "01_CFXML_Landed_Costing_Outgoing_Template_Output.xml");

            doTest("02_CFXML_Landed_Costing_Outgoing_Template_Input.xml", "02_CFXML_Landed_Costing_Outgoing_Template_Output.xml");
        }

        private static void InitialiseCodeMapsTestingContext()
        {
            string Sender = "XXXXXXXXXX";
            string Recipient = "XXXXXXXXXX";
            string TS_Name = "XXXXXXXXXX";

            UnitTestHelper helper = new UnitTestHelper();
            helper.SetActiveTS(Sender, Recipient, TS_Name);
            CodeSet cs;

            //##########################//
            //## Common Code Mappings ##//
            //##########################//
            
            helper.SetActiveTS("eHub", "eHub", "Common Code Mappings");

            cs = helper.NewCodeSet("Transport Mode");

            cs.AddFields("Code", "Description");
            cs.AddRecord("AIR", "Air");
            cs.AddRecord("SEA", "Sea");
            cs.AddRecord("ROA", "Road");
            cs.AddRecord("RAI", "Rail");
            cs.AddRecord("STO", "Storage");
            cs.AddRecord("%", "");

            //######################//
            //## Action Procedure ##//
            //######################//

            Action_Procedure ap;
            
            ap = helper.NewActionProcedure();
            ap.Procedure = "GetIATAfromUNLOCO";
            ap.OutputParm = "@IATACode";
            ap.AddInputParms("@UNLOCOCode", "DEHAM");
            ap.Result = "HAM";

            ap = helper.NewActionProcedure();
            ap.Procedure = "GetIATAfromUNLOCO";
            ap.OutputParm = "@IATACode";
            ap.AddInputParms("@UNLOCOCode", "AUSYD");
            ap.Result = "SYD";

            ap = helper.NewActionProcedure();
            ap.Procedure = "GetIATAfromUNLOCO";
            ap.OutputParm = "@IATACode";
            ap.AddInputParms("@UNLOCOCode", "ZAJNB");
            ap.Result = "JNB";

            ap = helper.NewActionProcedure();
            ap.Procedure = "GetIATAfromUNLOCO";
            ap.OutputParm = "@IATACode";
            ap.AddInputParms("@UNLOCOCode", "ZADUR");
            ap.Result = "DUR";

            CodeMapsTestingContext ctx = helper.GetCodeMapsTestingContext();

            var ta = new TransformAccessor();
            ta.SetCodeMapsTestingContext(ctx);

            helper.WriteSqlToFile(@"c:\temp\output.sql");
        }
    }
}
