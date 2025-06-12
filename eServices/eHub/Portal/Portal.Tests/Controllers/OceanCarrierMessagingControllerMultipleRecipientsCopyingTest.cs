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
	public class OceanCarrierMessagingControllerMultipleRecipientsCopyingTest
	{
		static TestRequest request;
		static OceanCarrierMessagingController controller;
		static eHubTransactionsContext context;

		static TestDbSet<eHubRoutingRule> testRules = new TestDbSet<eHubRoutingRule>();
		static eHubClient clientService = new eHubClient() { CC_ID = "OCM_MutipleRecipientsCopying", CC_FriendlyName = "OCM Mutiple Recipients Copying" };
		static eHubClient clientTestSender = new eHubClient() { CC_ID = "TESTSENDER__MultipleRecipients" };


		[ClassInitialize()]
		public static void ClassInit(TestContext testContext)
		{
            var fakeIdentity = new GenericIdentity("CORP\\Test.Name");
            var principal = new GenericPrincipal(fakeIdentity, null);

            request = new TestRequest();
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
						RR_Group_Name = "MultipleRecipientsCopying",
						eHubRoutingRules_Group = testRules.AddRange(new[]
						{
							new eHubRoutingRule { RR_Condition_Expression = "[@SourceParty,Equal,TESTSENDER__MultipleRecipients] && [@SCACUniShip,Equal,ONEY] && [@Port,Equal,CNNBG]", RR_Result_Value = "CarrierHandlingAgent", eHubServiceProvider = null, RR_Group_Ordering = 2 },
							new eHubRoutingRule { RR_Condition_Expression = "[@SourceParty,IsMatch,^TES.*$] && [@SCACUniShip,Equal,YMLU] && [@Port,IsMatch,^(NOTEXIST|CNNBG|HKHKJ)$]", RR_Result_Value = "CarrierBookingAgent", eHubServiceProvider = null, RR_Group_Ordering = 2000 }
						}).ToList()
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
		public void TestSelectMultipleRecipientsType()
		{
			request.Clear();
			request.Container["id"] = "MultipleRecipientsCopying";

			var result = controller.SelectType();
			Assert.AreEqual("MultipleRecipientsCopying", controller.Session["OCMTypeID"]);
			AssertEx.JsonResultMatchesList(new[] { Row.RowNames.client, Row.RowNames.carrier, Row.RowNames.port, Row.RowNames.docName, Row.RowNames.shipNamespace, Row.RowNames.partyToCopy }, result, "rows");
		}

		[TestMethod]
		public void TestMultipleRecipientsCopyingValuesDatabase()
		{
			var expectedCopyingRuleList = new List<object>
			{
				new { id = "1", client = "TESTSENDER__MultipleRecipients", carrier = "ONEY", port = "CNNBG", docName = "[ANY]", partyToCopy = "CarrierHandlingAgent" },
				new { id = "2", client = "TES*", carrier = "YMLU", port = "NOTEXIST,CNNBG,HKHKJ",  docName = "[ANY]", partyToCopy = "CarrierBookingAgent"}
			};

			request.Clear();
			request.Container["id"] = "MultipleRecipientsCopying";
			request.Container["rows"] = "2";
			request.Container["_search"] = "false";
			controller.SelectType();
			Assert.AreEqual("MultipleRecipientsCopying", controller.Session["OCMTypeID"]);
			var result = controller.Values();
			AssertEx.JsonResultMatchesList(expectedCopyingRuleList, result, "oceanMessagingValues");
		}

		[TestMethod]
		public void TestMultipleRecipientsCopyingListTableValues()
		{
			var expected1 = new List<object>
			{
				new { id = "1", client = "TESTSENDER__MultipleRecipients", carrier = "ONEY", port = "CNNBG", docName = "[ANY]", partyToCopy = "CarrierHandlingAgent" },
			};

			var expectedEmpty = new List<object>();

			var expected2 = new List<object>
			{
				new { id = "2", client = "TES*", carrier = "YMLU", port = "NOTEXIST,CNNBG,HKHKJ",  docName = "[ANY]", partyToCopy = "CarrierBookingAgent"}
			};

			request.Clear();
			request.Container["id"] = "MultipleRecipientsCopying";
			request.Container["rows"] = "1";
			controller.SelectType();
			Assert.AreEqual("MultipleRecipientsCopying", controller.Session["OCMTypeID"]);

			// Test carrier
			request.Container["_search"] = "true";
			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"carrier\",\"op\":\"eq\",\"data\":\"ONEY\"}]}";

			var result = controller.Values();
			AssertEx.JsonResultMatchesList(expected1, result, "oceanMessagingValues");

			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"carrier\",\"op\":\"bw\",\"data\":\"O\"}]}";

			result = controller.Values();
			AssertEx.JsonResultMatchesList(expected1, result, "oceanMessagingValues");

			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"carrier\",\"op\":\"ew\",\"data\":\"Y\"}]}";

			result = controller.Values();
			AssertEx.JsonResultMatchesList(expected1, result, "oceanMessagingValues");

			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"carrier\",\"op\":\"cn\",\"data\":\"NY\"}]}";

			result = controller.Values();
			AssertEx.JsonResultMatchesList(expectedEmpty, result, "oceanMessagingValues", true);

			// Test port
			request.Container["_search"] = "true";
			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"port\",\"op\":\"eq\",\"data\":\"EXIST\"}]}";

			result = controller.Values();
			AssertEx.JsonResultMatchesList(expectedEmpty, result, "oceanMessagingValues", true);

			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"port\",\"op\":\"cn\",\"data\":\"NNB\"}]}";

			result = controller.Values();
			AssertEx.JsonResultMatchesList(expected1, result, "oceanMessagingValues");

			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"port\",\"op\":\"bw\",\"data\":\"NOT\"}]}";

			result = controller.Values();
			AssertEx.JsonResultMatchesList(expected2, result, "oceanMessagingValues");

			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"port\",\"op\":\"ew\",\"data\":\"EXISTED\"}]}";

			result = controller.Values();
			AssertEx.JsonResultMatchesList(expectedEmpty, result, "oceanMessagingValues", true);

			// Test client
			request.Container["_search"] = "true";
			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"client\",\"op\":\"eq\",\"data\":\"TESTSENDER__MultipleRecipients\"}]}";

			result = controller.Values();
			AssertEx.JsonResultMatchesList(expected1, result, "oceanMessagingValues");

			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"client\",\"op\":\"bw\",\"data\":\"TES\"}]}";

			result = controller.Values();
			AssertEx.JsonResultMatchesList(expected1, result, "oceanMessagingValues");

			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"client\",\"op\":\"ew\",\"data\":\"TS\"}]}";

			result = controller.Values();
			AssertEx.JsonResultMatchesList(expected1, result, "oceanMessagingValues");

			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"client\",\"op\":\"cn\",\"data\":\"ABC\"}]}";

			result = controller.Values();
			AssertEx.JsonResultMatchesList(expectedEmpty, result, "oceanMessagingValues", true);

			// Test doc name
			request.Container["_search"] = "true";
			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"docName\",\"op\":\"eq\",\"data\":\"[ANY]\"}]}";

			result = controller.Values();
			AssertEx.JsonResultMatchesList(expected1, result, "oceanMessagingValues");

			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"docName\",\"op\":\"bw\",\"data\":\"eMa\"}]}";

			result = controller.Values();
			AssertEx.JsonResultMatchesList(expectedEmpty, result, "oceanMessagingValues", true);

			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"docName\",\"op\":\"ew\",\"data\":\"]\"}]}";

			result = controller.Values();
			AssertEx.JsonResultMatchesList(expected1, result, "oceanMessagingValues");

			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"docName\",\"op\":\"cn\",\"data\":\"ANY\"}]}";

			result = controller.Values();
			AssertEx.JsonResultMatchesList(expected1, result, "oceanMessagingValues");

			// Test party to copy
			request.Container["_search"] = "true";
			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"partyToCopy\",\"op\":\"eq\",\"data\":\"CarrierHandlingAgent\"}]}";

			result = controller.Values();
			AssertEx.JsonResultMatchesList(expected1, result, "oceanMessagingValues");

			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"partyToCopy\",\"op\":\"bw\",\"data\":\"Handling\"}]}";

			result = controller.Values();
			AssertEx.JsonResultMatchesList(expectedEmpty, result, "oceanMessagingValues", true);

			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"partyToCopy\",\"op\":\"ew\",\"data\":\"HandlingAgent\"}]}";

			result = controller.Values();
			AssertEx.JsonResultMatchesList(expected1, result, "oceanMessagingValues");

			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"partyToCopy\",\"op\":\"cn\",\"data\":\"Booking\"}]}";

			result = controller.Values();
			AssertEx.JsonResultMatchesList(expected2, result, "oceanMessagingValues");
		}

		[TestMethod]
		public void TestBuildExpression()
		{
			request.Clear();
			request.Container["id"] = "MultipleRecipientsCopying";
			request.Container["rows"] = "2";
			request.Container["_search"] = "false";
			controller.SelectType();
			var OCMMessageType = controller.GetOceanCarrierMessagingType() as MultipleRecipientType;
			Assert.AreEqual("[@SourceParty,IsMatch,^HYE.*$]", OCMMessageType.BuildExpression("@SourceParty", "HYE*", Row.RowNames.client));
			Assert.AreEqual("[@SourceParty,Equal,HYEHYEHYE]", OCMMessageType.BuildExpression("@SourceParty", "HYEHYEHYE", Row.RowNames.client));
			Assert.AreEqual("[@Port,Equal,CNNBG]", OCMMessageType.BuildExpression("@Port", "CNNBG", Row.RowNames.port));
			Assert.AreEqual("[@Port,Equal,NOTEXIST|CNNBG]", OCMMessageType.BuildExpression("@Port", "NOTEXIST|CNNBG", Row.RowNames.port));
			Assert.AreEqual("[@Port,IsMatch,^(NOTEXIST|CNNBG|HKHKJ)$]", OCMMessageType.BuildExpression("@Port", "NOTEXIST,CNNBG,HKHKJ", Row.RowNames.port));
		}

		[TestMethod]
		public void TestEditAndSaveCopyingListValues()
		{
			var expectedAdd = new List<object>
			{
				new { id = "1", client = "TESTSENDER__MultipleRecipients", carrier = "ONEY", port = "CNNBG", docName = "[ANY]", partyToCopy = "CarrierHandlingAgent" },
				new { id = "2", client = "TES*", carrier = "YMLU", port = "NOTEXIST,CNNBG,HKHKJ",  docName = "[ANY]", partyToCopy = "CarrierBookingAgent"},
				new { id = "3", client = "[ANY]", carrier = "[ANY]", port = "[ANY]", docName = "[ANY]",  partyToCopy = "CarrierHandlingAgent"}
			};

			var expectedAddInline = new List<object>
			{
				new { id = "1", client = "TESTSENDER__MultipleRecipients", carrier = "ONEY", port = "CNNBG", docName = "[ANY]", partyToCopy = "CarrierHandlingAgent" },
				new { id = "3", client = "[ANY]", carrier = "[ANY]", port = "[ANY]", docName = "[ANY]",  partyToCopy = "CarrierHandlingAgent"},
				new { id = "2", client = "TES*", carrier = "YMLU", port = "NOTEXIST,CNNBG,HKHKJ",  docName = "[ANY]", partyToCopy = "CarrierBookingAgent"}
			};

			var expectedDelete = new List<object>
			{
				new { id = "1", client = "TESTSENDER__MultipleRecipients", carrier = "ONEY", port = "CNNBG", docName = "[ANY]", partyToCopy = "CarrierHandlingAgent" },
				new { id = "2", client = "TES*", carrier = "YMLU", port = "NOTEXIST,CNNBG,HKHKJ",  docName = "[ANY]", partyToCopy = "CarrierBookingAgent"}
			};

			var expectedEdit = new List<object>
			{
				new { id = "1", client = "TESTSENDER__MultipleRecipients", carrier = "HLCU", port = "CNNBO", docName = "[ANY]", partyToCopy = "CarrierBookingAgent" },
				new { id = "3", client = "[ANY]", carrier = "[ANY]", port = "[ANY]", docName = "[ANY]",  partyToCopy = "CarrierHandlingAgent"},
				new { id = "2", client = "TES*", carrier = "YMLU", port = "NOTEXIST,CNNBG,HKHKJ",  docName = "[ANY]", partyToCopy = "CarrierBookingAgent"}
			};


			request.Container["id"] = "MultipleRecipientsCopying";
			controller.SelectType();

			//Add
			request.Clear();
			request.Container["rows"] = "10";
			request.Container["_search"] = "false";
			request.Container["rowLimit"] = "4";
			request.Container["id"] = "3";
			request.Container["oper"] = "add";
			request.Container["client"] = "[ANY]";
			request.Container["partyToCopy"] = "CarrierHandlingAgent";

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

			//Add Inline
			request.Clear();
			request.Container["rows"] = "10";
			request.Container["_search"] = "false";
			request.Container["rowLimit"] = "4";
			request.Container["id"] = "3";
			request.Container["afterRowId"] = "2";
			request.Container["oper"] = "add";
			request.Container["client"] = "[ANY]";
			request.Container["port"] = "[ANY]";
			request.Container["docName"] = "[ANY]";
			request.Container["carrier"] = "[ANY]";
			request.Container["partyToCopy"] = "CarrierHandlingAgent";

			success = controller.ValuesEdit();
			result = controller.Values();
			Assert.AreEqual("{ success = True, id = 3 }", success.Data.ToString());
			AssertEx.JsonResultMatchesList(expectedAddInline, result, "oceanMessagingValues");

			//Edit
			request.Clear();
			request.Container["rows"] = "10";
			request.Container["_search"] = "false";
			request.Container["rowLimit"] = "4";
			request.Container["id"] = "1";
			request.Container["oper"] = "edit";
			request.Container["client"] = "TESTSENDER__MultipleRecipients";
			request.Container["port"] = "CNNBO";
			request.Container["docName"] = "[ANY]";
			request.Container["partyToCopy"] = "CarrierBookingAgent";
			request.Container["carrier"] = "HLCU";

			success = controller.ValuesEdit();
			result = controller.Values();

			Assert.AreEqual("{ success = True, id = 1 }", success.Data.ToString());
			AssertEx.JsonResultMatchesList(expectedEdit, result, "oceanMessagingValues");

			controller.SaveAll();

			Assert.AreEqual("{ success = True, id = 1 }", success.Data.ToString());
			AssertEx.JsonResultMatchesList(expectedEdit, result, "oceanMessagingValues");
		}

		[TestMethod]
		public void TestExportMultipleRecipientsCopyingCSV()
		{
			var expectedData = "Client ID,Carrier SCAC/C1C,Port/Country,Document Name,UShipment NameSpace,Party To Copy" +
							   Environment.NewLine +
							   "TESTSENDER__MultipleRecipients,ONEY,CNNBG,[ANY],[ANY],CarrierHandlingAgent" + Environment.NewLine +
							   "TES*,YMLU,\"NOTEXIST,CNNBG,HKHKJ\",[ANY],[ANY],CarrierBookingAgent" + Environment.NewLine;
			var expectedName = "MultipleRecipientsCopying.csv";
			request.Clear();
			request.Container["id"] = "MultipleRecipientsCopying";
			controller.SelectType();

			FileContentResult result = controller.ExportCSV();
			Assert.AreEqual(expectedData, Encoding.Default.GetString(result.FileContents));
			Assert.AreEqual(expectedName, result.FileDownloadName);
		}

		[TestMethod]
		public void TestImportMultipleRecipientsCopyingCSV()
		{
			var importCSV = "Client ID,Carrier SCAC/C1C,Port/Country,Document Name,UShipment NameSpace,Party To Copy" + Environment.NewLine +
							 "TESTSENDER__MultipleRecipients,HLCU,CNNBO,[ANY],[ANY],CarrierBookingAgent" + Environment.NewLine +
							 "TES*,HKHK,\"NOTEXIST,CNNBG,HKHKJ\",[ANY],[ANY],CarrierHandlingAgent" +
							 Environment.NewLine +
							 ",,,,,CarrierHandlingAgent" + Environment.NewLine;
			var expectedValues = new List<object>
			{
				new { id = "1", client = "TESTSENDER__MultipleRecipients", carrier = "HLCU", port = "CNNBO", docName = "[ANY]", partyToCopy = "CarrierBookingAgent", shipNamespace = "[ANY]"},
				new { id = "2", client = "TES*", carrier = "HKHK", port = "NOTEXIST,CNNBG,HKHKJ", docName = "[ANY]",  partyToCopy = "CarrierHandlingAgent", shipNamespace = "[ANY]"}
			};
			request.Clear();
			request.Container["id"] = "MultipleRecipientsCopying";
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
		public void TestImportMultipleRecipientsCopyingCSV_ShouldNotSucceed_IfMissingColumns()
		{
			var importCSV = "Client ID,Carrier SCAC/C1C,Document Name,UShipment NameSpace,Party To Copy" + Environment.NewLine +
							 "TESTSENDER__MultipleRecipients,HLCU,[ANY],[ANY],CarrierBookingAgent" + Environment.NewLine +
							 "TES*,HKHK,[ANY],[ANY],CarrierHandlingAgent" +
							 Environment.NewLine +
							 ",,,,CarrierHandlingAgent" + Environment.NewLine;
			request.Clear();
			request.Container["id"] = "MultipleRecipientsCopying";
			controller.SelectType();
			request.AddFile("uploadFile", importCSV);

			var result = controller.ImportCSV();
			Assert.AreEqual("{ success = False, message = The column headers in the csv file does not match the required names. Ensure the following column names are in the csv file: \"Client ID\", \"Purpose\", \"Port/Country\", \"Document Name\", \"UShipment NameSpace\", \"Party To Copy\" }", result.Data.ToString());
		}

		[TestMethod]
		public void TestImportCSV_ShouldNotSucceed_IfExtraColumns()
		{
			var importCSV = "Client ID,Carrier SCAC/C1C,Document Name,Document Name,UShipment NameSpace,Party To Copy" + Environment.NewLine +
							 "TESTSENDER__MultipleRecipients,HLCU,[ANY],[ANY],[ANY],CarrierBookingAgent" + Environment.NewLine +
							 "TES*,HKHK,[ANY],[ANY],[ANY],CarrierHandlingAgent" +
							 Environment.NewLine +
							 ",,,,,CarrierHandlingAgent" + Environment.NewLine;

			request.Clear();
			request.Container["id"] = "MultipleRecipientsCopying";
			controller.SelectType();
			request.AddFile("uploadFile", importCSV);

			var result = controller.ImportCSV();
			Assert.AreEqual("{ success = False, message = The column headers in the csv file does not match the required names. Ensure the following column names are in the csv file: \"Client ID\", \"Purpose\", \"Port/Country\", \"Document Name\", \"UShipment NameSpace\", \"Party To Copy\" }", result.Data.ToString());
		}

		[TestMethod]
		public void TestImportCSV_ShouldNotSucceed_IfColumnsNotInOrder()
		{
			var importCSV = "UShipment NameSpace,Client ID,Carrier SCAC/C1C,Document Name,UShipment NameSpace,Party To Copy" + Environment.NewLine +
							 "[ANY],TESTSENDER__MultipleRecipients,HLCU,[ANY],[ANY],CarrierBookingAgent" + Environment.NewLine +
							 "[ANY],TES*,HKHK,[ANY],[ANY],CarrierHandlingAgent" +
							 Environment.NewLine +
							 ",,,,,CarrierHandlingAgent" + Environment.NewLine;

			request.Clear();
			request.Container["id"] = "MultipleRecipientsCopying";
			controller.SelectType();
			request.AddFile("uploadFile", importCSV);

			var result = controller.ImportCSV();
			Assert.AreEqual("{ success = False, message = The column headers in the csv file does not match the required names. Ensure the following column names are in the csv file: \"Client ID\", \"Purpose\", \"Port/Country\", \"Document Name\", \"UShipment NameSpace\", \"Party To Copy\" }", result.Data.ToString());
		}

		[TestMethod]
		public void TestImportCSV_ShouldNotSucceed_IfInvalidDocumentName()
		{
			var importCSV = "Client ID,Carrier SCAC/C1C,Port/Country,Document Name,UShipment NameSpace,Party To Copy" + Environment.NewLine +
							 "TESTSENDER__MultipleRecipients,HLCU,CNNBO,TestDocument,[ANY],CarrierBookingAgent" + Environment.NewLine +
							 "TES*,HKHK,\"NOTEXIST,CNNBG,HKHKJ\",[ANY],[ANY],CarrierHandlingAgent" +
							 Environment.NewLine +
							 ",,,,,," + Environment.NewLine;

			request.Clear();
			request.Container["id"] = "MultipleRecipientsCopying";
			controller.SelectType();
			request.AddFile("uploadFile", importCSV);

			var result = controller.ImportCSV();
			Assert.AreEqual("{ success = False, message = The Document Name \"TestDocument\" is invalid in row 1 of the csv file. }", result.Data.ToString());
		}

		[TestMethod]
		public void TestImportCSV_ShouldNotSucceed_IfInvalidPartyToCopy()
		{
			var importCSV = "Client ID,Carrier SCAC/C1C,Port/Country,Document Name,UShipment NameSpace,Party To Copy" + Environment.NewLine +
							 "TESTSENDER__MultipleRecipients,HLCU,CNNBO,[ANY],[ANY],CarrierBookingAgent" + Environment.NewLine +
							 "TES*,HKHK,\"NOTEXIST,CNNBG,HKHKJ\",[ANY],[ANY],TestParty" +
							 Environment.NewLine +
							 ",,,,,CarrierBookingAgent" + Environment.NewLine;

			request.Clear();
			request.Container["id"] = "MultipleRecipientsCopying";
			controller.SelectType();
			request.AddFile("uploadFile", importCSV);

			var result = controller.ImportCSV();
			Assert.AreEqual("{ success = False, message = The Party To Copy \"TestParty\" is invalid in row 2 of the csv file. }", result.Data.ToString());
		}
	}
}
