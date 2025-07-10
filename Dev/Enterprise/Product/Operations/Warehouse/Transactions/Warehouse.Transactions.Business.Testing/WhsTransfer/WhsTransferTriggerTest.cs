using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsTransferTriggerTest : WhsDocketTriggerTest<WhsTransfer>
	{
		#region TestTrigger_UnfinalisedTransferWithDifferentStatusLine

		public void TestTrigger_UnfinalisedTransferWithDifferentStatusLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, data.Whs1.DefaultLocation, data.Whs1.DefaultLocation);

			transfer.RunPreSaveValidation();
			AssertEquals("Precondition: Transfer Line is Entered", DocketLineStatus.Codes.Entered, transferLine.WE_DocketLineStatus);
			AssertNoExceptionThrown("Should not throw exception", () => Factory.Save());

			transferLine.PickedTime = ZDateTimeOffset.Now;
			transfer.RunPreSaveValidation();
			AssertEquals("Precondition: Transfer Line is HeldForTransfer", DocketLineStatus.Codes.HeldForTransfer, transferLine.WE_DocketLineStatus);
			AssertNoExceptionThrown("Should not throw exception", () => Factory.Save());

			transferLine.FinaliseDocketLine();
			transfer.RunPreSaveValidation();
			AssertEquals("Precondition: Transfer is not finalised", false, transfer.IsFinalised);
			AssertIsFinalisedPrecondition(transferLine);
			AssertNoExceptionThrown("Should not throw exception", () => Factory.Save());
		}

		#endregion

		#region Implementation

		protected override WhsTransfer GetNewDocket(OrgHeader client, WhsWarehouse warehouse)
		{
			return Helper.CreateWhsTransfer(client, warehouse);
		}

		protected override WhsDocketLine GetNewDocketLine(WhsTransfer docket, OrgSupplierPart part, WhsLocation location)
		{
			var uniqueReceiveRef = "REC" + docket.WD_ExternalReference + docket.Lines.Count;
			Helper.CreateWhsReceiveWithInventory(docket.Client, docket.Warehouse, uniqueReceiveRef, part, 10m);

			var transferLine = Helper.CreateWhsTransferLine(docket, part, 10m, location, location);
			transferLine.RunPreSaveValidation(); // to commit inventory

			return transferLine;
		}

		protected override bool CanBeCancelled => false;

		protected override ZString DefaultDocketLineStatus => "ENT";

		protected override string GetDocketSubTypeNotDefault() => TransferType.Codes.InterWhsSource;

		#endregion
	}
}
