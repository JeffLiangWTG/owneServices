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
	public class AsyncPollingRegistrationsControllerTest : BaseControllerTest<AsyncPollingRegistrationsController>
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

			AssertEx.PropertyValuesAreEquals(new { RT_ID = "REG002", RT_Description = "Registration 2" }, context.eHubRegistrationTypes.Select(r => new { r.RT_ID, r.RT_Description }).FirstOrDefault(), false);
			Assert.IsTrue(logger.Log.Contains("Info - [CORP\\Test.Name] [add] eHubRegistrationType:"));
			Assert.IsTrue(logger.Log.Contains("RT_RegistrantType=AsyncPolling, RT_ID=REG002, RT_Description=Registration 2"));
			var rtPK = context.eHubRegistrationTypes.First().RT_PK;

			request.Clear();
			request.Container["RT_PK"] = rtPK.ToString();
			request.Container["RT_ID"] = "REG002a";
			request.Container["RT_Description"] = "Registration 2a";
			request.Container["oper"] = "edit";

			RegistrationTypeInfoEditTest(logger);

			AssertEx.PropertyValuesAreEquals(new { RT_PK = rtPK, RT_ID = "REG002a", RT_Description = "Registration 2a" }, context.eHubRegistrationTypes.First(r => r.RT_PK == rtPK), false);
			Assert.IsTrue(logger.Log.Contains("Info - [CORP\\Test.Name] [edit] eHubRegistrationType:"));
			Assert.IsTrue(logger.Log.Contains("RT_RegistrantType=AsyncPolling, RT_ID=REG002a, RT_Description=Registration 2a"));

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
			Assert.IsTrue(logger.Log.Contains("RT_RegistrantType=AsyncPolling, RT_ID=REG002a, RT_Description=Registration 2a"));
			Assert.IsTrue(logger.Log.Contains("Info - [CORP\\Test.Name] [del] eHubClientSystemRegistration:"));
			Assert.IsTrue(logger.Log.Contains("CD_Code=REG001, CD_Attr1=username01, CD_Attr2=password01, CD_Flag1=1"));
			Assert.IsTrue(logger.Log.Contains("Info - [CORP\\Test.Name] [del] eHubClientRegistration:"));
			Assert.IsTrue(logger.Log.Contains("CX_Code=REG001, CX_Attr1=, CX_Password1=, CX_Flag1=, CX_Flag2="));

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
			var rt = new eHubRegistrationType { RT_PK = new Guid("{00000000-EEEE-1111-1111-000000000000}"), RT_ID = "REG001", RT_Description = "Registration 1" };
			var cc1 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), CC_ID = "CLIENT001" };
			var cc2 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-2222-000000000000}"), CC_ID = "CLIENT002" };
			var cs1 = new eHubClientSystem { EH_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), EH_ID = "CLI001" };
			var cs2 = new eHubClientSystem { EH_PK = new Guid("{00000000-CCCC-1111-2222-000000000000}"), EH_ID = "CLI002" };

			var pr1 = new eHubAsyncPollingRegistration { PR_PK = new Guid("{00000000-FFFF-1111-1111-000000000000}"), eHubClient = cc1, eHubClientSystem = cs1, PR_RT = rt.RT_PK, PR_Text = "TEXT001", PR_CreatedUTC = DateTime.Parse("2020-10-12 09:45:07") };
			var pr2 = new eHubAsyncPollingRegistration { PR_PK = new Guid("{00000000-FFFF-1111-2222-000000000000}"), eHubClient = cc2, eHubClientSystem = cs2, PR_RT = rt.RT_PK, PR_Text = "TEXT002", PR_CreatedUTC = DateTime.Parse("2020-10-12 09:45:07") };
			context.eHubRegistrationTypes.AddObject(rt);
			context.eHubAsyncPollingRegistrations.AddObject(pr1);
			context.eHubAsyncPollingRegistrations.AddObject(pr2);

			var result = controller.AsyncPollingRegistrations(rt.RT_PK);

			Assert.IsNotNull(result);
			AssertEx.JsonResultMatchesList(new List<object> { new {PR_PK = new Guid("{00000000-FFFF-1111-1111-000000000000}"), PR_EH_ID = "CLI001", PR_CC_ID = "CLIENT001", PR_Text = "TEXT001", PR_XML = new Guid("{00000000-FFFF-1111-1111-000000000000}"), PR_CreatedUTC = "2020-10-12 09:45:07", CustomValue1 = "", CustomValue2 = "" },
																new {PR_PK = new Guid("{00000000-FFFF-1111-2222-000000000000}"), PR_EH_ID = "CLI002", PR_CC_ID = "CLIENT002", PR_Text = "TEXT002", PR_XML = new Guid("{00000000-FFFF-1111-2222-000000000000}"), PR_CreatedUTC = "2020-10-12 09:45:07", CustomValue1 = "", CustomValue2 =""  }},
				result, "AsyncPollingRegistrations");
		}

		[TestMethod]
		public void TestRegistrationsEdit()
		{
			var logger = new TestLogger();
			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			controller.Context = context;

			var rt = new eHubRegistrationType { RT_PK = new Guid("{00000000-EEEE-1111-1111-000000000000}"), RT_ID = "REG001", RT_Description = "Registration 1" };
			var cc = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), CC_ID = "CLIENT001" };
			var cs = new eHubClientSystem { EH_PK = new Guid("{00000000-CCCC-1111-2222-000000000000}"), EH_ID = "CLI002" };
			context.eHubRegistrationTypes.AddObject(rt);
			context.eHubClients.AddObject(cc);
			context.eHubClientSystems.AddObject(cs);

			request.Clear();
			request.Container["id"] = "_empty";
			request.Container["PR_CC_ID"] = "CLIENT001";
			request.Container["PR_Text"] = "Text001";
			request.Container["PR_EH_ID"] = "CLI002";
			request.Container["PR_XML"] = "test";
			request.Container["oper"] = "add";

			var responseAdd = RegistrationEditTest(rt.RT_PK, logger); 

			var resultState = (bool)responseAdd.Data.GetType().GetProperty("success").GetValue(responseAdd.Data, null);
			Assert.IsTrue(resultState);
			Assert.IsTrue(logger.Log.Contains("Info - [CORP\\Test.Name] [add] eHubAsyncPollingRegistration:"));
			Assert.IsTrue(logger.Log.Contains("R_CC=00000000-cccc-1111-1111-000000000000, PR_EH=00000000-cccc-1111-2222-000000000000, PR_RT=00000000-eeee-1111-1111-000000000000, PR_Text=Text001" ));
			var newId = (Guid)responseAdd.Data.GetType().GetProperty("id").GetValue(responseAdd.Data, null);
			CollectionAssert.AreEqual(new[] { new Tuple<Guid, Guid, String>(newId, rt.RT_PK,"Text001") },
				context.eHubAsyncPollingRegistrations.Select(pr => new Tuple<Guid,Guid, String>(pr.PR_PK, pr.PR_RT, pr.PR_Text)).ToArray());

			request.Clear();
			request.Container["PR_PK"] = newId.ToString();
			request.Container["PR_CC_ID"] = "CLIENT001";
			request.Container["PR_Text"] = "";
			request.Container["PR_EH_ID"] = "CLI002";
			request.Container["PR_XML"] = "test";
			request.Container["oper"] = "edit";

			var responseEdit = RegistrationEditTest(rt.RT_PK, logger);

			Assert.IsTrue((bool)responseEdit.Data.GetType().GetProperty("success").GetValue(responseEdit.Data, null));
			Assert.IsTrue(logger.Log.Contains("Info - [CORP\\Test.Name] [edit] eHubAsyncPollingRegistration:"));
			Assert.IsTrue(logger.Log.Contains("R_CC=00000000-cccc-1111-1111-000000000000, PR_EH=00000000-cccc-1111-2222-000000000000, PR_RT=00000000-eeee-1111-1111-000000000000, PR_Text=Text001"));
			CollectionAssert.AreEqual(new[] { new Tuple<Guid, Guid, String>(newId, rt.RT_PK, "") },
				context.eHubAsyncPollingRegistrations.Select(pr => new Tuple< Guid, Guid, String>(pr.PR_PK, pr.PR_RT, pr.PR_Text)).ToArray());

			request.Clear();
			request.Container["PR_PK"] = newId.ToString();
			request.Container["oper"] = "del";

			var responseDel = RegistrationEditTest(rt.RT_PK, logger);

			Assert.IsTrue((bool)responseDel.Data.GetType().GetProperty("success").GetValue(responseDel.Data, null));
			Assert.AreEqual(0, context.eHubAsyncPollingRegistrations.Count());
			Assert.IsTrue(logger.Log.Contains("Info - [CORP\\Test.Name] [del] eHubAsyncPollingRegistration:"));
			Assert.IsTrue(logger.Log.Contains("PR_CC=00000000-cccc-1111-1111-000000000000, PR_EH=00000000-cccc-1111-2222-000000000000, PR_RT=00000000-eeee-1111-1111-000000000000, PR_Text="));
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
		public void TestClientSystemsFilterCaseInsensitive()
		{
			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			controller.Context = context;
			context.eHubClientSystems.AddObject(new eHubClientSystem { EH_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), EH_ID = "CLI001", EH_URL = "url01.com" });
			context.eHubClientSystems.AddObject(new eHubClientSystem { EH_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), EH_ID = "CLI002", EH_URL = "url02.com" });
			context.eHubClientSystems.AddObject(new eHubClientSystem { EH_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), EH_ID = "CLI003", EH_URL = "" });
			context.eHubClientSystems.AddObject(new eHubClientSystem { EH_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), EH_ID = "CLI004", EH_URL = "url04.com" });
			context.eHubClientSystems.AddObject(new eHubClientSystem { EH_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), EH_ID = "CLI005", EH_URL = "url22.com" });
			context.eHubClientSystems.AddObject(new eHubClientSystem { EH_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), EH_ID = "Test_CLI005", EH_URL = "url22.com" });

			context.ediProdClients.AddObject(new ediProdClient { CC_ID = "CLIPRD004", CC_PK = new Guid("{10000000-CCCC-1111-1111-000000000000}"), EnterpriseServerCode = "CLI004", LE_EnterpriseCode = "CLI", LD_LicenceType = "PRD", LD_ServerCode = "004" });
			context.ediProdClients.AddObject(new ediProdClient { CC_ID = "CLIPRD005", CC_PK = new Guid("{20000000-CCCC-1111-1111-000000000000}"), EnterpriseServerCode = "CLI005", LE_EnterpriseCode = "CLI", LD_LicenceType = "TST", LD_ServerCode = "005" });

			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["sidx"] = "EH_ID";
			request.Container["sord"] = "asc";
			request.Container["_search"] = "true";
			request.Container["EH_ID"] = "test";
			IList<object> expected1 = new List<object>
			{
				new { EH_ID = "Test_CLI005", EH_URL = "url22.com" },
			};

			var result1 = controller.ClientSystems(true);

			Assert.IsNotNull(result1);
			AssertEx.JsonResultMatchesList(expected1, result1, "eHubClientSystems");
		}

		[TestMethod]
		public void TesteHubPortalSemanticsFactory()
		{
			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			controller.Context = context;
			var expectedDict = new Dictionary<string, string>()
			{
				{"PR_EH_ID", "Client System"},
				{"PR_Text", "Staff"},
				{"PR_XML", "XML"},
				{"PR_CC_ID", "Client"},
				{"PR_CreatedUTC", "Created UTC"}
			};
			var resultDict = new Dictionary<string, string>();
			foreach (var key in expectedDict.Keys)
			{
				resultDict[key] = controller.GetCustomisedHeaderForID("ACAS_BRProtocol", key);
			}

			CollectionAssert.AreEquivalent(resultDict.ToList(), expectedDict.ToList());
		}

		[TestMethod]
		public void TesteHubPortalSemanticsFactory_ACAS_BRProtocol()
		{
			var expectedColumnNames = new Dictionary<string, string>()
			{
				{"PR_EH_ID", "Client System"},
				{"PR_Text", "Staff"},
				{"PR_XML", "XML"},
				{"PR_CC_ID", "Client"},
				{"PR_CreatedUTC", "Created UTC"}
			};

			TesteHubPortalSemanticsFactory("ACAS_BRProtocol", expectedColumnNames);
		}

		private void TesteHubPortalSemanticsFactory(string registrationTypeId, Dictionary<string, string> expectedColumnNames)
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

		protected JsonResult RegistrationEditTest(Guid regType, ILog logger)
		{
			controller.logger = logger;
			return controller.AsyncPollingRegistrationsEdit(regType);
		}
	}
}
