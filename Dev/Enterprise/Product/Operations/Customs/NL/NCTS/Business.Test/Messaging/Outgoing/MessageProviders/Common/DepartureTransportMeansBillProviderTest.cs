using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(DepartureTransportMeansBillProvider))]
sealed class DepartureTransportMeansBillProviderTest : Customs.Business.Testing.DataProviderTestCase<DepartureTransportMeansBillProvider>
{
	public void TestSequenceNumeric()
	{
		AssertEquals(1, Provider.SequenceNumeric);
	}

	public void TestTypeOfIdentification_TransportMode1()
	{
		AssertEquals(10, Provider.TypeOfIdentification);
	}

	public void TestTypeOfIdentification_TransportMode2()
	{
		transportMode = ModeOfTransportList.Codes._2_RailTransport;
		cusTransportMeans.TPM_SequenceNumber = 1;
		AssertEquals(20, Provider.TypeOfIdentification);

		cusTransportMeans.TPM_SequenceNumber = 2;
		AssertEquals(21, Provider.TypeOfIdentification);
	}

	public void TestTypeOfIdentification_TransportMode3()
	{
		transportMode = ModeOfTransportList.Codes._3_RoadTransport;
		cusTransportMeans.TPM_SequenceNumber = 1;
		AssertEquals(30, Provider.TypeOfIdentification);

		cusTransportMeans.TPM_SequenceNumber = 2;
		AssertEquals(31, Provider.TypeOfIdentification);
	}

	public void TestTypeOfIdentification_TransportMode4()
	{
		transportMode = ModeOfTransportList.Codes._4_AirTransport;
		AssertEquals(40, Provider.TypeOfIdentification);
	}

	public void TestTypeOfIdentification_TransportMode8()
	{
		transportMode = ModeOfTransportList.Codes._8_InlandWaterwayTransport;
		AssertEquals(81, Provider.TypeOfIdentification);
	}

	public void TestTypeOfIdentification_TransportMode9()
	{
		transportMode = ModeOfTransportList.Codes._9_OwnPropulsion;
		cusTransportMeans.TPM_TypeOfIdentification = "20";
		AssertEquals(20, Provider.TypeOfIdentification);
		cusTransportMeans.TPM_TypeOfIdentification = "x";
		AssertNull(Provider.TypeOfIdentification);
	}

	public void TestTypeOfIdentification_OtherMode()
	{
		transportMode = ModeOfTransportList.Codes._7_FixedTransportInstallations;
		AssertNull(Provider.TypeOfIdentification);
	}

	public void TestId()
	{
		cusTransportMeans.TPM_IdentificationNumber = "ABC123";
		AssertEquals("ABC123", Provider.Id);
	}

	public void TestNationality()
	{
		cusTransportMeans.TPM_RN_NKTransportNationality = Core.Constants.CountryCodes.Belgium;
		AssertEquals(Core.Constants.CountryCodes.Belgium, Provider.Nationality);
	}

	protected override void SetUp()
	{
		cusTransportMeans = Factory.New<DepartureCusTransportMeans>();
		transportMode = ModeOfTransportList.Codes._1_SeaTransport;
	}

	DepartureCusTransportMeans cusTransportMeans;
	string transportMode;

	protected override DepartureTransportMeansBillProvider GetProvider() => new DepartureTransportMeansBillProvider(cusTransportMeans, transportMode, 1);
}
