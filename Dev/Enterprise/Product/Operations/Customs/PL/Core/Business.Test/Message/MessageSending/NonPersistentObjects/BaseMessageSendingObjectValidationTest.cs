using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business.Declaration;
using static Enterprise.Core.Constants;
using ExportSecurityTypeList = Enterprise.Customs.EU.Business.ExportSecurityTypeList;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class BaseMessageSendingObjectValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckDeclarationDate() => CombineAssertions(() =>
	{
		sendingObject.ShouldSend = false;
		sendingObject.Action = Constants.MessageSendingObjectActionCodes.ZC415;
		sendingObject.DeclarationDate = ZDate.Today.AddDays(-1);
		var message = "Invalid declaration date – must be TODAY";
		var propertyInfo = sendingObject.DeclarationDateInfo;
		AssertNoError("ShouldSend false, action ZC415, date yesterday", propertyInfo, message);

		sendingObject.ShouldSend = true;
		AssertHasError("ShouldSend true, action ZC415, date yesterday", propertyInfo, message);

		sendingObject.Action = Constants.MessageSendingObjectActionCodes.ZCX05;
		AssertNoError("ShouldSend true, action ZCX05, date yesterday", propertyInfo, message);

		sendingObject.DeclarationDate = ZDate.Today;
		sendingObject.Action = Constants.MessageSendingObjectActionCodes.ZC415;
		AssertNoError("ShouldSend true, action ZC415, date today", propertyInfo, message);
	});

	public void TestCheckAction() => CombineAssertions(() =>
	{
		var propertyInfo = sendingObject.ActionInfo;
		sendingObject.Validation.ValidateAction();
		AssertHasErrorContaining("Empty", propertyInfo, MandatoryValidation.MustBeEntered);

		sendingObject.ShouldSend = false;
		sendingObject.Validation.ValidateAction();
		AssertNoErrorContaining("Empty but ShouldSend is false", propertyInfo, MandatoryValidation.MustBeEntered);

		sendingObject.ShouldSend = true;
		sendingObject.Action = Constants.MessageSendingObjectActionCodes.ZC415;
		AssertNoErrorContaining("Not empty", propertyInfo, MandatoryValidation.MustBeEntered);
	});

	public void TestCheckSecurity() => CombineAssertions(() =>
	{
		declaration.JE_EntryStyle = EntryStyleListExportUCC.Codes.ExportNormal;

		sendingObject.Action = ExportMessageSendingObjectActionList.Codes.CC514;
		sendingObject.Validation.ValidateSecurity();
		AssertNoNotifications("Validation should be run for CC515 or CC513", sendingObject.SecurityInfo);

		sendingObject.Action = ExportMessageSendingObjectActionList.Codes.CC513;
		sendingObject.Validation.ValidateSecurity();
		AssertHasErrorContaining("Empty value", sendingObject.SecurityInfo, MandatoryValidation.MustBeEntered);

		sendingObject.Security = "A";
		sendingObject.Validation.ValidateSecurity();
		AssertHasMessageErrorContaining("Invalid input", sendingObject.SecurityInfo, ListValidation.InvalidCodeMessageError.ToString());

		declaration.JE_EntryStyle = EntryStyleListExportUCC.Codes.ExportToSpecialTerritory;
		sendingObject.Validation.ValidateSecurity();
		AssertNoErrorContaining("Error - Disabled for Export To Special Territory", sendingObject.SecurityInfo, MandatoryValidation.MustBeEntered);
		AssertNoMessageErrorContaining("MessageError - Disabled for Export To Special Territory", sendingObject.SecurityInfo, ListValidation.InvalidCodeMessageError.ToString());

		declaration.JE_EntryStyle = EntryStyleListExportUCC.Codes.ExportNormal;
		sendingObject.Security = ExportSecurityTypeList.Codes.EXS;
		sendingObject.Validation.ValidateSecurity();
		AssertNoErrorContaining("Error - Valid input", sendingObject.SecurityInfo, MandatoryValidation.MustBeEntered);
		AssertNoMessageErrorContaining("MessageError - Valid input", sendingObject.SecurityInfo, ListValidation.InvalidCodeMessageError.ToString());
	});

	public void TestCheckAmendmentInvalidationReason() => CombineAssertions(() =>
	{
		sendingObject.Action = ExportMessageSendingObjectActionList.Codes.CC513;
		sendingObject.Validation.ValidateAmendmentInvalidationReason();
		AssertNoNotifications("Validation should be run for CC514", sendingObject.AmendmentInvalidationReasonInfo);

		sendingObject.Action = ExportMessageSendingObjectActionList.Codes.CC514;
		sendingObject.Validation.ValidateAmendmentInvalidationReason();
		AssertHasErrorContaining("Empty declaration", sendingObject.AmendmentInvalidationReasonInfo, MandatoryValidation.MustBeEntered);

		sendingObject.AmendmentInvalidationReason = "something";
		AssertNoNotifications("Valid input", sendingObject.AmendmentInvalidationReasonInfo);
	});

	public void TestCheckCorrectionAcceptance() => CombineAssertions(() =>
	{
		sendingObject.Action = ExportMessageSendingObjectActionList.Codes.CC515;
		sendingObject.Validation.ValidateCorrectionAcceptance();
		AssertNoNotifications("Validation should be run for CC566 only", sendingObject.CorrectionAcceptanceInfo);

		sendingObject.Action = ExportMessageSendingObjectActionList.Codes.CC566;
		sendingObject.Validation.ValidateCorrectionAcceptance();
		AssertHasErrorContaining("Empty value", sendingObject.CorrectionAcceptanceInfo, MandatoryValidation.MustBeEntered);

		sendingObject.CorrectionAcceptance = "2";
		sendingObject.Validation.ValidateCorrectionAcceptance();
		AssertHasMessageErrorContaining("Invalid input", sendingObject.CorrectionAcceptanceInfo, ListValidation.InvalidCodeMessageError.ToString());

		sendingObject.CorrectionAcceptance = MessageSendingObjectCorrectionAcceptanceList.Codes._1;
		sendingObject.Validation.ValidateCorrectionAcceptance();
		AssertNoNotifications("Valid input", sendingObject.CorrectionAcceptanceInfo);
	});

	public void TestCheckAcceptanceComment() => CombineAssertions(() =>
	{
		sendingObject.Action = ExportMessageSendingObjectActionList.Codes.CC515;
		sendingObject.Validation.ValidateAcceptanceComment();
		AssertNoNotifications("Validation should be run for CC566 only", sendingObject.AcceptanceCommentInfo);

		sendingObject.Action = ExportMessageSendingObjectActionList.Codes.CC566;
		sendingObject.Validation.ValidateAcceptanceComment();
		AssertHasErrorContaining("Empty value", sendingObject.AcceptanceCommentInfo, MandatoryValidation.MustBeEntered);

		sendingObject.AcceptanceComment = "Abc";
		sendingObject.Validation.ValidateAcceptanceComment();
		AssertNoNotifications("Valid input", sendingObject.AcceptanceCommentInfo);
	});

	public void TestAcceptanceCommentMaxLength() => AssertEquals(512, sendingObject.AcceptanceCommentInfo.MaxLength);

	public void TestCheckResponseMessage() => CombineAssertions(() =>
	{
		const string expectedWarning = "Linked Control Notification doesn't contain discrepancies and PDW request.";

		sendingObject.Action = ExportMessageSendingObjectActionList.Codes.CC515;
		sendingObject.Validation.ValidateResponseMessage();
		AssertNoNotifications("Validation should be run for CC566 only", sendingObject.ResponseMessageInfo);

		sendingObject.Action = ExportMessageSendingObjectActionList.Codes.CC566;
		sendingObject.Validation.ValidateResponseMessage();
		AssertHasWarning("Empty value", sendingObject.ResponseMessageInfo, expectedWarning);

		sendingObject.ResponseMessage = "Abc";
		sendingObject.Validation.ValidateResponseMessage();
		AssertNoNotifications("Valid input", sendingObject.ResponseMessageInfo);
	});

	public void TestCheckRuleR211() => CombineAssertions(() =>
	{
		var messageError = "[C0211] IF Security is '2' Itinerary Countries are required.";
		sendingObject.Security = ExportSecurityTypeList.Codes.EXS;
		sendingObject.Action = ExportMessageSendingObjectActionList.Codes.CC514;
		sendingObject.Validation.ValidateSecurity();
		AssertNoMessageErrorContaining("Validation should be run for CC515 or CC513", sendingObject.SecurityInfo, messageError);

		sendingObject.Action = ExportMessageSendingObjectActionList.Codes.CC513;
		sendingObject.Security = ExportSecurityTypeList.Codes.EXS;
		sendingObject.Validation.ValidateSecurity();
		AssertHasMessageErrorContaining("513 - Empty ItineraryCountries for Security = '2'", sendingObject.SecurityInfo, messageError);

		sendingObject.Action = ExportMessageSendingObjectActionList.Codes.CC515;
		sendingObject.Security = ExportSecurityTypeList.Codes.EXS;
		sendingObject.Validation.ValidateSecurity();
		AssertHasMessageErrorContaining("515 - Empty ItineraryCountries for Security = '2'", sendingObject.SecurityInfo, messageError);

		sendingObject.Security = ExportSecurityTypeList.Codes.NotUsed;
		sendingObject.Validation.ValidateSecurity();
		AssertNoMessageErrorContaining("Security is not '2'", sendingObject.SecurityInfo, messageError);

		sendingObject.Security = ExportSecurityTypeList.Codes.EXS;
		var itineraryCountry = declaration.ItineraryCountries.AddNew();
		itineraryCountry.CY_Code = CountryCodes.Poland;
		sendingObject.Validation.ValidateSecurity();
		AssertNoMessageErrorContaining("itineraryCountry is not empty", sendingObject.SecurityInfo, messageError);
	});

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		sendingObject = new BaseMessageSendingObject(entryHeader);
	}

	JobDeclaration declaration;
	BaseMessageSendingObject sendingObject;
}
