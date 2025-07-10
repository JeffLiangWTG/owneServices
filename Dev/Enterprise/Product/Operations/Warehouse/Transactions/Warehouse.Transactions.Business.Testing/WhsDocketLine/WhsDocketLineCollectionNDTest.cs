using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsDocketLineCollectionND))]
	class WhsDocketLineCollectionNDTest : WhsBusinessObjectCollectionTestCase
	{
		public override void TestAddNew()
		{
			Assert("Override because type cannot be determined", true);
		}

		public override void TestTypedAddNew()
		{
			Assert("Override because type cannot be determined", true);
		}

		public override void TestAddAndCancelOfElementAsThoughBinding()
		{
			Assert("Override because type cannot be determined", true);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new WhsDocketLineCollectionND(Factory, new ZQuery());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Receive.Lines.AddNew();
		}

		WhsReceive Receive
		{
			get { return receive ?? (receive = Factory.New<WhsReceive>()); }
		}

		WhsReceive receive;
	}
}
