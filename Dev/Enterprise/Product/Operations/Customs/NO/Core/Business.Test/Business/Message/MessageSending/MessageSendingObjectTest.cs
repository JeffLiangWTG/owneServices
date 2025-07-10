using System.Collections.Immutable;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(MessageSendingObject))]
sealed class MessageSendingObjectTest : NonPersistentBusinessObjectTestCase
{
	public void TestEntryHeader()
	{
		AssertSame(nameof(action.Header), entry, action.Header);
	}

	public void TestEntryInstruction()
	{
		AssertSame(nameof(action.Header.EntryInstruction), entryInstruction, action.Header.EntryInstruction);
	}

	public void TestProperties()
	{
		var declarationType = "1";
		var description = "Test Description";
		var procedure = "1234";
		var messageType = "FU";
		var entryStatus = Common.Shared.EntryStatusList.Codes.Clear;
		var entryNumber = "12345";

		entryInstruction.CEI_Style = declarationType;
		entryInstruction.CEI_Description = description;
		entryInstruction.CEI_Procedure = procedure;
		action.MessageType = messageType;
		entry.CH_EntryStatus = entryStatus;
		CombineAssertions(() =>
		{
			AssertEquals(nameof(action.DeclarationType), declarationType, action.DeclarationType);
			AssertEquals(nameof(action.Description), description, action.Description);
			AssertEquals(nameof(action.Procedure), procedure, action.Procedure);
			AssertEquals(nameof(action.MessageType), messageType, action.MessageType);
			AssertEquals(nameof(action.EntryStatus), entryStatus, action.EntryStatus);
			AssertEquals(nameof(action.EntryNumber), ZString.Empty, action.EntryNumber);

			entry.EntryNumber = entryNumber;
			AssertEquals(nameof(action.EntryNumber), entryNumber, action.EntryNumber);
		});
	}

	public void TestCustomsOffice_Import_DefaultValue()
	{
		var customsOffice = "MainOffice";
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		var declarant = Factory.New<OrgHeader>();
		declarant.OH_Code = "Dec1";
		declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
		SetupCustomsOfficesForTest(declarant, "1111", CusAuthorizationHeaderTypeList.Codes.ImportCustomsDeclaration, customsOffice);

		Factory.Save();

		action = GetMessageSendingObject();
		AssertEquals(nameof(action.CustomsOffice), customsOffice, action.CustomsOffice);
	}

	public void TestCustomsOffice_Export_DefaultValue()
	{
		var customsOffice = "MainOffice";
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

		var declarant = Factory.New<OrgHeader>();
		declarant.OH_Code = "Dec1";
		declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
		SetupCustomsOfficesForTest(declarant, "1111", CusAuthorizationHeaderTypeList.Codes.ExportCustomsDeclaration, customsOffice);

		Factory.Save();

		action = GetMessageSendingObject();
		AssertEquals(nameof(action.CustomsOffice), customsOffice, action.CustomsOffice);
	}

	public void TestCustomsOffice_ValueFromHeader()
	{
		var customsOffice = "NonDefault";
		entry.CH_ToCustomsControllingUnit = customsOffice;
		Factory.Save();

		action = GetMessageSendingObject();

		AssertEquals(nameof(action.CustomsOffice), customsOffice, action.CustomsOffice);
	}

	public void TestCaptions()
	{
		CombineAssertions(() =>
		{
			AssertEquals(nameof(action.DeclarationType), "Declaration Type", action.DeclarationTypeInfo.Description);
			AssertEquals(nameof(action.Description), "Description", action.DescriptionInfo.Description);
			AssertEquals(nameof(action.Procedure), "Procedure", action.ProcedureInfo.Description);
			AssertEquals(nameof(action.MessageType), "Message Type", action.MessageTypeInfo.Description);
			AssertEquals(nameof(action.EntryStatus), "Entry Status", action.EntryStatusInfo.Description);
			AssertEquals(nameof(action.EntryNumber), "Entry Number", action.EntryNumberInfo.Description);
		});
	}

	public void TestValidationType()
	{
		AssertType<MessageSendingObjectValidation>(action.Validation);
	}

