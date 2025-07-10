using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NCATKMessageSendingObject))]
	sealed class NCATKMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new NCATKMessageSendingObject(Factory.NewWithValidTestData<CusTWControllingMessageHeader>());
		}

		protected override void SetUp()
		{
			base.SetUp();
			new TestTWCreator(Factory).CreateRefCusCodeForControllingMessageType();
		}
	}
}
