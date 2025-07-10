using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX201_07MessageSendingObjectValidation))]
	sealed class NX201_07MessageSendingObjectValidationTest : MessageSendingObjectValidationTest
	{
		public void TestValidateShouldSend()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			var messageSendingObject = new NX201_07MessageSendingObject(header);
			var actionForReasonDescriptionList = new ZString[] { "1", "50" };
			var expectedWarningMessage = "Reason Description is required when Action is '1' or '50'.";
			foreach (var action in actionForReasonDescriptionList)
			{
				messageSendingObject.ShouldSend = false;
				messageSendingObject.ReasonDescription = ZString.Empty;
				messageSendingObject.Action = action;
				messageSendingObject.ShouldSend = true;
				AssertHasErrorContaining(messageSendingObject.ShouldSendInfo, expectedWarningMessage);
				messageSendingObject.ShouldSend = false;
				messageSendingObject.ReasonDescription = "A";
				messageSendingObject.ShouldSend = true;
				AssertNoError(messageSendingObject.ShouldSendInfo, expectedWarningMessage);
			}

			messageSendingObject.ShouldSend = false;
			messageSendingObject.Action = "9";
			messageSendingObject.ReasonDescription = ZString.Empty;
			messageSendingObject.ShouldSend = true;
			AssertNoError(messageSendingObject.ShouldSendInfo, expectedWarningMessage);

			var document = messageSendingObject.SupportingDocuments.AddNew();
			NXMMessageSendingObjectValidationTestShared.TestValidateShouldSend_LineNumber(document);
		}
	}
}
