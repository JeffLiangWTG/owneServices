using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(TransferUnAssignAllLinesActionMethodApplicator))]
	public class TransferUnAssignAllLinesActionMethodApplicatorTest : UnAssignAllLinesActionMethodApplicatorTest<TransferUnAssignAllLinesActionMethodApplicator, WhsTransfer>
	{
		protected override IEnumerable<WhsPickLine> OperationalActionSampleData()
		{
			option = AssignLineOptions.PickOnly;

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var loc1 = data.Whs1.FindLocation("A-1");
			var loc2 = data.Whs1.FindLocation("A-2");

			var notify = new TestNotificationBuffer();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive", notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, loc1);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 5m, loc1);

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();

			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			transfer.Option = AssignLineOptions.PickOnly;
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, loc1, loc2);
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part2, 5m, loc1, loc2);

			Helper.CreateWhsPickLine(transferLine1, inventory1, 10m);
			Helper.CreateWhsPickLine(transferLine2, inventory2, 5m);

			Factory.Save();

			var pickLines = new List<WhsPickLine>();

			pickLines.AddRange(transferLine1.PickLines);
			pickLines.AddRange(transferLine2.PickLines);

			transfer.WD_DocketStatus = DocketStatus.Codes.Entered;

			foreach (var line in pickLines)
			{
				line.WZ_GS_NKAssignedTo = SelectedUser.GS_Code;
			}

			Factory.Save();

			CreatedDocket = transfer;

			return pickLines;
		}

		protected override string OperationName => "Transfer";

		protected override string JobNo => CreatedDocket.WD_DocketID;

		protected override string FinaliseStatus => DocketStatus.Codes.Finalised;

		protected override string CancelledStatus => DocketStatus.Codes.Cancelled;

		protected override void SetDocketStatus(string status)
		{
			if (status == DocketStatus.Codes.Finalised)
			{
				CreatedDocket.WD_FinalisedDate = ZDateTimeOffset.Now;
			}
			else
			{
				CreatedDocket.WD_DocketStatus = status;
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var applicator = (TransferUnAssignAllLinesActionMethodApplicator)base.GetNewBusinessObject();
			applicator.Option = option;
			return applicator;
		}

		AssignLineOptions option;

		protected override TransferUnAssignAllLinesActionMethodApplicator GetNewApplicator()
		{
			return new TransferUnAssignAllLinesActionMethodApplicator(Factory);
		}
	}
}
