using System.Linq;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

abstract class ConsignmentProviderBaseTest<T> : Customs.Business.Testing.DataProviderTestCase<T> where T : ConsignmentProviderBase
{
	public void TestDepartureTransportMeans_SeaTransport()
	{
		CombineAssertions(() =>
		{
			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
			AssertEquals("No DepartureTransportMeans", 0, GetProvider().DepartureTransportMeans.Count);

			SetTransportAtDeparture("A");
			if (IsTransportTypeAtDepartureRequired)
			{
				AssertEquals("No TransportTypeAtDeparture", 0, GetProvider().DepartureTransportMeans.Count);
			}

			SetTransportTypeAtDeparture("1");
			var providerVesselNameNotEmpty = GetProvider();
			AssertEquals("_1_SeaTransport", 1, providerVesselNameNotEmpty.DepartureTransportMeans.Count);
			var testData = providerVesselNameNotEmpty.DepartureTransportMeans.Cast<DepartureTransportMeansProvider>().Single();
			AssertEquals("Type Of Identification", "1", testData.TypeOfIdentification);
			AssertEquals("Identification Number", "A", testData.IdentificationNumber);
		});
	}

	public void TestDepartureTransportMeans_RailTransport()
	{
		CombineAssertions(() =>
		{
			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._2_RailTransport;
			AssertEquals("No DepartureTransportMeans", 0, GetProvider().DepartureTransportMeans.Count);

			SetTransportAtDeparture("A");
			if (IsTransportTypeAtDepartureRequired)
			{
				AssertEquals("No TransportTypeAtDeparture", 0, GetProvider().DepartureTransportMeans.Count);
			}

			SetTransportTypeAtDeparture("20");
			var providerTransportNotEmpty = GetProvider();
			AssertEquals("Only transport is defined: _2_RailTransport", 1, providerTransportNotEmpty.DepartureTransportMeans.Count);
			var testData = providerTransportNotEmpty.DepartureTransportMeans.Cast<DepartureTransportMeansProvider>().Single();
			AssertEquals("Only transport is defined: Type Of Identification", "20", testData.TypeOfIdentification);
			AssertEquals("Only transport is defined: Identification Number", "A", testData.IdentificationNumber);
		});
	}

	public void TestDepartureTransportMeans_RoadTransport()
	{
		CombineAssertions(() =>
		{
			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
			AssertEquals("No DepartureTransportMeans", 0, GetProvider().DepartureTransportMeans.Count);

			SetTransportAtDeparture("A");
			if (IsTransportTypeAtDepartureRequired)
			{
				AssertEquals("No TransportTypeAtDeparture", 0, GetProvider().DepartureTransportMeans.Count);
			}

			SetTransportTypeAtDeparture("30");
			var providerTransportNotEmpty = GetProvider();
			AssertEquals("Only transport is defined: _3_RoadTransport", 1, providerTransportNotEmpty.DepartureTransportMeans.Count);
			var testData = providerTransportNotEmpty.DepartureTransportMeans.Cast<DepartureTransportMeansProvider>().Single();
			AssertEquals("Only transport is defined: Type Of Identification", "30", testData.TypeOfIdentification);
			AssertEquals("Only transport is defined: Identification Number", "A", testData.IdentificationNumber);

			SetTrailer1IDAtDeparture("B");
			var providerTrailer1IDNotEmpty = GetProvider();
			AssertEquals("Transport and trailer1 are defined: _3_RoadTransport", 2, providerTrailer1IDNotEmpty.DepartureTransportMeans.Count);
			testData = providerTrailer1IDNotEmpty.DepartureTransportMeans.Cast<DepartureTransportMeansProvider>().Skip(1).Single();
			AssertEquals("Transport and trailer1 are defined: Type Of Identification", "31", testData.TypeOfIdentification);
			AssertEquals("Transport and trailer1 are defined: Identification Number", "B", testData.IdentificationNumber);

			SetTrailer2IDAtDeparture("C");
			var providerTrailer2IDNotEmpty = GetProvider();
			AssertEquals("Transport and 2 trailers are defined: _3_RoadTransport", 3, providerTrailer2IDNotEmpty.DepartureTransportMeans.Count);
			testData = providerTrailer2IDNotEmpty.DepartureTransportMeans.Cast<DepartureTransportMeansProvider>().Skip(2).Single();
			AssertEquals("Transport and 2 trailers are defined: Type Of Identification", "31", testData.TypeOfIdentification);
			AssertEquals("Transport and 2 trailers are defined: Identification Number", "C", testData.IdentificationNumber);
		});
	}

