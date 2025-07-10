using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class CC013CProviderTest : Customs.Business.Testing.DataProviderTestCase<CC013CProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null NctsDepartureMovementHeader", "Value cannot be null.\r\nParameter name: movementHeader", () => new CC013CProvider(null, null, null));
			AssertExceptionThrown<ArgumentNullException>("Null Message Type", "Value cannot be null.\r\nParameter name: messageType", () => new CC013CProvider(movementHeader, null, null));
			AssertExceptionThrown<ArgumentNullException>("Null MessageSendingObject", "Value cannot be null.\r\nParameter name: messageSendingObject", () => new CC013CProvider(movementHeader, string.Empty, null));
			AssertNoExceptionThrown("Valid constructor args", () => new CC013CProvider(movementHeader, string.Empty, messageSendingObject));
		});
	}

	public void TestTransitOperation() => AssertNotNull(Provider.TransitOperation);

	public void TestCustomsOfficeOfDeparture()
	{
		CombineAssertions(() =>
		{
			AssertNull("No NCTSOfficeOfDeparture", GetProvider().CustomsOfficeOfDeparture);

			AddCustomsOfficeOfType(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);
			AssertNotNull("NCTSOfficeOfDeparture", GetProvider().CustomsOfficeOfDeparture);
		});
	}

	public void TestCustomsOfficeOfDestinationDeclared()
	{
		CombineAssertions(() =>
		{
			AssertNull("No NCTSOfficeOfDestination", GetProvider().CustomsOfficeOfDestinationDeclared);

			AddCustomsOfficeOfType(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
			AssertNotNull("NCTSOfficeOfDestination", GetProvider().CustomsOfficeOfDestinationDeclared);
		});
	}

	public void TestCustomsOfficeOfTransitDeclared()
	{
		TestHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
		{
			AssertEquals("No NCTSOfficeOfTransit", 0, GetProvider().CustomsOfficeOfTransitDeclared.Count);

			AddCustomsOfficeOfType(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
			AddCustomsOfficeOfType(OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit);
			AssertEquals("NCTSOfficeOfTransit", 2, GetProvider().CustomsOfficeOfTransitDeclared.Count);
			AssertEquals("sequence number should start with 1", "1", GetProvider().CustomsOfficeOfTransitDeclared.First().SequenceNumber);
			AssertEquals("2nd NCTSOfficeOfTransit should have sequence number 2", "2", GetProvider().CustomsOfficeOfTransitDeclared.Last().SequenceNumber);

			movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.TIR;
			AssertEquals("C0030 - BM_InBondEntryType is TIR", 0, GetProvider().CustomsOfficeOfTransitDeclared.Count);

			movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T2SM;
			AssertEquals("C0030 - BM_InBondEntryType is T2SM", 0, GetProvider().CustomsOfficeOfTransitDeclared.Count);
		});

		TestHelper.RunAssertionsInPhase5TransitionPeriod(() =>
		{
			movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.TIR;
			AssertEquals("B1836 - BM_InBondEntryType is TIR", 0, GetProvider().CustomsOfficeOfTransitDeclared.Count);

			movementHeader.BM_InBondEntryType = ZString.Empty;
			AssertEquals("B1836 - BM_InBondEntryType is not TIR", 2, GetProvider().CustomsOfficeOfTransitDeclared.Count);

			movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T2SM;
			AssertEquals("B1836 - C0030 should be disabled - BM_InBondEntryType is T2SM", 2, GetProvider().CustomsOfficeOfTransitDeclared.Count);
		});
	}

	public void TestCustomsOfficeOfExitForTransitDeclared()
	{
		CombineAssertions(() =>
		{
			AssertEquals("No NCTSOfficeOfExitForTransit", 0, GetProvider().CustomsOfficeOfExitForTransitDeclared.Count);

			var office1 = AddCustomsOfficeOfType(OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit);
			office1.CY_Data = "PL123456";
			var office2 = AddCustomsOfficeOfType(OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit);
			office2.CY_Data = "DE987654";
			AssertEquals("C0587 - NCTSOfficeOfExitForTransit", 0, GetProvider().CustomsOfficeOfExitForTransitDeclared.Count);

			movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
			AssertEquals("BM_TypeOfSecurity is EXI", 2, GetProvider().CustomsOfficeOfExitForTransitDeclared.Count);
			AssertEquals("sequence number should start with 1", "1", GetProvider().CustomsOfficeOfExitForTransitDeclared.First().SequenceNumber);
			AssertEquals("2nd NCTSOfficeOfExitForTransit should have sequence number 2", "2", GetProvider().CustomsOfficeOfExitForTransitDeclared.Last().SequenceNumber);

			movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
			AssertEquals("BM_TypeOfSecurity is BTH", 2, GetProvider().CustomsOfficeOfExitForTransitDeclared.Count);

			AddCustomsOfficeOfType(OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit);
			AssertEquals("Empty office added count should not change", 2, GetProvider().CustomsOfficeOfExitForTransitDeclared.Count);
		});
	}

	public void TestHolderOfTheTransitProcedure() => AssertNotNull(Provider.HolderOfTheTransitProcedure);

	public void TestRepresentative() => AssertNotNull(Provider.Representative);

	public void TestConsignment() { AssertNotNull(Provider.Consignment); }

	public void TestGuarantee()
	{
		CombineAssertions(() =>
		{
			AssertEquals("No Guarantee", 0, GetProvider().Guarantee.Count);

			var guarantee1 = movementHeader.Guarantees.AddNew();
			var guarantee2 = movementHeader.Guarantees.AddNew();
			var guarantee3 = movementHeader.Guarantees.AddNew();
			AssertEquals("1 unique PW_BondType and PW_BondNumber2 Guarantee", 1, GetProvider().Guarantee.Count);

			guarantee2.PW_BondNumber2 = "A";
			guarantee3.PW_BondType = "A";
			AssertEquals("3 unique Guarantee", 3, GetProvider().Guarantee.Count);
			AssertEquals("sequence number should start with 1", "1", GetProvider().Guarantee.First().SequenceNumber);
			AssertEquals("3rd Guarantee should have sequence number 3", "3", GetProvider().Guarantee.Last().SequenceNumber);
		});
	}

	public void TestPhaseID() => AssertNull(Provider.PhaseID);

	NctsPLOfficeCode AddCustomsOfficeOfType(ZString officeCode) => movementHeader.CustomsOffices.AddNew(officeCode);

	protected override CC013CProvider GetProvider()
	{
		return new CC013CProvider(movementHeader, string.Empty, messageSendingObject);
	}

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		movementHeader = nctsHeader.MovementHeader;
		messageSendingObject = new MessageSendingObject(nctsHeader);
		movementHeader.CustomsOffices.RemoveAll();
	}

	NctsHeader nctsHeader;
	NctsDepartureMovementHeader movementHeader;
	MessageSendingObject messageSendingObject;
}
