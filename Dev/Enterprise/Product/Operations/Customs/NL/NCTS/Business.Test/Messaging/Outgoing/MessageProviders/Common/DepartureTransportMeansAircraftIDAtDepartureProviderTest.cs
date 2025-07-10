using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(DepartureTransportMeansAircraftIDAtDepartureProvider))]
sealed class DepartureTransportMeansAircraftIDAtDepartureProviderTest : DepartureTransportMeansProviderAbstractTest<DepartureTransportMeansAircraftIDAtDepartureProvider>
{
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

			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._4_AirTransport;
			AssertEquals(41, Provider.TypeOfIdentification);

			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._8_InlandWaterwayTransport;
			AssertEquals(81, Provider.TypeOfIdentification);

			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._9_OwnPropulsion;
			const string transportAtDepartureType = "11";
			movementHeader.BM_TransportAtDepartureType = transportAtDepartureType;
			AssertEquals(11, Provider.TypeOfIdentification);
		});
	}
}
