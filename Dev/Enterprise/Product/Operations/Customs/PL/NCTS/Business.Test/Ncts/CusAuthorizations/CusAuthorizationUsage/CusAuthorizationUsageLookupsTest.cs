using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(CusAuthorizationUsage))]
sealed class CusAuthorizationUsageLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestAuthorisationRuleList()
	{
		AssertType<CusAuthorisationRuleCollection>(lookups.AuthorisationRuleList);
	}

	public void TestAuthorisationRule_FilterBusinessObjectDefaults()
	{
		var authorisationRuleList = cusAuthorizationUsage.Lookups.AuthorisationRuleList;

		CombineAssertions(() =>
		{
			AssertEquals("Number", "Number", authorisationRuleList.FilterBusinessObjectDefaults[GetFilterDefaultKey(CusAuthorisationRuleCollection.FilterConstants.AuthorizationNumber)].Value);
			AssertEquals("Holder", orgHeader.PK, authorisationRuleList.FilterBusinessObjectDefaults[GetFilterDefaultKey(CusAuthorisationRuleCollection.FilterConstants.AuthorizationHolder)].Value);
			AssertEquals("Type", "ACE", authorisationRuleList.FilterBusinessObjectDefaults[GetFilterDefaultKey(CusAuthorisationRuleCollection.FilterConstants.AuthorizationType)].Value);
			var countryFilterBusinessObjectDefault = authorisationRuleList.FilterBusinessObjectDefaults[GetFilterDefaultKey(CusAuthorisationRuleCollection.FilterConstants.Country)];
			AssertEquals("Country Value", cusAuthorizationUsage.Header.CountryCode, countryFilterBusinessObjectDefault.Value);
			AssertEquals("Country Value", false, countryFilterBusinessObjectDefault.IsRemovable);
		});

		ZString GetFilterDefaultKey(string filter) => FilterBusinessObjectDefault.Create(filter, "Property").Key;
	}

	public void TestCodeList_AddCustomsValue_Departure()
	{
		var newFactory = new BusinessObjectFactory();
		var helper = new UniversalReferenceTestDataHelper(newFactory);
		var eunau = EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU;
		helper.CreateCusMapType(eunau, MapDirectionList.Codes.OUT, "EUNAU", true);
		helper.CreateCusMap(eunau, "ACR", "C521", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		helper.CreateCusMap(eunau, "SSE", "C523", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		helper.CreateCusMap(eunau, "TRD", "C524", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		newFactory.Save();

		var header = newFactory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		var usage = header.MovementHeader.CusAuthorizationUsages.AddNew();
		var lookups = usage.Lookups;

		var codes = (CodeDescriptionPairList)lookups.CodeList;
		CombineAssertions(() =>
		{
			AssertEquals("CodesAsString", "ACR, SSE, TRD", codes.CodesAsString);
			AssertEquals("First Description", "C521 - Authorized Consignor Transit", codes.GetDescriptionFromCode("ACR"));
			AssertEquals("Second Description", "C523 - Special Seals", codes.GetDescriptionFromCode("SSE"));
			AssertEquals("Third Description", "C524 - Transit Reduced Dataset", codes.GetDescriptionFromCode("TRD"));
		});
	}

	public void TestCodeList_AddCustomsValue_Arrival()
	{
		var newFactory = new BusinessObjectFactory();
		var helper = new UniversalReferenceTestDataHelper(newFactory);
		var eunau = EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU;
		helper.CreateCusMapType(eunau, MapDirectionList.Codes.OUT, "EUNAU", true);
		helper.CreateCusMap(eunau, "ACE", "C522", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		helper.CreateCusMap(eunau, "ACT", "C520", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		newFactory.Save();

		var header = newFactory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Arrival);
		var usage = header.CusAuthorizationUsages.AddNew();
		var lookups = usage.Lookups;

		var codes = (CodeDescriptionPairList)lookups.CodeList;
		CombineAssertions(() =>
		{
			AssertEquals("CodesAsString", "ACE, ACT", codes.CodesAsString);
			AssertEquals("First Description", "C522 - Authorization for the status of authorized consignee for Union transit", codes.GetDescriptionFromCode("ACE"));
			AssertEquals("Second Description", "C520 - Authorization for the status of authorized consignee for TIR procedure", codes.GetDescriptionFromCode("ACT"));
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		var nctsHeader = Factory.New<NctsHeader>();
		cusAuthorizationUsage = nctsHeader.CusAuthorizationUsages.AddNew();
		cusAuthorizationUsage.AGC_Code = "ACE";
		cusAuthorizationUsage.AGC_Number = "Number";
		cusAuthorizationUsage.AGC_OH_Owner = orgHeader.PK;
		lookups = new CusAuthorizationUsageLookups(cusAuthorizationUsage);
	}
	CusAuthorizationUsage cusAuthorizationUsage;
	OrgHeader orgHeader;
	CusAuthorizationUsageLookups lookups;
}
