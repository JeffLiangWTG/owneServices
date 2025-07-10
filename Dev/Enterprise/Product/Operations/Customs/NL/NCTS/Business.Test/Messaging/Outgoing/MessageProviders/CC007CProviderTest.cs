using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(CC007CProvider))]
sealed class CC007CProviderTest : MessageHeaderProviderAbstractTest<CC007CProvider>
{
	public void TestConstructor() => AssertExceptionThrown<ArgumentNullException>(() => new CC007CProvider(null));

	public void TestTransitOperation() => CombineAssertions(() =>
	{
		AssertNotNull(Provider.TransitOperation);
		AssertType<TransitOperationProvider>(Provider.TransitOperation);
	});

	public void TestAuthorisations() => CombineAssertions(() =>
	{
		nctsHeader.CusAuthorizationUsages.AddNew();
		AssertNotNull(Provider.Authorisations);
		AssertEquals("Number of Authorisations", 1, Provider.Authorisations.Count);
	});

	public void TestCustomsOfficeOfDestination()
	{
		nctsHeader.ArrivalMovementHeader.DestinationCustomsOfficeCodeForArrival = "CusOffID";
		AssertEquals("CustomsOfficeOfDestination", "CusOffID", Provider.CustomsOfficeOfDestination);
	}

	public void TestTraderIdentificationNumber()
	{
		NCTSTestHelper.CreateJobDocAddressForTest(Factory, "IMD", nctsHeader.DestinationTrader, countryCode: "NL", traderTin: "152425232B01", suffix: "");
		CreateProvider();
		AssertEquals("TraderIdentificationNumber", "NL152425232B01", Provider.TraderIdentificationNumber);
	}

	public void TestTraderCommunicationLanguage() => AssertEquals("en", provider.TraderCommunicationLanguage);

	public void TestConsignment() => CombineAssertions(() =>
	{
		AssertNotNull(Provider.Consignment);
		AssertType<ConsignmentProvider>(Provider.Consignment);
	});

	protected override string MessageType => "CC007C";

	protected override string MovementType => NctsMovementType.Codes.Arrival;
}
