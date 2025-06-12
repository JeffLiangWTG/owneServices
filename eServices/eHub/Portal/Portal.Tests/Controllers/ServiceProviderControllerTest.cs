using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.eHub.Portal.Controllers;
using System.Web.Mvc;
using CargoWise.eHub.Portal.Tests.TestHelpers;
using CargoWise.eHub.Portal.Models.eHubTransactions;
using Common.Logging;
using Newtonsoft.Json;

namespace CargoWise.eHub.Portal.Tests.Controllers
{
	[TestClass]
	public class ServiceProviderControllerTest : BaseControllerTest<ServiceProviderController>
	{

		[TestMethod]
		public void TestIndex()
		{
			var context = TestContext();
			controller.Context = context;

			var result = controller.Index() as ViewResult;
			Assert.IsNotNull(result);
			Assert.AreEqual(string.Empty, result.ViewName, "Should be empty (Index)");
		}

		[TestMethod]
		public void TestGetColumnNames()
		{
			var context = TestContext();
			controller.Context = context;

			var result = controller.GetColumnNames();
			Assert.IsNotNull(result);

			IList<object> expected = new List<object> { "Provider", "SHIPPING_INSTRUCTION", "SHIPPING_PORT_MESSAGE" };

			AssertEx.JsonResultMatchesList(expected, result, "names");
		}

		[TestMethod]
		public void TestGetProviderList()
		{
			var context = TestContext();
			controller.Context = context;

			var result = controller.GetColumnNames();
			Assert.IsNotNull(result);

			IList<object> expected = new List<object> { "Provider", "SHIPPING_INSTRUCTION", "SHIPPING_PORT_MESSAGE" };

			AssertEx.JsonResultMatchesList(expected, result, "names");
		}

		[TestMethod]
		public void TestGetCW1Clients()
		{
			var context = TestContext();
			controller.Context = context;

			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["sidx"] = "Provider_CC_ID";
			request.Container["sord"] = "asc";
			request.Container["_search"] = "false";

			var result = controller.CW1Clients();
			Assert.IsNotNull(result);

			IList<object> expected = new List<object> {
				new {Provider_CC_ID = "TEST0001", Provider_CC_PK = Guid.Parse("F6EE53F7-02CD-41F2-84D8-EFC9C71FF798")},
				new {Provider_CC_ID = "TEST0002", Provider_CC_PK = Guid.Parse("6E926D81-2F51-446E-8D59-222D05266A8D")},
				new {Provider_CC_ID = "TEST0003", Provider_CC_PK = Guid.Parse("97286C5B-5607-4B23-A079-CB3D0366F790")}
			};

			AssertEx.JsonResultMatchesList(expected, result, "serviceProvider");
		}

		[TestMethod]
		public void TestServiceProviderAdd()
		{
            var logger = new TestLogger();
            var context = TestContext();
			controller.Context = context;

			request.Clear();
			request.Container["oper"] = "add";
			request.Container["Provider"] = "TEST0001";
			request.Container["SHIPPING_INSTRUCTION"] = "on";
			request.Container["SHIPPING_PORT_MESSAGE"] = "off";

			var service = context.eHubServiceProviders.Where(s => s.SP_CC_Provider == Guid.Parse("F6EE53F7-02CD-41F2-84D8-EFC9C71FF798")).FirstOrDefault();
			Assert.IsNull(service);

			var result = ServiceProviderEditTest(logger);

			Assert.IsTrue(bool.Parse(result.Data.GetType().GetProperty("success").GetValue(result.Data, null).ToString()));

			var services = context.eHubServiceProviders.Where(s => s.SP_CC_Provider == Guid.Parse("F6EE53F7-02CD-41F2-84D8-EFC9C71FF798"));
			var serviceShippingInstruction = context.eHubServiceProviders.Where(s => s.SP_CC_Provider == Guid.Parse("F6EE53F7-02CD-41F2-84D8-EFC9C71FF798") && s.SP_CC_Service == Guid.Parse("FF315BDD-AD3D-439B-9F92-67025DC05921")).FirstOrDefault();
			var serviceShippingPortMessage = context.eHubServiceProviders.Where(s => s.SP_CC_Provider == Guid.Parse("F6EE53F7-02CD-41F2-84D8-EFC9C71FF798") && s.SP_CC_Service == Guid.Parse("2FAF9E90-CE5E-43D4-B1A5-01859C73F58A")).FirstOrDefault();
			Assert.AreEqual(1, services.Count());
			Assert.IsNotNull(serviceShippingInstruction);
			Assert.IsNull(serviceShippingPortMessage);
            Assert.IsTrue(logger.Log.Contains("Info - [add] eHubServiceProvider:"));
            Assert.IsTrue(logger.Log.Contains("SP_CC_Service=ff315bdd-ad3d-439b-9f92-67025dc05921, SP_CC_Provider=f6ee53f7-02cd-41f2-84d8-efc9c71ff798, SP_RR="));
        }

