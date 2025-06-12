using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using CargoWise.eHub.Portal.Controllers;
using CargoWise.eHub.Portal.Models.eHubTransactions;
using CargoWise.eHub.Portal.Models.View;
using CargoWise.eHub.Portal.Tests.Fakes;
using CargoWise.eHub.Portal.Tests.TestHelpers;
using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvcContrib.TestHelper.Ui;

namespace CargoWise.eHub.Portal.Tests.Controllers
{
	[TestClass]
	public class USCustomsControllerTest : BaseControllerTest<USCustomsController>
	{
		[TestMethod]
		public void TestIndex()
		{
			ViewResult result = controller.Index() as ViewResult;
			Assert.IsNotNull(result);
			Assert.AreEqual(string.Empty, result.ViewName, "Should be empty (Index)");
		}

		[TestMethod]
		public void TestRegistry()
		{
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["sidx"] = "CC_ID";
			request.Container["sord"] = "asc";
			eHubClient client = context.eHubClients.Where(c => c.eHubUSCustomsRegistry.Count > 0).First();
			JsonResult result = controller.Registry();
			Assert.IsNotNull(result);
			var records = result.Data.GetType().GetProperty("records").GetValue(result.Data, null);
			Assert.AreEqual(2, records, "records");
			var registryList = result.Data.GetType().GetProperty("eHubUSCustomsRegistry").GetValue(result.Data, null) as IEnumerable<USCustomsRegistryFormattedData>;
			Assert.IsNotNull(registryList);
			Assert.AreEqual(2, registryList.Count(), "Registry Count: ");
			var registry = registryList.FirstOrDefault();
			Assert.AreEqual(client.CC_ID, registry.CC_ID, "CC_ID: ");
			Assert.AreEqual(client.CC_FriendlyName, registry.CC_FriendlyName, "CC_FiendlyName");
			Assert.AreEqual(client.CC_USCustomsRecipient, registry.CC_USCustomsRecipient, "CC_USCustomsRecipient: ");
			Assert.IsNotNull(registry.ISF);
			Assert.AreEqual("ISF_Value", registry.ISF, "ISF: ");
			Assert.AreEqual(false, registry.ISF_IsProduction, "ISF_IsProduction: ");
			Assert.IsNotNull(registry.MAN);
			Assert.AreEqual("MAN_VALUE,MAN_VALUE_2", registry.MAN, "MAN: ");
			Assert.AreEqual(false, registry.MAN_IsProduction, "MAN_IsProduction: ");
			Assert.IsNotNull(registry.USE);
			Assert.AreEqual("USE_VALUE", registry.USE, "USE: ");
			Assert.AreEqual(false, registry.USE_IsProduction, "USE_IsProduction: ");
			Assert.IsNotNull(registry.USI);
			Assert.AreEqual("USI_VALUE", registry.USI, "USI: ");
			Assert.AreEqual(false, registry.USI_IsProduction, "USI_IsProduction: ");
			Assert.IsNotNull(registry.AMS);
			Assert.AreEqual("AMS_VALUE", registry.AMS, "AMS: ");
			Assert.AreEqual(false, registry.AMS_IsProduction, "AMS_IsProduction: ");
			Assert.IsNotNull(registry.AMA);
			Assert.AreEqual("AMA_VALUE", registry.AMA, "AMA: ");
			Assert.AreEqual(false, registry.AMA_IsProduction, "AMA_IsProduction: ");
			Assert.AreEqual("UEM_VALUE", registry.UEM);
			Assert.AreEqual(false, registry.UEM_IsProduction);
			Assert.IsNotNull(registry.USD);
			Assert.AreEqual("USD_VALUE", registry.USD.ER_Value, "USD.ER_Value: ");
			Assert.AreEqual(false, registry.USD.ER_IsProduction, "USD.ER_IsProduction: ");
		}

		[TestMethod]
		public void TestValues_MultipleFilter_AND()
		{
			var eHubClients = context.eHubClients.Where(c => c.eHubUSCustomsRegistry.Count > 0).ToList();

			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["sidx"] = "CC_ID";
			request.Container["sord"] = "desc";
			request.Container["filters"] = "{\"groupOp\":\"AND\",\"rules\":[{\"field\":\"CC_ID\",\"op\":\"ew\",\"data\":\"2\"},{\"field\":\"USE\",\"op\":\"bw\",\"data\":\"U\"}]}";

			var result = controller.Registry();

			Assert.IsNotNull(result);

			var registryList = result.Data.GetType().GetProperty("eHubUSCustomsRegistry").GetValue(result.Data, null) as IEnumerable<USCustomsRegistryFormattedData>;

			Assert.AreEqual(1, registryList.Count());
			var registry = registryList.FirstOrDefault();
			foreach (var client in eHubClients)
			{
				if (client.CC_ID == registry.CC_ID)
				{
					Assert.AreEqual(client.CC_ID, registry.CC_ID, "CC_ID mismatch");
					Assert.AreEqual(client.CC_FriendlyName, registry.CC_FriendlyName, "CC_FriendlyName mismatch");
					Assert.AreEqual(client.CC_USCustomsRecipient, registry.CC_USCustomsRecipient, "CC_USCustomsRecipient mismatch");
				}
			}
		}

	


