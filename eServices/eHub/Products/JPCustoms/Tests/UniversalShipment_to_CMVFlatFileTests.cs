using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Core.Transforms.Helper.Testing;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eHub.Products.JPCustoms.Transforms.Helper;
using CargoWise.eHub.Products.JPCustoms.Transforms.UniversalShipment2CMVFlatFile;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.JPCustoms.Tests
{
    [TestClass]
    public class UniversalShipment_to_CMVFlatFileTests
    {
        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void UniversalShipment_to_CMVFlatFile_Test_BlanketChange_N()
        {
            InitialiseMessageTestingContext();
            InitialiseCodeMapsTestingContext();

            string sourceFile = "UniversalShipment_to_CMVFlatFileTests_input.input2017_BlanketChange_N.xml";
            string outputFile = "UniversalShipment_to_CMVFlatFileTests_output.output2017_BlanketChange_N.xml";

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

            mapTester.Execute<UniversalShipment_to_CMVFlatFile>(sourceFile, outputFile);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void UniversalShipment_to_CMVFlatFile_Test_BlanketChange_Y()
        {
            TestMap("UniversalShipment_to_CMVFlatFileTests_input.input2017_BlanketChange_Y.xml", "UniversalShipment_to_CMVFlatFileTests_output.output2017_BlanketChange_Y.xml");
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void UniversalShipment_to_CMVFlatFile_Test_EmptyVesselDetails()
        {
            TestMap("UniversalShipment_to_CMVFlatFileTests_input.input2017_EmptyVesselDetails.xml", "UniversalShipment_to_CMVFlatFileTests_output.output2017_EmptyVesselDetails.xml");
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void UniversalShipment_to_CMVFlatFile_Test_Empty()
        {
            TestMap("UniversalShipment_to_CMVFlatFileTests_input.input2017_Empty.xml", "UniversalShipment_to_CMVFlatFileTests_output.output2017_Empty.xml");
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void UniversalShipment_to_CMVFlatFile_TestUserNamePassword()
        {
            InitialiseMessageTestingContext();
            InitialiseCodeMapsTestingContext();

            string sourceFile = "UniversalShipment_to_CMVFlatFileTests_input.input2017_UserNamePassword.xml";
            string outputFile = "UniversalShipment_to_CMVFlatFileTests_output.output2017_UserNamePassword.xml";

            var JPCustomsDataModelAccessor = new JPCustomsDataModelAccessor();
            var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
            JPCustomsDataModelAccessor.DataModelAccessor = mockDataModelAccessor;

            var clientSPRegistration = new eHubClientRegistration() { CX_Code = "ClientSPID", CX_Password1 = "ClientSPPassword" };
            mockDataModelAccessor.Expect(x => x.GetClientRegistration("HYEDAUVIZ", "JPCustomsAccount_ClientLevel")).Return(null).Repeat.Any();
            mockDataModelAccessor.Expect(x => x.GetClientSystemRegistration("HYEVIZ", null, "JPCustomsAccount_SystemLevel")).Return(null).Repeat.Twice();
            mockDataModelAccessor.Expect(x => x.GetClientRegistration("eHub", "JPCustomsAccount_ClientLevel")).Return(clientSPRegistration).Repeat.Any();


            var extensionObjects = new Dictionary<string, object>() { 
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", JPCustomsDataModelAccessor }
            };
            MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

            mapTester.Execute<UniversalShipment_to_CMVFlatFile>(sourceFile, outputFile);

            mockDataModelAccessor.VerifyAllExpectations();
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

            mapTester.Execute<UniversalShipment_to_CMVFlatFile>(sourceFile, outputFile);
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
                    "@UNLOCO", "", 
                    "@localtime", ""
                },
                Result = ""
            });

            ctx.ActionProcedures.Add(new ActionProcedure
            {
                Procedure = "CalculateTimeZoneOffset",
                OutputParm = "@offset",
                InputParms = new List<string> { 
                    "@UNLOCO", "BEANR", 
                    "@localtime", "2013-07-19T05:09:45.000"
                },
                Result = "+02:00"
            });

            ctx.ActionProcedures.Add(new ActionProcedure
            {
                Procedure = "CalculateTimeZoneOffset",
                OutputParm = "@offset",
                InputParms = new List<string> { 
                    "@UNLOCO", "GBLON", 
                    "@localtime", "2013-09-25T00:45:00"
                },
                Result = "+01:00"
            });

            ctx.ActionProcedures.Add(new ActionProcedure
            {
                Procedure = "CalculateTimeZoneOffset",
                OutputParm = "@offset",
                InputParms = new List<string> { 
                    "@UNLOCO", "AUSYD", 
                    "@localtime","2017-05-03T18:35:00"
                },
                Result = "+09:00"
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
                Result = "TSTID"
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
                Result = "TSTPW"
            });

            var ta = new TransformAccessor();
            ta.SetCodeMapsTestingContext(ctx);
        }
    }
}