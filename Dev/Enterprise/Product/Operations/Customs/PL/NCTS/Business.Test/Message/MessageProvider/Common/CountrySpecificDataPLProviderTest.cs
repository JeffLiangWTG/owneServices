using System;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class CountrySpecificDataPLProviderTest : Customs.Business.Testing.DataProviderTestCase<CountrySpecificDataPLProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null NctsCommonMovementHeader", "Value cannot be null.\r\nParameter name: movementHeader", () => new CountrySpecificDataPLProvider(null, null));
			AssertExceptionThrown<ArgumentNullException>("Null NctsHeader", "Value cannot be null.\r\nParameter name: NctsCommonMovementHeader.Header", () => new CountrySpecificDataPLProvider(Factory.New<NctsArrivalMovementHeader>(), null));

			AssertExceptionThrown<ArgumentNullException>("Null arrival MessageSendingObject", "Value cannot be null.\r\nParameter name: messageSendingObject", () => new CountrySpecificDataPLProvider(arrivalMovementHeader, null));
			AssertExceptionThrown<ArgumentNullException>("Null departure MessageSendingObject", "Value cannot be null.\r\nParameter name: messageSendingObject", () => new CountrySpecificDataPLProvider(destinationMovementHeader, null));

			AssertNoExceptionThrown("Valid departure constructor", () => new CountrySpecificDataPLProvider(destinationMovementHeader, destinationMessageSendingObject));
			AssertNoExceptionThrown("Valid arrival constructor", () => new CountrySpecificDataPLProvider(arrivalMovementHeader, arrivalMessageSendingObject));
		});
	}

	public void TestCommunicationChannel()
	{
		AssertNotNull(Provider.CommunicationChannel);
	}

	public void TestRepresentativeIdentificationNumber_DepartureMovement()
	{
		CombineAssertions(() =>
		{
			destinationRepresentativeOrgHeader.CustomsCodes.RemoveAll();
			AssertEquals("RepresentativeIdentificationNumber should be null", null, Provider.RepresentativeIdentificationNumber);

			destinationRepresentativeOrgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "111", CountryCodes.Poland);
			AssertEquals("RepresentativeIdentificationNumber should be PL111", "PL111", GetProvider().RepresentativeIdentificationNumber);
		});
	}

	public void TestRepresentativeIdentificationNumber_ArrivalMovement()
	{
		CombineAssertions(() =>
		{
			arrivalRepresentativeOrgHeader.CustomsCodes.RemoveAll();
			AssertEquals("Arrival header representativeIdentificationNumber should be null", null, GetProviderForArrival().RepresentativeIdentificationNumber);

			arrivalRepresentativeOrgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123", CountryCodes.Poland);
			AssertEquals("Arrival header representativeIdentificationNumber should be PL123", "PL123", GetProviderForArrival().RepresentativeIdentificationNumber);
		});
	}

	public void TestLocationOfGoodsCodeFromAuthorisation_ArrivalMovement()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Empty when not have authorization usage", string.Empty, GetProviderForArrival().LocationOfGoodsCodeFromAuthorisation);

			authorisationHeader = Factory.New<CusAuthorisationHeader>();
			authorisationHeader.CPH_Type = "ACE";
			authorisationHeader.CPH_Number = "AUTH0000001";
			authorisationHeader.CPH_OH_PermitHolder = arrivalRepresentativeOrgHeader.PK;

			var rule1 = authorisationHeader.CusAuthorisationRules.AddNew();
			rule1.CPR_RuleCode = "LOC";
			rule1.CPR_ValueFrom = "TST1";

			authorization = arrivalMovementHeader.Header.CusAuthorizationUsages.AddNew();
			authorization.AGC_Code = "ACE";
			authorization.AGC_Number = "AUTH0000001";
			authorization.AGC_OH_Owner = arrivalRepresentativeOrgHeader.PK;
			authorization.AGC_Location = "TST1";
			AssertEquals("Empty when authorisation header only have one LOC rule", string.Empty, GetProviderForArrival().LocationOfGoodsCodeFromAuthorisation);

			var rule2 = authorisationHeader.CusAuthorisationRules.AddNew();
			rule2.CPR_RuleCode = "LOC";
			rule2.CPR_ValueFrom = "TST2";
			authorization.AGC_Location = "";
			AssertEquals("Empty when authorisation header has more than one LOC rules but the location is empty", string.Empty, GetProviderForArrival().LocationOfGoodsCodeFromAuthorisation);

			authorization.AGC_Location = "TST1";
			AssertEquals("Arrival movement should get Location when authorisation header has more than one LOC rules", "TST1", GetProviderForArrival().LocationOfGoodsCodeFromAuthorisation);

			authorization.AGC_Location = "TST2";
			AssertEquals("Arrival movement should get Location from selected rule when authorisation header has more than one LOC rules", "TST2", GetProviderForArrival().LocationOfGoodsCodeFromAuthorisation);
		});
	}

	public void TestLocationOfGoodsCodeFromAuthorisation_DepartureMovement()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Empty when not have authorization usage", string.Empty, GetProvider().LocationOfGoodsCodeFromAuthorisation);
			authorisationHeader = Factory.New<CusAuthorisationHeader>();
			authorisationHeader.CPH_Type = "code";
			authorisationHeader.CPH_OH_PermitHolder = destinationRepresentativeOrgHeader.PK;
			authorisationHeader.CPH_Number = "testnumber";
			var rule1 = authorisationHeader.CusAuthorisationRules.AddNew();
			rule1.CPR_RuleCode = "LOC";
			rule1.CPR_ValueFrom = "T1";
			authorization = destinationMovementHeader.CusAuthorizationUsages.AddNew();
			authorization.AGC_Number = "testnumber";
			authorization.AGC_Code = "code";
			authorization.AGC_OH_Owner = destinationRepresentativeOrgHeader.PK;
			authorization.AGC_Location = "T1";
			AssertEquals("Empty when authorisation header only have one LOC rule", string.Empty, GetProvider().LocationOfGoodsCodeFromAuthorisation);

			var rule2 = authorisationHeader.CusAuthorisationRules.AddNew();
			rule2.CPR_RuleCode = "LOC";
			rule2.CPR_ValueFrom = "T2";
			authorization.AGC_Location = "";
			AssertEquals("Empty when authorisation header has more than one LOC rules but the location is empty", string.Empty, GetProvider().LocationOfGoodsCodeFromAuthorisation);

			authorization.AGC_Location = "T1";
			AssertEquals("Departure movement should get Location when authorisation header has more than one LOC rule", "T1", GetProvider().LocationOfGoodsCodeFromAuthorisation);

			var authorisationHeader1 = Factory.New<CusAuthorisationHeader>();
			authorisationHeader1.CPH_Type = "test";
			authorisationHeader1.CPH_OH_PermitHolder = destinationRepresentativeOrgHeader.PK;
			authorisationHeader1.CPH_Number = "testnumber1";
			var rule3 = authorisationHeader1.CusAuthorisationRules.AddNew();
			rule3.CPR_RuleCode = "LOC";
			rule3.CPR_ValueFrom = "T3";
			var rule4 = authorisationHeader1.CusAuthorisationRules.AddNew();
			rule4.CPR_RuleCode = "LOC";
			rule4.CPR_ValueFrom = "T4";
			authorization = destinationMovementHeader.CusAuthorizationUsages.AddNew();
			authorization.AGC_Number = "testnumber1";
			authorization.AGC_Code = "test";
			authorization.AGC_OH_Owner = destinationRepresentativeOrgHeader.PK;
			authorization.AGC_Location = "T4";
			AssertEquals("Departure movement should get the Location in first row when authorisation header has more than one LOC rules and there are two authorization usages", "T1", GetProvider().LocationOfGoodsCodeFromAuthorisation);
		});
	}

	public void TestTIRPageNumberValue()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Default", string.Empty, Provider.TIRPageNumberValue);

			arrivalMessageSendingObject.TirPageNumber = "A";
			AssertEquals("Only numbers are allowed", "A", GetProviderForArrival().TIRPageNumberValue);

			arrivalMessageSendingObject.TirPageNumber = "1";
			AssertEquals("Value should be Left Pad 2 with 0", "1", GetProviderForArrival().TIRPageNumberValue);
		});
	}

	public void TestTIRUnloadingNumberValue()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Default", string.Empty, Provider.TIRUnloadingNumberValue);

			arrivalMessageSendingObject.TirUnloadingNumber = "A";
			AssertEquals("Not empty", "A", GetProviderForArrival().TIRUnloadingNumberValue);
		});
	}

	protected override CountrySpecificDataPLProvider GetProvider() => new CountrySpecificDataPLProvider(destinationMovementHeader, destinationMessageSendingObject);

	CountrySpecificDataPLProvider GetProviderForArrival() => new CountrySpecificDataPLProvider(arrivalMovementHeader, arrivalMessageSendingObject);

	protected override void SetUp()
	{
		base.SetUp();

		nctsArrivalHeader = Factory.New<NctsHeader>();
		nctsArrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		arrivalMovementHeader = nctsArrivalHeader.ArrivalMovementHeader;
		arrivalMovementHeader.CustomsOffices.RemoveAll();
		arrivalRepresentativeOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
		arrivalMovementHeader.Representative.E2_OA_Address = arrivalRepresentativeOrgHeader.MainAddress.PK;
		arrivalMessageSendingObject = new MessageSendingObjectParent(nctsArrivalHeader).SelectedSendingObjects.First() as MessageSendingObject;

		destinationNctsHeader = Factory.New<NctsHeader>();
		destinationNctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		destinationMovementHeader = destinationNctsHeader.MovementHeader;
		destinationMovementHeader.CustomsOffices.RemoveAll();
		destinationRepresentativeOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
		destinationMovementHeader.Representative.E2_OA_Address = destinationRepresentativeOrgHeader.MainAddress.PK;
		destinationMessageSendingObject = new MessageSendingObjectParent(destinationNctsHeader).SelectedSendingObjects.First() as MessageSendingObject;
	}

	NctsHeader nctsArrivalHeader;
	NctsArrivalMovementHeader arrivalMovementHeader;
	OrgHeader arrivalRepresentativeOrgHeader;
	MessageSendingObject arrivalMessageSendingObject;

	NctsHeader destinationNctsHeader;
	NctsDepartureMovementHeader destinationMovementHeader;
	OrgHeader destinationRepresentativeOrgHeader;
	CusAuthorisationHeader authorisationHeader;
	CusAuthorizationUsage authorization;
	MessageSendingObject destinationMessageSendingObject;
}
