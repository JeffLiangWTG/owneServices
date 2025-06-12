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
using CargoWise.eHub.Clients.COR.Transforms.Uniship_2_ACR_X12_214;


namespace CargoWise.eHub.Clients.COR.Tests
{
    [TestClass]
    public class Uniship_2_ACR_X12_214Test
    {
        private string path;
        private MapTester mapTester;

        private void doTest(string sourceFile, string expectedFile)
        {
            mapTester.ExecuteCompiled<Uniship_2_ACR_X12_214>(path + sourceFile,
                                                              path + expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUniship_2_ACR_X12_214()
        {
            InitialiseCodeMapsTestingContext();

            mapTester = new MapTester(Assembly.GetExecutingAssembly());

            path = "Uniship_2_ACR_X12_214.TestFiles.";

            doTest("01_D1_With_Containers_Input.xml",
                   "01_D1_With_Containers_Output.xml");

            doTest("02_AF_Zero_Containers_Input.xml",
                   "02_AF_Zero_Containers_Output.xml");
        }

        private static void InitialiseCodeMapsTestingContext()
        {
            string Sender = "CORDTWENT";
            string Recipient = "CORDTWENT_ARC";
            string TS_Name = "ARC 214 - Send Shipment Status";


            UnitTestHelper helper = new UnitTestHelper();
            helper.SetActiveTS(Sender, Recipient, TS_Name);
            CodeSet cs;


            cs = helper.NewCodeSet("Defaults", isDefault: true);

            cs.AddFields("SCAC");
            cs.AddRecord("XXXX");
            

            cs = helper.NewCodeSet("Shipment Status");

            cs.AddFields("Trigger Purpose", "Shipment Status - AT701", "Stop Number - L1101");
            cs.AddRecord("AF"             ,"AF"                      , "1"                  );
            cs.AddRecord("D1"             ,"D1"                      , "2"                  );

            CodeMapsTestingContext ctx = helper.GetCodeMapsTestingContext();

            var ta = new TransformAccessor();
            ta.SetCodeMapsTestingContext(ctx);
        }


    }
}
