using System;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class ActiveBorderTransportMeansTypeProviderTest : Customs.Business.Testing.DataProviderTestCase<ActiveBorderTransportMeansTypeProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Null NctsDepartureMovementHeader", "Value cannot be null.\r\nParameter name: movementHeader", () => new ActiveBorderTransportMeansTypeProvider(1, movementHeader: null));
	}

	public void TestSequenceNumber() => AssertEquals(ExpectedSequenceNumber.ToString(), Provider.SequenceNumber);

	public void TestCustomsOfficeAtBorderReferenceNumber()
	{
		CombineAssertions(() =>
		{
			movementHeader.BM_CustomsOfficeAtBorder = "A";
			AssertEquals("CustomsOfficeAtBorderReferenceNumber = A", "A", GetProvider().CustomsOfficeAtBorderReferenceNumber);

			movementHeader.BM_CustomsOfficeAtBorder = ZString.Empty;
			AssertEquals("NaCustomsOfficeAtBorderReferenceNumber is empty", string.Empty, GetProvider().CustomsOfficeAtBorderReferenceNumber);
		});
	}

	public void TestTypeOfIdentification()
	{
		CombineAssertions(() =>
		{
			movementHeader.BM_ActiveBorderIdentificationType = "B";
			AssertEquals("TypeOfIdentification = B", "B", GetProvider().TypeOfIdentification);

			movementHeader.BM_ActiveBorderIdentificationType = ZString.Empty;
			AssertEquals("TypeOfIdentification is empty", string.Empty, GetProvider().TypeOfIdentification);
		});
	}

	public void TestIdentificationNumber()
	{
		CombineAssertions(() =>
		{
			movementHeader.BM_TOLCarrierID = "C";
			AssertEquals("IdentificationNumber = C", "C", GetProvider().IdentificationNumber);

			movementHeader.BM_TOLCarrierID = ZString.Empty;
			AssertEquals("IdentificationNumber is empty", string.Empty, GetProvider().IdentificationNumber);
		});
	}

	public void TestNationality()
	{
		CombineAssertions(() =>
		{
			movementHeader.BM_RN_NKTOLCarrierNationality = "D";
			AssertEquals("Nationality = D", "D", GetProvider().Nationality);

			movementHeader.BM_RN_NKTOLCarrierNationality = ZString.Empty;
			AssertEquals("Nationality is empty", string.Empty, GetProvider().Nationality);
		});
	}

	public void TestConveyanceReferenceNumber()
	{
		CombineAssertions(() =>
		{
			movementHeader.BM_ConveyanceNumber = "E";
			AssertEquals("ConveyanceReferenceNumber = E", "E", GetProvider().ConveyanceReferenceNumber);

			movementHeader.BM_ConveyanceNumber = ZString.Empty;
			AssertEquals("ConveyanceReferenceNumber is empty", string.Empty, GetProvider().ConveyanceReferenceNumber);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		movementHeader = nctsHeader.MovementHeader;
	}

	const int ExpectedSequenceNumber = 99;

	NctsDepartureMovementHeader movementHeader;

	protected override ActiveBorderTransportMeansTypeProvider GetProvider() => new (ExpectedSequenceNumber, movementHeader);
}
