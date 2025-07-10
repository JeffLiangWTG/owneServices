using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Universal.Testing
{
	class ZZRefCarrierAttributeCombinedLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestNameList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var parentGrouping = helper.CreateNewOrGetExistingDataGrouping("EUN", "EuropeanUnion");
			helper.CreateNewOrGetExistingDataGrouping("IT", "Italy", parentGrouping);
			Factory.Save();
			var carrierCode1 = Factory.New<RefCarrierCode>();
			carrierCode1.ZZ4_Code = "TestNameList1";
			carrierCode1.ZZ4_ZZZ_NKDataGrouping = "IT";
			carrierCode1.ZZ4_Description = "One";
			var attr1A = carrierCode1.Attributes.AddNew();
			attr1A.ZZG_Name = "TestNameA";
			attr1A.ZZG_Value = "aa";
			var carrierCode2 = Factory.New<RefCarrierCode>();
			carrierCode2.ZZ4_Code = "TestNameList2";
			carrierCode2.ZZ4_ZZZ_NKDataGrouping = "IT";
			carrierCode2.ZZ4_Description = "Two";
			var attr2A = carrierCode2.Attributes.AddNew();
			attr2A.ZZG_Name = "TestNameA";
			attr2A.ZZG_Value = "aa";
			var attr2B = carrierCode2.Attributes.AddNew();
			attr2B.ZZG_Name = "TestNameB";
			attr2B.ZZG_Value = "bb";
			Factory.Save();
			var carrier = Factory.New<ZZRefCarrierCombined>();
			carrier.ZZ4_Code = "TestNameList";
			carrier.ZZ4_CountryOrGrouping = ZString.Empty;
			carrier.ZZ4_Description = "Test Carrier";
			var attribute = carrier.Attributes.AddNew();
			AssertEquals("no attributes", 0, attribute.Lookups.NameList.Count);
			carrier.ZZ4_CountryOrGrouping = "AU";
			AssertEquals("no attributes for AU", 0, attribute.Lookups.NameList.Count);
			carrier.ZZ4_CountryOrGrouping = "IT";
			ISet<string> nameSet = new HashSet<string>();
			foreach (var name in attribute.Lookups.NameList)
			{
				nameSet.Add(name.ToString());
			}

			AssertEquals("Does not have 2 attributes for IT", 2, attribute.Lookups.NameList.Count);
			AssertEquals("Does not have attribute for IT", true, nameSet.Contains("TestNameA"));
			AssertEquals("Does not have attribute for IT", true, nameSet.Contains("TestNameB"));
		}
	}
}
