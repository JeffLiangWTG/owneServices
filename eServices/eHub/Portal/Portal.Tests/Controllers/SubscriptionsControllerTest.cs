using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using CargoWise.eHub.Portal.Controllers;
using CargoWise.eHub.Portal.Tests.Fakes;
using CargoWise.eHub.Portal.Tests.TestHelpers;
using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Portal.Tests.Controllers
{
	[TestClass]
	public class SubscriptionsControllerTest : BaseControllerTest<SubscriptionsController>
	{
		[TestMethod]
		public void TestIndex()
		{
			var result = controller.Index() as ViewResult;
			Assert.IsNotNull(result);
			Assert.AreEqual(string.Empty, result.ViewName, "Should be empty (Index)");
		}

		[TestMethod]
		public void TestTypesGet()
		{
			var expected = new List<object> {
				{ new { ST_PK = new Guid("{00000000-AAAA-1111-0000-000000000000}"), ST_ID = "SUB1", ST_Name = "Subscription 1", ST_ExpiryDays = (int?)null } } ,
				{ new { ST_PK = new Guid("{00000000-AAAA-2222-0000-000000000000}"), ST_ID = "SUB2", ST_Name = "Subscription 2", ST_ExpiryDays = 30 } } ,
				{ new { ST_PK = new Guid("{00000000-AAAA-3333-0000-000000000000}"), ST_ID = "SUB3", ST_Name = "Subscription 3", ST_ExpiryDays = (int?)null } } ,
			};

			var result = controller.Types();

			Assert.IsNotNull(result);
			AssertEx.JsonResultMatchesList(expected, result, "eHubSubscriptionTypes");
		}

		[TestMethod]
		public void TestTypesInfoGet()
		{
			var expected = new { ST_PK = new Guid("{00000000-AAAA-1111-0000-000000000000}"), ST_ID = "SUB1", ST_Name = "Subscription 1", ST_ExpiryDays = (int?)null };

			var result = controller.TypeInfo(new Guid("{00000000-AAAA-1111-0000-000000000000}"));

			Assert.IsNotNull(result);
			var resultData = result.Data.GetType().GetProperty("eHubSubscriptionType").GetValue(result.Data, null);
			AssertEx.PropertyValuesAreEquals(expected, resultData, false);
		}

		[TestMethod]
		public void TestTypeInfoEdit()
		{
            var logger = new TestLogger();

            request.Clear();
			request.Container["ST_ID"] = "SUB4";
			request.Container["ST_Name"] = "Subscription 4";
			request.Container["ST_ExpiryDays"] = "60";
			request.Container["oper"] = "add";

			TypeInfoEditTest(logger);

			var resultAdd = context.eHubSubscriptionTypes.Select(t => new { t.ST_PK, t.ST_ID, t.ST_Name, t.ST_ExpiryDays }).First(t => t.ST_ID == "SUB4");
			var newId = resultAdd.ST_PK;
			var expectedAdd = new { ST_PK = newId, ST_ID = "SUB4", ST_Name = "Subscription 4", ST_ExpiryDays = 60 };
			AssertEx.PropertyValuesAreEquals(expectedAdd, resultAdd, false);
            Assert.IsTrue(logger.Log.Contains("Info - [add] eHubSubscriptionType:"));
            Assert.IsTrue(logger.Log.Contains("ST_ID=SUB4, ST_Name=Subscription 4, ST_ExpiryDays=60"));

            request.Clear();
			request.Container["ST_PK"] = newId.ToString();
			request.Container["ST_ID"] = "SUB4a";
			request.Container["ST_Name"] = "Subscription 4a";
			request.Container["ST_ExpiryDays"] = "";
			request.Container["oper"] = "edit";
			var expectedEdit = new { ST_PK = newId, ST_ID = "SUB4a", ST_Name = "Subscription 4a", ST_ExpiryDays = (int?)null };

			TypeInfoEditTest(logger);

			var resultEdit = context.eHubSubscriptionTypes.Select(t => new { t.ST_PK, t.ST_ID, t.ST_Name, t.ST_ExpiryDays }).First(t => t.ST_PK == newId);
			AssertEx.PropertyValuesAreEquals(expectedEdit, resultEdit, false);
            Assert.IsTrue(logger.Log.Contains("Info - [edit] eHubSubscriptionType:"));
            Assert.IsTrue(logger.Log.Contains("ST_ID=SUB4a, ST_Name=Subscription 4a, ST_ExpiryDays="));

            request.Clear();
			request.Container["ST_PK"] = newId.ToString();
			request.Container["oper"] = "del";

			TypeInfoEditTest(logger);

			Assert.IsNull(context.eHubSubscriptionTypes.FirstOrDefault(t => t.ST_PK == newId));
            Assert.IsTrue(logger.Log.Contains("Info - [del] eHubSubscriptionType:"));
            Assert.IsTrue(logger.Log.Contains("ST_ID=SUB4a, ST_Name=Subscription 4a, ST_ExpiryDays="));
        }

		[TestMethod]
		public void TestAutoSubscribesGet()
		{
            var expected1 = new List<object> { };

			var result1 = controller.AutoSubscribes(new Guid("{00000000-AAAA-1111-0000-000000000000}"));

			Assert.IsNotNull(result1);
			AssertEx.JsonResultMatchesList(expected1, result1, "eHubSubscriptionAutoSubscribes", true);

            var expected2 = new List<object> {
				{ new { SA_PK = new Guid("{00000000-BBBB-1111-0000-000000000000}"), DT_Code = "MSG0001", CC_ID_Recipient = (string)null, SA_ValueXpath = "/*/VALUE", SA_ValueProperty = (string)null, SA_ReferenceXpath = "/*/REFERENCE", SA_ReferenceProperty = (string)null } } ,
			};

			var result2 = controller.AutoSubscribes(new Guid("{00000000-AAAA-2222-0000-000000000000}"));

			Assert.IsNotNull(result2);
			AssertEx.JsonResultMatchesList(expected2, result2, "eHubSubscriptionAutoSubscribes");
        }

		[TestMethod]
		public void TestAutoSubscribesEdit()
		{
            var logger = new TestLogger();

            request.Clear();
			request.Container["id"] = "_empty";
			request.Container["DT_Code"] = "MSG0002";
			request.Container["CC_ID_Recipient"] = "";
			request.Container["SA_ValueXpath"] = "";
			request.Container["SA_ValueProperty"] = "PROPERTYVAL";
			request.Container["SA_ReferenceXpath"] = "";
			request.Container["SA_ReferenceProperty"] = "PROPERTYREF";
			request.Container["oper"] = "add";

			var responseAdd = AutoSubscribesEditTest(new Guid("{00000000-AAAA-1111-0000-000000000000}"), logger);

			var resultState = (bool)responseAdd.Data.GetType().GetProperty("success").GetValue(responseAdd.Data, null);
			Assert.IsTrue(resultState);
			var newId = (Guid)responseAdd.Data.GetType().GetProperty("id").GetValue(responseAdd.Data, null);
			var resultAdd = context.eHubSubscriptionAutoSubscribes.Select(a => new
			{
				a.SA_PK,
				a.SA_DT,
				CC_ID_Recipient = a.eHubClient_Recipient != null ? a.eHubClient_Recipient.CC_ID : null,
				a.SA_ValueXpath,
				a.SA_ValueProperty,
				a.SA_ReferenceXpath,
				a.SA_ReferenceProperty
			}).First(a => a.SA_PK == newId);
			var expectedAdd = new { SA_PK = newId, SA_DT = new Guid("{00000000-EEEE-2222-0000-000000000000}"), CC_ID_Recipient = (string)null, SA_ValueXpath = (string)null, SA_ValueProperty = "PROPERTYVAL", SA_ReferenceXpath = (string)null, SA_ReferenceProperty = "PROPERTYREF" };
			AssertEx.PropertyValuesAreEquals(expectedAdd, resultAdd, false);
            Assert.IsTrue(logger.Log.Contains("Info - [add] eHubSubscriptionAutoSubscribe:"));
            Assert.IsTrue(logger.Log.Contains("SA_ST=00000000-aaaa-1111-0000-000000000000, SA_CC_Recipient=, SA_DT=00000000-eeee-2222-0000-000000000000, SA_ValueXpath=, SA_ValueProperty=PROPERTYVAL, SA_ReferenceXpath=, SA_ReferenceProperty=PROPERTYREF"));

            request.Clear();
			request.Container["SA_PK"] = newId.ToString();
			request.Container["DT_Code"] = "MSG0001";
			request.Container["CC_ID_Recipient"] = "";
			request.Container["SA_ValueXpath"] = "XPATHVAL";
			request.Container["SA_ValueProperty"] = "";
			request.Container["SA_ReferenceXpath"] = "XPATHREF";
			request.Container["SA_ReferenceProperty"] = "";
			request.Container["oper"] = "edit";

			var responseEdit = AutoSubscribesEditTest(new Guid("{00000000-AAAA-1111-0000-000000000000}"), logger);

			Assert.IsTrue((bool)responseEdit.Data.GetType().GetProperty("success").GetValue(responseEdit.Data, null));
			var resultEdit = context.eHubSubscriptionAutoSubscribes.Select(a => new
			{
				a.SA_PK,
				a.SA_DT,
				CC_ID_Recipient = a.eHubClient_Recipient != null ? a.eHubClient_Recipient.CC_ID : null,
				a.SA_ValueXpath,
				a.SA_ValueProperty,
				a.SA_ReferenceXpath,
				a.SA_ReferenceProperty
			}).First(a => a.SA_PK == newId);
			var expectedEdit = new { SA_PK = newId, SA_DT = new Guid("{00000000-EEEE-1111-0000-000000000000}"), CC_ID_Recipient = (string)null, SA_ValueXpath = "XPATHVAL", SA_ValueProperty = (string)null, SA_ReferenceXpath = "XPATHREF", SA_ReferenceProperty = (string)null };
			AssertEx.PropertyValuesAreEquals(expectedAdd, resultAdd, false);
            Assert.IsTrue(logger.Log.Contains("Info - [edit] eHubSubscriptionAutoSubscribe:"));
            Assert.IsTrue(logger.Log.Contains("SA_ST=00000000-aaaa-1111-0000-000000000000, SA_CC_Recipient=, SA_DT=00000000-eeee-1111-0000-000000000000, SA_ValueXpath=XPATHVAL, SA_ValueProperty=, SA_ReferenceXpath=XPATHREF, SA_ReferenceProperty="));

            request.Clear();
			request.Container["SA_PK"] = newId.ToString();
			request.Container["oper"] = "del";

			var responseDel = AutoSubscribesEditTest(new Guid("{00000000-AAAA-1111-0000-000000000000}"), logger);

            Assert.IsTrue((bool)responseDel.Data.GetType().GetProperty("success").GetValue(responseDel.Data, null));
			Assert.IsNull(context.eHubSubscriptionAutoSubscribes.FirstOrDefault(a => a.SA_PK == newId));
            Assert.IsTrue(logger.Log.Contains("Info - [del] eHubSubscriptionAutoSubscribe:"));
            Assert.IsTrue(logger.Log.Contains("SA_ST=00000000-aaaa-1111-0000-000000000000, SA_CC_Recipient=, SA_DT=00000000-eeee-1111-0000-000000000000, SA_ValueXpath=XPATHVAL, SA_ValueProperty=, SA_ReferenceXpath=XPATHREF, SA_ReferenceProperty="));
        }

		[TestMethod]
		public void TestLookupsGet()
		{
			var expected1 = new List<object> { };

			var result1 = controller.Lookups(new Guid("{00000000-AAAA-2222-0000-000000000000}"));

			Assert.IsNotNull(result1);
			AssertEx.JsonResultMatchesList(expected1, result1, "eHubSubscriptionLookups", true);

			var expected2 = new List<object> {
				{ new { SL_PK = new Guid("{00000000-CCCC-1111-0000-000000000000}"), DT_Code = "MSG0001", SL_ValueXpath = "/*/VALUE", SL_ValueProperty = (string)null } } ,
			};

			var result2 = controller.Lookups(new Guid("{00000000-AAAA-2222-0000-000000000000}"));

			Assert.IsNotNull(result2);
			AssertEx.JsonResultMatchesList(expected2, result2, "eHubSubscriptionLookups", true);
		}

		[TestMethod]
		public void TestLookupsEdit()
		{
            var logger = new TestLogger();

            request.Clear();
			request.Container["id"] = "_empty";
			request.Container["DT_Code"] = "MSG0002";
			request.Container["SL_ValueXpath"] = "";
			request.Container["SL_ValueProperty"] = "PROPERTYVAL";
			request.Container["oper"] = "add";

			var responseAdd = LookupsEditTest(new Guid("{00000000-AAAA-2222-0000-000000000000}"), logger);

			var resultState = (bool)responseAdd.Data.GetType().GetProperty("success").GetValue(responseAdd.Data, null);
			Assert.IsTrue(resultState);
			var newId = (Guid)responseAdd.Data.GetType().GetProperty("id").GetValue(responseAdd.Data, null);
			var resultAdd = context.eHubSubscriptionLookups.Select(a => new
			{
				a.SL_PK,
				a.SL_DT,
				a.SL_ValueXpath,
				a.SL_ValueProperty,
			}).First(a => a.SL_PK == newId);
			var expectedAdd = new { SL_PK = newId, SL_DT = new Guid("{00000000-EEEE-2222-0000-000000000000}"), SL_ValueXpath = (string)null, SL_ValueProperty = "PROPERTYVAL" };
			AssertEx.PropertyValuesAreEquals(expectedAdd, resultAdd, false);
            Assert.IsTrue(logger.Log.Contains("Info - [add] eHubSubscriptionLookup:"));
            Assert.IsTrue(logger.Log.Contains("SL_ST=00000000-aaaa-2222-0000-000000000000, SL_DT=00000000-eeee-2222-0000-000000000000, SL_ValueXpath=, SL_ValueProperty=PROPERTYVAL"));

            request.Clear();
			request.Container["SL_PK"] = newId.ToString();
			request.Container["DT_Code"] = "MSG0001";
			request.Container["SL_ValueXpath"] = "XPATHVAL";
			request.Container["SL_ValueProperty"] = "";
			request.Container["oper"] = "edit";

			var responseEdit = LookupsEditTest(new Guid("{00000000-AAAA-1111-0000-000000000000}"), logger);

			Assert.IsTrue((bool)responseEdit.Data.GetType().GetProperty("success").GetValue(responseEdit.Data, null));
			var resultEdit = context.eHubSubscriptionLookups.Select(a => new
			{
				a.SL_PK,
				a.SL_DT,
				a.SL_ValueXpath,
				a.SL_ValueProperty,
			}).First(a => a.SL_PK == newId);
			var expectedEdit = new { SL_PK = newId, SL_DT = new Guid("{00000000-EEEE-1111-0000-000000000000}"), SL_ValueXpath = "XPATHVAL", SL_ValueProperty = (string)null };
			AssertEx.PropertyValuesAreEquals(expectedAdd, resultAdd, false);
            Assert.IsTrue(logger.Log.Contains("Info - [edit] eHubSubscriptionLookup:"));
            Assert.IsTrue(logger.Log.Contains("SL_ST=00000000-aaaa-2222-0000-000000000000, SL_DT=00000000-eeee-1111-0000-000000000000, SL_ValueXpath=XPATHVAL, SL_ValueProperty="));

            request.Clear();
			request.Container["SL_PK"] = newId.ToString();
			request.Container["oper"] = "del";

			var responseDel = LookupsEditTest(new Guid("{00000000-AAAA-1111-0000-000000000000}"), logger);

            Assert.IsTrue((bool)responseDel.Data.GetType().GetProperty("success").GetValue(responseDel.Data, null));
			Assert.IsNull(context.eHubSubscriptionLookups.FirstOrDefault(a => a.SL_PK == newId));
            Assert.IsTrue(logger.Log.Contains("Info - [del] eHubSubscriptionLookup:"));
            Assert.IsTrue(logger.Log.Contains("SL_ST=00000000-aaaa-2222-0000-000000000000, SL_DT=00000000-eeee-1111-0000-000000000000, SL_ValueXpath=XPATHVAL, SL_ValueProperty="));
        }

		[TestMethod]
		public void TestValuesGet()
		{
			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["sidx"] = "eHubClient_Provider.CC_ID";
			request.Container["sord"] = "asc";
			IList<object> expected1 = new List<object>
			{
				new { SV_PK = new Guid("{00000000-DDDD-1111-1111-000000000000}"), SV_CC_Provider = "TEST0001", SV_CC_Subscriber = "TEST0002", SV_Value = "VALUE1", SV_Reference = (string)null, SV_SubscribedUTC = "2014-01-01T12:00:00", SV_ExpiryUTC = "", SV_ReferenceType = (string)null },
				new { SV_PK = new Guid("{00000000-DDDD-1111-2222-000000000000}"), SV_CC_Provider = "TEST0001", SV_CC_Subscriber = "TEST0003", SV_Value = "VALUE2", SV_Reference = (string)null, SV_SubscribedUTC = "2014-01-02T12:00:00", SV_ExpiryUTC = "", SV_ReferenceType = (string)null },
			};

			var result1 = controller.Values(new Guid("{00000000-AAAA-1111-0000-000000000000}"));

			Assert.IsNotNull(result1);
			AssertEx.JsonResultMatchesList(expected1, result1, "eHubSubscriptionValues");

			request.Clear();
			request.Container["page"] = "2";
			request.Container["rows"] = "1";
			request.Container["sidx"] = "eHubClient_Provider.CC_ID";
			request.Container["sord"] = "asc";
			IList<object> expected2 = new List<object>
			{
				new { SV_PK = new Guid("{00000000-DDDD-2222-2222-000000000000}"), SV_CC_Provider = "TEST0001", SV_CC_Subscriber = "TEST0003", SV_Value = "VALUE4", SV_Reference = "REF2", SV_SubscribedUTC = "2014-01-04T12:00:00", SV_ExpiryUTC = "2014-02-04T12:00:00", SV_ReferenceType = "REFTYPE2" },
			};

			var result2 = controller.Values(new Guid("{00000000-AAAA-2222-0000-000000000000}"));

			Assert.IsNotNull(result1);
			AssertEx.JsonResultMatchesList(expected2, result2, "eHubSubscriptionValues");

			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["sidx"] = "SV_Value";
			request.Container["sord"] = "desc";
			IList<object> expected3 = new List<object>
			{
				new { SV_PK = new Guid("{00000000-DDDD-1111-2222-000000000000}"), SV_CC_Provider = "TEST0001", SV_CC_Subscriber = "TEST0003", SV_Value = "VALUE2", SV_Reference = (string)null, SV_SubscribedUTC = "2014-01-02T12:00:00", SV_ExpiryUTC = "", SV_ReferenceType = (string)null },
				new { SV_PK = new Guid("{00000000-DDDD-1111-1111-000000000000}"), SV_CC_Provider = "TEST0001", SV_CC_Subscriber = "TEST0002", SV_Value = "VALUE1", SV_Reference = (string)null, SV_SubscribedUTC = "2014-01-01T12:00:00", SV_ExpiryUTC = "", SV_ReferenceType = (string)null },
			};

			var result3 = controller.Values(new Guid("{00000000-AAAA-1111-0000-000000000000}"));

			Assert.IsNotNull(result1);
			AssertEx.JsonResultMatchesList(expected3, result3, "eHubSubscriptionValues");

			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["sidx"] = "SV_Value";
			request.Container["sord"] = "asc";
			request.Container["filters"] = "{\"groupOp\":\"AND\",\"rules\":[{ \"field\":\"SV_Value\",\"op\":\"ew\",\"data\":\"1\"}]}";
			IList<object> expected4 = new List<object>
			{
				new { SV_PK = new Guid("{00000000-DDDD-1111-1111-000000000000}"), SV_CC_Provider = "TEST0001", SV_CC_Subscriber = "TEST0002", SV_Value = "VALUE1", SV_Reference = (string)null, SV_SubscribedUTC = "2014-01-01T12:00:00", SV_ExpiryUTC = "", SV_ReferenceType = (string)null },
			};

			var result4 = controller.Values(new Guid("{00000000-AAAA-1111-0000-000000000000}"));

			Assert.IsNotNull(result1);
			AssertEx.JsonResultMatchesList(expected4, result4, "eHubSubscriptionValues");
		}

		[TestMethod]
		public void TestValues_MultipleFilter_AND()
		{
			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["sidx"] = "SV_Value";
			request.Container["sord"] = "asc";
			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"eHubClient_Subscriber.CC_ID\",\"op\":\"bw\",\"data\":\"T\"},{ \"field\":\"SV_Value\",\"op\":\"ew\",\"data\":\"1\"}]}";
			IList<object> expected = new List<object>
			{
				new { SV_PK = new Guid("{00000000-DDDD-1111-1111-000000000000}"), SV_CC_Provider = "TEST0001", SV_CC_Subscriber = "TEST0002", SV_Value = "VALUE1", SV_Reference = (string)null, SV_SubscribedUTC = "2014-01-01T12:00:00", SV_ExpiryUTC = "", SV_ReferenceType = (string)null },
			};

			var result = controller.Values(new Guid("{00000000-AAAA-1111-0000-000000000000}"));

			Assert.IsNotNull(result);
			AssertEx.JsonResultMatchesList(expected, result, "eHubSubscriptionValues");

			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["sidx"] = "SV_Value";
			request.Container["sord"] = "asc";
			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[{ \"field\":\"eHubClient_Subscriber.CC_ID\",\"op\":\"bw\",\"data\":\"T\"},{ \"field\":\"eHubClient_Subscribed.CC_ID\",\"op\":\"ge\",\"data\":\"2014-01-02T12:00:00\"}]}";
			IList<object> expected2 = new List<object>
			{
				new { SV_PK = new Guid("{00000000-DDDD-1111-2222-000000000000}"), SV_CC_Provider = "TEST0001", SV_CC_Subscriber = "TEST0003", SV_Value = "VALUE2", SV_Reference = (string)null, SV_SubscribedUTC = "2014-01-02T12:00:00", SV_ExpiryUTC = "", SV_ReferenceType = (string)null},
			};

			var result2 = controller.Values(new Guid("{00000000-AAAA-1111-0000-000000000000}"));

			Assert.IsNotNull(result2);
			AssertEx.JsonResultMatchesList(expected2, result2, "eHubSubscriptionValues");
		}

		[TestMethod]
		public void TestValues_MultipleFilter_OR()
		{
			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["sidx"] = "SV_Value";
			request.Container["sord"] = "asc";
			request.Container["filters"] = "{ \"groupOp\":\"OR\",\"rules\":[{ \"field\":\"eHubClient_Subscriber.CC_ID\",\"op\":\"bw\",\"data\":\"TEST0002\"},{ \"field\":\"SV_Value\",\"op\":\"ew\",\"data\":\"VALUE2\"}]}";
			IList<object> expected1 = new List<object>
			{
				new { SV_PK = new Guid("{00000000-DDDD-1111-1111-000000000000}"), SV_CC_Provider = "TEST0001", SV_CC_Subscriber = "TEST0002", SV_Value = "VALUE1", SV_Reference = (string)null, SV_SubscribedUTC = "2014-01-01T12:00:00", SV_ExpiryUTC = "", SV_ReferenceType = (string)null },
				new { SV_PK = new Guid("{00000000-DDDD-1111-2222-000000000000}"), SV_CC_Provider = "TEST0001", SV_CC_Subscriber = "TEST0003", SV_Value = "VALUE2", SV_Reference = (string)null, SV_SubscribedUTC = "2014-01-02T12:00:00", SV_ExpiryUTC = "", SV_ReferenceType = (string)null },
			};

			var result = controller.Values(new Guid("{00000000-AAAA-1111-0000-000000000000}"));

			Assert.IsNotNull(result);
			AssertEx.JsonResultMatchesList(expected1, result, "eHubSubscriptionValues");

			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["sidx"] = "SV_SubscribedUTC";
			request.Container["sord"] = "desc";
			request.Container["filters"] = "{ \"groupOp\":\"OR\",\"rules\":[{ \"field\":\"eHubClient_Subscriber.CC_ID\",\"op\":\"bw\",\"data\":\"TEST0002\"},{ \"field\":\"SV_Value\",\"op\":\"ew\",\"data\":\"VALUE1\"}]}";
			IList<object> expected2 = new List<object>
			{
				new { SV_PK = new Guid("{00000000-DDDD-1111-1111-000000000000}"), SV_CC_Provider = "TEST0001", SV_CC_Subscriber = "TEST0002", SV_Value = "VALUE1", SV_Reference = (string)null, SV_SubscribedUTC = "2014-01-01T12:00:00", SV_ExpiryUTC = "", SV_ReferenceType = (string)null },
			};

			var result2 = controller.Values(new Guid("{00000000-AAAA-1111-0000-000000000000}"));

			Assert.IsNotNull(result2);
			AssertEx.JsonResultMatchesList(expected2, result2, "eHubSubscriptionValues");
		}

		[TestMethod]
		public void TestValuesEdit()
		{
            var logger = new TestLogger();

            request.Clear();
			request.Container["id"] = "_empty";
			request.Container["SV_CC_Provider"] = "TEST0001";
			request.Container["SV_CC_Subscriber"] = "TEST0002";
			request.Container["SV_Value"] = "VALUE5";
			request.Container["SV_Reference"] = "";
			request.Container["oper"] = "add";
			request.Container["SV_ReferenceType"] = "";

			var responseAdd = ValuesEditTest(new Guid("{00000000-AAAA-2222-0000-000000000000}"), logger);

			var resultState = (bool)responseAdd.Data.GetType().GetProperty("success").GetValue(responseAdd.Data, null);
			Assert.IsTrue(resultState);
			var newId = (Guid)responseAdd.Data.GetType().GetProperty("id").GetValue(responseAdd.Data, null);
			var resultAdd = context.eHubSubscriptionValues.Select(a => new
			{
				a.SV_PK,
				a.SV_CC_Sender,
				a.SV_CC_Recipient,
				a.SV_Value,
				a.SV_Reference,
				a.SV_ReferenceType,
				SV_SubscribedUTC = a.SV_SubscribedUTC != null,
				SV_ExpiryUTC = a.SV_ExpiryUTC == a.SV_SubscribedUTC.AddDays(30),
			}).First(a => a.SV_PK == newId);
			var expectedAdd = new
			{
				SV_PK = newId,
				SV_CC_Sender = new Guid("{00000000-AAAA-1111-0000-000000000000}"),
				SV_CC_Recipient = new Guid("{00000000-AAAA-2222-0000-000000000000}"),
				SV_Value = "VALUE5",
				SV_Reference = (string)null,
				SV_ReferenceType = (string)null,
				SV_SubscribedUTC = true,
				SV_ExpiryUTC = true
			};
			AssertEx.PropertyValuesAreEquals(expectedAdd, resultAdd, false);
            Assert.IsTrue(logger.Log.Contains("Info - [add] eHubSubscriptionValue:"));
            Assert.IsTrue(logger.Log.Contains("SV_ST=00000000-aaaa-2222-0000-000000000000, SV_CC_Sender=00000000-aaaa-1111-0000-000000000000, SV_CC_Recipient=00000000-aaaa-2222-0000-000000000000, SV_Value=VALUE5, SV_Reference=, SV_ReferenceType="));

			request.Clear();
			request.Container["id"] = "_empty";
			request.Container["SV_CC_Provider"] = "TEST0001";
			request.Container["SV_CC_Subscriber"] = "TEST0002";
			request.Container["SV_Value"] = "";
			request.Container["SV_Reference"] = "<CustomXml>DummyXMLValues<CustomXml>";
			request.Container["oper"] = "add";
			request.Container["SV_ReferenceType"] = "";

			var responseAdd2 = ValuesEditTest(new Guid("{00000000-AAAA-2222-0000-000000000000}"), logger);

			var resultState2 = (bool)responseAdd2.Data.GetType().GetProperty("success").GetValue(responseAdd2.Data, null);
			Assert.IsTrue(resultState2);
			var newId2 = (Guid)responseAdd2.Data.GetType().GetProperty("id").GetValue(responseAdd2.Data, null);
			var resultAdd2 = context.eHubSubscriptionValues.Select(a => new
			{
				a.SV_PK,
				a.SV_CC_Sender,
				a.SV_CC_Recipient,
				a.SV_Value,
				a.SV_Reference,
				a.SV_ReferenceType,
				SV_SubscribedUTC = a.SV_SubscribedUTC != null,
				SV_ExpiryUTC = a.SV_ExpiryUTC == a.SV_SubscribedUTC.AddDays(30),
			}).First(a => a.SV_PK == newId2);
			var expectedAdd2 = new
			{
				SV_PK = newId2,
				SV_CC_Sender = new Guid("{00000000-AAAA-1111-0000-000000000000}"),
				SV_CC_Recipient = new Guid("{00000000-AAAA-2222-0000-000000000000}"),
				SV_Value = "",
				SV_Reference = "<CustomXml>DummyXMLValues<CustomXml>",
				SV_ReferenceType = (string)null,
				SV_SubscribedUTC = true,
				SV_ExpiryUTC = true
			};
			AssertEx.PropertyValuesAreEquals(expectedAdd2, resultAdd2, false);
			Assert.IsTrue(logger.Log.Contains("Info - [add] eHubSubscriptionValue:"));
			Assert.IsTrue(logger.Log.Contains("SV_ST=00000000-aaaa-2222-0000-000000000000, SV_CC_Sender=00000000-aaaa-1111-0000-000000000000, SV_CC_Recipient=00000000-aaaa-2222-0000-000000000000, SV_Value=, SV_Reference=<CustomXml>DummyXMLValues<CustomXml>, SV_ReferenceType="));

			request.Clear();
			request.Container["SV_PK"] = newId.ToString();
			request.Container["SV_CC_Provider"] = "TEST0002";
			request.Container["SV_CC_Subscriber"] = "TEST0003";
			request.Container["SV_Value"] = "VALUE6";
			request.Container["SV_Reference"] = "REF3";
			request.Container["SV_ReferenceType"] = "REFTYPE3";
			request.Container["oper"] = "edit";

			var responseEdit = ValuesEditTest(new Guid("{00000000-AAAA-2222-0000-000000000000}"), logger);

			Assert.IsTrue((bool)responseEdit.Data.GetType().GetProperty("success").GetValue(responseEdit.Data, null));
			var resultEdit = context.eHubSubscriptionValues.Select(a => new
			{
				a.SV_PK,
				a.SV_CC_Sender,
				a.SV_CC_Recipient,
				a.SV_Value,
				a.SV_Reference,
				a.SV_ReferenceType,
				SV_SubscribedUTC = a.SV_SubscribedUTC != null,
				SV_ExpiryUTC = a.SV_ExpiryUTC == a.SV_SubscribedUTC.AddDays(30),
			}).First(a => a.SV_PK == newId);
			var expectedEdit = new
			{
				SV_PK = newId,
				SV_CC_Sender = new Guid("{00000000-AAAA-2222-0000-000000000000}"),
				SV_CC_Recipient = new Guid("{00000000-AAAA-3333-0000-000000000000}"),
				SV_Value = "VALUE6",
				SV_Reference = "REF3",
				SV_ReferenceType = "REFTYPE3",
				SV_SubscribedUTC = true,
				SV_ExpiryUTC = true
			};
			AssertEx.PropertyValuesAreEquals(expectedAdd, resultAdd, false);
            Assert.IsTrue(logger.Log.Contains("Info - [edit] eHubSubscriptionValue:"));
            Assert.IsTrue(logger.Log.Contains("SV_ST=00000000-aaaa-2222-0000-000000000000, SV_CC_Sender=00000000-aaaa-2222-0000-000000000000, SV_CC_Recipient=00000000-aaaa-3333-0000-000000000000, SV_Value=VALUE6, SV_Reference=REF3, SV_ReferenceType=REFTYPE3"));

            request.Clear();
			request.Container["SV_PK"] = newId.ToString();
			request.Container["oper"] = "del";

			var responseDel = ValuesEditTest(new Guid("{00000000-AAAA-2222-0000-000000000000}"), logger);

            Assert.IsTrue((bool)responseDel.Data.GetType().GetProperty("success").GetValue(responseDel.Data, null));
			Assert.IsNull(context.eHubSubscriptionValues.FirstOrDefault(a => a.SV_PK == newId));
            Assert.IsTrue(logger.Log.Contains("Info - [del] eHubSubscriptionValue:"));
            Assert.IsTrue(logger.Log.Contains("SV_ST=00000000-aaaa-2222-0000-000000000000, SV_CC_Sender=00000000-aaaa-2222-0000-000000000000, SV_CC_Recipient=00000000-aaaa-3333-0000-000000000000, SV_Value=VALUE6, SV_Reference=REF3, SV_ReferenceType=REFTYPE3"));
        }

		[TestMethod]
		public void TestValuesExportCsv()
		{
			request.Clear();
			request.Container["typePK"] = "{00000000-AAAA-1111-0000-000000000000}";

			string expectedData = "Provider,Subscriber,Reference Type,Value,Reference,Subscribed Time\r\nTEST0001,TEST0002,,VALUE1,,2014-01-01T12:00:00.0000000Z\r\nTEST0001,TEST0003,,VALUE2,,2014-01-02T12:00:00.0000000Z\r\n";
			string expectedName = "SUB1_Subscription1.csv";

			var result = controller.ValuesExportCsv();
			Assert.AreEqual(expectedData, Encoding.Default.GetString(result.FileContents));
			Assert.AreEqual(expectedName, result.FileDownloadName);
		}

		[TestMethod]
		public void TestCreateMessageDetailsLink()
		{
			var provider = "TestProvider";
			var subscriber = "TestSubscriber";
			var subscribedUTC = DateTime.Parse("2014-01-07T12:00");
			ConfigurationManager.AppSettings.Set("AdminWebsite", "http://someServer/eHubAdmin");
			string expectedUrl = "http://someServer/eHubAdmin/Messages?Role=Specific&DateFilterType=WithinDates&DateRangeType=UTC&IsIncludingArchiveStaging=False&SenderInbox=TestSubscriber&RecipientOutbox=TestProvider&From=20140107115950&To=20140107120010";
			string actualUrl = controller.CreateMessageDetailsLink(provider, subscriber, subscribedUTC);
			Assert.AreEqual(expectedUrl, actualUrl);
		}

		[TestMethod]
		public void TestValuesImportCsv()
		{
            var logger = new TestLogger();

            request.Clear();
			request.Container["typePK"] = "{00000000-AAAA-1111-0000-000000000000}";
			request.Container["option"] = "merge";
			request.AddFile("uploadFile", "Provider,Subscriber,Reference Type,Value,Reference\r\nTEST0001,TEST0003,REFTYPE3,VALUE2,REF3\r\nTEST0002,TEST0003,,VALUE5\r\nTEST0001,TEST0003,REFTYPE9,VALUE2,REF3\r\n");
			List<object> expectedMerge = new List<object>
			{
				new { SV_CC_Provider = new Guid("{00000000-AAAA-1111-0000-000000000000}"), SV_CC_Subscriber = new Guid("{00000000-AAAA-2222-0000-000000000000}"),
					SV_Value = "VALUE1", SV_Reference = (string)null, SV_SubscribedUTC = true, SV_ExpiryUTC = false, SV_ReferenceType = (string)null },
				new { SV_CC_Provider = new Guid("{00000000-AAAA-1111-0000-000000000000}"), SV_CC_Subscriber = new Guid("{00000000-AAAA-3333-0000-000000000000}"),
					SV_Value = "VALUE2", SV_Reference = (string)null, SV_SubscribedUTC = true, SV_ExpiryUTC = false, SV_ReferenceType = (string)null },
				new { SV_CC_Provider = new Guid("{00000000-AAAA-1111-0000-000000000000}"), SV_CC_Subscriber = new Guid("{00000000-AAAA-3333-0000-000000000000}"),
					SV_Value = "VALUE2", SV_Reference = "REF3", SV_SubscribedUTC = true, SV_ExpiryUTC = false, SV_ReferenceType = "REFTYPE3" },
				new { SV_CC_Provider = new Guid("{00000000-AAAA-1111-0000-000000000000}"), SV_CC_Subscriber = new Guid("{00000000-AAAA-3333-0000-000000000000}"),
					SV_Value = "VALUE2", SV_Reference = "REF3", SV_SubscribedUTC = true, SV_ExpiryUTC = false, SV_ReferenceType = "REFTYPE9" },
				new { SV_CC_Provider = new Guid("{00000000-AAAA-2222-0000-000000000000}"), SV_CC_Subscriber = new Guid("{00000000-AAAA-3333-0000-000000000000}"),
					SV_Value = "VALUE5", SV_Reference = (string)null, SV_SubscribedUTC = true, SV_ExpiryUTC = false, SV_ReferenceType = (string)null },
			};

			ValuesImportCsvTest(logger);

			var resultMerge = context.eHubSubscriptionValues
				.Where(a => a.SV_ST == new Guid("{00000000-AAAA-1111-0000-000000000000}"))
				.OrderBy(a => a.SV_Value)
				.ThenBy(a => a.SV_ReferenceType)
				.Select(a => new
				{
					SV_CC_Provider = a.SV_CC_Sender,
					SV_CC_Subscriber = a.SV_CC_Recipient,
					a.SV_Value,
					a.SV_Reference,
					a.SV_ReferenceType,
					SV_SubscribedUTC = a.SV_SubscribedUTC != null,
					SV_ExpiryUTC = a.SV_ExpiryUTC != null
				})
				.ToList();

			expectedMerge.Zip(resultMerge, (e, r) => new { e, r }).ToList().ForEach(z => AssertEx.PropertyValuesAreEquals(z.e, z.r, false));
            Assert.IsTrue(logger.Log.Contains("Info - [add] eHubSubscriptionValue:"));
            Assert.IsTrue(logger.Log.Contains("SV_ST=00000000-aaaa-1111-0000-000000000000, SV_CC_Sender=00000000-aaaa-1111-0000-000000000000, SV_CC_Recipient=00000000-aaaa-3333-0000-000000000000, SV_Value=VALUE2, SV_Reference=REF3, SV_ReferenceType=REFTYPE9"));
			Assert.IsTrue(logger.Log.Contains("SV_ST=00000000-aaaa-1111-0000-000000000000, SV_CC_Sender=00000000-aaaa-2222-0000-000000000000, SV_CC_Recipient=00000000-aaaa-3333-0000-000000000000, SV_Value=VALUE5, SV_Reference=, SV_ReferenceType="));

			request.Clear();
			request.Container["typePK"] = "{00000000-AAAA-2222-0000-000000000000}";
			request.Container["option"] = "replace";
			request.AddFile("uploadFile", "Provider,Subscriber,Reference Type,Value,Reference\r\nTEST0001,TEST0003,REFTYPE3,VALUE2,REF3\r\nTEST0002,TEST0003,REFTYPE4,VALUE5,REF4\r\nTEST0001,TEST0003,REFTYPE9,VALUE2,REF3\r\n");
			IList<object> expectedReplace = new List<object>
			{
				new { SV_CC_Provider = new Guid("{00000000-AAAA-1111-0000-000000000000}"), SV_CC_Subscriber = new Guid("{00000000-AAAA-3333-0000-000000000000}"), SV_Value = "VALUE2", SV_Reference = "REF3", SV_SubscribedUTC = true, SV_ExpiryUTC = true, SV_ReferenceType = "REFTYPE3" },
				new { SV_CC_Provider = new Guid("{00000000-AAAA-2222-0000-000000000000}"), SV_CC_Subscriber = new Guid("{00000000-AAAA-3333-0000-000000000000}"), SV_Value = "VALUE5", SV_Reference = "REF4", SV_SubscribedUTC = true, SV_ExpiryUTC = true, SV_ReferenceType = "REFTYPE4" },
			};

			ValuesImportCsvTest(logger);

            var resultReplace = context.eHubSubscriptionValues.Where(a => a.SV_ST == new Guid("{00000000-AAAA-2222-0000-000000000000}")).Select(a => new
			{
				SV_CC_Provider = a.SV_CC_Sender,
				SV_CC_Subscriber = a.SV_CC_Recipient,
				a.SV_Value,
				a.SV_Reference,
				a.SV_ReferenceType,
				SV_SubscribedUTC = a.SV_SubscribedUTC != null,
				SV_ExpiryUTC = a.SV_ExpiryUTC == a.SV_SubscribedUTC.AddDays(30),
			}).ToList();

			expectedReplace.Zip(resultReplace, (e, r) => new { e, r }).ToList().ForEach(z => AssertEx.PropertyValuesAreEquals(z.e, z.r, false));
			Assert.IsTrue(logger.Log.Contains("Info - [del] eHubSubscriptionValue:"));
			Assert.IsTrue(logger.Log.Contains("SV_PK=00000000-dddd-2222-2222-000000000000, SV_ST=00000000-aaaa-2222-0000-000000000000, SV_CC_Sender=00000000-aaaa-1111-0000-000000000000, SV_CC_Recipient=00000000-aaaa-3333-0000-000000000000, SV_Value=VALUE4, SV_Reference=REF2, SV_ReferenceType=REFTYPE2"));
			Assert.IsTrue(logger.Log.Contains("SV_PK=00000000-dddd-2222-1111-000000000000, SV_ST=00000000-aaaa-2222-0000-000000000000, SV_CC_Sender=00000000-aaaa-1111-0000-000000000000, SV_CC_Recipient=00000000-aaaa-2222-0000-000000000000, SV_Value=VALUE3, SV_Reference=REF1, SV_ReferenceType=REFTYPE1"));
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
		public void TestMessageTypesGet()
		{
			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "8";
			request.Container["sidx"] = "DT_Code";
			request.Container["sord"] = "asc";
			request.Container["_search"] = "false";
			IList<object> expected1 = new List<object>
			{
				new { DT_Code = "MSG0001" },
				new { DT_Code = "MSG0002" },
			};

			var result1 = controller.MessageTypes();

			Assert.IsNotNull(result1);
			AssertEx.JsonResultMatchesList(expected1, result1, "eHubMessageTypes");

			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["sidx"] = "DT_Code";
			request.Container["sord"] = "desc";
			request.Container["_search"] = "true";
			request.Container["DT_Code"] = "2";
			IList<object> expected2 = new List<object>
			{
				new { DT_Code = "MSG0002" },
			};

			var result2 = controller.MessageTypes();

			Assert.IsNotNull(result2);
			AssertEx.JsonResultMatchesList(expected2, result2, "eHubMessageTypes");
		}

        protected void TypeInfoEditTest(ILog logger)
        {
            controller.logger = logger;
            controller.TypeInfoEdit();
        }

        protected JsonResult AutoSubscribesEditTest(Guid type, ILog logger)
        {
            controller.logger = logger;
            return controller.AutoSubscribesEdit(type);
        }

        protected JsonResult LookupsEditTest(Guid type, ILog logger)
        {
            controller.logger = logger;
            return controller.LookupsEdit(type);
        }

        protected JsonResult ValuesEditTest(Guid type, ILog logger)
        {
            controller.logger = logger;
            return controller.ValuesEdit(type);
        }

        protected void ValuesImportCsvTest(ILog logger)
        {
            controller.logger = logger;
            controller.ValuesImportCsv();
        }
    }
}
