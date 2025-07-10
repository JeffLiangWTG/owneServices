using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class NctsArrivalMovementHeaderLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestAuthorizationCodeList()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var eunau = EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU;
		helper.CreateCusMapType(eunau, MapDirectionList.Codes.OUT, "EUNAU", true);
		helper.CreateCusMap(eunau, "ACE", "C522", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		helper.CreateCusMap(eunau, "ACT", "C520", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		Factory.Save();

		CombineAssertions(() =>
		{
			var codes = lookups.AuthorizationCodeList;
			AssertEquals("CodesAsString", "ACE, ACT", codes.CodesAsString);
			AssertEquals("First Description", "C522 - Authorization for the status of authorized consignee for Union transit", codes.GetDescriptionFromCode("ACE"));
			AssertEquals("Second Description", "C520 - Authorization for the status of authorized consignee for TIR procedure", codes.GetDescriptionFromCode("ACT"));
		});
	}

	public void TestAuthorisationRule_FilterBusinessObjectDefaults()
	{
		arrivalMovementHeader.AuthorizationCode = "ACE";
		arrivalMovementHeader.AuthorizationNumber = "Number";
		arrivalMovementHeader.AuthorizationOwner = orgHeader.PK;
		var authorisationRuleList = (CusAuthorisationRuleCollection)lookups.AuthorizationRuleList;

		CombineAssertions(() =>
		{
			AssertEquals("Number", "Number", authorisationRuleList.FilterBusinessObjectDefaults[GetFilterDefaultKey(CusAuthorisationRuleCollection.FilterConstants.AuthorizationNumber)].Value);
			AssertEquals("Holder", orgHeader.PK, authorisationRuleList.FilterBusinessObjectDefaults[GetFilterDefaultKey(CusAuthorisationRuleCollection.FilterConstants.AuthorizationHolder)].Value);
			AssertEquals("Type", "ACE", authorisationRuleList.FilterBusinessObjectDefaults[GetFilterDefaultKey(CusAuthorisationRuleCollection.FilterConstants.AuthorizationType)].Value);
			var countryFilterBusinessObjectDefault = authorisationRuleList.FilterBusinessObjectDefaults[GetFilterDefaultKey(CusAuthorisationRuleCollection.FilterConstants.Country)];
			AssertEquals("Country Value", arrivalMovementHeader.Header.CountryCode, countryFilterBusinessObjectDefault.Value);
			AssertEquals("Country Value", expected: false, countryFilterBusinessObjectDefault.IsRemovable);
		});

		ZString GetFilterDefaultKey(string filter) => FilterBusinessObjectDefault.Create(filter, "Property").Key;
	}

	public void TestNctsTransitStatusList()
	{
		AssertType<EU.NCTS.Business.CodeDescriptionPairLists.NCTS5ArrivalCustomsStatusList>(lookups.NctsTransitStatusList);
	}

	protected override void SetUp()
	{
		base.SetUp();
		orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		arrivalMovementHeader = nctsHeader.ArrivalMovementHeader;
		lookups = arrivalMovementHeader.Lookups;
	}

	NctsArrivalMovementHeader arrivalMovementHeader;
	OrgHeader orgHeader;
	NctsArrivalMovementHeaderLookups lookups;
}
