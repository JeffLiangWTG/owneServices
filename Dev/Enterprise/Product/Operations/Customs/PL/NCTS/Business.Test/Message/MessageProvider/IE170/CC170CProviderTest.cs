using System;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.Universal.Constants;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class CC170CProviderTest : Customs.Business.Testing.DataProviderTestCase<CC170CProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null NctsDepartureMovementHeader", "Value cannot be null.\r\nParameter name: movementHeader",
				() => new CC170CProvider(null, null));
			AssertExceptionThrown<ArgumentNullException>("Null Message Type", "Value cannot be null.\r\nParameter name: messageType",
				() => new CC170CProvider(movementHeader, null));
			AssertNoExceptionThrown("Valid constructor args", () => new CC170CProvider(movementHeader, string.Empty));
		});
	}

	public void TestTransitOperationLRN()
	{
		movementHeader.BM_PaperlessInbondNum = "ABC123";
		AssertEquals("The value should be equal to LRN PlaceHolder", EDIMessage.PL_NCTS_LRN_PlaceHolder, GetProvider().TransitOperationLRN);
	}

	public void TestTransitOperationLimitDate()
	{
		CombineAssertions(() =>
		{
			movementHeader.IsSimplifiedNctsProcedure = false;
			movementHeader.BM_ExportDate = ZDateTime.Empty;
			AssertNull("Should be null when BM_ExportDate is empty", GetProvider().TransitOperationLimitDate);

			var testDateTime = DateTime.Now.AddDays(3);
			movementHeader.BM_ExportDate = testDateTime;
			AssertNull("Should be null when is not simplified procedure", GetProvider().TransitOperationLimitDate);

			movementHeader.IsSimplifiedNctsProcedure = true;
			AssertEquals("The value should be equal to BM_ExportDate and is simplified procedure", testDateTime, GetProvider().TransitOperationLimitDate);
		});
	}

	public void TestCustomsOfficeOfDeparture()
	{
		CombineAssertions(() =>
		{
			AssertNull("Header does not contain any offices with code NCTSOfficeOfDeparture", GetProvider().CustomsOfficeOfDeparture);

			AddCustomsOfficeOfType(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);
			AssertNotNull("Header contains an offices with code NCTSOfficeOfDeparture", GetProvider().CustomsOfficeOfDeparture);
		});
	}
	public void TestHolderOfTheTransitProcedure()
	{
		CombineAssertions(() =>
		{
			AssertNotNull(Provider.HolderOfTheTransitProcedure);

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "Abba";

			var outNCTSTPPeriod = 70;
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
						RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
			{
				AssertEquals("NameMaxLength for IE170 is always 70 no matter if its in or outside transition period", outNCTSTPPeriod, GetProvider().HolderOfTheTransitProcedure.NameMaxLength);

				nctsHeader.Principal.OrganisationPK = orgHeader.PK;
				AssertEquals("StreetAndNumberMaxLength for IE170 is always 70 no matter if its in or outside transition period", outNCTSTPPeriod, GetProvider().HolderOfTheTransitProcedure.Address.StreetAndNumberMaxLength);
			}
		});
	}

	public void TestRepresentative() => AssertNotNull(Provider.Representative);

	public void TestConsignment() => AssertNotNull(Provider.Consignment);

	public void TestPhaseID()
	{
		TestHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
		{
			AssertEquals("PhaseID outside Transition Period", CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS.PhaseID.NCTS_5_1, GetProvider().PhaseID);
		});

		TestHelper.RunAssertionsInPhase5TransitionPeriod(() =>
		{
			AssertEquals("PhaseID in Transition Period", CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS.PhaseID.NCTS_5_0, GetProvider().PhaseID);
		});
	}

	protected override CC170CProvider GetProvider() => new CC170CProvider(movementHeader, "Type");

	NctsPLOfficeCode AddCustomsOfficeOfType(ZString officeCode) => nctsHeader.MovementHeader.CustomsOffices.AddNew(officeCode);

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		nctsHeader.MovementHeader.CustomsOffices.RemoveAll();
		movementHeader = nctsHeader.MovementHeader;
	}

	NctsHeader nctsHeader;
	NctsDepartureMovementHeader movementHeader;
}
