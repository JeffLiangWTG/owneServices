namespace Enterprise.Customs.PL.Business.Testing;

sealed class CommodityProvider_CC515_CC513Test : AESCommodityProviderTest
{
	public new void TestConstructor() => CombineAssertions(() =>
	{
		AssertNull("Null entryLine", CommodityProvider_CC515_CC513.NewOrNull(null));
		AssertNotNull("EntryLine is not null", CommodityProvider_CC515_CC513.NewOrNull(EntryLine));
	});

	protected override AESCommodityProvider GetProvider() => CommodityProvider_CC515_CC513.NewOrNull(EntryLine);

	public override void TestDangerousGoods()
	{
		base.TestDangerousGoods();

		AesRuleTestHelper.TestR0089E(Instruction, () => GetProvider().DangerousGoods);
	}
}
