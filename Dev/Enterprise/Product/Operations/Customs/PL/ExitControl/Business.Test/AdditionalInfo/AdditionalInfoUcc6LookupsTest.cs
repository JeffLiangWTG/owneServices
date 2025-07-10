using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code;

namespace Enterprise.Customs.PL.ExitControl.Business.Testing;

sealed class AdditionalInfoUcc6LookupsTest : BusinessObjectLookupsTestCase
{
	public void TestSubTypeList()
	{
		var report = Factory.New<CusExitReport>();
		report.CER_Calc_Discrepancies = false;
		var lookups = new AdditionalInfoUcc6Lookups((AdditionalInfo)report.AdditionalInfos.AddNew());
		var subTypeList = lookups.SubTypeList;
		AssertContainsExactElementsInAnyOrder(
			[AdditionalInfoSubTypeList.Codes.AdditionalInformation],
			subTypeList.GetAllCodes());

		report.CER_Calc_Discrepancies = true;
		subTypeList = lookups.SubTypeList;
		AssertContainsExactElementsInAnyOrder(
			[AdditionalInfoSubTypeList.Codes.AdditionalInformation, AdditionalInfoSubTypeList.Codes.TransportDocument],
			subTypeList.GetAllCodes());
	}

	public void TestCodeList()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateCusCodeType(ExportAdditionalInformation, "Exit Additional Information");
		helper.CreateCusCodeList(Core.Constants.CountryCodes.Poland, ExportAdditionalInformation, "INF", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
		helper.CreateCusCodeList(Core.Constants.CountryCodes.Poland, ExportAdditionalInformation, "REF", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
		helper.CreateCusCodeList(Core.Constants.CountryCodes.Poland, ExportAdditionalInformation, "TRA", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
		Factory.Save();

		var additonalInfo = Factory.New<AdditionalInfo>();
		additonalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
		var lookups = new AdditionalInfoUcc6Lookups(additonalInfo);
		var codeList = (ZZRefCusCodeListCombinedCollection)lookups.CodeList;
		codeList.Load();
		AssertContainsExactElementsInAnyOrder(
			["INF", "REF", "TRA"],
			codeList.Select<string>(x => x.ZZD_Code));
	}
}
