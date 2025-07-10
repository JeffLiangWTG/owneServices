using System;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(ZZRefCarrierAttributeCombinedCollection))]
	class ZZRefCarrierAttributeCombinedCollectionTest : ActiveBusinessObjectCollectionTestCase<ZZRefCarrierAttributeCombinedCollection>
	{
		public override void TestAddNew()
		{
			var carrier = Factory.New<ZZRefCarrierCombined>();
			var attribute = carrier.Attributes.AddNew("BOB", "B");
			AssertEquals("attribute.ZZG_Name", "BOB", attribute.ZZG_Name);
			AssertEquals("attribute.ZZG_Value", "B", attribute.ZZG_Value);
			AssertEquals("carrier.Attributes.Count", 1, carrier.Attributes.Count);
			var attribute2 = carrier.Attributes.AddNew();
			AssertEquals("carrier.Attributes.Count", 2, carrier.Attributes.Count);
			AssertCollectionContains(attribute2, carrier.Attributes);
		}

		public void TestAllowNewForSystem()
		{
			var carrier = Factory.New<ZZRefCarrierCombined>();
			var allowNewProperty = typeof(ZZRefCarrierAttributeCombinedCollection).GetProperty("AllowNew", BindingFlags.Instance | BindingFlags.NonPublic);
			var allowNew = allowNewProperty.GetValue(carrier.Attributes, Array.Empty<object>());
			Assert((bool)allowNew);
			carrier.ZZ4_IsSystem = true;
			allowNew = allowNewProperty.GetValue(carrier.Attributes, Array.Empty<object>());
			Assert(!(bool)allowNew);
		}

		protected override ZZRefCarrierAttributeCombinedCollection GetCollectionToTest()
		{
			return new ZZRefCarrierAttributeCombinedCollection(Factory.New<ZZRefCarrierCombined>());
		}
	}
}
