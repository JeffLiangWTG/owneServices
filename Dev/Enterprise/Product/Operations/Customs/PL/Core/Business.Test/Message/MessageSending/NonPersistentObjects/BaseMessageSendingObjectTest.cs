using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(BaseMessageSendingObject))]
sealed class BaseMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
{
	public void TestHeader() => AssertType<CusEntryHeader>(GetNewJobDeclarationMessageSendingObject().Header);

	public void TestCaptions()
	{
		CombineAssertions(() =>
		{
			AssertEquals("MessageStatus", "Status", DataBoundResourceStrings.GetDataForProperty(typeof(BaseMessageSendingObject), nameof(BaseMessageSendingObject.MessageStatus)).Caption);
			AssertEquals("EntryStatus", "Entry Status", DataBoundResourceStrings.GetDataForProperty(typeof(BaseMessageSendingObject), nameof(BaseMessageSendingObject.EntryStatus)).Caption);
			AssertEquals("EntryNumber", "MRN", DataBoundResourceStrings.GetDataForProperty(typeof(BaseMessageSendingObject), nameof(BaseMessageSendingObject.EntryNumber)).Caption);
			AssertEquals("Action", "Action", DataBoundResourceStrings.GetDataForProperty(typeof(BaseMessageSendingObject), nameof(BaseMessageSendingObject.Action)).Caption);
			AssertEquals("ReferenceNumber", "Ref No.", DataBoundResourceStrings.GetDataForProperty(typeof(BaseMessageSendingObject), nameof(BaseMessageSendingObject.ReferenceNumber)).Caption);
			AssertEquals("DeclarationDate", "Declaration Date", DataBoundResourceStrings.GetDataForProperty(typeof(BaseMessageSendingObject), nameof(BaseMessageSendingObject.DeclarationDate)).Caption);
			AssertEquals("EntryDescription", "Entry Description", DataBoundResourceStrings.GetDataForProperty(typeof(BaseMessageSendingObject), nameof(BaseMessageSendingObject.EntryDescription)).Caption);
			AssertEquals("ResponseMessage", "Response to Message No.", DataBoundResourceStrings.GetDataForProperty(typeof(BaseMessageSendingObject), nameof(BaseMessageSendingObject.ResponseMessage)).Caption);
		});
	}

	public void TestMessageStatus()
	{
		var sendingObj = GetNewJobDeclarationMessageSendingObject();
		var header = sendingObj.Header;
		header.CH_Status = ZString.Empty;
		CombineAssertions(() =>
		{
			AssertEquals(ZString.Empty, sendingObj.MessageStatus);

			header.CH_Status = "a";
			AssertEquals("a", sendingObj.MessageStatus);
		});
	}

	public void TestEntryStatus()
	{
		var sendingObj = GetNewJobDeclarationMessageSendingObject();
		var header = sendingObj.Header;
		header.CH_EntryStatus = ZString.Empty;
		CombineAssertions(() =>
		{
			AssertEquals(ZString.Empty, sendingObj.EntryStatus);

			header.CH_EntryStatus = "b";
			AssertEquals("b", sendingObj.EntryStatus);
		});
	}

	public void TestAction()
	{
		var sendingObj = GetNewJobDeclarationMessageSendingObject();
		sendingObj.Action = ZString.Empty;
		CombineAssertions(() =>
		{
			AssertEquals(ZString.Empty, sendingObj.Action);

			sendingObj.Action = "c";
			AssertEquals("c", sendingObj.Action);
		});
	}

	public void TestLocalReferenceNumber()
	{
		var sendingObj = GetNewJobDeclarationMessageSendingObject();
		sendingObj.LocalReferenceNumber = ZString.Empty;
		CombineAssertions(() =>
		{
			AssertEquals(ZString.Empty, sendingObj.LocalReferenceNumber);

			sendingObj.LocalReferenceNumber = "d";
			AssertEquals("d", sendingObj.LocalReferenceNumber);
		});
	}

	public void TestActionList()
	{
		var sendingObj = GetNewJobDeclarationMessageSendingObject();
		CombineAssertions(() =>
		{
			var list = sendingObj.ActionList;
			AssertEquals("List should be empty by default", string.Empty, list.CodesAsString);
		});
	}

	public void TestEntryDescription()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		var sendingObj = new BaseMessageSendingObject(entryHeader);

