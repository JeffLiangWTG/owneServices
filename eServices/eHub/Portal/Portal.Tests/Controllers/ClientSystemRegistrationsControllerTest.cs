using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using CargoWise.eHub.DataModel.Business;
using CargoWise.eHub.Portal.Controllers;
using CargoWise.eHub.Portal.Models.eHubTransactions;
using CargoWise.eHub.Portal.Tests.TestHelpers;
using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Portal.Tests.Controllers
{
	[TestClass]
	public class ClientSystemRegistrationsControllerTest : BaseControllerTest<ClientSystemRegistrationsController>
	{
		[TestMethod]
		public void TestIndex()
		{
			var result = controller.Index() as ViewResult;
			Assert.IsNotNull(result);
			Assert.AreEqual(string.Empty, result.ViewName, "Should be empty (Index)");
		}

		[TestMethod]
		public void TestRegistrationTypesGet()
		{
			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			controller.Context = context;
			context.eHubRegistrationTypes.AddObject(new eHubRegistrationType { RT_PK = new Guid("{00000000-EEEE-1111-1111-000000000000}"), RT_ID = "REG001", RT_Description = "Registration 1" });

			var result = controller.RegistrationTypes();

			Assert.IsNotNull(result);
			AssertEx.JsonResultMatchesList(new List<object> {
				{ new { RT_PK = new Guid("{00000000-EEEE-1111-1111-000000000000}"), RT_ID = "REG001", RT_Description = "Registration 1" } } ,
			}, result, "eHubRegistrationTypes", true);
		}

		[TestMethod]
		public void TestRegistrationTypeInfoGet()
		{
			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			controller.Context = context;
			context.eHubRegistrationTypes.AddObject(new eHubRegistrationType { RT_PK = new Guid("{00000000-EEEE-1111-1111-000000000000}"), RT_ID = "REG001", RT_Description = "Registration 1" });

			var result = controller.RegistrationTypeInfo(new Guid("{00000000-EEEE-1111-1111-000000000000}"));

			Assert.IsNotNull(result);
			var resultData = result.Data.GetType().GetProperty("eHubRegistrationType").GetValue(result.Data, null);
			AssertEx.PropertyValuesAreEquals(new { RT_PK = new Guid("{00000000-EEEE-1111-1111-000000000000}"), RT_ID = "REG001", RT_Description = "Registration 1" }, resultData, false);
		}

		[TestMethod]
		public void TestRegistrationTypeInfoEdit()
		{
            var logger = new TestLogger();
            var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			controller.Context = context;

			request.Clear();
			request.Container["RT_ID"] = "REG002";
			request.Container["RT_Description"] = "Registration 2";
			request.Container["oper"] = "add";

			RegistrationTypeInfoEditTest(logger);

			AssertEx.PropertyValuesAreEquals(new { RT_ID = "REG002", RT_Description = "Registration 2", RT_RegistrantType = "ClientSystem" }, context.eHubRegistrationTypes.Select(r => new { r.RT_ID, r.RT_Description, r.RT_RegistrantType }).FirstOrDefault(), false);
            Assert.IsTrue(logger.Log.Contains("Info - [CORP\\Test.Name] [add] eHubRegistrationType:"));
            Assert.IsTrue(logger.Log.Contains("RT_RegistrantType=ClientSystem, RT_ID=REG002, RT_Description=Registration 2"));
            var rtPK = context.eHubRegistrationTypes.First().RT_PK;

			request.Clear();
			request.Container["RT_PK"] = rtPK.ToString();
			request.Container["RT_ID"] = "REG002a";
			request.Container["RT_Description"] = "Registration 2a";
			request.Container["oper"] = "edit";

            RegistrationTypeInfoEditTest(logger);

            AssertEx.PropertyValuesAreEquals(new { RT_PK = rtPK, RT_ID = "REG002a", RT_Description = "Registration 2a" }, context.eHubRegistrationTypes.First(r => r.RT_PK == rtPK), false);
            Assert.IsTrue(logger.Log.Contains("Info - [CORP\\Test.Name] [edit] eHubRegistrationType:"));
            Assert.IsTrue(logger.Log.Contains("RT_RegistrantType=ClientSystem, RT_ID=REG002a, RT_Description=Registration 2a"));

            var registrationType = context.eHubRegistrationTypes.First();
			var clientSystem = new eHubClientSystem { EH_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), EH_ID = "CLI001" };
			var clientSystemRegistration = new eHubClientSystemRegistration { CD_PK = new Guid("{00000000-FFFF-1111-1111-000000000000}"), eHubClientSystem = clientSystem, eHubRegistrationType = registrationType, CD_Code = "REG001", CD_Attr1 = "username01", CD_Attr2 = "password01", CD_Flag1 = 1 };
			var client = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), CC_ID = "CLIENT001" };
			var clientRegistration = new eHubClientRegistration { CX_PK = new Guid("{00000000-FFFF-1111-1111-000000000000}"), eHubClient = client, eHubRegistrationType = registrationType, CX_Code = "REG001" };
			context.eHubClientSystems.AddObject(clientSystem);
			context.eHubClientSystemRegistrations.AddObject(clientSystemRegistration);
			context.eHubClients.AddObject(client);
			context.eHubClientRegistrations.AddObject(clientRegistration);
			context.SaveChanges();

			Assert.IsNotNull(context.eHubRegistrationTypes.FirstOrDefault(r => r.RT_PK == rtPK), "Precondition");
			Assert.IsNotNull(context.eHubClientSystemRegistrations.FirstOrDefault(r => r.CD_PK == clientSystemRegistration.CD_PK), "Precondition");
			Assert.IsNotNull(context.eHubClientRegistrations.FirstOrDefault(r => r.CX_PK == clientRegistration.CX_PK), "Precondition");

			request.Clear();
			request.Container["RT_PK"] = rtPK.ToString();
			request.Container["oper"] = "del";

            RegistrationTypeInfoEditTest(logger);

            Assert.IsNull(context.eHubRegistrationTypes.FirstOrDefault(r => r.RT_PK == rtPK));
			Assert.IsNull(context.eHubClientSystemRegistrations.FirstOrDefault(r => r.CD_PK == clientSystemRegistration.CD_PK));
			Assert.IsNull(context.eHubClientRegistrations.FirstOrDefault(r => r.CX_PK == clientRegistration.CX_PK));
            Assert.IsTrue(logger.Log.Contains("Info - [CORP\\Test.Name] [del] eHubRegistrationType:"));
            Assert.IsTrue(logger.Log.Contains("RT_RegistrantType=ClientSystem, RT_ID=REG002a, RT_Description=Registration 2a"));
            Assert.IsTrue(logger.Log.Contains("Info - [CORP\\Test.Name] [del] eHubClientSystemRegistration:"));
            Assert.IsTrue(logger.Log.Contains("CD_Code=REG001, CD_Attr1=username01, CD_Attr2=password01, CD_Flag1=1"));
            Assert.IsTrue(logger.Log.Contains("Info - [CORP\\Test.Name] [del] eHubClientRegistration:"));
            Assert.IsTrue(logger.Log.Contains("CX_Code=REG001, CX_Attr1=, CX_Password1=, CX_Flag1=, CX_Flag2="));

		}

		[TestMethod]
		public void TestClientSystemRegistrationsGet()
		{
			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["sidx"] = "CX_CC_ID";
			request.Container["sord"] = "asc";

			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			controller.Context = context;
			var cdPk1 = new Guid("{00000000-FFFF-1111-1111-000000000000}");
			var cdPk2 = new Guid("{00000000-FFFF-1111-2222-000000000000}");
			var rt = new eHubRegistrationType { RT_PK = new Guid("{00000000-EEEE-1111-1111-000000000000}"), RT_ID = "REG001", RT_Description = "Registration 1" };
			var cs1 = new eHubClientSystem { EH_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), EH_ID = "CLI001" };
			var cs2 = new eHubClientSystem { EH_PK = new Guid("{00000000-CCCC-1111-2222-000000000000}"), EH_ID = "CLI002" };
            var cx1 = new eHubClientSystemRegistration { CD_PK = cdPk1, eHubClientSystem = cs1, CD_RT = rt.RT_PK, CD_Code = "REG001", CD_Attr1 = "username01", CD_Attr2 = "password01", CD_Flag1 = (byte)1, CD_IssuedUTC = Convert.ToDateTime("2019-01-01 01:00:00"), CD_ExpiryUTC = Convert.ToDateTime("2019-01-01 01:10:00"), CD_ConfigXml = "<test>1</test>"};
            var cx2 = new eHubClientSystemRegistration { CD_PK = cdPk2, eHubClientSystem = cs2, CD_RT = rt.RT_PK, CD_Code = "REG002", CD_Attr1 = "username02", CD_Attr2 = "password02", CD_Flag1 = (byte)2, CD_IssuedUTC = Convert.ToDateTime("2019-01-02 01:00:00"), CD_ExpiryUTC = Convert.ToDateTime("2019-01-02 01:10:00") };
			context.eHubRegistrationTypes.AddObject(rt);
			context.eHubClientSystemRegistrations.AddObject(cx1);
			context.eHubClientSystemRegistrations.AddObject(cx2);

			var result = controller.ClientSystemRegistrations(rt.RT_PK);

			Assert.IsNotNull(result);

            AssertEx.JsonResultMatchesList(new List<object> {
				new {CD_PK = new Guid("{00000000-FFFF-1111-1111-000000000000}"), CD_EH_ID = "CLI001", CD_Code = "REG001", CD_Attr1 = "username01", CD_Attr2 = "password01", CD_Flag1 = (byte)1, CD_IssuedUTC = "2019-01-01 01:00:00", CD_ExpiryUTC = "2019-01-01 01:10:00", CD_ConfigXml = cdPk1},
                new {CD_PK = new Guid("{00000000-FFFF-1111-2222-000000000000}"), CD_EH_ID = "CLI002", CD_Code = "REG002", CD_Attr1 = "username02", CD_Attr2 = "password02", CD_Flag1 = (byte)2, CD_IssuedUTC = "2019-01-02 01:00:00", CD_ExpiryUTC = "2019-01-02 01:10:00", CD_ConfigXml = cdPk2}
			}, result, "eHubClientSystemRegistrations");
		}

		[TestMethod]
		public void TestClientSystemRegistrationStatusListGet()
		{
			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			controller.Context = context;

			var rt = new eHubRegistrationType { RT_PK = new Guid("{00000000-EEEE-1111-1111-000000000000}"), RT_ID = "REG001", RT_Description = "Registration 1" };
			ClientSystemRegistrationStatusFactory.GetClientSystemRegistrationStatus(rt.RT_ID).Add(0, "Valid");
			ClientSystemRegistrationStatusFactory.GetClientSystemRegistrationStatus(rt.RT_ID).Add(1, "InValid");
			context.eHubRegistrationTypes.AddObject(rt);
		
			Assert.AreEqual("0:Valid;1:InValid", controller.GetStatusDescriptionList(rt.RT_PK));

			var rt1 = new eHubRegistrationType { RT_PK = new Guid("{00000000-EEEE-1111-2222-000000000000}"), RT_ID = "REG002", RT_Description = "Registration 2" };
			context.eHubRegistrationTypes.AddObject(rt1);

			Assert.AreEqual(":Not Applicable", controller.GetStatusDescriptionList(rt1.RT_PK));
		}

		[TestMethod]
		public void TestClientSystemRegistraction_MultipleFilter_AND()
		{
			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["filters"] = "{\"groupOp\":\"AND\",\"rules\":[{\"field\":\"CD_Code\",\"op\":\"bw\",\"data\":\"REG\"},{\"field\":\"CD_Attr1\",\"op\":\"bw\",\"data\":\"username01\"}]}";

			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			controller.Context = context;
			var cdPk1 = new Guid("{00000000-FFFF-1111-1111-000000000000}");
			var cdPk2 = new Guid("{00000000-FFFF-1111-2222-000000000000}");
			var rt = new eHubRegistrationType { RT_PK = new Guid("{00000000-EEEE-1111-1111-000000000000}"), RT_ID = "REG001", RT_Description = "Registration 1" };
			var cs1 = new eHubClientSystem { EH_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), EH_ID = "CLI001" };
			var cs2 = new eHubClientSystem { EH_PK = new Guid("{00000000-CCCC-1111-2222-000000000000}"), EH_ID = "CLI002" };
			var cx1 = new eHubClientSystemRegistration { CD_PK = cdPk1, eHubClientSystem = cs1, CD_RT = rt.RT_PK, CD_Code = "REG001", CD_Attr1 = "username01", CD_Attr2 = "password01", CD_Flag1 = (byte)1, CD_IssuedUTC = Convert.ToDateTime("2019-01-01 01:00:00"), CD_ExpiryUTC = Convert.ToDateTime("2019-01-01 01:10:00"), CD_ConfigXml = "<test>1</test>" };
			var cx2 = new eHubClientSystemRegistration { CD_PK = cdPk2, eHubClientSystem = cs2, CD_RT = rt.RT_PK, CD_Code = "REG002", CD_Attr1 = "username02", CD_Attr2 = "password02", CD_Flag1 = (byte)2, CD_IssuedUTC = Convert.ToDateTime("2019-01-02 01:00:00"), CD_ExpiryUTC = Convert.ToDateTime("2019-01-02 01:10:00") };

			context.eHubRegistrationTypes.AddObject(rt);
			context.eHubClientSystemRegistrations.AddObject(cx1);
			context.eHubClientSystemRegistrations.AddObject(cx2);

			var result = controller.ClientSystemRegistrations(rt.RT_PK);

			Assert.IsNotNull(result);
			AssertEx.JsonResultMatchesList(new List<object> {
				new { CD_PK = new Guid("{00000000-FFFF-1111-1111-000000000000}"), CD_EH_ID = "CLI001", CD_Code = "REG001", CD_Attr1 = "username01", CD_Attr2 = "password01", CD_Flag1 = (byte)1, CD_IssuedUTC = "2019-01-01 01:00:00", CD_ExpiryUTC = "2019-01-01 01:10:00", CD_ConfigXml = cdPk1},
			}, result, "eHubClientSystemRegistrations");
		}

		[TestMethod]
		public void TestClientSystemRegistraction_MultipleFilter_OR()
		{
			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["filters"] = "{\"groupOp\":\"OR\",\"rules\":[{\"field\":\"CD_Code\",\"op\":\"bw\",\"data\":\"REG001\"},{\"field\":\"CD_Attr1\",\"op\":\"bw\",\"data\":\"username02\"}]}";

			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			controller.Context = context;
			var cdPk1 = new Guid("{00000000-FFFF-1111-1111-000000000000}");
			var cdPk2 = new Guid("{00000000-FFFF-1111-2222-000000000000}");
			var rt = new eHubRegistrationType { RT_PK = new Guid("{00000000-EEEE-1111-1111-000000000000}"), RT_ID = "REG001", RT_Description = "Registration 1" };
			var cs1 = new eHubClientSystem { EH_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), EH_ID = "CLI001" };
			var cs2 = new eHubClientSystem { EH_PK = new Guid("{00000000-CCCC-1111-2222-000000000000}"), EH_ID = "CLI002" };
			var cx1 = new eHubClientSystemRegistration { CD_PK = cdPk1, eHubClientSystem = cs1, CD_RT = rt.RT_PK, CD_Code = "REG001", CD_Attr1 = "username01", CD_Attr2 = "password01", CD_Flag1 = (byte)1, CD_IssuedUTC = Convert.ToDateTime("2019-01-01 01:00:00"), CD_ExpiryUTC = Convert.ToDateTime("2019-01-01 01:10:00"), CD_ConfigXml = "<test>1</test>" };
			var cx2 = new eHubClientSystemRegistration { CD_PK = cdPk2, eHubClientSystem = cs2, CD_RT = rt.RT_PK, CD_Code = "REG002", CD_Attr1 = "username02", CD_Attr2 = "password02", CD_Flag1 = (byte)2, CD_IssuedUTC = Convert.ToDateTime("2019-01-02 01:00:00"), CD_ExpiryUTC = Convert.ToDateTime("2019-01-02 01:10:00") };

			context.eHubRegistrationTypes.AddObject(rt);
			context.eHubClientSystemRegistrations.AddObject(cx1);
			context.eHubClientSystemRegistrations.AddObject(cx2);

			var result = controller.ClientSystemRegistrations(rt.RT_PK);

			Assert.IsNotNull(result);
			AssertEx.JsonResultMatchesList(new List<object> {
				new {CD_PK = new Guid("{00000000-FFFF-1111-1111-000000000000}"), CD_EH_ID = "CLI001", CD_Code = "REG001", CD_Attr1 = "username01", CD_Attr2 = "password01", CD_Flag1 = (byte)1, CD_IssuedUTC = "2019-01-01 01:00:00", CD_ExpiryUTC = "2019-01-01 01:10:00", CD_ConfigXml = cdPk1},
				new {CD_PK = new Guid("{00000000-FFFF-1111-2222-000000000000}"), CD_EH_ID = "CLI002", CD_Code = "REG002", CD_Attr1 = "username02", CD_Attr2 = "password02", CD_Flag1 = (byte)2, CD_IssuedUTC = "2019-01-02 01:00:00", CD_ExpiryUTC = "2019-01-02 01:10:00", CD_ConfigXml = cdPk2}
			}, result, "eHubClientSystemRegistrations");
		}

		[TestMethod]
		public void TestRegistrationsEdit()
		{
            var logger = new TestLogger();
            var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			controller.Context = context;
			var rt = new eHubRegistrationType { RT_PK = new Guid("{00000000-EEEE-1111-1111-000000000000}"), RT_ID = "REG001", RT_Description = "Registration 1" };
			var cs = new eHubClientSystem { EH_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), EH_ID = "CLI001" };
			context.eHubRegistrationTypes.AddObject(rt);
			context.eHubClientSystems.AddObject(cs);

			request.Clear();
			request.Container["id"] = "_empty";
			request.Container["CD_EH_ID"] = "CLI001";
			request.Container["CD_Code"] = "REG001";
			request.Container["CD_Attr1"] = "username01";
			request.Container["CD_Attr2"] = "password01";
			request.Container["CD_Flag1"] = "1";
			request.Container["oper"] = "add";

            var responseAdd = ClientSystemRegistrationsEditTest(rt.RT_PK, logger);

			var resultState = (bool)responseAdd.Data.GetType().GetProperty("success").GetValue(responseAdd.Data, null);
			Assert.IsTrue(resultState, "add failure");
            Assert.IsTrue(logger.Log.Contains("Info - [CORP\\Test.Name] [add] eHubClientSystemRegistration:"));
            Assert.IsTrue(logger.Log.Contains("CD_Qualifier=, CD_Code=REG001, CD_Attr1=username01, CD_Attr2=password01, CD_Flag1=1, CD_ExpiryUTC=, CD_IssuedUTC=, CD_RT=00000000-eeee-1111-1111-000000000000, CD_EH=00000000-cccc-1111-1111-000000000000"));
            var newId = (Guid)responseAdd.Data.GetType().GetProperty("id").GetValue(responseAdd.Data, null);
			
			CollectionAssert.AreEqual(new[] { new Tuple<Guid, Guid, Guid, String, String, String, byte?>(newId, cs.EH_PK, rt.RT_PK, "REG001", "username01", "password01", (byte)1) },
				GetEHubClientRegistrationContent(context));

			request.Clear();
			request.Container["CD_PK"] = newId.ToString();
			request.Container["CD_EH_ID"] = "CLI001";
			request.Container["CD_Code"] = "REG002";
			request.Container["CD_Attr1"] = "username02";
			request.Container["CD_Attr2"] = "password02";
			request.Container["CD_Flag1"] = "2";
			request.Container["oper"] = "edit";

			var responseEdit = ClientSystemRegistrationsEditTest(rt.RT_PK, logger);

            Assert.IsTrue((bool)responseEdit.Data.GetType().GetProperty("success").GetValue(responseEdit.Data, null), "edit failure");
            Assert.IsTrue(logger.Log.Contains("Info - [CORP\\Test.Name] [edit] eHubClientSystemRegistration:"));
            Assert.IsTrue(logger.Log.Contains("CD_Qualifier=, CD_Code=REG002, CD_Attr1=username02, CD_Attr2=password02, CD_Flag1=2, CD_ExpiryUTC=, CD_IssuedUTC=, CD_RT=00000000-eeee-1111-1111-000000000000, CD_EH=00000000-cccc-1111-1111-000000000000"));

            CollectionAssert.AreEqual(new[] { new Tuple<Guid, Guid, Guid, String, String, String, byte?>(newId, cs.EH_PK, rt.RT_PK, "REG002", "username02", "password02", 2) },
				GetEHubClientRegistrationContent(context));

			request.Clear();
			request.Container["CD_PK"] = newId.ToString();
			request.Container["oper"] = "del";

			var responseDel = ClientSystemRegistrationsEditTest(rt.RT_PK, logger);

            Assert.IsTrue((bool)responseDel.Data.GetType().GetProperty("success").GetValue(responseDel.Data, null), "del failure");
            Assert.IsTrue(logger.Log.Contains("Info - [CORP\\Test.Name] [del] eHubClientSystemRegistration:"));
            Assert.IsTrue(logger.Log.Contains("CD_Qualifier=, CD_Code=REG002, CD_Attr1=username02, CD_Attr2=password02, CD_Flag1=2, CD_ExpiryUTC=, CD_IssuedUTC=, CD_RT=00000000-eeee-1111-1111-000000000000, CD_EH=00000000-cccc-1111-1111-000000000000"));
            Assert.AreEqual(0, context.eHubClientRegistrations.Count());
		}

		[TestMethod]
		public void TestRegistrationsEdit_Disabled()
		{
			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			controller.Context = context;
			var rt = new eHubRegistrationType { RT_PK = new Guid("{00000000-EEEE-1111-1111-000000000000}"), RT_ID = "REG001", RT_Description = "Registration 1" };
			var cs = new eHubClientSystem { EH_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), EH_ID = "CLI001" };
			context.eHubRegistrationTypes.AddObject(rt);
			context.eHubClientSystems.AddObject(cs);

			request.Clear();
			request.Container["id"] = "_empty";
			request.Container["CD_EH_ID"] = "CLI001";
			request.Container["CD_Code"] = "REG001";
			request.Container["CD_Attr1"] = "username01";
			request.Container["CD_Attr2"] = "password01";
			request.Container["CD_Flag1"] = "1";
			request.Container["oper"] = "add";

			var responseAdd = controller.ClientSystemRegistrationsEdit(rt.RT_PK);

			var resultState = (bool)responseAdd.Data.GetType().GetProperty("success").GetValue(responseAdd.Data, null);
            Assert.IsTrue(resultState, "add failure");
			var newId = (Guid)responseAdd.Data.GetType().GetProperty("id").GetValue(responseAdd.Data, null);
			CollectionAssert.AreEqual(new[] { new Tuple<Guid, Guid, Guid, String, String, String, byte?>(newId, cs.EH_PK, rt.RT_PK, "REG001", "username01", "password01", 1) },
				GetEHubClientRegistrationContent(context));

			request.Clear();
			request.Container["CD_PK"] = newId.ToString();
			request.Container["CD_EH_ID"] = "CLI001";
			request.Container["CD_Code"] = "REG002";
			request.Container["CD_Attr1"] = "username02";
			request.Container["CD_Attr2"] = "password02";
			request.Container["CD_Flag1"] = "2";
			request.Container["oper"] = "edit";

			var responseEdit = controller.ClientSystemRegistrationsEdit(rt.RT_PK);

            Assert.IsTrue((bool)responseEdit.Data.GetType().GetProperty("success").GetValue(responseEdit.Data, null), "dit failure");
			CollectionAssert.AreEqual(new[] { new Tuple<Guid, Guid, Guid, String, String, String, byte?>(newId, cs.EH_PK, rt.RT_PK, "REG002", "username02", "password02", 2) },
				GetEHubClientRegistrationContent(context));

			request.Clear();
			request.Container["CD_PK"] = newId.ToString();
			request.Container["oper"] = "del";

			var responseDel = controller.ClientSystemRegistrationsEdit(rt.RT_PK);

            Assert.IsTrue((bool)responseDel.Data.GetType().GetProperty("success").GetValue(responseDel.Data, null), "del failure");
			Assert.AreEqual(0, context.eHubClientRegistrations.Count());
		}

		Tuple<Guid, Guid, Guid, String, String, String, byte?>[] GetEHubClientRegistrationContent(Fakes.TestContext context)
		{
			return context.eHubClientSystemRegistrations.Select(cd => new Tuple<Guid, Guid, Guid, String, String, String, byte?>(cd.CD_PK, cd.CD_EH, cd.CD_RT, cd.CD_Code, cd.CD_Attr1, cd.CD_Attr2, cd.CD_Flag1)).ToArray();
		}

		[TestMethod]
		public void TestClientSystemsGetIncludeNonProdCWSystems()
		{
			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			controller.Context = context;
			context.eHubClientSystems.AddObject(new eHubClientSystem { EH_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), EH_ID = "CLI001", EH_URL = "url01.com" });
			context.eHubClientSystems.AddObject(new eHubClientSystem { EH_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), EH_ID = "CLI002", EH_URL = "url02.com" });
			context.eHubClientSystems.AddObject(new eHubClientSystem { EH_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), EH_ID = "CLI003", EH_URL = "" });
			context.eHubClientSystems.AddObject(new eHubClientSystem { EH_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), EH_ID = "CLI004", EH_URL = "url04.com" });
			context.eHubClientSystems.AddObject(new eHubClientSystem { EH_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), EH_ID = "CLI005", EH_URL = "url22.com" });

			context.ediProdClients.AddObject(new ediProdClient { CC_ID = "CLIPRD004", CC_PK = new Guid("{10000000-CCCC-1111-1111-000000000000}"), EnterpriseServerCode = "CLI004", LE_EnterpriseCode = "CLI", LD_LicenceType = "PRD", LD_ServerCode = "004" });
			context.ediProdClients.AddObject(new ediProdClient { CC_ID = "CLIPRD005", CC_PK = new Guid("{20000000-CCCC-1111-1111-000000000000}"), EnterpriseServerCode = "CLI005", LE_EnterpriseCode = "CLI", LD_LicenceType = "TST", LD_ServerCode = "005" });

			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["sidx"] = "EH_ID";
			request.Container["sord"] = "asc";
			request.Container["_search"] = "false";
			IList<object> expected1 = new List<object>
		    {
		        new { EH_ID = "CLI001", EH_URL = "url01.com" },
		        new { EH_ID = "CLI002", EH_URL = "url02.com" },
		        new { EH_ID = "CLI003", EH_URL = "" },
				new { EH_ID = "CLI004", EH_URL = "url04.com" },
				new { EH_ID = "CLI005", EH_URL = "url22.com" }
			};

			var result1 = controller.ClientSystems(true);

			Assert.IsNotNull(result1);
			AssertEx.JsonResultMatchesList(expected1, result1, "eHubClientSystems");

			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["sidx"] = "EH_URL";
			request.Container["sord"] = "desc";
			request.Container["_search"] = "true";
			request.Container["EH_URL"] = "url";
			IList<object> expected2 = new List<object>
		    {
			    new { EH_ID = "CLI005", EH_URL = "url22.com" },
				new { EH_ID = "CLI004", EH_URL = "url04.com" },
				new { EH_ID = "CLI002", EH_URL = "url02.com" },
		        new { EH_ID = "CLI001", EH_URL = "url01.com" }
			};

			var result2 = controller.ClientSystems(true);

			Assert.IsNotNull(result2);
			AssertEx.JsonResultMatchesList(expected2, result2, "eHubClientSystems");

			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["sidx"] = "EH_ID";
			request.Container["sord"] = "asc";
			request.Container["_search"] = "true";
			request.Container["EH_ID"] = "CLI00";
			request.Container["EH_URL"] = "2";
			IList<object> expected3 = new List<object>
		    {
		        new { EH_ID = "CLI002", EH_URL = "url02.com" },
		        new { EH_ID = "CLI005", EH_URL = "url22.com" }
			};

			var result3 = controller.ClientSystems(true);

			Assert.IsNotNull(result2);
			AssertEx.JsonResultMatchesList(expected3, result3, "eHubClientSystems");
		}

		[TestMethod]
		public void TestClientSystemsGetExcludeNonProdCWSystems()
		{
			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			controller.Context = context;
			context.eHubClientSystems.AddObject(new eHubClientSystem { EH_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), EH_ID = "CLI001", EH_URL = "url01.com" });
			context.eHubClientSystems.AddObject(new eHubClientSystem { EH_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), EH_ID = "CLI002", EH_URL = "url02.com" });
			context.eHubClientSystems.AddObject(new eHubClientSystem { EH_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), EH_ID = "CLI003", EH_URL = "" });
			context.eHubClientSystems.AddObject(new eHubClientSystem { EH_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), EH_ID = "CLI004", EH_URL = "url04.com" });
			context.eHubClientSystems.AddObject(new eHubClientSystem { EH_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), EH_ID = "CLI005", EH_URL = "url05.com" });

			context.ediProdClients.AddObject(new ediProdClient { CC_ID = "CLIPR1004", CC_PK = new Guid("{10000000-CCCC-1111-1111-000000000000}"), EnterpriseServerCode = "CLI004", LE_EnterpriseCode = "CLI", LD_LicenceType = "PRD", LD_ServerCode = "004" });
			context.ediProdClients.AddObject(new ediProdClient { CC_ID = "CLIPR2004", CC_PK = new Guid("{10000000-CCCC-1111-1111-000000000000}"), EnterpriseServerCode = "CLI004", LE_EnterpriseCode = "CLI", LD_LicenceType = "PRD", LD_ServerCode = "004" });
			context.ediProdClients.AddObject(new ediProdClient { CC_ID = "CLIPR3004", CC_PK = new Guid("{10000000-CCCC-1111-1111-000000000000}"), EnterpriseServerCode = "CLI004", LE_EnterpriseCode = "CLI", LD_LicenceType = "PRD", LD_ServerCode = "004" });
			context.ediProdClients.AddObject(new ediProdClient { CC_ID = "CLIPRD005", CC_PK = new Guid("{20000000-CCCC-1111-1111-000000000000}"), EnterpriseServerCode = "CLI005", LE_EnterpriseCode = "CLI", LD_LicenceType = "TST", LD_ServerCode = "005" });

			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["sidx"] = "EH_ID";
			request.Container["sord"] = "asc";
			request.Container["_search"] = "false";
			IList<object> expected1 = new List<object>
			{
				new { EH_ID = "CLI004", EH_URL = "url04.com" }
			};

			var result1 = controller.ClientSystems(false);

			Assert.IsNotNull(result1);
			AssertEx.JsonResultMatchesList(expected1, result1, "eHubClientSystems");

			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["sidx"] = "EH_URL";
			request.Container["sord"] = "desc";
			request.Container["_search"] = "true";
			request.Container["EH_URL"] = "url";
			IList<object> expected2 = new List<object>
			{
				new { EH_ID = "CLI004", EH_URL = "url04.com" }
			};

			var result2 = controller.ClientSystems(false);

			Assert.IsNotNull(result2);
			AssertEx.JsonResultMatchesList(expected2, result2, "eHubClientSystems");

			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["sidx"] = "EH_ID";
			request.Container["sord"] = "asc";
			request.Container["_search"] = "true";
			request.Container["EH_ID"] = "CLI00";
			request.Container["EH_URL"] = "4";
			IList<object> expected3 = new List<object>
			{
				new { EH_ID = "CLI004", EH_URL = "url04.com" }
			};

			var result3 = controller.ClientSystems(false);

			Assert.IsNotNull(result2);
			AssertEx.JsonResultMatchesList(expected3, result3, "eHubClientSystems");
		}

		[TestMethod]
		public void TestClientSystemsFilterCaseInsensitive()
		{
			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			controller.Context = context;
			context.eHubClientSystems.AddObject(new eHubClientSystem { EH_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), EH_ID = "CLI001", EH_URL = "url01.com" });
			context.eHubClientSystems.AddObject(new eHubClientSystem { EH_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), EH_ID = "CLI002", EH_URL = "url02.com" });
			context.eHubClientSystems.AddObject(new eHubClientSystem { EH_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), EH_ID = "CLI003", EH_URL = "" });
			context.eHubClientSystems.AddObject(new eHubClientSystem { EH_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), EH_ID = "CLI004", EH_URL = "url04.com" });
			context.eHubClientSystems.AddObject(new eHubClientSystem { EH_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), EH_ID = "CLI005", EH_URL = "url05.com" });
			context.ediProdClients.AddObject(new ediProdClient { CC_ID = "CLIPRD004", CC_PK = new Guid("{10000000-CCCC-1111-1111-000000000000}"), EnterpriseServerCode = "CLI004", LE_EnterpriseCode = "CLI", LD_LicenceType = "PRD", LD_ServerCode = "004" });
			context.ediProdClients.AddObject(new ediProdClient { CC_ID = "CLIPRD005", CC_PK = new Guid("{20000000-CCCC-1111-1111-000000000000}"), EnterpriseServerCode = "CLI005", LE_EnterpriseCode = "CLI", LD_LicenceType = "TST", LD_ServerCode = "005" });
			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["sidx"] = "EH_ID";
			request.Container["sord"] = "asc";
			request.Container["_search"] = "true";
			request.Container["EH_ID"] = "cli00";
			request.Container["EH_URL"] = "4";
			IList<object> expected = new List<object>
			{
				new { EH_ID = "CLI004", EH_URL = "url04.com" }
			};
			var result = controller.ClientSystems(false);
			Assert.IsNotNull(result);
			AssertEx.JsonResultMatchesList(expected, result, "eHubClientSystems");
		}

		[TestMethod]
        public void TesteHubPortalSemanticsFactory_GBCustomsAuthorisationToken()
	    {
            var expectedColumnNames = new Dictionary<string, string>
	        {
	            {"CD_EH_ID", "Client System"},
	            {"CD_Code", "EORI Badge"},
	            {"CD_Attr1", "Authorisation Token"},
	            {"CD_Attr2", "Redirect URI"},
	            {"CD_Flag1", "Token State"},
	            {"CD_IssuedUTC", "Issued UTC" },
	            {"CD_ExpiryUTC", "Expiry UTC"}
	        };

            TesteHubPortalSemanticsFactory("GBCustomsAuthorisationToken", expectedColumnNames);
	    }

	    [TestMethod]
        public void TesteHubPortalSemanticsFactory_GBCustomMCPAccount()
	    {
	        var expectedColumnNames = new Dictionary<string, string>
	        {
	            {"CD_EH_ID", "Client System"},
	            {"CD_Code", "Badge"},
	            {"CD_Attr1", "Topic"},
                {"CD_Flag1", "Token State"}
            };

            TesteHubPortalSemanticsFactory("GBCustoms-MCPAccount", expectedColumnNames);
	    }

        [TestMethod]
        public void TesteHubPortalSemanticsFactory_ACAS_BR_Token()
        {
	        var expectedColumnNames = new Dictionary<string, string>
	        {
				{"CD_EH_ID", "Client System"},
				{"CD_Code", "Code"},
				{"CD_Attr1", "Attribute 1"},
				{"CD_Attr2", "Attribute 2"},
				{"CD_Flag1", "Flag 1"}
			};

	        TesteHubPortalSemanticsFactory("ACAS_BRToken", expectedColumnNames);
        }

        [TestMethod]
        public void TesteHubPortalSemanticsFactory_GEI_IN_AuthenticationSystemLevel()
        {
	        var expectedColumnNames = new Dictionary<string, string>
	        {
		        {"CD_EH_ID", "Client System"},
		        {"CD_ConfigXml", "ConfigXml"}
			};

	        TesteHubPortalSemanticsFactory("GEI_IN_AuthenticationSystemLevel", expectedColumnNames);
		}

		[TestMethod]
		public void TesteHubPortalSemanticsFactory_GBCustoms_ICSNI()
		{
			var expectedColumnNames = new Dictionary<string, string>
			{
				{"CD_EH_ID", "Client System"},
				{"CD_Code", "Declarant"},
				{"CD_Flag1", "Status"},
				{"CD_Attr1", "Active Subscription"},
				{"CD_IssuedUTC", "Issued UTC"},
				{"CD_ConfigXml", "Config Xml"}
			};

			TesteHubPortalSemanticsFactory("GBCustoms-ICSNI", expectedColumnNames);
		}

		[TestMethod]
		public void TestDownloadConfigFile()
		{

			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			controller.Context = context;
			var rt = new eHubRegistrationType { RT_PK = new Guid("{00000000-EEEE-1111-1111-000000000000}"), RT_ID = "REG001", RT_Description = "Registration 1" };
			var eh1 = new eHubClientSystem() { EH_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), EH_ID = "CLS001" };
			var cd1 = new eHubClientSystemRegistration
			{
				CD_PK = new Guid("{00000000-FFFF-1111-1111-000000000000}"),
				eHubClientSystem = eh1,
				CD_RT = rt.RT_PK,
				CD_ConfigXml = "<TESTConfiguration></TESTConfiguration>"
			};

			context.eHubRegistrationTypes.AddObject(rt);
			context.eHubClientSystemRegistrations.AddObject(cd1);

			request.Clear();
			request.Container["CD_PK"] = "00000000-FFFF-1111-1111-000000000000";

			var result = controller.DownloadConfiguration(cd1.CD_PK);

			Assert.AreEqual("<TESTConfiguration></TESTConfiguration>", Encoding.Default.GetString(result.FileContents));
			Assert.AreEqual("00000000-ffff-1111-1111-000000000000.xml", result.FileDownloadName);
		}

		void TesteHubPortalSemanticsFactory(string registrationTypeId, Dictionary<string, string> expectedColumnNames)
	    {
	        var context = new Fakes.TestContext();
	        controller.Context = context;

	        var resultDict = new Dictionary<string, string>();
	        foreach (var key in expectedColumnNames.Keys)
	        {
	            resultDict[key] = controller.GetCustomisedHeaderForID(registrationTypeId, key);
	        }

	        CollectionAssert.AreEquivalent(resultDict.ToList(), expectedColumnNames.ToList());
	    }

        protected void RegistrationTypeInfoEditTest(ILog logger)
        {
            controller.logger = logger;
            controller.RegistrationTypeInfoEdit();
        }

        protected JsonResult ClientSystemRegistrationsEditTest(Guid regType, ILog logger)
        {
            controller.logger = logger;
            return controller.ClientSystemRegistrationsEdit(regType);
        }
    }
}
