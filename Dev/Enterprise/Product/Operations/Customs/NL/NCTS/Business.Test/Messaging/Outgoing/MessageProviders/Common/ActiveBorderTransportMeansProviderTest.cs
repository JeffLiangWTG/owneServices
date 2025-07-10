using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(ActiveBorderTransportMeansProvider))]
sealed class ActiveBorderTransportMeansProviderTest : Customs.Business.Testing.DataProviderTestCase<ActiveBorderTransportMeansProvider>
{
	public void TestSequenceNumeric()
	{
		AssertEquals(1, Provider.SequenceNumeric);
	}

	public void TestCustomsOfficeAtBorderReferenceNumber()
	{
		depHeader.BM_CustomsOfficeAtBorder = "BE112233";
		AssertEquals("BE112233", Provider.CustomsOfficeAtBorderReferenceNumber);
	}

	public void TestTypeOfIdentification()
	{
		depHeader.BM_ActiveBorderIdentificationType = "40";
		AssertEquals(40, Provider.TypeOfIdentification);
		depHeader.BM_ActiveBorderIdentificationType = "x";
		AssertNull(Provider.TypeOfIdentification);
	}

	public void TestId()
	{
		depHeader.BM_TOLCarrierID = "ABC123";
		AssertEquals("ABC123", Provider.Id);
	}

	public void TestNationality()
	{
		depHeader.BM_RN_NKTOLCarrierNationality = Core.Constants.CountryCodes.Belgium;
		AssertEquals(Core.Constants.CountryCodes.Belgium, Provider.Nationality);
	}

	public void TestConveyanceReferenceNumber()
	{
		depHeader.BM_ConveyanceNumber = "CONVNR";
		AssertEquals("CONVNR", Provider.ConveyanceReferenceNumber);
	}

	protected override ActiveBorderTransportMeansProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();

		header = Factory.New<NctsHeader>();
		header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		depHeader = header.MovementHeader;
		provider = new ActiveBorderTransportMeansProvider(depHeader);
	}

	ActiveBorderTransportMeansProvider provider;
	NctsHeader header;
	NctsDepartureMovementHeader depHeader;
}
