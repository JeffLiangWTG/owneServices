using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Testing;

public static class CusAuthorizationUsageTestHelper
{
	public static void AddAuthorizationUsageCodes(BusinessObjectFactory factory)
	{
		var helper = new UniversalReferenceTestDataHelper(factory);
		var grouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
		helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, null, grouping);
		const string codeType = EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_AUTH;
		helper.CreateNewOrGetExistingCusCodeType(codeType, "AuthorizationUsageCodes");
		helper.CreateNewOrGetExistingCusCodeType("0RAND", "RandomCodeType");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, codeType, "123", "description1", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, codeType, "321", "description2", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "0RAND", "888", "888 random desc", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		factory.Save();
	}
}