		[TestMethod]
		public void TestValues_MultipleFilter_OR()
		{
			var eHubClients = context.eHubClients.Where(c => c.eHubUSCustomsRegistry.Count > 0).ToList();

			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["sidx"] = "MAN";
			request.Container["sord"] = "asc";
			request.Container["filters"] = "{\"groupOp\":\"OR\",\"rules\":[{\"field\":\"CC_ID\",\"op\":\"ew\",\"data\":\"2\"},{\"field\":\"CC_FriendlyName\",\"op\":\"ew\",\"data\":\"s\"}]}";

			var result = controller.Registry();

			Assert.IsNotNull(result);

			var registryList = result.Data.GetType().GetProperty("eHubUSCustomsRegistry").GetValue(result.Data, null) as IEnumerable<USCustomsRegistryFormattedData>;

			Assert.AreEqual(2, registryList.Count());
			var registry = registryList.FirstOrDefault();
			var registry2 = registryList.Last();
			foreach (var client in eHubClients)
			{
				if (client.CC_ID != registry.CC_ID)
				{
					registry = registry2;
				}
				Assert.AreEqual(client.CC_ID, registry.CC_ID, "CC_ID mismatch");
				Assert.AreEqual(client.CC_FriendlyName, registry.CC_FriendlyName, "CC_FriendlyName mismatch");
				Assert.AreEqual(client.CC_USCustomsRecipient, registry.CC_USCustomsRecipient, "CC_USCustomsRecipient mismatch");
			}
		}
	

