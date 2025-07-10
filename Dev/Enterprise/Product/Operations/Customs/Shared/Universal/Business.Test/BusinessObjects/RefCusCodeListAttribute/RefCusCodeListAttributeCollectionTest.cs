using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusCodeListAttributeCollection))]
	class RefCusCodeListAttributeCollectionTest : ActiveBusinessObjectCollectionTestCase<RefCusCodeListAttributeCollection>
	{
		public override void TestAddNew()
		{
			var cusCodeList = Factory.New<RefCusCodeList>();
			var attribute = cusCodeList.Attributes.AddNew("BOB", "B");
			AssertEquals("attribute.ZZE_ZXE_NKName", "BOB", attribute.ZZE_ZXE_NKName);
			AssertEquals("attribute.ZZE_Value", "B", attribute.ZZE_Value);
			AssertEquals("cusCodeList.Attributes.Count", 1, cusCodeList.Attributes.Count);
			var attribute2 = cusCodeList.Attributes.AddNew();
			AssertEquals("cusCodeList.Attributes.Count", 2, cusCodeList.Attributes.Count);
			AssertCollectionContains(attribute2, cusCodeList.Attributes);
			var attribute3 = cusCodeList.Attributes.AddNew("BOB", "B");
			AssertEquals("cusCodeList.Attributes.Count", 2, cusCodeList.Attributes.Count);
			AssertEquals(attribute, attribute3);
		}

		protected override RefCusCodeListAttributeCollection GetCollectionToTest()
		{
			return new RefCusCodeListAttributeCollection(Factory.New<RefCusCodeList>());
		}
	}
}
