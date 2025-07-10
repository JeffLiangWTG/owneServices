using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class ReceiveLinesUpdaterForUnloadTestTest : WhsSecureServiceTestCase
	{
		public void TestReceiveLinesUpdaterForUnload_ThrowsExceptionWithEmptyReceiveLineCollection()
		{
			IReceiveLinesUpdater receiveLinesUpdater = new ReceiveLinesUpdaterForUnload();
			AssertExceptionThrown<ArgumentException>("Receive Lines Updater does not accept an empty list of receive lines.",
				() => receiveLinesUpdater.UpdateReceiveLinesAndReturnExcessUnloadQty(null, Enumerable.Empty<WhsReceiveLine>().ToList(), 0, "UNT", ZGuid.Empty, string.Empty, false));
		}

		public void TestReceiveLineUpdaterForUnloadWithEmptyPalletIdMatching_ThrowsExceptionWithEmptyReceiveLineCollection()
		{
			IReceiveLinesUpdater receiveLinesUpdater = new ReceiveLineUpdaterForUnloadWithEmptyPalletIdMatching();
			AssertExceptionThrown<ArgumentException>("Receive Lines Updater does not accept an empty list of receive lines.",
				() => receiveLinesUpdater.UpdateReceiveLinesAndReturnExcessUnloadQty(null, Enumerable.Empty<WhsReceiveLine>().ToList(), 0, "UNT", ZGuid.Empty, string.Empty, false));
		}

		public void TestReceiveLineUpdaterForUnloadWithEmptyPalletIdMatching_ThrowsExceptionWithInvalidReceiveLineInCollection()
		{
			IReceiveLinesUpdater receiveLinesUpdater = new ReceiveLineUpdaterForUnloadWithEmptyPalletIdMatching();

			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 2);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = (WhsReceiveLine)Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 0m).InDocketLine;

			var receiveLineList = new List<WhsReceiveLine>();
			receiveLineList.Add(receiveLine);
			AssertEquals("Precondition: receive line has no expected quantity.", 0m, receiveLine.WE_ClientOrderedUnits);

			AssertExceptionThrown<ArgumentException>("Receive Lines Updater with empty pallet id matching does not accept an receive line with no expected quantity.",
				() => receiveLinesUpdater.UpdateReceiveLinesAndReturnExcessUnloadQty(null, receiveLineList, 0, "UNT", ZGuid.Empty, string.Empty, false));

			receiveLine.WE_ClientOrderedUnits = 10m;
			receiveLine.WE_TransactionQuantity = 10m;
			AssertEquals("Precondition: receive line has expected quantity.", 10m, receiveLine.WE_ClientOrderedUnits);
			AssertEquals("Precondition: receive line has transaction quantity.", 10m, receiveLine.WE_TransactionQuantity);

			AssertExceptionThrown<ArgumentException>("Receive Lines Updater with empty pallet id matching does not accept an receive line with transaction quantity.",
				() => receiveLinesUpdater.UpdateReceiveLinesAndReturnExcessUnloadQty(null, receiveLineList, 0, "UNT", ZGuid.Empty, string.Empty, false));
		}
	}
}