	public void TestLookupsType()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			action = GetMessageSendingObject();
			AssertType<ImportMessageSendingObjectLookups>(action.Lookups);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			action = GetMessageSendingObject();
			AssertType<ExportMessageSendingObjectLookups>(action.Lookups);
		});
	}

	public void TestMessageType_DefaultValue_FU()
	{
		var validProcedurePrefixes = new[] { "10", "11", "40", "41" };
		var invalidSubStyles = new[] { "P", "S", "REC" };

		entryInstruction.CEI_Procedure = "4000";
		action = GetMessageSendingObject();

		AssertEquals("Prerequisite: CH_BGMReference is empty", ZString.Empty, entry.CH_BGMReference);
		AssertEquals("Prerequisite: CEI_Procedure starts with 40/41/10/11", true, validProcedurePrefixes.Any(prefix => entryInstruction.CEI_Procedure.StartsWith(prefix)));
		AssertEquals("Prerequisite: CEI_SubStyle is not P, S nor REC", false, invalidSubStyles.Any(subStyle => entryInstruction.CEI_SubStyle == subStyle));
		AssertEquals("DefaultValue_FU", MessageSendingMessageTypes.Codes.CompleteOrdinaryDeclaration, action.MessageType);
	}

	public void TestMessageType_DefaultValue_MA()
	{
		var invalidProcedurePrefixes = new[] { "10", "11", "40", "41" };
		var invalidSubStyles = new[] { "P", "S", "REC" };

		entryInstruction.CEI_Procedure = "5000";
		action = GetMessageSendingObject();

		AssertEquals("Prerequisite: CH_BGMReference is empty", ZString.Empty, entry.CH_BGMReference);
		AssertEquals("Prerequisite: CEI_Procedure does not start with 40/41/10/11", false, invalidProcedurePrefixes.Any(prefix => entryInstruction.CEI_Procedure.StartsWith(prefix)));
		AssertEquals("Prerequisite: CEI_SubStyle is not P, S nor REC", false, invalidSubStyles.Any(subStyle => entryInstruction.CEI_SubStyle == subStyle));
		AssertEquals("Message type defaults to MA", MessageSendingMessageTypes.Codes.ManualDeclaration, action.MessageType);
	}

	public void TestMessageType_DefaultValue_KO() => CombineAssertions(() =>
	{
		var invalidSubStyles = new[] { "P", "REC" };

		entry.CH_EntryStatus = UniversalReferenceConstants.CusEntryStatus.MEC;
		action = GetMessageSendingObject();
		AssertEquals("When MEC, Prerequisite: CH_EntryStatus is MEC", UniversalReferenceConstants.CusEntryStatus.MEC, entry.CH_EntryStatus);
		AssertEquals("When MEC, Prerequisite: CEI_SubStyle is not P nor REC", false, invalidSubStyles.Any(subStyle => entryInstruction.CEI_SubStyle == subStyle));
		AssertEquals("When MEC, Message type defaults to KO", MessageSendingMessageTypes.Codes.Correction, action.MessageType);

		entry.CH_EntryStatus = UniversalReferenceConstants.CusEntryStatus.MEM;
		declaration.JE_GoodsNumber = "202501DT";
		action = GetMessageSendingObject();
		AssertEquals("When MEM, Prerequisite: CH_EntryStatus is MEM", UniversalReferenceConstants.CusEntryStatus.MEM, entry.CH_EntryStatus);
		AssertEquals("When MEM, Prerequisite: JE_GoodsNumber is a digitoll goods number", "DT", declaration.JE_GoodsNumber.Substring(6));
		AssertEquals("When MEM, Message type defaults to KO", MessageSendingMessageTypes.Codes.Correction, action.MessageType);
	});

	public void TestMessageType_DefaultValue_FO()
	{
		entryInstruction.CEI_SubStyle = "P";
		action = GetMessageSendingObject();

		AssertEquals("Prerequisite: CEI_SubStyle is P", "P", entryInstruction.CEI_SubStyle);
		AssertEquals("Message type defaults to FO", MessageSendingMessageTypes.Codes.PreliminaryDeclaration, action.MessageType);
	}

	public void TestMessageType_DefaultValue_EN()
	{
		entry.CH_EntryStatus = UniversalReferenceConstants.CusEntryStatus.UAR;
		action = GetMessageSendingObject();

		AssertEquals("Prerequisite: CH_EntryStatus is UAR", UniversalReferenceConstants.CusEntryStatus.UAR, entry.CH_EntryStatus);
		AssertEquals("Message type defaults to EN", MessageSendingMessageTypes.Codes.FinalDeclaration, action.MessageType);
	}

	public void TestMessageType_DefaultValue_RE()
	{
		entryInstruction.CEI_SubStyle = "REC";
		action = GetMessageSendingObject();

		AssertEquals("Prerequisite: CEI_SubStyle is REC", "REC", entryInstruction.CEI_SubStyle);
		AssertEquals("Message type defaults to RE", MessageSendingMessageTypes.Codes.RefundDeclaration, action.MessageType);
	}

	public void TestMessageType_DefaultValue_Empty()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		CombineAssertions(() =>
		{
			action = GetMessageSendingObject();
			AssertNotNullOrEmpty("DefaultValue is not empty", action.MessageType);

			entry.CH_BGMReference = "1234567890123456789012301";
			entry.CH_EntryStatus = MessageSendingStatusCodes.Codes.FinalApproval;
			action = GetMessageSendingObject();
			AssertEquals("DefaultValue Blank for TK", ZString.Empty, action.MessageType);

			entry.CH_EntryStatus = MessageSendingStatusCodes.Codes.RefusalOfDeclaration;
			action = GetMessageSendingObject();
			AssertEquals("DefaultValue Blank for IU", ZString.Empty, action.MessageType);
		});
	}

	public void TestisFirstMessageToSend()
	{
		CombineAssertions(() =>
		{
			entry.CH_EntryStatus = ZString.Empty;
			Assert("Empty EntryStatus", action.IsFirstMessageToSend);

			entry.CH_EntryStatus = MessageSendingStatusCodes.Codes.InputControlError;
			entry.CH_BGMReference = ZString.Empty;
			Assert("When BGM-Ref is empty", action.IsFirstMessageToSend);

			entry.CH_BGMReference = "1234567890123456789012301";
			Assert("When BGM-Ref is not empty", !action.IsFirstMessageToSend);
		});
	}

	public void TestShouldSend_DefaultValue() => CombineAssertions(() =>
	{
		foreach (var entryStatus in StatusAreNotAllowedToSendMessageToCustom)
		{
			entry.CH_EntryStatus = entryStatus;
			action = GetMessageSendingObject();
			AssertEquals($"When EntryStatus: {entryStatus}, ShouldSend", expected: false, action.ShouldSend);
			AssertEquals($"When EntryStatus: {entryStatus}, ShouldSend_ReadOnly", expected: true, action.ShouldSendInfo.ReadOnly);
		}

		foreach (var entryStatus in StatusAreAllowedToSendMessageToCustom)
		{
			entry.CH_EntryStatus = entryStatus;
			action = GetMessageSendingObject();
			AssertEquals($"When EntryStatus: {entryStatus}, ShouldSend", expected: true, action.ShouldSend);
			AssertEquals($"When EntryStatus: {entryStatus}, ShouldSend_ReadOnly", expected: false, action.ShouldSendInfo.ReadOnly);
		}
	});

	public void TestShouldSend_DefaultValue_DigitollDeclaration() => CombineAssertions(() =>
	{
		entry.CH_EntryStatus = UniversalReferenceConstants.CusEntryStatus.MEM;
		declaration.JE_GoodsNumber = "201501DT";
		action = GetMessageSendingObject();
		AssertEquals("DigiToll Declaration, ShouldSend", expected: true, action.ShouldSend);
		AssertEquals("DigiToll Declaration, ShouldSend_ReadOnly", expected: false, action.ShouldSendInfo.ReadOnly);
	});

	protected override BusinessObject GetNewBusinessObject() => GetMessageSendingObject();
	MessageSendingObject GetMessageSendingObject() => new MessageSendingObject(entry);

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		entry = declaration.CustomsEntryHeaders.AddNew();
		entryInstruction = Factory.NewWithValidTestData<CusEntryInstruction>();
		entry.CH_CEI_Instruction = entryInstruction.PK;
		action = GetMessageSendingObject();
	}

	MessageSendingObject action;
	JobDeclaration declaration;
	CusEntryHeader entry;
	CusEntryInstruction entryInstruction;

	void SetupCustomsOfficesForTest(OrgHeader declarant, ZString declarationNumber, ZString authHeaderType, ZString mcoCode)
	{
		var auth = Factory.New<CusAuthorisationHeader>();
		auth.CPH_Type = authHeaderType;
		auth.CPH_Number = declarationNumber;
		auth.CPH_OH_PermitHolder = declarant.PK;
		auth.CPH_OA_AppliesTo = declarant.MainAddress.PK;
		var rule = auth.CusAuthorisationRules.AddNew();
		rule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.MainCustomsOffice;
		rule.CPR_ValueFrom = mcoCode;
		var linkedRule = rule.LinkedCusAuthorisationRules.AddNew();
		linkedRule.CPR_RuleCode = LinkedCusAuthorisationRuleTypeList.Codes.ValidCustomsOffices;
		linkedRule.CPR_ValueFrom = "RandomCode";

		Factory.Save();
	}

	static readonly ImmutableHashSet<string> StatusAreNotAllowedToSendMessageToCustom = ImmutableHashSet.Create(
		UniversalReferenceConstants.CusEntryStatus.MEM,
		UniversalReferenceConstants.CusEntryStatus.MEG,
		UniversalReferenceConstants.CusEntryStatus.MED,
		UniversalReferenceConstants.CusEntryStatus.TKR
	);

	static readonly ImmutableHashSet<string> StatusAreAllowedToSendMessageToCustom = ImmutableHashSet.Create(
		UniversalReferenceConstants.CusEntryStatus.MEC,
		UniversalReferenceConstants.CusEntryStatus.IUR,
		UniversalReferenceConstants.CusEntryStatus.UAR,
		UniversalReferenceConstants.CusEntryStatus.Rejected,
		UniversalReferenceConstants.CusEntryStatus.NA
	);
}
