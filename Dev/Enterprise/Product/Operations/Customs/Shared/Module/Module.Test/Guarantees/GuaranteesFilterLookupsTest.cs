using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Module.Testing
{
	public class GuaranteesFilterLookupsTest : TestCaseWithFactory
	{
		public void TestGuaranteeTypeList()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			var guaranteeTypesAU = new GuaranteesFilterStripBusinessObject().Lookups.GuaranteeTypeList;
			CombineAssertions(() =>
			{
				AssertEquals($"Count of Guarantee Types for {Core.Constants.CountryCodes.Australia} have to be 1", 1, guaranteeTypesAU.Count);
				AssertEquals($"The code of the first Guarantee Type for {Core.Constants.CountryCodes.Australia} have to be 'ZZZ_AU1'", "ZZZ_AU1", guaranteeTypesAU[0].Code);
				AssertEquals($"The description of the first Guarantee Type for {Core.Constants.CountryCodes.Australia} have to be 'Description for ZZZ_AU1'", "Description for ZZZ_AU1", guaranteeTypesAU[0].Description);
			});

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			var guaranteeTypesUS = new GuaranteesFilterStripBusinessObject().Lookups.GuaranteeTypeList;
			CombineAssertions(() =>
			{
				AssertEquals($"Count of Guarantee Types for {Core.Constants.CountryCodes.UnitedStates} have to be 2", 2, guaranteeTypesUS.Count);
				AssertEquals($"The code of the first Guarantee Type for {Core.Constants.CountryCodes.UnitedStates} have to be 'ZZZ_US1'", "ZZZ_US1", guaranteeTypesUS[0].Code);
				AssertEquals($"The description of the first Guarantee Type for {Core.Constants.CountryCodes.UnitedStates} have to be 'Description for ZZZ_US1'", "Description for ZZZ_US1", guaranteeTypesUS[0].Description);
				AssertEquals($"The code of the second Guarantee Type for {Core.Constants.CountryCodes.UnitedStates} have to be 'ZZZ_US2'", "ZZZ_US2", guaranteeTypesUS[1].Code);
				AssertEquals($"The description of the second Guarantee Type for {Core.Constants.CountryCodes.UnitedStates} have to be 'Description for ZZZ_US2'", "Description for ZZZ_US2", guaranteeTypesUS[1].Description);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GuaranteeType, "Guarantee Types");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GuaranteeType, "ZZZ_AU1", "Description for ZZZ_AU1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GuaranteeType, "ZZZ_US1", "Description for ZZZ_US1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GuaranteeType, "ZZZ_US2", "Description for ZZZ_US2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
		}
	}
}
