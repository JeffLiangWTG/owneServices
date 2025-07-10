using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class CC515RootProviderTest : CC515513RootProviderTest<CC515RootProvider, ICC515CExportOperation>
{
	public override void TestConstructor() => CombineAssertions(() =>
	{
		AssertExceptionThrown<ArgumentNullException>("Null BaseMessageSendingObject", "Value cannot be null.\r\nParameter name: sendingObject", () => new CC515RootProvider(sendingObject: null));
		AssertExceptionThrown<ArgumentNullException>("Null Related Declaration", "Value cannot be null.\r\nParameter name: EntryHeader.Declaration", () => new CC515RootProvider(new BaseMessageSendingObject(Factory.New<CusEntryHeader>())));
		AssertExceptionThrown<ArgumentNullException>("Null Related Instruction", "Value cannot be null.\r\nParameter name: EntryHeader.EntryInstruction", () => new CC515RootProvider(new BaseMessageSendingObject(Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew())));
		AssertNoExceptionThrown("Valid data", () => new CC515RootProvider(new BaseMessageSendingObject(entryHeader)));
	});

	public override void TestCurrencyExchange()
	{
		base.TestCurrencyExchange();
		AesRuleTestHelper.TestR0089E(instruction, () => GetProvider().CurrencyExchange);
	}

	public void TestExportOperationType() => AssertType<CC515ExportOperationProvider>(Provider.ExportOperation);

	public void TestGoodsShipmentType() => AssertType<GoodsShipmentProvider_CC515_CC513>(Provider.GoodsShipment);

	protected override string ExpectedMessageType => Constants.MessageType.AES.CC515C;

	protected override CC515RootProvider GetProvider() => new CC515RootProvider(sendingObject);
}
