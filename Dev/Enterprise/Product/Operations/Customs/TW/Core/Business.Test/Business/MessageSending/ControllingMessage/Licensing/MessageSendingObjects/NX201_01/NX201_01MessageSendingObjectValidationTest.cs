using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX201_01MessageSendingObjectValidation))]
	sealed class NX201_01MessageSendingObjectValidationTest : MessageSendingObjectValidationTest
	{
		public void TestValidateShouldSend()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			var messageSendingObject = new NX201_01MessageSendingObject(header);
			var actionForReasonDescriptionList = new ZString[] { NX201_01ActionCodeList.Codes._5, NX201_01ActionCodeList.Codes._17, NX201_01ActionCodeList.Codes._52 };
			var expectedWarningMessage = "Reason Description is required when Action is '5' or '17' or '52'.";
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

		public void TestCheckAction()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			var messageSendingObject = new NX201_01MessageSendingObject(header);
			var actionForPermitNumberList = new ZString[] { NX201_01ActionCodeList.Codes._5, NX201_01ActionCodeList.Codes._17, NX201_01ActionCodeList.Codes._52 };
			var actionForProcessingNumberList = new ZString[] { NX201_01ActionCodeList.Codes._4, NX201_01ActionCodeList.Codes._17 };
			var permitNumberErrorMessage = "Please enter a Permit Number.";
			var processingNumberErrorMessage = "Please enter a Processing Number.";
			var targetInfo = messageSendingObject.ActionInfo;

			foreach (var action in actionForPermitNumberList)
			{
				header.PermitNumber = ZString.Empty;
				messageSendingObject.Action = action;
				AssertHasErrorContaining(targetInfo, permitNumberErrorMessage);
				messageSendingObject.Action = NX201_01ActionCodeList.Codes._4;
				AssertNoError(targetInfo, permitNumberErrorMessage);
				header.PermitNumber = "A";
				messageSendingObject.Action = action;
				AssertNoError(targetInfo, permitNumberErrorMessage);
			}

			foreach (var action in actionForProcessingNumberList)
			{
				header.ProcessingNumber = ZString.Empty;
				messageSendingObject.Action = action;
				AssertHasErrorContaining(targetInfo, processingNumberErrorMessage);
				messageSendingObject.Action = NX201_01ActionCodeList.Codes._5;
				AssertNoError(targetInfo, processingNumberErrorMessage);
				header.ProcessingNumber = "A";
				messageSendingObject.Action = action;
				AssertNoError(targetInfo, processingNumberErrorMessage);
			}
		}
	}
}
