using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eHub.Products.JPCustoms.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.JPCustoms.Tests
{
	[TestClass]
	public class JPCustomsDataModelAccessorTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestGetUsernamePasswordFromClientSystemRegistration()
		{
			var JPCustomsDataModelAccessor = new JPCustomsDataModelAccessor();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
			JPCustomsDataModelAccessor.DataModelAccessor = mockDataModelAccessor;

			var registration = new eHubClientSystemRegistration() { CD_Code = "TestClientSystemUserName", CD_Attr1 = "TestClientSystemPassword" };
            mockDataModelAccessor.Stub(x => x.GetClientRegistration("HYEDAUVIZ", "JPCustomsAccount_ClientLevel")).Return(null);
            mockDataModelAccessor.Stub(x => x.GetClientSystemRegistration("HYEVIZ", null, "JPCustomsAccount_SystemLevel")).Return(registration);

			Assert.AreEqual("TestClientSystemUserName", JPCustomsDataModelAccessor.GetUsername("HYEDAUVIZ"));
			Assert.AreEqual("TestClientSystemPassword", JPCustomsDataModelAccessor.GetPassword("HYEDAUVIZ"));
		}

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestGetSPUsernamePasswordFromClientSystemRegistration()
        {
            InitialiseCodeMapsTestingContext();
            var JPCustomsDataModelAccessor = new JPCustomsDataModelAccessor();
            var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
            JPCustomsDataModelAccessor.DataModelAccessor = mockDataModelAccessor;

            mockDataModelAccessor.Stub(x => x.GetClientRegistration("eHub", "JPCustomsAccount_ClientLevel")).Return(null);

            Assert.AreEqual("SPID", JPCustomsDataModelAccessor.GetSPID());
            Assert.AreEqual("SPPW", JPCustomsDataModelAccessor.GetSPPassword());
        }


		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestGetUsernamePasswordFromeHubMessageReferneceRegistry()
		{
			InitialiseCodeMapsTestingContext();
			var JPCustomsDataModelAccessor = new JPCustomsDataModelAccessor();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
			JPCustomsDataModelAccessor.DataModelAccessor = mockDataModelAccessor;

            mockDataModelAccessor.Stub(x => x.GetClientRegistration("HYEDAUVIZ", "JPCustomsAccount_ClientLevel")).Return(null);
            mockDataModelAccessor.Stub(x => x.GetClientSystemRegistration("HYEVIZ", null, "JPCustomsAccount_SystemLevel")).Return(null);

            Assert.AreEqual("USRID", JPCustomsDataModelAccessor.GetUsername("HYEDAUVIZ"));
			Assert.AreEqual("USRPW", JPCustomsDataModelAccessor.GetPassword("HYEDAUVIZ"));
		}

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestGetUsernamePasswordFromClient()
        {
            InitialiseCodeMapsTestingContext();
            var JPCustomsDataModelAccessor = new JPCustomsDataModelAccessor();
            var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
            var clientRegistration = new eHubClientRegistration() { CX_Code = "USRID", CX_Password1 = "USRPW" };

            JPCustomsDataModelAccessor.DataModelAccessor = mockDataModelAccessor;

            mockDataModelAccessor.Stub(x => x.GetClientRegistration("HYEDAUVIZ", "JPCustomsAccount_ClientLevel")).Return(clientRegistration);

            Assert.AreEqual("USRID", JPCustomsDataModelAccessor.GetUsername("HYEDAUVIZ"));
            Assert.AreEqual("USRPW", JPCustomsDataModelAccessor.GetPassword("HYEDAUVIZ"));
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