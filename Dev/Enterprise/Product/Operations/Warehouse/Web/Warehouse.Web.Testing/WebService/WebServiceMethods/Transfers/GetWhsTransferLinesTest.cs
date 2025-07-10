using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class GetWhsTransferLinesTest : WhsTransferSecureServiceTestCase
	{
		#region TestGetWhsTransferLines

		#region TestGetWhsTransferLines

		public void TestGetWhsTransferLines()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			locationA1.FormattedCheckDigit = "11";

			var locationA2 = data.Whs1.FindLocation("A-2");
			locationA2.FormattedCheckDigit = "22";

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 200m, locationA1, "");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 200m, locationA2, "");

			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transfer1Line1 = Helper.CreateWhsTransferLine(transfer1, data.Part1, 10m, locationA1, locationA2);
			var transfer1Line2 = Helper.CreateWhsTransferLine(transfer1, data.Part1, 15m, locationA2, locationA1);
			var transfer1Line3 = Helper.CreateWhsTransferLine(transfer1, data.Part1, 20m, locationA1, locationA2);
			transfer1.RunPreSaveValidation(); // populate picklines

			var transfer2 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR2", Notify);
			var transfer2Line1 = Helper.CreateWhsTransferLine(transfer2, data.Part1, 25m, locationA1, locationA2);
			var transfer2Line2 = Helper.CreateWhsTransferLine(transfer2, data.Part1, 30m, locationA1, locationA2);
			transfer2.RunPreSaveValidation(); // populate picklines

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetWhsTransferLines(new Guid[] { transfer1.PK.ToGuid() });
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response.Error);
				AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));
				AssertEquals(3, response.TotalTransferLinesToLoad);
				AssertEquals(3, response.LineInfoCollection.Count);
				AssertContainsExactElementsInAnyOrder(new[] { transfer1Line1.PK.ToGuid(), transfer1Line2.PK.ToGuid(), transfer1Line3.PK.ToGuid() }, response.LineInfoCollection.Select(l => l.PK));
				AssertContainsExactElementsInAnyOrder(new[] { "11", "22", "11" }, response.LineInfoCollection.Select(s => s.LocationFormattedCheckDigit));
				AssertContainsExactElementsInAnyOrder(new[] { "22", "11", "22" }, response.LineInfoCollection.Select(s => s.DestLocationFormattedCheckDigit));
			});
		}

		public void TestGetWhsTransferLines_MultipleTransfers()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			locationA1.FormattedCheckDigit = "11";

			var locationA2 = data.Whs1.FindLocation("A-2");
			locationA2.FormattedCheckDigit = "22";

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 200m, locationA1, "");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 200m, locationA2, "");

			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transfer1Line1 = Helper.CreateWhsTransferLine(transfer1, data.Part1, 10m, locationA1, locationA2);
			var transfer1Line2 = Helper.CreateWhsTransferLine(transfer1, data.Part1, 15m, locationA1, locationA2);
			var transfer1Line3 = Helper.CreateWhsTransferLine(transfer1, data.Part1, 20m, locationA1, locationA2);
			transfer1.RunPreSaveValidation(); // populate picklines

			var transfer2 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR2", Notify);
			var transfer2Line1 = Helper.CreateWhsTransferLine(transfer2, data.Part1, 25m, locationA2, locationA1);
			var transfer2Line2 = Helper.CreateWhsTransferLine(transfer2, data.Part1, 30m, locationA2, locationA1);
			transfer2.RunPreSaveValidation(); // populate picklines

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetWhsTransferLines(new Guid[] { transfer1.PK.ToGuid(), transfer2.PK.ToGuid() });
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response.Error);
				AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));
				AssertEquals(5, response.TotalTransferLinesToLoad);
				AssertEquals(5, response.LineInfoCollection.Count);
				AssertContainsExactElementsInAnyOrder(new[] { transfer1Line1.PK.ToGuid(), transfer1Line2.PK.ToGuid(), transfer1Line3.PK.ToGuid(), transfer2Line1.PK.ToGuid(), transfer2Line2.PK.ToGuid() }, response.LineInfoCollection.Select(l => l.PK));
				AssertContainsExactElementsInAnyOrder(new[] { "11", "11", "11", "22", "22" }, response.LineInfoCollection.Select(s => s.LocationFormattedCheckDigit));
				AssertContainsExactElementsInAnyOrder(new[] { "22", "22", "22", "11", "11" }, response.LineInfoCollection.Select(s => s.DestLocationFormattedCheckDigit));
			});
		}

		#endregion

		#region TestGetWhsTransferLines_WithMatchingLines

		public void TestGetWhsTransferLines_WithMatchingLines()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			locationA1.FormattedCheckDigit = "11";

			var locationA2 = data.Whs1.FindLocation("A-2");
			locationA2.FormattedCheckDigit = "22";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA1);
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transfer1Line = Helper.CreateWhsTransferLine(transfer, data.Part1, 25m, locationA1, locationA2);
			transfer.RunPreSaveValidation(); // populate picklines

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetWhsTransferLines(new[] { transfer.PK.ToGuid() });
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response.Error);
				AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));
				AssertEquals(1, response.TotalTransferLinesToLoad);
				AssertEquals(1, response.LineInfoCollection.Count);
				AssertEquals("Qty must be summarized with matching lines", 10m + 8m + 7m, response.LineInfoCollection[0].Qty);
				AssertEquals("Pack qty must be summarized with matching lines", 10m + 8m + 7m, response.LineInfoCollection[0].Packs);
				AssertEquals("11", response.LineInfoCollection[0].LocationFormattedCheckDigit);
				AssertEquals("22", response.LineInfoCollection[0].DestLocationFormattedCheckDigit);
			});
		}

		#endregion

		#region TestGetWhsTransferLines_InterWhsChild

		public void TestGetWhsTransferLines_InterWhsChild_Dest()
		{
			TestGetWhsTransferLines_InterWhsChild(isSource: false);
		}

		public void TestGetWhsTransferLines_InterWhsChild_Source()
		{
			TestGetWhsTransferLines_InterWhsChild(isSource: true);
		}

		void TestGetWhsTransferLines_InterWhsChild(bool isSource)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var warehouse2 = Helper.CreateWarehouse("WH2", "A", 1, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, isSource ? data.Whs1 : warehouse2, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_DocketSubType = isSource ? TransferType.Codes.InterWhsSource : TransferType.Codes.InterWhsDest;
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, "A", warehouse2.PK, "A");
			transferLine.RunPreSaveValidation();

			transferLine.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition.", DocketLineStatus.Codes.HeldForTransfer, transferLine.WE_DocketLineStatus);

			var childTransfer = transfer.ChildTransfers.First();
			AssertNotNull("Precondition: Created child transfer.", childTransfer);
			Helper.Factory.Save();

			var webService = GetNewWebService(warehouse2, staff);
			var transferLineInfo = CreatePickingTransferLineInfo("A", "", data.Org1, data.Part1, "", 10m);
			transferLineInfo.PK = transferLine.PK.ToGuid();
			AssertBusinessValidationError(webService, "Should have thrown an exception as child inter-whs transfers are not supported.", "Cannot transfer an Inter-Warehouse Transfer using the child job.", webService.GetWhsTransferLines(new[] { childTransfer.PK.ToGuid() }));
		}

		#endregion

		#endregion

	}
}
