using System;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestsSubclassesOf(typeof(MessageSendingObjectValidation))]
	abstract class TWMessageSendingObjectValidationAbstractTest<TMessageSendingObject> : MessageSendingObjectValidationTest
		where TMessageSendingObject : MessageSendingObject
	{
		[TestDate(2019, 9, 9)]
		[ExpectNoExceptions]
		public virtual void TestCheckAction()
		{
			var invalidMessageError = "Enter a valid Action.";
			var actionRequiredError = "Action is required if Send is ticked.";
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var cusHead = Factory.NewWithValidTestData<CusEntryHeader>();
			cusHead.CH_JE = declaration.PK;
			cusHead.CH_CEI_Instruction = entryInstruction1.PK;
			var action = (TMessageSendingObject)Activator.CreateInstance(typeof(TMessageSendingObject), cusHead);
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
			action.Action = "0";
			AssertHasErrorContaining(targetInfo, invalidMessageError);
			AssertNoErrorContaining(targetInfo, actionRequiredError);
			action.ShouldSend = false;
			action.Validation.ValidateAll();
			AssertNoErrorContaining(targetInfo, actionRequiredError);
			AssertNoErrorContaining(targetInfo, invalidMessageError);
			action.Action = "9";
			AssertNoErrorContaining(targetInfo, actionRequiredError);
			AssertNoErrorContaining(targetInfo, invalidMessageError);

			entryInstruction1.CEI_DateForDuty = TestDateAttribute.Date;
			action.ShouldSend = true;
			action.Action = ActionCodeList.Codes.Create;
			action.Validation.ValidateAction();
			AssertNoWarning(targetInfo, ValidationConstants.MessageSendingObject.DeclarationDateShouldBeToday);
			entryInstruction1.CEI_DateForDuty = new ZDateTime(2019, 05, 15);
			action.Validation.ValidateAction();
			AssertHasWarning(targetInfo, ValidationConstants.MessageSendingObject.DeclarationDateShouldBeToday);
			cusHead.CH_EntryStatus = EntryStatusCodeList.Codes.ARM;
			var disposition = cusHead.CusDispositions.AddNew();
			disposition.CDI_Type = Common.CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			disposition.CDI_StatusKey = EntryStatusCodeList.Codes.ARM;
			disposition.CDI_Status = "A01";
			NUnit.Framework.Assert.That(!cusHead.HasBeenLodgedAtCustoms, NUnit.Framework.Is.True);
			action.Action = ZString.Empty;
			AssertHasErrorContaining(targetInfo, actionRequiredError);
			action.Action = ActionCodeList.Codes.Update;
			AssertHasMessageError(targetInfo, ValidationConstants.MessageSendingObject.ActionCodeIsInvalidWhenNotReceived);
			action.Action = ActionCodeList.Codes.Create;
			AssertNoMessageError(targetInfo, ValidationConstants.MessageSendingObject.ActionCodeIsInvalidWhenNotReceived);
			cusHead.CH_EntryStatus = EntryStatusCodeList.Codes.RFM;
			NUnit.Framework.Assert.That(cusHead.HasBeenLodgedAtCustoms, NUnit.Framework.Is.True);
			action.Action = ActionCodeList.Codes.Update;
			AssertNoMessageError(targetInfo, ValidationConstants.MessageSendingObject.ActionCodeIsInvalidWhenReceived);
			action.Action = ActionCodeList.Codes.Create;
			AssertHasMessageError(targetInfo, ValidationConstants.MessageSendingObject.ActionCodeIsInvalidWhenReceived);

			cusHead.CH_EntryStatus = EntryStatusCodeList.Codes.ARM;
			disposition.CDI_Status = "F88";

			action.Action = ZString.Empty;
			action.Validation.ValidateAction();
			AssertNoNotifications(targetInfo);
		}

		internal CusEntryHeader NewCusEntryHeader(ZString entryNum)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			var cusHead = Factory.NewWithValidTestData<CusEntryHeader>();
			cusHead.CH_JE = declaration.PK;
			cusHead.CH_CEI_Instruction = declaration.CusEntryInstruction.PK;
			if (!entryNum.IsEmpty)
			{
				var entryInstruction1 = declaration.CusEntryInstruction;
				entryInstruction1.CEI_Style = "B1";
				entryInstruction1.CEI_CustomsOffice = "BA";
				entryInstruction1.CEI_OA_Warehouse2 = orgHeader.MainAddress.PK;
				cusHead.CH_CEI_Instruction = entryInstruction1.PK;
			}

			var cusNum = Factory.NewWithValidTestData<CusEntryNumber>();
			cusNum.CE_ParentID = cusHead.PK;
			cusNum.CE_Category = "CUS";
			cusNum.CE_EntryType = SharedJobMessageTypeList.Codes.Import;
			cusNum.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			cusNum.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum.CE_EntryNum = entryNum;
			Factory.Save();
			return cusHead;
		}

		OrgHeader orgHeader;
		protected override void SetUp()
		{
			base.SetUp();
			orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var org2Code = orgHeader.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.CBF, "00612348", Core.Constants.CountryCodes.Taiwan);
			var warehouseAddress = orgHeader.Addresses.AddNew();
			warehouseAddress.Address1 = "Address1";
			warehouseAddress.Address2 = "Address2";
			org2Code.OK_OA_PremisesAddress = orgHeader.MainAddress.PK;
		}
	}
}
