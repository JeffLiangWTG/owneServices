extern alias DataModelShared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using CargoWise.eHub.Portal.Controllers.Service;
using CargoWise.eHub.Portal.Helpers;
using CargoWise.eHub.Portal.Models.View.OceanCarrierMessaging;
using CargoWise.eHub.Portal.Tests.Model;
using CargoWise.eHub.Portal.Tests.TestHelpers;
using Common.Logging;
using Common.Logging.Configuration;
using Common.Logging.Simple;
using DataModelShared::eServices.eHubDataModel.Common;
using eServices.eHubDataModel.eHubTransactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Portal.Tests.Controllers
{
	[TestClass]
	public class OceanCarrierMessagingControllerTest
	{
		static TestRequest request;
		static OceanCarrierMessagingController controller;
		static eHubTransactionsContext context;

        static TestDbSet<eHubRoutingRule> testRules = new TestDbSet<eHubRoutingRule>();
		static TestDbSet<eHubRoutingRuleFact> testFacts = new TestDbSet<eHubRoutingRuleFact>();
		static eHubClient clientService = new eHubClient() { CC_ID = "SHIPPING_INSTRUCTION", CC_FriendlyName = "Shipping Instruction" };
		static eHubClient clientInttra = new eHubClient() { CC_ID = "INTTRA" };
		static eHubClient clientGtnexus = new eHubClient() { CC_ID = "GTNEXUS" };
		static eHubClient clientInttra1 = new eHubClient() { CC_ID = "1STOPCSYD_SCH", CC_FriendlyName = "1-Stop Connection" };
		static eHubClient clientInttra2 = new eHubClient() { CC_ID = "2ACNVENVE", CC_FriendlyName = "2 Achieve B.V" };
		static eHubClient clientSubShipment = new eHubClient() { CC_ID = "OCM_SUBSHIPMENT_SPLIT"};
		static eHubClient clientContainer = new eHubClient() { CC_ID = "OCM_CONTAINER_SPLIT" };

		static eHubClient clientProd = new eHubClient() { CC_PK = new Guid("0A6C707C-87AA-4241-8CA5-6DD2BBEE1BAB"), CC_ID = "WTLPRDSV1" };
		static eHubClient clientNonProd = new eHubClient() { CC_PK = new Guid("323A7F4F-9C1C-45FF-BEB0-A69BC82013A5"), CC_ID = "WTLTSTSV2" };

		static ediProdClient ediClientProd = new ediProdClient() {CC_PK = new Guid("0A6C707C-87AA-4241-8CA5-6DD2BBEE1BAB"), CC_ID = "WTLPRDSV1", EnterpriseServerCode = "WTLSV1", LD_LicenceType = "PRD", LD_ServerCode = "SV1", LE_EnterpriseCode = "WTL"};
		static ediProdClient ediClientNonProd = new ediProdClient() { CC_PK = new Guid("323A7F4F-9C1C-45FF-BEB0-A69BC82013A5"), CC_ID = "WTLTSTSV2", EnterpriseServerCode = "WTLSV2", LD_LicenceType = "TST", LD_ServerCode = "SV2", LE_EnterpriseCode = "WTL" };

		static eHubServiceProvider providerInttra = new eHubServiceProvider { eHubClient_Service = clientService, eHubClient_Provider = clientInttra };
		static eHubServiceProvider providerGtnexus = new eHubServiceProvider { eHubClient_Service = clientService, eHubClient_Provider = clientGtnexus };

		private readonly IOcmConfigBackupManager _backupManager = MockRepository.GenerateMock<IOcmConfigBackupManager>();

		[ClassInitialize()]
		public static void ClassInit(TestContext testContext)
		{
            request = new TestRequest();

            var fakeIdentity = new GenericIdentity("CORP\\Test.Name");
            var principal = new GenericPrincipal(fakeIdentity, null);

            var serverMock = MockRepository.GenerateMock<HttpServerUtilityBase>();
            var httpContext = MockRepository.GenerateMock<HttpContextBase>();
			httpContext.Stub(x => x.Request).Return(request);

			httpContext.Stub(c => c.Session).Return(new FakeSessionState());
			httpContext.Stub(c => c.Response).Return(new FakeResponseState());
            httpContext.Stub(x => x.User).Return(principal);
            httpContext.Stub(c => c.Server).Return(serverMock);

            var routeData = new RouteData();
			context = MockRepository.GenerateMock<eHubTransactionsContext>();

			var testFacts = new TestDbSet<eHubRoutingRuleFact>();
			var testClients = new TestDbSet<eHubClient>();
			var testediProdClients = new TestDbSet<ediProdClient>();
			context.Stub(x => x.eHubRoutingRules).Return(testRules);
			context.Stub(x => x.eHubRoutingRuleFacts).Return(testFacts);
			context.Stub(x => x.eHubClients).Return(testClients);
			context.Stub(x => x.ediProdClients).Return(testediProdClients);

			var clientTestSender = new eHubClient() { CC_ID = "TESTSENDER__1", CC_FriendlyName = "Test Sender" };

			clientService.eHubServiceProviders_Service.Add(providerInttra);
			clientService.eHubServiceProviders_Service.Add(providerGtnexus);

			testClients.Add(clientInttra);
			testClients.Add(clientGtnexus);
			testClients.Add(clientInttra1);
			testClients.Add(clientInttra2);
			testClients.Add(clientService);
			testClients.Add(clientSubShipment);
			testClients.Add(clientContainer);
			testClients.Add(new eHubClient() { CC_ID = "TEST0001", CC_FriendlyName = "Test Client 1" });
			testClients.Add(new eHubClient() { CC_ID = "TEST0002", CC_FriendlyName = "Test Client 2" });
			testClients.Add(new eHubClient() { CC_ID = "TEST0003", CC_FriendlyName = "Test Client 3" });
			testClients.Add(clientTestSender);
			testClients.Add(clientNonProd);
			testClients.Add(clientProd);
			testediProdClients.Add(ediClientNonProd);
			testediProdClients.Add(ediClientProd);

			controller = new OceanCarrierMessagingController(context);
			controller.ControllerContext = new ControllerContext(httpContext, routeData, controller);
		}

		[TestInitialize()]
		public void Initialize()
		{
			controller.Session.Clear();
			SetupRoutingRules();
			controller.ConfigBackupManager = _backupManager;
		}

        private static string CaptureConsoleLog(Action callback)
        {
            using (var sw = new StringWriter())
            {
                Console.SetOut(sw);
                callback();
                return sw.ToString();
            }
        }

        private static void SetupRoutingRules()
		{
			clientService.eHubRoutingRule = testRules.Add(new eHubRoutingRule
			{
				RR_Group_MatchMultiple = false,
				eHubRoutingRules_Group = testRules.AddRange(new[] {
					new eHubRoutingRule {
						RR_Group_MatchMultiple = false,
						RR_Group_Ordering = 1,
						RR_Group_Name = "Test",
						eHubRoutingRules_Group = testRules.AddRange(new[]
						{
							new eHubRoutingRule {  RR_Condition_Expression = "[@SourceParty,Equal,DUMMY]", eHubClient_Recipient = clientInttra1, RR_Group_Ordering = 100}
						}).ToList()
					},
					new eHubRoutingRule {
						RR_Group_MatchMultiple = false,
						RR_Group_Ordering = 100,
						RR_Group_Name = "SplitingBlackList",
					},
					new eHubRoutingRule {
						RR_Group_MatchMultiple = false,
						RR_Group_Ordering = 200,
						RR_Group_Name = "SplitingWhiteList",
					},
					new eHubRoutingRule {
						RR_Group_MatchMultiple = false,
						RR_Group_Ordering = 1000,
						RR_Group_Name = "CarrierHandlingAgent",
						eHubRoutingRules_Group = testRules.AddRange(new[]
						{
							new eHubRoutingRule { RR_Condition_Expression = "[@SourceParty,IsMatch,^TEST.*$]&&[@SCAC,Equal,AAAA]&&[@DocumentName,Equal,Shipping Instruction]", eHubServiceProvider = providerInttra, RR_Group_Ordering = 1000 }
						}).ToList()
					},
					new eHubRoutingRule {
						RR_Group_MatchMultiple = false,
						RR_Group_Ordering = 2000,
						RR_Group_Name = "CarrierBookingAgent",
						eHubRoutingRules_Group = testRules.AddRange(new[]
						{
							new eHubRoutingRule { RR_Condition_Expression = "[@SourceParty,Equal,TESTSENDER__1]", eHubServiceProvider = providerInttra, RR_Group_Ordering = 1000 }
						}).ToList()
					},
					new eHubRoutingRule {
						RR_Group_MatchMultiple = false,
						RR_Group_Ordering = 3000,
						RR_Group_Name = "DefaultCarrier",
						eHubRoutingRules_Group = testRules.AddRange(new[]
						{
							new eHubRoutingRule { RR_Condition_Expression = "[@SourceParty,Equal,TESTSENDER__1]", eHubServiceProvider = providerInttra, RR_Group_Ordering = 1000 },
							new eHubRoutingRule { RR_Condition_Expression = "[@SCAC,Equal,AAAA]&&[@NVOCC,Equal,N]", eHubServiceProvider = providerInttra, RR_Group_Ordering = 2000 },
							new eHubRoutingRule { RR_Condition_Expression = "[@SourceParty,Equal,TESTSENDER__1]&&[@SCAC,Equal,AAAA]&&[@NVOCC,Equal,Y]", eHubServiceProvider = providerInttra, RR_Group_Ordering = 3000 },
							new eHubRoutingRule { RR_Condition_Expression = "[@SourceParty,IsMatch,^TEST.*$]&&[@SCAC,Equal,AAAA]&&[@DocumentName,Equal,Shipping Instruction]", eHubServiceProvider = providerInttra, RR_Group_Ordering = 4000 }
						}).ToList()
					}
				}).ToList(),
				eHubRoutingRule_Failed = testRules.Add(new eHubRoutingRule { RR_Failed_ErrorCode = "IRJ", RR_Failed_ErrorDescription = "Shipping Instruction Withdrawal|not supported by carrier, contact carrier direct" }),
				eHubRoutingRuleFacts = new List<eHubRoutingRuleFact>
				{
					testFacts.Add(new eHubRoutingRuleFact { RX_Name = "@SourceParty", RX_Type = "PROPERTY", RX_Query = "http://schemas.microsoft.com/BizTalk/2003/system-properties#SourceParty" }),
					testFacts.Add(new eHubRoutingRuleFact { RX_Name = "@SCAC", RX_Type = "XPATH", RX_Query = "//s0:TransportLegCollection/s0:TransportLeg[s0:LegType/text()='Main']/s0:Carrier/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/text()='CCC']/s0:Value/text()" }),
				}
			});
		}

		private class FakeSessionState : HttpSessionStateBase
		{
			readonly Dictionary<string, object> items = new Dictionary<string, object>();
			public override object this[string name]
			{
				get { return items.ContainsKey(name) ? items[name] : null; }
				set { items[name] = value; }
			}

			public override void Clear()
			{
				items.Clear();
			}
		}

		private class FakeResponseState : HttpResponseBase
		{
			string result = "";
			public override void Write(char ch)
			{
				result = ch + "";
			}
			public override void Write(string str)
			{
				result = str;
			}
			public override string ToString()
			{
				return result;
			}
		}

		[TestMethod]
		public void TestIndex()
		{
			ViewResult result = controller.Index() as ViewResult;
			Assert.IsNotNull(result);
			OCMIndexModel model = result.Model as OCMIndexModel;
			Assert.IsNotNull(model);
			Assert.AreEqual("DefaultCarrier", model.Selected);
			Assert.AreEqual("SplitingBlackList", model.Options[0].OptionID);
			Assert.AreEqual("Spliting Black List", model.Options[0].Description);
			Assert.AreEqual("SplitingWhiteList", model.Options[1].OptionID);
			Assert.AreEqual("Spliting White List", model.Options[1].Description);
			Assert.AreEqual("MultipleRecipientsCopying", model.Options[2].OptionID);
			Assert.AreEqual("Multiple Recipients Copying", model.Options[2].Description);
			Assert.AreEqual("CarrierHandlingAgent", model.Options[3].OptionID);
			Assert.AreEqual("1st Phase - Carrier Handling Agent", model.Options[3].Description);
			Assert.AreEqual("CarrierBookingAgent", model.Options[4].OptionID);
			Assert.AreEqual("2nd Phase - Carrier Booking Agent", model.Options[4].Description);
			Assert.AreEqual("DefaultCarrier", model.Options[5].OptionID);
			Assert.AreEqual("Default Phase - Default Carrier", model.Options[5].Description);
			Assert.AreEqual("DefaultCarrier", controller.Session["OCMTypeID"]);
		}

		[TestMethod]
		public void TestConstructor()
		{
			Assert.IsNotNull(new OceanCarrierMessagingController());
		}

		[TestMethod]
		public void TestSelectType()
		{
			request.Clear();
			var result = controller.SelectType();
			Assert.AreEqual("DefaultCarrier", controller.Session["OCMTypeID"]);
			AssertEx.JsonResultMatchesList(new[] { Row.RowNames.client, Row.RowNames.carrier, Row.RowNames.eventBranch, Row.RowNames.port, Row.RowNames.shpType, Row.RowNames.docName, Row.RowNames.provider }, result, "rows");

			request.Clear();
			request.Container["id"] = "CarrierHandlingAgent";

			result = controller.SelectType();
			Assert.AreEqual("CarrierHandlingAgent", controller.Session["OCMTypeID"]);
			AssertEx.JsonResultMatchesList(new[] { Row.RowNames.client, Row.RowNames.carrier, Row.RowNames.carrierAgent, Row.RowNames.eventBranch, Row.RowNames.port, Row.RowNames.shpType, Row.RowNames.docName, Row.RowNames.provider }, result, "rows");

			request.Clear();
			request.Container["id"] = "CarrierBookingAgent";

			result = controller.SelectType();
			Assert.AreEqual("CarrierBookingAgent", controller.Session["OCMTypeID"]);
			AssertEx.JsonResultMatchesList(new[] { Row.RowNames.client, Row.RowNames.carrier, Row.RowNames.carrierAgent, Row.RowNames.eventBranch, Row.RowNames.port, Row.RowNames.shpType, Row.RowNames.docName, Row.RowNames.provider }, result, "rows");

			request.Clear();
			request.Container["id"] = "SplitingBlackList";

			result = controller.SelectType();
			Assert.AreEqual("SplitingBlackList", controller.Session["OCMTypeID"]);
			AssertEx.JsonResultMatchesList(new[] { Row.RowNames.client, Row.RowNames.purpose, Row.RowNames.port, Row.RowNames.docName, Row.RowNames.shipNamespace }, result, "rows");

			request.Clear();
			request.Container["id"] = "SplitingWhiteList";

			result = controller.SelectType();
			Assert.AreEqual("SplitingWhiteList", controller.Session["OCMTypeID"]);
			AssertEx.JsonResultMatchesList(new[] { Row.RowNames.client, Row.RowNames.purpose, Row.RowNames.port, Row.RowNames.docName, Row.RowNames.shipNamespace, Row.RowNames.splitBy}, result, "rows");

			request.Clear();
			request.Container["id"] = "DefaultCarrier";
			SetupRoutingRules();
			result = controller.SelectType();
			Assert.AreEqual("DefaultCarrier", controller.Session["OCMTypeID"]);
			AssertEx.JsonResultMatchesList(new[] { Row.RowNames.client, Row.RowNames.carrier, Row.RowNames.eventBranch, Row.RowNames.port, Row.RowNames.shpType, Row.RowNames.docName, Row.RowNames.provider }, result, "rows");
		}

		[TestMethod]
		public void TestValues()
		{
			request.Clear();
			request.Container["rows"] = "15";
			request.Container["_search"] = "false";

			var expected = new List<object>
			{
				new { id = "1", client = "TESTSENDER__1", carrier = "[ANY]", shpType = "[ANY]", docName = "[ANY]", provider = "INTTRA" },
				new { id = "2", client = "[ANY]", carrier = "AAAA", shpType = "VOCC", docName = "[ANY]", provider = "INTTRA" },
				new { id = "3", client = "TESTSENDER__1", carrier = "AAAA", port = "[ANY]", shpType = "NVOCC", docName = "[ANY]", provider = "INTTRA" },
				new { id = "4", client = "TEST*", carrier = "AAAA", port = "[ANY]", shpType = "[ANY]", docName = "Shipping Instruction", provider = "INTTRA" },
				new { id = "default", client = "Default", carrier = "Default", shpType = "Default", docName = "Default", provider = "Reject" }
			};
			var result = controller.Values();
			AssertEx.JsonResultMatchesList(expected, result, "oceanMessagingValues");

			request.Clear();
			request.Container["id"] = "CarrierHandlingAgent";
			controller.SelectType();
			Assert.AreEqual("CarrierHandlingAgent", controller.Session["OCMTypeID"]);
			SetupRoutingRules();

			request.Clear();
			request.Container["rows"] = "15";
			request.Container["_search"] = "false";
			expected = new List<object>
			{
				new { id = "1", client = "TEST*", carrier = "AAAA", port = "[ANY]", shpType = "[ANY]", docName = "Shipping Instruction", provider = "INTTRA" },
				new { id = "default", client = "Default", carrier = "Default", shpType = "Default", docName = "Default", provider = "Reject" }
			};
			result = controller.Values();
			AssertEx.JsonResultMatchesList(expected, result, "oceanMessagingValues");

			request.Clear();
			request.Container["id"] = "CarrierBookingAgent";
			controller.SelectType();
			Assert.AreEqual("CarrierBookingAgent", controller.Session["OCMTypeID"]);

			request.Clear();
			request.Container["rows"] = "15";
			request.Container["_search"] = "false";
			expected = new List<object>
			{
				new { id = "1", client = "TESTSENDER__1", carrier = "[ANY]", port = "[ANY]", shpType = "[ANY]", docName = "[ANY]", provider = "INTTRA" },
				new { id = "default", client = "Default", carrier = "Default", shpType = "Default", docName = "Default", provider = "Reject" }
			};
			result = controller.Values();
			AssertEx.JsonResultMatchesList(expected, result, "oceanMessagingValues");
		}

		[TestMethod]
		public void TestMultipleFiltersWithANDCondition()
		{
			request.Clear();
			request.Container["rows"] = "15";
			request.Container["_search"] = "false";
			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"client\",\"op\":\"bw\",\"data\":\"TEST*\"}]}";

			var expected = new List<object>
			{
				new { id = "4", client = "TEST*", carrier = "AAAA", port = "[ANY]", shpType = "[ANY]", docName = "Shipping Instruction", provider = "INTTRA" },
				new { id = "default", client = "Default", carrier = "Default", shpType = "Default", docName = "Default", provider = "Reject" },
			};
			var result = controller.Values();
			AssertEx.JsonResultMatchesList(expected, result, "oceanMessagingValues");
		}

		[TestMethod]
		public void TestSortWithASCCondition()
		{
			request.Clear();
			request.Container["rows"] = "15";
			request.Container["_search"] = "false";
			request.Container["sidx"] = "carrier";
			request.Container["sord"] = "asc";

			var expected = new List<object>
			{
				new { id = "1", client = "TESTSENDER__1", carrier = "[ANY]", shpType = "[ANY]", docName = "[ANY]", provider = "INTTRA" },
				new { id = "2", client = "[ANY]", carrier = "AAAA", shpType = "VOCC", docName = "[ANY]", provider = "INTTRA" },
				new { id = "3", client = "TESTSENDER__1", carrier = "AAAA", port = "[ANY]", shpType = "NVOCC", docName = "[ANY]", provider = "INTTRA" },
				new { id = "4", client = "TEST*", carrier = "AAAA", port = "[ANY]", shpType = "[ANY]", docName = "Shipping Instruction", provider = "INTTRA" },
				new { id = "default", client = "Default", carrier = "Default", shpType = "Default", docName = "Default", provider = "Reject" },
			};
			var result = controller.Values();
			AssertEx.JsonResultMatchesList(expected, result, "oceanMessagingValues");
		}

		[TestMethod]
		public void TestSortWithDESCCondition()
		{
			request.Clear();
			request.Container["rows"] = "15";
			request.Container["_search"] = "false";
			request.Container["sidx"] = "carrier";
			request.Container["sord"] = "desc";

			var expected = new List<object>
			{
				new { id = "default", client = "Default", carrier = "Default", shpType = "Default", docName = "Default", provider = "Reject" },
				new { id = "2", client = "[ANY]", carrier = "AAAA", shpType = "VOCC", docName = "[ANY]", provider = "INTTRA" },
				new { id = "3", client = "TESTSENDER__1", carrier = "AAAA", port = "[ANY]", shpType = "NVOCC", docName = "[ANY]", provider = "INTTRA" },
				new { id = "4", client = "TEST*", carrier = "AAAA", port = "[ANY]", shpType = "[ANY]", docName = "Shipping Instruction", provider = "INTTRA" },
				new { id = "1", client = "TESTSENDER__1", carrier = "[ANY]", shpType = "[ANY]", docName = "[ANY]", provider = "INTTRA" },
			};
			var result = controller.Values();
			AssertEx.JsonResultMatchesList(expected, result, "oceanMessagingValues");
		}

		[TestMethod]
		public void TestMultipleFiltersWithORCondition()
		{
			request.Clear();
			request.Container["rows"] = "15";
			request.Container["_search"] = "false";
			request.Container["filters"] = "{ \"groupOp\":\"OR\",\"rules\":[{ \"field\":\"client\",\"op\":\"bw\",\"data\":\"Default\"},{ \"field\":\"provider\",\"op\":\"ew\",\"data\":\"INTTRA\"}]}";

			var expected = new List<object>
			{
				new { id = "default", client = "Default", carrier = "Default", shpType = "Default", docName = "Default", provider = "Reject" },
				new { id = "1", client = "TESTSENDER__1", carrier = "[ANY]", shpType = "[ANY]", docName = "[ANY]", provider = "INTTRA" },
				new { id = "2", client = "[ANY]", carrier = "AAAA", shpType = "VOCC", docName = "[ANY]", provider = "INTTRA" },
				new { id = "3", client = "TESTSENDER__1", carrier = "AAAA", port = "[ANY]", shpType = "NVOCC", docName = "[ANY]", provider = "INTTRA" },
				new { id = "4", client = "TEST*", carrier = "AAAA", port = "[ANY]", shpType = "[ANY]", docName = "Shipping Instruction", provider = "INTTRA" },
			};
			var result = controller.Values();
			AssertEx.JsonResultMatchesList(expected, result, "oceanMessagingValues");
		}

		[TestMethod]
		public void TestClientsGetIncludeNonProdCWSystems()
		{
			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "15";
			request.Container["sidx"] = "CC_ID";
			request.Container["_search"] = "false";
			IList<object> expected1 = new List<object>
			{
				new { CC_ID = "[ANY]", CC_FriendlyName = "[ANY]" },
				new { CC_ID = "1STOPCSYD_SCH", CC_FriendlyName = "1-Stop Connection" },
				new { CC_ID = "2ACNVENVE", CC_FriendlyName = "2 Achieve B.V" },
				new { CC_ID = "GTNEXUS", CC_FriendlyName = "" },
				new { CC_ID = "INTTRA", CC_FriendlyName = "" },
				new { CC_ID = "OCM_CONTAINER_SPLIT", CC_FriendlyName = "" },
				new { CC_ID = "OCM_SUBSHIPMENT_SPLIT", CC_FriendlyName = "" },
				new { CC_ID = "SHIPPING_INSTRUCTION", CC_FriendlyName = "Shipping Instruction" },
				new { CC_ID = "TEST0001", CC_FriendlyName = "Test Client 1" },
				new { CC_ID = "TEST0002", CC_FriendlyName = "Test Client 2" },
				new { CC_ID = "TEST0003", CC_FriendlyName = "Test Client 3" },
				new { CC_ID = "TESTSENDER__1", CC_FriendlyName = "Test Sender" },
				new { CC_ID = "WTLPRDSV1", CC_FriendlyName = "" },
				new { CC_ID = "WTLTSTSV2", CC_FriendlyName = "" }
			};

			var result1 = controller.Clients(true); 

			Assert.IsNotNull(result1);
			AssertEx.JsonResultMatchesList(expected1, result1, "eHubClients");

			request.Container["sord"] = "asc";

			result1 = controller.Clients(true);

			Assert.IsNotNull(result1);
			AssertEx.JsonResultMatchesList(expected1, result1, "eHubClients");

			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "15";
			request.Container["sidx"] = "CC_FriendlyName";
			request.Container["sord"] = "desc";
			request.Container["_search"] = "true";
			request.Container["CC_ID"] = "TEST";
			IList<object> expected2 = new List<object>
			{
				new { CC_ID = "[ANY]", CC_FriendlyName = "[ANY]" },
				new { CC_ID = "TESTSENDER__1", CC_FriendlyName = "Test Sender" },
				new { CC_ID = "TEST0003", CC_FriendlyName = "Test Client 3" },
				new { CC_ID = "TEST0002", CC_FriendlyName = "Test Client 2" },
				new { CC_ID = "TEST0001", CC_FriendlyName = "Test Client 1" },
			};

			var result2 = controller.Clients(true);

			Assert.IsNotNull(result2);
			AssertEx.JsonResultMatchesList(expected2, result2, "eHubClients");

			request.Container["sidx"] = "CC_ID";

			result2 = controller.Clients(true);

			Assert.IsNotNull(result2);
			AssertEx.JsonResultMatchesList(expected2, result2, "eHubClients");


			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "15";
			request.Container["sidx"] = "CC_FriendlyName";
			request.Container["sord"] = "asc";
			request.Container["_search"] = "true";
			request.Container["CC_ID"] = "TEST";
			request.Container["CC_FriendlyName"] = "3";
			IList<object> expected3 = new List<object>
			{
				new { CC_ID = "[ANY]", CC_FriendlyName = "[ANY]" },
				new { CC_ID = "TEST0003", CC_FriendlyName = "Test Client 3" },
			};

			var result3 = controller.Clients(true);

			Assert.IsNotNull(result2);
			AssertEx.JsonResultMatchesList(expected3, result3, "eHubClients");
		}

		[TestMethod]
		public void TestClientsGetExcludeNonProdCWSystems()
		{
			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "17";
			request.Container["sidx"] = "CC_ID";
			request.Container["_search"] = "false";
			IList<object> expected1 = new List<object>
			{
				new { CC_ID = "[ANY]", CC_FriendlyName = "[ANY]" },
				new { CC_ID = "1STOPCSYD_SCH", CC_FriendlyName = "1-Stop Connection" },
				new { CC_ID = "2ACNVENVE", CC_FriendlyName = "2 Achieve B.V" },
				new { CC_ID = "GTNEXUS", CC_FriendlyName = "" },
				new { CC_ID = "INTTRA", CC_FriendlyName = "" },
				new { CC_ID = "OCM_CONTAINER_SPLIT", CC_FriendlyName = "" },
				new { CC_ID = "OCM_SUBSHIPMENT_SPLIT", CC_FriendlyName = "" },
				new { CC_ID = "SHIPPING_INSTRUCTION", CC_FriendlyName = "Shipping Instruction" },
				new { CC_ID = "TEST0001", CC_FriendlyName = "Test Client 1" },
				new { CC_ID = "TEST0002", CC_FriendlyName = "Test Client 2" },
				new { CC_ID = "TEST0003", CC_FriendlyName = "Test Client 3" },
				new { CC_ID = "TESTSENDER__1", CC_FriendlyName = "Test Sender" },
				new { CC_ID = "WTLPRDSV1", CC_FriendlyName = "" }
			};

			var result1 = controller.Clients(false);

			Assert.IsNotNull(result1);
			AssertEx.JsonResultMatchesList(expected1, result1, "eHubClients");

			request.Container["sord"] = "asc";

			result1 = controller.Clients(false);

			Assert.IsNotNull(result1);
			AssertEx.JsonResultMatchesList(expected1, result1, "eHubClients");

			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "15";
			request.Container["sidx"] = "CC_FriendlyName";
			request.Container["sord"] = "desc";
			request.Container["_search"] = "true";
			request.Container["CC_ID"] = "TEST";
			IList<object> expected2 = new List<object>
			{
				new { CC_ID = "[ANY]", CC_FriendlyName = "[ANY]" },
				new { CC_ID = "TESTSENDER__1", CC_FriendlyName = "Test Sender" },
				new { CC_ID = "TEST0003", CC_FriendlyName = "Test Client 3" },
				new { CC_ID = "TEST0002", CC_FriendlyName = "Test Client 2" },
				new { CC_ID = "TEST0001", CC_FriendlyName = "Test Client 1" },
			};

			var result2 = controller.Clients(false);

			Assert.IsNotNull(result2);
			AssertEx.JsonResultMatchesList(expected2, result2, "eHubClients");

			request.Container["sidx"] = "CC_ID";

			result2 = controller.Clients(false);

			Assert.IsNotNull(result2);
			AssertEx.JsonResultMatchesList(expected2, result2, "eHubClients");


			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "15";
			request.Container["sidx"] = "CC_FriendlyName";
			request.Container["sord"] = "asc";
			request.Container["_search"] = "true";
			request.Container["CC_ID"] = "TEST";
			request.Container["CC_FriendlyName"] = "3";
			IList<object> expected3 = new List<object>
			{
				new { CC_ID = "[ANY]", CC_FriendlyName = "[ANY]" },
				new { CC_ID = "TEST0003", CC_FriendlyName = "Test Client 3" },
			};

			var result3 = controller.Clients(false);

			Assert.IsNotNull(result2);
			AssertEx.JsonResultMatchesList(expected3, result3, "eHubClients");
		}

		[TestMethod]
		public void TestClientsFilterCaseInsensitive()
		{
			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "15";
			request.Container["sidx"] = "CC_FriendlyName";
			request.Container["sord"] = "desc";
			request.Container["_search"] = "true";
			request.Container["CC_ID"] = "test";
			IList<object> expected = new List<object>
			{
				new { CC_ID = "[ANY]", CC_FriendlyName = "[ANY]" },
				new { CC_ID = "TESTSENDER__1", CC_FriendlyName = "Test Sender" },
				new { CC_ID = "TEST0003", CC_FriendlyName = "Test Client 3" },
				new { CC_ID = "TEST0002", CC_FriendlyName = "Test Client 2" },
				new { CC_ID = "TEST0001", CC_FriendlyName = "Test Client 1" },
			};

			var result = controller.Clients(true);

			Assert.IsNotNull(result);
			AssertEx.JsonResultMatchesList(expected, result, "eHubClients");
		}

		[TestMethod]
		public void TestServiceProvidersGet()
		{
			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["sidx"] = "ID";
			request.Container["_search"] = "true";
			request.Container["selectedID"] = "default";
			var expected1 = new List<object>
			{
				new { ID = "Reject" },
				new { ID = "GTNEXUS" },
				new { ID = "INTTRA" }
			};

			var result1 = controller.ServiceProviders();

			Assert.IsNotNull(result1);
			AssertEx.JsonResultMatchesList(expected1, result1, "serviceProvider");

			request.Container["ID"] = "N";
			request.Container["sord"] = "desc";
			request.Container["selectedID"] = "1";

			var result2 = controller.ServiceProviders();

			var expected2 = new List<object>
			{
				new { ID = "INTTRA" },
				new { ID = "GTNEXUS" }
			};
			Assert.IsNotNull(result1);
			AssertEx.JsonResultMatchesList(expected2, result2, "serviceProvider");
		}

		[TestMethod]
		public void TestDocumentNamesGet()
		{
			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["sidx"] = "docName";
			request.Container["_search"] = "false";
			var expected1 = new List<object>
			{
				new { docName = "[ANY]"},
				new { docName = "Booking Request"},
				new { docName = "eManifest"},
			    new { docName = "Shipping Instruction"},
                new { docName = "Shipping Order"},
				new { docName = "Verified Gross Container Weight" }
			};

			var result1 = controller.DocumentNames();

			Assert.IsNotNull(result1);
			AssertEx.JsonResultMatchesList(expected1, result1, "docNames");

			request.Container["sord"] = "desc"; // Because we use jqgrid's getonce, we don't need any async sord, filter.
			result1 = controller.DocumentNames();

			Assert.IsNotNull(result1);
			AssertEx.JsonResultMatchesList(expected1, result1, "docNames");
		}

		[TestMethod]
		public void TestShipmentTypesGet()
		{
			request.Clear(); // // Because we use jqgrid's getonce, we don't need any async request.

			var expected1 = new List<object>
			{
				new { shpType = "[ANY]", description = "[ANY]" },
				new { shpType = "NVOCC", description = "For Co-Loaders" },
				new { shpType = "VOCC", description = "For Ocean Carriers" },
			};

			var result1 = controller.ShipmentTypes();

			Assert.IsNotNull(result1);
			AssertEx.JsonResultMatchesList(expected1, result1, "shipmentTypes");
		}

		[TestMethod]
		public void TestOceanMessagingTableValues()
		{
			var expected1 = new List<object>
			{
				new { id = "1", client = "TESTSENDER__1", carrier = "[ANY]", shpType = "[ANY]", docName = "[ANY]", provider = "INTTRA" },
				new { id = "2", client = "[ANY]", carrier = "AAAA", shpType = "VOCC", docName = "[ANY]", provider = "INTTRA" },
				new { id = "default", client = "Default", carrier = "Default", shpType = "Default", docName = "Default", provider = "Reject" }
			};

			var expected2 = new List<object>
			{
				new { id = "1", client = "TESTSENDER__1", carrier = "[ANY]", shpType = "[ANY]", docName = "[ANY]", provider = "INTTRA" },
				new { id = "3", client = "TESTSENDER__1", carrier = "AAAA", shpType = "NVOCC", docName = "[ANY]", provider = "INTTRA" },
				new { id = "default", client = "Default", carrier = "Default", shpType = "Default", docName = "Default", provider = "Reject" }
			};

			var expected2b = new List<object>
			{
				new { id = "1", client = "TESTSENDER__1", carrier = "[ANY]", shpType = "[ANY]", docName = "[ANY]", provider = "INTTRA" },
				new { id = "3", client = "TESTSENDER__1", carrier = "AAAA", shpType = "NVOCC", docName = "[ANY]", provider = "INTTRA" },
				new { id = "4", client = "TEST*", carrier = "AAAA", shpType = "[ANY]", docName = "Shipping Instruction" , provider = "INTTRA" },
				new { id = "default", client = "Default", carrier = "Default", shpType = "Default", docName = "Default", provider = "Reject" }
			};

			var expected3 = new List<object>
			{
				new { id = "4", client = "TEST*", carrier = "AAAA", provider = "INTTRA", docName = "Shipping Instruction" },
				new { id = "default", client = "Default", carrier = "Default", shpType = "Default", docName = "Default", provider = "Reject" }
			};

			var expected4 = new List<object>
			{
				new { id = "default", client = "Default", carrier = "Default", shpType = "Default", docName = "Default", provider = "Reject" }
			};

			var expected5 = new List<object>
			{
				new { id = "1", client = "TESTSENDER__1", carrier = "[ANY]", shpType = "[ANY]", docName = "[ANY]", provider = "INTTRA" },
				new { id = "default", client = "Default", carrier = "Default", shpType = "Default", docName = "Default", provider = "Reject" }
			};

			var expected6 = new List<object>
			{
				new { id = "2", client = "[ANY]", carrier = "AAAA", shpType = "VOCC", docName = "[ANY]", provider = "INTTRA" },
				new { id = "3", client = "TESTSENDER__1", carrier = "AAAA", shpType = "NVOCC", docName = "[ANY]", provider = "INTTRA" },
				new { id = "default", client = "Default", carrier = "Default", shpType = "Default", docName = "Default", provider = "Reject" }
			};

			var expected7 = new List<object>
			{
				new { id = "3", client = "TESTSENDER__1", carrier = "AAAA", shpType = "NVOCC", docName = "[ANY]", provider = "INTTRA" },
				new { id = "default", client = "Default", carrier = "Default", shpType = "Default", docName = "Default", provider = "Reject" }
			};

			// Test Client field
			request.Clear();
			request.Container["rows"] = "2";
			request.Container["_search"] = "false";
			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"client\",\"op\":\"cn\",\"data\":\"n\"}]}";

			var result = controller.Values();

			AssertEx.JsonResultMatchesList(expected1, result, "oceanMessagingValues");

			request.Container["rows"] = "4";
			request.Container["page"] = "2";
			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"client\",\"op\":\"eq\",\"data\":\"TESTSENDER__1\"}]}";

			result = controller.Values();
			AssertEx.JsonResultMatchesList(expected2, result, "oceanMessagingValues");

			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"client\",\"op\":\"bw\",\"data\":\"T\"}]}";

			result = controller.Values();
			AssertEx.JsonResultMatchesList(expected2b, result, "oceanMessagingValues");

			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"client\",\"op\":\"ew\",\"data\":\"1\"}]}";

			result = controller.Values();
			AssertEx.JsonResultMatchesList(expected2, result, "oceanMessagingValues");

			// Test carrier field
			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"carrier\",\"op\":\"cn\",\"data\":\"a\"}]}";
			request.Container["rows"] = "3";

			result = controller.Values();
			AssertEx.JsonResultMatchesList(expected3, result, "oceanMessagingValues");

			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"carrier\",\"op\":\"eq\",\"data\":\"[ANY]\"}]}";
			result = controller.Values();
			AssertEx.JsonResultMatchesList(expected5, result, "oceanMessagingValues");

			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"carrier\",\"op\":\"bw\",\"data\":\"[\"}]}";
			result = controller.Values();
			AssertEx.JsonResultMatchesList(expected5, result, "oceanMessagingValues");

			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"carrier\",\"op\":\"ew\",\"data\":\"]\"}]}";
			result = controller.Values();
			AssertEx.JsonResultMatchesList(expected5, result, "oceanMessagingValues");

			// Test Provider
			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"provider\",\"op\":\"eq\",\"data\":\"INTTRA\"}]}";

			result = controller.Values();
			AssertEx.JsonResultMatchesList(expected3, result, "oceanMessagingValues");

			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"provider\",\"op\":\"cn\",\"data\":\"INT\"}]}";

			result = controller.Values();
			AssertEx.JsonResultMatchesList(expected3, result, "oceanMessagingValues");


			request.Container["filters"] = "{ \"groupOp\":\"OR\",\"rules\":[{ \"field\":\"provider\",\"op\":\"bw\",\"data\":\"O\"}]}";

			result = controller.Values();
			AssertEx.JsonResultMatchesList(expected4, result, "oceanMessagingValues");

			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"provider\",\"op\":\"ew\",\"data\":\"O\"}]}";

			result = controller.Values();
			AssertEx.JsonResultMatchesList(expected4, result, "oceanMessagingValues");

			// Test Document Name
			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"docName\",\"op\":\"eq\",\"data\":\"Shipping Instruction\"}]}";

			result = controller.Values();
			AssertEx.JsonResultMatchesList(expected3, result, "oceanMessagingValues");

			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"docName\",\"op\":\"cn\",\"data\":\"Instruction\"}]}";

			result = controller.Values();
			AssertEx.JsonResultMatchesList(expected3, result, "oceanMessagingValues");

			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"docName\",\"op\":\"bw\",\"data\":\"Shipping\"}]}";

			result = controller.Values();
			AssertEx.JsonResultMatchesList(expected3, result, "oceanMessagingValues");

			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"docName\",\"op\":\"ew\",\"data\":\"Instruction\"}]}";

			result = controller.Values();
			AssertEx.JsonResultMatchesList(expected3, result, "oceanMessagingValues");

			// Test Shipment Type
			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"shpType\",\"op\":\"eq\",\"data\":\"NVOCC\"}]}";

			result = controller.Values();
			AssertEx.JsonResultMatchesList(expected7, result, "oceanMessagingValues");

			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"shpType\",\"op\":\"cn\",\"data\":\"OCC\"}]}";

			result = controller.Values();
			AssertEx.JsonResultMatchesList(expected6, result, "oceanMessagingValues");

			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"shpType\",\"op\":\"bw\",\"data\":\"NVO\"}]}";

			result = controller.Values();
			AssertEx.JsonResultMatchesList(expected7, result, "oceanMessagingValues");

			// Invalid search - Not search anything - return default value
			request.Container["filters"] = "{}";

			result = controller.Values();
			AssertEx.JsonResultMatchesList(expected3, result, "oceanMessagingValues");

			// Test Inline search
			request.Container["filters"] = null;
			request.Container["_search"] = "true";
			request.Container["client"] = "Test";
			request.Container["carrier"] = "Test";

			result = controller.Values();
			AssertEx.JsonResultMatchesList(expected4, result, "oceanMessagingValues");

			request.Container["client"] = null;
			request.Container["carrier"] = null;
			request.Container["provider"] = "INTTRA";
			request.Container["docName"] = "Shipping";
			result = controller.Values();
			AssertEx.JsonResultMatchesList(expected3, result, "oceanMessagingValues");

			request.Container["client"] = null;
			request.Container["carrier"] = null;
			request.Container["provider"] = "INTTRA";
			request.Container["docName"] = null;
			request.Container["shpType"] = "NVOCC";
			result = controller.Values();
			AssertEx.JsonResultMatchesList(expected7, result, "oceanMessagingValues");
		}

		[TestMethod]
		public void TestClearCache()
		{
			controller.Session["OCM_DefaultCarrier"] = "Test";
			Assert.IsNotNull(controller.Session["OCM_DefaultCarrier"]);
			controller.ClearCache();
			Assert.IsNull(controller.Session["OCM_DefaultCarrier"]);
		}

		[TestMethod]
		public void TestSaveValues()
		{
			var expectedSuccessMessage = new { success = true, id = "n0" };
			var expectedValues = new List<object>
			{
				new { id = "1", client = "TESTSENDER__1", carrier = "[ANY]", shpType = "[ANY]", docName = "[ANY]", provider = "INTTRA" },
				new { id = "2", client = "[ANY]", carrier = "AAAA", shpType = "VOCC", docName = "[ANY]", provider = "INTTRA" },
				new { id = "3", client = "TESTSENDER__1", carrier = "AAAA", shpType = "NVOCC", docName = "[ANY]", provider = "INTTRA" },
				new { id = "4", client = "TEST*", carrier = "AAAA", shpType = "[ANY]", docName = "Shipping Instruction", provider = "INTTRA" },
				new { id = "n0", client = "[ANY]", carrier = "[ANY]", shpType = "NVOCC", docName = "Verified Gross Container Weight", provider = "GTNEXUS" },
				new { id = "default", client = "Default", carrier = "Default", shpType = "Default", docName = "Default", provider = "Reject" }
			};

			var expectedValuesAfterSave_FromDatabase = new List<object>
			{
				new { id = "1", client = "TESTSENDER__1", carrier = "[ANY]", shpType = "[ANY]", docName = "[ANY]", provider = "INTTRA" },
				new { id = "2", client = "[ANY]", carrier = "AAAA", shpType = "VOCC", docName = "[ANY]", provider = "INTTRA" },
				new { id = "3", client = "TESTSENDER__1", carrier = "AAAA", shpType = "NVOCC", docName = "[ANY]", provider = "INTTRA" },
				new { id = "4", client = "TEST*", carrier = "AAAA", shpType = "[ANY]", docName = "Shipping Instruction", provider = "INTTRA" },
				new { id = "5", client = "[ANY]", carrier = "[ANY]", shpType = "NVOCC", docName = "Verified Gross Container Weight", provider = "GTNEXUS" },
				new { id = "default", client = "Default", carrier = "Default", shpType = "Default", docName = "Default", provider = "Reject" }
			};

			var expectedValuesDefaultProviderGTNEXUS = new List<object>
			{
				new { id = "1", client = "TESTSENDER__1", carrier = "[ANY]", shpType = "[ANY]", docName = "[ANY]", provider = "INTTRA" },
				new { id = "2", client = "[ANY]", carrier = "AAAA", shpType = "VOCC", docName = "[ANY]", provider = "INTTRA" },
				new { id = "3", client = "TESTSENDER__1", carrier = "AAAA", shpType = "NVOCC", docName = "[ANY]", provider = "INTTRA" },
				new { id = "4", client = "TEST*", carrier = "AAAA", shpType = "[ANY]", docName = "Shipping Instruction", provider = "INTTRA" },
				new { id = "5", client = "[ANY]", carrier = "[ANY]", shpType = "NVOCC", docName = "Verified Gross Container Weight", provider = "GTNEXUS" },
				new { id = "default", client = "Default", carrier = "Default", shpType = "Default", docName = "Default", provider = "GTNEXUS" }
			};

			var expectedEmptyExpression = new List<object>
			{
				new { id = "1", client = "TESTSENDER__1", carrier = "[ANY]", shpType = "[ANY]", docName = "[ANY]", provider = "INTTRA" },
				new { id = "2", client = "[ANY]", carrier = "AAAA", shpType = "VOCC", docName = "[ANY]", provider = "INTTRA" },
				new { id = "3", client = "TESTSENDER__1", carrier = "AAAA", shpType = "NVOCC", docName = "[ANY]", provider = "INTTRA" },
				new { id = "4", client = "TEST*", carrier = "AAAA", shpType = "[ANY]", docName = "Shipping Instruction", provider = "INTTRA" },
				new { id = "5", client = "[ANY]", carrier = "[ANY]", shpType = "[ANY]", docName = "[ANY]", provider = "INTTRA" },
				new { id = "6", client = "[ANY]", carrier = "[ANY]", shpType = "NVOCC", docName = "Verified Gross Container Weight", provider = "GTNEXUS" },
				new { id = "default", client = "Default", carrier = "Default", shpType = "Default", docName = "Default", provider = "GTNEXUS" }
			};

			var expectedSuccessSaveMessage = new { message = "Success tranfering all the data into database" };
			var expectedUnchangeSaveMessage = new { message = "No changes to be applied." };
			var expectedFailingSaveMessage = new { message = "Fail to save all data into database. Reason: Duplicate rules detected." };

			var result = controller.SaveAll();
			Assert.AreEqual(expectedUnchangeSaveMessage.ToString(), result.Data.ToString(), "Precodition");

			request.Clear();
			request.Container["id"] = "n0";
			request.Container["afterRowId"] = "default";
			request.Container["oper"] = "add";
			request.Container["rows"] = "10";
			request.Container["_search"] = "false";
			request.Container["rowLimit"] = "4";
			request.Container["provider"] = "GTNEXUS";
			request.Container["client"] = "[ANY]";
			request.Container["carrier"] = "[ANY]";
			request.Container["docName"] = "Verified Gross Container Weight";
			request.Container["shpType"] = "NVOCC";

			var success = controller.ValuesEdit();
			var oceanValues = controller.Values();

			Assert.AreEqual(expectedSuccessMessage.ToString(), success.Data.ToString());
			AssertEx.JsonResultMatchesList(expectedValues, oceanValues, "oceanMessagingValues");

			result = controller.SaveAll();
			Assert.AreEqual(expectedSuccessSaveMessage.ToString(), result.Data.ToString());
			AssertEx.JsonResultMatchesList(expectedValuesAfterSave_FromDatabase, controller.Values(), "oceanMessagingValues");

			result = controller.SaveAll();
			Assert.AreEqual(expectedUnchangeSaveMessage.ToString(), result.Data.ToString());

			request.Container["id"] = "default";
			request.Container["oper"] = "edit";
			request.Container["provider"] = "GTNEXUS";

			success = controller.ValuesEdit();
			result = controller.SaveAll();

			Assert.AreEqual("{ success = True, id = default }", success.Data.ToString());
			Assert.AreEqual(expectedSuccessSaveMessage.ToString(), result.Data.ToString());
			AssertEx.JsonResultMatchesList(expectedValuesDefaultProviderGTNEXUS, controller.Values(), "oceanMessagingValues");

			request.Container["id"] = "n0";
			request.Container["oper"] = "add";
			request.Container["afterRowId"] = "5";
			request.Container["provider"] = "GTNEXUS";
			request.Container["docName"] = "Verified Gross Container Weight";
			request.Container["shpType"] = "NVOCC";

			success = controller.ValuesEdit();
			result = controller.SaveAll();

			Assert.AreEqual("{ success = True, id = n0 }", success.Data.ToString());
			Assert.AreEqual(expectedFailingSaveMessage.ToString(), result.Data.ToString());
			controller.ClearCache();
			AssertEx.JsonResultMatchesList(expectedValuesDefaultProviderGTNEXUS, controller.Values(), "oceanMessagingValues");

			request.Container["id"] = "n0";
			request.Container["oper"] = "add";
			request.Container["afterRowId"] = "5";
			request.Container["provider"] = "INTTRA";
			request.Container["docName"] = "[ANY]";
			request.Container["shpType"] = "[ANY]";
			success = controller.ValuesEdit();
			result = controller.SaveAll();
			Assert.AreEqual("{ success = True, id = n0 }", success.Data.ToString());
			Assert.AreEqual(expectedSuccessSaveMessage.ToString(), result.Data.ToString());
			AssertEx.JsonResultMatchesList(expectedEmptyExpression, controller.Values(), "oceanMessagingValues");

			controller.ClearCache();
		}

		[TestMethod]
		public void TestEditValues()
		{
			var before = new List<object>
			{
				new { id = "1", client = "TESTSENDER__1", carrier = "[ANY]", port = "[ANY]",shpType = "[ANY]", docName = "[ANY]", provider = "INTTRA" },
				new { id = "2", client = "[ANY]", carrier = "AAAA", port = "[ANY]",shpType = "VOCC", docName = "[ANY]", provider = "INTTRA" },
				new { id = "3", client = "TESTSENDER__1", carrier = "AAAA", port = "[ANY]", shpType = "NVOCC", docName = "[ANY]", provider = "INTTRA" },
				new { id = "4", client = "TEST*", carrier = "AAAA", port = "[ANY]", shpType = "[ANY]", docName = "Shipping Instruction", provider = "INTTRA" },
				new { id = "default", client = "Default", carrier = "Default", port = "Default", shpType = "Default", docName = "Default", provider = "Reject" }
			};

			var after = new List<object>
			{
				new { id = "1", client = "TESTSENDER__2", carrier = "Test", port = "TestPort", shpType = "[ANY]", docName = "[ANY]", provider = "INTTRA" },
				new { id = "2", client = "[ANY]", carrier = "AAAA", port = "[ANY]", shpType = "VOCC", docName = "[ANY]", provider = "INTTRA" },
				new { id = "3", client = "TESTSENDER__1", carrier = "AAAA", port = "[ANY]", shpType = "NVOCC", docName = "[ANY]", provider = "INTTRA" },
				new { id = "4", client = "TEST*", carrier = "AAAA", port = "[ANY]", shpType = "[ANY]", docName = "Shipping Instruction", provider = "INTTRA" },
				new { id = "default", client = "Default", carrier = "Default", port = "Default", shpType = "Default", docName = "Default", provider = "Reject" }
			};

			request.Clear();
			request.Container["rows"] = "10";
			request.Container["_search"] = "false";
			request.Container["rowLimit"] = "4";
			request.Container["id"] = "1";
			request.Container["oper"] = "edit";
			request.Container["client"] = "TESTSENDER__2";
			request.Container["carrier"] = "Test";
			request.Container["provider"] = "INTTRA";
			request.Container["carrierAgent"] = "TestAgent";
			request.Container["port"] = "TestPort";
			request.Container["docName"] = "[ANY]";

			var result = controller.Values();

			AssertEx.JsonResultMatchesList(before, result, "oceanMessagingValues");

			var success = controller.ValuesEdit();
			result = controller.Values();

			Assert.AreEqual("{ success = True, id = 1 }", success.Data.ToString());
			AssertEx.JsonResultMatchesList(after, result, "oceanMessagingValues");
		}

		[TestMethod]
		public void TestAddValues_Fail()
		{
			request.Clear();
			request.Container["rows"] = "10";
			request.Container["_search"] = "false";
			request.Container["rowLimit"] = "3";
			request.Container["id"] = "_empty";
			request.Container["oper"] = "add";
			request.Container["client"] = null;
			request.Container["provider"] = null;

			var success = controller.ValuesEdit();
			var result = controller.Values();

			Assert.AreEqual("{ success = False, message = Client ID and Carrier SCAC/C1C and Document Name and Service Provider cannot be empty. }", success.Data.ToString());
		}

		[TestMethod]
		public void TestDeleteValues()
		{
			var before = new List<object>
			{
				new { id = "1", client = "TESTSENDER__1", carrier = "[ANY]", port = "[ANY]", shpType = "[ANY]", docName = "[ANY]", provider = "INTTRA" },
				new { id = "2", client = "[ANY]", carrier = "AAAA", port = "[ANY]", shpType = "VOCC", docName = "[ANY]", provider = "INTTRA" },
				new { id = "3", client = "TESTSENDER__1", carrier = "AAAA",  port = "[ANY]", shpType = "NVOCC", docName = "[ANY]", provider = "INTTRA" },
				new { id = "4", client = "TEST*", carrier = "AAAA", port = "[ANY]", shpType = "[ANY]", docName = "Shipping Instruction", provider = "INTTRA" },
				new { id = "default", client = "Default", carrier = "Default", port = "Default", shpType = "Default", docName = "Default", provider = "Reject" }
			};

			var after = new List<object>
			{
				new { id = "2", client = "[ANY]", carrier = "AAAA", port = "[ANY]", shpType = "VOCC", docName = "[ANY]", provider = "INTTRA" },
				new { id = "3", client = "TESTSENDER__1", carrier = "AAAA", port = "[ANY]", shpType = "NVOCC", docName = "[ANY]", provider = "INTTRA" },
				new { id = "4", client = "TEST*", carrier = "AAAA", port = "[ANY]", shpType = "[ANY]", docName = "Shipping Instruction", provider = "INTTRA" },
				new { id = "default", client = "Default", carrier = "Default", port = "Default", shpType = "Default", docName = "Default", provider = "Reject" }
			};

			request.Clear();
			request.Container["rows"] = "10";
			request.Container["_search"] = "false";
			request.Container["rowLimit"] = "3";
			request.Container["id"] = "1";
			request.Container["oper"] = "Do nothing";

			var success = controller.ValuesEdit();
			var result = controller.Values();

			Assert.AreEqual("{ success = True, id = 1 }", success.Data.ToString());
			AssertEx.JsonResultMatchesList(before, result, "oceanMessagingValues");

			request.Container["oper"] = "del";

			success = controller.ValuesEdit();
			result = controller.Values();

			Assert.AreEqual("{ success = True, id = 1 }", success.Data.ToString());
			AssertEx.JsonResultMatchesList(after, result, "oceanMessagingValues");
		}

		[TestMethod]
		public void TestMoveRow()
		{
			var expected1 = new List<object>
			{
				new { id = "1", client = "TESTSENDER__1", carrier = "[ANY]", shpType = "[ANY]", docName = "[ANY]", provider = "INTTRA" },
				new { id = "2", client = "[ANY]", carrier = "AAAA", shpType = "VOCC", docName = "[ANY]", provider = "INTTRA" },
				new { id = "3", client = "TESTSENDER__1", carrier = "AAAA", shpType = "NVOCC", docName = "[ANY]", provider = "INTTRA" },
				new { id = "4", client = "TEST*", carrier = "AAAA", shpType = "[ANY]", docName = "Shipping Instruction", provider = "INTTRA" },
				new { id = "default", client = "Default", carrier = "Default", shpType = "Default", docName = "Default", provider = "Reject" }
			};

			var expected2 = new List<object>
			{
				new { id = "2", client = "[ANY]", carrier = "AAAA", shpType = "VOCC", docName = "[ANY]", provider = "INTTRA" },
				new { id = "1", client = "TESTSENDER__1", carrier = "[ANY]", shpType = "[ANY]", docName = "[ANY]", provider = "INTTRA" },
				new { id = "3", client = "TESTSENDER__1", carrier = "AAAA", shpType = "NVOCC", docName = "[ANY]", provider = "INTTRA" },
				new { id = "4", client = "TEST*", carrier = "AAAA", shpType = "[ANY]", docName = "Shipping Instruction", provider = "INTTRA" },
				new { id = "default", client = "Default", carrier = "Default", shpType = "Default", docName = "Default", provider = "Reject" }
			};

			var expected3 = new List<object>
			{
				new { id = "1", client = "TESTSENDER__1", carrier = "[ANY]", shpType = "[ANY]", docName = "[ANY]", provider = "INTTRA" },
				new { id = "2", client = "[ANY]", carrier = "AAAA", shpType = "VOCC", docName = "[ANY]", provider = "INTTRA" },
				new { id = "n1", client = "[ANY]", carrier = "[ANY]", shpType = "[ANY]", docName = "[ANY]", provider = "GTNEXUS" },
				new { id = "3", client = "TESTSENDER__1", carrier = "AAAA", shpType = "NVOCC", docName = "[ANY]", provider = "INTTRA" },
				new { id = "4", client = "TEST*", carrier = "AAAA", shpType = "[ANY]", docName = "Shipping Instruction", provider = "INTTRA" },
				new { id = "default", client = "Default", carrier = "Default", shpType = "Default", docName = "Default", provider = "Reject" }
			};

			var expected4 = new List<object>
			{
				new { id = "2", client = "[ANY]", carrier = "AAAA", shpType = "VOCC", docName = "[ANY]", provider = "INTTRA" },
				new { id = "n1", client = "[ANY]", carrier = "[ANY]", shpType = "[ANY]", docName = "[ANY]", provider = "GTNEXUS" },
				new { id = "1", client = "TESTSENDER__1", carrier = "[ANY]", shpType = "[ANY]", docName = "[ANY]", provider = "INTTRA" },
				new { id = "3", client = "TESTSENDER__1", carrier = "AAAA", shpType = "NVOCC", docName = "[ANY]", provider = "INTTRA" },
				new { id = "4", client = "TEST*", carrier = "AAAA", shpType = "[ANY]", docName = "Shipping Instruction", provider = "INTTRA" },
				new { id = "n0", client = "[ANY]", carrier = "[ANY]", shpType = "[ANY]", docName = "[ANY]", provider = "GTNEXUS" },
				new { id = "default", client = "Default", carrier = "Default", shpType = "Default", docName = "Default", provider = "Reject" }
			};

			var expectedPage = new { page = 1 };

			request.Clear();
			request.Container["rows"] = "10";
			request.Container["_search"] = "false";
			request.Container["rowLimit"] = "3";

			// Default
			var values = controller.Values();
			AssertEx.JsonResultMatchesList(expected1, values, "oceanMessagingValues");

			// Move down
			request.Container["id"] = "1";
			request.Container["option"] = "down";

			var result = controller.MoveRow();
			values = controller.Values();

			Assert.AreEqual(expectedPage.ToString(), result.Data.ToString());
			AssertEx.JsonResultMatchesList(expected2, values, "oceanMessagingValues");

			// Move up
			request.Container["option"] = "up";

			result = controller.MoveRow();
			values = controller.Values();

			Assert.AreEqual(expectedPage.ToString(), result.Data.ToString());
			AssertEx.JsonResultMatchesList(expected1, values, "oceanMessagingValues");

			// Move to
			request.Container["option"] = "set";
			request.Container["newPosition"] = "2";

			result = controller.MoveRow();
			values = controller.Values();

			Assert.AreEqual(expectedPage.ToString(), result.Data.ToString());
			AssertEx.JsonResultMatchesList(expected2, values, "oceanMessagingValues");

			request.Container["id"] = "n1";
			request.Container["afterRowId"] = "1";
			request.Container["oper"] = "edit";
			request.Container["provider"] = "GTNEXUS";
			request.Container["client"] = "[ANY]";
			request.Container["docName"] = "[ANY]";
			request.Container["carrier"] = "[ANY]";

			var editResult = controller.ValuesEdit(); // Add value.(Try to edit invalid id, adding new value with that id)
			Assert.AreEqual("{ success = True, id = n1 }", editResult.Data.ToString());

			request.Container["id"] = "1";
			request.Container["option"] = "set";
			request.Container["newPosition"] = "1";

			result = controller.MoveRow();
			values = controller.Values();

			Assert.AreEqual(expectedPage.ToString(), result.Data.ToString());
			AssertEx.JsonResultMatchesList(expected3, values, "oceanMessagingValues");

			// Same position unchange
			result = controller.MoveRow();
			values = controller.Values();

			Assert.AreEqual(expectedPage.ToString(), result.Data.ToString());
			AssertEx.JsonResultMatchesList(expected3, values, "oceanMessagingValues");

			// np and cp in both equal true in product code
			request.Container["id"] = "n0";
			request.Container["afterRowId"] = "default";
			request.Container["oper"] = "add";

			editResult = controller.ValuesEdit(); // Add value.(Try to edit invalid id, adding new value with that id)
			Assert.AreEqual("{ success = True, id = n0 }", editResult.Data.ToString());

			request.Container["id"] = "1";
			request.Container["option"] = "set";
			request.Container["newPosition"] = "3";

			result = controller.MoveRow();
			values = controller.Values();

			Assert.AreEqual(expectedPage.ToString(), result.Data.ToString());
			AssertEx.JsonResultMatchesList(expected4, values, "oceanMessagingValues");

			// Do nothing
			request.Container["option"] = "do nothing";

			result = controller.MoveRow();
			values = controller.Values();

			Assert.AreEqual(expectedPage.ToString(), result.Data.ToString());
			AssertEx.JsonResultMatchesList(expected4, values, "oceanMessagingValues");

			controller.ClearCache();
		}

		[TestMethod]
		public void TestReformatSessionRowID()
		{
			var before = new List<object>
			{
				new { id = "1", client = "TESTSENDER__1", carrier = "[ANY]", shpType = "[ANY]", docName = "[ANY]", provider = "INTTRA" },
				new { id = "2", client = "[ANY]", carrier = "AAAA", shpType = "VOCC", docName = "[ANY]", provider = "INTTRA" },
				new { id = "3", client = "TESTSENDER__1", carrier = "AAAA", shpType = "NVOCC", docName = "[ANY]", provider = "INTTRA" },
				new { id = "4", client = "TEST*", carrier = "AAAA", shpType = "[ANY]", docName = "Shipping Instruction", provider = "INTTRA" },
				new { id = "n0", client = "TESTSENDER__1", carrier = "Test", shpType = "[ANY]" , docName = "[ANY]", provider = "INTTRA"},
				new { id = "default", client = "Default", carrier = "Default", shpType = "Default", docName = "Default", provider = "Reject" }
			};

			var after = new List<object>
			{
				new { id = "1", client = "TESTSENDER__1", carrier = "[ANY]", shpType = "[ANY]", docName = "[ANY]", provider = "INTTRA" },
				new { id = "2", client = "[ANY]", carrier = "AAAA", shpType = "VOCC", docName = "[ANY]", provider = "INTTRA" },
				new { id = "3", client = "TESTSENDER__1", carrier = "AAAA", shpType = "NVOCC", docName = "[ANY]", provider = "INTTRA" },
				new { id = "4", client = "TEST*", carrier = "AAAA", shpType = "[ANY]", docName = "Shipping Instruction", provider = "INTTRA" },
				new { id = "5", client = "TESTSENDER__1", carrier = "Test", shpType = "[ANY]" , docName = "[ANY]", provider = "INTTRA"},
				new { id = "default", client = "Default", carrier = "Default", shpType = "Default", docName = "Default", provider = "Reject" }
			};

			request.Clear();
			request.Container["id"] = "n0";
			request.Container["afterRowId"] = "default";
			request.Container["oper"] = "add";
			request.Container["rows"] = "10";
			request.Container["_search"] = "false";
			request.Container["rowLimit"] = "10";
			request.Container["client"] = "TESTSENDER__1";
			request.Container["carrier"] = "Test";
			request.Container["provider"] = "INTTRA";
			request.Container["docName"] = "[ANY]";

			var result = controller.ValuesEdit();
			var oceanValues = controller.Values();

			Assert.AreEqual("{ success = True, id = n0 }", result.Data.ToString());
			AssertEx.JsonResultMatchesList(before, oceanValues, "oceanMessagingValues");

			controller.ReformatSessionRowID();
			oceanValues = controller.Values();

			AssertEx.JsonResultMatchesList(after, oceanValues, "oceanMessagingValues");
		}

		[TestMethod]
		public void TestExportCSV()
		{
			string expectedData = "Client ID,Carrier SCAC/C1C,Event Branch,Port/Country,Shipment Type,Document Name,Service Provider" + Environment.NewLine +
								"TESTSENDER__1,[ANY],[ANY],[ANY],[ANY],[ANY],INTTRA" + Environment.NewLine +
								"[ANY],AAAA,[ANY],[ANY],VOCC,[ANY],INTTRA" + Environment.NewLine +
								"TESTSENDER__1,AAAA,[ANY],[ANY],NVOCC,[ANY],INTTRA" + Environment.NewLine +
								"TEST*,AAAA,[ANY],[ANY],[ANY],Shipping Instruction,INTTRA" + Environment.NewLine +
								"Default,Default,Default,Default,Default,Default,Reject" + Environment.NewLine;
			string expectedName = "DefaultCarrier.csv";
			FileContentResult result = controller.ExportCSV();
			Assert.AreEqual(expectedData, Encoding.Default.GetString(result.FileContents));
			Assert.AreEqual(expectedName, result.FileDownloadName);
		}

		[TestMethod]
		public void TestExportCSVCarrierHandlingAgent()
		{
			controller.Session["OCMTypeID"] = "CarrierHandlingAgent";
			string expectedData = "Client ID,Carrier SCAC/C1C,Handling Agent C1C,Event Branch,Port/Country,Shipment Type,Document Name,Service Provider" + Environment.NewLine +
								"TEST*,AAAA,[ANY],[ANY],[ANY],[ANY],Shipping Instruction,INTTRA" + Environment.NewLine +
								"Default,Default,Default,Default,Default,Default,Default,Reject" + Environment.NewLine;
			string expectedName = "CarrierHandlingAgent.csv";
			FileContentResult result = controller.ExportCSV();
			Assert.AreEqual(expectedData, Encoding.Default.GetString(result.FileContents));
			Assert.AreEqual(expectedName, result.FileDownloadName);
		}

		[TestMethod]
		public void TestExportCSVCarrierBookingAgent()
		{
			controller.Session["OCMTypeID"] = "CarrierBookingAgent";
			string expectedData = "Client ID,Carrier SCAC/C1C,Booking Agent C1C,Event Branch,Port/Country,Shipment Type,Document Name,Service Provider" + Environment.NewLine +
								"TESTSENDER__1,[ANY],[ANY],[ANY],[ANY],[ANY],[ANY],INTTRA" + Environment.NewLine +
								"Default,Default,Default,Default,Default,Default,Default,Reject" + Environment.NewLine;
			string expectedName = "CarrierBookingAgent.csv";
			FileContentResult result = controller.ExportCSV();
			Assert.AreEqual(expectedData, Encoding.Default.GetString(result.FileContents));
			Assert.AreEqual(expectedName, result.FileDownloadName);
		}

		[TestMethod]
		public void TestImportCSV()
		{
			_backupManager.Expect(x =>
				x.Backup(Arg<HttpServerUtilityBase>.Is.Anything, Arg<string>.Is.Anything, Arg<byte[]>.Is.Anything));
			string importCSV1 = "Client ID,Carrier SCAC/C1C,Event Branch,Port/Country,Shipment Type,Document Name,Service Provider" + Environment.NewLine +
							"TESTSENDER__1,[ANY],[ANY],[ANY],NVOCC,[ANY],INTTRA" + Environment.NewLine +
							"[ANY],AAAA,[ANY],AUSYD,VOCC,[ANY],INTTRA" + Environment.NewLine +
							"Default,Default,DefaultDefault,Default,Default,Reject" + Environment.NewLine;
			string importCSV2 = "Client ID,Carrier SCAC/C1C,Event Branch,Port/Country,Shipment Type,Document Name,Service Provider" + Environment.NewLine +
							"TEST*,[ANY],[ANY],[ANY],NVOCC,[ANY],INTTRA" + Environment.NewLine +
							"[ANY],AAAA,[ANY],AUSYD,[ANY],[ANY],INTTRA" + Environment.NewLine;
			var expectedValues = new List<object>
			{
				new { id = "1", client = "TESTSENDER__1", carrier = "[ANY]", eventBranch = "[ANY]", port = "[ANY]", shpType = "NVOCC", docName = "[ANY]", provider = "INTTRA" },
				new { id = "2", client = "[ANY]", carrier = "AAAA", eventBranch = "[ANY]", port = "AUSYD", shpType = "VOCC", docName = "[ANY]", provider = "INTTRA" },
				new { id = "default", client = "Default", carrier = "Default", eventBranch = "Default", port = "Default", shpType = "Default", docName = "Default", provider = "Reject" }
			};
			request.Clear();
			request.AddFile("uploadFile", importCSV1);

			var result = controller.ImportCSV();
			Assert.AreEqual("{ success = True, message = CSV file successfully imported. }", result.Data.ToString());

			
			request.Clear();
			request.Container["_search"] = "false";
			request.Container["rows"] = importCSV1.Length.ToString();
			var oceanValues = controller.Values();

			AssertEx.JsonResultEqualList(expectedValues, oceanValues, "oceanMessagingValues");

			request.Clear();
			request.AddFile("uploadFile", importCSV2);

			result = controller.ImportCSV();
			Assert.AreEqual("{ success = True, message = CSV file successfully imported. }", result.Data.ToString());

			result = controller.ImportCSV();
			Assert.AreEqual("{ success = False, message = Stream was not readable. }", result.Data.ToString());

			_backupManager.VerifyAllExpectations();
		}

		[TestMethod]
		public void TestImportCSV_ShouldNotSucceed_IfMissingColumns()
		{
			_backupManager.Expect(x =>
				x.Backup(Arg<HttpServerUtilityBase>.Is.Anything, Arg<string>.Is.Anything, Arg<byte[]>.Is.Anything));
			string importCSV1 = "Client ID,Carrier SCAC/C1C,Event Branch,Shipment Type,Document Name,Service Provider" + Environment.NewLine +
							"TESTSENDER__1,[ANY],[ANY],NVOCC,[ANY],INTTRA" + Environment.NewLine +
							"[ANY],AAAA,[ANY],VOCC,[ANY],INTTRA" + Environment.NewLine +
							"Default,Default,DefaultDefault,Default,Default,Reject" + Environment.NewLine;

			request.Clear();
			request.AddFile("uploadFile", importCSV1);

			var result = controller.ImportCSV();
			Assert.AreEqual("{ success = False, message = The column headers in the csv file does not match the required names. Ensure the following column names are in the csv file: \"Client ID\", \"Carrier SCAC/C1C\", \"Event Branch\", \"Port/Country\", \"Shipment Type\", \"Document Name\", \"Service Provider\" }", result.Data.ToString());

			_backupManager.VerifyAllExpectations();
		}

		[TestMethod]
		public void TestImportCSV_ShouldNotSucceed_IfExtraColumns()
		{
			_backupManager.Expect(x =>
				x.Backup(Arg<HttpServerUtilityBase>.Is.Anything, Arg<string>.Is.Anything, Arg<byte[]>.Is.Anything));
			string importCSV1 = "Client ID,Carrier SCAC/C1C,Event Branch,Port/Country,Shipment Type,Document Name,Document Name,Service Provider" + Environment.NewLine +
							"TESTSENDER__1,[ANY],[ANY],[ANY],NVOCC,[ANY],[ANY],INTTRA" + Environment.NewLine +
							"[ANY],AAAA,[ANY],AUSYD,VOCC,[ANY],[ANY],INTTRA" + Environment.NewLine +
							"Default,Default,DefaultDefault,Default,Default,Reject" + Environment.NewLine;

			request.Clear();
			request.AddFile("uploadFile", importCSV1);

			var result = controller.ImportCSV();
			Assert.AreEqual("{ success = False, message = The column headers in the csv file does not match the required names. Ensure the following column names are in the csv file: \"Client ID\", \"Carrier SCAC/C1C\", \"Event Branch\", \"Port/Country\", \"Shipment Type\", \"Document Name\", \"Service Provider\" }", result.Data.ToString());

			_backupManager.VerifyAllExpectations();
		}

		[TestMethod]
		public void TestImportCSV_ShouldNotSucceed_IfColumnsNotInOrder()
		{
			_backupManager.Expect(x =>
				x.Backup(Arg<HttpServerUtilityBase>.Is.Anything, Arg<string>.Is.Anything, Arg<byte[]>.Is.Anything));
			string importCSV1 = "Document Name,Client ID,Carrier SCAC/C1C,Event Branch,Port/Country,Shipment Type,Service Provider" + Environment.NewLine +
							"[ANY],TESTSENDER__1,[ANY],[ANY],[ANY],NVOCC,INTTRA" + Environment.NewLine +
							"[ANY],[ANY],AAAA,[ANY],AUSYD,VOCC,INTTRA" + Environment.NewLine +
							"Default,Default,DefaultDefault,Default,Default,Reject" + Environment.NewLine;

			request.Clear();
			request.AddFile("uploadFile", importCSV1);

			var result = controller.ImportCSV();
			Assert.AreEqual("{ success = False, message = The column headers in the csv file does not match the required names. Ensure the following column names are in the csv file: \"Client ID\", \"Carrier SCAC/C1C\", \"Event Branch\", \"Port/Country\", \"Shipment Type\", \"Document Name\", \"Service Provider\" }", result.Data.ToString());

			_backupManager.VerifyAllExpectations();
		}

		[TestMethod]
		public void TestImportCSV_ShouldNotSucceed_IfInvalidShipmentType()
		{
			_backupManager.Expect(x =>
				x.Backup(Arg<HttpServerUtilityBase>.Is.Anything, Arg<string>.Is.Anything, Arg<byte[]>.Is.Anything));
			string importCSV1 = "Client ID,Carrier SCAC/C1C,Event Branch,Port/Country,Shipment Type,Document Name,Service Provider" + Environment.NewLine +
							"TESTSENDER__1,[ANY],[ANY],[ANY],NOT,[ANY],INTTRA" + Environment.NewLine +
							"[ANY],AAAA,[ANY],AUSYD,VOCC,[ANY],INTTRA" + Environment.NewLine +
							"Default,Default,DefaultDefault,Default,Default,Reject" + Environment.NewLine;

			request.Clear();
			request.AddFile("uploadFile", importCSV1);

			var result = controller.ImportCSV();
			Assert.AreEqual("{ success = False, message = The Shipment Type \"NOT\" is invalid in row 1 of the csv file. }", result.Data.ToString());

			_backupManager.VerifyAllExpectations();
		}

		[TestMethod]
		public void TestImportCSV_ShouldNotSucceed_IfInvalidDocumentName()
		{
			_backupManager.Expect(x =>
				x.Backup(Arg<HttpServerUtilityBase>.Is.Anything, Arg<string>.Is.Anything, Arg<byte[]>.Is.Anything));
			string importCSV1 = "Client ID,Carrier SCAC/C1C,Event Branch,Port/Country,Shipment Type,Document Name,Service Provider" + Environment.NewLine +
							"TESTSENDER__1,[ANY],[ANY],[ANY],NVOCC,[ANY],INTTRA" + Environment.NewLine +
							"[ANY],AAAA,[ANY],AUSYD,VOCC,NotDocument,INTTRA" + Environment.NewLine +
							"Default,Default,DefaultDefault,Default,Default,Reject" + Environment.NewLine;

			request.Clear();
			request.AddFile("uploadFile", importCSV1);

			var result = controller.ImportCSV();
			Assert.AreEqual("{ success = False, message = The Document Name \"NotDocument\" is invalid in row 2 of the csv file. }", result.Data.ToString());

			_backupManager.VerifyAllExpectations();
		}

		[TestMethod]
		public void TestImportCSV_ShouldNotSucceed_IfInvalidServiceProvider()
		{
			_backupManager.Expect(x =>
				x.Backup(Arg<HttpServerUtilityBase>.Is.Anything, Arg<string>.Is.Anything, Arg<byte[]>.Is.Anything));
			string importCSV1 = "Client ID,Carrier SCAC/C1C,Event Branch,Port/Country,Shipment Type,Document Name,Service Provider" + Environment.NewLine +
							"TESTSENDER__1,[ANY],[ANY],[ANY],NVOCC,[ANY],INTTRA" + Environment.NewLine +
							"[ANY],AAAA,[ANY],AUSYD,VOCC,[ANY],INTTRA" + Environment.NewLine +
							"[ANY],AAAA,[ANY],AUSYD,VOCC,[ANY],NotServiceProvider" + Environment.NewLine +
							"Default,Default,DefaultDefault,Default,Default,Reject" + Environment.NewLine;

			request.Clear();
			request.AddFile("uploadFile", importCSV1);

			var result = controller.ImportCSV();
			Assert.AreEqual("{ success = False, message = The Service Provider \"NotServiceProvider\" is invalid in row 3 of the csv file. }", result.Data.ToString());

			_backupManager.VerifyAllExpectations();
		}


		[TestMethod]
		public void TestImportCSVCarrierAgent()
		{
			_backupManager.Expect(x =>
				x.Backup(Arg<HttpServerUtilityBase>.Is.Anything, Arg<string>.Is.Anything, Arg<byte[]>.Is.Anything));
			controller.Session["OCMTypeID"] = "CarrierBookingAgent";
			string importCSV1 = "Client ID,Carrier SCAC/C1C,Booking Agent C1C,Event Branch,Port/Country,Shipment Type,Document Name,Service Provider" + Environment.NewLine +
							"TESTSENDER__1,[ANY],[ANY],[ANY],[ANY],NVOCC,[ANY],INTTRA" + Environment.NewLine +
							"[ANY],AAAA,BBBB,[ANY],AUSYD,VOCC,[ANY],INTTRA" + Environment.NewLine +
							",,,,,,,INTTRA" + Environment.NewLine +
							"Default,Default,Default,Default,Default,Default,Default,Reject" + Environment.NewLine;
			var expectedValues = new List<object>
			{
				new { id = "1", client = "TESTSENDER__1", carrier = "[ANY]", carrierAgent = "[ANY]", eventBranch = "[ANY]", port = "[ANY]", shpType = "NVOCC", docName = "[ANY]", provider = "INTTRA" },
				new { id = "2", client = "[ANY]", carrier = "AAAA", carrierAgent = "BBBB", eventBranch = "[ANY]", port = "AUSYD", shpType = "VOCC", docName = "[ANY]",  provider = "INTTRA" },
				new { id = "3", client = "[ANY]", carrier = "[ANY]", carrierAgent = "[ANY]", eventBranch = "[ANY]", port = "[ANY]", shpType = "[ANY]", docName = "[ANY]" , provider = "INTTRA" },
				new { id = "default", client = "Default", carrier = "Default", carrierAgent = "Default", eventBranch = "Default", port = "Default", shpType = "Default", docName = "Default", provider = "Reject" }
			};
			request.Clear();
			request.AddFile("uploadFile", importCSV1);

			var result = controller.ImportCSV();
			Assert.AreEqual("{ success = True, message = CSV file successfully imported. }", result.Data.ToString());

			request.Clear();
			request.Container["_search"] = "false";
			request.Container["rows"] = importCSV1.Length.ToString();
			var oceanValues = controller.Values();
			AssertEx.JsonResultEqualList(expectedValues, oceanValues, "oceanMessagingValues");
			_backupManager.VerifyAllExpectations();
		}

		[TestMethod]
		public void TestImportCSVCarrierAgent_ShouldNotSucceed_IfMissingColumns()
		{
			_backupManager.Expect(x =>
				x.Backup(Arg<HttpServerUtilityBase>.Is.Anything, Arg<string>.Is.Anything, Arg<byte[]>.Is.Anything));
			controller.Session["OCMTypeID"] = "CarrierBookingAgent";
			string importCSV1 = "Client ID,Carrier SCAC/C1C,Booking Agent C1C,Port/Country,Shipment Type,Document Name,Service Provider" + Environment.NewLine +
							"TESTSENDER__1,[ANY],[ANY],[ANY],NVOCC,[ANY],INTTRA" + Environment.NewLine +
							"[ANY],AAAA,BBBB,AUSYD,VOCC,[ANY],INTTRA" + Environment.NewLine +
							",,,,,,INTTRA" + Environment.NewLine +
							"Default,Default,Default,Default,Default,Default,Default,Reject" + Environment.NewLine;

			request.Clear();
			request.AddFile("uploadFile", importCSV1);

			var result = controller.ImportCSV();
			Assert.AreEqual("{ success = False, message = The column headers in the csv file does not match the required names. Ensure the following column names are in the csv file: \"Client ID\", \"Carrier SCAC/C1C\", \"Booking Agent C1C\", \"Event Branch\", \"Port/Country\", \"Shipment Type\", \"Document Name\", \"Service Provider\" }", result.Data.ToString());

			_backupManager.VerifyAllExpectations();
		}

		[TestMethod]
		public void TestImportCSVCarrierAgent_ShouldNotSucceed_IfExtraColumns()
		{
			_backupManager.Expect(x =>
				x.Backup(Arg<HttpServerUtilityBase>.Is.Anything, Arg<string>.Is.Anything, Arg<byte[]>.Is.Anything));
			controller.Session["OCMTypeID"] = "CarrierBookingAgent";
			string importCSV1 = "Client ID,Carrier SCAC/C1C,Booking Agent C1C,Booking Agent C1C,Event Branch,Port/Country,Shipment Type,Document Name,Document Name,Service Provider" + Environment.NewLine +
							"TESTSENDER__1,[ANY],[ANY],[ANY],[ANY],NVOCC,[ANY],[ANY],INTTRA" + Environment.NewLine +
							"[ANY],AAAA,[ANY],[ANY],AUSYD,VOCC,[ANY],[ANY],INTTRA" + Environment.NewLine +
							"Default,Default,DefaultDefault,Default,Default,Reject" + Environment.NewLine;

			request.Clear();
			request.AddFile("uploadFile", importCSV1);

			var result = controller.ImportCSV();
			Assert.AreEqual("{ success = False, message = The column headers in the csv file does not match the required names. Ensure the following column names are in the csv file: \"Client ID\", \"Carrier SCAC/C1C\", \"Booking Agent C1C\", \"Event Branch\", \"Port/Country\", \"Shipment Type\", \"Document Name\", \"Service Provider\" }", result.Data.ToString());

			_backupManager.VerifyAllExpectations();
		}

		[TestMethod]
		public void TestImportCSVCarrierAgent_ShouldNotSucceed_IfColumnsNotInOrder()
		{
			_backupManager.Expect(x =>
				x.Backup(Arg<HttpServerUtilityBase>.Is.Anything, Arg<string>.Is.Anything, Arg<byte[]>.Is.Anything));
			controller.Session["OCMTypeID"] = "CarrierBookingAgent";
			string importCSV1 = "Document Name,Client ID,Carrier SCAC/C1C,Booking Agent C1C,Event Branch,Port/Country,Shipment Type,Service Provider" + Environment.NewLine +
							"[ANY],TESTSENDER__1,[ANY],[ANY],[ANY],[ANY],NVOCC,INTTRA" + Environment.NewLine +
							"[ANY],[ANY],AAAA,BBBB,[ANY],AUSYD,VOCC,INTTRA" + Environment.NewLine +
							",,,,,,,INTTRA" + Environment.NewLine +
							"Default,Default,Default,Default,Default,Default,Default,Reject" + Environment.NewLine;

			request.Clear();
			request.AddFile("uploadFile", importCSV1);

			var result = controller.ImportCSV();
			Assert.AreEqual("{ success = False, message = The column headers in the csv file does not match the required names. Ensure the following column names are in the csv file: \"Client ID\", \"Carrier SCAC/C1C\", \"Booking Agent C1C\", \"Event Branch\", \"Port/Country\", \"Shipment Type\", \"Document Name\", \"Service Provider\" }", result.Data.ToString());

			_backupManager.VerifyAllExpectations();
		}

		[TestMethod]
		public void TestImportCSVCarrierAgent_ShouldNotSucceed_IfInvalidShipmentType()
		{
			_backupManager.Expect(x =>
				x.Backup(Arg<HttpServerUtilityBase>.Is.Anything, Arg<string>.Is.Anything, Arg<byte[]>.Is.Anything));
			controller.Session["OCMTypeID"] = "CarrierBookingAgent";
			string importCSV1 = "Client ID,Carrier SCAC/C1C,Booking Agent C1C,Event Branch,Port/Country,Shipment Type,Document Name,Service Provider" + Environment.NewLine +
							"TESTSENDER__1,[ANY],[ANY],[ANY],[ANY],NOT,[ANY],INTTRA" + Environment.NewLine +
							"[ANY],AAAA,BBBB,[ANY],AUSYD,VOCC,[ANY],INTTRA" + Environment.NewLine +
							",,,,,,,INTTRA" + Environment.NewLine +
							"Default,Default,Default,Default,Default,Default,Default,Reject" + Environment.NewLine;

			request.Clear();
			request.AddFile("uploadFile", importCSV1);

			var result = controller.ImportCSV();
			Assert.AreEqual("{ success = False, message = The Shipment Type \"NOT\" is invalid in row 1 of the csv file. }", result.Data.ToString());

			_backupManager.VerifyAllExpectations();
		}

		[TestMethod]
		public void TestImportCSVCarrierAgent_ShouldNotSucceed_IfInvalidDocumentName()
		{
			_backupManager.Expect(x =>
				x.Backup(Arg<HttpServerUtilityBase>.Is.Anything, Arg<string>.Is.Anything, Arg<byte[]>.Is.Anything));
			controller.Session["OCMTypeID"] = "CarrierBookingAgent";
			string importCSV1 = "Client ID,Carrier SCAC/C1C,Booking Agent C1C,Event Branch,Port/Country,Shipment Type,Document Name,Service Provider" + Environment.NewLine +
							"TESTSENDER__1,[ANY],[ANY],[ANY],[ANY],NVOCC,[ANY],INTTRA" + Environment.NewLine +
							"[ANY],AAAA,BBBB,[ANY],AUSYD,VOCC,NotDocument,INTTRA" + Environment.NewLine +
							",,,,,,,INTTRA" + Environment.NewLine +
							"Default,Default,Default,Default,Default,Default,Default,Reject" + Environment.NewLine;

			request.Clear();
			request.AddFile("uploadFile", importCSV1);

			var result = controller.ImportCSV();
			Assert.AreEqual("{ success = False, message = The Document Name \"NotDocument\" is invalid in row 2 of the csv file. }", result.Data.ToString());

			_backupManager.VerifyAllExpectations();
		}

		[TestMethod]
		public void TestImportCSVCarrierAgent_ShouldNotSucceed_IfInvalidServiceProvider()
		{
			_backupManager.Expect(x =>
				x.Backup(Arg<HttpServerUtilityBase>.Is.Anything, Arg<string>.Is.Anything, Arg<byte[]>.Is.Anything));
			controller.Session["OCMTypeID"] = "CarrierBookingAgent";
			string importCSV1 = "Client ID,Carrier SCAC/C1C,Booking Agent C1C,Event Branch,Port/Country,Shipment Type,Document Name,Service Provider" + Environment.NewLine +
							"TESTSENDER__1,[ANY],[ANY],[ANY],[ANY],NVOCC,[ANY],INTTRA" + Environment.NewLine +
							"[ANY],AAAA,BBBB,[ANY],AUSYD,VOCC,[ANY],INTTRA" + Environment.NewLine +
							",,,,,,,NotServiceProvider" + Environment.NewLine +
							"Default,Default,Default,Default,Default,Default,Default,Reject" + Environment.NewLine;

			request.Clear();
			request.AddFile("uploadFile", importCSV1);

			var result = controller.ImportCSV();
			Assert.AreEqual("{ success = False, message = The Service Provider \"NotServiceProvider\" is invalid in row 3 of the csv file. }", result.Data.ToString());

			_backupManager.VerifyAllExpectations();
		}

		[TestMethod]
		public void TestImportCSVCarrierHandlingAgent()
		{
			_backupManager.Expect(x =>
				x.Backup(Arg<HttpServerUtilityBase>.Is.Anything, Arg<string>.Is.Anything, Arg<byte[]>.Is.Anything));
			controller.Session["OCMTypeID"] = "CarrierHandlingAgent";
			string importCSV1 = "Client ID,Carrier SCAC/C1C,Handling Agent C1C,Event Branch,Port/Country,Shipment Type,Document Name,Service Provider" + Environment.NewLine +
							"TESTSENDER__1,[ANY],[ANY],[ANY],[ANY],NVOCC,[ANY],INTTRA" + Environment.NewLine +
							"[ANY],AAAA,BBBB,[ANY],AUSYD,VOCC,[ANY],INTTRA" + Environment.NewLine +
							",,,,,,,INTTRA" + Environment.NewLine +
							"Default,Default,Default,Default,Default,Default,Default,Reject" + Environment.NewLine;
			var expectedValues = new List<object>
			{
				new { id = "1", client = "TESTSENDER__1", carrier = "[ANY]", carrierAgent = "[ANY]", eventBranch = "[ANY]", port = "[ANY]", shpType = "NVOCC", docName = "[ANY]", provider = "INTTRA" },
				new { id = "2", client = "[ANY]", carrier = "AAAA", carrierAgent = "BBBB", eventBranch = "[ANY]", port = "AUSYD", shpType = "VOCC", docName = "[ANY]", provider = "INTTRA" },
				new { id = "3", client = "[ANY]", carrier = "[ANY]", carrierAgent = "[ANY]", eventBranch = "[ANY]", port = "[ANY]", shpType = "[ANY]", docName = "[ANY]", provider = "INTTRA" },
				new { id = "default", client = "Default", carrier = "Default", carrierAgent = "Default", eventBranch = "Default", port = "Default", shpType = "Default", docName = "Default", provider = "Reject" }
			};
			request.Clear();
			request.AddFile("uploadFile", importCSV1);

			var result = controller.ImportCSV();
			Assert.AreEqual("{ success = True, message = CSV file successfully imported. }", result.Data.ToString());

			request.Clear();
			request.Container["_search"] = "false";
			request.Container["rows"] = importCSV1.Length.ToString();
			var oceanValues = controller.Values();
			AssertEx.JsonResultEqualList(expectedValues, oceanValues, "oceanMessagingValues");
			_backupManager.VerifyAllExpectations();
		}

		[TestMethod]
		public void TestImportCSVCarrierHandlingAgent_ShouldNotSucceed_IfMissingColumns()
		{
			_backupManager.Expect(x =>
				x.Backup(Arg<HttpServerUtilityBase>.Is.Anything, Arg<string>.Is.Anything, Arg<byte[]>.Is.Anything));
			controller.Session["OCMTypeID"] = "CarrierHandlingAgent";
			string importCSV1 = "Client ID,Carrier SCAC/C1C,Handling Agent C1C,Port/Country,Shipment Type,Document Name,Service Provider" + Environment.NewLine +
							"TESTSENDER__1,[ANY],[ANY],[ANY],NVOCC,[ANY],INTTRA" + Environment.NewLine +
							"[ANY],AAAA,BBBB,AUSYD,VOCC,[ANY],INTTRA" + Environment.NewLine +
							",,,,,,INTTRA" + Environment.NewLine +
							"Default,Default,Default,Default,Default,Default,Default,Reject" + Environment.NewLine;

			request.Clear();
			request.AddFile("uploadFile", importCSV1);

			var result = controller.ImportCSV();
			Assert.AreEqual("{ success = False, message = The column headers in the csv file does not match the required names. Ensure the following column names are in the csv file: \"Client ID\", \"Carrier SCAC/C1C\", \"Handling Agent C1C\", \"Event Branch\", \"Port/Country\", \"Shipment Type\", \"Document Name\", \"Service Provider\" }", result.Data.ToString());

			_backupManager.VerifyAllExpectations();
		}

		[TestMethod]
		public void TestImportCSVCarrierHandlingAgent_ShouldNotSucceed_IfExtraColumns()
		{
			_backupManager.Expect(x =>
				x.Backup(Arg<HttpServerUtilityBase>.Is.Anything, Arg<string>.Is.Anything, Arg<byte[]>.Is.Anything));
			controller.Session["OCMTypeID"] = "CarrierHandlingAgent";
			string importCSV1 = "Client ID,Carrier SCAC/C1C,Handling Agent C1C,Booking Agent C1C,Event Branch,Port/Country,Shipment Type,Document Name,Document Name,Service Provider" + Environment.NewLine +
							"TESTSENDER__1,[ANY],[ANY],[ANY],[ANY],NVOCC,[ANY],[ANY],INTTRA" + Environment.NewLine +
							"[ANY],AAAA,[ANY],[ANY],AUSYD,VOCC,[ANY],[ANY],INTTRA" + Environment.NewLine +
							"Default,Default,DefaultDefault,Default,Default,Reject" + Environment.NewLine;

			request.Clear();
			request.AddFile("uploadFile", importCSV1);

			var result = controller.ImportCSV();
			Assert.AreEqual("{ success = False, message = The column headers in the csv file does not match the required names. Ensure the following column names are in the csv file: \"Client ID\", \"Carrier SCAC/C1C\", \"Handling Agent C1C\", \"Event Branch\", \"Port/Country\", \"Shipment Type\", \"Document Name\", \"Service Provider\" }", result.Data.ToString());

			_backupManager.VerifyAllExpectations();
		}

		[TestMethod]
		public void TestImportCSVCarrierHandlingAgent_ShouldNotSucceed_IfColumnsNotInOrder()
		{
			_backupManager.Expect(x =>
				x.Backup(Arg<HttpServerUtilityBase>.Is.Anything, Arg<string>.Is.Anything, Arg<byte[]>.Is.Anything));
			controller.Session["OCMTypeID"] = "CarrierHandlingAgent";
			string importCSV1 = "Document Name,Client ID,Carrier SCAC/C1C,Handling Agent C1C,Event Branch,Port/Country,Shipment Type,Service Provider" + Environment.NewLine +
							"[ANY],TESTSENDER__1,[ANY],[ANY],[ANY],[ANY],NVOCC,INTTRA" + Environment.NewLine +
							"[ANY],[ANY],AAAA,BBBB,[ANY],AUSYD,VOCC,INTTRA" + Environment.NewLine +
							",,,,,,,INTTRA" + Environment.NewLine +
							"Default,Default,Default,Default,Default,Default,Default,Reject" + Environment.NewLine;

			request.Clear();
			request.AddFile("uploadFile", importCSV1);

			var result = controller.ImportCSV();
			Assert.AreEqual("{ success = False, message = The column headers in the csv file does not match the required names. Ensure the following column names are in the csv file: \"Client ID\", \"Carrier SCAC/C1C\", \"Handling Agent C1C\", \"Event Branch\", \"Port/Country\", \"Shipment Type\", \"Document Name\", \"Service Provider\" }", result.Data.ToString());

			_backupManager.VerifyAllExpectations();
		}

		[TestMethod]
		public void TestImportCSVCarrierHandlingAgent_ShouldNotSucceed_IfInvalidShipmentType()
		{
			_backupManager.Expect(x =>
				x.Backup(Arg<HttpServerUtilityBase>.Is.Anything, Arg<string>.Is.Anything, Arg<byte[]>.Is.Anything));
			controller.Session["OCMTypeID"] = "CarrierHandlingAgent";
			string importCSV1 = "Client ID,Carrier SCAC/C1C,Handling Agent C1C,Event Branch,Port/Country,Shipment Type,Document Name,Service Provider" + Environment.NewLine +
							"TESTSENDER__1,[ANY],[ANY],[ANY],[ANY],NOT,[ANY],INTTRA" + Environment.NewLine +
							"[ANY],AAAA,BBBB,[ANY],AUSYD,VOCC,[ANY],INTTRA" + Environment.NewLine +
							",,,,,,,INTTRA" + Environment.NewLine +
							"Default,Default,Default,Default,Default,Default,Default,Reject" + Environment.NewLine;

			request.Clear();
			request.AddFile("uploadFile", importCSV1);

			var result = controller.ImportCSV();
			Assert.AreEqual("{ success = False, message = The Shipment Type \"NOT\" is invalid in row 1 of the csv file. }", result.Data.ToString());

			_backupManager.VerifyAllExpectations();
		}

		[TestMethod]
		public void TestImportCSVCarrierHandlingAgent_ShouldNotSucceed_IfInvalidDocumentName()
		{
			_backupManager.Expect(x =>
				x.Backup(Arg<HttpServerUtilityBase>.Is.Anything, Arg<string>.Is.Anything, Arg<byte[]>.Is.Anything));
			controller.Session["OCMTypeID"] = "CarrierHandlingAgent";
			string importCSV1 = "Client ID,Carrier SCAC/C1C,Handling Agent C1C,Event Branch,Port/Country,Shipment Type,Document Name,Service Provider" + Environment.NewLine +
							"TESTSENDER__1,[ANY],[ANY],[ANY],[ANY],NVOCC,[ANY],INTTRA" + Environment.NewLine +
							"[ANY],AAAA,BBBB,[ANY],AUSYD,VOCC,NotDocument,INTTRA" + Environment.NewLine +
							",,,,,,,INTTRA" + Environment.NewLine +
							"Default,Default,Default,Default,Default,Default,Default,Reject" + Environment.NewLine;

			request.Clear();
			request.AddFile("uploadFile", importCSV1);

			var result = controller.ImportCSV();
			Assert.AreEqual("{ success = False, message = The Document Name \"NotDocument\" is invalid in row 2 of the csv file. }", result.Data.ToString());

			_backupManager.VerifyAllExpectations();
		}

		[TestMethod]
		public void TestImportCSVCarrierHandlingAgent_ShouldNotSucceed_IfInvalidServiceProvider()
		{
			_backupManager.Expect(x =>
				x.Backup(Arg<HttpServerUtilityBase>.Is.Anything, Arg<string>.Is.Anything, Arg<byte[]>.Is.Anything));
			controller.Session["OCMTypeID"] = "CarrierHandlingAgent";
			string importCSV1 = "Client ID,Carrier SCAC/C1C,Handling Agent C1C,Event Branch,Port/Country,Shipment Type,Document Name,Service Provider" + Environment.NewLine +
							"TESTSENDER__1,[ANY],[ANY],[ANY],[ANY],NVOCC,[ANY],INTTRA" + Environment.NewLine +
							"[ANY],AAAA,BBBB,[ANY],AUSYD,VOCC,[ANY],INTTRA" + Environment.NewLine +
							",,,,,,,NotServiceProvider" + Environment.NewLine +
							"Default,Default,Default,Default,Default,Default,Default,Reject" + Environment.NewLine;

			request.Clear();
			request.AddFile("uploadFile", importCSV1);

			var result = controller.ImportCSV();
			Assert.AreEqual("{ success = False, message = The Service Provider \"NotServiceProvider\" is invalid in row 3 of the csv file. }", result.Data.ToString());

			_backupManager.VerifyAllExpectations();
		}

		[TestMethod]
		public void TestExtendSession()
		{
			Assert.AreEqual(new EmptyResult().ToString(), controller.ExtendSession().ToString());
		}

        [TestMethod]
        public void TestCsvLoggerDebug_CallTest()
        {
            var abstractOceanCarrierMessagingTypeMock = MockRepository.GeneratePartialMock<AbstractOceanCarrierMessagingType>(controller, context);
            //var memoryStream = new MemoryStream();
            //var streamWriter = new StreamWriter(memoryStream);
            //var postedFileBase = MockRepository.GenerateStub<HttpPostedFileBase>();
            var row = new Row {client = "[ANY]", carrier = "YMLU", carrierAgent = "C1H1", port = "CN*", shpType = "VOCC", docName = "eManifest", provider = "EASIPASS"};
            LinkedList<Row> testLinkedList = new LinkedList<Row>();
            {
                testLinkedList.AddFirst(row);
            }
            string[] testColumns = { Row.RowNames.id, Row.RowNames.client, Row.RowNames.carrier, Row.RowNames.carrierAgent, Row.RowNames.port, Row.RowNames.shpType, Row.RowNames.docName, Row.RowNames.provider, Row.RowNames.purpose, Row.RowNames.splitBy, Row.RowNames.shipNamespace, Row.RowNames.partyToCopy };
            //string testString = "client='[ANY]', carrier='YMLU', carrierAgent='C1H1', port='CN*', shpType='VOCC', docName='eManifest', provider='EASIPASS'";
            //streamWriter.WriteLine("Client ID,Carrier SCAC,Port/Country,Shipment Type,Document Name,Service Provider");
            //streamWriter.WriteLine(testString);
            //streamWriter.Flush();
            //memoryStream.Position = 0;

            //postedFileBase.Stub(x => x.InputStream).Return(memoryStream);
            abstractOceanCarrierMessagingTypeMock.Stub(Mock => Mock.JqGridColumns()).Return(testColumns);
            abstractOceanCarrierMessagingTypeMock.Stub(Mock => Mock.Name).Return("MockType");
            abstractOceanCarrierMessagingTypeMock.Stub(Mock => Mock.ID).Return("MockType");
            abstractOceanCarrierMessagingTypeMock.Stub(Mock => Mock.ValuesDatabase()).Return(testLinkedList.ToList());
            abstractOceanCarrierMessagingTypeMock.CurrentSessionCache = testLinkedList;
            abstractOceanCarrierMessagingTypeMock.Stub(Mock => Mock.SaveToDatabase(testLinkedList));
            abstractOceanCarrierMessagingTypeMock.Expect(Mock => Mock.CsvLoggerDebug(null, null)).IgnoreArguments()
                .Repeat.Once();

            AbstractOceanCarrierMessagingType.CsvLogger = GetTestLogger("OCMCsvLogger");
            Assert.AreEqual(true, AbstractOceanCarrierMessagingType.CsvLogger.IsDebugEnabled, "Precondition");
            abstractOceanCarrierMessagingTypeMock.SaveCore();
            
            abstractOceanCarrierMessagingTypeMock.VerifyAllExpectations();
        }

        [TestMethod]
        public void TestLoggerDebug_OutputTest()
        {
            request.Clear();
            request.Container["id"] = "n0";
            request.Container["afterRowId"] = "default";
            request.Container["oper"] = "add";
            request.Container["rows"] = "10";
            request.Container["_search"] = "false";
            request.Container["rowLimit"] = "4";
            request.Container["provider"] = "GTNEXUS";
            request.Container["client"] = "[ANY]";
            request.Container["carrier"] = "[ANY]";
			request.Container["eventBranch"] = "[ANY]";
			request.Container["docName"] = "Verified Gross Container Weight";
            request.Container["shpType"] = "NVOCC";

            var success = controller.ValuesEdit();
            var oceanValues = controller.Values();

            AbstractOceanCarrierMessagingType.CsvLogger = GetTestLogger("OCMCsvLogger");
            Assert.AreEqual(true, AbstractOceanCarrierMessagingType.CsvLogger.IsDebugEnabled, "Precondition");
            var logString = CaptureConsoleLog(() => controller.SaveAll());
			var expectedLogEntry = Regex.Replace(@"[DEBUG]OCMCsvLogger-[DefaultCarrier]CORP\Test.Namesavedroutingrules:client='TESTSENDER__1',carrier='[ANY]',provider='INTTRA',port='[ANY]',docName='[ANY]',shpType='[ANY]',eventBranch='[ANY]'client='[ANY]',carrier='AAAA',provider='INTTRA',port='[ANY]',docName='[ANY]',shpType='VOCC',eventBranch='[ANY]'client='TESTSENDER__1',carrier='AAAA',provider='INTTRA',port='[ANY]',docName='[ANY]',shpType='NVOCC',eventBranch='[ANY]'client='TEST*',carrier='AAAA',provider='INTTRA',port='[ANY]',docName='ShippingInstruction',shpType='[ANY]',eventBranch='[ANY]'client='[ANY]',carrier='[ANY]',provider='GTNEXUS',port='[ANY]',docName='VerifiedGrossContainerWeight',shpType='NVOCC',eventBranch='[ANY]'client='Default',carrier='Default',provider='Reject',port='Default',docName='Default',shpType='Default',eventBranch='Default'----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------[DefaultCarrier]PreviousState:client='TESTSENDER__1',carrier='[ANY]',provider='INTTRA',port='[ANY]',docName='[ANY]',shpType='[ANY]',eventBranch='[ANY]'client='[ANY]',carrier='AAAA',provider='INTTRA',port='[ANY]',docName='[ANY]',shpType='VOCC',eventBranch='[ANY]'client='TESTSENDER__1',carrier='AAAA',provider='INTTRA',port='[ANY]',docName='[ANY]',shpType='NVOCC',eventBranch='[ANY]'client='TEST*',carrier='AAAA',provider='INTTRA',port='[ANY]',docName='ShippingInstruction',shpType='[ANY]',eventBranch='[ANY]'client='Default',carrier='Default',provider='Reject',port='Default',docName='Default',shpType='Default',eventBranch='Default'"
			, @"\s +", string.Empty);

			StringAssert.Contains(Regex.Replace(logString, @"\s+", string.Empty), expectedLogEntry);
        }

		[TestMethod]
		public void TestOCMLogger_OutputTest()
		{
			request.Clear();
			request.Container["id"] = "n0";
			request.Container["afterRowId"] = "default";
			request.Container["oper"] = "add";
			request.Container["rows"] = "10";
			request.Container["_search"] = "false";
			request.Container["rowLimit"] = "4";
			request.Container["provider"] = "GTNEXUS";
			request.Container["client"] = "[ANY]";
			request.Container["carrier"] = "[ANY]";
			request.Container["docName"] = "Verified Gross Container Weight";
			request.Container["shpType"] = "NVOCC";

			var success = controller.ValuesEdit();
			var oceanValues = controller.Values();

			AbstractOceanCarrierMessagingType.OCMLogger = GetTestLogger("OCMSavingLogger");
			Assert.AreEqual(true, AbstractOceanCarrierMessagingType.OCMLogger.IsDebugEnabled, "Precondition");
			var logString = CaptureConsoleLog(() => controller.SaveAll());

			var expectedLogEntry = "Finding group rules for [DefaultCarrier]";
			StringAssert.Contains(logString, expectedLogEntry);

			expectedLogEntry = "Successfully found group rules for [DefaultCarrier]";
			StringAssert.Contains(logString, expectedLogEntry);

			expectedLogEntry = "[CORP\\Test.Name][Default Carrier] Saving routing rules data.";
			StringAssert.Contains(logString, expectedLogEntry);

			expectedLogEntry = "[CORP\\Test.Name][Default Carrier] Saved routing rules data.";
			StringAssert.Contains(logString, expectedLogEntry);

			expectedLogEntry = "Adding 'Insert' and 'Delete' to the context started.";
			StringAssert.Contains(logString, expectedLogEntry);

			expectedLogEntry = "'Insert' and 'Delete' have been added to the context after calling RoutingRuleEngine and before calling 'SaveChanges'.";
			StringAssert.Contains(logString, expectedLogEntry);

			expectedLogEntry = "Changes have been saved to the database.";
			StringAssert.Contains(logString, expectedLogEntry);

			expectedLogEntry = "Old Rules are deleted from the Context";
			StringAssert.Contains(logString, expectedLogEntry);

			expectedLogEntry = "Start deleting old rules from the Context.";
			StringAssert.Contains(logString, expectedLogEntry);

			expectedLogEntry = "New rules are added to the Context.";
			StringAssert.Contains(logString, expectedLogEntry);

			expectedLogEntry = "Start adding new rules to the Context.";
			StringAssert.Contains(logString, expectedLogEntry);
		}

		private ILog GetTestLogger(string name)
        {
            var properties = new NameValueCollection();
            properties["level"] = "Trace";
            properties["showLogName"] = "true";
            properties["showDateTime"] = "true";
            properties["dateTimeFormat"] = "yyyy/MM/dd HH:mm:ss:fff";
            var loggerAdapter = new ConsoleOutLoggerFactoryAdapter(properties);
            return loggerAdapter.GetLogger(name);
        }
	}
}
