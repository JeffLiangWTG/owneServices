using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class NctsDepartureMovementHeaderLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestTypeOfSecurityList()
	{
		AssertEquals("Values", "NON, EXI", lookups.TypeOfSecurityList.CodesAsString);
		AssertSame("Cached", lookups.TypeOfSecurityList, lookups.TypeOfSecurityList);
	}

	public void TestNctsTransitStatusList()
	{
		AssertEquals("Values", new EU.NCTS.Business.CodeDescriptionPairLists.NCTS5DepartureCustomsStatusList().CodesAsString + ", " + NCTSDepartureCustomsStatusList.Codes.EmergencyProcedure, lookups.NctsTransitStatusList.CodesAsString);
		AssertSame("Cached", lookups.NctsTransitStatusList, lookups.NctsTransitStatusList);
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		movementHeader = nctsHeader.MovementHeader;
		lookups = movementHeader.Lookups;
	}
	NctsHeader nctsHeader;
	NctsDepartureMovementHeader movementHeader;
	NctsDepartureMovementHeaderLookups lookups;
}
