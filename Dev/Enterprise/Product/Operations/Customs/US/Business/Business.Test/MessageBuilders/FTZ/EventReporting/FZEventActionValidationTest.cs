using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class FZEventActionValidationTest : TestCaseWithFactory
	{
		public void TestSetDefaults()
		{
			GlbStaff.CurrentUser.GS_FullName = "Timothy Kensington-Double-Barrelled-Shotgun";
			GlbStaff.CurrentUser.GS_WorkPhone = "+18005550100";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			var action = new FZEventAction(declaration, FZEventType.Unconcur);
			AssertEquals("Contact Name truncated due to max length", "Timothy Kensington-Double-Barrelled-Shot", action.US_FTZContactName);
			AssertEquals("Contact Phone Number", "8005550100", action.US_FTZContactPhone);

			var branch = Factory.Load<GlbBranch>(Env.CurrentBranchPK);
			branch.GB_Phone = "+1 738 294 5000";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Dan Brown";
			staff.GS_WorkPhone = "+86 158 5050 3354";
			staff.GS_GB_HomeBranch = branch.PK;
			DataRegistry.Business.USCustomsDataRegistry.Instance.CargoReleaseFTZContact.SetValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, staff.PK.ToGuid());
			action = new FZEventAction(declaration, FZEventType.Unconcur);
			AssertEquals("Dan Brown", action.US_FTZContactName);
			AssertEquals("15850503354", action.US_FTZContactPhone);

			staff.GS_WorkPhone = ZString.Empty;
			action = new FZEventAction(declaration, FZEventType.Unconcur);
			AssertEquals("Dan Brown", action.US_FTZContactName);
			AssertEquals("7382945000", action.US_FTZContactPhone);
		}

		public void TestCheckUS_ActionCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_F_DeliveryCode = FTZDeliveryCodeList.Codes.PartialManifestReported;

			var action = new FZEventAction(declaration, FZEventType.Concur);
			action.US_ActionCode = "!";
			AssertHasMessageError(action.US_ActionCodeInfo, ListValidation.InvalidCodeMessageError);

			action.US_ActionCode = ZString.Empty;
			AssertHasMessageErrorContaining(action.US_ActionCodeInfo, MandatoryValidation.YouHaveNotEntered);

			action.US_ActionCode = FTZActionCodeList.Codes.A;
			AssertNoMessageError(action.US_ActionCodeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(action.US_ActionCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageError(action.US_ActionCodeInfo, FZEventActionValidation.FinalDeliveryRequired);

			declaration.US_F_DeliveryCode = FTZDeliveryCodeList.Codes.FinalManifestPortionReported;
			action.US_ActionCode = FTZActionCodeList.Codes.C;
			AssertNoMessageError(action.US_ActionCodeInfo, FZEventActionValidation.FinalDeliveryRequired);
		}

		public void TestCheckUS_ReasonCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			var action = new FZEventAction(declaration, FZEventType.Unconcur);
			action.US_ReasonCode = "!";
			AssertHasMessageError(action.US_ReasonCodeInfo, ListValidation.InvalidCodeMessageError);

			action.US_ReasonCode = ZString.Empty;
			AssertHasMessageErrorContaining(action.US_ReasonCodeInfo, MandatoryValidation.YouHaveNotEntered);

			action.US_ReasonCode = FTZUnconcurrenceReasonCodeList.Codes._01;
			AssertNoMessageError(action.US_ReasonCodeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(action.US_ReasonCodeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_FTZContactName()
		{
			var declaration = Factory.New<JobDeclaration>();
			var action = new FZEventAction(declaration, FZEventType.Unconcur);
			action.US_FTZContactName = ZString.Empty;
			AssertHasMessageErrorContaining(action.US_FTZContactNameInfo, MandatoryValidation.YouHaveNotEntered);

			action.US_FTZContactName = "Dan Brown";
			AssertNoMessageErrorContaining(action.US_FTZContactNameInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_FTZContactPhone()
		{
			var declaration = Factory.New<JobDeclaration>();
			var action = new FZEventAction(declaration, FZEventType.Unconcur);
			action.US_FTZContactPhone = ZString.Empty;
			AssertHasMessageErrorContaining(action.US_FTZContactPhoneInfo, MandatoryValidation.YouHaveNotEntered);

			action.US_FTZContactPhone = "0288018001";
			AssertNoMessageErrorContaining(action.US_FTZContactPhoneInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_Reasons()
		{
			var declaration = Factory.New<JobDeclaration>();
			var action = new FZEventAction(declaration, FZEventType.Unconcur);
			action.US_ReasonCode = FTZUnconcurrenceReasonCodeList.Codes._01;
			action.Validation.ValidateUS_Reasons();
			AssertNoMessageErrorContaining(action.US_ReasonsInfo, MandatoryValidation.YouHaveNotEntered);

			action.US_ReasonCode = FTZUnconcurrenceReasonCodeList.Codes._02;
			action.Validation.ValidateUS_Reasons();
			AssertHasMessageErrorContaining(action.US_ReasonsInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageError(action.US_ReasonsInfo, "You have not entered a Replacement In Bond Number.");

			action.US_Reasons = "123";
			AssertNoMessageError(action.US_ReasonsInfo, "You have not entered a Replacement In Bond Number.");

			action.US_ReasonCode = FTZUnconcurrenceReasonCodeList.Codes._03;
			action.Validation.ValidateUS_Reasons();
			AssertHasMessageError(action.US_ReasonsInfo, "You have not entered a Replacement FTZ Admission Number.");

			action.US_Reasons = "123";
			AssertNoMessageError(action.US_ReasonsInfo, "You have not entered a Replacement FTZ Admission Number.");

			action.US_ReasonCode = FTZUnconcurrenceReasonCodeList.Codes._04;
			action.Validation.ValidateUS_Reasons();
			AssertHasMessageError(action.US_ReasonsInfo, "You have not entered a Replacement Entry Number.");

			action.US_Reasons = "123";
			AssertNoMessageError(action.US_ReasonsInfo, "You have not entered a Replacement Entry Number.");
		}

		public void TestCheckUS_ReasonsCode02Format()
		{
			var declaration = Factory.New<JobDeclaration>();
			var action = new FZEventAction(declaration, FZEventType.Unconcur);
			action.US_ReasonCode = FTZUnconcurrenceReasonCodeList.Codes._01;
			action.Validation.ValidateUS_Reasons();
			AssertNoMessageErrorContaining(action.US_ReasonsInfo, MandatoryValidation.YouHaveNotEntered);

			action.US_ReasonCode = FTZUnconcurrenceReasonCodeList.Codes._02;
			action.Validation.ValidateUS_Reasons();
			AssertHasMessageErrorContaining(action.US_ReasonsInfo, MandatoryValidation.YouHaveNotEntered);

			action.US_Reasons = "123";
			AssertNoMessageErrorContaining(action.US_ReasonsInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasWarning(action.US_ReasonsInfo, ITNumberValidator.Constants.ITNumber.Invalid);

			action.US_Reasons = "257700052";
			AssertNoWarning(action.US_ReasonsInfo, ITNumberValidator.Constants.ITNumber.Invalid);

			action.US_Reasons = "V236542J144";
			AssertHasWarning(action.US_ReasonsInfo, ValidationConstants.AllocateInBondNumber.InvalidPaperless);

			action.US_Reasons = "V2365425144";
			AssertNoWarning(action.US_ReasonsInfo, ValidationConstants.AllocateInBondNumber.InvalidPaperless);

			action.US_Reasons = "v2365425144";
			AssertNoWarning(action.US_ReasonsInfo, ValidationConstants.AllocateInBondNumber.InvalidPaperless);

			action.US_Reasons = "VHH65425144";
			AssertNoWarning(action.US_ReasonsInfo, ValidationConstants.AllocateInBondNumber.InvalidPaperless);

			action.US_Reasons = "vHH65425144";
			AssertNoWarning(action.US_ReasonsInfo, ValidationConstants.AllocateInBondNumber.InvalidPaperless);

			action.US_Reasons = "VH765425144";
			AssertNoWarning(action.US_ReasonsInfo, ValidationConstants.AllocateInBondNumber.InvalidPaperless);

			action.US_Reasons = "V7603245271";
			AssertHasWarningContaining(action.US_ReasonsInfo, "Invalid check digit. The last digit should be");

			action.US_Reasons = "V7603245275";
			AssertNoWarningContaining(action.US_ReasonsInfo, "Invalid check digit. The last digit should be");

			action.US_Reasons = "v7603245275";
			AssertNoWarningContaining(action.US_ReasonsInfo, "Invalid check digit. The last digit should be");

			action.US_Reasons = "11111111111";
			AssertHasWarning(action.US_ReasonsInfo, ITNumberValidator.Constants.ITNumber.Invalid);

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			action = new FZEventAction(declaration, FZEventType.Unconcur);
			action.US_ReasonCode = FTZUnconcurrenceReasonCodeList.Codes._02;

			//if Air then AWB number format is also correct (11n)
			action.US_Reasons = "12345678";
			AssertHasWarning(action.US_ReasonsInfo, ITNumberValidator.Constants.ITNumber.InvalidForAir);

			action.US_Reasons = "1111G111111";
			AssertHasWarning(action.US_ReasonsInfo, ITNumberValidator.Constants.ITNumber.InvalidForAir);

			action.US_Reasons = "11111111111";
			AssertNoWarning(action.US_ReasonsInfo, ITNumberValidator.Constants.ITNumber.InvalidForAir);

			action.US_Reasons = "123456789";
			AssertHasWarning(action.US_ReasonsInfo, ValidationConstants.AllocateInBondNumber.InvalidCheckDigit("2"));

			action.US_Reasons = "123456782";
			AssertNoWarning(action.US_ReasonsInfo, ValidationConstants.AllocateInBondNumber.InvalidCheckDigit("2"));

			action.US_Reasons = "Vblah-blah";
			AssertHasWarning(action.US_ReasonsInfo, ValidationConstants.AllocateInBondNumber.InvalidPaperless);

			action.US_Reasons = "12365";
			AssertNoWarning(action.US_ReasonsInfo, ValidationConstants.AllocateInBondNumber.InvalidPaperless);

			action.US_Reasons = "vblah-blah";
			AssertHasWarning(action.US_ReasonsInfo, ValidationConstants.AllocateInBondNumber.InvalidPaperless);
		}

		public void TestCheckUS_ReasonsFormatForCode04Or05()
		{
			var declaration = Factory.New<JobDeclaration>();
			var action = new FZEventAction(declaration, FZEventType.Unconcur);
			action.US_ReasonCode = FTZUnconcurrenceReasonCodeList.Codes._04;
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, new EntryFiler { EntryFilerCode = "XJ5" });

			action.US_Reasons = "XJ5";
			AssertHasWarning(action.US_ReasonsInfo, EntryNumberValidator.EntryNumberFormat);
			AssertNoWarning(action.US_ReasonsInfo, EntryNumberValidator.InvalidCheckDigit + 1);

			action.US_Reasons = "XJ560011282";
			AssertNoWarning(action.US_ReasonsInfo, EntryNumberValidator.EntryNumberFormat);
			AssertHasWarning(action.US_ReasonsInfo, EntryNumberValidator.InvalidCheckDigit + 1);

			action.US_Reasons = "XJ560011281";
			AssertNoMessageError(action.US_ReasonsInfo, EntryNumberValidator.EntryNumberFormat);
			AssertNoMessageError(action.US_ReasonsInfo, EntryNumberValidator.InvalidCheckDigit + 1);

			USCustomsDataRegistry.Instance.EntryFiler.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, new EntryFiler { EntryFilerCode = "SV9" });
			action.US_Reasons = "XJ560011282";
			AssertHasWarning(action.US_ReasonsInfo, EntryNumberValidator.InvalidCheckDigitOtherFiler + 1);

			action.US_Reasons = "XJ560011281";
			AssertNoWarning(action.US_ReasonsInfo, EntryNumberValidator.InvalidCheckDigitOtherFiler + 1);
		}
	}
}
