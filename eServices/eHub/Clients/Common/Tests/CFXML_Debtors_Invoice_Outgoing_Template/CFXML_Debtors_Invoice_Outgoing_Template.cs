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
using CargoWise.eHub.Clients.Common.Transforms.CFXML_Debtors_Invoice_Outgoing_Template;

namespace CargoWise.eHub.Clients.Common.Tests
{
    [TestClass]
    public class CFXML_Debtors_Invoice_Outgoing_TemplateTests
    {
        private string path;
        private MapTester mapTester;

        private void doTest(string sourceFile, string expectedFile)
        {
            mapTester.ExecuteCompiled<CFXML_Debtors_Invoice_Outgoing_Template>(path + sourceFile, path + expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestCFXML_Debtors_Invoice_Outgoing_Template()
        {
            InitialiseCodeMapsTestingContext();

            mapTester = new MapTester(Assembly.GetExecutingAssembly());

            path = "CFXML_Debtors_Invoice_Outgoing_Template.TestFiles.";

            doTest("01_Unitrans_Input.xml",
                "01_Unitrans_Output.xml");

            doTest("02_Unitrans_Input.xml",
                "02_Unitrans_Output.xml");
        }

        private static void InitialiseCodeMapsTestingContext()
        {
            string Sender = "CFRDI2121S";
            string Recipient = "CFRDI2121R";
            string TS_Name = "CFR Accounts Receivable";

            UnitTestHelper helper = new UnitTestHelper();
            helper.SetActiveTS(Sender, Recipient, TS_Name);

            CodeMapsTestingContext ctx = helper.GetCodeMapsTestingContext();

            var ta = new TransformAccessor();
            ta.SetCodeMapsTestingContext(ctx);

            //helper.WriteSqlToFile(@"c:\temp\output.sql");
        }
    }
}
