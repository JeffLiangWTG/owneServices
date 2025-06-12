using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Core.Transforms.Helper.Testing;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eHub.Products.JPCustoms.Transforms.Helper;
using CargoWise.eHub.Products.JPCustoms.Transforms.UniversalShipment2AHRFlatFile2017;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.JPCustoms.Tests
{
	[TestClass]
	public class UniversalShipment_to_AHRFlatFile2017Tests
	{
        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void UniversalShipment_to_AHRFlatFile_TestUserNamePassword()
        {
            InitialiseMessageTestingContext();
            InitialiseCodeMapsTestingContext();

            string sourceFile = "UniversalShipment_to_AHRFlatFileTests_input.input2017_UserNamePassword.xml";
            string outputFile = "UniversalShipment_to_AHRFlatFileTests_output.output2017_UserNamePassword.xml";


            var JPCustomsDataModelAccessor = new JPCustomsDataModelAccessor();
            var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
            JPCustomsDataModelAccessor.DataModelAccessor = mockDataModelAccessor;

            var clientSPRegistration = new eHubClientRegistration() { CX_Code = "ClientSPID", CX_Password1 = "ClientSPPassword" };
            mockDataModelAccessor.Expect(x => x.GetClientRegistration("HYEDAUVIZ", "JPCustomsAccount_ClientLevel")).Return(null).Repeat.Any();
            mockDataModelAccessor.Expect(x => x.GetClientSystemRegistration("HYEVIZ", null, "JPCustomsAccount_SystemLevel")).Return(null).Repeat.Any();
            mockDataModelAccessor.Expect(x => x.GetClientRegistration("eHub", "JPCustomsAccount_ClientLevel")).Return(clientSPRegistration).Repeat.Any();


            var extensionObjects = new Dictionary<string, object>() { 
                { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS3", JPCustomsDataModelAccessor }
            };
            MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

            mapTester.Execute<UniversalShipment_to_AHRFlatFile>(sourceFile, outputFile);

            mockDataModelAccessor.VerifyAllExpectations();
        }

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UniversalShipment_to_AHRFlatFile_TestDetailedDescription()
		{
			InitialiseMessageTestingContext();
			InitialiseCodeMapsTestingContext();

			string sourceFile = "UniversalShipment_to_AHRFlatFileTests_input.input2017_DetailedDescription.xml";
			string outputFile = "UniversalShipment_to_AHRFlatFileTests_output.output2017_DetailedDescription.xml";

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

			mapTester.Execute<UniversalShipment_to_AHRFlatFile>(sourceFile, outputFile);

			mockDataModelAccessor.VerifyAllExpectations();
		}

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void UniversalShipment_to_AHRFlatFile_Test1()
        {
            InitialiseMessageTestingContext();
            InitialiseCodeMapsTestingContext();

            string sourceFile = "UniversalShipment_to_AHRFlatFileTests_input.input2017_1.xml";
            string outputFile = "UniversalShipment_to_AHRFlatFileTests_output.output2017_1.xml";

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

            mapTester.Execute<UniversalShipment_to_AHRFlatFile>(sourceFile, outputFile);

            mockDataModelAccessor.VerifyAllExpectations();
        }

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UniversalShipment_to_AHRFlatFile_Test2()
		{
			TestMap("UniversalShipment_to_AHRFlatFileTests_input.input2017_2.xml", "UniversalShipment_to_AHRFlatFileTests_output.output2017_2.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UniversalShipment_to_AHRFlatFile_Test3()
		{
			TestMap("UniversalShipment_to_AHRFlatFileTests_input.input2017_3.xml", "UniversalShipment_to_AHRFlatFileTests_output.output2017_3.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UniversalShipment_to_AHRFlatFile_Test4()
		{
			TestMap("UniversalShipment_to_AHRFlatFileTests_input.input2017_4.xml", "UniversalShipment_to_AHRFlatFileTests_output.output2017_4.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UniversalShipment_to_AHRFlatFile_Test5()
		{
			TestMap("UniversalShipment_to_AHRFlatFileTests_input.input2017_5.xml", "UniversalShipment_to_AHRFlatFileTests_output.output2017_5.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UniversalShipment_to_AHRFlatFile_Test6()
		{
			TestMap("UniversalShipment_to_AHRFlatFileTests_input.input2017_6.xml", "UniversalShipment_to_AHRFlatFileTests_output.output2017_6.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UniversalShipment_to_AHRFlatFile_Test7()
		{
			TestMap("UniversalShipment_to_AHRFlatFileTests_input.input2017_7.xml", "UniversalShipment_to_AHRFlatFileTests_output.output2017_7.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UniversalShipment_to_AHRFlatFile_Test7_EmptyVesselCode()
		{
			TestMap("UniversalShipment_to_AHRFlatFileTests_input.input2017_7_EmptyVesselCode.xml", "UniversalShipment_to_AHRFlatFileTests_output.output2017_7_EmptyVesselCode.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UniversalShipment_to_AHRFlatFile_Test7_NormalVesselCode()
		{
			TestMap("UniversalShipment_to_AHRFlatFileTests_input.input2017_7_NormalVesselCode.xml", "UniversalShipment_to_AHRFlatFileTests_output.output2017_7_NormalVesselCode.xml");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UniversalShipment_to_AHRFlatFile_TestCompletion()
		{
			TestMap("UniversalShipment_to_AHRFlatFileTests_input.input2017_Completion.xml", "UniversalShipment_to_AHRFlatFileTests_output.output2017_Completion.xml");
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

			mapTester.Execute<UniversalShipment_to_AHRFlatFile>(sourceFile, outputFile);
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
					"@localtime","2013-10-17T08:17:00"
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