		[TestMethod]
		public void TestServiceProviderEdit()
		{
            var logger = new TestLogger();
            var context = TestContext();
			var providedService = new eHubServiceProvider() { SP_CC_Provider = Guid.Parse("F6EE53F7-02CD-41F2-84D8-EFC9C71FF798"), SP_CC_Service = Guid.Parse("FF315BDD-AD3D-439B-9F92-67025DC05921") };
			context.eHubServiceProviders.AddObject(providedService);
			controller.Context = context;

			request.Clear();
			request.Container["oper"] = "edit";
			request.Container["Provider"] = "TEST0001";
			request.Container["id"] = "TEST0001";
			request.Container["SHIPPING_INSTRUCTION"] = "off";
			request.Container["SHIPPING_PORT_MESSAGE"] = "on";

			var service = context.eHubServiceProviders.Where(s => s.SP_CC_Provider == Guid.Parse("F6EE53F7-02CD-41F2-84D8-EFC9C71FF798") && s.SP_CC_Service == Guid.Parse("FF315BDD-AD3D-439B-9F92-67025DC05921")).FirstOrDefault();
			Assert.IsNotNull(service);

			var result = ServiceProviderEditTest(logger);

			Assert.IsTrue(bool.Parse(result.Data.GetType().GetProperty("success").GetValue(result.Data, null).ToString()));

			var services = context.eHubServiceProviders.Where(s => s.SP_CC_Provider == Guid.Parse("F6EE53F7-02CD-41F2-84D8-EFC9C71FF798"));
			var serviceShippingInstruction = context.eHubServiceProviders.Where(s => s.SP_CC_Provider == Guid.Parse("F6EE53F7-02CD-41F2-84D8-EFC9C71FF798") && s.SP_CC_Service == Guid.Parse("FF315BDD-AD3D-439B-9F92-67025DC05921")).FirstOrDefault();
			var serviceShippingPortMessage = context.eHubServiceProviders.Where(s => s.SP_CC_Provider == Guid.Parse("F6EE53F7-02CD-41F2-84D8-EFC9C71FF798") && s.SP_CC_Service == Guid.Parse("2FAF9E90-CE5E-43D4-B1A5-01859C73F58A")).FirstOrDefault();
			Assert.AreEqual(1, services.Count());
			Assert.IsNull(serviceShippingInstruction);
			Assert.IsNotNull(serviceShippingPortMessage);
            Assert.IsTrue(logger.Log.Contains("Info - [add] eHubServiceProvider:"));
            Assert.IsTrue(logger.Log.Contains("SP_CC_Service=2faf9e90-ce5e-43d4-b1a5-01859c73f58a, SP_CC_Provider=f6ee53f7-02cd-41f2-84d8-efc9c71ff798, SP_RR="));
        }