		[TestMethod]
		public void TestRegistryEdit()
		{
			var logger = new TestLogger();
			var clientA = new TestEHubClient((Fakes.TestContext)context)
			{
				CC_PK = new Guid("{C48462E0-4D1C-4BEC-872E-C6A957007581}"),
				CC_ID = "CLIENT_USCUST_2",
				CC_FriendlyName = "Client US Customs 2",
				CC_AS2_Code = "CLIENT_USCUST_AS2_B",
				CC_IsAirServiceProvider = false
			};
			var ediProdClientA = new ediProdClient()
			{
				CC_ID = "CLIENT_USCUST_2",
				LD_LicenceType = "TST"
			};
			var clientB = new TestEHubClient((Fakes.TestContext)context)
			{
				CC_PK = new Guid("{D48462E0-4D1C-4BEC-872E-C6A957007582}"),
				CC_ID = "CLIENT_USCUST_3",
				CC_FriendlyName = "Client US Customs 3",
				CC_AS2_Code = "CLIENT_USCUST_AS2_C",
				CC_IsAirServiceProvider = false
			};
			var ediProdClientB = new ediProdClient()
			{
				CC_ID = "CLIENT_USCUST_3",
				LD_LicenceType = "PRD"
			};
			context.eHubClients.AddObject(clientA);
			context.eHubClients.AddObject(clientB);
			context.ediProdClients.AddObject(ediProdClientA);
			context.ediProdClients.AddObject(ediProdClientB);

			request.Container["oper"] = "add";
			request.Container["id"] = clientA.CC_ID;
			request.Container["CC_ID"] = clientA.CC_ID;
			request.Container["CC_USCustomsRecipient"] = "true";
			request.Container["USE"] = "USE_VALUE";
			request.Container["USE_IsProduction"] = "true";
			request.Container["USI"] = "USI_VALUE";
			request.Container["USI_IsProduction"] = "true";
			request.Container["ISF"] = "ISF_VALUE";
			request.Container["ISF_IsProduction"] = "true";
			request.Container["AMS"] = "AMS_VALUE,AMS_VALUE_3";
			request.Container["AMS_IsProduction"] = "true";
			request.Container["AMA"] = "AMA_VALUE,AMA_VALUE_3";
			request.Container["AMA_IsProduction"] = "true";
			request.Container["MAN"] = "MAN_VALUE";
			request.Container["MAN_IsProduction"] = "true";
			request.Container["UEM"] = "UEM_VALUE";
			request.Container["UEM_IsProduction"] = "true";

			Assert.IsFalse(clientA.eHubUSCustomsRegistry.Any());

			JsonResult responseAdd = RegistryEditTest(logger) as JsonResult;

			context.eHubUSCustomsRegistry.Count();

			var regsClientA = context.eHubUSCustomsRegistry.Where(r => r.ER_CC_Client == clientA.CC_PK).ToList();
			var usdClientA = regsClientA.Where(r => r.ER_ApplicationCode == "USD").First();

			Assert.AreEqual(10, clientA.eHubUSCustomsRegistry.Count);
			Assert.AreEqual(10, regsClientA.Count());
			Assert.AreEqual("Entry Filer Code", usdClientA.ER_Name);
			Assert.AreEqual("", usdClientA.ER_Value);
			Assert.AreEqual(true, usdClientA.ER_IsProduction, "add will use the value in the request");
			Assert.IsTrue(logger.Log.Contains(@"Info - [CORP\Test.Name] [add] eHubClient"));
			Assert.IsTrue(logger.Log.Contains(" CC_ID=CLIENT_USCUST_2, CC_FriendlyName=Client US Customs 2"));

			request.Container["oper"] = "edit";
			request.Container["CC_ID"] = clientA.CC_ID;
			request.Container["CC_USCustomsRecipient"] = "true";
			request.Container["USE"] = "USE_VALUE_2";
			request.Container["USE_IsProduction"] = "true";
			request.Container["USI"] = "USI_VALUE_2";
			request.Container["USI_IsProduction"] = "true";
			request.Container["ISF"] = "ISF_VALUE_2";
			request.Container["ISF_IsProduction"] = "true";
			request.Container["AMS"] = "AMS_VALUE_2";
			request.Container["AMS_IsProduction"] = "true";
			request.Container["AMA"] = "AMA_VALUE_2";
			request.Container["AMA_IsProduction"] = "true";
			request.Container["MAN"] = "MAN_VALUE_2";
			request.Container["MAN_IsProduction"] = "true";
			request.Container["UEM"] = "UEM_VALUE_2";
			request.Container["UEM_IsProduction"] = "true";

			RegistryEditTest(logger);

			regsClientA = context.eHubUSCustomsRegistry.Where(r => r.ER_CC_Client == clientA.CC_PK).ToList();
			usdClientA = regsClientA.Where(r => r.ER_ApplicationCode == "USD").First();

			clientA.eHubUSCustomsRegistry.ToList().ForEach(r =>
			{
				Assert.IsTrue(r.ER_Value.Contains("VALUE_2") || r.ER_ApplicationCode == "USD");
				Assert.IsFalse(r.ER_IsProduction ?? true, "edit will use the client's licence type and ignore the value in the request");
			});

			Assert.AreEqual(8, clientA.eHubUSCustomsRegistry.Count);
			Assert.AreEqual(8, regsClientA.Count());
			Assert.AreEqual("Entry Filer Code", usdClientA.ER_Name);
			Assert.AreEqual("", usdClientA.ER_Value);
			Assert.AreEqual(false, usdClientA.ER_IsProduction);
			Assert.IsTrue(logger.Log.Contains(@"Info - [CORP\Test.Name] [edit] eHubUSCustomsRegistry"));
			Assert.IsTrue(logger.Log.Contains("ER_ApplicationCode=MAN, ER_Name=Client Network ID, ER_IsProduction=False"));

			request.Container["oper"] = "edit";
			request.Container["CC_ID"] = clientA.CC_ID;
			request.Container["USE"] = "";
			request.Container["MAN"] = "MAN_VALUE_1,MAN_VALUE_5,MAN_VALUE_6";

			RegistryEditTest(logger);

			regsClientA = context.eHubUSCustomsRegistry.Where(r => r.ER_CC_Client == clientA.CC_PK).ToList();
			usdClientA = regsClientA.Where(r => r.ER_ApplicationCode == "USD").First();

			Assert.AreEqual(9, clientA.eHubUSCustomsRegistry.Count);
			Assert.AreEqual(9, regsClientA.Count());
			Assert.AreEqual("Entry Filer Code", usdClientA.ER_Name);
			Assert.AreEqual("", usdClientA.ER_Value);
			Assert.AreEqual(false, usdClientA.ER_IsProduction);
			Assert.IsTrue(logger.Log.Contains(@"Info - [CORP\Test.Name] [edit] eHubUSCustomsRegistry"));
			Assert.IsTrue(logger.Log.Contains("ER_ApplicationCode=MAN, ER_Name=Client Network ID, ER_IsProduction=False"));

			request.Container["oper"] = "edit";
			request.Container["id"] = clientA.CC_ID;
			request.Container["CC_ID"] = clientB.CC_ID;

			RegistryEditTest(logger);

			var regClientB = context.eHubUSCustomsRegistry.Where(r => r.ER_CC_Client == clientB.CC_PK);
			var usdClientB = regClientB.Where(r => r.ER_ApplicationCode == "USD").First();

			Assert.AreEqual(0, clientA.eHubUSCustomsRegistry.Count);
			Assert.AreEqual(0, context.eHubUSCustomsRegistry.Where(r => r.ER_CC_Client == clientA.CC_PK).Count());
			Assert.AreEqual(9, clientB.eHubUSCustomsRegistry.Count);
			Assert.AreEqual(9, regClientB.Count());
			Assert.AreEqual("Entry Filer Code", usdClientB.ER_Name);
			Assert.AreEqual("", usdClientB.ER_Value);
			Assert.AreEqual(true, usdClientB.ER_IsProduction);
			Assert.IsTrue(logger.Log.Contains(@"Info - [CORP\Test.Name] [edit] eHubUSCustomsRegistry"));
			Assert.IsTrue(logger.Log.Contains(" ER_CC_Client=d48462e0-4d1c-4bec-872e-c6a957007582, ER_ApplicationCode=MAN, ER_Name=Client Network ID, ER_IsProduction=True"));

			request.Container["oper"] = "del";
			request.Container["id"] = clientB.CC_ID;

			RegistryEditTest(logger);

			Assert.IsFalse(clientB.eHubUSCustomsRegistry.Any());
			Assert.IsTrue(logger.Log.Contains(@"Info - [CORP\Test.Name] [del] eHubClient"));
			Assert.IsTrue(logger.Log.Contains(" CC_ID=CLIENT_USCUST_3, CC_FriendlyName=Client US Customs 3, CC_USCustomsRecipient=, CC_Odyssey_OH=00000000-0000-0000-0000-000000000000, CC_DistributionZone=, CC_Password=, CC_IsAirServiceProvider=False, CC_AirlineCode=, CC_AirServiceProvider=, CC_AirlinePrefix=, CC_EmailAddress=, CC_AS2_Code=CLIENT_USCUST_AS2_C, CC_SCAC_Code=, CC_OwnerCategory=, CC_SystemCategory=, CC_RR=, CC_RequireStatusResponse=False, CC_NotificationForInboxRecipient=False"));
		}

