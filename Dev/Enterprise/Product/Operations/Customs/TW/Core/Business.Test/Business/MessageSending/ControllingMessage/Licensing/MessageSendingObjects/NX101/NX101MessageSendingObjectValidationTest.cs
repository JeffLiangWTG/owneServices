using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX101MessageSendingObjectValidation))]
	sealed class NX101MessageSendingObjectValidationTest : LicensingMessageSendingObjectValidationAbstractTest<NX101MessageSendingObject>
	{
		public override void TestCheckAction()
		{
			base.TestCheckAction();

			var warningMessage = "You have entered Previous Permit Number, if you want to apply for a replacement or lost replacement, please select '18' or '17'.";
			var errorMessage = "When Return Previous COO is false, you must apply for Special Application.";
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			var messageSendingObject = new NX101MessageSendingObject(header);
			messageSendingObject.Action = NX101ActionCodeList.Codes._9;
			AssertNoWarning(messageSendingObject.ActionInfo, warningMessage);

			header.TW1_PrePermitNumber = "X1";
			messageSendingObject.Validation.ValidateAction();
			AssertHasWarning(messageSendingObject.ActionInfo, warningMessage);

			messageSendingObject.Action = NX101ActionCodeList.Codes._18;
			AssertNoWarning(messageSendingObject.ActionInfo, warningMessage);
			AssertHasMessageError(messageSendingObject.ActionInfo, errorMessage);

			header.TW1_ReturnPreviousCOO = true;
			messageSendingObject.Validation.ValidateAction();
			AssertNoMessageError(messageSendingObject.ActionInfo, errorMessage);

			header.TW1_ReturnPreviousCOO = false;
			header.TW1_IsSpecialApplication = true;
			messageSendingObject.Validation.ValidateAction();
			AssertNoMessageError(messageSendingObject.ActionInfo, errorMessage);

			var errorText = "Code 9 - Create does not support Certificate Type 13.";
			header.TW1_CertificateType = CertificateTypeList.Codes.Code13;
			messageSendingObject.Action = NX101ActionCodeList.Codes._9;
			AssertHasError(messageSendingObject.ActionInfo, errorText);

			messageSendingObject.Action = NX101ActionCodeList.Codes._17;
			AssertNoError(messageSendingObject.ActionInfo, errorText);

			header.TW1_CertificateType = CertificateTypeList.Codes.Code5;
			messageSendingObject.Action = NX101ActionCodeList.Codes._9;
			AssertNoError(messageSendingObject.ActionInfo, errorText);

			header.TW1_PrePermitNumber = "123";
			errorMessage = "You have not entered a Previous Permit No.";
			messageSendingObject.Action = NX101ActionCodeList.Codes._17;
			AssertNoMessageError(messageSendingObject.ActionInfo, errorMessage);

			header.TW1_PrePermitNumber = ZString.Empty;
			messageSendingObject.Validation.ValidateAction();
			AssertHasMessageError(messageSendingObject.ActionInfo, errorMessage);

			messageSendingObject.Action = NX101ActionCodeList.Codes._18;
			AssertHasMessageError(messageSendingObject.ActionInfo, errorMessage);
		}
	}
}
