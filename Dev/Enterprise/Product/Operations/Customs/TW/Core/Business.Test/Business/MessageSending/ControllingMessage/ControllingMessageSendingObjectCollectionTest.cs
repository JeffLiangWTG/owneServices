using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(ControllingMessageSendingObjectCollection))]
	sealed class ControllingMessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ControllingMessageSendingObjectCollection>
	{
		[ExpectNoExceptions]
		public void TestAllowNew()
		{
			NUnit.Framework.Assert.That(!GetCollectionToTest().AllowNew, NUnit.Framework.Is.True);
		}

		protected override ControllingMessageSendingObjectCollection GetCollectionToTest()
		{
			return new ControllingMessageSendingObjectCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ControllingMessageSendingObject(Factory.NewWithValidTestData<CusTWControllingMessageHeader>());
		}
	}
}
