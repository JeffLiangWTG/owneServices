using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.Universal;
using NUnit.Framework;
using NctsRefCusCodeListLevelTypes = Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListLevelTypes;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(NctsBillAdditionalDocumentLookups))]
sealed class NctsBillAdditionalDocumentLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestTypeCodeList_SubTypeTRA() => AssertTypeCodeList(AdditionalInfoSubTypeList.Codes.TransportDocument);

	public void TestTypeCodeList_SubTypeINF() => AssertTypeCodeList(AdditionalInfoSubTypeList.Codes.AdditionalInformation);

	public void TestTypeCodeList_SubTypeREF() => AssertTypeCodeList(AdditionalInfoSubTypeList.Codes.AdditionalReference);

	void AssertTypeCodeList(string subType)
	{
		additionalDocument.CSI_SubType = subType;
		var codeType = additionalDocument.GetCodeTypeBySubType();

		var helper = new UniversalReferenceTestDataHelper(Factory);
		var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Poland, "Poland", eun);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
		helper.CreateNewOrGetExistingCusCodeType(codeType, $"CusCodeType{codeType}");

		var refCusCodeList1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, codeType, codeType + "01", "01 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCodeList1.PK, RefCusCodeListAttributeTypes.Codes.Level, NctsRefCusCodeListLevelTypes.House);
		var refCusCodeList3 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Poland, codeType, codeType + "03", "03 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		var refCusCodeList4 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Poland, codeType, codeType + "04", "04 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCodeList4.PK, RefCusCodeListAttributeTypes.Codes.Level, NctsRefCusCodeListLevelTypes.Item);
		var refCusCodeList5 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, codeType, codeType + "INV01", "Invalid Country", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCodeList5.PK, RefCusCodeListAttributeTypes.Codes.Level, NctsRefCusCodeListLevelTypes.House);
		var refCusCodeList6 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Poland, "DC000", codeType + "INV02", "Invalid Type", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCodeList6.PK, RefCusCodeListAttributeTypes.Codes.Level, NctsRefCusCodeListLevelTypes.House);
		var refCusCodeList7 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Poland, codeType, codeType + "INV03", "Invalid StartDate", ZDateTime.Today.AddDays(2), ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCodeList7.PK, RefCusCodeListAttributeTypes.Codes.Level, NctsRefCusCodeListLevelTypes.House);
		var refCusCodeList8 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Poland, codeType, codeType + "INV04", "Invalid EndDate", ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddDays(-2));
		helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCodeList8.PK, RefCusCodeListAttributeTypes.Codes.Level, NctsRefCusCodeListLevelTypes.House);
		var refCusCodeList9 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Poland, codeType, codeType + "INV04", "Invalid EndDate", ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddDays(-2));
		Factory.Save();

		var typeCodesList = lookups.TypeCodeList;
		var completeFilter = typeCodesList.CompleteFilter;
		CombineAssertions(() =>
		{
			AssertType<ZZRefCusCodeListCombinedCollection>("List Type", typeCodesList);
			AssertSame("Cached", typeCodesList, lookups.TypeCodeList);
			AssertEquals("Matched EUN", true, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList1.PK).MatchesFilter(completeFilter));
			AssertEquals("No CusCodeListAttribute", true, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList3.PK).MatchesFilter(completeFilter));
			AssertEquals("Unmatched AttributeValue", true, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList4.PK).MatchesFilter(completeFilter));
			AssertEquals("Not PL", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList5.PK).MatchesFilter(completeFilter));
			AssertEquals("Unmatched CodeType", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList6.PK).MatchesFilter(completeFilter));
			AssertEquals("Unmatched StartDate", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList7.PK).MatchesFilter(completeFilter));
			AssertEquals("Unmatched EndDate", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList8.PK).MatchesFilter(completeFilter));
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		var bill = nctsHeader.Bills.AddNew();
		additionalDocument = bill.AdditionalDocuments.AddNew();
		lookups = new NctsBillAdditionalDocumentLookups(additionalDocument);
	}
	NctsHeader nctsHeader;
	NctsBillAdditionalDocument additionalDocument;
	NctsBillAdditionalDocumentLookups lookups;
}
