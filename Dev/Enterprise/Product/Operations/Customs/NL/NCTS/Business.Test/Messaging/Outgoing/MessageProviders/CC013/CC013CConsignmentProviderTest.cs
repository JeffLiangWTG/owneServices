using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(CC013CConsignmentProvider))]
sealed class CC013CConsignmentProviderTest : ConsignmentProviderAbstractTest<CC013CConsignmentProvider>
{
	public void TestConsignor()
	{
		var movementHeader = nctsHeader.MovementHeader;
		var consignor = movementHeader.DocAddresses.AddNew();
		consignor.E2_AddressType = AutoDocAddressTypes.Codes.ConsignorDocumentaryAddress;
		AssertNotNull("BM_ReducedDatasetIndicator is false", Provider.Consignor);

		movementHeader.BM_ReducedDatasetIndicator = true;
		AssertNull("BM_ReducedDatasetIndicator is true", Provider.Consignor);
	}

	public void TestInlandModeOfTransport()
	{
		var movementHeader = nctsHeader.MovementHeader;
		movementHeader.BM_InlandTransportMode = "1";
		AssertEquals("BM_ReducedDatasetIndicator is false", 1, Provider.InlandModeOfTransport);

		movementHeader.BM_ReducedDatasetIndicator = true;
		AssertNull("BM_ReducedDatasetIndicator is true", Provider.InlandModeOfTransport);
	}

	protected override CC013CConsignmentProvider CreateProvider(NctsHeader nctsHeader) => new CC013CConsignmentProvider(nctsHeader);
}
