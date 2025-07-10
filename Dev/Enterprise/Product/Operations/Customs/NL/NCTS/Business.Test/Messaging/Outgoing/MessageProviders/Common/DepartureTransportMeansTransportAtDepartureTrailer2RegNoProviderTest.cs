using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(DepartureTransportMeansTransportAtDepartureTrailer2RegNoProvider))]
sealed class DepartureTransportMeansTransportAtDepartureTrailer2RegNoProviderTest : DepartureTransportMeansProviderAbstractTest<DepartureTransportMeansTransportAtDepartureTrailer2RegNoProvider>
{
	public override void TestId()
	{
		const string identificationNumber = "123";
		movementHeader.BM_TransportAtDeparture = "456";
		movementHeader.BM_TransportAtDepartureTrailer2RegNo = identificationNumber;

		AssertEquals(identificationNumber, Provider.Id);
	}

	public override void TestNationality()
	{
		const string nationality = "BE";
		movementHeader.BM_RN_NKTransportAtDepartureTrailer2Nationality = nationality;

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

			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
			AssertEquals(31, Provider.TypeOfIdentification);

			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._8_InlandWaterwayTransport;
			AssertEquals(81, Provider.TypeOfIdentification);

			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._9_OwnPropulsion;
			const string transportAtDepartureType = "11";
			movementHeader.BM_TransportAtDepartureType = transportAtDepartureType;
			AssertEquals(11, Provider.TypeOfIdentification);
		});
	}
}
