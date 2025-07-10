using System;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class PlaceOfLoadingOrUnloadingProviderTest : Customs.Business.Testing.DataProviderTestCase<PlaceOfLoadingOrUnloadingProvider>
{
	public void TestNewOrNull()
	{
		CombineAssertions(() =>
		{
			AssertNull("Empty Unlocode and empty location and Missing Factory", PlaceOfLoadingOrUnloadingProvider.NewOrNull(ZString.Empty, ZString.Empty, null, false));
			AssertNull("Empty Unlocode and empty location", PlaceOfLoadingOrUnloadingProvider.NewOrNull(ZString.Empty, ZString.Empty, Factory, false));
			AssertExceptionThrown<ArgumentNullException>("Missing Factory", "Value cannot be null.\r\nParameter name: factory", () => PlaceOfLoadingOrUnloadingProvider.NewOrNull("BEBRU", ZString.Empty, null, false));
			AssertNotNull("Not empty Unlocode", PlaceOfLoadingOrUnloadingProvider.NewOrNull("BEBRU", ZString.Empty, Factory, false));
			AssertNotNull("Not empty location", PlaceOfLoadingOrUnloadingProvider.NewOrNull(ZString.Empty, "asd", Factory, false));
		});
	}

	public void TestUNLocode()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Valid Unlocode", "BEBRU", Provider.UNLocode);

			AssertEquals("Invalid Unlocode should also be accepted as long as they are 5 character long", "01234", GetProvider("01234", "asd").UNLocode);

			AssertEquals("Invalid Unlocode 4 character long", string.Empty, GetProvider("1234", "asd").UNLocode);

			AssertEquals("Invalid Unlocode 6 character long", string.Empty, GetProvider("123456", "asd").UNLocode);
		});
	}

	public void TestCountry()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Not Empty Unloco", CountryCodes.Belgium, Provider.Country);
			AssertEquals("Invalid Unlocode and empty location", ZString.Empty, PlaceOfLoadingOrUnloadingProvider.NewOrNull("12345", ZString.Empty, movementHeader.Factory, false).Country);
			AssertEquals("Invalid Unlocode but not empty location", "12", PlaceOfLoadingOrUnloadingProvider.NewOrNull("12345", "asd", movementHeader.Factory, false).Country);
		});
	}

	public void TestLocation()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Empty location", ZString.Empty, Provider.Location);
			AssertEquals("Not empty location", "asd", PlaceOfLoadingOrUnloadingProvider.NewOrNull("12345", "asd", movementHeader.Factory, false).Location);
			AssertEquals("Missing unlocode with not empty location", "asd", PlaceOfLoadingOrUnloadingProvider.NewOrNull(ZString.Empty, "asd", movementHeader.Factory, false).Location);
		});
	}

	public void TestLocationMaxLength()
	{
		const int inNCTSTPPeriod = 17;
		const int outNCTSTPPeriod = 35;

		CombineAssertions(() =>
		{
			AssertEquals("Location max length outside transition period", outNCTSTPPeriod, GetProvider().LocationMaxLength);
			AssertEquals("Location max length in transition period", inNCTSTPPeriod, PlaceOfLoadingOrUnloadingProvider.NewOrNull("BEBRU", ZString.Empty, movementHeader.Factory, true).LocationMaxLength);
		});
	}

	PlaceOfLoadingOrUnloadingProvider GetProvider(ZString unlocode, string location = "") => PlaceOfLoadingOrUnloadingProvider.NewOrNull(unlocode, location, movementHeader.Factory, false);
	protected override PlaceOfLoadingOrUnloadingProvider GetProvider() => GetProvider("BEBRU");

	protected override void SetUp()
	{
		base.SetUp();

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		movementHeader = nctsHeader.MovementHeader;
	}
	NctsDepartureMovementHeader movementHeader;
}