		[TestMethod]
		public void TestRegistryEdit_DIS()
		{
			var client = new TestEHubClient((Fakes.TestContext)context)
			{
				CC_PK = new Guid("{C48462E0-4D1C-4BEC-872E-C6A957007581}"),
				CC_ID = "CLIENT_USCUST_1",
				CC_FriendlyName = "Client US Customs 1",
				CC_AS2_Code = "CLIENT_USCUST_AS2_B",
				CC_IsAirServiceProvider = false
			};
			var ediProdClient = new ediProdClient()
			{
				CC_ID = "CLIENT_USCUST_1",
				LD_LicenceType = "TST"
			};
			context.eHubClients.AddObject(client);
			context.ediProdClients.AddObject(ediProdClient);

			request.Container["oper"] = "add";
			request.Container["id"] = client.CC_ID;
			request.Container["CC_ID"] = client.CC_ID;
			request.Container["CC_USCustomsRecipient"] = "False";
			request.Container["USE"] = "USE_Value";
			request.Container["USE_IsProduction"] = "True";
			request.Container["USI"] = "USI_Value";
			request.Container["USI_IsProduction"] = "True";
			request.Container["MAN"] = "MAN_VALUE";
			request.Container["MAN_IsProduction"] = "True";

			controller.RegistryEdit();

			var regs = context.eHubUSCustomsRegistry.Where(r => r.ER_CC_Client == client.CC_PK);
			Assert.AreEqual(4, regs.Count());
			regs.ForEach(r =>
			{
				Assert.AreEqual(true, r.ER_IsProduction, "add will use the value in the request");
			});

			var usd = regs.Where(r => r.ER_ApplicationCode == "USD").First();
			Assert.AreEqual("Entry Filer Code", usd.ER_Name);
			Assert.AreEqual("", usd.ER_Value);

			request.Container["oper"] = "edit";
			request.Container["id"] = client.CC_ID;
			request.Container["CC_ID"] = client.CC_ID;
			request.Container["CC_USCustomsRecipient"] = "False";
			request.Container["USE"] = "";
			request.Container["USI"] = "";
			request.Container["ISF"] = "ISF_VALUE";
			request.Container["ISF_IsProduction"] = "True";

			controller.RegistryEdit();

			regs = context.eHubUSCustomsRegistry.Where(r => r.ER_CC_Client == client.CC_PK);
			Assert.AreEqual(3, regs.Count());
			regs.ForEach(r =>
			{
				Assert.AreEqual(false, r.ER_IsProduction, "edit will use the client's licence type and ignore the value in the request");
			});

			usd = regs.Where(r => r.ER_ApplicationCode == "USD").First();
			Assert.AreEqual("Entry Filer Code", usd.ER_Name);
			Assert.AreEqual("", usd.ER_Value);

			request.Container["oper"] = "edit";
			request.Container["id"] = client.CC_ID;
			request.Container["CC_ID"] = client.CC_ID;
			request.Container["CC_USCustomsRecipient"] = "False";
			request.Container["ISF"] = "";

			controller.RegistryEdit();

			regs = context.eHubUSCustomsRegistry.Where(r => r.ER_CC_Client == client.CC_PK);

			Assert.AreEqual(1, regs.Count(), "DIS will be removed because there is no USE or USI or ISF");
			Assert.AreEqual("MAN", regs.First().ER_ApplicationCode);
			Assert.AreEqual(false, regs.First().ER_IsProduction);
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
				new { CC_ID = "WTLPRDSV1", CC_FriendlyName = "Prod System 1" },
				new { CC_ID = "WTLTSTSV2", CC_FriendlyName = "Test System 2" },
			};

