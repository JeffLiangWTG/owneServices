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
using CargoWise.eHub.Clients.Common.Transforms.CFXML_UniShip_2_CustomsXml;

namespace CargoWise.eHub.Clients.Common.Tests
{
    [TestClass]
    public class CFXML_UniShip_2_CustomsXmlTests
    {
        private string path;
        private MapTester mapTester;

        private void doTest(string sourceFile, string expectedFile)
        {
            mapTester.ExecuteCompiled<CFXML_UniShip_2_CustomsXml>(path + sourceFile, path + expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestCFXML_UniShip_2_CustomsXml()
        {
            InitialiseCodeMapsTestingContext();

            mapTester = new MapTester(Assembly.GetExecutingAssembly());

            path = "CFXML_UniShip_2_CustomsXml.TestFiles.";

            doTest("Declaration_Input_01.xml",
                "Declaration_Output_01.xml");

            doTest("Declaration_Input_02.xml",
                "Declaration_Output_02.xml");
        }

        private static void InitialiseCodeMapsTestingContext()
        {
            string Sender = "XXXXXXXXXX";
            string Recipient = "XXXXXXXXXX";
            string TS_Name = "XXXXXXXXXX";

            UnitTestHelper helper = new UnitTestHelper();
            helper.SetActiveTS(Sender, Recipient, TS_Name);

            CodeMapsTestingContext ctx = helper.GetCodeMapsTestingContext();

            var ta = new TransformAccessor();
            ta.SetCodeMapsTestingContext(ctx);

            //helper.WriteSqlToFile(@"c:\temp\output.sql");
        }
    }
}
