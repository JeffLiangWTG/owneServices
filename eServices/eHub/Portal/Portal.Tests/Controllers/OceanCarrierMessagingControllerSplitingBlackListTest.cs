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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Portal.Tests.Controllers
{
	[TestClass]
	public class OceanCarrierMessagingControllerSplitingBlackListTest
	{
		static TestRequest request;
		static OceanCarrierMessagingController controller;
		static eHubTransactionsContext context;

		static TestDbSet<eHubRoutingRule> testRules = new TestDbSet<eHubRoutingRule>();
		static eHubClient clientService = new eHubClient() { CC_ID = "SHIPPING_INSTRUCTION", CC_FriendlyName = "Shipping Instruction" };
		static eHubClient clientTestSender = new eHubClient() { CC_ID = "TESTSENDER__BlackList" };


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
						RR_Group_Name = "SplitingBlackList",
						eHubRoutingRules_Group = testRules.AddRange(new[]
						{
							new eHubRoutingRule { RR_Condition_Expression = "[@SourceParty,Equal,TESTSENDER__BlackList] && [@Purpose,Equal,WTH] && [@Port,Equal,CNNBG] && [@UShipmentNamespace,Equal,http://www.cargowise.com/Schemas/Universal/2012/11/ContainerGrossWeightVerification/2]", eHubServiceProvider = null, RR_Group_Ordering = 2 },
							new eHubRoutingRule { RR_Condition_Expression = "[@SourceParty,Equal,TESTSENDER__BlackList] && [@Port,Equal,CNNGB]", eHubServiceProvider = null, RR_Group_Ordering = 2000 }
						}).ToList()
					},
					new eHubRoutingRule {
						RR_Group_MatchMultiple = false,
						RR_Group_Ordering = 200,
						RR_Group_Name = "SplitingWhiteList",
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
		public void TestSelectBlackListType()
		{
			request.Clear();
			request.Container["id"] = "SplitingBlackList";

			var result = controller.SelectType();
			Assert.AreEqual("SplitingBlackList", controller.Session["OCMTypeID"]);
			AssertEx.JsonResultMatchesList(new[] { Row.RowNames.client, Row.RowNames.purpose, Row.RowNames.port, Row.RowNames.docName, Row.RowNames.shipNamespace}, result, "rows");
		}

		[TestMethod]
		public void TestBlackListValuesDatabase()
		{
			var expectedSplitingBlackList = new List<object>
			{
				new { id = "1", client = "TESTSENDER__BlackList", purpose = "WTH", port = "CNNBG", docName = "[ANY]", shipNamespace = "http://www.cargowise.com/Schemas/Universal/2012/11/ContainerGrossWeightVerification/2" },
				new { id = "2", client = "TESTSENDER__BlackList", purpose = "[ANY]", port = "CNNGB",  docName = "[ANY]", shipNamespace = "[ANY]"}
			};

			request.Clear();
			request.Container["id"] = "SplitingBlackList";
			request.Container["rows"] = "2";
			request.Container["_search"] = "false";
			controller.SelectType();
			Assert.AreEqual("SplitingBlackList", controller.Session["OCMTypeID"]);
			var result = controller.Values();
			AssertEx.JsonResultMatchesList(expectedSplitingBlackList, result, "oceanMessagingValues");
		}

		[TestMethod]
		public void TestSplitBlackListTableValues()
		{
			var expected1 = new List<object>
			{
				new { id = "1", client = "TESTSENDER__BlackList", purpose = "WTH", port = "CNNBG", docName = "[ANY]", shipNamespace = "http://www.cargowise.com/Schemas/Universal/2012/11/ContainerGrossWeightVerification/2"}
			};

			var expectedEmpty = new List<object>();

			var expected2 = new List<object>
			{
				new { id = "2", client = "TESTSENDER__BlackList", purpose = "[ANY]", port = "CNNGB",  docName = "[ANY]", shipNamespace = "[ANY]"}
			};

			request.Clear();
			request.Container["id"] = "SplitingBlackList";
			request.Container["rows"] = "1";
			controller.SelectType();
			Assert.AreEqual("SplitingBlackList", controller.Session["OCMTypeID"]);

			// Test purpose
			request.Container["_search"] = "true";
			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"purpose\",\"op\":\"eq\",\"data\":\"WTH\"}]}";

			var result = controller.Values();
			AssertEx.JsonResultMatchesList(expected1, result, "oceanMessagingValues");

			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"purpose\",\"op\":\"BW\",\"data\":\"W\"}]}";

			result = controller.Values();
			AssertEx.JsonResultMatchesList(expected1, result, "oceanMessagingValues");

			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"purpose\",\"op\":\"ew\",\"data\":\"H\"}]}";

			result = controller.Values();
			AssertEx.JsonResultMatchesList(expected1, result, "oceanMessagingValues");

			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"purpose\",\"op\":\"cn\",\"data\":\"WI\"}]}";

			result = controller.Values();
			AssertEx.JsonResultMatchesList(expectedEmpty, result, "oceanMessagingValues", true);

			// Test shipNamespace
			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"shipNamespace\",\"op\":\"eq\",\"data\":\"www\"}]}";

			result = controller.Values();
			AssertEx.JsonResultMatchesList(expectedEmpty, result, "oceanMessagingValues", true);

			request.Container["searchString"] = "www";
			request.Container["searchOper"] = "cn";
			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"shipNamespace\",\"op\":\"cn\",\"data\":\"www\"}]}";

			result = controller.Values();
			AssertEx.JsonResultMatchesList(expected1, result, "oceanMessagingValues");

			request.Container["searchString"] = "[";
			request.Container["searchOper"] = "bw";
			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"shipNamespace\",\"op\":\"bw\",\"data\":\"[\"}]}";

			result = controller.Values();
			AssertEx.JsonResultMatchesList(expected2, result, "oceanMessagingValues");

			request.Container["searchString"] = "S";
			request.Container["searchOper"] = "ew";
			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"shipNamespace\",\"op\":\"ew\",\"data\":\"S\"}]}";

			result = controller.Values();
			AssertEx.JsonResultMatchesList(expectedEmpty, result, "oceanMessagingValues", true);
		}

		[TestMethod]
		public void TestEditAndSaveSplitListValues()
		{
			var expectedAdd = new List<object>
			{
				new { id = "1", client = "TESTSENDER__BlackList", purpose = "WTH", port = "CNNBG", docName = "[ANY]", shipNamespace = "http://www.cargowise.com/Schemas/Universal/2012/11/ContainerGrossWeightVerification/2" },
				new { id = "2", client = "TESTSENDER__BlackList", purpose = "[ANY]", port = "CNNGB", docName = "[ANY]", shipNamespace = "[ANY]"},
				new { id = "3", client = "[ANY]", purpose = "[ANY]", port = "[ANY]", docName = "[ANY]",  shipNamespace = "[ANY]"}
			};

			var expectedDelete = new List<object>
			{
				new { id = "1", client = "TESTSENDER__BlackList", purpose = "WTH", port = "CNNBG", docName = "[ANY]", shipNamespace = "http://www.cargowise.com/Schemas/Universal/2012/11/ContainerGrossWeightVerification/2" },
				new { id = "2", client = "TESTSENDER__BlackList", purpose = "[ANY]", port = "CNNGB", docName = "[ANY]", shipNamespace = "[ANY]"}
			};

			var expectedEdit = new List<object>
			{
				new { id = "1", client = "TESTSENDER__BlackList", purpose = "WTH", port = "CNNBG", docName = "[ANY]", shipNamespace = "http://www.cargowise.com/Schemas/Universal/2012/11/ContainerGrossWeightVerification/2" },
				new { id = "2", client = "TESTSENDER__BlackList", purpose = "WTH", port = "TestPort", docName = "[ANY]", shipNamespace = "http://www.cargowise.com/Schemas/Universal/2012/11/ContainerGrossWeightVerification/2"},
			};


			request.Container["id"] = "SplitingBlackList";
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

			var success = controller.ValuesEdit();
			var result = controller.Values();
			Assert.AreEqual("{ success = True, id = 3 }", success.Data.ToString());
			AssertEx.JsonResultMatchesList(expectedAdd, result, "oceanMessagingValues");

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

			//Edit
			request.Clear();
			request.Container["rows"] = "10";
			request.Container["_search"] = "false";
			request.Container["rowLimit"] = "4";
			request.Container["id"] = "2";
			request.Container["oper"] = "edit";
			request.Container["client"] = "TESTSENDER__BlackList";
			request.Container["port"] = "TestPort";
			request.Container["docName"] = "[ANY]";
			request.Container["shipNamespace"] = "http://www.cargowise.com/Schemas/Universal/2012/11/ContainerGrossWeightVerification/2";
			request.Container["purpose"] = "WTH";

			success = controller.ValuesEdit();
			result = controller.Values();

			Assert.AreEqual("{ success = True, id = 2 }", success.Data.ToString());
			AssertEx.JsonResultMatchesList(expectedEdit, result, "oceanMessagingValues");

			controller.SaveAll();

			Assert.AreEqual("{ success = True, id = 2 }", success.Data.ToString());
			AssertEx.JsonResultMatchesList(expectedEdit, result, "oceanMessagingValues");
		}

		[TestMethod]
		public void TestExportSplitBlackListCSV()
		{
			var expectedData = "Client ID,Purpose,Port/Country,Document Name,UShipment NameSpace" +
			                   Environment.NewLine +
							   "TESTSENDER__BlackList,WTH,CNNBG,[ANY],http://www.cargowise.com/Schemas/Universal/2012/11/ContainerGrossWeightVerification/2" + Environment.NewLine +
							   "TESTSENDER__BlackList,[ANY],CNNGB,[ANY],[ANY]" + Environment.NewLine;
			var expectedName = "SplitingBlackList.csv";
			request.Clear();
			request.Container["id"] = "SplitingBlackList";
			controller.SelectType();

			FileContentResult result = controller.ExportCSV();
			Assert.AreEqual(expectedData, Encoding.Default.GetString(result.FileContents));
			Assert.AreEqual(expectedName, result.FileDownloadName);
		}

		[TestMethod]
		public void TestImportSplitBlackListCSV()
		{
			var importCSV = "Client ID,Purpose,Port/Country,Document Name,UShipment NameSpace" + Environment.NewLine +
							 "TESTSENDER__BlackList,WTH,CNNBG,[ANY],[ANY]" + Environment.NewLine +
							 "[ANY],WTH,AUSYD,[ANY],http://www.cargowise.com/Schemas/Universal/2012/11/ContainerGrossWeightVerification/2" +
			                 Environment.NewLine +
			                 ",,,," + Environment.NewLine;
			var expectedValues = new List<object>
			{
				new { id = "1", client = "TESTSENDER__BlackList", purpose = "WTH", port = "CNNBG", docName = "[ANY]", shipNamespace = "[ANY]"},
				new { id = "2", client = "[ANY]", purpose = "WTH", port = "AUSYD", docName = "[ANY]",  shipNamespace = "http://www.cargowise.com/Schemas/Universal/2012/11/ContainerGrossWeightVerification/2"}
			};
			request.Clear();
			request.Container["id"] = "SplitingBlackList";
			controller.SelectType();
			request.AddFile("uploadFile", importCSV);

			var result = controller.ImportCSV();
			Assert.AreEqual("{ success = True, message = CSV file successfully imported. }", result.Data.ToString());

			request.Clear();
			request.Container["rows"] = "2";
			request.Container["_search"] = "false";
			var oceanValues = controller.Values();
			AssertEx.JsonResultMatchesList(expectedValues, oceanValues, "oceanMessagingValues");

			result = controller.ImportCSV();
			Assert.AreEqual("{ success = False, message = Stream was not readable. }", result.Data.ToString());
		}

		[TestMethod]
		public void TestImportSplitBlackListCSV_ShouldNotSucceed_IfMissingColumns()
		{
			var importCSV = "Client ID,Purpose,Document Name,UShipment NameSpace" + Environment.NewLine +
							 "TESTSENDER__BlackList,WTH,[ANY],[ANY]" + Environment.NewLine +
							 "[ANY],WTH,[ANY],http://www.cargowise.com/Schemas/Universal/2012/11/ContainerGrossWeightVerification/2" +
							 Environment.NewLine +
							 ",,," + Environment.NewLine;
			request.Clear();
			request.Container["id"] = "SplitingBlackList";
			controller.SelectType();
			request.AddFile("uploadFile", importCSV);

			var result = controller.ImportCSV();
			Assert.AreEqual("{ success = False, message = The column headers in the csv file does not match the required names. Ensure the following column names are in the csv file: \"Client ID\", \"Purpose\", \"Port/Country\", \"Document Name\", \"UShipment NameSpace\" }", result.Data.ToString());
		}

		[TestMethod]
		public void TestImportSplitBlackListCSV_ShouldNotSucceed_IfExtraColumns()
		{
			var importCSV = "Client ID,Purpose,Port/Country,Document Name,Document Name,UShipment NameSpace" + Environment.NewLine +
							 "TESTSENDER__BlackList,WTH,CNNBG,[ANY],[ANY],[ANY]" + Environment.NewLine +
							 "[ANY],WTH,AUSYD,[ANY],[ANY],http://www.cargowise.com/Schemas/Universal/2012/11/ContainerGrossWeightVerification/2" +
							 Environment.NewLine +
							 ",,,,," + Environment.NewLine;
			request.Clear();
			request.Container["id"] = "SplitingBlackList";
			controller.SelectType();
			request.AddFile("uploadFile", importCSV);

			var result = controller.ImportCSV();
			Assert.AreEqual("{ success = False, message = The column headers in the csv file does not match the required names. Ensure the following column names are in the csv file: \"Client ID\", \"Purpose\", \"Port/Country\", \"Document Name\", \"UShipment NameSpace\" }", result.Data.ToString());
		}

		[TestMethod]
		public void TestImportSplitBlackListCSV_ShouldNotSucceed_IfColumnsNotInOrder()
		{
			var importCSV = "Document Name,Client ID,Purpose,Port/Country,UShipment NameSpace" + Environment.NewLine +
							 "[ANY],TESTSENDER__BlackList,WTH,CNNBG,[ANY]" + Environment.NewLine +
							 "[ANY],[ANY],WTH,AUSYD,http://www.cargowise.com/Schemas/Universal/2012/11/ContainerGrossWeightVerification/2" +
							 Environment.NewLine +
							 ",,,," + Environment.NewLine;
			request.Clear();
			request.Container["id"] = "SplitingBlackList";
			controller.SelectType();
			request.AddFile("uploadFile", importCSV);

			var result = controller.ImportCSV();
			Assert.AreEqual("{ success = False, message = The column headers in the csv file does not match the required names. Ensure the following column names are in the csv file: \"Client ID\", \"Purpose\", \"Port/Country\", \"Document Name\", \"UShipment NameSpace\" }", result.Data.ToString());
		}

		[TestMethod]
		public void TestImportSplitBlackListCSV_ShouldNotSucceed_IfInvalidDocumentName()
		{
			var importCSV = "Client ID,Purpose,Port/Country,Document Name,UShipment NameSpace" + Environment.NewLine +
							 "TESTSENDER__BlackList,WTH,CNNBG,TestDocument,[ANY]" + Environment.NewLine +
							 "[ANY],WTH,AUSYD,[ANY],http://www.cargowise.com/Schemas/Universal/2012/11/ContainerGrossWeightVerification/2" +
							 Environment.NewLine +
							 ",,,," + Environment.NewLine;
			request.Clear();
			request.Container["id"] = "SplitingBlackList";
			controller.SelectType();
			request.AddFile("uploadFile", importCSV);

			var result = controller.ImportCSV();
			Assert.AreEqual("{ success = False, message = The Document Name \"TestDocument\" is invalid in row 1 of the csv file. }", result.Data.ToString());
		}
	}
}
