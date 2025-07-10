using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(ModuleShipmentCollection))]
	sealed class ModuleShipmentCollectionBOCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ModuleShipmentCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<CommonShipment>();
		}

		[ExpectNoExceptions]
		public void TestNullBusinessObject()
		{
			ModuleShipmentCollectionForTest testCollection = new ModuleShipmentCollectionForTest(Factory);
			testCollection.ParentConsol = Factory.New<CommonConsol>();
			testCollection.CallAddNotificationWhenAdditionalFilterNotMet(new StringCollectionX(), null);
		}
	}
}
