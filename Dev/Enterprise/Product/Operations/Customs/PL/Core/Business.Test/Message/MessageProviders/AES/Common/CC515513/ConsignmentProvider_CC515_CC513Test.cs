using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class ConsignmentProvider_CC515_CC513Test : AESConsignmentProviderTest
{
	protected override AESConsignmentProvider GetProvider() => new ConsignmentProvider_CC515_CC513(EntryHeader);

	public override void TestCarrierIdentificationNumber()
	{
		base.TestCarrierIdentificationNumber();
		AesRuleTestHelper.TestR0089E(EntryInstruction, () => GetProvider().CarrierIdentificationNumber);
	}

	public override void TestConsignor()
	{
		base.TestConsignor();
		AesRuleTestHelper.TestR0089E(EntryInstruction, () => GetProvider().Consignor);
	}

	public override void TestCountryOfRoutingOfConsignments()
	{
		base.TestCountryOfRoutingOfConsignments();
		AesRuleTestHelper.TestR0089E(EntryInstruction, () => GetProvider().CountryOfRoutingOfConsignments);
	}

	public override void TestTransportChargesMethodOfPayment()
	{
		base.TestTransportChargesMethodOfPayment();
		AesRuleTestHelper.TestR0089E(EntryInstruction, () => GetProvider().TransportChargesMethodOfPayment);
	}

	public override void TestTransportDocument()
	{
		base.TestTransportDocument();
		AesRuleTestHelper.TestR0089E(EntryInstruction, () => GetProvider().TransportDocument);
	}

	public override void TestConsigneeType()
	{
		var org = Factory.New<OrgHeader>();
		var orgAddress = org.Addresses.AddNew();
		declaration.ImporterDocumentaryAddress.E2_OA_Address = orgAddress.PK;

		AssertType<ConsignmentConsigneeConsignorProvider_CC515_CC513>(GetProvider().Consignee);
	}
}
