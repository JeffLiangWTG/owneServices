using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CargoWise.eHub.Portal.Controllers;
using CargoWise.eHub.Portal.Models.eHubTransactions;
using CargoWise.eHub.Portal.Tests.TestHelpers;
using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Portal.Tests.Controllers
{
	[TestClass]
	public class ZACustomsControllerTest : BaseControllerTest<ZACustomsController>
	{
		[TestMethod]
		public void TestIndex()
		{
			var result = controller.Index() as ViewResult;
			Assert.IsNotNull(result);
			Assert.AreEqual(string.Empty, result.ViewName, "Should be empty (Index)");
		}

		void GenerateDefaultValues(Fakes.TestContext context)
		{
			var rt = new eHubRegistrationType { RT_PK = new Guid("{00000000-EEEE-1111-1111-000000000000}"), RT_ID = ZACustomsController.ZACustomsCode, RT_Description = "Registration 1" };
			var cc1 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), CC_ID = "CLIENT001" };
			var cc2 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-2222-000000000000}"), CC_ID = "CLIENT002" };
			var ZACustomsClient = new eHubClient { CC_PK = new Guid("{FA785A75-C381-49F5-B874-7211653054CF}"), CC_ID = "ZACustoms" };
			var cx1 = new eHubClientRegistration { CX_PK = new Guid("{00000000-FFFF-1111-1111-000000000000}"), eHubClient = cc1, CX_RT = rt.RT_PK, CX_Code = "REG001" };
			var cx2 = new eHubClientRegistration { CX_PK = new Guid("{00000000-FFFF-1111-2222-000000000000}"), eHubClient = cc2, CX_RT = rt.RT_PK, CX_Code = "REG002" };
			var ZACustomsTS = new eHubTransformationSet { TS_PK = new Guid("{78a449d9-d581-488f-86a1-8a7980a3e94f}"), eHubClient_Sender = ZACustomsClient, eHubClient_Recipient = ZACustomsClient };
			var cs = new eHubCodeSet { CS_PK = new Guid("{ac4fc9a6-854e-4c9e-b93d-e6bebcdc5736}"), CS_Name = "Agent Profile", eHubTransformationSet = ZACustomsTS, eHubClient_Sender = ZACustomsClient, eHubClient_Recipient = ZACustomsClient };
			var csr1 = new eHubCodeSetResult { CR_PK = new Guid("{83e4b535-3b86-4750-b018-4f3f573b8bcd}"), eHubCodeSet = cs, CR_Order = 1, CR_Name = "SenderID" };
			var csr2 = new eHubCodeSetResult { CR_PK = new Guid("{b0821a76-68fd-4529-91ff-4aa0f4bc0f00}"), eHubCodeSet = cs, CR_Order = 2, CR_Name = "SenderSubID" };
			var csr3 = new eHubCodeSetResult { CR_PK = new Guid("{35d276eb-eb07-4f53-8431-433a32522752}"), eHubCodeSet = cs, CR_Order = 3, CR_Name = "TradingPartnerID" };
			var csr4 = new eHubCodeSetResult { CR_PK = new Guid("{bb19ab3d-9977-477c-be86-7270f0d7b90f}"), eHubCodeSet = cs, CR_Order = 4, CR_Name = "AACID" };
			var cmk1 = new eHubCodeMapKey { CK_PK = new Guid("{08b10d13-a4f4-4a41-b7c3-1e0c95fe8866}"), eHubCodeSet = cs, CK_Order = 1, CK_Key1Value = cx1.CX_Code };
			var cmk2 = new eHubCodeMapKey { CK_PK = new Guid("{50364029-836d-4779-802f-df46a42d138c}"), eHubCodeSet = cs, CK_Order = 2, CK_Key1Value = cx2.CX_Code };
			var cmv1_1 = new eHubCodeMapValue { eHubCodeMapKey = cmk1, eHubCodeSetResult = csr1, CV_OutputCode = "Sender 1" };
			var cmv1_2 = new eHubCodeMapValue { eHubCodeMapKey = cmk1, eHubCodeSetResult = csr2, CV_OutputCode = "Sender Sub 1" };
			var cmv1_3 = new eHubCodeMapValue { eHubCodeMapKey = cmk1, eHubCodeSetResult = csr3, CV_OutputCode = "Trading Partner 1" };
			var cmv1_4 = new eHubCodeMapValue { eHubCodeMapKey = cmk1, eHubCodeSetResult = csr4, CV_OutputCode = "AACID 1" };
			var cmv2_1 = new eHubCodeMapValue { eHubCodeMapKey = cmk2, eHubCodeSetResult = csr1, CV_OutputCode = "Sender 2" };
			var cmv2_2 = new eHubCodeMapValue { eHubCodeMapKey = cmk2, eHubCodeSetResult = csr2, CV_OutputCode = "Sender Sub 2" };
			var cmv2_3 = new eHubCodeMapValue { eHubCodeMapKey = cmk2, eHubCodeSetResult = csr3, CV_OutputCode = "Trading Partner 2" };
			var cmv2_4 = new eHubCodeMapValue { eHubCodeMapKey = cmk2, eHubCodeSetResult = csr4, CV_OutputCode = "AACID 2" };

			context.eHubRegistrationTypes.AddObject(rt);
			context.eHubClients.AddObject(cc1);
			context.eHubClients.AddObject(cc2);
			context.eHubClients.AddObject(ZACustomsClient);
			context.eHubClientRegistrations.AddObject(cx1);
			context.eHubClientRegistrations.AddObject(cx2);
			context.eHubTransformationSets.AddObject(ZACustomsTS);
			context.eHubCodeSets.AddObject(cs);
			context.eHubCodeMapKeys.AddObject(cmk1);
			context.eHubCodeMapKeys.AddObject(cmk2);
			context.eHubCodeSetResults.AddObject(csr1);
			context.eHubCodeSetResults.AddObject(csr2);
			context.eHubCodeSetResults.AddObject(csr3);
			context.eHubCodeSetResults.AddObject(csr4);
			context.eHubCodeMapValues.AddObject(cmv1_1);
			context.eHubCodeMapValues.AddObject(cmv1_2);
			context.eHubCodeMapValues.AddObject(cmv1_3);
			context.eHubCodeMapValues.AddObject(cmv1_4);
			context.eHubCodeMapValues.AddObject(cmv2_1);
			context.eHubCodeMapValues.AddObject(cmv2_2);
			context.eHubCodeMapValues.AddObject(cmv2_3);
			context.eHubCodeMapValues.AddObject(cmv2_4);
		}

		[TestMethod]
		public void TestRegistrationsGet()
		{
			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["sidx"] = "CX_CC_ID";
			request.Container["sord"] = "asc";

			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			controller.Context = context;
			GenerateDefaultValues(context);

			var result = controller.Registrations();

			Assert.IsNotNull(result);
			AssertEx.JsonResultMatchesList(new List<object> { 
					new {CX_PK = "00000000-ffff-1111-1111-000000000000", CX_CC_ID = "CLIENT001", CX_Code = "REG001", SenderID = "Sender 1", SenderSubID = "Sender Sub 1", TradingPartnerID = "Trading Partner 1", AACID = "AACID 1"},
					new {CX_PK = "00000000-ffff-1111-2222-000000000000", CX_CC_ID = "CLIENT002", CX_Code = "REG002", SenderID = "Sender 2", SenderSubID = "Sender Sub 2", TradingPartnerID = "Trading Partner 2", AACID = "AACID 2"}
				}, result, "eHubClientRegistrations");
		}

		[TestMethod]
		public void TestRegistrationsEdit_Success()
		{
            var logger = new TestLogger();
            var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			controller.Context = context;
			GenerateDefaultValues(context);

			var codeMapKeysResult = context.eHubCodeMapKeys.Select(ck => new Tuple<Guid, Guid, int, String>(ck.CK_PK, ck.CK_CS, ck.CK_Order, ck.CK_Key1Value)).ToArray();
			CollectionAssert.AreEqual(new[] {
					new Tuple<Guid, Guid, int, String>(new Guid("{08b10d13-a4f4-4a41-b7c3-1e0c95fe8866}"), new Guid("{ac4fc9a6-854e-4c9e-b93d-e6bebcdc5736}"), 1, "REG001"),
					new Tuple<Guid, Guid, int, String>(new Guid("{50364029-836d-4779-802f-df46a42d138c}"), new Guid("{ac4fc9a6-854e-4c9e-b93d-e6bebcdc5736}"), 2, "REG002")
				}, codeMapKeysResult, "Precondition");

			request.Clear();
			request.Container["id"] = "_empty";
			request.Container["CX_CC_ID"] = "CLIENT001";
			request.Container["CX_Code"] = "REG003";
			request.Container["SenderID"] = "SenderID3";
			request.Container["SenderSubID"] = "SenderSubID3";
			request.Container["TradingPartnerID"] = "TradingPartnerID3";
			request.Container["AACID"] = "AACID3";
			request.Container["oper"] = "add";

			JsonResult responseAdd = RegistrationEditTest(logger) as JsonResult;

			var resultState = (bool)responseAdd.Data.GetType().GetProperty("success").GetValue(responseAdd.Data, null);
			Assert.IsTrue(resultState);
			var newId = (Guid)responseAdd.Data.GetType().GetProperty("id").GetValue(responseAdd.Data, null);
			var eHubClientRegistrationsResult = context.eHubClientRegistrations.Select(cx => new Tuple<Guid, Guid, Guid, String, String>(cx.CX_PK, cx.CX_CC, cx.CX_RT, cx.CX_Qualifier, cx.CX_Code)).ToArray();
			CollectionAssert.AreEqual(new[] {
					new Tuple<Guid, Guid, Guid, String, String>(new Guid("{00000000-ffff-1111-1111-000000000000}"), new Guid("{00000000-cccc-1111-1111-000000000000}"), new Guid("{00000000-eeee-1111-1111-000000000000}"), null, "REG001"),
					new Tuple<Guid, Guid, Guid, String, String>(new Guid("{00000000-ffff-1111-2222-000000000000}"), new Guid("{00000000-cccc-1111-2222-000000000000}"), new Guid("{00000000-eeee-1111-1111-000000000000}"), null, "REG002"),
					new Tuple<Guid, Guid, Guid, String, String>(newId, new Guid("{00000000-cccc-1111-1111-000000000000}"), new Guid("{00000000-eeee-1111-1111-000000000000}"), "REG003", "REG003")
				}, eHubClientRegistrationsResult);
            Assert.IsTrue(logger.Log.Contains("Info - [add] eHubClientRegistration:"));
            Assert.IsTrue(logger.Log.Contains("CX_Qualifier=REG003, CX_Code=REG003, CX_Attr1=, CX_Password1=, CX_Flag1=, CX_Flag2=, CX_CC=00000000-cccc-1111-1111-000000000000, CX_RT=00000000-eeee-1111-1111-000000000000"));
            var codeMapKeyResult = context.eHubCodeMapKeys.FirstOrDefault(x => x.CK_Key1Value == "REG003");
            Assert.IsNotNull(codeMapKeyResult);
            Assert.IsTrue(logger.Log.Contains("Info - [add] eHubCodeMapKey:"));
            Assert.IsTrue(logger.Log.Contains("CK_Key1Value=REG003"));
            var codeMapValuesResult = codeMapKeyResult.eHubCodeMapValues.Select(x => new Tuple<Guid, Guid, String>(x.CV_CK, x.CV_CR, x.CV_OutputCode)).ToArray();
			CollectionAssert.AreEqual(new[] {
					new Tuple<Guid, Guid, String>(codeMapKeyResult.CK_PK, new Guid("{83e4b535-3b86-4750-b018-4f3f573b8bcd}"), "SenderID3"),
					new Tuple<Guid, Guid, String>(codeMapKeyResult.CK_PK, new Guid("{b0821a76-68fd-4529-91ff-4aa0f4bc0f00}"), "SenderSubID3"),
					new Tuple<Guid, Guid, String>(codeMapKeyResult.CK_PK, new Guid("{35d276eb-eb07-4f53-8431-433a32522752}"), "TradingPartnerID3"),
					new Tuple<Guid, Guid, String>(codeMapKeyResult.CK_PK, new Guid("{bb19ab3d-9977-477c-be86-7270f0d7b90f}"), "AACID3")
				}, codeMapValuesResult);
            Assert.IsTrue(logger.Log.Contains("Info - [add] eHubCodeMapValue:"));
            Assert.IsTrue(logger.Log.Contains("CV_OutputCode=SenderID3, CV_CR=83e4b535-3b86-4750-b018-4f3f573b8bcd, CV_PassThroughKey="));
            Assert.IsTrue(logger.Log.Contains("CV_OutputCode=SenderSubID3, CV_CR=b0821a76-68fd-4529-91ff-4aa0f4bc0f00, CV_PassThroughKey="));
            Assert.IsTrue(logger.Log.Contains("CV_OutputCode=TradingPartnerID3, CV_CR=35d276eb-eb07-4f53-8431-433a32522752, CV_PassThroughKey="));
            Assert.IsTrue(logger.Log.Contains("CV_OutputCode=AACID3, CV_CR=bb19ab3d-9977-477c-be86-7270f0d7b90f, CV_PassThroughKey="));
            
            request.Clear();
			request.Container["CX_PK"] = newId.ToString();
			request.Container["CX_CC_ID"] = "CLIENT001";
			request.Container["CX_Code"] = "REG004";
			request.Container["SenderID"] = "SenderID4";
			request.Container["SenderSubID"] = "SenderSubID4";
			request.Container["TradingPartnerID"] = "TradingPartnerID4";
			request.Container["AACID"] = "AACID4";
			request.Container["oper"] = "edit";

            JsonResult responseEdit = RegistrationEditTest(logger) as JsonResult;
			Assert.IsTrue((bool)responseEdit.Data.GetType().GetProperty("success").GetValue(responseEdit.Data, null));
          
            eHubClientRegistrationsResult = context.eHubClientRegistrations.Select(cx => new Tuple<Guid, Guid, Guid, String, String>(cx.CX_PK, cx.CX_CC, cx.CX_RT, cx.CX_Qualifier, cx.CX_Code)).ToArray();
			CollectionAssert.AreEqual(new[] {
					new Tuple<Guid, Guid, Guid, String, String>(new Guid("{00000000-ffff-1111-1111-000000000000}"), new Guid("{00000000-CCCC-1111-1111-000000000000}"), new Guid("{00000000-eeee-1111-1111-000000000000}"), null, "REG001"),
					new Tuple<Guid, Guid, Guid, String, String>(new Guid("{00000000-ffff-1111-2222-000000000000}"), new Guid("{00000000-CCCC-1111-2222-000000000000}"), new Guid("{00000000-eeee-1111-1111-000000000000}"), null, "REG002"),
					new Tuple<Guid, Guid, Guid, String, String>(newId, new Guid("{00000000-CCCC-1111-1111-000000000000}"), new Guid("{00000000-eeee-1111-1111-000000000000}"), "REG004", "REG004")
				}, eHubClientRegistrationsResult);
            Assert.IsTrue(logger.Log.Contains("Info - [edit] eHubClientRegistration:"));
            Assert.IsTrue(logger.Log.Contains("CX_Qualifier=REG004, CX_Code=REG004, CX_Attr1=, CX_Password1=, CX_Flag1=, CX_Flag2=, CX_CC=00000000-cccc-1111-1111-000000000000, CX_RT=00000000-eeee-1111-1111-000000000000"));
            codeMapKeyResult = context.eHubCodeMapKeys.FirstOrDefault(x => x.CK_PK == codeMapKeyResult.CK_PK);
			Assert.AreEqual("REG004", codeMapKeyResult.CK_Key1Value);
            Assert.IsTrue(logger.Log.Contains("Info - [edit] eHubCodeMapKey:"));
            Assert.IsTrue(logger.Log.Contains("CK_Key1Value=REG004"));
            codeMapValuesResult = codeMapKeyResult.eHubCodeMapValues.Select(x => new Tuple<Guid, Guid, String>(x.CV_CK, x.CV_CR, x.CV_OutputCode)).ToArray();
			CollectionAssert.AreEqual(new[] {
					new Tuple<Guid, Guid, String>(codeMapKeyResult.CK_PK, new Guid("{83e4b535-3b86-4750-b018-4f3f573b8bcd}"), "SenderID4"),
					new Tuple<Guid, Guid, String>(codeMapKeyResult.CK_PK, new Guid("{b0821a76-68fd-4529-91ff-4aa0f4bc0f00}"), "SenderSubID4"),
					new Tuple<Guid, Guid, String>(codeMapKeyResult.CK_PK, new Guid("{35d276eb-eb07-4f53-8431-433a32522752}"), "TradingPartnerID4"),
					new Tuple<Guid, Guid, String>(codeMapKeyResult.CK_PK, new Guid("{bb19ab3d-9977-477c-be86-7270f0d7b90f}"), "AACID4")
				}, codeMapValuesResult);
            Assert.IsTrue(logger.Log.Contains("CV_OutputCode=SenderID4, CV_CR=83e4b535-3b86-4750-b018-4f3f573b8bcd, CV_PassThroughKey="));
            Assert.IsTrue(logger.Log.Contains("CV_OutputCode=SenderSubID4, CV_CR=b0821a76-68fd-4529-91ff-4aa0f4bc0f00, CV_PassThroughKey="));
            Assert.IsTrue(logger.Log.Contains("CV_OutputCode=TradingPartnerID4, CV_CR=35d276eb-eb07-4f53-8431-433a32522752, CV_PassThroughKey="));
            Assert.IsTrue(logger.Log.Contains("CV_OutputCode=AACID4, CV_CR=bb19ab3d-9977-477c-be86-7270f0d7b90f, CV_PassThroughKey="));

            codeMapKeysResult = context.eHubCodeMapKeys.Select(ck => new Tuple<Guid, Guid, int, String>(ck.CK_PK, ck.CK_CS, ck.CK_Order, ck.CK_Key1Value)).ToArray();
			CollectionAssert.AreEqual(new[] {
					new Tuple<Guid, Guid, int, String>(new Guid("{08b10d13-a4f4-4a41-b7c3-1e0c95fe8866}"), new Guid("{ac4fc9a6-854e-4c9e-b93d-e6bebcdc5736}"), 1, "REG001"),
					new Tuple<Guid, Guid, int, String>(new Guid("{50364029-836d-4779-802f-df46a42d138c}"), new Guid("{ac4fc9a6-854e-4c9e-b93d-e6bebcdc5736}"), 3, "REG002"),
					new Tuple<Guid, Guid, int, String>(codeMapKeyResult.CK_PK, new Guid("{ac4fc9a6-854e-4c9e-b93d-e6bebcdc5736}"), 2, "REG004")
				}, codeMapKeysResult);

			request.Clear();
			request.Container["CX_PK"] = newId.ToString();
			request.Container["oper"] = "del";

            JsonResult responseDel = RegistrationEditTest(logger) as JsonResult;

            Assert.IsTrue((bool)responseDel.Data.GetType().GetProperty("success").GetValue(responseDel.Data, null));
			Assert.AreEqual(2, context.eHubClientRegistrations.Count());
			eHubClientRegistrationsResult = context.eHubClientRegistrations.Select(cx => new Tuple<Guid, Guid, Guid, String, String>(cx.CX_PK, cx.CX_CC, cx.CX_RT, cx.CX_Qualifier, cx.CX_Code)).ToArray();
			CollectionAssert.AreEqual(new[] {
					new Tuple<Guid, Guid, Guid, String, String>(new Guid("{00000000-ffff-1111-1111-000000000000}"), new Guid("{00000000-CCCC-1111-1111-000000000000}"), new Guid("{00000000-eeee-1111-1111-000000000000}"), null, "REG001"),
					new Tuple<Guid, Guid, Guid, String, String>(new Guid("{00000000-ffff-1111-2222-000000000000}"), new Guid("{00000000-CCCC-1111-2222-000000000000}"), new Guid("{00000000-eeee-1111-1111-000000000000}"), null, "REG002")
				}, eHubClientRegistrationsResult);
            Assert.IsTrue(logger.Log.Contains("Info - [del] eHubClientRegistration:"));
            Assert.IsTrue(logger.Log.Contains("CX_Qualifier=REG004, CX_Code=REG004, CX_Attr1=, CX_Password1=, CX_Flag1=, CX_Flag2=, CX_CC=00000000-cccc-1111-1111-000000000000, CX_RT=00000000-eeee-1111-1111-000000000000"));
            codeMapKeysResult = context.eHubCodeMapKeys.Select(ck => new Tuple<Guid, Guid, int, String>(ck.CK_PK, ck.CK_CS, ck.CK_Order, ck.CK_Key1Value)).ToArray();
			CollectionAssert.AreEqual(new[] {
					new Tuple<Guid, Guid, int, String>(new Guid("{08b10d13-a4f4-4a41-b7c3-1e0c95fe8866}"), new Guid("{ac4fc9a6-854e-4c9e-b93d-e6bebcdc5736}"), 1, "REG001"),
					new Tuple<Guid, Guid, int, String>(new Guid("{50364029-836d-4779-802f-df46a42d138c}"), new Guid("{ac4fc9a6-854e-4c9e-b93d-e6bebcdc5736}"), 3, "REG002")
				}, codeMapKeysResult);
			Assert.IsNull(context.eHubCodeMapValues.FirstOrDefault(x => x.CV_CK == codeMapKeyResult.CK_PK));
        }

        [TestMethod]
		public void TestRegistrationsEdit_Fail()
		{
			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			controller.Context = context;
			GenerateDefaultValues(context);

			request.Clear();
			request.Container["id"] = "_empty";
			request.Container["CX_CC_ID"] = "CLIENT001";
			request.Container["CX_Qualifier"] = "QUAL001";
			request.Container["CX_Code"] = "REG001";
			request.Container["SenderID"] = "SenderID";
			request.Container["SenderSubID"] = "SenderSubID";
			request.Container["TradingPartnerID"] = "TradingPartnerID";
			request.Container["AACID"] = "AACID";
			request.Container["oper"] = "add";

			var responseAdd = controller.RegistrationEdit();

			var resultState = (bool)responseAdd.Data.GetType().GetProperty("success").GetValue(responseAdd.Data, null);
			Assert.IsFalse(resultState);
			var message = (string)responseAdd.Data.GetType().GetProperty("message").GetValue(responseAdd.Data, null);
			Assert.AreEqual("\"REG001\" Agent Id already exists. Please choose a different Agent Id.", message);

			// 00000000-ffff-1111-2222-000000000000 <-> REG001
			// 00000000-ffff-2222-2222-000000000000 <-> REG002 (Update this CX_Code to REG001)
			request.Clear();
			request.Container["CX_PK"] = "00000000-ffff-2222-2222-000000000000"; 
			request.Container["CX_CC_ID"] = "CLIENT001";
			request.Container["CX_Qualifier"] = "";
			request.Container["CX_Code"] = "REG001";
			request.Container["SenderID"] = "SenderID4";
			request.Container["SenderSubID"] = "SenderSubID4";
			request.Container["TradingPartnerID"] = "TradingPartnerID4";
			request.Container["AACID"] = "AACID4";
			request.Container["oper"] = "edit";

			var responseEdit = controller.RegistrationEdit();
			resultState = (bool)responseAdd.Data.GetType().GetProperty("success").GetValue(responseAdd.Data, null);
			Assert.IsFalse(resultState);
			message = (string)responseAdd.Data.GetType().GetProperty("message").GetValue(responseAdd.Data, null);
			Assert.AreEqual("\"REG001\" Agent Id already exists. Please choose a different Agent Id.", message);
		}

		[TestMethod]
		public void TestClientsGetIncludeNonProdCWSystems()
		{
			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "8";
			request.Container["sidx"] = "CC_ID";
			request.Container["sord"] = "asc";
			request.Container["_search"] = "false";
			IList<object> expected1 = new List<object>
		    {
		        new { CC_ID = "CLIENT_MESSAGE_REF", CC_FriendlyName = "Client Message Ref" },
		        new { CC_ID = "CLIENT_MESSAGE_REF_2", CC_FriendlyName = "Client Message Ref 2" },
		        new { CC_ID = "CLIENT_MESSAGE_REF_3", CC_FriendlyName = "Client Message Ref 3" },
		        new { CC_ID = "CLIENT_MESSAGE_REF_4", CC_FriendlyName = "Client Message Ref 4" },
		        new { CC_ID = "CLIENT_USCUST", CC_FriendlyName = "Client US Customs" },
				new { CC_ID = "CLIENT_USCUST2", CC_FriendlyName = "Client US Customs 2" },
				new { CC_ID = "CLIENT0004", CC_FriendlyName = "Client 4" },
		        new { CC_ID = "CLIENT0005", CC_FriendlyName = "Client 4" },
		        new { CC_ID = "CLIENT0006", CC_FriendlyName = "Client 6" },
		    };

			var result1 = controller.Clients(true);

			Assert.IsNotNull(result1);
			AssertEx.JsonResultMatchesList(expected1, result1, "eHubClients");

			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["sidx"] = "CC_FriendlyName";
			request.Container["sord"] = "desc";
			request.Container["_search"] = "true";
			request.Container["CC_ID"] = "TEST";
			IList<object> expected2 = new List<object>
		    {
		        new { CC_ID = "TEST0003", CC_FriendlyName = "Test Client 3" },
		        new { CC_ID = "TEST0002", CC_FriendlyName = "Test Client 2" },
		        new { CC_ID = "TEST0001", CC_FriendlyName = "Test Client 1" },
		    };

			var result2 = controller.Clients(true);

			Assert.IsNotNull(result2);
			AssertEx.JsonResultMatchesList(expected2, result2, "eHubClients");

			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["sidx"] = "CC_FriendlyName";
			request.Container["sord"] = "asc";
			request.Container["_search"] = "true";
			request.Container["CC_ID"] = "TEST";
			request.Container["CC_FriendlyName"] = "3";
			IList<object> expected3 = new List<object>
		    {
		        new { CC_ID = "TEST0003", CC_FriendlyName = "Test Client 3" },
		    };

			var result3 = controller.Clients(true);

			Assert.IsNotNull(result2);
			AssertEx.JsonResultMatchesList(expected3, result3, "eHubClients");
		}

		[TestMethod]
		public void TestClientsExcludeNonProdCWSystems()
		{
			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "15";
			request.Container["sidx"] = "CC_ID";
			request.Container["sord"] = "asc";
			request.Container["_search"] = "false";
			IList<object> expected1 = new List<object>
			{
				new { CC_ID = "CLIENT_MESSAGE_REF", CC_FriendlyName = "Client Message Ref" },
				new { CC_ID = "CLIENT_MESSAGE_REF_2", CC_FriendlyName = "Client Message Ref 2" },
				new { CC_ID = "CLIENT_MESSAGE_REF_3", CC_FriendlyName = "Client Message Ref 3" },
				new { CC_ID = "CLIENT_MESSAGE_REF_4", CC_FriendlyName = "Client Message Ref 4" },
				new { CC_ID = "CLIENT_USCUST", CC_FriendlyName = "Client US Customs" },
				new { CC_ID = "CLIENT_USCUST2", CC_FriendlyName = "Client US Customs 2" },
				new { CC_ID = "CLIENT0004", CC_FriendlyName = "Client 4" },
				new { CC_ID = "CLIENT0005", CC_FriendlyName = "Client 4" },
				new { CC_ID = "CLIENT0006", CC_FriendlyName = "Client 6" },
				new { CC_ID = "CLIENT0007", CC_FriendlyName = "Client 7" },
				new { CC_ID = "CLIENT0008", CC_FriendlyName = "Client 8" },
				new { CC_ID = "TEST0001", CC_FriendlyName = "Test Client 1" },
				new { CC_ID = "TEST0002", CC_FriendlyName = "Test Client 2" },
				new { CC_ID = "TEST0003", CC_FriendlyName = "Test Client 3" },
				new { CC_ID = "WTLPRDSV1", CC_FriendlyName = "Prod System 1" },
			};

			var result1 = controller.Clients(false);

			Assert.IsNotNull(result1);
			AssertEx.JsonResultMatchesList(expected1, result1, "eHubClients");

			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "15";
			request.Container["sidx"] = "CC_ID";
			request.Container["sord"] = "asc";
			request.Container["_search"] = "false";
			expected1 = new List<object>
			{
				new { CC_ID = "CLIENT_MESSAGE_REF", CC_FriendlyName = "Client Message Ref" },
				new { CC_ID = "CLIENT_MESSAGE_REF_2", CC_FriendlyName = "Client Message Ref 2" },
				new { CC_ID = "CLIENT_MESSAGE_REF_3", CC_FriendlyName = "Client Message Ref 3" },
				new { CC_ID = "CLIENT_MESSAGE_REF_4", CC_FriendlyName = "Client Message Ref 4" },
				new { CC_ID = "CLIENT_USCUST", CC_FriendlyName = "Client US Customs" },
				new { CC_ID = "CLIENT_USCUST2", CC_FriendlyName = "Client US Customs 2" },
				new { CC_ID = "CLIENT0004", CC_FriendlyName = "Client 4" },
				new { CC_ID = "CLIENT0005", CC_FriendlyName = "Client 4" },
				new { CC_ID = "CLIENT0006", CC_FriendlyName = "Client 6" },
				new { CC_ID = "CLIENT0007", CC_FriendlyName = "Client 7" },
				new { CC_ID = "CLIENT0008", CC_FriendlyName = "Client 8" },
				new { CC_ID = "TEST0001", CC_FriendlyName = "Test Client 1" },
				new { CC_ID = "TEST0002", CC_FriendlyName = "Test Client 2" },
				new { CC_ID = "TEST0003", CC_FriendlyName = "Test Client 3" },
				new { CC_ID = "WTLPRDSV1", CC_FriendlyName = "Prod System 1" },
				new { CC_ID = "WTLTSTSV2", CC_FriendlyName = "Test System 2" },
			};

			result1 = controller.Clients(true);

			Assert.IsNotNull(result1);
			AssertEx.JsonResultMatchesList(expected1, result1, "eHubClients");
		}

		[TestMethod]
		public void TestClientsFilterCaseInsensitive()
		{
			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "15";
			request.Container["sidx"] = "CC_ID";
			request.Container["sord"] = "asc";
			request.Container["_search"] = "true";
			request.Container["CC_ID"] = "test";

			IList<object> expected1 = new List<object>
			{
				new { CC_ID = "TEST0001", CC_FriendlyName = "Test Client 1" },
				new { CC_ID = "TEST0002", CC_FriendlyName = "Test Client 2" },
				new { CC_ID = "TEST0003", CC_FriendlyName = "Test Client 3" },
			};

			var result1 = controller.Clients(false);

			Assert.IsNotNull(result1);
			AssertEx.JsonResultMatchesList(expected1, result1, "eHubClients");
		}

		[TestMethod]
		public void TestRegistrations_MultipleFilter_AND()
		{
			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["filters"] = "{\"groupOp\":\"AND\",\"rules\":[{\"field\":\"CX_CC_ID\",\"op\":\"cn\",\"data\":\"1\"},{\"field\":\"CX_Code\",\"op\":\"bw\",\"data\":\"X\"}]}";

			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			controller.Context = context;

			var cxPk1 = new Guid("{00000000-FFFF-1111-1111-000000000000}");
			var cxPk2 = new Guid("{00000000-FFFF-1111-2222-000000000000}");
			var cxPk3 = new Guid("{00000000-FFFF-1111-3333-000000000000}");
			var cxPk4 = new Guid("{00000000-FFFF-1111-4444-000000000000}");
			var cxPk5 = new Guid("{00000000-FFFF-1111-5555-000000000000}");
			var cxPk6 = new Guid("{00000000-FFFF-1111-6666-000000000000}");

			var cc1 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), CC_ID = "DummyID11" };
			var cc2 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-2222-000000000000}"), CC_ID = "DummyID22" };
			var cc3 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-3333-000000000000}"), CC_ID = "DummyID31" };

			var rt1 = new eHubRegistrationType { RT_PK = new Guid("{00000000-EEEE-1111-1111-000000000000}"), RT_ID = ZACustomsController.ZACustomsCode, RT_Description = "Registration 1", RT_RegistrantType = "Client" };

			var cx1 = new eHubClientRegistration
			{
				CX_PK = cxPk1,
				eHubClient = cc1,
				eHubRegistrationType = rt1,
				CX_Code = "XREG001",
				CX_Qualifier = "QualifierValue1",
				CX_Attr1 = "Attr1",
				CX_Password1 = "Password1",
				CX_Flag1 = 11,
				CX_Flag2 = 12,
				CX_ConfigXml = "ConfigXml1"
			};
			var cx2 = new eHubClientRegistration
			{
				CX_PK = cxPk2,
				eHubClient = cc2,
				eHubRegistrationType = rt1,
				CX_Code = "XREG002",
				CX_Qualifier = "QualifierValue2",
			};
			var cx3 = new eHubClientRegistration
			{
				CX_PK = cxPk3,
				eHubClient = cc3,
				eHubRegistrationType = rt1,
				CX_Code = "XREG003",
				CX_Qualifier = "DummyQualifierValue3",
			};
			var cx4 = new eHubClientRegistration
			{
				CX_PK = cxPk4,
				eHubClient = cc1,
				eHubRegistrationType = rt1,
				CX_Code = "REG004",
				CX_Qualifier = "QualifierValue4",
			};
			var cx5 = new eHubClientRegistration
			{
				CX_PK = cxPk5,
				eHubClient = cc2,
				eHubRegistrationType = rt1,
				CX_Code = "REG005",
				CX_Qualifier = "QualifierValue5",
			};
			var cx6 = new eHubClientRegistration
			{
				CX_PK = cxPk6,
				eHubClient = cc3,
				eHubRegistrationType = rt1,
				CX_Code = "REG006",
				CX_Qualifier = "DummyQualifierValue6"
			};

			context.eHubRegistrationTypes.AddObject(rt1);
			context.eHubClientRegistrations.AddObject(cx1);
			context.eHubClientRegistrations.AddObject(cx2);
			context.eHubClientRegistrations.AddObject(cx3);
			context.eHubClientRegistrations.AddObject(cx4);
			context.eHubClientRegistrations.AddObject(cx5);
			context.eHubClientRegistrations.AddObject(cx6);

			var result = controller.Registrations();

			Assert.IsNotNull(result);
			AssertEx.JsonResultMatchesList(new List<object> { new {
				CX_PK = cxPk1.ToString(),
				CX_CC_ID = cc1.CC_ID,
				CX_Code = "XREG001",
			}, new {
				CX_PK = cxPk3.ToString(),
				CX_CC_ID = cc3.CC_ID,
				CX_Code = "XREG003",
			} }, result, "eHubClientRegistrations");
		}

		[TestMethod]
		public void TestRegistrations_MultipleFilter_OR()
		{
			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["filters"] = "{\"groupOp\":\"OR\",\"rules\":[{\"field\":\"CX_CC_ID\",\"op\":\"cn\",\"data\":\"1\"},{\"field\":\"CX_Code\",\"op\":\"bw\",\"data\":\"X\"}]}";

			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			controller.Context = context;

			var cxPk1 = new Guid("{00000000-FFFF-1111-1111-000000000000}");
			var cxPk2 = new Guid("{00000000-FFFF-1111-2222-000000000000}");
			var cxPk3 = new Guid("{00000000-FFFF-1111-3333-000000000000}");
			var cxPk4 = new Guid("{00000000-FFFF-1111-4444-000000000000}");
			var cxPk5 = new Guid("{00000000-FFFF-1111-5555-000000000000}");
			var cxPk6 = new Guid("{00000000-FFFF-1111-6666-000000000000}");

			var cc1 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), CC_ID = "DummyID11" };
			var cc2 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-2222-000000000000}"), CC_ID = "DummyID22" };
			var cc3 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-3333-000000000000}"), CC_ID = "DummyID31" };

			var rt1 = new eHubRegistrationType { RT_PK = new Guid("{00000000-EEEE-1111-1111-000000000000}"), RT_ID = ZACustomsController.ZACustomsCode, RT_Description = "Registration 1", RT_RegistrantType = "Client" };

			var cx1 = new eHubClientRegistration
			{
				CX_PK = cxPk1,
				eHubClient = cc1,
				eHubRegistrationType = rt1,
				CX_Code = "XREG001",
				CX_Qualifier = "QualifierValue1",
				CX_Attr1 = "Attr1",
				CX_Password1 = "Password1",
				CX_Flag1 = 11,
				CX_Flag2 = 12,
				CX_ConfigXml = "ConfigXml1"
			};
			var cx2 = new eHubClientRegistration
			{
				CX_PK = cxPk2,
				eHubClient = cc2,
				eHubRegistrationType = rt1,
				CX_Code = "XREG002",
				CX_Qualifier = "QualifierValue2",
			};
			var cx3 = new eHubClientRegistration
			{
				CX_PK = cxPk3,
				eHubClient = cc3,
				eHubRegistrationType = rt1,
				CX_Code = "XREG003",
				CX_Qualifier = "DummyQualifierValue3",
			};
			var cx4 = new eHubClientRegistration
			{
				CX_PK = cxPk4,
				eHubClient = cc1,
				eHubRegistrationType = rt1,
				CX_Code = "REG004",
				CX_Qualifier = "QualifierValue4",
			};
			var cx5 = new eHubClientRegistration
			{
				CX_PK = cxPk5,
				eHubClient = cc2,
				eHubRegistrationType = rt1,
				CX_Code = "REG005",
				CX_Qualifier = "QualifierValue5",
			};
			var cx6 = new eHubClientRegistration
			{
				CX_PK = cxPk6,
				eHubClient = cc3,
				eHubRegistrationType = rt1,
				CX_Code = "REG006",
				CX_Qualifier = "DummyQualifierValue6"
			};

			context.eHubRegistrationTypes.AddObject(rt1);
			context.eHubClientRegistrations.AddObject(cx1);
			context.eHubClientRegistrations.AddObject(cx2);
			context.eHubClientRegistrations.AddObject(cx3);
			context.eHubClientRegistrations.AddObject(cx4);
			context.eHubClientRegistrations.AddObject(cx5);
			context.eHubClientRegistrations.AddObject(cx6);

			var result = controller.Registrations();

			Assert.IsNotNull(result);
			AssertEx.JsonResultMatchesList(new List<object> { new {
				CX_PK = cxPk4.ToString(),
				CX_CC_ID = cc1.CC_ID,
				CX_Code = "REG004",
			}, new {
				CX_PK = cxPk1.ToString(),
				CX_CC_ID = cc1.CC_ID,
				CX_Code = "XREG001",
			}, new {
				CX_PK = cxPk2.ToString(),
				CX_CC_ID = cc2.CC_ID,
				CX_Code = "XREG002",
			}, new {
				CX_PK = cxPk6.ToString(),
				CX_CC_ID = cc3.CC_ID,
				CX_Code = "REG006",
			}, new {
				CX_PK = cxPk3.ToString(),
				CX_CC_ID = cc3.CC_ID,
				CX_Code = "XREG003",
			} }, result, "eHubClientRegistrations");
		}

		protected JsonResult RegistrationEditTest(ILog logger)
        {
           controller.logger = logger;
           return controller.RegistrationEdit();
        }
    }
}
