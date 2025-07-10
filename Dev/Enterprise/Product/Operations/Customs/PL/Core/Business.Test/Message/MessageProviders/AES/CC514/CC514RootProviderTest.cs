using System;
using Enterprise.Customs.PL.Business.Declaration;
using CusEntryHeader = Enterprise.Customs.PL.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class CC514RootProviderTest : AESBaseProviderWithPartiesBaseTest<CC514RootProvider>
{
	public override void TestConstructor() => CombineAssertions(() =>
	{
		AssertExceptionThrown<ArgumentNullException>("Null BaseMessageSendingObject", "Value cannot be null.\r\nParameter name: sendingObject", () => new CC514RootProvider(sendingObject: null));
		AssertExceptionThrown<ArgumentNullException>("Null Related Declaration", "Value cannot be null.\r\nParameter name: EntryHeader.Declaration", () => new CC514RootProvider(new BaseMessageSendingObject(Factory.New<CusEntryHeader>())));
		AssertExceptionThrown<ArgumentNullException>("Null Related Instruction", "Value cannot be null.\r\nParameter name: EntryHeader.EntryInstruction", () => new CC514RootProvider(new BaseMessageSendingObject(Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew())));
		AssertNoExceptionThrown("Valid data", () => new CC514RootProvider(new BaseMessageSendingObject(entryHeader)));
	});

	public void TestExportOperation() => AssertNotNull(Provider.ExportOperation);

	public void TestCustomsOfficeOfExport() => AssertNotNull(Provider.CustomsOfficeOfExport);

	protected override string ExpectedMessageType => Constants.MessageType.AES.CC514C;

	protected override CC514RootProvider GetProvider() => new CC514RootProvider(sendingObject);
}
