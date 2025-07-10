using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

class GuaranteeBondDetailLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestBondTypeList()
	{
		var lookups = new GuaranteeBondDetailLookups(Factory.New<GuaranteeBondDetail>());
		var list = lookups.BondTypeList;
		CombineAssertions(() =>
		{
			AssertEquals("CodesAsString", "CON, STB", list.CodesAsString);
			AssertSame("Cached", lookups.BondTypeList, list);
		});
	}

	public void TestGuaranteeCollection()
	{
		var importer = Factory.NewWithValidTestData<OrgHeader>();

		CreateBaseCusGuaranteeHeader(PLGuaranteeTypeList.Codes.TRA, "number1", "1", CountryCodes.Poland, importer);
		CreateBaseCusGuaranteeHeader(PLGuaranteeTypeList.Codes.TRA, "number2", "2", CountryCodes.Poland, importer);
		CreateBaseCusGuaranteeHeader(PLGuaranteeTypeList.Codes.GEN, "number3", "2", CountryCodes.Poland, importer);
		CreateBaseCusGuaranteeHeader(PLGuaranteeTypeList.Codes.GEN, "number4", "1", CountryCodes.Poland, importer);
		CreateBaseCusGuaranteeHeader("SMT", "number5", "1", CountryCodes.Poland, importer);

		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		var entry = declaration.CustomsEntryInstructions.AddNew();
		var guarantee = entry.Guarantees.AddNew();

		var list = guarantee.Lookups.GuaranteeCollection;
		AssertEquals(2, list.Count);
	}

	BaseCusGuaranteeHeader CreateBaseCusGuaranteeHeader(ZString type, ZString number, ZString subType, ZString countryCode, OrgHeader orgHeader)
	{
		var result = Factory.NewWithValidTestData<CusGuaranteeHeader>();
		result.CPH_Type = type;
		result.CPH_Number = number;
		result.CPH_OH_PermitHolder = orgHeader.PK;
		result.CPH_RN_NKCountryCode = countryCode;
		result.CPH_SubType = subType;
		result.CPH_StartDate = ZDate.Today.AddDays(-1);
		result.CPH_EndDate = ZDate.Today.AddDays(1);
		return result;
	}
}
