using System.Linq;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class GoodsShipmentProvider_CC515_CC513Test : AESGoodsShipmentProviderTest
{
	protected override AESGoodsShipmentProvider GetProvider() => new GoodsShipmentProvider_CC515_CC513(EntryHeader);

	public override void TestAdditionalReferences()
	{
		base.TestAdditionalReferences();
		AesRuleTestHelper.TestR0089E(EntryInstruction, () => GetProvider().AdditionalReferences);
	}

	public override void TestDeliveryTerms()
	{
		base.TestDeliveryTerms();
		AesRuleTestHelper.TestR0089E(EntryInstruction, () => GetProvider().DeliveryTerms);
	}

	public override void TestNatureOfTransaction()
	{
		base.TestNatureOfTransaction();

		using (Declaration.TemporarilySetIsAESTransitionPeriod(false))
		{
			AesRuleTestHelper.TestR0089E(EntryInstruction, () => GetProvider().NatureOfTransaction);
		}

		using (Declaration.TemporarilySetIsAESTransitionPeriod(true))
		{
			AesRuleTestHelper.TestR0089E(EntryInstruction, () => GetProvider().NatureOfTransaction);
		}
	}

	public void TestConsignmentType() => AssertType<ConsignmentProvider_CC515_CC513>(GetProvider().AESConsignment);

	public void TestGoodsItemType() => CombineAssertions(() =>
	{
		var firstItem = GetProvider().GoodsItems.First();
		AssertType<GoodsItemProvider_CC515_CC513>(firstItem);
		TestHelper.TestCollectionHasCorrectIndexOrder(GetProvider().GoodsItems, (item) => item.DeclarationGoodsItemNumber);
	});
}