		[TestMethod]
		public void TestServiceProviderDelete()
		{
            var logger = new TestLogger();
            var context = TestContext();
			var providedService = new eHubServiceProvider() { SP_CC_Provider = Guid.Parse("6A3430C9-5D64-48F9-9045-4A2D8072BD99"), SP_CC_Service = Guid.Parse("FF315BDD-AD3D-439B-9F92-67025DC05921") };
			var providedService2 = new eHubServiceProvider() { SP_CC_Provider = Guid.Parse("6A3430C9-5D64-48F9-9045-4A2D8072BD99"), SP_CC_Service = Guid.Parse("2FAF9E90-CE5E-43D4-B1A5-01859C73F58A") };
			context.eHubServiceProviders.AddObject(providedService);
			context.eHubServiceProviders.AddObject(providedService2);
			controller.Context = context;

			request.Clear();
			request.Container["oper"] = "del";
			request.Container["id"] = "INTTRA_SI";

			var service = context.eHubServiceProviders.Where(s => s.SP_CC_Provider == Guid.Parse("6A3430C9-5D64-48F9-9045-4A2D8072BD99") && s.SP_CC_Service == Guid.Parse("FF315BDD-AD3D-439B-9F92-67025DC05921")).FirstOrDefault();
			Assert.IsNotNull(service);
			service = context.eHubServiceProviders.Where(s => s.SP_CC_Provider == Guid.Parse("6A3430C9-5D64-48F9-9045-4A2D8072BD99") && s.SP_CC_Service == Guid.Parse("2FAF9E90-CE5E-43D4-B1A5-01859C73F58A")).FirstOrDefault();
			Assert.IsNotNull(service);

			var result = ServiceProviderEditTest(logger);

			Assert.IsTrue(bool.Parse(result.Data.GetType().GetProperty("success").GetValue(result.Data, null).ToString()));

			var services = context.eHubServiceProviders.Where(s => s.SP_CC_Provider == Guid.Parse("6A3430C9-5D64-48F9-9045-4A2D8072BD99"));
			var serviceShippingInstruction = context.eHubServiceProviders.Where(s => s.SP_CC_Provider == Guid.Parse("6A3430C9-5D64-48F9-9045-4A2D8072BD99") && s.SP_CC_Service == Guid.Parse("FF315BDD-AD3D-439B-9F92-67025DC05921")).FirstOrDefault();
			var serviceShippingPortMessage = context.eHubServiceProviders.Where(s => s.SP_CC_Provider == Guid.Parse("6A3430C9-5D64-48F9-9045-4A2D8072BD99") && s.SP_CC_Service == Guid.Parse("2FAF9E90-CE5E-43D4-B1A5-01859C73F58A")).FirstOrDefault();
			Assert.AreEqual(0, services.Count());
			Assert.IsNull(serviceShippingInstruction);
			Assert.IsNull(serviceShippingPortMessage);
            Assert.IsTrue(logger.Log.Contains("Info - [del] eHubServiceProvider: SP_PK=00000000-0000-0000-0000-000000000000, SP_CC_Service=2faf9e90-ce5e-43d4-b1a5-01859c73f58a, SP_CC_Provider=6a3430c9-5d64-48f9-9045-4a2d8072bd99, SP_RR="));
        }

