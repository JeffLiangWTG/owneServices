using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Services.OperationalActions.Support.Testing;

namespace Enterprise.Customs.US.InBond.Business.OperationalActions.Testing
{
	public class OperationalActionBulkDepartureAddPerMovementMessageSenderTest : TestCaseWithFactory
	{
		public void TestSendWhenThereAreErrorsOnInBond()
		{
			var inBondHeader = Factory.New<CusInBondHeader>();
			inBondHeader.BH_JobReference = "INB00001";
			inBondHeader.BH_ImportTransportMode = TransportModeCodes.Codes.TruckNonContainer;
			var bill = inBondHeader.Bills.AddNew();
			bill.B0_MasterBillNumber = "MB1";
			var moveHeader = inBondHeader.MovementHeader;
			moveHeader.InBondNumber = "693548231";
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var container1 = moveDetail.Containers.AddNew();
			container1.BC_ContainerNum = "12345";
			var container2 = moveDetail.Containers.AddNew();
			container2.BC_ContainerNum = "12345";
			Factory.Save();

			var movement = Factory.Load<USInBondMoveHeader>(moveHeader.PK);
			var log = new DummyOperationalActionSectionLog();
			var sender = new OperationalActionBulkDepartureAddPerMovementMessageSender(movement);

			sender.OperationalActionSendMessage(true, log);
			AssertContains("Please fix following errors before sending Departure Add message.", log.MessagesString());

			container2.BC_ContainerNum = "123456";
			Factory.Save();

			log = new DummyOperationalActionSectionLog();
			sender = new OperationalActionBulkDepartureAddPerMovementMessageSender(movement);
			sender.OperationalActionSendMessage(true, log);
			AssertNotContains("Please fix following errors before sending Departure Add message.", log.MessagesString());

			moveHeader.Messages.Reload(true);
			AssertEquals(1, moveHeader.Messages.Count);
			Assert("Mutex is unlocked", !inBondHeader.IsSendCustomsMessageMutexLocked);
		}

		public void TestSendWhenThereAreMessageErrorsOnInBond()
		{
			var inBondHeader = Factory.New<CusInBondHeader>();
			inBondHeader.BH_JobReference = "INB00001";
			inBondHeader.BH_ImportTransportMode = TransportModeCodes.Codes.TruckNonContainer;
			var bill = inBondHeader.Bills.AddNew();
			bill.B0_MasterBillNumber = "MB1";
			var moveHeader = inBondHeader.MovementHeader;
			moveHeader.InBondNumber = "693548232";
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var container1 = moveDetail.Containers.AddNew();
			container1.BC_ContainerNum = "12345";
			Factory.Save();

			var movement = Factory.Load<USInBondMoveHeader>(moveHeader.PK);
			var log = new DummyOperationalActionSectionLog();
			var sender = new OperationalActionBulkDepartureAddPerMovementMessageSender(movement);

			sender.OperationalActionSendMessage(false, log);
			AssertContains("Cannot send the Departure Add message because job has message errors.", log.MessagesString());

			log = new DummyOperationalActionSectionLog();
			sender = new OperationalActionBulkDepartureAddPerMovementMessageSender(movement);
			sender.OperationalActionSendMessage(true, log);
			AssertNotContains("Cannot send the Departure Add message because job has message errors.", log.MessagesString());

			moveHeader.Messages.Reload(true);
			AssertEquals(1, moveHeader.Messages.Count);
			Assert("Mutex is unlocked", !inBondHeader.IsSendCustomsMessageMutexLocked);
		}

		public void TestSendWhenThereIsNoValidMoveHeaderOnInBond()
		{
			var inBondHeader = Factory.New<CusInBondHeader>();
			inBondHeader.BH_JobReference = "INB00001";
			inBondHeader.BH_ImportTransportMode = TransportModeCodes.Codes.TruckNonContainer;
			var bill = inBondHeader.Bills.AddNew();
			bill.B0_MasterBillNumber = "MB1";
			var moveHeader = inBondHeader.MovementHeader;
			moveHeader.InBondNumber = "693548232";
			moveHeader.BM_CustomsStatus = ImportMessageStatusList.Codes.AwaitingDepartureOriginal;
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var container1 = moveDetail.Containers.AddNew();
			container1.BC_ContainerNum = "12345";
			Factory.Save();

			var movement = Factory.Load<USInBondMoveHeader>(moveHeader.PK);
			var log = new DummyOperationalActionSectionLog();
			var sender = new OperationalActionBulkDepartureAddPerMovementMessageSender(movement);
			sender.OperationalActionSendMessage(true, log);
			AssertContains("This In-Bond Movement is not available for sending a 'Departure Add' message to Customs.", log.MessagesString());

			moveHeader.BM_CustomsStatus = ZString.Empty;
			Factory.Save();

			log = new DummyOperationalActionSectionLog();
			sender = new OperationalActionBulkDepartureAddPerMovementMessageSender(movement);

			sender.OperationalActionSendMessage(true, log);
			AssertNotContains("This In-Bond Movement is not available for sending a 'Departure Add' message to Customs.", log.MessagesString());

			moveHeader.Messages.Reload(true);
			AssertEquals(1, moveHeader.Messages.Count);
			Assert("Mutex is unlocked", !inBondHeader.IsSendCustomsMessageMutexLocked);
		}
	}
}
