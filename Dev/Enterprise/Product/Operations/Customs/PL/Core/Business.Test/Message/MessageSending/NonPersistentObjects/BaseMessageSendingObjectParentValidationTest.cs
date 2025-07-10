using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class BaseMessageSendingObjectParentValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCustomsOffice()
	{
		CombineAssertions(() =>
		{
			var messageSendingObjectParent = GetJobDeclarationMessageSendingObjectParent();
			var messageError = "You have not entered a Customs Office.";
			messageSendingObjectParent.Validation.ValidateCustomsOffice();
			AssertNoMessageError("no eDoc added, value empty", messageSendingObjectParent.CustomsOfficeInfo, messageError);
			messageSendingObjectParent.EDocs.AddNew();
			messageSendingObjectParent.Validation.ValidateCustomsOffice();
			AssertHasMessageError("eDoc added, value empty", messageSendingObjectParent.CustomsOfficeInfo, messageError);
			messageSendingObjectParent.CustomsOffice = "123";
			AssertNoMessageError("eDoc added, value not empty", messageSendingObjectParent.CustomsOfficeInfo, messageError);
		});
	}

	public void TestCheckPurposeOfSending()
	{
		CombineAssertions(() =>
		{
			var messageSendingObjectParent = GetJobDeclarationMessageSendingObjectParent();
			var messageError = "You have not entered a Purpose of Sending.";
			messageSendingObjectParent.Validation.ValidatePurposeOfSending();
			AssertNoMessageError("no eDoc added, value empty", messageSendingObjectParent.PurposeOfSendingInfo, messageError);
			messageSendingObjectParent.EDocs.AddNew();
			messageSendingObjectParent.Validation.ValidatePurposeOfSending();
			AssertHasMessageError("eDoc added, value empty", messageSendingObjectParent.PurposeOfSendingInfo, messageError);
			messageSendingObjectParent.PurposeOfSending = "123";
			AssertNoMessageError("eDoc added, value not empty", messageSendingObjectParent.PurposeOfSendingInfo, messageError);
		});
	}

	public void TestCheckRefNumber()
	{
		CombineAssertions(() =>
		{
			var messageSendingObjectParent = GetJobDeclarationMessageSendingObjectParent();
			var messageError = "You have not entered a Ref number.";
			messageSendingObjectParent.Validation.ValidateRefNumber();
			AssertNoMessageError("no eDoc added, value empty", messageSendingObjectParent.RefNumberInfo, messageError);
			messageSendingObjectParent.EDocs.AddNew();
			messageSendingObjectParent.Validation.ValidateRefNumber();
			AssertHasMessageError("eDoc added, value empty", messageSendingObjectParent.RefNumberInfo, messageError);
			messageSendingObjectParent.RefNumber = "123";
			AssertNoMessageError("eDoc added, value not empty", messageSendingObjectParent.RefNumberInfo, messageError);
		});
	}

	public void TestCheckMrnNumber()
	{
		CombineAssertions(() =>
		{
			var messageSendingObjectParent = GetJobDeclarationMessageSendingObjectParent();
			var messageError = "You have not entered a MRN number.";
			messageSendingObjectParent.PurposeOfSending = MessageSendingPurposeOfSendingList.Codes._1.ToUpper();
			messageSendingObjectParent.Validation.ValidateMrnNumber();
			AssertNoMessageError("no eDoc added, value empty", messageSendingObjectParent.MrnNumberInfo, messageError);
			messageSendingObjectParent.EDocs.AddNew();
			messageSendingObjectParent.Validation.ValidateMrnNumber();
			AssertHasMessageError("eDoc added, value empty", messageSendingObjectParent.MrnNumberInfo, messageError);
			messageSendingObjectParent.PurposeOfSending = MessageSendingPurposeOfSendingList.Codes._0.ToUpper();
			messageSendingObjectParent.Validation.ValidateMrnNumber();
			AssertNoMessageError("eDoc added, value empty, other purpose of sending", messageSendingObjectParent.MrnNumberInfo, messageError);
			messageSendingObjectParent.PurposeOfSending = MessageSendingPurposeOfSendingList.Codes._1.ToUpper();
			messageSendingObjectParent.MrnNumber = "123";
			AssertNoMessageError("eDoc added, value not empty", messageSendingObjectParent.MrnNumberInfo, messageError);
		});
	}

	public void TestCheckProcedure()
	{
		CombineAssertions(() =>
		{
			var messageSendingObjectParent = GetJobDeclarationMessageSendingObjectParent();
			var messageError = "You have not entered a Procedure.";
			messageSendingObjectParent.Validation.ValidateProcedure();
			AssertNoMessageError("no eDoc added, value empty", messageSendingObjectParent.ProcedureInfo, messageError);
			messageSendingObjectParent.EDocs.AddNew();
			messageSendingObjectParent.Validation.ValidateProcedure();
			AssertHasMessageError("eDoc added, value empty", messageSendingObjectParent.ProcedureInfo, messageError);
			messageSendingObjectParent.Procedure = "123";
			AssertNoMessageError("eDoc added, value not empty", messageSendingObjectParent.ProcedureInfo, messageError);
		});
	}

	BusinessObject GetNewBusinessObject() => new BaseMessageSendingObjectParent(declaration);
	BaseMessageSendingObjectParent GetJobDeclarationMessageSendingObjectParent() => (BaseMessageSendingObjectParent)GetNewBusinessObject();

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		declaration.CustomsEntryHeaders.AddNew();
	}

	JobDeclaration declaration;
}
