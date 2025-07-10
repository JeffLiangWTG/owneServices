using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(CustomsOfficesOfExitForTransitProvider))]
sealed class CustomsOfficesOfExitForTransitProviderTest : Customs.Business.Testing.DataProviderTestCase<CustomsOfficesOfExitForTransitProvider>
{
	public void TestSequenceNumeric()
	{
		AssertEquals(1, provider.SequenceNumeric);
	}

	public void TestReferenceNumber()
	{
		AssertEquals("ExtID", provider.ReferenceNumber);
	}

	protected override void SetUp()
	{
		base.SetUp();

		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		var movementHeader = header.MovementHeader;

		customsOfficeOfDestination = movementHeader.CustomsOffices.AddNew();
		customsOfficeOfDestination.CY_Code = "EXT";
		customsOfficeOfDestination.CY_Data = "ExtID";
		provider = new CustomsOfficesOfExitForTransitProvider(customsOfficeOfDestination.CY_Data, 1);
	}

	CustomsOfficesOfExitForTransitProvider provider;
	NctsEuOfficeCode customsOfficeOfDestination;

	protected override CustomsOfficesOfExitForTransitProvider GetProvider() => provider;
}
