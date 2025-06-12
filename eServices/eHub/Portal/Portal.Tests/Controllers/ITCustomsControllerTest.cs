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
	public class ITCustomsControllerTest : BaseControllerTest<ITCustomsController>
	{
		void GenerateDefaultValues(Fakes.TestContext context)
		{
			// eHubClient
			var client1 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-1111-000000000000}"), CC_ID = "Client1" };
			var client2 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-2222-000000000000}"), CC_ID = "Client2" };
			var client3 = new eHubClient { CC_PK = new Guid("{00000000-CCCC-1111-3333-000000000000}"), CC_ID = "Client3" };

			// eHubClientSystem
			var clientSystem1 = new eHubClientSystem { EH_PK = new Guid("{00000000-FFFF-1111-1111-000000000000}"), EH_ID = "ClientSystem1" };
			var clientSystem2 = new eHubClientSystem { EH_PK = new Guid("{00000000-FFFF-1111-2222-000000000000}"), EH_ID = "ClientSystem2" };
			var clientSystem3 = new eHubClientSystem { EH_PK = new Guid("{00000000-FFFF-1111-3333-000000000000}"), EH_ID = "ClientSystem3" };

			// eHubITCustomsJobStatus
			var jobStatus1 = new eHubITCustomsJobStatu
			{
				eHubClient = client1,
				eHubClientSystem = clientSystem1,
				IT_PK = new Guid("{00000000-1111-1111-1111-000000000000}"),
				IT_ProdInd = false,
				IT_FileName = "FileName1",
				IT_JobID = "JobID1",
				IT_PollingStartUTC = new DateTime(2017, 01, 01, 01, 01, 01).ToUniversalTime(),
				IT_FileLastModifiedUTC = new DateTime(2017, 01, 01, 01, 02, 01).ToUniversalTime(),
				IT_LastStatus = "AAA",
				IT_ReferenceID = "ReferenceID1",
				IT_MessageType = "MessageType1",
				IT_MessageTrackingID = new Guid("{00000000-1111-1111-9999-000000000000}"),
				IT_NotifiedInvalidProfile = true,
				IT_Version = new byte[001]
			};

			var jobStatus2 = new eHubITCustomsJobStatu
			{
				eHubClient = client2,
				eHubClientSystem = clientSystem2,
				IT_PK = new Guid("{00000000-1111-1111-2222-000000000000}"),
				IT_ProdInd = false,
				IT_FileName = "FileName2",
				IT_JobID = "JobID2",
				IT_PollingStartUTC = new DateTime(2017, 01, 01, 01, 01, 02).ToUniversalTime(),
				IT_FileLastModifiedUTC = new DateTime(2017, 01, 01, 01, 02, 02).ToUniversalTime(),
				IT_LastStatus = "BBB",
				IT_ReferenceID = "ReferenceID2",
				IT_MessageType = "",
				IT_MessageTrackingID = new Guid("{00000000-1111-1111-8888-000000000000}"),
				IT_NotifiedInvalidProfile = true,
				IT_Version = new byte[002]
			};

			var jobStatus3 = new eHubITCustomsJobStatu
			{
				eHubClient = client3,
				eHubClientSystem = clientSystem3,
				IT_PK = new Guid("{00000000-1111-1111-3333-000000000000}"),
				IT_ProdInd = false,
				IT_FileName = "FileName3",
				IT_JobID = "JobID3",
				IT_PollingStartUTC = new DateTime(2017, 01, 01, 01, 01, 03).ToUniversalTime(),
				IT_FileLastModifiedUTC = new DateTime(2017, 01, 01, 01, 02, 03).ToUniversalTime(),
				IT_LastStatus = "CCC",
				IT_ReferenceID = "ReferenceID3",
				IT_MessageType = "MessageType3",
				IT_MessageTrackingID = new Guid("{00000000-1111-1111-7777-000000000000}"),
				IT_NotifiedInvalidProfile = true,
				IT_Version = new byte[003]
			};

			var jobStatus4 = new eHubITCustomsJobStatu
			{
				eHubClient = client2,
				eHubClientSystem = clientSystem2,
				IT_PK = new Guid("{00000000-1111-1111-4444-000000000000}"),
				IT_ProdInd = false,
				IT_FileName = "FileName2",
				IT_JobID = "JobID2",
				IT_PollingStartUTC = new DateTime(2017, 01, 01, 01, 01, 04).ToUniversalTime(),
				IT_FileLastModifiedUTC = new DateTime(2017, 01, 01, 01, 02, 04).ToUniversalTime(),
				IT_LastStatus = "BBB",
				IT_ReferenceID = "ReferenceID2",
				IT_MessageType = "",
				IT_MessageTrackingID = new Guid("{00000000-1111-1111-6666-000000000000}"),
				IT_NotifiedInvalidProfile = true,
				IT_Version = new byte[004]
			};

			context.eHubClients.AddObject(client1);
			context.eHubClients.AddObject(client2);
			context.eHubClients.AddObject(client3);

			context.eHubClientSystems.AddObject(clientSystem1);
			context.eHubClientSystems.AddObject(clientSystem2);
			context.eHubClientSystems.AddObject(clientSystem3);

			context.eHubITCustomsJobStatus.AddObject(jobStatus1);
			context.eHubITCustomsJobStatus.AddObject(jobStatus2);
			context.eHubITCustomsJobStatus.AddObject(jobStatus3);
			context.eHubITCustomsJobStatus.AddObject(jobStatus4);
		}

		void GenerateRequest()
		{
			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "3";
		}

		CargoWise.eHub.Portal.Tests.Fakes.TestContext GenerateContext()
		{
			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			controller.Context = context;

			return context;
		}

		[TestMethod]
		public void TestIndex()
		{
			var result = controller.Index() as ViewResult;
			Assert.IsNotNull(result);
			Assert.AreEqual(string.Empty, result.ViewName, "Should be empty (Index)");
		}

		[TestMethod]
		public void TestJobStatus_Add4JobStatus_JsonResultPropertiesMatch()
		{
			GenerateRequest();

			var context = GenerateContext();
			GenerateDefaultValues(context);

			var expectedResult = controller.JobStatus();

			AssertEx.JsonResultMatchesList(new List<object> {
					new {SenderID = "Client1", ClientSystemID = "ClientSystem1", ProdInd = false, FileName = "FileName1", JobID = "JobID1",  PollingStart = new DateTime(2017, 01, 01, 01, 01, 01), FileLastModified = new DateTime(2017, 01, 01, 01, 02, 01), LastStatus = "AAA", ReferenceID = "ReferenceID1", MessageType="MessageType1", MessageTrackingID = new Guid("{00000000-1111-1111-9999-000000000000}"), NotifiedInvalidProfile = true },
					new {SenderID = "Client2", ClientSystemID = "ClientSystem2", ProdInd = false, FileName = "FileName2", JobID = "JobID2",  PollingStart = new DateTime(2017, 01, 01, 01, 01, 02), FileLastModified = new DateTime(2017, 01, 01, 01, 02, 02), LastStatus = "BBB", ReferenceID = "ReferenceID2", MessageType="", MessageTrackingID = new Guid("{00000000-1111-1111-8888-000000000000}"), NotifiedInvalidProfile = true },
					new {SenderID = "Client3", ClientSystemID = "ClientSystem3", ProdInd = false, FileName = "FileName3", JobID = "JobID3",  PollingStart = new DateTime(2017, 01, 01, 01, 01, 03), FileLastModified = new DateTime(2017, 01, 01, 01, 02, 03), LastStatus = "CCC", ReferenceID = "ReferenceID3", MessageType="MessageType3", MessageTrackingID = new Guid("{00000000-1111-1111-7777-000000000000}"), NotifiedInvalidProfile = true },
					new {SenderID = "Client2", ClientSystemID = "ClientSystem2", ProdInd = false, FileName = "FileName2", JobID = "JobID2",  PollingStart = new DateTime(2017, 01, 01, 01, 01, 04), FileLastModified = new DateTime(2017, 01, 01, 01, 02, 04), LastStatus = "BBB", ReferenceID = "ReferenceID2", MessageType="", MessageTrackingID = new Guid("{00000000-1111-1111-6666-000000000000}"), NotifiedInvalidProfile = true },
			}, expectedResult, "eHubITCustomsJobStatus");
		}

		[TestMethod]
		public void TestJobStatus_SortBySenderIDDesc_OrderMatch()
		{
			GenerateRequest();
			request.Container["sidx"] = "SenderID";
			request.Container["sord"] = "desc";

			var context = GenerateContext();
			GenerateDefaultValues(context);

			var expectedResult = controller.JobStatus();

			AssertEx.JsonResultMatchesList(new List<object> {
					new {SenderID = "Client3", ClientSystemID = "ClientSystem3", ProdInd = false, FileName = "FileName3", JobID = "JobID3",  PollingStart = new DateTime(2017, 01, 01, 01, 01, 03), FileLastModified = new DateTime(2017, 01, 01, 01, 02, 03), LastStatus = "CCC", ReferenceID = "ReferenceID3", MessageType="MessageType3",  MessageTrackingID = new Guid("{00000000-1111-1111-7777-000000000000}"), NotifiedInvalidProfile = true },
					new {SenderID = "Client2", ClientSystemID = "ClientSystem2", ProdInd = false, FileName = "FileName2", JobID = "JobID2",  PollingStart = new DateTime(2017, 01, 01, 01, 01, 02), FileLastModified = new DateTime(2017, 01, 01, 01, 02, 02), LastStatus = "BBB", ReferenceID = "ReferenceID2", MessageType="",  MessageTrackingID = new Guid("{00000000-1111-1111-8888-000000000000}"), NotifiedInvalidProfile = true },
					new {SenderID = "Client2", ClientSystemID = "ClientSystem2", ProdInd = false, FileName = "FileName2", JobID = "JobID2",  PollingStart = new DateTime(2017, 01, 01, 01, 01, 04), FileLastModified = new DateTime(2017, 01, 01, 01, 02, 04), LastStatus = "BBB", ReferenceID = "ReferenceID2", MessageType="", MessageTrackingID = new Guid("{00000000-1111-1111-6666-000000000000}"), NotifiedInvalidProfile = true },
					new {SenderID = "Client1", ClientSystemID = "ClientSystem1", ProdInd = false, FileName = "FileName1", JobID = "JobID1",  PollingStart = new DateTime(2017, 01, 01, 01, 01, 01), FileLastModified = new DateTime(2017, 01, 01, 01, 02, 01), LastStatus = "AAA", ReferenceID = "ReferenceID1", MessageType="MessageType1", MessageTrackingID = new Guid("{00000000-1111-1111-9999-000000000000}"), NotifiedInvalidProfile = true },
			}, expectedResult, "eHubITCustomsJobStatus");
		}

		[TestMethod]
		public void TestJobStatus_SortByPollingStartUTCAsc_OrderMatch()
		{
			GenerateRequest();
			request.Container["sidx"] = "PollingStartUTC";
			request.Container["sord"] = "asc";

			var context = GenerateContext();
			GenerateDefaultValues(context);

			var expectedResult = controller.JobStatus();

			AssertEx.JsonResultMatchesList(new List<object> {
					new {SenderID = "Client1", ClientSystemID = "ClientSystem1", ProdInd = false, FileName = "FileName1", JobID = "JobID1",  PollingStart = new DateTime(2017, 01, 01, 01, 01, 01), FileLastModified = new DateTime(2017, 01, 01, 01, 02, 01), LastStatus = "AAA", ReferenceID = "ReferenceID1", MessageType="MessageType1", MessageTrackingID = new Guid("{00000000-1111-1111-9999-000000000000}"), NotifiedInvalidProfile = true },
					new {SenderID = "Client2", ClientSystemID = "ClientSystem2", ProdInd = false, FileName = "FileName2", JobID = "JobID2",  PollingStart = new DateTime(2017, 01, 01, 01, 01, 02), FileLastModified = new DateTime(2017, 01, 01, 01, 02, 02), LastStatus = "BBB", ReferenceID = "ReferenceID2", MessageType="", MessageTrackingID = new Guid("{00000000-1111-1111-8888-000000000000}"), NotifiedInvalidProfile = true },
					new {SenderID = "Client3", ClientSystemID = "ClientSystem3", ProdInd = false, FileName = "FileName3", JobID = "JobID3",  PollingStart = new DateTime(2017, 01, 01, 01, 01, 03), FileLastModified = new DateTime(2017, 01, 01, 01, 02, 03), LastStatus = "CCC", ReferenceID = "ReferenceID3", MessageType="MessageType3", MessageTrackingID = new Guid("{00000000-1111-1111-7777-000000000000}"), NotifiedInvalidProfile = true },
					new {SenderID = "Client2", ClientSystemID = "ClientSystem2", ProdInd = false, FileName = "FileName2", JobID = "JobID2",  PollingStart = new DateTime(2017, 01, 01, 01, 01, 04), FileLastModified = new DateTime(2017, 01, 01, 01, 02, 04), LastStatus = "BBB", ReferenceID = "ReferenceID2", MessageType="", MessageTrackingID = new Guid("{00000000-1111-1111-6666-000000000000}"), NotifiedInvalidProfile = true },
			}, expectedResult, "eHubITCustomsJobStatus");
		}

		[TestMethod]
		public void TestJobStatus_SearchBySenderID_GetMatchingRows()
		{
			GenerateRequest();
			request.Container["filters"] = "{\"groupOp\":\"AND\",\"rules\":[{ \"field\":\"SenderID\",\"op\":\"eq\",\"data\":\"Client2\"}]}";

			var context = GenerateContext();
			GenerateDefaultValues(context);

			var expectedResult = controller.JobStatus();

			AssertEx.JsonResultMatchesList(new List<object> {
					new {SenderID = "Client2", ClientSystemID = "ClientSystem2", ProdInd = false, FileName = "FileName2", JobID = "JobID2",  PollingStart = new DateTime(2017, 01, 01, 01, 01, 02), FileLastModified = new DateTime(2017, 01, 01, 01, 02, 02), LastStatus = "BBB", ReferenceID = "ReferenceID2", MessageType="", MessageTrackingID = new Guid("{00000000-1111-1111-8888-000000000000}"), NotifiedInvalidProfile = true },
					new {SenderID = "Client2", ClientSystemID = "ClientSystem2", ProdInd = false, FileName = "FileName2", JobID = "JobID2",  PollingStart = new DateTime(2017, 01, 01, 01, 01, 04), FileLastModified = new DateTime(2017, 01, 01, 01, 02, 04), LastStatus = "BBB", ReferenceID = "ReferenceID2", MessageType="", MessageTrackingID = new Guid("{00000000-1111-1111-6666-000000000000}"), NotifiedInvalidProfile = true },
			}, expectedResult, "eHubITCustomsJobStatus");
		}

		[TestMethod]
		public void TestJobStatus_SearchByClientSystemID_GetMatchingRow()
		{
			GenerateRequest();
			request.Container["filters"] = "{\"groupOp\":\"AND\",\"rules\":[{ \"field\":\"ClientSystemID\",\"op\":\"ew\",\"data\":\"3\"}]}";

			var context = GenerateContext();
			GenerateDefaultValues(context);

			var expectedResult = controller.JobStatus();

			AssertEx.JsonResultMatchesList(new List<object> {
					new {SenderID = "Client3", ClientSystemID = "ClientSystem3", ProdInd = false, FileName = "FileName3", JobID = "JobID3",  PollingStart = new DateTime(2017, 01, 01, 01, 01, 03), FileLastModified = new DateTime(2017, 01, 01, 01, 02, 03), LastStatus = "CCC", ReferenceID = "ReferenceID3", MessageType="MessageType3", MessageTrackingID = new Guid("{00000000-1111-1111-7777-000000000000}"), NotifiedInvalidProfile = true },
			}, expectedResult, "eHubITCustomsJobStatus");
		}

		[TestMethod]
		public void TestJobStatus_SearchByFileName_GetMatchingRows()
		{
			GenerateRequest();
			request.Container["filters"] = "{\"groupOp\":\"AND\",\"rules\":[{ \"field\":\"FileName\",\"op\":\"cn\",\"data\":\"FileName2\"}]}";

			var context = GenerateContext();
			GenerateDefaultValues(context);

			var expectedResult = controller.JobStatus();

			AssertEx.JsonResultMatchesList(new List<object> {
					new {SenderID = "Client2", ClientSystemID = "ClientSystem2", ProdInd = false, FileName = "FileName2", JobID = "JobID2",  PollingStart = new DateTime(2017, 01, 01, 01, 01, 02), FileLastModified = new DateTime(2017, 01, 01, 01, 02, 02), LastStatus = "BBB", ReferenceID = "ReferenceID2", MessageType="", MessageTrackingID = new Guid("{00000000-1111-1111-8888-000000000000}"), NotifiedInvalidProfile = true },
					new {SenderID = "Client2", ClientSystemID = "ClientSystem2", ProdInd = false, FileName = "FileName2", JobID = "JobID2",  PollingStart = new DateTime(2017, 01, 01, 01, 01, 04), FileLastModified = new DateTime(2017, 01, 01, 01, 02, 04), LastStatus = "BBB", ReferenceID = "ReferenceID2", MessageType="", MessageTrackingID = new Guid("{00000000-1111-1111-6666-000000000000}"), NotifiedInvalidProfile = true },
			}, expectedResult, "eHubITCustomsJobStatus");
		}

		[TestMethod]
		public void TestJobStatus_SearchByJobID_GetMatchingRows()
		{
			GenerateRequest();
			request.Container["filters"] = "{\"groupOp\":\"AND\",\"rules\":[{ \"field\":\"JobID\",\"op\":\"eq\",\"data\":\"JobID2\"}]}";

			var context = GenerateContext();
			GenerateDefaultValues(context);

			var expectedResult = controller.JobStatus();

			AssertEx.JsonResultMatchesList(new List<object> {
					new {SenderID = "Client2", ClientSystemID = "ClientSystem2", ProdInd = false, FileName = "FileName2", JobID = "JobID2",  PollingStart = new DateTime(2017, 01, 01, 01, 01, 02), FileLastModified = new DateTime(2017, 01, 01, 01, 02, 02), LastStatus = "BBB", ReferenceID = "ReferenceID2", MessageType="", MessageTrackingID = new Guid("{00000000-1111-1111-8888-000000000000}"), NotifiedInvalidProfile = true },
					new {SenderID = "Client2", ClientSystemID = "ClientSystem2", ProdInd = false, FileName = "FileName2", JobID = "JobID2",  PollingStart = new DateTime(2017, 01, 01, 01, 01, 04), FileLastModified = new DateTime(2017, 01, 01, 01, 02, 04), LastStatus = "BBB", ReferenceID = "ReferenceID2", MessageType="", MessageTrackingID = new Guid("{00000000-1111-1111-6666-000000000000}"), NotifiedInvalidProfile = true },
			}, expectedResult, "eHubITCustomsJobStatus");
		}

		[TestMethod]
		public void TestJobStatus_SearchByLastStatus_GetMatchingRow()
		{
			GenerateRequest();
			request.Container["filters"] = "{\"groupOp\":\"AND\",\"rules\":[{ \"field\":\"LastStatus\",\"op\":\"bw\",\"data\":\"A\"}]}";

			var context = GenerateContext();
			GenerateDefaultValues(context);

			var expectedResult = controller.JobStatus();

			AssertEx.JsonResultMatchesList(new List<object> {
					new {SenderID = "Client1", ClientSystemID = "ClientSystem1", ProdInd = false, FileName = "FileName1", JobID = "JobID1",  PollingStart = new DateTime(2017, 01, 01, 01, 01, 01), FileLastModified = new DateTime(2017, 01, 01, 01, 02, 01), LastStatus = "AAA", ReferenceID = "ReferenceID1", MessageType="MessageType1", MessageTrackingID = new Guid("{00000000-1111-1111-9999-000000000000}"), NotifiedInvalidProfile = true },
			}, expectedResult, "eHubITCustomsJobStatus");
		}

		[TestMethod]
		public void TestJobStatus_SearchByReferenceID_GetMatchingRow()
		{
			GenerateRequest();
			request.Container["filters"] = "{\"groupOp\":\"AND\",\"rules\":[{ \"field\":\"ReferenceID\",\"op\":\"cn\",\"data\":\"ReferenceID1\"}]}";

			var context = GenerateContext();
			GenerateDefaultValues(context);

			var expectedResult = controller.JobStatus();

			AssertEx.JsonResultMatchesList(new List<object> {
					new {SenderID = "Client1", ClientSystemID = "ClientSystem1", ProdInd = false, FileName = "FileName1", JobID = "JobID1",  PollingStart = new DateTime(2017, 01, 01, 01, 01, 01), FileLastModified = new DateTime(2017, 01, 01, 01, 02, 01), LastStatus = "AAA", ReferenceID = "ReferenceID1", MessageType="MessageType1", MessageTrackingID = new Guid("{00000000-1111-1111-9999-000000000000}"), NotifiedInvalidProfile = true },
			}, expectedResult, "eHubITCustomsJobStatus");
		}

		[TestMethod]
		public void TestJobStatus_SearchByMessageTrackingID_GetMatchingRow()
		{
			GenerateRequest();
			request.Container["filters"] = "{\"groupOp\":\"AND\",\"rules\":[{ \"field\":\"MessageTrackingID\",\"op\":\"eq\",\"data\":\"00000000-1111-1111-9999-000000000000\"}]}";

			var context = GenerateContext();
			GenerateDefaultValues(context);

			var expectedResult = controller.JobStatus();

			AssertEx.JsonResultMatchesList(new List<object> {
					new {SenderID = "Client1", ClientSystemID = "ClientSystem1", ProdInd = false, FileName = "FileName1", JobID = "JobID1",  PollingStart = new DateTime(2017, 01, 01, 01, 01, 01), FileLastModified = new DateTime(2017, 01, 01, 01, 02, 01), LastStatus = "AAA", ReferenceID = "ReferenceID1", MessageType="MessageType1", MessageTrackingID = new Guid("{00000000-1111-1111-9999-000000000000}"), NotifiedInvalidProfile = true },
			}, expectedResult, "eHubITCustomsJobStatus");
		}

		[TestMethod]
		public void TestJobStatus_SortByPollingStartUTCAscSearchByJobID_GetMatchingRowsInOrder()
		{
			GenerateRequest();
			request.Container["sidx"] = "PollingStartUTC";
			request.Container["sord"] = "desc";
			request.Container["filters"] = "{\"groupOp\":\"AND\",\"rules\":[{ \"field\":\"JobID\",\"op\":\"eq\",\"data\":\"JobID2\"}]}";

			var context = GenerateContext();
			GenerateDefaultValues(context);

			var expectedResult = controller.JobStatus();

			AssertEx.JsonResultMatchesList(new List<object> {
				new {SenderID = "Client2", ClientSystemID = "ClientSystem2", ProdInd = false, FileName = "FileName2", JobID = "JobID2",  PollingStart = new DateTime(2017, 01, 01, 01, 01, 02), FileLastModified = new DateTime(2017, 01, 01, 01, 02, 02), LastStatus = "BBB", ReferenceID = "ReferenceID2", MessageType="", MessageTrackingID = new Guid("{00000000-1111-1111-8888-000000000000}"), NotifiedInvalidProfile = true },
				new {SenderID = "Client2", ClientSystemID = "ClientSystem2", ProdInd = false, FileName = "FileName2", JobID = "JobID2",  PollingStart = new DateTime(2017, 01, 01, 01, 01, 04), FileLastModified = new DateTime(2017, 01, 01, 01, 02, 04), LastStatus = "BBB", ReferenceID = "ReferenceID2", MessageType="", MessageTrackingID = new Guid("{00000000-1111-1111-6666-000000000000}"), NotifiedInvalidProfile = true },
			}, expectedResult, "eHubITCustomsJobStatus");
		}

		[TestMethod]
		public void AddOrUpdateOrDeleteOnJobStatus_Success()
		{
			var logger = new TestLogger();
			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			controller.Context = context;
			GenerateDefaultValues(context);

			var eHubITCustomsJobStatuResult = context.eHubITCustomsJobStatus.Select(ck => new Tuple<Guid, Guid, Guid, string, Guid>(ck.IT_PK, ck.IT_CC_Sender, ck.IT_EH_ClientSystem, ck.IT_LastStatus, ck.IT_MessageTrackingID)).ToArray();
			CollectionAssert.AreEqual(new[] {
					new Tuple<Guid, Guid, Guid, string, Guid>(new Guid("{00000000-1111-1111-1111-000000000000}"),new Guid("{00000000-CCCC-1111-1111-000000000000}"), new Guid("{00000000-FFFF-1111-1111-000000000000}"), "AAA", new Guid("{00000000-1111-1111-9999-000000000000}")),
					new Tuple<Guid, Guid, Guid, string, Guid>(new Guid("{00000000-1111-1111-2222-000000000000}"),new Guid("{00000000-CCCC-1111-2222-000000000000}"), new Guid("{00000000-FFFF-1111-2222-000000000000}"), "BBB", new Guid("{00000000-1111-1111-8888-000000000000}")),
					new Tuple<Guid, Guid, Guid, string, Guid>(new Guid("{00000000-1111-1111-3333-000000000000}"),new Guid("{00000000-CCCC-1111-3333-000000000000}"), new Guid("{00000000-FFFF-1111-3333-000000000000}"), "CCC", new Guid("{00000000-1111-1111-7777-000000000000}")),
					new Tuple<Guid, Guid, Guid, string, Guid>(new Guid("{00000000-1111-1111-4444-000000000000}"),new Guid("{00000000-CCCC-1111-2222-000000000000}"), new Guid("{00000000-FFFF-1111-2222-000000000000}"), "BBB", new Guid("{00000000-1111-1111-6666-000000000000}")),
				}, eHubITCustomsJobStatuResult, "Precondition");

			var messageTrackingID = Guid.NewGuid().ToString();
			request.Clear();
			request.Container["id"] = "_empty";
			request.Container["oper"] = "add";

			request.Container["FileLastModified"] = new DateTime(2017, 01, 01, 01, 02, 04).ToString();
			request.Container["LastStatus"] = "DDD";
			request.Container["ReferenceID"] = "ReferenceID5";
			request.Container["NotifiedInvalidProfile"] = bool.TrueString;
			request.Container["FileName"] = "FileName5";
			request.Container["JobID"] = "JobID5";
			request.Container["SenderID"] = "Client2";
			request.Container["ClientSystemID"] = "ClientSystem2";
			request.Container["ProdInd"] = bool.FalseString;
			request.Container["PollingStart"] = new DateTime(2017, 01, 01, 01, 01, 04).ToString();
			request.Container["MessageType"] = "";
			request.Container["MessageTrackingID"] = messageTrackingID;


			JsonResult responseAdd = AddOrUpdateOrDeleteOnJobStatusTest(logger) as JsonResult;

			var resultState = (bool)responseAdd.Data.GetType().GetProperty("success").GetValue(responseAdd.Data, null);
			Assert.IsTrue(resultState);
			var newId = (Guid)responseAdd.Data.GetType().GetProperty("newid").GetValue(responseAdd.Data, null);

			var eHubITCustomsJobStatusResResult = context.eHubITCustomsJobStatus.Select(ck => new Tuple<Guid, Guid, Guid, string, string>(ck.IT_PK, ck.IT_CC_Sender, ck.IT_EH_ClientSystem, ck.IT_LastStatus, ck.IT_FileName)).ToArray();
			CollectionAssert.AreEqual(new[] {
					new Tuple<Guid, Guid, Guid, string, string>(new Guid("{00000000-1111-1111-1111-000000000000}"),new Guid("{00000000-CCCC-1111-1111-000000000000}"), new Guid("{00000000-FFFF-1111-1111-000000000000}"), "AAA", "FileName1"),
					new Tuple<Guid, Guid, Guid, string, string>(new Guid("{00000000-1111-1111-2222-000000000000}"),new Guid("{00000000-CCCC-1111-2222-000000000000}"), new Guid("{00000000-FFFF-1111-2222-000000000000}"), "BBB", "FileName2"),
					new Tuple<Guid, Guid, Guid, string, string>(new Guid("{00000000-1111-1111-3333-000000000000}"),new Guid("{00000000-CCCC-1111-3333-000000000000}"), new Guid("{00000000-FFFF-1111-3333-000000000000}"), "CCC", "FileName3"),
					new Tuple<Guid, Guid, Guid, string, string>(new Guid("{00000000-1111-1111-4444-000000000000}"),new Guid("{00000000-CCCC-1111-2222-000000000000}"), new Guid("{00000000-FFFF-1111-2222-000000000000}"), "BBB", "FileName2"),
					new Tuple<Guid, Guid, Guid, string, string>(newId,new Guid("{00000000-CCCC-1111-2222-000000000000}"), new Guid("{00000000-FFFF-1111-2222-000000000000}"), "DDD", "FileName5"),
				}, eHubITCustomsJobStatusResResult);

			var addedRecord = context.eHubITCustomsJobStatus.Single(c => c.IT_PK == newId);

			Assert.IsTrue(logger.Log.Contains(@"Info - [add] eHubITCustomsJobStatus"));
			Assert.IsTrue(logger.Log.Contains(@"Info - [CORP\Test.Name] [Before Changes] [add] eHubITCustomsJobStatus: IT_CC_Sender=00000000-0000-0000-0000-000000000000, IT_EH_ClientSystem=00000000-0000-0000-0000-000000000000, IT_FileName=, IT_LastStatus=, IT_MessageTrackingID=00000000-0000-0000-0000-000000000000, IT_NotifiedInvalidProfile = , IT_ProdInd = False"));
			Assert.IsTrue(logger.Log.Contains($@"Info - [CORP\Test.Name] [After Changes] [add] eHubITCustomsJobStatus: IT_CC_Sender=00000000-cccc-1111-2222-000000000000, IT_EH_ClientSystem=00000000-ffff-1111-2222-000000000000, IT_FileName=FileName5, IT_LastStatus=DDD, IT_MessageTrackingID={messageTrackingID}, IT_NotifiedInvalidProfile = True, IT_ProdInd = False"));

			messageTrackingID = Guid.NewGuid().ToString();
			request.Clear();
			request.Container["id"] = "00000000-1111-1111-4444-000000000000";
			request.Container["oper"] = "edit";

			request.Container["FileLastModified"] = new DateTime(2017, 01, 01, 01, 02, 04).ToString();
			request.Container["LastStatus"] = "DDD";
			request.Container["ReferenceID"] = "ReferenceID5";
			request.Container["NotifiedInvalidProfile"] = bool.TrueString;
			request.Container["FileName"] = "FileName5";
			request.Container["JobID"] = "JobID5";
			request.Container["SenderID"] = "Client3";
			request.Container["ClientSystemID"] = "ClientSystem3";
			request.Container["ProdInd"] = bool.FalseString;
			request.Container["PollingStart"] = new DateTime(2017, 01, 01, 01, 01, 04).ToString();
			request.Container["MessageType"] = "";
			request.Container["MessageTrackingID"] = messageTrackingID;


			JsonResult responseEdit = AddOrUpdateOrDeleteOnJobStatusTest(logger) as JsonResult;
			Assert.IsTrue((bool)responseEdit.Data.GetType().GetProperty("success").GetValue(responseEdit.Data, null));

			var eHubITCustomsJobStatuseditResult = context.eHubITCustomsJobStatus.Select(ck => new Tuple<Guid, Guid, Guid, string, string, string>(ck.IT_PK, ck.IT_CC_Sender, ck.IT_EH_ClientSystem, ck.IT_LastStatus, ck.IT_FileName, ck.IT_JobID)).ToArray();
			CollectionAssert.AreEqual(new[] {
					new Tuple<Guid, Guid, Guid, string, string, string>(new Guid("{00000000-1111-1111-1111-000000000000}"),new Guid("{00000000-CCCC-1111-1111-000000000000}"), new Guid("{00000000-FFFF-1111-1111-000000000000}"), "AAA", "FileName1","JobID1"),
					new Tuple<Guid, Guid, Guid, string, string, string>(new Guid("{00000000-1111-1111-2222-000000000000}"),new Guid("{00000000-CCCC-1111-2222-000000000000}"), new Guid("{00000000-FFFF-1111-2222-000000000000}"), "BBB", "FileName2","JobID2"),
					new Tuple<Guid, Guid, Guid, string, string, string>(new Guid("{00000000-1111-1111-3333-000000000000}"),new Guid("{00000000-CCCC-1111-3333-000000000000}"), new Guid("{00000000-FFFF-1111-3333-000000000000}"), "CCC", "FileName3","JobID3"),
					new Tuple<Guid, Guid, Guid, string, string, string>(new Guid("{00000000-1111-1111-4444-000000000000}"),new Guid("{00000000-CCCC-1111-3333-000000000000}"), new Guid("{00000000-FFFF-1111-3333-000000000000}"), "DDD", "FileName5","JobID5"),
					new Tuple<Guid, Guid, Guid, string, string, string>(newId,new Guid("{00000000-CCCC-1111-2222-000000000000}"), new Guid("{00000000-FFFF-1111-2222-000000000000}"), "DDD", "FileName5","JobID5"),
				}, eHubITCustomsJobStatuseditResult);

			Assert.IsTrue(logger.Log.Contains("Info - [edit] eHubITCustomsJobStatus"));
			Assert.IsTrue(logger.Log.Contains(@"Info - [CORP\Test.Name] [Before Changes] [edit] eHubITCustomsJobStatus: IT_CC_Sender=00000000-cccc-1111-2222-000000000000, IT_EH_ClientSystem=00000000-ffff-1111-2222-000000000000, IT_FileName=FileName2, IT_LastStatus=BBB, IT_MessageTrackingID=00000000-1111-1111-6666-000000000000, IT_NotifiedInvalidProfile = True, IT_ProdInd = False"));
			Assert.IsTrue(logger.Log.Contains($@"Info - [CORP\Test.Name] [After Changes] [edit] eHubITCustomsJobStatus: IT_CC_Sender=00000000-cccc-1111-3333-000000000000, IT_EH_ClientSystem=00000000-ffff-1111-3333-000000000000, IT_FileName=FileName5, IT_LastStatus=DDD, IT_MessageTrackingID={messageTrackingID}, IT_NotifiedInvalidProfile = True, IT_ProdInd = False"));

			request.Clear();
			request.Container["id"] = newId.ToString();
			request.Container["oper"] = "del";

			JsonResult responseDel = AddOrUpdateOrDeleteOnJobStatusTest(logger) as JsonResult;

			Assert.IsTrue((bool)responseDel.Data.GetType().GetProperty("success").GetValue(responseDel.Data, null));
			Assert.AreEqual(4, context.eHubITCustomsJobStatus.Count());
			eHubITCustomsJobStatuResult = context.eHubITCustomsJobStatus.Select(ck => new Tuple<Guid, Guid, Guid, string, Guid>(ck.IT_PK, ck.IT_CC_Sender, ck.IT_EH_ClientSystem, ck.IT_LastStatus, ck.IT_MessageTrackingID)).ToArray();
			CollectionAssert.AreEqual(new[] {
					new Tuple<Guid, Guid, Guid, string, Guid>(new Guid("{00000000-1111-1111-1111-000000000000}"),new Guid("{00000000-CCCC-1111-1111-000000000000}"), new Guid("{00000000-FFFF-1111-1111-000000000000}"), "AAA", new Guid("{00000000-1111-1111-9999-000000000000}")),
					new Tuple<Guid, Guid, Guid, string, Guid>(new Guid("{00000000-1111-1111-2222-000000000000}"),new Guid("{00000000-CCCC-1111-2222-000000000000}"), new Guid("{00000000-FFFF-1111-2222-000000000000}"), "BBB", new Guid("{00000000-1111-1111-8888-000000000000}")),
					new Tuple<Guid, Guid, Guid, string, Guid>(new Guid("{00000000-1111-1111-3333-000000000000}"),new Guid("{00000000-CCCC-1111-3333-000000000000}"), new Guid("{00000000-FFFF-1111-3333-000000000000}"), "CCC", new Guid("{00000000-1111-1111-7777-000000000000}")),
					new Tuple<Guid, Guid, Guid, string, Guid>(new Guid("{00000000-1111-1111-4444-000000000000}"),new Guid("{00000000-CCCC-1111-3333-000000000000}"), new Guid("{00000000-FFFF-1111-3333-000000000000}"), "DDD", Guid.Parse(messageTrackingID)),
				}, eHubITCustomsJobStatuResult);

			Assert.IsTrue(logger.Log.Contains("Info - [del] eHubITCustomsJobStatus"));
			Assert.IsTrue(logger.Log.Contains($@"Info - [CORP\Test.Name] [Before Changes] [del] eHubITCustomsJobStatus: IT_CC_Sender=00000000-cccc-1111-2222-000000000000, IT_EH_ClientSystem=00000000-ffff-1111-2222-000000000000, IT_FileName=FileName5, IT_LastStatus=DDD, IT_MessageTrackingID={addedRecord.IT_MessageTrackingID}, IT_NotifiedInvalidProfile = True, IT_ProdInd = False"));
			Assert.IsTrue(logger.Log.Contains($@"Info - [CORP\Test.Name] [After Changes] [del] eHubITCustomsJobStatus: IT_CC_Sender=00000000-cccc-1111-2222-000000000000, IT_EH_ClientSystem=00000000-ffff-1111-2222-000000000000, IT_FileName=FileName5, IT_LastStatus=DDD, IT_MessageTrackingID={addedRecord.IT_MessageTrackingID}, IT_NotifiedInvalidProfile = True, IT_ProdInd = False"));
		}

		[TestMethod]
		public void AddOrUpdateOrDeleteOnJobStatus_Fail()
		{
			var logger = new TestLogger();
			var context = new CargoWise.eHub.Portal.Tests.Fakes.TestContext();
			controller.Context = context;
			GenerateDefaultValues(context);

			var eHubITCustomsJobStatuResult = context.eHubITCustomsJobStatus.Select(ck => new Tuple<Guid, Guid, Guid, string, Guid>(ck.IT_PK, ck.IT_CC_Sender, ck.IT_EH_ClientSystem, ck.IT_LastStatus, ck.IT_MessageTrackingID)).ToArray();
			CollectionAssert.AreEqual(new[] {
					new Tuple<Guid, Guid, Guid, string, Guid>(new Guid("{00000000-1111-1111-1111-000000000000}"),new Guid("{00000000-CCCC-1111-1111-000000000000}"), new Guid("{00000000-FFFF-1111-1111-000000000000}"), "AAA", new Guid("{00000000-1111-1111-9999-000000000000}")),
					new Tuple<Guid, Guid, Guid, string, Guid>(new Guid("{00000000-1111-1111-2222-000000000000}"),new Guid("{00000000-CCCC-1111-2222-000000000000}"), new Guid("{00000000-FFFF-1111-2222-000000000000}"), "BBB", new Guid("{00000000-1111-1111-8888-000000000000}")),
					new Tuple<Guid, Guid, Guid, string, Guid>(new Guid("{00000000-1111-1111-3333-000000000000}"),new Guid("{00000000-CCCC-1111-3333-000000000000}"), new Guid("{00000000-FFFF-1111-3333-000000000000}"), "CCC", new Guid("{00000000-1111-1111-7777-000000000000}")),
					new Tuple<Guid, Guid, Guid, string, Guid>(new Guid("{00000000-1111-1111-4444-000000000000}"),new Guid("{00000000-CCCC-1111-2222-000000000000}"), new Guid("{00000000-FFFF-1111-2222-000000000000}"), "BBB", new Guid("{00000000-1111-1111-6666-000000000000}")),
				}, eHubITCustomsJobStatuResult, "Precondition");

			request.Clear();

			request.Container["id"] = "test";
			request.Container["oper"] = "add";

			JsonResult responseAdd = AddOrUpdateOrDeleteOnJobStatusTest(logger) as JsonResult;

			var resultState = (bool)responseAdd.Data.GetType().GetProperty("success").GetValue(responseAdd.Data, null);
			Assert.IsFalse(resultState);
			var message = (string)responseAdd.Data.GetType().GetProperty("message").GetValue(responseAdd.Data, null);
			Assert.AreEqual("Id must be GUID", message);

			request.Clear();
			request.Container["id"] = "00000000-1111-1111-1111-000000000003";
			request.Container["oper"] = "edit";


			responseAdd = AddOrUpdateOrDeleteOnJobStatusTest(logger) as JsonResult;
			resultState = (bool)responseAdd.Data.GetType().GetProperty("success").GetValue(responseAdd.Data, null);
			Assert.IsFalse(resultState);
			message = (string)responseAdd.Data.GetType().GetProperty("message").GetValue(responseAdd.Data, null);
			Assert.AreEqual("Record does not exist because Id could not be found", message);

			request.Clear();
			request.Container["id"] = "_empty";
			request.Container["oper"] = "add";

			request.Container["FileLastModified"] = new DateTime(2017, 01, 01, 01, 02, 04).ToString();
			request.Container["LastStatus"] = "DDD";
			request.Container["ReferenceID"] = "ReferenceID5";
			request.Container["NotifiedInvalidProfile"] = bool.TrueString;
			request.Container["FileName"] = "FileName5";
			request.Container["JobID"] = "JobID5";
			request.Container["SenderID"] = "NotExistClient2";
			request.Container["ClientSystemID"] = "ClientSystem2";
			request.Container["ProdInd"] = bool.FalseString;
			request.Container["PollingStart"] = new DateTime(2017, 01, 01, 01, 01, 04).ToString();
			request.Container["MessageType"] = "";

			responseAdd = AddOrUpdateOrDeleteOnJobStatusTest(logger) as JsonResult;
			resultState = (bool)responseAdd.Data.GetType().GetProperty("success").GetValue(responseAdd.Data, null);
			Assert.IsFalse(resultState);
			message = (string)responseAdd.Data.GetType().GetProperty("message").GetValue(responseAdd.Data, null);
			Assert.AreEqual("Client must be selected", message);


			request.Clear();
			request.Container["id"] = "_empty";
			request.Container["oper"] = "add";

			request.Container["FileLastModified"] = new DateTime(2017, 01, 01, 01, 02, 04).ToString();
			request.Container["LastStatus"] = "DDD";
			request.Container["ReferenceID"] = "ReferenceID5";
			request.Container["NotifiedInvalidProfile"] = bool.TrueString;
			request.Container["FileName"] = "FileName5";
			request.Container["JobID"] = "JobID5";
			request.Container["SenderID"] = "Client2";
			request.Container["ClientSystemID"] = "NotExistClientSystem2";
			request.Container["ProdInd"] = bool.FalseString;
			request.Container["PollingStart"] = new DateTime(2017, 01, 01, 01, 01, 04).ToString();
			request.Container["MessageType"] = "";

			responseAdd = AddOrUpdateOrDeleteOnJobStatusTest(logger) as JsonResult;
			resultState = (bool)responseAdd.Data.GetType().GetProperty("success").GetValue(responseAdd.Data, null);
			Assert.IsFalse(resultState);
			message = (string)responseAdd.Data.GetType().GetProperty("message").GetValue(responseAdd.Data, null);
			Assert.AreEqual("Client system must be selected", message);


			request.Clear();
			request.Container["id"] = "_empty";
			request.Container["oper"] = "add";

			request.Container["FileLastModified"] = new DateTime(2017, 01, 01, 01, 02, 04).ToString();
			request.Container["LastStatus"] = "DDD";
			request.Container["NotifiedInvalidProfile"] = bool.TrueString;
			request.Container["FileName"] = "FileName5";
			request.Container["JobID"] = "JobID5";
			request.Container["SenderID"] = "Client2";
			request.Container["ClientSystemID"] = "ClientSystem2";
			request.Container["ProdInd"] = bool.FalseString;
			request.Container["PollingStart"] = new DateTime(2017, 01, 01, 01, 01, 04).ToString();
			request.Container["MessageType"] = "";

			responseAdd = AddOrUpdateOrDeleteOnJobStatusTest(logger) as JsonResult;
			resultState = (bool)responseAdd.Data.GetType().GetProperty("success").GetValue(responseAdd.Data, null);
			Assert.IsFalse(resultState);
			message = (string)responseAdd.Data.GetType().GetProperty("message").GetValue(responseAdd.Data, null);
			Assert.AreEqual("referenceID must be filled", message);

			request.Clear();
			request.Container["id"] = "_empty";
			request.Container["oper"] = "add";

			request.Container["FileLastModified"] = new DateTime(2017, 01, 01, 01, 02, 04).ToString();
			request.Container["LastStatus"] = "DDD";
			request.Container["NotifiedInvalidProfile"] = bool.TrueString;
			request.Container["JobID"] = "JobID5";
			request.Container["SenderID"] = "Client2";
			request.Container["ReferenceID"] = "ReferenceID5";
			request.Container["ClientSystemID"] = "ClientSystem2";
			request.Container["ProdInd"] = bool.FalseString;
			request.Container["PollingStart"] = new DateTime(2017, 01, 01, 01, 01, 04).ToString();
			request.Container["MessageType"] = "";

			responseAdd = AddOrUpdateOrDeleteOnJobStatusTest(logger) as JsonResult;
			resultState = (bool)responseAdd.Data.GetType().GetProperty("success").GetValue(responseAdd.Data, null);
			Assert.IsFalse(resultState);
			message = (string)responseAdd.Data.GetType().GetProperty("message").GetValue(responseAdd.Data, null);
			Assert.AreEqual("fileName must be filled", message);

			request.Clear();
			request.Container["id"] = "_empty";
			request.Container["oper"] = "add";

			request.Container["FileLastModified"] = new DateTime(2017, 01, 01, 01, 02, 04).ToString();
			request.Container["LastStatus"] = "DDD";
			request.Container["ReferenceID"] = "ReferenceID5";
			request.Container["NotifiedInvalidProfile"] = bool.TrueString;
			request.Container["FileName"] = "FileName5";
			request.Container["SenderID"] = "Client2";
			request.Container["ClientSystemID"] = "ClientSystem2";
			request.Container["ProdInd"] = bool.FalseString;
			request.Container["PollingStart"] = new DateTime(2017, 01, 01, 01, 01, 04).ToString();
			request.Container["MessageType"] = "";

			responseAdd = AddOrUpdateOrDeleteOnJobStatusTest(logger) as JsonResult;
			resultState = (bool)responseAdd.Data.GetType().GetProperty("success").GetValue(responseAdd.Data, null);
			Assert.IsFalse(resultState);
			message = (string)responseAdd.Data.GetType().GetProperty("message").GetValue(responseAdd.Data, null);
			Assert.AreEqual("jobID must be filled", message);

			request.Clear();
			request.Container["id"] = "_empty";
			request.Container["oper"] = "add";

			request.Container["FileLastModified"] = new DateTime(2017, 01, 01, 01, 02, 04).ToString();
			request.Container["LastStatus"] = "DDD";
			request.Container["ReferenceID"] = "ReferenceID5";
			request.Container["NotifiedInvalidProfile"] = bool.TrueString;
			request.Container["FileName"] = "FileName5";
			request.Container["JobID"] = "JobID5";
			request.Container["SenderID"] = "Client2";
			request.Container["ClientSystemID"] = "ClientSystem2";
			request.Container["ProdInd"] = bool.FalseString;
			request.Container["PollingStart"] = new DateTime(2017, 01, 01, 01, 01, 04).ToString();

			responseAdd = AddOrUpdateOrDeleteOnJobStatusTest(logger) as JsonResult;
			resultState = (bool)responseAdd.Data.GetType().GetProperty("success").GetValue(responseAdd.Data, null);
			Assert.IsFalse(resultState);
			message = (string)responseAdd.Data.GetType().GetProperty("message").GetValue(responseAdd.Data, null);
			Assert.AreEqual("messageType must be filled", message);


			request.Clear();
			request.Container["id"] = "_empty";
			request.Container["oper"] = "add";

			request.Container["FileLastModified"] = new DateTime(2017, 01, 01, 01, 02, 04).ToString();
			request.Container["LastStatus"] = "DDD";
			request.Container["ReferenceID"] = "ReferenceID5";
			request.Container["NotifiedInvalidProfile"] = bool.TrueString;
			request.Container["FileName"] = "FileName5";
			request.Container["JobID"] = "JobID5";
			request.Container["SenderID"] = "Client2";
			request.Container["ClientSystemID"] = "ClientSystem2";
			request.Container["ProdInd"] = bool.FalseString;
			request.Container["PollingStart"] = new DateTime(2017, 01, 01, 01, 01, 04).ToString();
			request.Container["MessageType"] = "";

			responseAdd = AddOrUpdateOrDeleteOnJobStatusTest(logger) as JsonResult;
			resultState = (bool)responseAdd.Data.GetType().GetProperty("success").GetValue(responseAdd.Data, null);
			Assert.IsFalse(resultState);
			message = (string)responseAdd.Data.GetType().GetProperty("message").GetValue(responseAdd.Data, null);
			Assert.AreEqual("messageTrackingID must be filled", message);

			request.Clear();
			request.Container["id"] = "_empty";
			request.Container["oper"] = "add";

			request.Container["FileLastModified"] = new DateTime(2017, 01, 01, 01, 02, 04).ToString();
			request.Container["LastStatus"] = "DDD";
			request.Container["ReferenceID"] = "ReferenceID5";
			request.Container["NotifiedInvalidProfile"] = bool.TrueString;
			request.Container["FileName"] = "FileName5";
			request.Container["JobID"] = "JobID5";
			request.Container["SenderID"] = "Client2";
			request.Container["ClientSystemID"] = "ClientSystem2";
			request.Container["ProdInd"] = bool.FalseString;
			request.Container["PollingStart"] = new DateTime(2017, 01, 01, 01, 01, 04).ToString();
			request.Container["MessageType"] = "";
			request.Container["MessageTrackingID"] = "";

			responseAdd = AddOrUpdateOrDeleteOnJobStatusTest(logger) as JsonResult;
			resultState = (bool)responseAdd.Data.GetType().GetProperty("success").GetValue(responseAdd.Data, null);
			Assert.IsFalse(resultState);
			message = (string)responseAdd.Data.GetType().GetProperty("message").GetValue(responseAdd.Data, null);
			Assert.AreEqual("messageTrackingID must be GUID", message);
		}

		protected JsonResult AddOrUpdateOrDeleteOnJobStatusTest(ILog logger)
		{
			controller.logger = logger;
			return controller.AddOrUpdateOrDeleteOnJobStatus();
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
				new { CC_ID = "TEST0003", CC_FriendlyName = "Test Client 3" },
				new { CC_ID = "TEST0002", CC_FriendlyName = "Test Client 2" },
				new { CC_ID = "TEST0001", CC_FriendlyName = "Test Client 1" },
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
			request.Container["CC_FriendlyName"] = "3";
			IList<object> expected3 = new List<object>
			{
				new { CC_ID = "TEST0003", CC_FriendlyName = "Test Client 3" },
			};

			var result3 = controller.Clients(true);

			Assert.IsNotNull(result2);
			AssertEx.JsonResultMatchesList(expected3, result3, "eHubClients", true);
		} 

		[TestMethod]
		public void TestClientSystemsGetIncludeNonProdCWSystems()
		{
			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "8";
			request.Container["sidx"] = "EH_ID";
			request.Container["sord"] = "asc";
			request.Container["_search"] = "false";


			var utcDateTime = new DateTime(2022, 09, 26, 10, 05, 20);
			IList<object> expected1 = new List<object>
			{
				new { EH_ID = "Test WTLEHK1", EH_LastUpdateUTC = utcDateTime },
				new { EH_ID = "Test WTLEHK2", EH_LastUpdateUTC = utcDateTime.AddDays(-5) },
				new { EH_ID = "WTLEHK3", EH_LastUpdateUTC = utcDateTime.AddMinutes(-5) },
				new { EH_ID = "WTLEHK4", EH_LastUpdateUTC = utcDateTime.AddHours(-5)},
				new { EH_ID = "WTLSV1", EH_LastUpdateUTC = utcDateTime.AddHours(-5)},
				new { EH_ID = "WTLSV2", EH_LastUpdateUTC = utcDateTime.AddHours(-5)},
			};

			var result1 = controller.ClientSystem(true);

			Assert.IsNotNull(result1);
			AssertEx.JsonResultMatchesList(expected1, result1, "eHubClientSystem", true);

			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["sidx"] = "EH_LastUpdateUTC";
			request.Container["sord"] = "desc";
			request.Container["_search"] = "true";
			request.Container["EH_ID"] = "Test";

			IList<object> expected2 = new List<object>
			{
				new { EH_ID = "Test WTLEHK1", EH_LastUpdateUTC = utcDateTime },
				new { EH_ID = "Test WTLEHK2", EH_LastUpdateUTC = utcDateTime.AddDays(-5) },
			};

			var result2 = controller.ClientSystem(true);

			Assert.IsNotNull(result2);
			AssertEx.JsonResultMatchesList(expected2, result2, "eHubClientSystem", true);
		}

		[TestMethod]

		public void TestClientSystemsGetExcludeNonProdCWSystems()
		{
			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "8";
			request.Container["sidx"] = "EH_ID";
			request.Container["sord"] = "asc";
			request.Container["_search"] = "false";


			var utcDateTime = new DateTime(2022, 09, 26, 10, 05, 20);
			IList<object> expected1 = new List<object>
			{
				new { EH_ID = "WTLSV1", EH_LastUpdateUTC = utcDateTime.AddHours(-5) }
			};

			var result1 = controller.ClientSystem(false);

			Assert.IsNotNull(result1);
			AssertEx.JsonResultMatchesList(expected1, result1, "eHubClientSystem");
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
		public void TestClientSystemsFilterCaseInsensitive()
		{
			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "8";
			request.Container["sidx"] = "EH_ID";
			request.Container["sord"] = "asc";
			request.Container["_search"] = "true";
			request.Container["EH_ID"] = "wtlsv";


			var utcDateTime = new DateTime(2022, 09, 26, 10, 05, 20);
			IList<object> expected1 = new List<object>
			{
				new { EH_ID = "WTLSV1", EH_LastUpdateUTC = utcDateTime.AddHours(-5)},
				new { EH_ID = "WTLSV2", EH_LastUpdateUTC = utcDateTime.AddHours(-5)},
			};

			var result1 = controller.ClientSystem(true);

			Assert.IsNotNull(result1);
			AssertEx.JsonResultMatchesList(expected1, result1, "eHubClientSystem");
		}
	}
}
