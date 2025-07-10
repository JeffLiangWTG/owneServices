using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(MessageSendingMovementCollection))]
	class MessageSendingMovementCollectionTest : NonPersistentBusinessObjectCollectionTestCase<MessageSendingMovementCollection>
	{
		public void TestReleaseAllInBondNumberMutex()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader1 = header.InBondMovementHeaders.AddNew();
			var moveHeader2 = header.InBondMovementHeaders.AddNew();
			var moveHeader3 = header.InBondMovementHeaders.AddNew();
			var sendingObjectCollection = new MessageSendingMovementCollection(Factory);
			sendingObjectCollection.Add(new MessageSendingMovement(moveHeader1, Messaging.Business.ActionCode.SubsequentInBondAmendment));
			sendingObjectCollection.Add(new MessageSendingMovement(moveHeader2, Messaging.Business.ActionCode.SubsequentInBondAmendment));
			sendingObjectCollection.Add(new MessageSendingMovement(moveHeader3, Messaging.Business.ActionCode.SubsequentInBondOriginal));
			moveHeader1.LockInBondNumberAllocationMutex();
			AssertEquals(true, moveHeader1.InBondNumberAllocationMutexHasLock());
			AssertEquals(false, moveHeader2.InBondNumberAllocationMutexHasLock());
			moveHeader3.LockInBondNumberAllocationMutex();
			AssertEquals(true, moveHeader3.InBondNumberAllocationMutexHasLock());
			sendingObjectCollection.ReleaseAllInBondNumberMutex();
			AssertEquals(false, moveHeader1.InBondNumberAllocationMutexHasLock());
			AssertEquals(false, moveHeader2.InBondNumberAllocationMutexHasLock());
			AssertEquals(false, moveHeader3.InBondNumberAllocationMutexHasLock());
		}

		#region Implementation

		protected override MessageSendingMovementCollection GetCollectionToTest()
		{
			return new MessageSendingMovementCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new MessageSendingMovement(Header.PTTMovements.AddNew(), Enterprise.Customs.US.AMS.Messaging.Business.ActionCode.VesselArrival);
		}

		CusInBondHeader Header
		{
			get { return header ?? (header = Factory.New<CusInBondHeader>()); }
		}
		CusInBondHeader header;

		#endregion
	}
}
