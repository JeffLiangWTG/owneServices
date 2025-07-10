using System;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class CC566RootProviderTest : AESBaseProviderTest<CC566RootProvider>
{
	public override void TestConstructor() => CombineAssertions(() =>
	{
		AssertExceptionThrown<ArgumentNullException>("Null BaseMessageSendingObject", "Value cannot be null.\r\nParameter name: sendingObject", () => new CC566RootProvider(sendingObject: null));
		AssertExceptionThrown<ArgumentNullException>("Null Related Declaration", "Value cannot be null.\r\nParameter name: EntryHeader.Declaration", () => new CC566RootProvider(new(Factory.New<CusEntryHeader>())));
		AssertExceptionThrown<ArgumentNullException>("Null Related Instruction", "Value cannot be null.\r\nParameter name: EntryHeader.EntryInstruction", () => new CC566RootProvider(new(Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew())));
		AssertNoExceptionThrown("Valid data", () => new CC515RootProvider(new(entryHeader)));
	});

	public void TestExportOperation() => AssertType<CC566ExportOperationProvider>(Provider.ExportOperation);

	public void TestPdWResponse() => AssertType<PdWResponseProvider>(Provider.PdWResponse);

	public new void TestCorrelationIdentifier() => CombineAssertions(() =>
	{
		sendingObject.ResponseMessage = null;
		AssertNull("ResponseMessage is not defined.", GetProvider().CorrelationIdentifier);

		sendingObject.ResponseMessage = "TstMessageNum";
		AssertEquals("ResponseMessage selected.", expected: "TstMessageNum", actual: GetProvider().CorrelationIdentifier);
	});

	protected override string ExpectedMessageType => Constants.MessageType.AES.CC566C;

	protected override CC566RootProvider GetProvider() => new(sendingObject);
}
