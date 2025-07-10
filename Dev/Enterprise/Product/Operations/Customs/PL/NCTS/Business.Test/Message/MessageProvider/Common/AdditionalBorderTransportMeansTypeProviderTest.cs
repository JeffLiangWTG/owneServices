using System;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class AdditionalBorderTransportMeansTypeProviderTest : Customs.Business.Testing.DataProviderTestCase<AdditionalBorderTransportMeansTypeProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null NctsDepartureMovementHeader", "Value cannot be null.\r\nParameter name: movementHeader", () => new AdditionalBorderTransportMeansTypeProvider(1, null, null));
			AssertExceptionThrown<ArgumentNullException>("Null CusTransportMeans", "Value cannot be null.\r\nParameter name: transportMeans", () => new AdditionalBorderTransportMeansTypeProvider(1, null, movementHeader));
		});
	}

	public void TestSequenceNumber() => AssertEquals(ExpectedSequenceNumber.ToString(), Provider.SequenceNumber);

	public void TestCustomsOfficeAtBorderReferenceNumber()
	{
		CombineAssertions(() =>
		{
			transportMeans.TPM_CustomsOffice = "A";
			AssertEquals("CustomsOfficeAtBorderReferenceNumber = A", "A", GetProvider().CustomsOfficeAtBorderReferenceNumber);

			transportMeans.TPM_CustomsOffice = ZString.Empty;
			AssertEquals("NaCustomsOfficeAtBorderReferenceNumber is empty", string.Empty, GetProvider().CustomsOfficeAtBorderReferenceNumber);
		});
	}

	public void TestTypeOfIdentification()
	{
		CombineAssertions(() =>
		{
			transportMeans.TPM_TypeOfIdentification = "B";
			AssertEquals("TypeOfIdentification = B", "B", GetProvider().TypeOfIdentification);

			transportMeans.TPM_TypeOfIdentification = ZString.Empty;
			AssertEquals("TypeOfIdentification is empty", string.Empty, GetProvider().TypeOfIdentification);
		});
	}

	public void TestIdentificationNumber()
	{
		CombineAssertions(() =>
		{
			transportMeans.TPM_IdentificationNumber = "C";
			AssertEquals("IdentificationNumber = C", "C", GetProvider().IdentificationNumber);

			transportMeans.TPM_IdentificationNumber = ZString.Empty;
			AssertEquals("IdentificationNumber is empty", string.Empty, GetProvider().IdentificationNumber);
		});
	}

	public void TestNationality()
	{
		CombineAssertions(() =>
		{
			transportMeans.TPM_RN_NKTransportNationality = "D";
			AssertEquals("Nationality = D", "D", GetProvider().Nationality);

			transportMeans.TPM_RN_NKTransportNationality = ZString.Empty;
			AssertEquals("Nationality is empty", string.Empty, GetProvider().Nationality);
		});
	}

	public void TestConveyanceReferenceNumber()
	{
		CombineAssertions(() =>
		{
			transportMeans.TPM_ReferenceNumber = "E";
			AssertEquals("ConveyanceReferenceNumber = E", "E", Provider.ConveyanceReferenceNumber);

			transportMeans.TPM_ReferenceNumber = ZString.Empty;
			AssertEquals("ConveyanceReferenceNumber is empty", string.Empty, Provider.ConveyanceReferenceNumber);
		});
	}

	protected override AdditionalBorderTransportMeansTypeProvider GetProvider() => new AdditionalBorderTransportMeansTypeProvider(ExpectedSequenceNumber, transportMeans, movementHeader);

	protected override void SetUp()
	{
		base.SetUp();
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		movementHeader = nctsHeader.MovementHeader;
		transportMeans = movementHeader.AdditionalTransportAtBorderList.AddNew();
	}

	const int ExpectedSequenceNumber = 99;

	NctsDepartureMovementHeader movementHeader;
	DepartureCusTransportMeans transportMeans;
}
