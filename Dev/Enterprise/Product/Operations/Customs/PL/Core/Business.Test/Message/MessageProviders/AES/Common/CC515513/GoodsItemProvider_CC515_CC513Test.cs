using CargoWise.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class GoodsItemProvider_CC515_CC513Test : AESGoodsItemProviderTest
{
	protected override AESGoodsItemProvider GetProvider() => new GoodsItemProvider_CC515_CC513(EntryLine, 999, new GoodsShipmentProvider_CC515_CC513(EntryHeader));

	public override void TestAdditionalReferences()
	{
		base.TestAdditionalReferences();
		AesRuleTestHelper.TestR0089E(EntryInstruction, () => GetProvider().AdditionalReferences);
	}

	public override void TestNatureOfTransaction()
	{
		base.TestNatureOfTransaction();
		using (Declaration.TemporarilySetIsAESTransitionPeriod(false))
		{
			AesRuleTestHelper.TestR0089E(EntryInstruction, () => GetProvider().NatureOfTransaction);
		}
	}

	public void TestNaturesOfTransaction_R0201()
	{
		var entryInstruction = Declaration.CustomsEntryInstructions.AddNew();
		var invoice1 = Declaration.Invoices.AddNew();
		invoice1.JZ_ValuationCode = "A";
		var invoice2 = Declaration.Invoices.AddNew();
		invoice2.JZ_ValuationCode = "A";
		var entryHeader = Declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		var entryLine1 = entryHeader.MergedLines.AddNew();
		var entryLine2 = entryHeader.MergedLines.AddNew();

		var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction.PK;
		invoiceLine1.JI_CL = entryLine1.PK;

		var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
		invoiceLine2.JI_CEI = entryInstruction.PK;
		invoiceLine2.JI_CL = entryLine2.PK;

		using var transitionPeriod = Declaration.TemporarilySetIsAESTransitionPeriod(false);
		CombineAssertions(() =>
		{
			var shipmentProvider = new GoodsShipmentProvider_CC515_CC513(entryHeader);
			AssertNotNullOrEmpty("Shipment NatureOfTransaction. InvoiceHeaders with same ValuationCode", shipmentProvider.NatureOfTransaction);
			shipmentProvider.GoodsItems.ForEach(goodItem
				=> AssertNullOrEmpty("GoodsItem NatureOfTransaction. InvoiceHeaders with same ValuationCode", goodItem.NatureOfTransaction));

			invoice2.JZ_ValuationCode = "B";
			shipmentProvider = new GoodsShipmentProvider_CC515_CC513(entryHeader);
			AssertNullOrEmpty("Shipment NatureOfTransaction. InvoiceHeaders with not same ValuationCode", shipmentProvider.NatureOfTransaction);
			shipmentProvider.GoodsItems.ForEach(goodItem
				=> AssertNotNullOrEmpty("GoodsItem NatureOfTransaction. InvoiceHeaders with not same ValuationCode", goodItem.NatureOfTransaction));
		});
	}

	public override void TestConsignor()
	{
		base.TestConsignor();
		AesRuleTestHelper.TestR0089E(EntryInstruction, () => GetProvider().Consignor);
	}

	public override void TestPreviousDocumentSpecialProcedures()
	{
		base.TestPreviousDocumentSpecialProcedures();
		AesRuleTestHelper.TestR0089E(EntryInstruction, () => GetProvider().PreviousDocumentSpecialProcedures);
	}

	public override void TestTransportDocuments()
	{
		base.TestTransportDocuments();
		using (Declaration.TemporarilySetIsAESTransitionPeriod(true))
		{
			AesRuleTestHelper.TestR0089E(EntryInstruction, () => GetProvider().TransportDocuments);
		}
	}

	public void TestCommodityType() => AssertType<CommodityProvider_CC515_CC513>(GetProvider().Commodity);

	public override void TestConsigneeType()
	{
		var orgHeader = Factory.New<OrgHeader>();
		var orgAddress = orgHeader.Addresses.AddNew();
		InvoiceLine.JI_OA_ConsigneeAddress = orgAddress.PK;

		AssertType<ConsigneeConsignorProvider_CC515_CC513>(GetProvider().Consignee);
	}
}
