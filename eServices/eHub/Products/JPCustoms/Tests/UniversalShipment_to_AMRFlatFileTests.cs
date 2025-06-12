using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Core.Transforms.Helper.Testing;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eHub.Products.JPCustoms.Transforms.Helper;
using CargoWise.eHub.Products.JPCustoms.Transforms.UniversalShipment2AMRFlatFile;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;


namespace CargoWise.eHub.Products.JPCustoms.Tests
{
    [TestClass]
    public class UniversalShipment_to_AMRFlatFileTests
    {

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void UniversalShipment_to_AMRFlatFile_Test1()
        {
            InitialiseMessageTestingContext();
            InitialiseCodeMapsTestingContext();

            string sourceFile = "UniversalShipment_to_AMRFlatFileTests_input.input_1.xml";
            string outputFile = "UniversalShipment_to_AMRFlatFileTests_output.output_1.xml";

            var JPCustomsDataModelAccessor = new JPCustomsDataModelAccessor();
            var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
            JPCustomsDataModelAccessor.DataModelAccessor = mockDataModelAccessor;

            var clientRegistration = new eHubClientRegistration() { CX_Code = "TestClientUserName", CX_Password1 = "TestClientPassword" };
            mockDataModelAccessor.Expect(x => x.GetClientRegistration("HYEDAUVIZ", "JPCustomsAccount_ClientLevel")).Return(clientRegistration).Repeat.Twice();
            mockDataModelAccessor.Expect(x => x.GetClientRegistration("eHub", "JPCustomsAccount_ClientLevel")).Return(null).Repeat.Any();

            var extensionObjects = new Dictionary<string, object>() { 
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", JPCustomsDataModelAccessor }
            };
            MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

            mapTester.Execute<UniversalShipment_to_AMRFlatFile>(sourceFile, outputFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void UniversalShipment_to_AMRFlatFile_Test2()
        {
            TestMap("UniversalShipment_to_AMRFlatFileTests_input.input_2.xml", "UniversalShipment_to_AMRFlatFileTests_output.output_2.xml");
        }

        private static void TestMap(string sourceFile, string outputFile)
        {
            InitialiseMessageTestingContext();
            InitialiseCodeMapsTestingContext();

            var JPCustomsDataModelAccessor = new JPCustomsDataModelAccessor();
            var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
            JPCustomsDataModelAccessor.DataModelAccessor = mockDataModelAccessor;

            var clientRegistration = new eHubClientRegistration() { CX_Code = "TestClientUserName", CX_Password1 = "TestClientPassword" };
            mockDataModelAccessor.Expect(x => x.GetClientRegistration("HYEDAUVIZ", "JPCustomsAccount_ClientLevel")).Return(clientRegistration).Repeat.Twice();
            mockDataModelAccessor.Expect(x => x.GetClientRegistration("eHub", "JPCustomsAccount_ClientLevel")).Return(null).Repeat.Any();

            var extensionObjects = new Dictionary<string, object>() { 
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", JPCustomsDataModelAccessor }
            };
            MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

            mapTester.Execute<UniversalShipment_to_AMRFlatFile>(sourceFile, outputFile);
        }

        static void InitialiseMessageTestingContext()
        {
            var mctx = new TestingMessageContext();
            mctx.Write("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "HYEDAUVIZ");

            var ca = new ContextAccessor();
            ca.SetTestingMessageContext(mctx);
        }

        static void InitialiseCodeMapsTestingContext()
        {
            CodeMapsTestingContext ctx = new CodeMapsTestingContext();

            ctx.ActionProcedures.Add(new ActionProcedure
            {
                Procedure = "CalculateTimeZoneOffset",
                OutputParm = "@offset",
                InputParms = new List<string> { 
                    "@UNLOCO", "AUSYD", 
                    "@localtime", "2014-06-08T12:35:00"
                },
                Result = "+02:00"
            });

            ctx.ActionProcedures.Add(new ActionProcedure
            {
                Procedure = "GetUserIDPasswordReference",
                OutputParm = "@Result",
                InputParms = new List<string> { 
                    "@ApplicationCode", "JPC", 
                    "@eHubID", "eHub", 
                    "@IsGettingPassword", "0"
                },
                Result = "SPID"
            });

            ctx.ActionProcedures.Add(new ActionProcedure
            {
                Procedure = "GetUserIDPasswordReference",
                OutputParm = "@Result",
                InputParms = new List<string> { 
                    "@ApplicationCode", "JPC", 
                    "@eHubID", "eHub", 
                    "@IsGettingPassword", "1"
                },
                Result = "SPPW"
            });

            ctx.ActionProcedures.Add(new ActionProcedure
            {
                Procedure = "GetUserIDPasswordReference",
                OutputParm = "@Result",
                InputParms = new List<string> { 
                    "@ApplicationCode", "JPC", 
                    "@eHubID", "HYEDAUVIZ", 
                    "@IsGettingPassword", "0"
                },
                Result = "USRID"
            });

            ctx.ActionProcedures.Add(new ActionProcedure
            {
                Procedure = "GetUserIDPasswordReference",
                OutputParm = "@Result",
                InputParms = new List<string> { 
                    "@ApplicationCode", "JPC", 
                    "@eHubID", "HYEDAUVIZ", 
                    "@IsGettingPassword", "1"
                },
                Result = "USRPW"
            });

            var ta = new TransformAccessor();
            ta.SetCodeMapsTestingContext(ctx);
        }
    }
}