			var result1 = controller.Clients(true);

			Assert.IsNotNull(result1);
			AssertEx.JsonResultMatchesList(expected1, result1, "eHubClients", true);

			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["sidx"] = "CC_FriendlyName";
			request.Container["sord"] = "desc";
			request.Container["_search"] = "true";
			request.Container["CC_ID"] = "TEST";
			IList<object> expected2 = new List<object>
			{
				new { CC_ID = "WTLPRDSV1", CC_FriendlyName = "Prod System 1" },
				new { CC_ID = "WTLTSTSV2", CC_FriendlyName = "Test System 2" },
			};

			var result2 = controller.Clients(true);

			Assert.IsNotNull(result2);
			AssertEx.JsonResultMatchesList(expected2, result2, "eHubClients", true);

			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["sidx"] = "CC_FriendlyName";
			request.Container["sord"] = "asc";
			request.Container["_search"] = "true";
			request.Container["CC_ID"] = "TEST";
			request.Container["CC_FriendlyName"] = "2";
			IList<object> expected3 = new List<object>
			{
				new { CC_ID = "WTLTSTSV2", CC_FriendlyName = "Test System 2" },
			};

			var result3 = controller.Clients(true);

			Assert.IsNotNull(result2);
			AssertEx.JsonResultMatchesList(expected3, result3, "eHubClients", true);
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
				new { CC_ID = "WTLPRDSV1", CC_FriendlyName = "Prod System 1" },
			};

			var result1 = controller.Clients(false);

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
			request.Container["CC_ID"] = "wtlp";

			IList<object> expected1 = new List<object>
			{
				new { CC_ID = "WTLPRDSV1", CC_FriendlyName = "Prod System 1" },
			};

			var result1 = controller.Clients(true);

			Assert.IsNotNull(result1);
			AssertEx.JsonResultMatchesList(expected1, result1, "eHubClients");
		}


		[TestMethod]
		public void TestDoesEdiProdHavePRDLicence_GivenediProdHasPRDLicence_ShouldReturnTrue()
		{
			context.ediProdClients.AddObject(new ediProdClient { CC_ID = "ProdClient", LD_LicenceType = "PRD" });
			Assert.IsTrue(controller.DoesClientHavePRDLicence("ProdClient"));
		}

		[TestMethod]
		public void TestDoesEdiProdHavePRDLicence_GivenediProdDoesNotHavePRDLicence_ShouldReturnFalse()
		{
			context.ediProdClients.AddObject(new ediProdClient { CC_ID = "TestClient", LD_LicenceType = "TST" });
			Assert.IsFalse(controller.DoesClientHavePRDLicence("TestClient"));
		}

		[TestMethod]
		public void TestDoesEdiProdHavePRDLicence_GiveneHubIdNotInView_ShouldReturnFalse()
		{
			context.ediProdClients.AddObject(new ediProdClient { CC_ID = "ProdClient", LD_LicenceType = "PRD" });
			Assert.IsFalse(controller.DoesClientHavePRDLicence("ClientNotPresent"));
		}

		protected ActionResult RegistryEditTest(ILog logger)
		{
			controller.logger = logger;
			return controller.RegistryEdit();
		}
	}
}
