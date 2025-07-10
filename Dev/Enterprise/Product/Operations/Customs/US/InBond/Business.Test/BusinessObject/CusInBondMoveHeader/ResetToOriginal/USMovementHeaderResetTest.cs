using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.US;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(USMovementHeaderReset))]
	sealed class USMovementHeaderResetTest : NonPersistentBusinessObjectTestCase
	{
		public void TestResetMovementHeadersToOriginal()
		{
			var moveheader = Header.MovementHeaders.AddNew();
			moveheader.AllocateInBondNumber("1");
			var bill = Header.Bills.AddNew();
			bill.B0_MasterBillNumber = "Bill1";
			var moveDetail = Header.MovementDetails.AddNew();
			moveDetail.B9_B0 = bill.PK;
			moveDetail.B9_BM = moveheader.PK;
			var container = moveDetail.Containers.AddNew();
			container.BC_ContainerNum = "Container1";
			var sendingObj = new InBondMessageSendingObject(moveheader, bill, container, InBondMessageType.ContainerLevelArrival);
			sendingObj.Send();
			StmALog log1 = container.Logs.AddNew(Events.MessageStatusChange, ImportMessageStatusList.Codes.AwaitingDepartureOriginal);
			var statusLogs = new LogsForNominatedEvent(container.Logs, Events.MessageStatusChange);
			USMovementHeaderReset moveHeaderReset = new USMovementHeaderReset(container, Collection);
			AssertEquals("Bill1", moveHeaderReset.RO_BillNumber);
			AssertEquals("Container1", moveHeaderReset.RO_ContainerNumber);
			AssertEquals("Containers", moveHeaderReset.RO_Level);
			moveHeaderReset.RO_ResetToOriginal = true;
			moveHeaderReset.RO_ResetReason = "";
			AssertHasErrorContaining(moveHeaderReset.RO_ResetReasonInfo, USMovementHeaderResetValidation.enterResettingReason);
			moveHeaderReset.RO_ResetReason = "dfdsgfs";
			AssertNoErrorContaining(moveHeaderReset.RO_ResetReasonInfo, USMovementHeaderResetValidation.enterResettingReason);
			AssertEquals(1, statusLogs.Count);
			moveHeaderReset.ResetToOriginal();
			AssertEquals("", (container as IResetToOriginal).CustomsStatus);
			AssertEquals(0, statusLogs.Count);
			var logs = container.Logs.GetAllLogs();
			AssertEquals(true, logs[0].IsCancelled);
			AssertEquals("RST", logs[1].SL_SE_NKEvent);
			AssertEquals("DCD", container.Messages[0].EM_Status);
		}

		protected override BusinessObject GetNewBusinessObject() => new USMovementHeaderReset(Header.MovementHeaders.AddNew(), Collection);

		CusInBondHeader header;
		CusInBondHeader Header => header ?? (header = Factory.New<CusInBondHeader>());

		USMovementHeaderResetCollection collection;
		USMovementHeaderResetCollection Collection => collection ?? (collection = new USMovementHeaderResetCollection(Header));
	}
}
