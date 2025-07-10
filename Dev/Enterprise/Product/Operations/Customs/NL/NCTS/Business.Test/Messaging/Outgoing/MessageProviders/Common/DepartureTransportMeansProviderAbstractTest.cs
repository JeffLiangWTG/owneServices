using System;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestsSubclassesOf(typeof(DepartureTransportMeansProvider))]
abstract class DepartureTransportMeansProviderAbstractTest<T> : Customs.Business.Testing.DataProviderTestCase<T> where T : DepartureTransportMeansProvider
{
	public abstract void TestNationality();

	public virtual void TestId()
	{
		const string identificationNumber = "123";
		movementHeader.BM_TransportAtDeparture = "456";
		movementHeader.BM_AircraftIDAtDeparture = identificationNumber;

		AssertEquals(identificationNumber, Provider.Id);
	}

	public virtual void TestTypeOfIdentification()
	{
		AssertEquals(0, Provider.TypeOfIdentification);
	}

	protected override void SetUp()
	{
		CreateMovementHeader();
		provider = CreateProvider();
	}

	protected T CreateProvider() => (T)Activator.CreateInstance(typeof(T), movementHeader, 1);

	protected void CreateMovementHeader()
	{
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		movementHeader = header.MovementHeader;
	}

	protected override T GetProvider() => provider;

	protected NctsDepartureMovementHeader movementHeader;
	T provider;
}
