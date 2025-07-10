using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCarrierCodeAttributeCollection))]
	internal class RefCarrierCodeAttributeCollectionTest : ActiveBusinessObjectCollectionTestCase<RefCarrierCodeAttributeCollection>
	{
		public override void TestAddNew()
		{
			var carrier = Factory.New<RefCarrierCode>();
			var attribute = carrier.Attributes.AddNew("BOB", "B");
			AssertEquals("attribute.ZZE_ZXE_NKName", "BOB", attribute.ZZG_Name);
			AssertEquals("attribute.ZZE_Value", "B", attribute.ZZG_Value);
			AssertEquals("carrier.Attributes.Count", 1, carrier.Attributes.Count);
			var attribute2 = carrier.Attributes.AddNew();
			AssertEquals("carrier.Attributes.Count", 2, carrier.Attributes.Count);
			AssertCollectionContains(attribute2, carrier.Attributes);
		}

		protected override RefCarrierCodeAttributeCollection GetCollectionToTest()
		{
			return new RefCarrierCodeAttributeCollection(Factory.New<RefCarrierCode>());
		}
	}
}
