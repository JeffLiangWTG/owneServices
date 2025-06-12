using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using CargoWise.eHub.Portal.Controllers;
using CargoWise.eHub.Portal.Tests.TestHelpers;
using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Portal.Tests.Controllers
{
	[TestClass]
	public class BroadcastSubscriptionsControllerTest : BaseControllerTest<BroadcastSubscriptionsController>
	{
		[TestMethod]
		public void TestIndex()
		{
			var result = controller.Index() as ViewResult;
			Assert.IsNotNull(result);
			Assert.AreEqual(string.Empty, result.ViewName, "Should be empty (Index)");
		}

		[TestMethod]
		public void TestBroadcastersGet()
		{
			var expected = new List<object> {
				{ new { SB_PK = new Guid("{00000000-EEEE-1111-1111-000000000000}"), ST_ID = "SUB3", ST_Name = "Subscription 3", CC_ID_Sender = "TEST0001" } } ,
				{ new { SB_PK = new Guid("{00000000-EEEE-1111-2222-000000000000}"), ST_ID = "SUB5", ST_Name = "Subscription 5", CC_ID_Sender = "TEST0001", CC_ID_Recipient = "TEST0002" } } ,
			};

			var result = controller.Broadcasters();

			Assert.IsNotNull(result);
			AssertEx.JsonResultMatchesList(expected, result, "eHubSubscriptionBroadcasters");
		}

		[TestMethod]
		public void TestBroadcasterInfoGet()
		{
			TestBroadcasterInfoGet(new { SB_PK = new Guid("{00000000-EEEE-1111-1111-000000000000}"), ST_ID = "SUB3", ST_Name = "Subscription 3", CC_ID_Sender = "TEST0001" });
			TestBroadcasterInfoGet(new { SB_PK = new Guid("{00000000-EEEE-1111-2222-000000000000}"), ST_ID = "SUB5", ST_Name = "Subscription 5", CC_ID_Sender = "TEST0001", CC_ID_Recipient = "TEST0002", SB_SubscriberSelectSql = "SELECT *" });
		}

		public void TestBroadcasterInfoGet(dynamic expected)
		{
			var result = controller.BroadcasterInfo(expected.SB_PK);

			Assert.IsNotNull(result);
			var resultData = result.Data.GetType().GetProperty("eHubSubscriptionBroadcaster").GetValue(result.Data, null);
			AssertEx.PropertyValuesAreEquals(expected, resultData, false);
		}

		[TestMethod]
		public void TestBroadcasterInfoEdit()
		{
            var logger = new TestLogger();

            var sbPK = new Guid("{00000000-EEEE-1111-1111-000000000000}");
			request.Clear();
			request.Container["SB_PK"] = sbPK.ToString();
			request.Container["ST_ID"] = "SUB6";
			request.Container["ST_Name"] = "Subscription 6";
			request.Container["CC_ID_Sender"] = "TEST0002";
			request.Container["CC_ID_Recipient"] = null;
			request.Container["oper"] = "add";

			BroadcasterInfoEditTest(logger);

			var resultAdd = context.eHubSubscriptionBroadcasters.Select(b => new 
			{
				SB_PK = b.SB_PK,
				ST_ID = b.eHubSubscriptionType.ST_ID,
				ST_Name = b.eHubSubscriptionType.ST_Name,
				CC_ID_Sender = b.eHubClient_Sender.CC_ID,
				CC_ID_Recipient = b.SB_CC_Recipient == null ? null : context.eHubClients.First(c => c.CC_PK == b.SB_CC_Recipient).CC_ID,
				SB_SubscriberSelectSql = b.SB_SubscriberSelectSql
			}).First(b => b.SB_PK == sbPK);
			var expectedAdd = new 
			{ 
				SB_PK = sbPK, 
				ST_ID = "SUB6", 
				ST_Name = "Subscription 6",
				CC_ID_Sender = "TEST0002",
				CC_ID_Recipient = (string)null,
				SB_SubscriberSelectSql = (string)null
			};
			AssertEx.PropertyValuesAreEquals(expectedAdd, resultAdd, false);
            Assert.IsTrue(logger.Log.Contains("Info - [add] eHubSubscriptionBroadcaster: SB_PK=00000000-eeee-1111-1111-000000000000"));
            Assert.IsTrue(logger.Log.Contains("SB_CC_Sender=00000000-aaaa-2222-0000-000000000000"));
			Assert.IsTrue(logger.Log.Contains("SB_CC_Recipient=NULL"));

            request.Clear();
			request.Container["SB_PK"] = sbPK.ToString();
			request.Container["ST_ID"] = "SUB6a";
			request.Container["ST_Name"] = "Subscription 6a";
			request.Container["CC_ID_Sender"] = "TEST0003";
			request.Container["CC_ID_Recipient"] = "CLIENT0004";
			request.Container["SB_SubscriberSelectSql"] = "SELECT *";
			request.Container["oper"] = "edit";
			var expectedEdit = new { SB_PK = sbPK, ST_ID = "SUB6a", ST_Name = "Subscription 6a", CC_ID_Sender = "TEST0003", CC_ID_Recipient = "CLIENT0004"};

            BroadcasterInfoEditTest(logger);

            var resultEdit = context.eHubSubscriptionBroadcasters.Select(b => new
			{
				SB_PK = b.SB_PK,
				ST_ID = b.eHubSubscriptionType.ST_ID,
				ST_Name = b.eHubSubscriptionType.ST_Name,
				CC_ID_Sender = b.eHubClient_Sender.CC_ID,
				CC_ID_Recipient = b.SB_CC_Recipient == null ? null : context.eHubClients.First(c => c.CC_PK == b.SB_CC_Recipient).CC_ID,
			}).First(b => b.SB_PK == sbPK);
			AssertEx.PropertyValuesAreEquals(expectedEdit, resultEdit, false);
            Assert.IsTrue(logger.Log.Contains("Info - [edit] eHubSubscriptionBroadcaster: SB_PK=00000000-eeee-1111-1111-000000000000"));
            Assert.IsTrue(logger.Log.Contains("SB_CC_Sender=00000000-aaaa-3333-0000-000000000000"));
			Assert.IsTrue(logger.Log.Contains("SB_CC_Recipient=00000000-aaaa-4444-0000-000000000000"));

            request.Clear();
			request.Container["SB_PK"] = sbPK.ToString();
			request.Container["oper"] = "del";

            BroadcasterInfoEditTest(logger);

            Assert.IsNull(context.eHubSubscriptionBroadcasters.FirstOrDefault(t => t.SB_PK == sbPK));
            Assert.IsTrue(logger.Log.Contains("Info - [del] eHubSubscriptionBroadcaster: SB_PK=00000000-eeee-1111-1111-000000000000"));
            Assert.IsTrue(logger.Log.Contains("SB_CC_Sender=00000000-aaaa-3333-0000-000000000000"));
			Assert.IsTrue(logger.Log.Contains("SB_CC_Recipient=00000000-aaaa-4444-0000-000000000000"));
        }

		[TestMethod]
		public void TestSubscribersGet()
		{
			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["sidx"] = "eHubClient_Subscriber.CC_ID";
			request.Container["sord"] = "asc";
			IList<object> expected1 = new List<object>
		    {
		        new { SV_PK = new Guid("{00000000-DDDD-3333-1111-000000000000}"), SV_CC_Provider = "TEST0001", SV_CC_SubscriberID = "CLIENT0006", SV_CC_SubscriberName = "Client 6", SV_SubscribedUTC = "2014-01-06T12:00:00", SV_Value = "FALSE" },
		        new { SV_PK = new Guid("{00000000-DDDD-3333-2222-000000000000}"), SV_CC_Provider = "TEST0001", SV_CC_SubscriberID = "CLIENT0007", SV_CC_SubscriberName = "Client 7", SV_SubscribedUTC = "2014-01-07T12:00:00", SV_Value = "FALSE" },
		    };

			var result1 = controller.Subscribers(new Guid("{00000000-EEEE-1111-1111-000000000000}"));

			Assert.IsNotNull(result1);
			AssertEx.JsonResultMatchesList(expected1, result1, "eHubSubscriptionValues");
		}

		[TestMethod]
		public void TestSubscribersGetWithANDCondition()
		{
			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["filters"] = "{ \"groupOp\":\"AND\",\"rules\":[" +
				"{ \"field\":\"eHubClient_Subscriber.CC_ID\",\"op\":\"bw\",\"data\":\"CLIENT0007\"}," +
				"{ \"field\":\"eHubClient_Subscriber.CC_FriendlyName\",\"op\":\"cw\",\"data\":\"Client 7\"}]}";
			IList<object> expected1 = new List<object>
			{
				new { SV_PK = new Guid("{00000000-DDDD-3333-2222-000000000000}"), SV_CC_Provider = "TEST0001", SV_CC_SubscriberID = "CLIENT0007", SV_CC_SubscriberName = "Client 7", SV_SubscribedUTC = "2014-01-07T12:00:00", SV_Value = "FALSE" },
			};

			var result1 = controller.Subscribers(new Guid("{00000000-EEEE-1111-1111-000000000000}"));

			Assert.IsNotNull(result1);
			AssertEx.JsonResultMatchesList(expected1, result1, "eHubSubscriptionValues");
		}

		[TestMethod]
		public void TestSubscribersGetWithORCondition()
		{
			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["filters"] = "{ \"groupOp\":\"OR\",\"rules\":[" +
				"{ \"field\":\"eHubClient_Subscriber.CC_ID\",\"op\":\"bw\",\"data\":\"CLIENT0007\"}," +
				"{ \"field\":\"eHubClient_Subscriber.CC_FriendlyName\",\"op\":\"cw\",\"data\":\"Client 6\"}]}";
			IList<object> expected1 = new List<object>
			{
				new { SV_PK = new Guid("{00000000-DDDD-3333-1111-000000000000}"), SV_CC_Provider = "TEST0001", SV_CC_SubscriberID = "CLIENT0006", SV_CC_SubscriberName = "Client 6", SV_SubscribedUTC = "2014-01-06T12:00:00", SV_Value = "FALSE" },
				new { SV_PK = new Guid("{00000000-DDDD-3333-2222-000000000000}"), SV_CC_Provider = "TEST0001", SV_CC_SubscriberID = "CLIENT0007", SV_CC_SubscriberName = "Client 7", SV_SubscribedUTC = "2014-01-07T12:00:00", SV_Value = "FALSE" },
			};

			var result1 = controller.Subscribers(new Guid("{00000000-EEEE-1111-1111-000000000000}"));

			Assert.IsNotNull(result1);
			AssertEx.JsonResultMatchesList(expected1, result1, "eHubSubscriptionValues");
		}

		[TestMethod]
		public void TestSubscribersEdit()
		{
            var logger = new TestLogger();

            var sbPK = new Guid("{00000000-EEEE-1111-1111-000000000000}");

			request.Clear();
			request.Container["id"] = "_empty";
			request.Container["SV_CC_Provider"] = "TEST0001";
			request.Container["SV_CC_SubscriberID"] = "CLIENT0008";
			request.Container["oper"] = "add";

			var responseAdd = SubscriberEditTest(sbPK, logger);

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
				SV_SubscribedUTC = a.SV_SubscribedUTC != null,
				a.SV_ExpiryUTC,
			}).First(a => a.SV_PK == newId);
			var expectedAdd = new
			{
				SV_PK = newId,
				SV_CC_Sender = new Guid("{00000000-AAAA-1111-0000-000000000000}"),
				SV_CC_Recipient = new Guid("{00000000-AAAA-8888-0000-000000000000}"),
				SV_Value = "FALSE",
				SV_Reference = (string)null,
				SV_SubscribedUTC = true,
				SV_ExpiryUTC = (DateTime?)null
			};
			AssertEx.PropertyValuesAreEquals(expectedAdd, resultAdd, false);
            Assert.IsTrue(logger.Log.Contains("Info - [add] eHubSubscriptionValue:"));
            Assert.IsTrue(logger.Log.Contains("SV_ST=00000000-aaaa-3333-0000-000000000000, SV_CC_Sender=00000000-aaaa-1111-0000-000000000000, SV_CC_Recipient=00000000-aaaa-8888-0000-000000000000, SV_Value=FALSE, SV_Reference=, SV_ReferenceType="));
            Assert.IsTrue(logger.Log.Contains("SV_ExpiryUTC="));

            request.Clear();
			request.Container["SV_PK"] = newId.ToString();
			request.Container["SV_CC_Provider"] = "TEST0001";
			request.Container["SV_CC_SubscriberID"] = "CLIENT0008";
            request.Container["SV_Value"] = "True";
			request.Container["oper"] = "edit";

			var responseEdit = SubscriberEditTest(sbPK, logger);

            Assert.IsTrue((bool)responseEdit.Data.GetType().GetProperty("success").GetValue(responseEdit.Data, null));
			var resultEdit = context.eHubSubscriptionValues.Select(a => new
			{
				a.SV_PK,
				a.SV_CC_Sender,
				a.SV_CC_Recipient,
				a.SV_Value,
				a.SV_Reference,
				SV_SubscribedUTC = a.SV_SubscribedUTC != null,
				a.SV_ExpiryUTC
			}).First(a => a.SV_PK == newId);
			var expectedEdit = new
			{
				SV_PK = newId,
				SV_CC_Sender = new Guid("{00000000-AAAA-1111-0000-000000000000}"),
				SV_CC_Recipient = new Guid("{00000000-AAAA-8888-0000-000000000000}"),
				SV_Value = "TRUE",
				SV_Reference = (string)null,
				SV_SubscribedUTC = true,
				SV_ExpiryUTC = (DateTime?)null
			};
			AssertEx.PropertyValuesAreEquals(expectedEdit, resultEdit, false);
            Assert.IsTrue(logger.Log.Contains("Info - [edit] eHubSubscriptionValue:"));
            Assert.IsTrue(logger.Log.Contains("SV_ST=00000000-aaaa-3333-0000-000000000000, SV_CC_Sender=00000000-aaaa-1111-0000-000000000000, SV_CC_Recipient=00000000-aaaa-8888-0000-000000000000, SV_Value=TRUE, SV_Reference=, SV_ReferenceType="));
            Assert.IsTrue(logger.Log.Contains("SV_ExpiryUTC="));

            request.Clear();
			request.Container["SV_PK"] = newId.ToString();
			request.Container["oper"] = "del";

			var responseDel = SubscriberEditTest(sbPK, logger);

            Assert.IsTrue((bool)responseDel.Data.GetType().GetProperty("success").GetValue(responseDel.Data, null));
			Assert.IsNull(context.eHubSubscriptionValues.FirstOrDefault(a => a.SV_PK == newId));
            Assert.IsTrue(logger.Log.Contains("Info - [del] eHubSubscriptionValue:"));
            Assert.IsTrue(logger.Log.Contains("SV_ST=00000000-aaaa-3333-0000-000000000000, SV_CC_Sender=00000000-aaaa-1111-0000-000000000000, SV_CC_Recipient=00000000-aaaa-8888-0000-000000000000, SV_Value=TRUE, SV_Reference=, SV_ReferenceType="));
            Assert.IsTrue(logger.Log.Contains("SV_ExpiryUTC="));
        }

		[TestMethod]
		public void TestValuesExportCsv()
		{
			request.Clear();
			request.Container["broadcasterPK"] = "{00000000-EEEE-1111-1111-000000000000}";

			string expectedData = "Subscriber,Archive Outbox\r\nCLIENT0006,FALSE\r\nCLIENT0007,FALSE\r\n";
			string expectedName = "SUB3_Subscription3.csv";

			var result = controller.SubscribersExportCsv();
			Assert.AreEqual(expectedData, Encoding.Default.GetString(result.FileContents));
			Assert.AreEqual(expectedName, result.FileDownloadName);
		}

		[TestMethod]
		public void TestValuesImportCsv()
		{
            var logger = new TestLogger();
            var expectedWarning = new
            {
                success = true,
                archivedWarning = true,
                message = "WARNING: Archived value must be 'TRUE' or 'FALSE'. Imported file has some invalid archived value(s). All of invalid values are converted to 'FALSE' for 'Archive Outbox'"
            };

            request.Clear();
			request.Container["broadcasterPK"] = "{00000000-EEEE-1111-1111-000000000000}";
			request.Container["option"] = "merge";
			request.AddFile("uploadFile", "Subscriber,Archive Outbox\r\nCLIENT0006,True\r\nCLIENT0008,fALsE\r\nCLIENT0005,Invalid");
			List<Tuple<Guid,Guid>> expectedMerge = new List<Tuple<Guid,Guid>>
		    {
                new Tuple<Guid,Guid>(new Guid("{00000000-AAAA-1111-0000-000000000000}"), new Guid("{00000000-AAAA-5555-0000-000000000000}")),
                new Tuple<Guid,Guid>(new Guid("{00000000-AAAA-1111-0000-000000000000}"), new Guid("{00000000-AAAA-6666-0000-000000000000}")),
		        new Tuple<Guid,Guid>(new Guid("{00000000-AAAA-1111-0000-000000000000}"), new Guid("{00000000-AAAA-7777-0000-000000000000}")),
		        new Tuple<Guid,Guid>(new Guid("{00000000-AAAA-1111-0000-000000000000}"), new Guid("{00000000-AAAA-8888-0000-000000000000}")),
		    };

            JsonResult responseWarning = SubscribersImportCsvTest(logger);

            Assert.IsNotNull(responseWarning);
            AssertEx.PropertyValuesAreEquals(expectedWarning, responseWarning.Data, false);

            var resultMerge = context.eHubSubscriptionValues.Where(a => a.SV_ST == new Guid("{00000000-AAAA-3333-0000-000000000000}"))
				.Select(a => new Tuple<Guid, Guid>(a.SV_CC_Sender, a.SV_CC_Recipient)).ToList();

			CollectionAssert.AreEquivalent(expectedMerge, resultMerge);
            Assert.IsTrue(logger.Log.Contains("Info - [add] eHubSubscriptionValue:"));
            Assert.IsTrue(logger.Log.Contains("SV_ST=00000000-aaaa-3333-0000-000000000000, SV_CC_Sender=00000000-aaaa-1111-0000-000000000000, SV_CC_Recipient=00000000-aaaa-5555-0000-000000000000, SV_Value=FALSE, SV_Reference=, SV_ReferenceType="));
            Assert.IsTrue(logger.Log.Contains("SV_ExpiryUTC="));
            Assert.IsTrue(logger.Log.Contains("Info - [edit] eHubSubscriptionValue:"));
            Assert.IsTrue(logger.Log.Contains("SV_PK=00000000-dddd-3333-1111-000000000000, SV_ST=00000000-aaaa-3333-0000-000000000000, SV_CC_Sender=00000000-aaaa-1111-0000-000000000000, SV_CC_Recipient=00000000-aaaa-6666-0000-000000000000, SV_Value=TRUE, SV_Reference=, SV_ReferenceType="));
            Assert.IsTrue(logger.Log.Contains("SV_ExpiryUTC="));
            Assert.IsTrue(logger.Log.Contains("Info - [add] eHubSubscriptionValue:"));
            Assert.IsTrue(logger.Log.Contains("SV_ST=00000000-aaaa-3333-0000-000000000000, SV_CC_Sender=00000000-aaaa-1111-0000-000000000000, SV_CC_Recipient=00000000-aaaa-8888-0000-000000000000, SV_Value=FALSE, SV_Reference=, SV_ReferenceType="));
            Assert.IsTrue(logger.Log.Contains("SV_ExpiryUTC="));

            logger = new TestLogger();
            request.Clear();
			request.Container["broadcasterPK"] = "{00000000-EEEE-1111-1111-000000000000}";
			request.Container["option"] = "replace";
			request.AddFile("uploadFile", "Subscriber,Archive Outbox\r\nCLIENT0006,InvalidValue\r\nCLIENT0008,trUe\r\nCLIENT0005,tRuE");
			List<Tuple<Guid,Guid>> expectedReplace = new List<Tuple<Guid,Guid>>
		    {
                new Tuple<Guid,Guid>(new Guid("{00000000-AAAA-1111-0000-000000000000}"), new Guid("{00000000-AAAA-5555-0000-000000000000}")),
                new Tuple<Guid,Guid>(new Guid("{00000000-AAAA-1111-0000-000000000000}"), new Guid("{00000000-AAAA-6666-0000-000000000000}")),
		        new Tuple<Guid,Guid>(new Guid("{00000000-AAAA-1111-0000-000000000000}"), new Guid("{00000000-AAAA-8888-0000-000000000000}")),
		    };

            responseWarning = SubscribersImportCsvTest(logger);

            Assert.IsNotNull(responseWarning);
            AssertEx.PropertyValuesAreEquals(expectedWarning, responseWarning.Data, false);

            var resultReplace = context.eHubSubscriptionValues.Where(a => a.SV_ST == new Guid("{00000000-AAAA-3333-0000-000000000000}"))
				.Select(a => new Tuple<Guid, Guid>(a.SV_CC_Sender, a.SV_CC_Recipient)).ToList();

			CollectionAssert.AreEquivalent(expectedReplace, resultReplace);
            Assert.IsTrue(logger.Log.Contains("Info - [edit] eHubSubscriptionValue:"));
            Assert.IsTrue(logger.Log.Contains("SV_ST=00000000-aaaa-3333-0000-000000000000, SV_CC_Sender=00000000-aaaa-1111-0000-000000000000, SV_CC_Recipient=00000000-aaaa-5555-0000-000000000000, SV_Value=TRUE, SV_Reference=, SV_ReferenceType="));
            Assert.IsTrue(logger.Log.Contains("SV_ExpiryUTC="));
            Assert.IsTrue(logger.Log.Contains("Info - [edit] eHubSubscriptionValue:"));
            Assert.IsTrue(logger.Log.Contains("SV_PK=00000000-dddd-3333-1111-000000000000, SV_ST=00000000-aaaa-3333-0000-000000000000, SV_CC_Sender=00000000-aaaa-1111-0000-000000000000, SV_CC_Recipient=00000000-aaaa-6666-0000-000000000000, SV_Value=FALSE, SV_Reference=, SV_ReferenceType="));
            Assert.IsTrue(logger.Log.Contains("SV_ExpiryUTC="));
            Assert.IsTrue(logger.Log.Contains("Info - [edit] eHubSubscriptionValue:"));
            Assert.IsTrue(logger.Log.Contains("SV_ST=00000000-aaaa-3333-0000-000000000000, SV_CC_Sender=00000000-aaaa-1111-0000-000000000000, SV_CC_Recipient=00000000-aaaa-8888-0000-000000000000, SV_Value=TRUE, SV_Reference=, SV_ReferenceType="));
            Assert.IsTrue(logger.Log.Contains("SV_ExpiryUTC="));
            Assert.IsTrue(logger.Log.Contains("Info - [del] eHubSubscriptionValue:"));
            Assert.IsTrue(logger.Log.Contains("SV_PK=00000000-dddd-3333-2222-000000000000, SV_ST=00000000-aaaa-3333-0000-000000000000, SV_CC_Sender=00000000-aaaa-1111-0000-000000000000, SV_CC_Recipient=00000000-aaaa-7777-0000-000000000000, SV_Value=FALSE, SV_Reference=, SV_ReferenceType="));
            Assert.IsTrue(logger.Log.Contains("SV_ExpiryUTC="));

            logger = new TestLogger();
            request.Clear();
            request.Container["broadcasterPK"] = "{00000000-EEEE-1111-1111-000000000000}";
            request.Container["option"] = "replace";
            request.AddFile("uploadFile", "Subscriber,Archive Outbox\r\nCLIENT0006,True\r\nCLIENT0005,fALse");
            expectedReplace = new List<Tuple<Guid, Guid>>
            {
                new Tuple<Guid,Guid>(new Guid("{00000000-AAAA-1111-0000-000000000000}"), new Guid("{00000000-AAAA-5555-0000-000000000000}")),
                new Tuple<Guid,Guid>(new Guid("{00000000-AAAA-1111-0000-000000000000}"), new Guid("{00000000-AAAA-6666-0000-000000000000}")),
            };

            var expectedSuccess = new
            {
                success = true,
                archivedWarning = false,
                message = "Imported file successfully!!!"
            };

            JsonResult responseSuccess = SubscribersImportCsvTest(logger);

            Assert.IsNotNull(responseSuccess);
            AssertEx.PropertyValuesAreEquals(expectedSuccess, responseSuccess.Data, false);

            resultReplace = context.eHubSubscriptionValues.Where(a => a.SV_ST == new Guid("{00000000-AAAA-3333-0000-000000000000}"))
                .Select(a => new Tuple<Guid, Guid>(a.SV_CC_Sender, a.SV_CC_Recipient)).ToList();

            CollectionAssert.AreEquivalent(expectedReplace, resultReplace);
            Assert.IsTrue(logger.Log.Contains("Info - [edit] eHubSubscriptionValue:"));
            Assert.IsTrue(logger.Log.Contains("SV_ST=00000000-aaaa-3333-0000-000000000000, SV_CC_Sender=00000000-aaaa-1111-0000-000000000000, SV_CC_Recipient=00000000-aaaa-5555-0000-000000000000, SV_Value=FALSE, SV_Reference=, SV_ReferenceType="));
            Assert.IsTrue(logger.Log.Contains("SV_ExpiryUTC="));
            Assert.IsTrue(logger.Log.Contains("Info - [edit] eHubSubscriptionValue:"));
            Assert.IsTrue(logger.Log.Contains("SV_PK=00000000-dddd-3333-1111-000000000000, SV_ST=00000000-aaaa-3333-0000-000000000000, SV_CC_Sender=00000000-aaaa-1111-0000-000000000000, SV_CC_Recipient=00000000-aaaa-6666-0000-000000000000, SV_Value=TRUE, SV_Reference=, SV_ReferenceType="));
            Assert.IsTrue(logger.Log.Contains("SV_ExpiryUTC="));
            Assert.IsTrue(logger.Log.Contains("Info - [del] eHubSubscriptionValue:"));
            Assert.IsTrue(logger.Log.Contains("SV_ST=00000000-aaaa-3333-0000-000000000000, SV_CC_Sender=00000000-aaaa-1111-0000-000000000000, SV_CC_Recipient=00000000-aaaa-8888-0000-000000000000, SV_Value=TRUE, SV_Reference=, SV_ReferenceType="));
            Assert.IsTrue(logger.Log.Contains("SV_ExpiryUTC="));

            request.Clear();
            request.Container["broadcasterPK"] = "{00000000-EEEE-1111-1111-000000000000}";
            request.Container["option"] = "merge";
            request.AddFile("uploadFile", "Subscriber,Archive Outbox\r\nCLIENT0006,True\r\nCLIENT0123,fALsE");
            expectedMerge = new List<Tuple<Guid, Guid>>
            {
                new Tuple<Guid,Guid>(new Guid("{00000000-AAAA-1111-0000-000000000000}"), new Guid("{00000000-AAAA-5555-0000-000000000000}")),
                new Tuple<Guid,Guid>(new Guid("{00000000-AAAA-1111-0000-000000000000}"), new Guid("{00000000-AAAA-6666-0000-000000000000}"))
            };

            var expectedError = new
            {
                success = false,
                message = "File cannot be imported because there exists a row with invalid subscriber ID. First invalid subscriber ID would be 'CLIENT0123'"
            };

            JsonResult responseError = SubscribersImportCsvTest(logger);

            Assert.IsNotNull(responseError);
            AssertEx.PropertyValuesAreEquals(expectedError, responseError.Data, false);

            resultMerge = context.eHubSubscriptionValues.Where(a => a.SV_ST == new Guid("{00000000-AAAA-3333-0000-000000000000}"))
                .Select(a => new Tuple<Guid, Guid>(a.SV_CC_Sender, a.SV_CC_Recipient)).ToList();

            CollectionAssert.AreEquivalent(expectedMerge, resultMerge);
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

            var sbPK = new Guid("{00000000-EEEE-1111-1111-000000000000}");

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
		        new { CC_ID = "CLIENT0008", CC_FriendlyName = "Client 8" },
		    };

			var result1 = controller.Clients(true, sbPK);

			Assert.IsNotNull(result1);
			AssertEx.JsonResultMatchesList(expected1, result1, "eHubClients");

            request.Clear();
            var delId = new Guid("{00000000-DDDD-3333-1111-000000000000}");
            request.Container["SV_PK"] = delId.ToString();
            request.Container["oper"] = "del";

            var logger = new TestLogger();
            var responseDel = SubscriberEditTest(sbPK, logger);

            Assert.IsTrue((bool)responseDel.Data.GetType().GetProperty("success").GetValue(responseDel.Data, null));
            Assert.IsNull(context.eHubSubscriptionValues.FirstOrDefault(a => a.SV_PK == delId));
            Assert.IsTrue(logger.Log.Contains("Info - [del] eHubSubscriptionValue:"));
            Assert.IsTrue(logger.Log.Contains("SV_ST=00000000-aaaa-3333-0000-000000000000, SV_CC_Sender=00000000-aaaa-1111-0000-000000000000, SV_CC_Recipient=00000000-aaaa-6666-0000-000000000000, SV_Value=FALSE, SV_Reference=, SV_ReferenceType="));
            Assert.IsTrue(logger.Log.Contains("SV_ExpiryUTC="));

            request.Clear();
            request.Container["page"] = "1";
            request.Container["rows"] = "8";
            request.Container["sidx"] = "CC_ID";
            request.Container["sord"] = "asc";
            request.Container["_search"] = "false";

            IList<object> expected2 = new List<object>
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

            var result2 = controller.Clients(true, sbPK);

            Assert.IsNotNull(result2);
            AssertEx.JsonResultMatchesList(expected2, result2, "eHubClients");

            request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["sidx"] = "CC_FriendlyName";
			request.Container["sord"] = "desc";
			request.Container["_search"] = "true";
			request.Container["CC_ID"] = "TEST";
			IList<object> expected3 = new List<object>
		    {
		        new { CC_ID = "TEST0003", CC_FriendlyName = "Test Client 3" },
		        new { CC_ID = "TEST0002", CC_FriendlyName = "Test Client 2" },
		        new { CC_ID = "TEST0001", CC_FriendlyName = "Test Client 1" },
		    };

			var result3 = controller.Clients(true, sbPK);

			Assert.IsNotNull(result3);
			AssertEx.JsonResultMatchesList(expected3, result3, "eHubClients");

            request.Clear();
            request.Container["id"] = "_empty";
            request.Container["SV_CC_Provider"] = "TEST0001";
            request.Container["SV_CC_SubscriberID"] = "TEST0003";
            request.Container["oper"] = "add";

            var responseAdd = SubscriberEditTest(sbPK, logger);

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
                SV_SubscribedUTC = a.SV_SubscribedUTC != null,
                a.SV_ExpiryUTC,
            }).First(a => a.SV_PK == newId);
            var expectedAdd = new
            {
                SV_PK = newId,
                SV_CC_Sender = new Guid("{00000000-AAAA-1111-0000-000000000000}"),
                SV_CC_Recipient = new Guid("{00000000-AAAA-3333-0000-000000000000}"),
                SV_Value = "FALSE",
                SV_Reference = (string)null,
                SV_SubscribedUTC = true,
                SV_ExpiryUTC = (DateTime?)null
            };
            AssertEx.PropertyValuesAreEquals(expectedAdd, resultAdd, false);
            Assert.IsTrue(logger.Log.Contains("Info - [add] eHubSubscriptionValue:"));
            Assert.IsTrue(logger.Log.Contains("SV_ST=00000000-aaaa-3333-0000-000000000000, SV_CC_Sender=00000000-aaaa-1111-0000-000000000000, SV_CC_Recipient=00000000-aaaa-3333-0000-000000000000, SV_Value=FALSE, SV_Reference=, SV_ReferenceType="));
            Assert.IsTrue(logger.Log.Contains("SV_ExpiryUTC="));

            request.Clear();
            request.Container["page"] = "1";
            request.Container["rows"] = "10";
            request.Container["sidx"] = "CC_FriendlyName";
            request.Container["sord"] = "desc";
            request.Container["_search"] = "true";
            request.Container["CC_ID"] = "TEST";
            IList<object> expected4 = new List<object>
            {
                new { CC_ID = "TEST0002", CC_FriendlyName = "Test Client 2" },
                new { CC_ID = "TEST0001", CC_FriendlyName = "Test Client 1" },
            };

            var result4 = controller.Clients(true, sbPK);

            Assert.IsNotNull(result4);
            AssertEx.JsonResultMatchesList(expected4, result4, "eHubClients");

            request.Clear();
            request.Container["SV_PK"] = newId.ToString();
            request.Container["SV_CC_Provider"] = "TEST0001";
            request.Container["SV_CC_SubscriberID"] = "TEST0002";
            request.Container["SV_Value"] = "True";
            request.Container["oper"] = "edit";

            var responseEdit = SubscriberEditTest(sbPK, logger);

            Assert.IsTrue((bool)responseEdit.Data.GetType().GetProperty("success").GetValue(responseEdit.Data, null));
            var resultEdit = context.eHubSubscriptionValues.Select(a => new
            {
                a.SV_PK,
                a.SV_CC_Sender,
                a.SV_CC_Recipient,
                a.SV_Value,
                a.SV_Reference,
                SV_SubscribedUTC = a.SV_SubscribedUTC != null,
                a.SV_ExpiryUTC
            }).First(a => a.SV_PK == newId);
            var expectedEdit = new
            {
                SV_PK = newId,
                SV_CC_Sender = new Guid("{00000000-AAAA-1111-0000-000000000000}"),
                SV_CC_Recipient = new Guid("{00000000-AAAA-2222-0000-000000000000}"),
                SV_Value = "TRUE",
                SV_Reference = (string)null,
                SV_SubscribedUTC = true,
                SV_ExpiryUTC = (DateTime?)null
            };
            AssertEx.PropertyValuesAreEquals(expectedEdit, resultEdit, false);
            Assert.IsTrue(logger.Log.Contains("Info - [edit] eHubSubscriptionValue:"));
            Assert.IsTrue(logger.Log.Contains("SV_ST=00000000-aaaa-3333-0000-000000000000, SV_CC_Sender=00000000-aaaa-1111-0000-000000000000, SV_CC_Recipient=00000000-aaaa-2222-0000-000000000000, SV_Value=TRUE, SV_Reference=, SV_ReferenceType="));
            Assert.IsTrue(logger.Log.Contains("SV_ExpiryUTC="));

            request.Clear();
            request.Container["page"] = "1";
            request.Container["rows"] = "10";
            request.Container["sidx"] = "CC_FriendlyName";
            request.Container["sord"] = "desc";
            request.Container["_search"] = "true";
            request.Container["CC_ID"] = "TEST";
            IList<object> expected5 = new List<object>
            {
                new { CC_ID = "TEST0003", CC_FriendlyName = "Test Client 3" },
                new { CC_ID = "TEST0001", CC_FriendlyName = "Test Client 1" },
            };

            var result5 = controller.Clients(true, sbPK);

            Assert.IsNotNull(result5);
            AssertEx.JsonResultMatchesList(expected5, result5, "eHubClients");

            request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["sidx"] = "CC_FriendlyName";
			request.Container["sord"] = "asc";
			request.Container["_search"] = "true";
			request.Container["CC_ID"] = "TEST";
			request.Container["CC_FriendlyName"] = "3";
			IList<object> expected6 = new List<object>
		    {
		        new { CC_ID = "TEST0003", CC_FriendlyName = "Test Client 3" },
		    };

			var result6 = controller.Clients(true, sbPK);

			Assert.IsNotNull(result6);
			AssertEx.JsonResultMatchesList(expected6, result6, "eHubClients");


			request.Clear();
			request.Container["page"] = "2";
			request.Container["rows"] = "2";
			request.Container["sidx"] = "CC_ID";
			request.Container["sord"] = "asc";
			request.Container["_search"] = "false";
			IList<object> expected7 = new List<object>
			{
				new { CC_ID = "CLIENT_MESSAGE_REF_3", CC_FriendlyName = "Client Message Ref 3" },
				new { CC_ID = "CLIENT_MESSAGE_REF_4", CC_FriendlyName = "Client Message Ref 4" },
			};

			var results7 = controller.Clients(true, null);

			Assert.IsNotNull(results7);
			AssertEx.JsonResultMatchesList(expected7, results7, "eHubClients");

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

			var sbPK = new Guid("{00000000-EEEE-1111-1111-000000000000}");

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
				new { CC_ID = "CLIENT0008", CC_FriendlyName = "Client 8" },
				new { CC_ID = "TEST0001", CC_FriendlyName = "Test Client 1" },
				new { CC_ID = "TEST0002", CC_FriendlyName = "Test Client 2" },
				new { CC_ID = "TEST0003", CC_FriendlyName = "Test Client 3" },
				new { CC_ID = "WTLPRDSV1", CC_FriendlyName = "Prod System 1" },
			};

			var result1 = controller.Clients(false, sbPK);

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
				new { CC_ID = "CLIENT0008", CC_FriendlyName = "Client 8" },
				new { CC_ID = "TEST0001", CC_FriendlyName = "Test Client 1" },
				new { CC_ID = "TEST0002", CC_FriendlyName = "Test Client 2" },
				new { CC_ID = "TEST0003", CC_FriendlyName = "Test Client 3" },
				new { CC_ID = "WTLPRDSV1", CC_FriendlyName = "Prod System 1" },
				new { CC_ID = "WTLTSTSV2", CC_FriendlyName = "Test System 2" },
			};

			result1 = controller.Clients(true, sbPK);

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

			var sbPK = new Guid("{00000000-EEEE-1111-1111-000000000000}");

			IList<object> expected1 = new List<object>
			{
				new { CC_ID = "TEST0001", CC_FriendlyName = "Test Client 1" },
				new { CC_ID = "TEST0002", CC_FriendlyName = "Test Client 2" },
				new { CC_ID = "TEST0003", CC_FriendlyName = "Test Client 3" },
			};

			var result1 = controller.Clients(false, sbPK);

			Assert.IsNotNull(result1);
			AssertEx.JsonResultMatchesList(expected1, result1, "eHubClients");
		}

		protected void BroadcasterInfoEditTest(ILog logger)
        {
            controller.logger = logger;
            controller.BroadcasterInfoEdit();
        }

        protected JsonResult SubscriberEditTest(Guid broadcaster, ILog logger)
        {
            controller.logger = logger;
            return controller.SubscriberEdit(broadcaster);
        }

        protected JsonResult SubscribersImportCsvTest(ILog logger)
        {
            controller.logger = logger;
            return controller.SubscribersImportCsv();
        }
    }
}
