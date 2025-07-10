using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	[TestedType(typeof(MessageSendingObject))]
	sealed class MessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			Assert("ShouldSend should be true", sendingObj.ShouldSend);
			AssertEquals("Action should be default to New", ActionCodeList.Codes.New, sendingObj.Action);
		}

		public void TestActionList()
		{
			AssertType<ActionCodeList>(sendingObj.ActionList);
		}

		public void TestBillNumber()
		{
			bill.ABL_BillNumber = "123";
			AssertEquals("BillNumber should be map to bill.ABL_BillNumber", "123", sendingObj.BillNumber);
		}

		public void TestActionCaption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(sendingObj.ActionInfo);
			AssertEquals("Action Caption", "Action", resourceStringData.Caption);
		}

		public void TestBagNumber()
		{
			bill.BagNumber = "B1";
			AssertEquals("BagNumber should be map to bill.BagNumber", "B1", sendingObj.BagNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();
			bill = Factory.NewWithValidTestData<AsycudaBill>();
			sendingObj = new MessageSendingObject(bill);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var bill = Factory.NewWithValidTestData<AsycudaBill>();
			return new MessageSendingObject(bill);
		}

		AsycudaBill bill;
		MessageSendingObject sendingObj;
	}
}
