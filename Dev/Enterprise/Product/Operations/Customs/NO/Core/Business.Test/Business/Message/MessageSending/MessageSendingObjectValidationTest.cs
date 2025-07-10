using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(MessageSendingObjectValidation))]
sealed class MessageSendingObjectValidationTest : BusinessObjectValidationTestCase
{
	public void TestMessageTypeValidation()
	{
		entryInstruction.CEI_Procedure = "4000";

		CombineAssertions(() =>
		{
			sendingObject.ShouldSend = true;
			entryHeader.CH_EntryStatus = ZString.Empty;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(sendingObject.MessageTypeInfo, MessageSendingMessageTypes.Codes.Correction, MessageSendingMessageTypes.Codes.CompleteOrdinaryDeclaration);

			entryHeader.CH_EntryStatus = UniversalReferenceConstants.CusEntryStatus.UAR;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(sendingObject.MessageTypeInfo, MessageSendingMessageTypes.Codes.Correction, MessageSendingMessageTypes.Codes.FinalDeclaration);

			entryHeader.CH_EntryStatus = MessageSendingStatusCodes.Codes.FinalApproval;
			ValidationTestHelper.AssertErrorIfEntered(sendingObject.MessageTypeInfo);

			entryHeader.CH_EntryStatus = MessageSendingStatusCodes.Codes.RefusalOfDeclaration;
			ValidationTestHelper.AssertErrorIfEntered(sendingObject.MessageTypeInfo);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		sendingObject = new MessageSendingObject(entryHeader);
		entryInstruction = Factory.NewWithValidTestData<CusEntryInstruction>();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
	}

	CusEntryHeader entryHeader;
	MessageSendingObject sendingObject;
	CusEntryInstruction entryInstruction;
}
