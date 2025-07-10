using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business.Testing
{
	internal sealed class TranshipmentMessageSendingObjectValidationTest : TestCaseWithFactory
	{
		public void TestCheckAction()
		{
			var invalidMessageError = "Enter a valid Action.";
			var actionRequiredError = "Action is required if Send is ticked.";
			var cusHead = Factory.NewWithValidTestData<CusInBondHeader>();
			var action = new TranshipmentMessageSendingObject(cusHead, "");
			action.ShouldSend = true;
			action.Action = ZString.Empty;
			var targetInfo = action.ActionInfo;
			AssertHasErrorContaining(targetInfo, actionRequiredError);
			AssertNoErrorContaining(targetInfo, invalidMessageError);
			action.ShouldSend = false;
			action.Validation.ValidateAll();
			AssertNoErrorContaining(targetInfo, actionRequiredError);
			AssertNoErrorContaining(targetInfo, invalidMessageError);
			action.ShouldSend = true;
			action.Action = "X";
			AssertHasErrorContaining(targetInfo, invalidMessageError);
			AssertNoErrorContaining(targetInfo, actionRequiredError);
			action.ShouldSend = false;
			action.Validation.ValidateAll();
			AssertNoErrorContaining(targetInfo, actionRequiredError);
			AssertNoErrorContaining(targetInfo, invalidMessageError);
			action.Action = "9";
			AssertNoErrorContaining(targetInfo, actionRequiredError);
			AssertNoErrorContaining(targetInfo, invalidMessageError);
		}

		public void TestCheckShouldSend()
		{
			var cusHead = NewCusEntryHeader("", "", ZString.Empty);
			var action = new TranshipmentMessageSendingObject(cusHead, "");
			action.ShouldSend = true;
			var targetInfo = action.ShouldSendInfo;
			AssertHasErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEnteredMessage(cusHead.ReceiptOfficeInfo.HumanReadableName));
			AssertHasErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEnteredMessage(cusHead.UnladingOfficeInfo.HumanReadableName));
			AssertHasErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEnteredMessage("Customs Broker Box Number"));
			cusHead = NewCusEntryHeader("AA", "BB", "123");
			action = new TranshipmentMessageSendingObject(cusHead, "");
			action.ShouldSend = true;
			targetInfo = action.ShouldSendInfo;
			AssertNoErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEnteredMessage(cusHead.ReceiptOfficeInfo.HumanReadableName));
			AssertNoErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEnteredMessage(cusHead.UnladingOfficeInfo.HumanReadableName));
			AssertNoErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEnteredMessage("Customs Broker Box Number"));
		}

		CusInBondHeader NewCusEntryHeader(ZString receiptOffice, ZString unladingOffice, ZString boxNumber)
		{
			var cusHead = Factory.NewWithValidTestData<CusInBondHeader>();
			cusHead.ReceiptOffice = receiptOffice;
			cusHead.UnladingOffice = unladingOffice;
			cusHead.TW_BoxNumber = boxNumber;
			var cusNum = Factory.NewWithValidTestData<CusEntryNumber>();
			cusNum.CE_ParentID = cusHead.PK;
			cusNum.CE_Category = "CUS";
			cusNum.CE_EntryType = "TRS";
			cusNum.CE_ParentTable = CusInBondHeaderSchema.Constants.TableName;
			cusNum.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			Factory.Save();
			return cusHead;
		}
	}
}