	public void TestDepartureTransportMeans_AirTransport()
	{
		CombineAssertions(() =>
		{
			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._4_AirTransport;
			AssertEquals("No DepartureTransportMeans", 0, GetProvider().DepartureTransportMeans.Count);

			SetTransportAtDeparture("A");
			if (IsTransportTypeAtDepartureRequired)
			{
				AssertEquals("No TransportTypeAtDeparture", 0, GetProvider().DepartureTransportMeans.Count);
			}

			SetTransportTypeAtDeparture("41");
			var providerAircraftIDNotEmpty = GetProvider();
			AssertEquals("Aircraft only: _4_AirTransport", 1, providerAircraftIDNotEmpty.DepartureTransportMeans.Count);
			var testData = providerAircraftIDNotEmpty.DepartureTransportMeans.Cast<DepartureTransportMeansProvider>().Single();
			AssertEquals("Aircraft only: Type Of Identification", "41", testData.TypeOfIdentification);
			AssertEquals("Aircraft only: Identification Number", "A", testData.IdentificationNumber);
		});
	}

	public void TestDepartureTransportMeans_InlandWaterwayTransport()
	{
		CombineAssertions(() =>
		{
			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._8_InlandWaterwayTransport;
			AssertEquals("No DepartureTransportMeans", 0, GetProvider().DepartureTransportMeans.Count);

			SetTransportAtDeparture("A");
			if (IsTransportTypeAtDepartureRequired)
			{
				AssertEquals("No TransportTypeAtDeparture", 0, GetProvider().DepartureTransportMeans.Count);
			}

			SetTransportTypeAtDeparture("81");
			var providerVesselNameNotEmpty = GetProvider();
			AssertEquals("_8_InlandWaterwayTransport", 1, providerVesselNameNotEmpty.DepartureTransportMeans.Count);
			var testData = providerVesselNameNotEmpty.DepartureTransportMeans.Cast<DepartureTransportMeansProvider>().Single();
			AssertEquals("Type Of Identification", "81", testData.TypeOfIdentification);
			AssertEquals("Identification Number", "A", testData.IdentificationNumber);
		});
	}

	public void TestDepartureTransportMeans_OwnPropulsion()
	{
		CombineAssertions(() =>
		{
			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._9_OwnPropulsion;
			AssertEquals("No DepartureTransportMeans", 0, GetProvider().DepartureTransportMeans.Count);

			SetTransportAtDeparture("2");
			if (IsTransportTypeAtDepartureRequired)
			{
				AssertEquals("No TransportTypeAtDeparture", 0, GetProvider().DepartureTransportMeans.Count);
			}

			SetTransportTypeAtDeparture("1");
			var providerTransportAndTransportTypeNotEmpty = GetProvider();
			AssertEquals("_9_OwnPropulsion", 1, providerTransportAndTransportTypeNotEmpty.DepartureTransportMeans.Count);
			var testData = providerTransportAndTransportTypeNotEmpty.DepartureTransportMeans.Cast<DepartureTransportMeansProvider>().Single();
			AssertEquals("Type Of Identification", "1", testData.TypeOfIdentification);
			AssertEquals("Identification Number", "2", testData.IdentificationNumber);
		});
	}

	public void TestDepartureTransportMeans_PostalConsignment()
	{
		CombineAssertions(() =>
		{
			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._5_PostalConsignment;
			AssertEquals("_5_PostalConsignment is not allowed", 0, GetProvider().DepartureTransportMeans.Count);
		});
	}

	public void TestDepartureTransportMeans_FixedTransportInstallations()
	{
		CombineAssertions(() =>
		{
			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._7_FixedTransportInstallations;
			AssertEquals("_7_FixedTransportInstallations is not allowed", 0, GetProvider().DepartureTransportMeans.Count);
		});
	}

	protected virtual void SetTransportTypeAtDeparture(string value) => nctsBill.TransportTypeAtDeparture = value;

	protected virtual void SetTransportAtDeparture(string transport) => nctsBill.TransportAtDeparture = transport;

	protected virtual void SetTrailer1IDAtDeparture(string value) => nctsBill.Trailer1IDAtDeparture = value;

	protected virtual void SetTrailer2IDAtDeparture(string value) => nctsBill.Trailer2IDAtDeparture = value;

	protected virtual void SetVesselNameAtDeparture(string value) => nctsBill.VesselNameAtDeparture = value;

	protected virtual bool IsTransportTypeAtDepartureRequired => false;

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		movementHeader = nctsHeader.MovementHeader;
		nctsBill = nctsHeader.Bills.AddNew();
	}

	protected NctsHeader nctsHeader;
	protected NctsDepartureMovementHeader movementHeader;
	protected NctsBill nctsBill;
}
