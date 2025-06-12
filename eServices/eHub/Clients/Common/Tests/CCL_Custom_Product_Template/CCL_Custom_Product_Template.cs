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
using CargoWise.eHub.Clients.Common.Transforms.CCL_Custom_Product_Template;

namespace CargoWise.eHub.Clients.Common.Tests
{
    [TestClass]
    public class CCL_Custom_Product_TemplateTests
    {
        private string path;
        private MapTester mapTester;

        private void doTest(string sourceFile, string expectedFile)
        {
            mapTester.ExecuteCompiled<CCL_Custom_Product_Template>(path + sourceFile, path + expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestCCL_Custom_Product_Template()
        {
            InitialiseCodeMapsTestingContext();

            mapTester = new MapTester(Assembly.GetExecutingAssembly());

            path = "CCL_Custom_Product_Template.TestFiles.";

            doTest("01_CSV_Input.xml",
                "01_CSV_Output.xml");

            doTest("02_CSV_Input.xml",
                "02_CSV_Output.xml");

        }

        private static void InitialiseCodeMapsTestingContext()
        {
            string Sender = "CCLCUSPRODUCT";
            string Recipient = "CCLCUSPRODUCT";
            string TS_Name = "CCLCUSPRODUCT";

            UnitTestHelper helper = new UnitTestHelper();
            helper.SetActiveTS(Sender, Recipient, TS_Name);

            CodeMapsTestingContext ctx = helper.GetCodeMapsTestingContext();

            var ta = new TransformAccessor();
            ta.SetCodeMapsTestingContext(ctx);

            //helper.WriteSqlToFile(@"c:\temp\output.sql");
        }
    }
}
