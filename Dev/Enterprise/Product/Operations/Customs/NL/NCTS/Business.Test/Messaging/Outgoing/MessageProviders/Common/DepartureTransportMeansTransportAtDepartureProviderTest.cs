using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(DepartureTransportMeansTransportAtDepartureProvider))]
sealed class DepartureTransportMeansTransportAtDepartureProviderTest : DepartureTransportMeansProviderAbstractTest<DepartureTransportMeansTransportAtDepartureProvider>
{
	public override void TestId()
	{
		const string identificationNumber = "123";
		movementHeader.BM_TransportAtDeparture = identificationNumber;

		AssertEquals(identificationNumber, Provider.Id);
	}

	public override void TestNationality()
	{
		const string nationality = "BE";
		movementHeader.BM_RN_NKTransportAtDepartureCountry = nationality;
		AssertEquals(nationality, Provider.Nationality);
	}

	public override void TestTypeOfIdentification()
	{
		CombineAssertions(() =>
		{
			movementHeader.BM_InlandTransportMode = ZString.Empty;
			AssertNull(Provider.TypeOfIdentification);

			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
			AssertEquals(11, Provider.TypeOfIdentification);

			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._2_RailTransport;
			AssertEquals(21, Provider.TypeOfIdentification);

			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
			AssertEquals(30, Provider.TypeOfIdentification);

			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._4_AirTransport;
			AssertEquals(40, Provider.TypeOfIdentification);

			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._8_InlandWaterwayTransport;
			AssertEquals(81, Provider.TypeOfIdentification);

			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._9_OwnPropulsion;
			const string transportAtDepartureType = "11";
			movementHeader.BM_TransportAtDepartureType = transportAtDepartureType;
			AssertEquals(11, Provider.TypeOfIdentification);
		});
	}

	public void TestTypeOfIdentification_WithVessel()
	{
		CombineAssertions(() =>
		{
			var refVessel = Factory.New<RefVessel>();
			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
			refVessel.RV_LloydsNumber = movementHeader.BM_TransportAtDeparture;
			AssertEquals(10, Provider.TypeOfIdentification);

			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._8_InlandWaterwayTransport;
			AssertEquals(80, Provider.TypeOfIdentification);
		});
	}
}
