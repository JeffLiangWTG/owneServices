using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(TranshipmentMessageSendingObjectCollection))]
	sealed class TranshipmentMessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TranshipmentMessageSendingObjectCollection>
	{
		[ExpectNoExceptions]
		public void TestAllowNew()
		{
			NUnit.Framework.Assert.That(!GetCollectionToTest().AllowNew, NUnit.Framework.Is.True);
		}

		protected override TranshipmentMessageSendingObjectCollection GetCollectionToTest()
		{
			return new TranshipmentMessageSendingObjectCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new TranshipmentMessageSendingObject(Factory.NewWithValidTestData<CusInBondHeader>(), "");
		}
	}
}
