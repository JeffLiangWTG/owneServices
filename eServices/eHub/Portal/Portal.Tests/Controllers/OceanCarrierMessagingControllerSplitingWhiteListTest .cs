extern alias DataModelShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using CargoWise.eHub.Portal.Controllers.Service;
using CargoWise.eHub.Portal.Models.View.OceanCarrierMessaging;
using CargoWise.eHub.Portal.Tests.Model;
using CargoWise.eHub.Portal.Tests.TestHelpers;
using DataModelShared::eServices.eHubDataModel.Common;
using eServices.eHubDataModel.eHubTransactions;
using eServices.eHubRoutingRuleEngine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Portal.Tests.Controllers
{
	[TestClass]
	public class OceanCarrierMessagingControllerSplitingWhiteListTest
	{
		static TestRequest request;
		static OceanCarrierMessagingController controller;
		static eHubTransactionsContext context;

		static TestDbSet<eHubRoutingRule> testRules = new TestDbSet<eHubRoutingRule>();
		static eHubClient clientService = new eHubClient() { CC_ID = "SHIPPING_INSTRUCTION", CC_FriendlyName = "Shipping Instruction" };
		static eHubClient clientSubShipment = new eHubClient() { CC_ID = "OCM_SUBSHIPMENT_SPLIT"};
		static eHubClient clientContainer = new eHubClient() { CC_ID = "OCM_CONTAINER_SPLIT" };
		static eHubClient clientTestSender = new eHubClient() { CC_ID = "TESTSENDER__1" };


		[ClassInitialize()]
		public static void ClassInit(TestContext testContext)
		{
			request = new TestRequest();
            var fakeIdentity = new GenericIdentity("CORP\\Test.Name");
            var principal = new GenericPrincipal(fakeIdentity, null);

            var httpContext = MockRepository.GenerateMock<HttpContextBase>();
			httpContext.Stub(x => x.Request).Return(request);

			httpContext.Stub(c => c.Session).Return(new FakeSessionState());
			httpContext.Stub(c => c.Response).Return(new FakeResponseState());
            httpContext.Stub(x => x.User).Return(principal);

            var routeData = new RouteData();
			context = MockRepository.GenerateMock<eHubTransactionsContext>();

			var testClients = new TestDbSet<eHubClient>();
			context.Stub(x => x.eHubRoutingRules).Return(testRules);
			context.Stub(x => x.eHubClients).Return(testClients);

			testClients.Add(clientService);
			testClients.Add(clientSubShipment);
			testClients.Add(clientContainer);
			testClients.Add(clientTestSender);

			controller = new OceanCarrierMessagingController(context);
			controller.ControllerContext = new ControllerContext(httpContext, routeData, controller);
		}

		[TestInitialize()]
		public void Initialize()
		{
			controller.Session.Clear();
			clientService.eHubRoutingRule = testRules.Add(new eHubRoutingRule
			{
				RR_Group_MatchMultiple = false,
				eHubRoutingRules_Group = testRules.AddRange(new[] {
					new eHubRoutingRule {
						RR_Group_MatchMultiple = false,
						RR_Group_Ordering = 100,
						RR_Group_Name = "SplitingBlackList"
					},
					new eHubRoutingRule {
						RR_Group_MatchMultiple = false,
						RR_Group_Ordering = 200,
						RR_Group_Name = "SplitingWhiteList",
						eHubRoutingRules_Group = testRules.AddRange(new[]
						{
							new eHubRoutingRule { RR_Condition_Expression = "[@SourceParty,Equal,TESTSENDER__WhiteList] && [@SubShipmentCount,NotEqual,0] && [@SubShipmentCount,NotEqual,1]]&& [@Purpose,Equal,WTH]",
								eHubClient_Recipient = null, RR_Result_Value = "OCM_SUBSHIPMENT_SPLIT", RR_Group_Ordering = 1000 },
							new eHubRoutingRule { RR_Condition_Expression = "[@SourceParty,Equal,TESTSENDER__WhiteList] && [@ContainerCount,NotEqual,0] && [@ContainerCount,NotEqual,1]",
								eHubClient_Recipient = null, RR_Result_Value = "OCM_CONTAINER_SPLIT", RR_Group_Ordering = 2000 }
						}).ToList()
					},
					new eHubRoutingRule {
						RR_Group_MatchMultiple = false,
						RR_Group_Ordering = 1000,
						RR_Group_Name = "CarrierHandlingAgent"
					},
					new eHubRoutingRule {
						RR_Group_MatchMultiple = false,
						RR_Group_Ordering = 2000,
						RR_Group_Name = "CarrierBookingAgent"
					},
					new eHubRoutingRule {
						RR_Group_MatchMultiple = false,
						RR_Group_Ordering = 3000,
						RR_Group_Name = "DefaultCarrier"
					}
				}).ToList()
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
		public void TestSelectWhiteListType()
		{
			request.Clear();
			request.Container["id"] = "SplitingWhiteList";

			var result = controller.SelectType();
			Assert.AreEqual("SplitingWhiteList", controller.Session["OCMTypeID"]);
			AssertEx.JsonResultMatchesList(new[] { Row.RowNames.client, Row.RowNames.purpose, Row.RowNames.port, Row.RowNames.docName, Row.RowNames.shipNamespace, Row.RowNames.splitBy}, result, "rows");
		}

		[TestMethod]
		public void TestWhiteListValuesDatabase()
		{

			var expectedSplitingWhiteList = new List<object>
			{
				new { id = "1", client = "TESTSENDER__WhiteList", purpose = "WTH", port = "[ANY]",  docName = "[ANY]", shipNamespace = "[ANY]", splitBy = "SubShipment"},
				new { id = "2", client = "TESTSENDER__WhiteList", purpose = "[ANY]", port = "[ANY]",  docName = "[ANY]", shipNamespace = "[ANY]", splitBy = "Container"}
			};

			request.Clear();
			request.Container["id"] = "SplitingWhiteList";
			request.Container["rows"] = "2";
			request.Container["_search"] = "false";
			controller.SelectType();
			Assert.AreEqual("SplitingWhiteList", controller.Session["OCMTypeID"]);
			var result = controller.Values();
			AssertEx.JsonResultMatchesList(expectedSplitingWhiteList, result, "oceanMessagingValues");
		}

		[TestMethod]
		public void TestSplitByGet()
		{
			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["sidx"] = "docName";
			request.Container["_search"] = "false";
			var expected = new List<object>
			{
				new { splitOption = "SubShipment"},
				new { splitOption = "Container"}
			};

			var result = controller.SplitByOptions();

			Assert.IsNotNull(result);
			AssertEx.JsonResultMatchesList(expected, result, "splitByOptions");

			request.Container["sord"] = "desc"; // Because we use jqgrid's getonce, we don't need any async sord, filter.
			result = controller.SplitByOptions();

			Assert.IsNotNull(result);
			AssertEx.JsonResultMatchesList(expected, result, "splitByOptions");
		}

		[TestMethod]
		public void TestSplitWhiteListTableValues()
		{
			var expectedPurpose = new List<object>
			{
				new { id = "1", client = "TESTSENDER__WhiteList", purpose = "WTH", port = "[ANY]", docName = "[ANY]", shipNamespace = "[ANY]", splitBy = "SubShipment"}
			};

			var expectedEmpty = new List<object>();

			var expectedSubShipmentList = new List<object>
			{
				new { id = "1", client = "TESTSENDER__WhiteList", purpose = "WTH", port = "[ANY]", docName = "[ANY]", shipNamespace = "[ANY]",  splitBy = "SubShipment"}
			};

			var expectedContainerList = new List<object>
			{
				new { id = "2", client = "TESTSENDER__WhiteList", purpose = "[ANY]", port = "[ANY]", docName = "[ANY]", shipNamespace = "[ANY]",  splitBy = "Container"},
			};

			request.Clear();
			request.Container["rows"] = "1";
			request.Container["id"] = "SplitingWhiteList";
			controller.SelectType();
			Assert.AreEqual("SplitingWhiteList", controller.Session["OCMTypeID"]);

			// Test purpose
			request.Container["_search"] = "true";
			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"purpose\",\"op\":\"eq\",\"data\":\"WTH\"}]}";

			var result = controller.Values();
			AssertEx.JsonResultMatchesList(expectedPurpose, result, "oceanMessagingValues");

			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"purpose\",\"op\":\"bw\",\"data\":\"W\"}]}";

			result = controller.Values();
			AssertEx.JsonResultMatchesList(expectedPurpose, result, "oceanMessagingValues");

			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"purpose\",\"op\":\"ew\",\"data\":\"H\"}]}";

			result = controller.Values();
			AssertEx.JsonResultMatchesList(expectedPurpose, result, "oceanMessagingValues");

			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"purpose\",\"op\":\"cn\",\"data\":\"WI\"}]}";

			result = controller.Values();
			AssertEx.JsonResultMatchesList(expectedEmpty, result, "oceanMessagingValues", true);

			// Test SplitBy
			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"splitBy\",\"op\":\"eq\",\"data\":\"subShipment\"}]}";

			result = controller.Values();
			AssertEx.JsonResultMatchesList(expectedSubShipmentList, result, "oceanMessagingValues");

			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"splitBy\",\"op\":\"cn\",\"data\":\"Container\"}]}";

			result = controller.Values();
			AssertEx.JsonResultMatchesList(expectedContainerList, result, "oceanMessagingValues");

			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"splitBy\",\"op\":\"bw\",\"data\":\"Sub\"}]}";

			result = controller.Values();
			AssertEx.JsonResultMatchesList(expectedSubShipmentList, result, "oceanMessagingValues");

			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"splitBy\",\"op\":\"ew\",\"data\":\"S\"}]}";

			result = controller.Values();
			AssertEx.JsonResultMatchesList(expectedEmpty, result, "oceanMessagingValues", true);
		}

		[TestMethod]
		public void TestEditAndSaveSplitListValues()
		{
			var expectedAdd = new List<object>
			{
				new { id = "1", client = "TESTSENDER__WhiteList", purpose = "WTH", port = "[ANY]", docName = "[ANY]",  shipNamespace = "[ANY]", splitBy = "SubShipment"},
				new { id = "2", client = "TESTSENDER__WhiteList", purpose = "[ANY]", port = "[ANY]", docName = "[ANY]", shipNamespace = "[ANY]", splitBy = "Container"},
				new { id = "3", client = "[ANY]", purpose = "[ANY]", port = "[ANY]", docName = "[ANY]",  shipNamespace = "[ANY]", splitBy = "Container"}
			};

			var expectedEdit = new List<object>
			{
				new { id = "1", client = "TESTSENDER__WhiteList", purpose = "WTH", port = "TestPort", docName = "[ANY]",  shipNamespace = "http://www.cargowise.com/Schemas/Universal/2012/11/ContainerGrossWeightVerification/2", splitBy = "Container"},
				new { id = "2", client = "TESTSENDER__WhiteList", purpose = "[ANY]", port = "[ANY]", docName = "[ANY]", shipNamespace = "[ANY]", splitBy = "Container"},
				new { id = "3", client = "[ANY]", purpose = "[ANY]", port = "[ANY]", docName = "[ANY]",  shipNamespace = "[ANY]", splitBy = "Container"}
			};

			var expectedDelete = new List<object>
			{
				new { id = "1", client = "TESTSENDER__WhiteList", purpose = "WTH", port = "TestPort", docName = "[ANY]",  shipNamespace = "http://www.cargowise.com/Schemas/Universal/2012/11/ContainerGrossWeightVerification/2", splitBy = "Container"},
				new { id = "2", client = "TESTSENDER__WhiteList", purpose = "[ANY]", port = "[ANY]", docName = "[ANY]", shipNamespace = "[ANY]", splitBy = "Container"}
			};


			request.Container["id"] = "SplitingWhiteList";
			controller.SelectType();
			//Add
			request.Clear();
			request.Container["rows"] = "10";
			request.Container["_search"] = "false";
			request.Container["rowLimit"] = "4";
			request.Container["id"] = "3";
			request.Container["oper"] = "add";
			request.Container["client"] = "[ANY]";
			request.Container["port"] = "[ANY]";
			request.Container["docName"] = "[ANY]";
			request.Container["shipNamespace"] = "[ANY]";
			request.Container["purpose"] = "[ANY]";
			request.Container["splitBy"] = "Container";

			var success = controller.ValuesEdit();
			var result = controller.Values();
			Assert.AreEqual("{ success = True, id = 3 }", success.Data.ToString());
			AssertEx.JsonResultMatchesList(expectedAdd, result, "oceanMessagingValues");

			//Edit
			request.Clear();
			request.Container["rows"] = "10";
			request.Container["_search"] = "false";
			request.Container["rowLimit"] = "4";
			request.Container["id"] = "1";
			request.Container["oper"] = "edit";
			request.Container["client"] = "TESTSENDER__WhiteList";
			request.Container["port"] = "TestPort";
			request.Container["docName"] = "[ANY]";
			request.Container["shipNamespace"] = "http://www.cargowise.com/Schemas/Universal/2012/11/ContainerGrossWeightVerification/2";
			request.Container["purpose"] = "WTH";
			request.Container["splitBy"] = "Container";

			success = controller.ValuesEdit();
			result = controller.Values();

			Assert.AreEqual("{ success = True, id = 1 }", success.Data.ToString());
			AssertEx.JsonResultMatchesList(expectedEdit, result, "oceanMessagingValues");

			//Delete
			request.Clear();
			request.Container["rows"] = "10";
			request.Container["_search"] = "false";
			request.Container["rowLimit"] = "3";
			request.Container["id"] = "3";
			request.Container["oper"] = "del";

			success = controller.ValuesEdit();
			result = controller.Values();

			Assert.AreEqual("{ success = True, id = 3 }", success.Data.ToString());
			AssertEx.JsonResultMatchesList(expectedDelete, result, "oceanMessagingValues");

			controller.SaveAll();
			var subRules = Rule.GetForReading(context.eHubClients.First(c => c.CC_ID == "SHIPPING_INSTRUCTION")).FindGroupRule("SplitingWhiteList").SubRules;



			var resultValue1 = subRules.ElementAt(0).ResultValue;
			var resultValue2 = subRules.ElementAt(1).ResultValue;


			Assert.AreEqual("OCM_CONTAINER_SPLIT", resultValue1);
			Assert.AreEqual("OCM_CONTAINER_SPLIT", resultValue2);
		}

		[TestMethod]
		public void TestExportSplitWhiteListCSV()
		{
			var expectedData = "Client ID,Purpose,Port/Country,Document Name,UShipment NameSpace,Split By" +
			                   Environment.NewLine +
			                   "TESTSENDER__WhiteList,WTH,[ANY],[ANY],[ANY],SubShipment" + Environment.NewLine +
			                   "TESTSENDER__WhiteList,[ANY],[ANY],[ANY],[ANY],Container" + Environment.NewLine;
			var expectedName = "SplitingWhiteList.csv";
			request.Clear();
			request.Container["id"] = "SplitingWhiteList";
			controller.SelectType();

			FileContentResult result = controller.ExportCSV();
			Assert.AreEqual(expectedData, Encoding.Default.GetString(result.FileContents));
			Assert.AreEqual(expectedName, result.FileDownloadName);
		}

		[TestMethod]
		public void TestImportSplitWhiteListCSV()
		{
			var importCSV = "Client ID,Purpose,Port/Country,Document Name,UShipment NameSpace,Split By" + Environment.NewLine +
			                "TESTSENDER__1,WTH,[ANY],[ANY],[ANY],SubShipment" + Environment.NewLine +
							"[ANY],WTH,AUSYD,[ANY],http://www.cargowise.com/Schemas/Universal/2012/11/ContainerGrossWeightVerification/2,Container,Container" +
			                Environment.NewLine +
							",,,,,Container" + Environment.NewLine;
			var expectedValues = new List<object>
			{
				new { id = "1", client = "TESTSENDER__1", purpose = "WTH", port = "[ANY]", docName = "[ANY]", shipNamespace = "[ANY]",  splitBy = "SubShipment"},
				new { id = "2", client = "[ANY]", purpose = "WTH", port = "AUSYD", docName = "[ANY]",  shipNamespace = "http://www.cargowise.com/Schemas/Universal/2012/11/ContainerGrossWeightVerification/2", splitBy = "Container"}
			};
			request.Clear();
			request.Container["id"] = "SplitingWhiteList";
			controller.SelectType();
			request.AddFile("uploadFile", importCSV);

			var result = controller.ImportCSV();
			Assert.AreEqual("{ success = True, message = CSV file successfully imported. }", result.Data.ToString());

			request.Clear();
			request.Container["_search"] = "false";
			request.Container["rows"] = "2";
			var oceanValues = controller.Values();
			AssertEx.JsonResultMatchesList(expectedValues, oceanValues, "oceanMessagingValues");

			result = controller.ImportCSV();
			Assert.AreEqual("{ success = False, message = Stream was not readable. }", result.Data.ToString());
		}

		[TestMethod]
		public void TestImportSplitWhiteListCSV_ShouldNotSucceed_IfMissingColumns()
		{
			var importCSV = "Client ID,Port/Country,Document Name,UShipment NameSpace,Split By" + Environment.NewLine +
							"TESTSENDER__1,[ANY],[ANY],[ANY],SubShipment" + Environment.NewLine +
							"[ANY],AUSYD,[ANY],http://www.cargowise.com/Schemas/Universal/2012/11/ContainerGrossWeightVerification/2,Container,Container" +
							Environment.NewLine +
							",,,,Container" + Environment.NewLine;
			request.Clear();
			request.Container["id"] = "SplitingWhiteList";
			controller.SelectType();
			request.AddFile("uploadFile", importCSV);

			var result = controller.ImportCSV();
			Assert.AreEqual("{ success = False, message = The column headers in the csv file does not match the required names. Ensure the following column names are in the csv file: \"Client ID\", \"Purpose\", \"Port/Country\", \"Document Name\", \"UShipment NameSpace\", \"Split By\" }", result.Data.ToString());
		}

		[TestMethod]
		public void TestImportSplitWhiteListCSV_ShouldNotSucceed_IfExtraColumns()
		{
			var importCSV = "Client ID,Purpose,Port/Country,Port/Country,Document Name,UShipment NameSpace,Split By" + Environment.NewLine +
							"TESTSENDER__1,WTH,[ANY],[ANY],[ANY],[ANY],SubShipment" + Environment.NewLine +
							"[ANY],WTH,[ANY],AUSYD,[ANY],http://www.cargowise.com/Schemas/Universal/2012/11/ContainerGrossWeightVerification/2,Container,Container" +
							Environment.NewLine +
							",,,,,,Container" + Environment.NewLine;
			request.Clear();
			request.Container["id"] = "SplitingWhiteList";
			controller.SelectType();
			request.AddFile("uploadFile", importCSV);

			var result = controller.ImportCSV();
			Assert.AreEqual("{ success = False, message = The column headers in the csv file does not match the required names. Ensure the following column names are in the csv file: \"Client ID\", \"Purpose\", \"Port/Country\", \"Document Name\", \"UShipment NameSpace\", \"Split By\" }", result.Data.ToString());
		}

		[TestMethod]
		public void TestImportSplitWhiteListCSV_ShouldNotSucceed_IfColumnsNotInOrder()
		{
			var importCSV = "UShipment NameSpace,Client ID,Purpose,Port/Country,Document Name,Split By" + Environment.NewLine +
							"[ANY],TESTSENDER__1,WTH,[ANY],[ANY],SubShipment" + Environment.NewLine +
							"http://www.cargowise.com/Schemas/Universal/2012/11/ContainerGrossWeightVerification/2,Container,Container,[ANY],WTH,AUSYD,[ANY],Container" +
							Environment.NewLine +
							",,,,,Container" + Environment.NewLine;
			request.Clear();
			request.Container["id"] = "SplitingWhiteList";
			controller.SelectType();
			request.AddFile("uploadFile", importCSV);

			var result = controller.ImportCSV();
			Assert.AreEqual("{ success = False, message = The column headers in the csv file does not match the required names. Ensure the following column names are in the csv file: \"Client ID\", \"Purpose\", \"Port/Country\", \"Document Name\", \"UShipment NameSpace\", \"Split By\" }", result.Data.ToString());
		}

		[TestMethod]
		public void TestImportSplitWhiteListCSV_ShouldNotSucceed_IfInvalidDocumentName()
		{
			var importCSV = "Client ID,Purpose,Port/Country,Document Name,UShipment NameSpace,Split By" + Environment.NewLine +
							"TESTSENDER__1,WTH,[ANY],TestDocument,[ANY],SubShipment" + Environment.NewLine +
							"[ANY],WTH,AUSYD,[ANY],http://www.cargowise.com/Schemas/Universal/2012/11/ContainerGrossWeightVerification/2,Container,Container" +
							Environment.NewLine +
							",,,,Container" + Environment.NewLine;
			request.Clear();
			request.Container["id"] = "SplitingWhiteList";
			controller.SelectType();
			request.AddFile("uploadFile", importCSV);

			var result = controller.ImportCSV();
			Assert.AreEqual("{ success = False, message = The Document Name \"TestDocument\" is invalid in row 1 of the csv file. }", result.Data.ToString());
		}

		[TestMethod]
		public void TestImportSplitWhiteListCSV_ShouldNotSucceed_IfInvalidSplitBy()
		{
			var importCSV = "Client ID,Purpose,Port/Country,Document Name,UShipment NameSpace,Split By" + Environment.NewLine +
							"TESTSENDER__1,WTH,[ANY],[ANY],[ANY],SubShipment" + Environment.NewLine +
							"[ANY],WTH,AUSYD,[ANY],http://www.cargowise.com/Schemas/Universal/2012/11/ContainerGrossWeightVerification/2,Container,Container" +
							Environment.NewLine +
							",,,,,TestSplitBy" + Environment.NewLine;

			request.Clear();
			request.Container["id"] = "SplitingWhiteList";
			controller.SelectType();
			request.AddFile("uploadFile", importCSV);

			var result = controller.ImportCSV();
			Assert.AreEqual("{ success = False, message = The Split By \"TestSplitBy\" is invalid in row 3 of the csv file. }", result.Data.ToString());
		}
	}
}
