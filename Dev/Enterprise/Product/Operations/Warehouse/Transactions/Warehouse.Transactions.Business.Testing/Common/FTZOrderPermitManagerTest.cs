using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration.Customs.PermitService;
using Enterprise.Warehouse.Transactions.CodeLists;
using Moq;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class FTZOrderPermitManagerTest : WhsTestCaseWithFactory
	{
		#region TestTryAquireFTZPermit_EachPermitRequestMustHaveAMatchingPermitResponse

		public void TestTryAquireFTZPermit_EachPermitRequestMustHaveAMatchingPermitResponse()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = Helper.CreateFTZWarehouseInUS();

			var order = Helper.CreateWhsOrder(data.Org1, whs);
			order.WD_DocketSubType = OrderType.Codes.Customs;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			orderLine.CustomsData.WB_CustomsQty = 50m;
			orderLine.CustomsData.WB_Tariff = "1020304050";

			var permitService = new Mock<IPermitService>();
			permitService.Setup(m => m.TryGetPermits(It.IsAny<IEnumerable<IPermitWithdrawRequest>>())).Returns(
				new WhsPermitWithdrawRequestResponseResultForTest
				{
					Responses = Array.Empty<IPermitWithdrawRequestResponse>()
				});

			using (ObjectFactory.Substitute(permitService.Object))
			{
				AssertExceptionThrown(typeof(InvalidOperationException),
					"Customs' TryGetPermits() did not provide a response for every Permit Request.",
					() => FTZOrderPermitManager.TryAquireFTZPermit(new[] { order }, new[] { orderLine }));
			}
		}

		#endregion

		#region TestIsPermitAvailable_EachPermitRequestMustHaveAMatchingPermitResponse

		public void TestIsPermitAvailable_EachPermitRequestMustHaveAMatchingPermitResponse()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = Helper.CreateFTZWarehouseInUS();

			var order = Helper.CreateWhsOrder(data.Org1, whs);
			order.WD_DocketSubType = OrderType.Codes.Customs;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			orderLine.CustomsData.WB_CustomsQty = 50m;
			orderLine.CustomsData.WB_Tariff = "1020304050";

			var permitService = new Mock<IPermitService>();
			permitService.Setup(m => m.IsPermitAvailable(It.IsAny<IEnumerable<IPermitWithdrawRequest>>())).Returns(
				new WhsPermitWithdrawRequestResponseResultForTest
				{
					Responses = Array.Empty<IPermitWithdrawRequestResponse>()
				});

			using (ObjectFactory.Substitute(permitService.Object))
			{
				AssertExceptionThrown(typeof(InvalidOperationException),
					"Customs' IsPermitAvailable() did not provide a response for every Permit Request.",
					() => FTZOrderPermitManager.IsPermitAvailable(order, new[] { orderLine }));
			}
		}

		#endregion

		#region TestIsPermitAvailable

		[TestDate(2017, 9, 4)]
		public void TestIsPermitAvailable()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = Helper.CreateFTZWarehouseInUS();
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs, "R1", data.Part1, 5m, whs.DefaultLocation, "");

			var order = Helper.CreateWhsOrder(data.Org1, whs);
			order.WD_DocketSubType = OrderType.Codes.Customs;
			var orderLineWithPermitErrors = Helper.CreateWhsOrderLine(order, data.Part1, 2m);
			orderLineWithPermitErrors.CustomsData.WB_CustomsQty = 50m;
			orderLineWithPermitErrors.CustomsData.WB_Tariff = "1020304050";

			var orderLineWithoutPermitErrors = Helper.CreateWhsOrderLine(order, data.Part1, 2m);

			Factory.Save();

			var permitService = new Mock<IPermitService>();
			var responseForOrderLineWithPermitIssues =
				new WhsPermitWithdrawRequestResponseForTest(orderLineWithPermitErrors, SuccessOrFailure.Failure, 0m,
					"001");
			var responseForOrderLineWithoutPermitIssues =
				new WhsPermitWithdrawRequestResponseForTest(orderLineWithoutPermitErrors, SuccessOrFailure.Success, 0m,
					"002");
			permitService.Setup(m => m.IsPermitAvailable(It.IsAny<IEnumerable<IPermitWithdrawRequest>>()))
				.Returns(new WhsPermitWithdrawRequestResponseResultForTest
				{
					Responses = new[]
					{
						responseForOrderLineWithPermitIssues, responseForOrderLineWithoutPermitIssues
					}
				});

			using (ObjectFactory.Substitute(permitService.Object))
			{
				var actionResult = FTZOrderPermitManager.IsPermitAvailable(order,
					new[] { orderLineWithPermitErrors, orderLineWithoutPermitErrors });
				AssertEquals("Must be in error.", false, actionResult.IsSuccess);
				AssertEquals(
					"Not every Order Line on this Pick could be matched to a weekly estimate. Errors below:\r\nNo weekly estimate found for Order Line 1 (2 UNT of Product P1) (Date: 04-Sep-17, ).",
					actionResult.ErrorMessage);

				permitService.Verify(m => m.TryGetPermits(It.IsAny<IEnumerable<IPermitWithdrawRequest>>()), Times.Never);
				permitService.Verify(m => m.RelinquishPermitTransactions(It.IsAny<IEnumerable<IPermitTransactionDetail>>()),
					Times.Never);
			}

			permitService.VerifyAll();
			AssertHasRowWarning("Order Has Warning.", order,
				"Not all Order Lines could be granted a weekly estimate for this Order.");
			AssertOrderLine(orderLineWithPermitErrors, "", ZShort.Zero,
				"No weekly estimate found for Order Line 1 (2 UNT of Product P1) (Date: 04-Sep-17, ).");
			AssertOrderLine(orderLineWithoutPermitErrors, "", ZShort.Zero);
		}

		public void TestIsPermitAvailable_OrderLineWithoutPermitIssues()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = Helper.CreateFTZWarehouseInUS();
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs, "R1", data.Part1, 5m, whs.DefaultLocation, "");

			var order = Helper.CreateWhsOrder(data.Org1, whs);
			order.WD_DocketSubType = OrderType.Codes.Customs;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			orderLine.CustomsData.WB_CustomsQty = 50m;
			orderLine.CustomsData.WB_Tariff = "1020304050";
			Factory.Save();

			var permitService = new Mock<IPermitService>();
			var responseForIsAvailablePermits =
				new WhsPermitWithdrawRequestResponseForTest(orderLine, SuccessOrFailure.Success, 0m, "001");
			permitService.Setup(m => m.IsPermitAvailable(It.IsAny<IEnumerable<IPermitWithdrawRequest>>())).Returns(
				new WhsPermitWithdrawRequestResponseResultForTest
				{
					Responses = new[] { responseForIsAvailablePermits }
				});

			using (ObjectFactory.Substitute(permitService.Object))
			{
				var actionResult = FTZOrderPermitManager.IsPermitAvailable(order, new[] { orderLine });
				AssertEquals("Must be a success.", true, actionResult.IsSuccess);
				AssertEquals("No Permit errors expected.", "", actionResult.ErrorMessage);
				AssertOrderLine(orderLine, "", ZShort.Zero);

				permitService.Verify(m => m.TryGetPermits(It.IsAny<IEnumerable<IPermitWithdrawRequest>>()), Times.Never);
				permitService.Verify(m => m.RelinquishPermitTransactions(It.IsAny<IEnumerable<IPermitTransactionDetail>>()), Times.Never);
			}

			permitService.VerifyAll();
		}

		#endregion

		#region TestTryAquireFTZPermit

		[TestDate(2017, 9, 4)]
		public void TestTryAquireFTZPermit()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = Helper.CreateFTZWarehouseInUS();
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs, "R1", data.Part1, 10m, whs.DefaultLocation, "");

			var order = Helper.CreateWhsOrder(data.Org1, whs, "O1");
			order.WD_DocketSubType = OrderType.Codes.Customs;
			var orderLineWithPermitErrors = Helper.CreateWhsOrderLine(order, data.Part1, 2m);
			orderLineWithPermitErrors.CustomsData.WB_CustomsQty = 50m;
			orderLineWithPermitErrors.CustomsData.WB_Tariff = "1020304050";

			var orderLineWithoutPermitErrors = Helper.CreateWhsOrderLine(order, data.Part1, 2m);

			var orderWithoutPermitIssues = Helper.CreateWhsOrder(data.Org1, whs, "O2");
			order.WD_DocketSubType = OrderType.Codes.Customs;
			var orderLineInOrderWithoutPermitErrors =
				Helper.CreateWhsOrderLine(orderWithoutPermitIssues, data.Part1, 1m);

			Factory.Save();

			var permitService = new Mock<IPermitService>();
			var responseForOrderLineWithPermitIssues =
				new WhsPermitWithdrawRequestResponseForTest(orderLineWithPermitErrors, SuccessOrFailure.Failure, 0m,
					"001");
			var responseForOrderLineWithoutPermitIssues =
				new WhsPermitWithdrawRequestResponseForTest(orderLineWithoutPermitErrors, SuccessOrFailure.Success, 0m,
					"002");
			var responseForOrderLineInOrderWithoutPermitIssues =
				new WhsPermitWithdrawRequestResponseForTest(orderLineInOrderWithoutPermitErrors,
					SuccessOrFailure.Success, 0m, "003");
			permitService.Setup(m => m.TryGetPermits(It.IsAny<IEnumerable<IPermitWithdrawRequest>>())).Returns(
				new WhsPermitWithdrawRequestResponseResultForTest
				{
					Responses = new[]
					{
						responseForOrderLineWithPermitIssues, responseForOrderLineWithoutPermitIssues,
						responseForOrderLineInOrderWithoutPermitIssues
					}
				});

			using (ObjectFactory.Substitute(permitService.Object))
			{
				var actionResult = FTZOrderPermitManager.TryAquireFTZPermit(new[] { order, orderWithoutPermitIssues },
					new[]
					{
						orderLineWithPermitErrors, orderLineWithoutPermitErrors, orderLineInOrderWithoutPermitErrors
					});
				AssertEquals("Must be in error.", false, actionResult.IsSuccess);
				AssertEquals(
					"Not every Order Line on this Pick could be matched to a weekly estimate. Errors below:\r\nNo weekly estimate found for Order Line 1 (2 UNT of Product P1) (Date: 04-Sep-17, ).",
					actionResult.ErrorMessage);

				permitService.Verify(m => m.IsPermitAvailable(It.IsAny<IEnumerable<IPermitWithdrawRequest>>()),
					Times.Never);
				permitService.Verify(m => m.RelinquishPermitTransactions(It.IsAny<IEnumerable<IPermitTransactionDetail>>()),
					Times.Never);
			}

			permitService.VerifyAll();
			AssertHasRowWarning("Order Has Warning.", order,
				"Not all Order Lines could be granted a weekly estimate for this Order.");
			AssertOrderLine(orderLineWithPermitErrors, "", ZShort.Zero,
				"No weekly estimate found for Order Line 1 (2 UNT of Product P1) (Date: 04-Sep-17, ).");
			AssertOrderLine(orderLineWithoutPermitErrors, "002", ZShort.Zero);
			AssertOrderLine(orderLineInOrderWithoutPermitErrors, "003", ZShort.Zero);
		}

		public void TestTryAquireFTZPermit_OrderLineWithoutPermitIssues()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = Helper.CreateFTZWarehouseInUS();
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs, "R1", data.Part1, 5m, whs.DefaultLocation, "");

			var order = Helper.CreateWhsOrder(data.Org1, whs);
			order.WD_DocketSubType = OrderType.Codes.Customs;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			orderLine.CustomsData.WB_CustomsQty = 50m;
			orderLine.CustomsData.WB_Tariff = "1020304050";
			Factory.Save();

			var permitService = new Mock<IPermitService>();
			var responseForIsAvailablePermits =
				new WhsPermitWithdrawRequestResponseForTest(orderLine, SuccessOrFailure.Success, 0m, "001");
			permitService.Setup(m => m.TryGetPermits(It.IsAny<IEnumerable<IPermitWithdrawRequest>>())).Returns(
				new WhsPermitWithdrawRequestResponseResultForTest
				{
					Responses = new[] { responseForIsAvailablePermits }
				});

			using (ObjectFactory.Substitute(permitService.Object))
			{
				var actionResult = FTZOrderPermitManager.TryAquireFTZPermit(new[] { order }, new[] { orderLine });
				AssertEquals("Must be a success.", true, actionResult.IsSuccess);
				AssertEquals("No Permit errors expected.", "", actionResult.ErrorMessage);
				AssertOrderLine(orderLine, "001", ZShort.Zero);

				permitService.Verify(m => m.IsPermitAvailable(It.IsAny<IEnumerable<IPermitWithdrawRequest>>()),
					Times.Never);
				permitService.Verify(m => m.RelinquishPermitTransactions(It.IsAny<IEnumerable<IPermitTransactionDetail>>()),
					Times.Never);
			}

			permitService.VerifyAll();
		}

		static void AssertOrderLine(WhsOrderLine orderLine, string entryKey, ZShort entryLineNumber,
			string warningMessage = null)
		{
			AssertEquals("OutwardEntryNumber", entryKey, orderLine.CustomsData.WB_EntryKey);
			AssertEquals("OutwardEntryLineNumber", entryLineNumber, orderLine.CustomsData.WB_EntryLineNo);

			if (string.IsNullOrEmpty(warningMessage))
			{
				AssertNoRowWarnings("OrderLine must not has any Warnings.", orderLine);
			}
			else
			{
				AssertHasRowWarning("OrderLine must have a Warning.", orderLine, warningMessage);
			}
		}

		#endregion
	}
}
