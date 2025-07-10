using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(ZZRefCusCodeListLanguageCombinedCollection))]
	class ZZRefCusCodeListLanguageCombinedCollectionTest : BusinessObjectCollectionTestCase
	{
		public override void TestAddNew()
		{
			var cusCodeList = Factory.New<ZZRefCusCodeListCombined>();
			AssertEquals("cusCodeList.Languages.AllowNew", true, cusCodeList.Languages.AllowNew);
			var language = cusCodeList.Languages.AddNew("DE", "DE Description");
			AssertEquals("language.ZZE_ZXE_NKName", "DE", language.ZXA_ZX6_NKLanguage);
			AssertEquals("language.ZZE_Value", "DE Description", language.ZXA_Description);
			AssertEquals("cusCodeList.Languages.Count", 1, cusCodeList.Languages.Count);
			var language2 = cusCodeList.Languages.AddNew();
			AssertEquals("cusCodeList.Languages.Count", 2, cusCodeList.Languages.Count);
			AssertCollectionContains(language2, cusCodeList.Languages);
			cusCodeList.ZZD_IsSystem = true;
			AssertEquals("cusCodeList.Languages.AllowNew", false, cusCodeList.Languages.AllowNew);
		}

		protected override Type GetExpectedCollectionType() => typeof(ZZRefCusCodeListLanguageCombinedCollection);

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ZZRefCusCodeListLanguageCombinedCollection(Factory.New<ZZRefCusCodeListCombined>());
		}
	}
}
