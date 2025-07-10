using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class AddJobServiceChargeTest : WhsSecureServiceTestCase
	{
		#region TestAddJobServices

		[TestDate(2014, 10, 20)]
		public void TestAddJobServices_Receive()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var abcService = receive.Services.AddIfNotExists("ABC").First();
			abcService.ES_Booked = ZDateTime.Today.AddDays(-1);
			abcService.ES_Completed = ZDateTime.Today.AddDays(-1);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");

			var receiveForPick = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receiveForPick, data.Part1, 10m, data.Whs1.DefaultLocation);
			receiveForPick.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			var orderWithPick = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(orderWithPick);
			var pickLine = orderWithPick.Lines.Single().PickLines.Single();
			Helper.Factory.Save();

			AssertEquals("Precondition.", 1, receive.Services.Count);

			var webService1 = GetNewWebService();
			var response1 = webService1.AddJobServiceCharge(receive.PK.ToGuid(), JobServiceSupporterStrategy.WhsReceive, "FUM", 1, "Some note");
			AssertSuccessfulResponse(response1, webService1);

			AssertEquals("Added Job Service.", 2, receive.Services.Count);
			var addedService = (WhsJobService)receive.Services.GetServices("FUM").FirstOrDefault();
			AssertJobServiceCharge(addedService, "FUM", ZDateTime.Today, ZDateTime.Today, 1m, "Some note");

			var webService2 = GetNewWebService();
			var response2 = webService2.AddJobServiceCharge(receive.PK.ToGuid(), JobServiceSupporterStrategy.WhsReceive, "ABC", 2, "Some note");
			AssertSuccessfulResponse(response2, webService2);

			AssertJobServiceCharge(abcService, "ABC", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(-1), 2m, "Some note");
		}

		[TestDate(2014, 10, 20)]
		public void TestAddJobServices_Order()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var abcService = receive.Services.AddIfNotExists("ABC").First();
			abcService.ES_Booked = ZDateTime.Today.AddDays(-1);
			abcService.ES_Completed = ZDateTime.Today.AddDays(-1);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");

			var receiveForPick = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receiveForPick, data.Part1, 10m, data.Whs1.DefaultLocation);
			receiveForPick.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			var orderWithPick = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(orderWithPick);
			var pickLine = orderWithPick.Lines.Single().PickLines.Single();
			Helper.Factory.Save();

			AssertEquals("Precondition.", false, order.Services.Any());

			var webService = GetNewWebService();
			var response = webService.AddJobServiceCharge(order.PK.ToGuid(), JobServiceSupporterStrategy.WhsReceive, "FUM", 1, "ABC");
			AssertSuccessfulResponse(response, webService);

			AssertEquals("Added Job Service.", 1, order.Services.Count);
			var addedService = (WhsJobService)order.Services.First();
			AssertJobServiceCharge(addedService, "FUM", ZDateTime.Today, ZDateTime.Today, 1m, "ABC");
		}

		[TestDate(2014, 10, 20)]
		public void TestAddJobServices_PickLine()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var abcService = receive.Services.AddIfNotExists("ABC").First();
			abcService.ES_Booked = ZDateTime.Today.AddDays(-1);
			abcService.ES_Completed = ZDateTime.Today.AddDays(-1);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");

			var receiveForPick = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receiveForPick, data.Part1, 10m, data.Whs1.DefaultLocation);
			receiveForPick.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			var orderWithPick = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(orderWithPick);
			var pickLine = orderWithPick.Lines.Single().PickLines.Single();
			Helper.Factory.Save();

			AssertEquals("Precondition.", false, orderWithPick.Services.Any());

			var webService = GetNewWebService();
			var response = webService.AddJobServiceCharge(pickLine.PK.ToGuid(), JobServiceSupporterStrategy.WhsPickLine, "FUM", 1, "ABC");
			AssertSuccessfulResponse(response, webService);

			AssertEquals("Added Job Service.", 1, orderWithPick.Services.Count);
			var addedService = (WhsJobService)orderWithPick.Services.First();
			AssertJobServiceCharge(addedService, "FUM", ZDateTime.Today, ZDateTime.Today, 1m, "ABC");
		}

		#endregion

		#region AssertJobServiceCharge

		void AssertJobServiceCharge(JobService jobService, string serviceCode, ZDateTime bookedTime, ZDateTime completedTime, Decimal serviceCount, string serviceNote)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Code", serviceCode, jobService.ES_ServiceCode);
				AssertEquals("DateBooked", bookedTime, jobService.ES_Booked);
				AssertEquals("DateCompleted", completedTime, jobService.ES_Completed);
				AssertEquals("Count", serviceCount, jobService.ES_ServiceCount);
				AssertEquals("ServiceNote", serviceNote, jobService.ES_ServiceNote);
			});
		}

		#endregion
	}
}
