using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(ZZRefCarrierCombinedCollection))]
	class ZZRefCarrierCombinedCollectionTest : ActiveBusinessObjectCollectionTestCase<ZZRefCarrierCombinedCollection>
	{
		protected override ZZRefCarrierCombinedCollection GetCollectionToTest()
		{
			return new ZZRefCarrierCombinedCollection(Factory, CountryCodes.EastTimor);
		}

		public void TestSetDefaultsForNewElement()
		{
			RefCarrierHelperTest.SetupMockRefCarrierConfig();
			var collection = new ZZRefCarrierCombinedCollection(Factory, CountryCodes.SouthAfrica, RefCarrierAttributeNames.MASTER, TransportModes.Sea);
			var carrier = collection.AddNew();
			AssertEquals("Has matching RefCarrierConfig", 1, carrier.Attributes.Count);
			AssertEquals(RefCarrierAttributeNames.MASTER, carrier.Attributes[0].ZZG_Name);
			AssertEquals(RefCarrierAttributeNames.MASTER, carrier.Attributes[0].ZZG_Value);

			collection = new ZZRefCarrierCombinedCollection(Factory, CountryCodes.SouthAfrica);
			carrier = collection.AddNew();
			AssertEquals("No carrier type provided", 0, carrier.Attributes.Count);

			collection = new ZZRefCarrierCombinedCollection(Factory, CountryCodes.France, RefCarrierAttributeNames.MASTER, TransportModes.Sea);
			carrier = collection.AddNew();
			AssertEquals("No matching RefCarrierConfig", 0, carrier.Attributes.Count);
		}
	}
}