		CombineAssertions(() =>
		{
			AssertEquals(" : CPC  - ", sendingObj.EntryDescription);

			entryInstruction.CEI_SubStyle = "A";
			AssertEquals("A : CPC  - ", sendingObj.EntryDescription);

			entryInstruction.CEI_Procedure = "13";
			AssertEquals("A : CPC 13 - ", sendingObj.EntryDescription);

			entryInstruction.CEI_Description = "someDescription";
			AssertEquals("A : CPC 13 - someDescription", sendingObj.EntryDescription);

			entryInstruction.CEI_Procedure = ZString.Empty;
			AssertEquals("A : CPC  - someDescription", sendingObj.EntryDescription);
		});
	}

	public void TestEntryNumber()
	{
		var sendingObj = GetNewJobDeclarationMessageSendingObject();

		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.MovementReferenceNumberSetter("12345", ZDateTime.BrettsBirthday);
		var sendingObj2 = new BaseMessageSendingObject(entryHeader);

		CombineAssertions(() =>
		{
			AssertEquals("Empty Entry Number", ZString.Empty, sendingObj.EntryNumber);
			AssertEquals("Entry Number is 12345", "12345", sendingObj2.EntryNumber);

			sendingObj.EntryNumber = "asd";
			AssertEquals("Entry Number is not empty", "asd", sendingObj.EntryNumber);
		});
	}

	public void TestEntryDescription_ReadOnly()
	{
		var sendingObj = GetNewJobDeclarationMessageSendingObject();
		var propertyInfo = sendingObj.EntryDescriptionInfo;
		AssertEquals(true, propertyInfo.ReadOnly);
	}

	public void TestEntryNumber_ReadOnly() => CombineAssertions(() =>
	{
		var sendingObj = GetNewJobDeclarationMessageSendingObject();
		var propertyInfo = sendingObj.EntryNumberInfo;
		AssertEquals("Header entry number is empty", false, propertyInfo.ReadOnly);

		sendingObj.Header.MovementReferenceNumberSetter("12345");
		AssertEquals("Header entry number is not empty", true, propertyInfo.ReadOnly);
	});

	public void TestDeclarationDate_ReadOnly()
	{
		var sendingObj = new BaseMessageSendingObject(Factory.New<CusEntryHeader>());
		Assert(sendingObj.DeclarationDateInfo.ReadOnly);
	}

	public void TestResponseMessageAutoFilledWithSingleMessage() => CombineAssertions(() =>
	{
		foreach (var (incomingMessageType, actionType) in new []
			{
				("560", "CC566"),
				("582", "CC583"),
			})
		{
			var entryHeader = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
			var sendingObj = new BaseMessageSendingObject(entryHeader);

			var unexpectedMessageType = "515";
			var unexpectedActionType = "CC515";
			_ = CreateTestMessage(unexpectedMessageType, "UnexpectedTestMessage");
			sendingObj.Action = actionType;
			AssertEquals($"None {incomingMessageType} message is received.", ZString.Empty, sendingObj.ResponseMessage);

			var message1 = CreateTestMessage(incomingMessageType, "IncomingTestMessage1");
			sendingObj.Action = unexpectedActionType;
			AssertEquals($"Single {incomingMessageType} message is received but Action is not {actionType}.", ZString.Empty, sendingObj.ResponseMessage);

			sendingObj.Action = actionType;
			AssertEquals($"Single {incomingMessageType} message is received and Action is {actionType}.", message1.EM_MessageNum, sendingObj.ResponseMessage);

			var message2 = CreateTestMessage(incomingMessageType, "IncomingTestMessage2");
			sendingObj.Action = string.Empty;
			sendingObj.Action = actionType;
			AssertEquals($"Multiple {incomingMessageType} messages are received.", ZString.Empty, sendingObj.ResponseMessage);

			sendingObj.Action = string.Empty;
			entryHeader.Messages.Remove(message2);
			sendingObj.Action = actionType;
			AssertEquals($"Single {incomingMessageType} message is received.", message1.EM_MessageNum, sendingObj.ResponseMessage);

			EDIMessage CreateTestMessage(ZString messageSubType, ZString messageNumber)
			{
				var result = Factory.New<EDIMessage>();
				result.EM_MessageSubType = messageSubType;
				result.EM_MessageNum = messageNumber;
				entryHeader.Messages.Add(result);
				return result;
			}
		}
	});

	public void TestSetMessageSendingObjectDefaultValues()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var declarationDate = new ZDateTime(2022, 1, 1);
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_DateForDuty = declarationDate;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryHeader.MovementReferenceNumberSetter("12345", ZDateTime.BrettsBirthday);
		entryHeader.CH_EntryStatus = ZString.Empty;
		entryHeader.CH_BGMReference = "A123";

		var sendingObj = new BaseMessageSendingObject(entryHeader);
		CombineAssertions(() =>
		{
			AssertEquals("Reference Number", "A123", sendingObj.LocalReferenceNumber);
			AssertEquals("Declaration date", declarationDate, sendingObj.DeclarationDate);
			AssertEquals("Entry number", "12345", sendingObj.EntryNumber);
			AssertEquals("Should send true - CE_EntryStatus empty", true, sendingObj.ShouldSend);
			entryHeader.CH_EntryStatus = PLEntryStatusList.Codes.NPP;
			sendingObj = new BaseMessageSendingObject(entryHeader);
			AssertEquals("Should send true - CE_EntryStatus NPP", true, sendingObj.ShouldSend);
			entryHeader.CH_EntryStatus = PLEntryStatusList.Codes.UPO;
			sendingObj = new BaseMessageSendingObject(entryHeader);
			AssertEquals("Should send false - CE_EntryStatus other", false, sendingObj.ShouldSend);
		});
	}

	public void TestEntryNumberMaxLen() => AssertEquals(35, GetNewJobDeclarationMessageSendingObject().EntryNumberInfo.MaxLength);

	public void TestExpectedEntryNumberLength() => AssertEquals(35, GetNewJobDeclarationMessageSendingObject().ExpectedEntryNumberLength);

	public void TestAmendmentInvalidationReasonMaxLength() => AssertEquals(512, GetNewJobDeclarationMessageSendingObject().AmendmentInvalidationReasonInfo.MaxLength);

	public void TestSecurity()
	{
		var sendingObj = GetNewJobDeclarationMessageSendingObject();
		sendingObj.Security = "0";
		CombineAssertions(() =>
		{
			AssertEquals("0", sendingObj.Security);

			sendingObj.Security = "2";
			AssertEquals("2", sendingObj.Security);
		});
	}

	public void TestSecurity_Caption() => AssertEquals("Security", DataBoundResourceStrings.GetDataForProperty(typeof(BaseMessageSendingObject), nameof(BaseMessageSendingObject.Security)).Caption);

	public void TestLookups() => AssertType<BaseMessageSendingObjectLookups>(GetNewJobDeclarationMessageSendingObject().Lookups);

	protected override BusinessObject GetNewBusinessObject()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		return new BaseMessageSendingObject(entryHeader);
	}

	BaseMessageSendingObject GetNewJobDeclarationMessageSendingObject() => (BaseMessageSendingObject)GetNewBusinessObject();
}
