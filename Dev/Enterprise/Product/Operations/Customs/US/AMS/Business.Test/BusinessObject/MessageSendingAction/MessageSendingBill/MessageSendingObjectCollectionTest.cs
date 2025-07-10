using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(MessageSendingObjectCollection))]
	sealed class MessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<MessageSendingObjectCollection>
	{
		public void TestUnRegisterBillAsEditableChildObject()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill1 = header.Bills.AddNew();
			var moveDetail1 = bill1.MovementDetail;
			var sendigBill1 = new MessageSendingObject(moveDetail1, Messaging.Business.ActionCode.Creating);

			var bill2 = header.Bills.AddNew();
			var moveDetail2 = bill2.MovementDetail;
			var sendigBill2 = new MessageSendingObject(moveDetail2, Messaging.Business.ActionCode.Creating);

			var collection = new MessageSendingObjectCollection(Factory);
			collection.Add(sendigBill1);
			collection.Add(sendigBill2);
			AssertEquals(true, sendigBill1.IsRegisteredEditableChildObject(bill1));
			AssertEquals(true, sendigBill2.IsRegisteredEditableChildObject(bill2));

			collection.UnRegisterBillAsEditableChildObject();
			AssertEquals(false, sendigBill1.IsRegisteredEditableChildObject(bill1));
			AssertEquals(false, sendigBill2.IsRegisteredEditableChildObject(bill2));
		}

		protected override MessageSendingObjectCollection GetCollectionToTest()
		{
			return new MessageSendingObjectCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var bill = Header.Bills.AddNew();
			var moveDetail = MoveHeader.MovementDetails.AddNew();
			moveDetail.B9_B0 = bill.PK;
			return new MessageSendingObject(moveDetail, Messaging.Business.ActionCode.Creating);
		}

		CusInBondHeader Header
		{
			get { return header ?? (header = Factory.New<CusInBondHeader>()); }
		}
		CusInBondHeader header;

		CusInBondMoveHeader MoveHeader
		{
			get { return Header.MovementHeader; }
		}
	}
}