		[TestMethod]
		public void TestGetProvidersMultipleFiltersWithANDCondition()
		{
			var context = TestContext();
			controller.Context = context;

			request.Clear();
			request.Container["rows"] = "15";
			request.Container["_search"] = "false";
			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[" +
					"{ \"field\":\"Provider\",\"op\":\"bw\",\"data\":\"T\"}," +
					"{ \"field\":\"Provider\",\"op\":\"ew\",\"data\":\"4\"}]}";
			var expected = new {
				page = 1,
				total = 1.0,
				records = 2,
				Services = new List<object> { new { Provider = "TEST0004", SHIPPING_INSTRUCTION = 0, SHIPPING_PORT_MESSAGE = 0 } },
			};

			var result = controller.GetProvidersWithServices("");
			Assert.AreEqual(JsonConvert.SerializeObject(expected), result.Content);
		}

		[TestMethod]
		public void TestGetProvidersMultipleFiltersWithORCondition()
		{
			var context = TestContext();
			controller.Context = context;

			request.Clear();
			request.Container["rows"] = "15";
			request.Container["_search"] = "false";
			request.Container["filters"] = "{ \"groupOp\":\"OR\",\"rules\":[" +
					"{ \"field\":\"Provider\",\"op\":\"bw\",\"data\":\"T\"}," +
					"{ \"field\":\"Provider\",\"op\":\"ew\",\"data\":\"4\"}]}";

			var expected = new {
				page = 1,
				total = 1.0,
				records = 2,
				Services = new List<object> {
					new { Provider = "TEST0004", SHIPPING_INSTRUCTION = 0, SHIPPING_PORT_MESSAGE = 0 },
					new { Provider = "TEST0005", SHIPPING_INSTRUCTION = 1, SHIPPING_PORT_MESSAGE = 0 }
				},
			};

			var result = controller.GetProvidersWithServices("");
			Assert.AreEqual(JsonConvert.SerializeObject(expected), result.Content);
		}

		[TestMethod]
		public void TestGetProvidersWithSortByAsc()
		{
			var context = TestContext();
			controller.Context = context;

			request.Clear();
			request.Container["rows"] = "15";
			request.Container["_search"] = "false";
			request.Container["sidx"] = "Provider";
			request.Container["sord"] = "asc";

			var expected = new
			{
				page = 1,
				total = 1.0,
				records = 2,
				Services = new List<object> {
					new { Provider = "TEST0004", SHIPPING_INSTRUCTION = 0, SHIPPING_PORT_MESSAGE = 0 },
					new { Provider = "TEST0005", SHIPPING_INSTRUCTION = 1, SHIPPING_PORT_MESSAGE = 0 }
				},
			};

			var result = controller.GetProvidersWithServices("");
			Assert.AreEqual(JsonConvert.SerializeObject(expected), result.Content);
		}

		[TestMethod]
		public void TestGetProvidersWithSortByDesc()
		{
			var context = TestContext();
			controller.Context = context;

			request.Clear();
			request.Container["rows"] = "15";
			request.Container["_search"] = "false";
			request.Container["sidx"] = "Provider";
			request.Container["sord"] = "desc";

			var expected = new
			{
				page = 1,
				total = 1.0,
				records = 2,
				Services = new List<object> {
					new { Provider = "TEST0005", SHIPPING_INSTRUCTION = 1, SHIPPING_PORT_MESSAGE = 0 },
					new { Provider = "TEST0004", SHIPPING_INSTRUCTION = 0, SHIPPING_PORT_MESSAGE = 0 },
				},
			};

			var result = controller.GetProvidersWithServices("");
			Assert.AreEqual(JsonConvert.SerializeObject(expected), result.Content);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void TestGetProvidersWithInvalidSortOrder()
		{
			var context = TestContext();
			controller.Context = context;

			request.Clear();
			request.Container["rows"] = "15";
			request.Container["_search"] = "false";
			request.Container["sidx"] = "Provider";
			request.Container["sord"] = "alphabetical";

			controller.GetProvidersWithServices("");
		}

		[TestMethod]
		public void TestGetProvidersWithProviderFromUrl()
		{
			var context = TestContext();
			controller.Context = context;

			request.Clear();
			request.Container["rows"] = "15";
			request.Container["_search"] = "false";

			var expected = new
			{
				page = 1,
				total = 1.0,
				records = 2,
				Services = new List<object> {
					new { Provider = "providerFromUrl", SHIPPING_INSTRUCTION = 0, SHIPPING_PORT_MESSAGE = 0 },
				},
			};

			var result = controller.GetProvidersWithServices("providerFromUrl");
			Assert.AreEqual(JsonConvert.SerializeObject(expected), result.Content);
		}

		[TestMethod]
		public void TestServiceIsUsedAsRoutingRule()
		{
            var logger = new TestLogger();
            var context = TestContext();
			var routingRule = new eHubRoutingRule() { RR_PK = Guid.Parse("71B1A83A-C91C-4244-9048-20FE7EA8BF8E") };
			context.eHubRoutingRules.AddObject(routingRule);
			var providedService = new eHubServiceProvider() { SP_CC_Provider = Guid.Parse("6A3430C9-5D64-48F9-9045-4A2D8072BD99"), SP_CC_Service = Guid.Parse("FF315BDD-AD3D-439B-9F92-67025DC05921"), SP_RR = Guid.Parse("71B1A83A-C91C-4244-9048-20FE7EA8BF8E") };
			context.eHubServiceProviders.AddObject(providedService);
			controller.Context = context;

			request.Clear();
			request.Container["oper"] = "del";
			request.Container["id"] = "INTTRA_SI";

			var service = context.eHubServiceProviders.Where(s => s.SP_CC_Provider == Guid.Parse("6A3430C9-5D64-48F9-9045-4A2D8072BD99") && s.SP_CC_Service == Guid.Parse("FF315BDD-AD3D-439B-9F92-67025DC05921")).FirstOrDefault();
			Assert.IsNotNull(service);

			var result = ServiceProviderEditTest(logger);

			Assert.IsFalse(bool.Parse(result.Data.GetType().GetProperty("success").GetValue(result.Data, null).ToString()));
			var message = "Cannot delete a service which is currently used in a routing rule";
			Assert.AreEqual(message, result.Data.GetType().GetProperty("message").GetValue(result.Data, null).ToString());

			var services = context.eHubServiceProviders.Where(s => s.SP_CC_Provider == Guid.Parse("6A3430C9-5D64-48F9-9045-4A2D8072BD99"));
			var serviceShippingInstruction = context.eHubServiceProviders.Where(s => s.SP_CC_Provider == Guid.Parse("6A3430C9-5D64-48F9-9045-4A2D8072BD99") && s.SP_CC_Service == Guid.Parse("FF315BDD-AD3D-439B-9F92-67025DC05921")).FirstOrDefault();
			Assert.AreEqual(1, services.Count());
			Assert.IsNotNull(serviceShippingInstruction);
            Assert.IsTrue(string.IsNullOrEmpty(logger.Log));
        }

		CargoWise.eHub.Portal.Tests.Fakes.TestContext TestContext()
		{
			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			var clientInttra = new eHubClient() { CC_PK = Guid.Parse("6A3430C9-5D64-48F9-9045-4A2D8072BD99"), CC_ID = "INTTRA_SI", CC_OwnerCategory = "Service Provider" };
			var clientGtnexus = new eHubClient() { CC_PK = Guid.Parse("291AB11B-8492-45B4-92F8-8FE6BBAC1B70"), CC_ID = "GTNEXUS", CC_OwnerCategory = "Service Provider" };
			var clientInttra1 = new eHubClient() { CC_PK = Guid.Parse("F4504B08-0BFE-48CD-8BD4-C4C507D1F042"), CC_ID = "1STOPCSYD_SCH", CC_FriendlyName = "1-Stop Connection", CC_OwnerCategory = "Service Provider" };
			var clientInttra2 = new eHubClient() { CC_PK = Guid.Parse("0653D162-DD02-4333-8939-DE29B234BEDE"), CC_ID = "2ACNVENVE", CC_FriendlyName = "2 Achieve B.V", CC_OwnerCategory = "Service Provider" };
			var clientServiceSI = new eHubClient() { CC_PK = Guid.Parse("FF315BDD-AD3D-439B-9F92-67025DC05921"), CC_ID = "SHIPPING_INSTRUCTION", CC_FriendlyName = "Shipping Instruction", CC_OwnerCategory = "Service" };
			var clientServiceSPM = new eHubClient() { CC_PK = Guid.Parse("2FAF9E90-CE5E-43D4-B1A5-01859C73F58A"), CC_ID = "SHIPPING_PORT_MESSAGE", CC_FriendlyName = "Shipping Port Message", CC_OwnerCategory = "Service" };
			var client1 = new eHubClient() { CC_PK = Guid.Parse("F6EE53F7-02CD-41F2-84D8-EFC9C71FF798"), CC_ID = "TEST0001", CC_FriendlyName = "Test Client 1", CC_OwnerCategory = "Enterprise" };
			var client2 = new eHubClient() { CC_PK = Guid.Parse("6E926D81-2F51-446E-8D59-222D05266A8D"), CC_ID = "TEST0002", CC_FriendlyName = "Test Client 2", CC_OwnerCategory = "Thrid Party" };
			var client3 = new eHubClient() { CC_PK = Guid.Parse("97286C5B-5607-4B23-A079-CB3D0366F790"), CC_ID = "TEST0003", CC_FriendlyName = "Test Client 3", CC_OwnerCategory = "Enterprise" };
			var client4 = new eHubClient() { CC_PK = Guid.Parse("97286C5B-5607-4B23-A079-CB3D0366F790"), CC_ID = "TEST0004", CC_FriendlyName = "Test Client 3", CC_OwnerCategory = "New Enterprise" };
			var client5 = new eHubClient() { CC_PK = Guid.Parse("FF315BDD-AD3D-439B-9F92-67025DC05921"), CC_ID = "TEST0005", CC_FriendlyName = "New Test Client 3", CC_OwnerCategory = "New Enterprise" };
			var providedService = new eHubServiceProvider() { eHubClient = client4, SP_CC_Service = Guid.Parse("97286C5B-5607-4B23-A079-CB3D0366F790") };
			var providedService2 = new eHubServiceProvider() { eHubClient = client5, SP_CC_Service = Guid.Parse("FF315BDD-AD3D-439B-9F92-67025DC05921") };

			context.eHubClients.AddObject(clientInttra);
			context.eHubClients.AddObject(clientGtnexus);
			context.eHubClients.AddObject(clientInttra1);
			context.eHubClients.AddObject(clientInttra2);
			context.eHubClients.AddObject(clientServiceSI);
			context.eHubClients.AddObject(clientServiceSPM);
			context.eHubClients.AddObject(client1);
			context.eHubClients.AddObject(client2);
			context.eHubClients.AddObject(client3);
			context.eHubServiceProviders.AddObject(providedService);
			context.eHubServiceProviders.AddObject(providedService2);

			return context;
		}

        protected JsonResult ServiceProviderEditTest(ILog logger)
        {
            controller.logger = logger;
            return controller.ServiceProviderEdit();
        }

    }
}
