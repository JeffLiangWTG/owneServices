using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class CC513RootProviderTest : CC515513RootProviderTest<CC513RootProvider, ICC513CExportOperation>
{
	public override void TestConstructor() => CombineAssertions(() =>
	{
		AssertExceptionThrown<ArgumentNullException>("Null BaseMessageSendingObject", "Value cannot be null.\r\nParameter name: sendingObject", () => new CC513RootProvider(sendingObject: null));
		AssertExceptionThrown<ArgumentNullException>("Null Related Declaration", "Value cannot be null.\r\nParameter name: EntryHeader.Declaration", () => new CC513RootProvider(new BaseMessageSendingObject(Factory.New<CusEntryHeader>())));
		AssertExceptionThrown<ArgumentNullException>("Null Related Instruction", "Value cannot be null.\r\nParameter name: EntryHeader.EntryInstruction", () => new CC513RootProvider(new BaseMessageSendingObject(Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew())));
		AssertNoExceptionThrown("Valid data", () => new CC513RootProvider(new BaseMessageSendingObject(entryHeader)));
	});

	protected override string ExpectedMessageType => Constants.MessageType.AES.CC513C;

	protected override CC513RootProvider GetProvider() => new CC513RootProvider(sendingObject);
}
