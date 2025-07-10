using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Universal.Testing
{
	class ZZRefCarrierAttributeCombinedValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZZG_Name()
		{
			var attribute = carrier.Attributes.AddNew();
			attribute.ZZG_Name = "name";
			AssertHasErrorContaining(attribute.ZZG_NameInfo, ListValidation.InvalidCodeError);
			attribute.ZZG_Name = "TestNameA";
			AssertNoErrorContaining(attribute.ZZG_NameInfo, ListValidation.InvalidCodeError);
			carrier.ZZ4_CountryOrGrouping = "AU";
			attribute.Validation.ValidateZZG_Name();
			AssertHasErrorContaining(attribute.ZZG_NameInfo, ListValidation.InvalidCodeError);
			attribute.ZZG_Name = "";
			AssertNoErrorContaining(attribute.ZZG_NameInfo, ListValidation.InvalidCodeError);
			carrier.ZZ4_CountryOrGrouping = "IT";
			attribute.ZZG_Name = "TestNameA";
			AssertNoErrors(attribute.ZZG_NameInfo);
			var attribute2 = carrier.Attributes.AddNew();
			attribute2.ZZG_Name = "TestNameA";
			AssertHasErrorContaining(attribute2.ZZG_NameInfo, "Duplicate attribute name 'TestNameA' is not allowed.");
			attribute2.ZZG_Name = "TestNameB";
			AssertNoErrorContaining(attribute2.ZZG_NameInfo, "Duplicate attribute name 'TestNameA' is not allowed.");
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var parentGrouping = helper.CreateNewOrGetExistingDataGrouping("EUN", "EuropeanUnion");
			helper.CreateNewOrGetExistingDataGrouping("IT", "Italy", parentGrouping);
			Factory.Save();
			var carrierCode = Factory.New<RefCarrierCode>();
			carrierCode.ZZ4_Code = "TestNameList1";
			carrierCode.ZZ4_ZZZ_NKDataGrouping = "IT";
			carrierCode.ZZ4_Description = "One";
			var attr1A = carrierCode.Attributes.AddNew();
			attr1A.ZZG_Name = "TestNameA";
			attr1A.ZZG_Value = "aa";
			Factory.Save();
			carrier = Factory.New<ZZRefCarrierCombined>();
			carrier.ZZ4_Code = "carrier";
			carrier.ZZ4_CountryOrGrouping = "IT";
			carrier.ZZ4_Description = "carrier1";
		}

		ZZRefCarrierCombined carrier;
	}
}
