using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using CargoWise.eHub.Portal.Controllers;
using CargoWise.eHub.Portal.Helpers;
using CargoWise.eHub.Portal.Models.eHubTransactions;
using CargoWise.eHub.Portal.Models.View.MessageReference;
using CargoWise.eHub.Portal.Tests.TestHelpers;
using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Portal.Tests.Controllers
{
    [TestClass]
    public class MessageReferenceControllerTest : BaseControllerTest<eHubMessageReferenceController>
    {

        [TestMethod]
        public void TestValues_MultipleFilter_AND()
        {
            request.Container["filters"] = "{\"groupOp\":\"AND\",\"rules\":[{\"field\":\"CC_ID\",\"op\":\"bw\",\"data\":\"C\"},{\"field\":\"CC_FriendlyName\",\"op\":\"cn\",\"data\":\"4\"}]}";
            var result = controller.List("CMR");

            Assert.IsNotNull(result);

            var registryList = result.Data.GetType().GetProperty("eHubMessageReferenceRegistry").GetValue(result.Data, null) as IEnumerable<MessageReferenceRegistry>;

            Assert.AreEqual(1, registryList.Count());
        }

        [TestMethod]
        public void TestValues_MultipleFilter_OR()
        {
            request.Container["filters"] = "{\"groupOp\":\"OR\",\"rules\":[{\"field\":\"CC_ID\",\"op\":\"bw\",\"data\":\"C\"},{\"field\":\"CC_FriendlyName\",\"op\":\"cn\",\"data\":\"4\"}]}";

            var result = controller.List("CMR");

            Assert.IsNotNull(result);

            var registryList = result.Data.GetType().GetProperty("eHubMessageReferenceRegistry").GetValue(result.Data, null) as IEnumerable<MessageReferenceRegistry>;

            Assert.AreEqual(5, registryList.Count());
        }

        [TestMethod]
        public void TestList()
        {
            var registries = context.eHubMessageReferenceRegistries.Where(r => r.CR_ApplicationCode.Equals("CMR")).OrderBy(r => r.eHubClient.CC_FriendlyName);

            var result = controller.List("CMR");
            var registryList = result.Data.GetType().GetProperty("eHubMessageReferenceRegistry").GetValue(result.Data, null) as IQueryable<MessageReferenceRegistry>;
            Assert.IsNotNull(registryList);
            Assert.AreEqual(registries.Count(), registryList.Count());

            AssertList(5, 1, 5, 1, 4, "CMR", "CC_ID", "ASC");
            AssertList(3, 1, 3, 1, 3, "CMR", "CC_FriendlyName", "ASC");
            AssertList(3, 2, 3, 2, 1, "CMR", "CC_FriendlyName", "ASC");
            AssertList(5, 3, 5, 1, 4, "CMR");
            AssertList(5, 1, 5, 1, 4, "JPC");
            AssertList(5, 1, 5, 1, 4, "NZC");
        }

        [TestMethod]
        public void TestAdd()
        {
            var logger = new TestLogger();
            var initialCount = context.eHubMessageReferenceRegistries.Count();
            AssertAdd(true, "CLIENT_MESSAGE_REF", "CMR", "REFM2AE2342", "p4ssw0rd", null, logger);
            Assert.IsTrue(logger.Log.Contains("Info - [add] eHubMessageReferenceRegistry:"));
            Assert.IsTrue(logger.Log.Contains("CR_CC_Client=07b420e9-9a5d-437c-b9f3-42b753950e4e, CR_ApplicationCode=CMR, CR_MessageReference=REFM2AE2342, CR_Password=p4ssw0rd"));

            logger = new TestLogger();
            AssertAdd(false, "CLIENT_MESSAGE_REF", "CMR", null, "p4ssw0rd", "Message Reference Required", logger);
            Assert.IsTrue(string.IsNullOrEmpty(logger.Log));

            logger = new TestLogger();
            AssertAdd(false, "CLIENT_MESSAGE_REF", "CMR", "", "p4ssw0rd", "Message Reference Required", logger);
            Assert.IsTrue(string.IsNullOrEmpty(logger.Log));

            logger = new TestLogger();
            AssertException<HttpException>(() => { DoAdd("", "CMR", "REFM2GWTST", "", logger); }, "eHub Client Not Found");
            Assert.IsTrue(string.IsNullOrEmpty(logger.Log));

            logger = new TestLogger();
            AssertException<HttpException>(() => { DoAdd(null, "CMR", "REFM2GWTST", "", logger); }, "eHub Client ID Not Indicated");
            Assert.IsTrue(string.IsNullOrEmpty(logger.Log));
        }

        [TestMethod]
        public void TestEdit()
        {
            var logger = new TestLogger();
            var registry = context.eHubMessageReferenceRegistries.First();
            var client = context.eHubClients.Last();
            AssertException<HttpException>(() => { DoUpdate(null, client.CC_ID, "JPC", "REF34MSGEAU", "", logger); }, "eHub Message Registry ID Not Indicated");
            Assert.IsTrue(string.IsNullOrEmpty(logger.Log));

            logger = new TestLogger();
            AssertException<HttpException>(() => { DoUpdate("{00000000-0000-0000-0000-000000000000}", client.CC_ID, "JPC", "REF34MSGEAU", "", logger); }, "eHub Message Registry Not Found");
            Assert.IsTrue(string.IsNullOrEmpty(logger.Log));

            logger = new TestLogger();
            AssertException<HttpException>(() => { DoUpdate(registry.CR_PK.ToString(), null, "JPC", "REF34MSGEAU", "", logger); }, "eHub Client ID Not Indicated");
            Assert.IsTrue(string.IsNullOrEmpty(logger.Log));

            logger = new TestLogger();
            AssertException<HttpException>(() => { DoUpdate(registry.CR_PK.ToString(), "", "JPC", "REF34MSGEAU", "", logger); }, "eHub Client Not Found");
            Assert.IsTrue(string.IsNullOrEmpty(logger.Log));

            logger = new TestLogger();
            AssertUpdate(registry, false, client.CC_ID, "JPC", null, "", "Message Reference Required", logger);
            Assert.IsTrue(string.IsNullOrEmpty(logger.Log));

            logger = new TestLogger();
            AssertUpdate(registry, false, client.CC_ID, "JPC", "", "", "Message Reference Required", logger);
            Assert.IsTrue(string.IsNullOrEmpty(logger.Log));

            logger = new TestLogger();
            AssertUpdate(registry, true, client.CC_ID, "JPC", "REF34MSGEAU", "", "", logger);
            Assert.IsTrue(logger.Log.Contains("Info - [edit] eHubMessageReferenceRegistry: CR_PK=b6758272-6ee6-4f0d-83c6-0fa8008fc211, CR_CC_Client=8470cd71-99cb-4617-9e7d-fbe72f022ecf, CR_ApplicationCode=JPC, CR_MessageReference=REF34MSGEAU, CR_Password="));

            logger = new TestLogger();
            AssertUpdate(registry, true, client.CC_ID, "JPC", "REF34MSGEAU", "p4ssw0rd", "", logger);
            Assert.IsTrue(logger.Log.Contains("Info - [edit] eHubMessageReferenceRegistry: CR_PK=b6758272-6ee6-4f0d-83c6-0fa8008fc211, CR_CC_Client=8470cd71-99cb-4617-9e7d-fbe72f022ecf, CR_ApplicationCode=JPC, CR_MessageReference=REF34MSGEAU, CR_Password=p4ssw0rd"));
        }

        [TestMethod]
        public void TestDelete()
        {
            var logger = new TestLogger();
            var registry = context.eHubMessageReferenceRegistries.First();
            AssertDelete(registry, logger);
            AssertException<HttpException>(() => { DoDelete(null, "CMR", logger); }, "eHub Message Registry ID Not Indicated");
            Assert.IsTrue(logger.Log.Contains("Info - [del] eHubMessageReferenceRegistry: CR_PK=b6758272-6ee6-4f0d-83c6-0fa8008fc211, CR_CC_Client=07b420e9-9a5d-437c-b9f3-42b753950e4e, CR_ApplicationCode=JPC, CR_MessageReference=JPC_MSG_REF, CR_Password=JPC_MSG_PASS"));

            logger = new TestLogger();
            AssertException<HttpException>(() => { DoDelete("", "CMR", logger); }, "Invalid Registry ID Format");
            Assert.IsTrue(string.IsNullOrEmpty(logger.Log));

            logger = new TestLogger();
            AssertException<HttpException>(() => { DoDelete("{00000000-0000-0000-0000-000000000000}", "CMR", logger); }, "eHub Message Registry Not Found");
            Assert.IsTrue(string.IsNullOrEmpty(logger.Log));
        }

		[TestMethod]
		public void TestClientList()
		{
			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "15";
			request.Container["sidx"] = "CC_ID";
			request.Container["sord"] = "asc";
			request.Container["_search"] = "true";
			request.Container["CC_ID"] = "TEST";

			var json = (JsonResult)controller.ClientList(true);

			var serializer = new JavaScriptSerializer();
			var result = serializer.Serialize(json.Data);
			Assert.AreEqual("{\"page\":1,\"total\":1,\"records\":3,\"eHubClients\":[" +
							"{\"CC_ID\":\"TEST0001\",\"CC_FriendlyName\":\"Test Client 1\"}," +
							"{\"CC_ID\":\"TEST0002\",\"CC_FriendlyName\":\"Test Client 2\"}," +
							"{\"CC_ID\":\"TEST0003\",\"CC_FriendlyName\":\"Test Client 3\"}]}", result);
		}

		[TestMethod]
		public void TestClientListIncludeNonProdCWSystems()
		{
			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "15";
			request.Container["sidx"] = "CC_ID";
			request.Container["sord"] = "asc";
			request.Container["_search"] = "true";
			request.Container["CC_ID"] = "WTL";

			var json = (JsonResult)controller.ClientList(true);

			var serializer = new JavaScriptSerializer();
			var result = serializer.Serialize(json.Data);
			Assert.AreEqual("{\"page\":1,\"total\":1,\"records\":2,\"eHubClients\":[" +
							"{\"CC_ID\":\"WTLPRDSV1\",\"CC_FriendlyName\":\"Prod System 1\"}," +
							"{\"CC_ID\":\"WTLTSTSV2\",\"CC_FriendlyName\":\"Test System 2\"}]}", result);
		}


		[TestMethod]
		public void TestClientListExcludeNonProdCWSystems()
		{
			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "15";
			request.Container["sidx"] = "CC_ID";
			request.Container["sord"] = "asc";
			request.Container["_search"] = "true";
			request.Container["CC_ID"] = "WTL";

			var json = (JsonResult)controller.ClientList(false);

			var serializer = new JavaScriptSerializer();
			var result = serializer.Serialize(json.Data);
			Assert.AreEqual("{\"page\":1,\"total\":1,\"records\":1,\"eHubClients\":[" +
							"{\"CC_ID\":\"WTLPRDSV1\",\"CC_FriendlyName\":\"Prod System 1\"}]}", result);
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

			var result1 = (JsonResult)controller.ClientList(false);

			Assert.IsNotNull(result1);
			AssertEx.JsonResultMatchesList(expected1, result1, "eHubClients");
		}

		private void AssertException<T>(Action action, string message = null) where T : Exception
        {
            try
            {
                action();
                Assert.Fail("Expected to throw an exception.");
            }
            catch (T e)
            {
                if (message != null) Assert.AreEqual(message, e.Message);
            }
            catch (Exception e)
            {
                Assert.Fail(string.Format("Expected {0} but was {1}", typeof(T).ToString(), e.GetType().ToString()));
            }

        }

        private JsonResult DoEdit(string CC_ID, string CR_ApplicationCode, string CR_MessageReference, string id, string operation, string CR_Password, ILog logger)
        {
            request.Clear();
            request.Container["CC_ID"] = CC_ID;
            request.Container["CR_MessageReference"] = CR_MessageReference;
            request.Container["CR_Password"] = CR_Password;
            request.Container["oper"] = operation;
            request.Container["id"] = id;
            return EditTest(CR_ApplicationCode, logger);
        }

        private T GetProperty<T>(object o, string property)
        {
            PropertyInfo propertyInfo = null;
            Assert.IsNotNull((propertyInfo = o.GetType().GetProperty(property)));
            return (T) propertyInfo.GetValue(o, null);
        }

        #region TestList Helpers

        private void AssertList(int rowCount, int pageNumber, int expectedRowCount, int expectedPageNumber, int dataCount, string applicationCode, string sidx = "CC_ID", string sord = "ASC", string searchField = null, string searchString = null)
        {
            SetListRequestParameters(rowCount, pageNumber, sidx, sord, searchField, searchString);
            var orderFormat = sidx.StartsWith("CC") ? "eHubClient.{0} {1}" : "{0} {1}";
            var registries = context.eHubMessageReferenceRegistries.Where(r => r.CR_ApplicationCode.Equals(applicationCode)).OrderBy(string.Format(orderFormat, sidx, sord));

            if (searchField != null && searchString != null)
            {
                var searchFormat = searchField.StartsWith("CC") ? "eHubClient.{0}.Contains(\"{1}\")" : "{0}.Contains(\"{1}\")";
                registries = registries.Where(string.Format(searchFormat, searchField, searchString));
            }

            var result = controller.List(applicationCode);
            var registryList = GetProperty<IQueryable<MessageReferenceRegistry>>(result.Data, "eHubMessageReferenceRegistry");
            var page = GetProperty<int>(result.Data, "page");
            var total = GetProperty<int>(result.Data, "total");
            var records = GetProperty<int>(result.Data, "records");

            AssertPagination(expectedRowCount, expectedPageNumber, dataCount, registries, registryList, page, total, records);
            AssertResult(pageNumber, expectedRowCount, registries, registryList);
        }

        private static void AssertResult(int pageNumber, int expectedRowCount, IQueryable<Models.eHubTransactions.eHubMessageReferenceRegistry> registries, IQueryable<MessageReferenceRegistry> registryList)
        {
            foreach (var registry in registries.Skip((pageNumber - 1) * expectedRowCount).Take(expectedRowCount))
            {
                Assert.IsTrue(registryList.Any(r => r.CR_PK.Equals(registry.CR_PK) && r.CR_ApplicationCode.Equals(registry.CR_ApplicationCode) &&
                    r.CR_MessageReference.Equals(registry.CR_MessageReference) && r.CR_Password.Equals(registry.CR_Password) &&
                    r.CC_ID.Equals(registry.eHubClient.CC_ID) && r.CC_FriendlyName.Equals(registry.eHubClient.CC_FriendlyName)),
                    string.Format("Registry {0} not exist.", registry.CR_PK));
            }
        }

        private static void AssertPagination(int expectedRowCount, int expectedPageNumber, int dataCount, IQueryable<Models.eHubTransactions.eHubMessageReferenceRegistry> registries, IQueryable<MessageReferenceRegistry> registryList, int? page, int? total, int? records)
        {
            Assert.IsNotNull(page);
            Assert.AreEqual(expectedPageNumber, page);
            Assert.IsNotNull(total);
            Assert.AreEqual(total, Convert.ToInt32(Math.Ceiling((double)registries.Count() / (double)expectedRowCount)));
            Assert.IsNotNull(records);
            Assert.AreEqual(registries.Count(), records);
            Assert.IsNotNull(registryList);
            Assert.AreEqual(dataCount, registryList.Count());
        }

        private void SetListRequestParameters(int rowCount, int pageNumber, string sidx, string sord, string searchField, string searchString)
        {
            request.Clear();
            request.Container["page"] = pageNumber.ToString();
            request.Container["rows"] = rowCount.ToString();
            request.Container["sidx"] = sidx;
            request.Container["sord"] = sord;
            request.Container["searchField"] = searchField;
            request.Container["searchString"] = searchString;
        }

        #endregion

        #region TestAdd Helpers

        private void AssertAdd(bool expectedResult, string CC_ID, string CR_ApplicationCode, string CR_MessageReference, string CR_Password, string expectedMessage, ILog logger)
        {
            var initialCount = context.eHubMessageReferenceRegistries.Count();
            var response = DoAdd(CC_ID, CR_ApplicationCode, CR_MessageReference, CR_Password, logger);
            Assert.IsNotNull(response);
            var success = GetProperty<bool>(response.Data, "success");
            Assert.AreEqual(expectedResult, success);

            if (success)
            {
                var newGuid = GetProperty<Guid>(response.Data, "newid");
                var newRegistry = context.eHubMessageReferenceRegistries.Where(r => r.CR_PK == newGuid).First();

                Assert.IsNotNull(newRegistry);
                Assert.AreEqual(newGuid, newRegistry.CR_PK);
                Assert.AreEqual(CC_ID, newRegistry.eHubClient.CC_ID);
                Assert.AreEqual(CR_ApplicationCode, newRegistry.CR_ApplicationCode);
                Assert.AreEqual(CR_MessageReference, newRegistry.CR_MessageReference);
                Assert.AreEqual(CR_Password, newRegistry.CR_Password ?? string.Empty);
                Assert.AreEqual(initialCount + 1, context.eHubMessageReferenceRegistries.Count());
            }
            else
            {
                string message = GetProperty<string>(response.Data, "message");
                Assert.AreEqual(expectedMessage, message);
            }
        }

        private JsonResult DoAdd(string CC_ID, string CR_ApplicationCode, string CR_MessageReference, string CR_Password, ILog logger)
        {
            return DoEdit(CC_ID, CR_ApplicationCode, CR_MessageReference, "_empty", eHubMessageReferenceController.Operation.Add, CR_Password, logger);
        }

        #endregion

        #region TestEdit Helpers

        private void AssertUpdate(eHubMessageReferenceRegistry registry, bool expectedResult, string CC_ID, string CR_ApplicationCode, string CR_MessageReference, string CR_Password, string expectedMessage, ILog logger)
        {
            var initialCount = context.eHubMessageReferenceRegistries.Count();
            var response = DoUpdate(registry.CR_PK.ToString(), CC_ID, CR_ApplicationCode, CR_MessageReference, CR_Password, logger);
            Assert.IsNotNull(response);
            var success = GetProperty<bool>(response.Data, "success");
            Assert.AreEqual(expectedResult, success);

            if (success)
            {
                var newGuid = GetProperty<Guid>(response.Data, "newid");
                Assert.AreEqual(registry.CR_PK, newGuid);
                Assert.IsNotNull(registry);
                Assert.AreEqual(newGuid, registry.CR_PK);
                Assert.AreEqual(CC_ID, registry.eHubClient.CC_ID);
                Assert.AreEqual(CR_ApplicationCode, registry.CR_ApplicationCode);
                Assert.AreEqual(CR_MessageReference, registry.CR_MessageReference);
                Assert.AreEqual(CR_Password, registry.CR_Password ?? string.Empty);
                Assert.AreEqual(initialCount, context.eHubMessageReferenceRegistries.Count());
            }
            else
            {
                string message = GetProperty<string>(response.Data, "message");
                Assert.AreEqual(expectedMessage, message);
            }
        }

        private JsonResult DoUpdate(string id, string CC_ID, string CR_ApplicationCode, string CR_MessageReference, string CR_Password, ILog logger)
        {
            return DoEdit(CC_ID, CR_ApplicationCode, CR_MessageReference, id, eHubMessageReferenceController.Operation.Edit, CR_Password, logger);
        }

        #endregion

        #region TestDelete Helpers

        private void AssertDelete(eHubMessageReferenceRegistry registry, ILog logger)
        {
            var guid = registry.CR_PK;
            var initialCount = context.eHubMessageReferenceRegistries.Count();
            var response = DoDelete(registry.CR_PK.ToString(), registry.CR_ApplicationCode, logger);
            var success = GetProperty<bool>(response.Data, "success");
            Assert.IsTrue(success);
            var newid = GetProperty<Guid>(response.Data, "newid");
            Assert.IsNotNull(newid);
            Assert.AreEqual(guid, newid);
            Assert.AreEqual(initialCount - 1, context.eHubMessageReferenceRegistries.Count());
        }

        private JsonResult DoDelete(string id, string applicationCode, ILog logger)
        {
            request.Clear();
            request.Container["oper"] = eHubMessageReferenceController.Operation.Delete;
            request.Container["id"] = id;
            return EditTest("", logger);
        }

        protected JsonResult EditTest(string applicationCode, ILog logger)
        {
            controller.logger = logger;
            return controller.Edit(applicationCode);
        }

        #endregion
    }
}
