using System;
using System.Linq;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.NL.Business.Common;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(CC013CProvider))]
sealed class CC013CProviderTest : MessageHeaderProviderAbstractTest<CC013CProvider>
{
	public void TestConstructor() => AssertExceptionThrown<ArgumentNullException>(() => new CC013CProvider((NctsHeader)null));

	public void TestTransitOperation() => CombineAssertions(() =>
	{
		AssertNotNull("Not Null", Provider.TransitOperation);
		AssertType<CC013CTransitOperationProvider>("Type", Provider.TransitOperation);
	});

	public void TestAuthorisations() => CombineAssertions(() =>
	{
		AssertNotNull("Not Null", Provider.Authorisations);
		AssertType<AuthorizationProvider[]>("Type", Provider.Authorisations);
	});

	public void TestCustomsOfficeOfDeparture()
	{
		var movementHeader = nctsHeader.CommonMovementHeader;
		var customsOfficeOfDeparture = movementHeader.CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);
		customsOfficeOfDeparture.CY_Data = "DepID";

		AssertEquals("DepID", Provider.CustomsOfficeOfDeparture);
	}

	public void TestCustomsOfficeOfDestination()
	{
		var movementHeader = nctsHeader.MovementHeader;
		var customsOfficeOfDestination = movementHeader.CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
		customsOfficeOfDestination.CY_Data = "DepID";

		AssertEquals("DepID", Provider.CustomsOfficeOfDestination);
	}

	public void TestCustomsOfficesOfTransit() => CombineAssertions(() =>
	{
		AssertNotNull("Not Null", Provider.CustomsOfficesOfTransit);
		AssertType<ICustomsOfficeOfTransit[]>("Type", Provider.CustomsOfficesOfTransit);
	});

	public void TestCustomsOfficesOfExitForTransit() => CombineAssertions(() =>
	{
		AssertNotNull("Not Null", Provider.CustomsOfficesOfExitForTransit);
		AssertType<ICustomsOfficeOfExitForTransit[]>("Type", Provider.CustomsOfficesOfExitForTransit);
	});

	public void TestHolderOfTheTransitProcedure() => CombineAssertions(() =>
	{
		AssertNotNull("Not Null", Provider.HolderOfTheTransitProcedure);
		AssertType<HolderOfTheTransitProcedureProvider>("Type", Provider.HolderOfTheTransitProcedure);
	});

	public void TestRepresentative() => CombineAssertions(() =>
	{
		AssertNull("JobDocAddress not available", Provider.Representative);
		NCTSTestHelper.CreateJobDocAddressForTest(Factory, "REP", nctsHeader.MovementHeader.Representative, "1", traderTir: "GBR/022/1234567");
		AssertNotNull("JobDocAddress available", Provider.Representative);
	});

	public void TestGuarantees() => CombineAssertions(() =>
	{
		AssertNotNull("Not Null", Provider.Guarantees);
		AssertType<IGuarantee[]>("Type", Provider.Guarantees);
	});

	public void TestConsignment()
	{
		AssertType<CC013CConsignmentProvider>(Provider.Consignment);
	}

	protected override string MessageType => NLConstants.WCoTypeCodes.Amendment;

	protected override string MovementType => NctsMovementType.Codes.Departure;
}
