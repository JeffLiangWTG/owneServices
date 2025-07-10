using System.Linq;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestsSubclassesOf(typeof(DepartureHeaderProvider))]
abstract class DepartureHeaderProviderAbstractTest<T> : MessageHeaderProviderAbstractTest<T> where T : DepartureHeaderProvider
{
	public void TestRepresentative() => CombineAssertions(() =>
	{
		AssertNull("not available", Provider.Representative);
		NCTSTestHelper.CreateJobDocAddressForTest(Factory, "REP", nctsHeader.MovementHeader.Representative, "1", traderTir: "GBR/022/1234567");
		var provider = new DepartureHeaderProvider(nctsHeader);
		AssertNotNull("available", provider.Representative);
	});

	public void TestCustomsOfficeOfDeparture()
	{
		var movementHeader = nctsHeader.MovementHeader;
		var customsOfficeOfDeparture = movementHeader.CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);
		customsOfficeOfDeparture.CY_Data = "DepID";

		AssertEquals("DepID", Provider.CustomsOfficeOfDeparture);
	}

	public void TestCustomsOfficeOfDestination()
	{
		var movementHeader = nctsHeader.MovementHeader;
		var customsOfficeOfDestination = movementHeader.CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
		customsOfficeOfDestination.CY_Data = "DesID";

		AssertEquals("DesID", Provider.CustomsOfficeOfDestination);
	}

	public void TestHolderOfTheTransitProcedure()
	{
		CombineAssertions(() =>
		{
			AssertType<HolderOfTheTransitProcedureProvider>(Provider.HolderOfTheTransitProcedure);
			AssertNotNull(Provider.HolderOfTheTransitProcedure);
		});
	}

	public void TestCorrelationIdentifierWithCIDEntryNum()
	{
		AssertNull(Provider.CorrelationIdentifier);
	}

	public void TestCustomsOfficesOfTransit()
	{
		AssertNotNull(Provider.CustomsOfficesOfTransit);
	}

	public void TestCustomsOfficesOfExitForTransit()
	{
		AssertNotNull(Provider.CustomsOfficesOfExitForTransit);
	}

	public void TestTransitOperation()
	{
		AssertNotNull(Provider.TransitOperation);
	}

	public void TestAuthorisations()
	{
		AssertNotNull(Provider.Authorisations);
	}

	public void TestGuarantees()
	{
		AssertNotNull(Provider.Guarantees);
		AssertType<IGuarantee[]>(Provider.Guarantees);
	}

	public void TestConsignment()
	{
		AssertType<ConsignmentProvider>(Provider.Consignment);
	}

	protected override string MovementType => NctsMovementType.Codes.Departure;
}
