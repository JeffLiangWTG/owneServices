using System;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class CustomsOfficeOfTransitProviderTest : Customs.Business.Testing.DataProviderTestCase<CustomsOfficeOfTransitProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Null NctsEuOfficeCode", "Value cannot be null.\r\nParameter name: customsOffice", () => new CustomsOfficeOfTransitProvider(1, null));
	}

	public void TestArrivalDateAndTimeEstimated()
	{
		CombineAssertions(() =>
		{
			AssertNull("Not valid or empty date", Provider.ArrivalDateAndTimeEstimated);

			office.CY_Date = new ZDateTime(2023, 1, 1);
			AssertEquals("Valid date", new ZDateTime(2023, 1, 1), GetProvider().ArrivalDateAndTimeEstimated);
		});
	}

	protected override CustomsOfficeOfTransitProvider GetProvider() => new CustomsOfficeOfTransitProvider(99, office);

	protected override void SetUp()
	{
		base.SetUp();

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		office = nctsHeader.MovementHeader.CustomsOffices.AddNew();
	}
	NctsPLOfficeCode office;
}
