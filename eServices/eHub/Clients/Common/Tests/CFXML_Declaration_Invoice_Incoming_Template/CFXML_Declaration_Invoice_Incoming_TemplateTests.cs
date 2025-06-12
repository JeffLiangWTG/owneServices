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
using CargoWise.eHub.Clients.Common.Transforms.CFXML_Declaration_Invoice_Incoming_Template;

namespace CargoWise.eHub.Clients.Common.Tests
{
    [TestClass]
    public class CFXML_Declaration_Invoice_Incoming_TemplateTests
    {
        private string path;
        private MapTester mapTester;

        private void doTest(string sourceFile, string expectedFile)
        {
            mapTester.ExecuteCompiled<CFXML_Declaration_Invoice_Incoming_Template>(path + sourceFile,
                                                                        path + expectedFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestCFXML_Declaration_Invoice_Incoming_Template()
        {
            InitialiseCodeMapsTestingContext();

            mapTester = new MapTester(Assembly.GetExecutingAssembly());

            path = "CFXML_Declaration_Invoice_Incoming_Template.TestFiles.";

            doTest("01_ShipmentDetails_Input.xml",
                   "01_ShipmentDetails_Output.xml");

            doTest("02_All_Fields_Input.xml",
                   "02_All_Fields_Output.xml");
        }

        private static void InitialiseCodeMapsTestingContext()
        {
            string Sender = "XXXXXXXXXX";
            string Recipient = "XXXXXXXXXX";
            string TS_Name = "XXXXXXXXXX";

            UnitTestHelper helper = new UnitTestHelper();
            helper.SetActiveTS(Sender, Recipient, TS_Name);
            
            CodeMapsTestingContext ctx = helper.GetCodeMapsTestingContext();

            ctx.ActionProcedures.Add(new ActionProcedure
            {
                Procedure = "GetColumnValue",
                OutputParm = "@result",
                InputParms = new List<string> { 
					"@tableName", "RefCountry", "@inputColumn", "RN_IsoAlpha3Code", "@outputColumn", "RN_Code", "@inputValue", "USA"
				},
                Result = "US"
            });
            
            var ta = new TransformAccessor();
            ta.SetCodeMapsTestingContext(ctx);
        }
    }
}
