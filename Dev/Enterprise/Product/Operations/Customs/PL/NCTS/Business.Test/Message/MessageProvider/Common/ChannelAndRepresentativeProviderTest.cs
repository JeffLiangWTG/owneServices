using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class ChannelAndRepresentativeProviderTest : Customs.Business.Testing.DataProviderTestCase<ChannelAndRepresentativeProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null NctsCommonMovementHeader", "Value cannot be null.\r\nParameter name: movementHeader", () => new ChannelAndRepresentativeProvider(null));
			AssertExceptionThrown<ArgumentNullException>("Null NctsHeader", "Value cannot be null.\r\nParameter name: NctsCommonMovementHeader.Header", () => new ChannelAndRepresentativeProvider(Factory.New<NctsArrivalMovementHeader>()));
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

			var destinationTraderOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			nctsArrivalHeader.DestinationTrader.E2_OA_Address = destinationTraderOrgHeader.MainAddress.PK;
			destinationTraderOrgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123", CountryCodes.Poland);
			AssertEquals("Arrival header representativeIdentificationNumber should be null as destinationTrader is same as reprentative", null, GetProviderForArrival().RepresentativeIdentificationNumber);
		});
	}

	protected override ChannelAndRepresentativeProvider GetProvider() => new ChannelAndRepresentativeProvider(destinationMovementHeader);

	public ChannelAndRepresentativeProvider GetProviderForArrival() => new ChannelAndRepresentativeProvider(arrivalMovementHeader);

	protected override void SetUp()
	{
		base.SetUp();

		nctsArrivalHeader = Factory.New<NctsHeader>();
		nctsArrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		arrivalMovementHeader = nctsArrivalHeader.ArrivalMovementHeader;
		arrivalMovementHeader.CustomsOffices.RemoveAll();
		arrivalRepresentativeOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
		arrivalMovementHeader.Representative.E2_OA_Address = arrivalRepresentativeOrgHeader.MainAddress.PK;

		destinationNctsHeader = Factory.New<NctsHeader>();
		destinationNctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		destinationMovementHeader = destinationNctsHeader.MovementHeader;
		destinationMovementHeader.CustomsOffices.RemoveAll();
		destinationRepresentativeOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
		destinationMovementHeader.Representative.E2_OA_Address = destinationRepresentativeOrgHeader.MainAddress.PK;
	}

	NctsHeader nctsArrivalHeader;
	NctsArrivalMovementHeader arrivalMovementHeader;
	OrgHeader arrivalRepresentativeOrgHeader;

	NctsHeader destinationNctsHeader;
	NctsDepartureMovementHeader destinationMovementHeader;
	OrgHeader destinationRepresentativeOrgHeader;
}
