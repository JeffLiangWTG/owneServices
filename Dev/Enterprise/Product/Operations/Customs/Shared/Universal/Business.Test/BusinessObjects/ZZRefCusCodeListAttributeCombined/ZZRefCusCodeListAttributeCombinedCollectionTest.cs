using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(ZZRefCusCodeListAttributeCombinedCollection))]
	class ZZRefCusCodeListAttributeCombinedCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestHasAttribute()
		{
			var cusCodeList = Factory.New<ZZRefCusCodeListCombined>();
			var attribute = cusCodeList.Attributes.AddNew("BOB", "B");
			AssertEquals(true, cusCodeList.Attributes.HasAttribute("BOB"));
			AssertEquals(false, cusCodeList.Attributes.HasAttribute("JACK"));
			AssertEquals(false, cusCodeList.Attributes.HasAttribute("BOB", "A"));
			AssertEquals(true, cusCodeList.Attributes.HasAttribute("BOB", "B"));
			AssertEquals(false, cusCodeList.Attributes.HasAttribute("JACK", "B"));
		}

		public void TestGetAttribute()
		{
			var cusCodeList = Factory.New<ZZRefCusCodeListCombined>();
			cusCodeList.Attributes.AddNew("BOB", "B");
			var attribute = cusCodeList.Attributes.GetAttributeValue("BOB");
			AssertEquals("B", attribute);
			attribute = cusCodeList.Attributes.GetAttributeValue("BOC");
			AssertEquals(ZString.Empty, attribute);
		}

		public override void TestAddNew()
		{
			var cusCodeList = Factory.New<ZZRefCusCodeListCombined>();
			AssertEquals("cusCodeList.Attributes.AllowNew", true, cusCodeList.Attributes.AllowNew);
			var attribute = cusCodeList.Attributes.AddNew("BOB", "B");
			AssertEquals("attribute.ZZE_ZXE_NKName", "BOB", attribute.ZZE_ZXE_NKName);
			AssertEquals("attribute.ZZE_Value", "B", attribute.ZZE_Value);
			AssertEquals("cusCodeList.Attributes.Count", 1, cusCodeList.Attributes.Count);
			var attribute2 = cusCodeList.Attributes.AddNew();
			AssertEquals("cusCodeList.Attributes.Count", 2, cusCodeList.Attributes.Count);
			AssertCollectionContains(attribute2, cusCodeList.Attributes);
			cusCodeList.ZZD_IsSystem = true;
			AssertEquals("cusCodeList.Attributes.AllowNew", false, cusCodeList.Attributes.AllowNew);
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(ZZRefCusCodeListAttributeCombinedCollection);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ZZRefCusCodeListAttributeCombinedCollection(Factory.New<ZZRefCusCodeListCombined>());
		}
	}
}
