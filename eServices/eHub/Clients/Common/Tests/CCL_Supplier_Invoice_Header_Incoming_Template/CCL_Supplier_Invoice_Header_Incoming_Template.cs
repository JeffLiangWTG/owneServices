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
using CargoWise.eHub.Clients.Common.Transforms.CCL_Supplier_Invoice_Header_Incoming_Template;

namespace CargoWise.eHub.Clients.Common.Tests
{
    [TestClass]
    public class CCL_Supplier_Invoice_Header_Incoming_TemplateTests
    {
        private string path;
        private MapTester mapTester;

        private void doTest(string sourceFile, string expectedFile)
        {
            mapTester.ExecuteCompiled<CCL_Supplier_Invoice_Header_Incoming_Template>(path + sourceFile, path + expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestCCL_Supplier_Invoice_Header_Incoming_Template()
        {
            InitialiseCodeMapsTestingContext();

            mapTester = new MapTester(Assembly.GetExecutingAssembly());

            path = "CCL_Supplier_Invoice_Header_Incoming_Template.TestFiles.";

            doTest("01_CCL_Supplier_Invoice_Header_Incoming_Template_Input.xml", "01_CCL_Supplier_Invoice_Header_Incoming_Template_Output.xml");

            doTest("02_CCL_Supplier_Invoice_Header_Incoming_Template_Input.xml", "02_CCL_Supplier_Invoice_Header_Incoming_Template_Output.xml");
        }

        private static void InitialiseCodeMapsTestingContext()
        {
            string Sender = "CCLINVHEADER";
            string Recipient = "CCLINVHEADER";
            string TS_Name = "CCLINVHEADER";

            UnitTestHelper helper = new UnitTestHelper();
            helper.SetActiveTS(Sender, Recipient, TS_Name);
                       
            CodeMapsTestingContext ctx = helper.GetCodeMapsTestingContext();

            var ta = new TransformAccessor();
            ta.SetCodeMapsTestingContext(ctx);

            helper.WriteSqlToFile(@"c:\temp\output.sql");
        }
    }
}
