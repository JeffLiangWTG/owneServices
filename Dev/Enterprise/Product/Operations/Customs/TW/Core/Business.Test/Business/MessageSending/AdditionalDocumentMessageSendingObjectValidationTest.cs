using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(AdditionalDocumentMessageSendingObjectValidation))]
	sealed class AdditionalDocumentMessageSendingObjectValidationTest : TWMessageSendingObjectValidationAbstractTest<AdditionalDocumentMessageSendingObject>
	{
		public void TestCheckContactOffice()
		{
			var sendingObject = GetAdditionalDocumentMessageSendingObject();
			var targetInfo = sendingObject.ContactOfficeInfo;
			sendingObject.MessageType = MessageTypeList.Codes.ADM;
			sendingObject.ContactOffice = "01";
			AssertNoMessageErrorContaining(targetInfo, ListValidation.InvalidCodeMessageError);
			sendingObject.ContactOffice = "XX";
			AssertHasMessageErrorContaining(targetInfo, ListValidation.InvalidCodeMessageError);
			sendingObject.MessageType = "XXX";
			sendingObject.Validation.ValidateContactOffice();
			AssertNoMessageErrorContaining(targetInfo, ListValidation.InvalidCodeMessageError);
		}

		public override void TestCheckAction()
		{
			var sendingObject = GetAdditionalDocumentMessageSendingObject();
			var targetInfo = sendingObject.ActionInfo;
			sendingObject.MessageType = MessageTypeList.Codes.ADM;
			sendingObject.Action = "2";
			AssertEquals(false, targetInfo.HasError("Enter a valid Action."));
		}

		AdditionalDocumentMessageSendingObject GetAdditionalDocumentMessageSendingObject()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWReceivingUnit, "TWReceivingUnit");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWReceivingUnit, "01", "TW Receiving Unit", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_EntryStatus = "A";
			entryHeader.CH_Status = "B";
			entryHeader.CH_CEI_Instruction = declaration.CusEntryInstruction.PK;
			return new AdditionalDocumentMessageSendingObject(entryHeader);
		}
	}
}
