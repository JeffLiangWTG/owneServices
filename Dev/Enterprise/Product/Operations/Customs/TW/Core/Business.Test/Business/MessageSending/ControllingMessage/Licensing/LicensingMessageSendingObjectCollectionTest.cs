using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(LicensingMessageSendingObjectCollection))]
	sealed class LicensingMessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<LicensingMessageSendingObjectCollection>
	{
		protected override LicensingMessageSendingObjectCollection GetCollectionToTest()
		{
			return new LicensingMessageSendingObjectCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new LicensingMessageSendingObjectForTest(Factory.NewWithValidTestData<CusTWControllingMessageHeader>());
		}
	}
}
