using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.AMS.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(MessageSendingMovement))]
	class MessageSendingMovementTest : NonPersistentBusinessObjectTestCase
	{
		public void TestClickSendWillLockMutex()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = (CusInBondMoveHeader)header.MovementHeaders.AddNew();
			foreach (var actionCode in new[] { ActionCode.SubsequentInBondOriginal, ActionCode.SubsequentInBondAmendment })
			{
				var sendingObj = new MessageSendingMovement(moveHeader, actionCode);
				sendingObj.MM_Send = true;
				AssertEquals(true, moveHeader.InBondNumberAllocationMutexHasLock());
				sendingObj.MM_Send = false;
				AssertEquals(false, moveHeader.InBondNumberAllocationMutexHasLock());
			}
		}

		public void TestSendingObjectForMovementDetail()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = (CusInBondMoveHeader)header.MovementHeaders.AddNew();
			var bill0 = header.Bills.AddNew();
			bill0.B0_InBondPortOfDestDCode = "1101";
			var bill1 = header.Bills.AddNew();
			bill1.B0_InBondPortOfDestDCode = "1001";

			var sendMovement0 = new MessageSendingMovement(bill0.MovementDetail, ActionCode.VesselArrival);
			AssertNotNull(sendMovement0.FirstSendingObject);
			AssertEquals("1101", sendMovement0.FirstSendingObject.MB_PortOfUnlading);

			var sendMovement1 = new MessageSendingMovement(bill1.MovementDetail, ActionCode.VesselArrival);
			AssertNotNull(sendMovement1.FirstSendingObject);
			AssertEquals("1001", sendMovement1.FirstSendingObject.MB_PortOfUnlading);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<CusInBondHeader>();
			return new MessageSendingMovement(header.MovementHeader, ActionCode.InBondExportation);
		}

		protected override BusinessObject GetNewBusinessObjectForSettingValueCallsRefreshBindingTest()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			AssertNotNull(bill.MovementDetail);
			return new MessageSendingMovement(header.MovementHeader, ActionCode.VesselArrival);
		}
		#endregion
	}
}
