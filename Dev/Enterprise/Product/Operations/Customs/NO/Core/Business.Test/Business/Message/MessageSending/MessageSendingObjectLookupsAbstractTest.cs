using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestsSubclassesOf(typeof(MessageSendingObjectLookups))]
abstract class MessageSendingObjectLookupsAbstractTest<T> : BusinessObjectLookupsTestCase where T : MessageSendingObjectLookups
{
	protected abstract string MessageType { get; }

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageType;
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		sendingObject = new MessageSendingObject(entryHeader);
		lookups = sendingObject.Lookups as T;
		entryInstruction = Factory.NewWithValidTestData<CusEntryInstruction>();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
	}

	// TODO: These constants are to be removed in a future WI, once they are added as valid values in the GUI
	const string SimplifiedDeclarationSubType = "S";
	const string RecalculationDeclarationSubType = "REC";

	protected JobDeclaration declaration;
	protected CusEntryHeader entryHeader;
	protected MessageSendingObject sendingObject;
	protected T lookups;
	protected CusEntryInstruction entryInstruction;

	protected BusinessObject GetNewBusinessObject()
	{
		return new MessageSendingObject(entryHeader);
	}

	protected void SetupCustomsOfficesForTest(ZString authHeaderType, ZString mcoCode, string[] vcoCodes, OrgHeader declarant)
	{
		var auth = Factory.New<CusAuthorisationHeader>();
		auth.CPH_Type = authHeaderType;
		auth.CPH_Number = "1234";
		auth.CPH_OH_PermitHolder = declarant.PK;
		auth.CPH_OA_AppliesTo = declarant.MainAddress.PK;
		var rule = auth.CusAuthorisationRules.AddNew();
		rule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.MainCustomsOffice;
		rule.CPR_ValueFrom = mcoCode;
		rule.CPR_Description = $"{mcoCode} Des";
		foreach (var vco in vcoCodes)
		{
			var linkedRule = rule.LinkedCusAuthorisationRules.AddNew();
			linkedRule.CPR_RuleCode = LinkedCusAuthorisationRuleTypeList.Codes.ValidCustomsOffices;
			linkedRule.CPR_ValueFrom = vco;
			linkedRule.CPR_Description = $"{vco} Des";
		}
	}

	public void TestDefaultMessageTypeIsFU() => CombineAssertions(() =>
	{
		var exportValidProcedures = new[] { "1000", "1100" };
		var importValidProcedures = new[] { "4000", "4100" };
		var validProcedures = MessageType == SharedJobMessageTypeList.Codes.Export ? exportValidProcedures : importValidProcedures;
		var validSubStyles = new[] { ImportDeclarationSubTypes.Codes.N, ImportDeclarationSubTypes.Codes.C };

		entryHeader.CH_EntryStatus = ZString.Empty;

		foreach (var validProcedure in validProcedures)
		{
			entryInstruction.CEI_Procedure = validProcedure;
			foreach (var validSubStyle in validSubStyles)
			{
				entryInstruction.CEI_SubStyle = validSubStyle;
				AssertEquals($"FU is set to default when CEI_Procedure is {validProcedure} and CEI_SubStyle is {validSubStyle}", $"{MessageSendingMessageTypes.Codes.CompleteOrdinaryDeclaration}, {MessageSendingMessageTypes.Codes.ManualDeclaration}", lookups.MessageTypeList.CodesAsString);
			}
		}
	});

	public virtual void TestDefaultMessageTypeIsNotFU() => CombineAssertions(() =>
	{
		var exportInvalidProcedures = new[] { "0100", "0200", "0300", "0400", "0500", "0600", "2000", "2100", "2200", "3000", "8000", "8100", "8200" };
		var importInvalidProcedures = new[] { "5000", "5100", "5200", "5700", "6000", "7000", "7100" };
		var invalidProcedures = MessageType == SharedJobMessageTypeList.Codes.Export ? exportInvalidProcedures : importInvalidProcedures;
		var invalidSubStyles = new[] { ImportDeclarationSubTypes.Codes.P, SimplifiedDeclarationSubType, RecalculationDeclarationSubType };

		entryHeader.CH_EntryStatus = ZString.Empty;
		entryInstruction.CEI_SubStyle = "N";

		foreach (var invalidProcedure in invalidProcedures)
		{
			entryInstruction.CEI_Procedure = invalidProcedure;
			AssertNotEquals($"FU is not set to default when CEI_Procedure is {invalidProcedure}",$"{MessageSendingMessageTypes.Codes.CompleteOrdinaryDeclaration}, {MessageSendingMessageTypes.Codes.ManualDeclaration}", lookups.MessageTypeList.CodesAsString);
		}

		entryInstruction.CEI_Procedure = "4000";

		foreach (var invalidSubStyle in invalidSubStyles)
		{
			entryInstruction.CEI_SubStyle = invalidSubStyle;
			AssertNotEquals($"FU is not set to default when CEI_SubStyle is {invalidSubStyle}", $"{MessageSendingMessageTypes.Codes.CompleteOrdinaryDeclaration}, {MessageSendingMessageTypes.Codes.ManualDeclaration}", lookups.MessageTypeList.CodesAsString);
		}

		entryInstruction.CEI_SubStyle = "N";
		entryHeader.CH_EntryStatus = MessageSendingStatusCodes.Codes.FinalApproval;

		AssertNotEquals("FU is not set to default when CH_BGMReference is not empty", MessageSendingMessageTypes.Codes.ManualDeclaration, lookups.MessageTypeList.CodesAsString);
	});

	public void TestDefaultMessageTypeIsMA() => CombineAssertions(() =>
	{
		var exportValidProcedures = new[] { "0100", "0200", "0300", "0400", "0500", "0600", "2000", "2100", "2200", "3000", "8000", "8100", "8200" };
		var importValidProcedures = new[] { "5000", "5100", "5200", "5700", "6000", "7000", "7100" };
		var validProcedures = MessageType == SharedJobMessageTypeList.Codes.Export ? exportValidProcedures : importValidProcedures;
		var validSubStyles = new[] { ImportDeclarationSubTypes.Codes.N, ImportDeclarationSubTypes.Codes.C };

		entryHeader.CH_EntryStatus = ZString.Empty;

		foreach (var validProcedure in validProcedures)
		{
			entryInstruction.CEI_Procedure = validProcedure;
			foreach (var validSubStyle in validSubStyles)
			{
				entryInstruction.CEI_SubStyle = validSubStyle;
				AssertEquals($"MA is set to default when CEI_Procedure is {validProcedure} and CEI_SubStyle is {validSubStyle}", MessageSendingMessageTypes.Codes.ManualDeclaration, lookups.MessageTypeList.CodesAsString);
			}
		}
	});

	public void TestDefaultMessageTypeIsNotMA() => CombineAssertions(() =>
	{
		var exportInvalidProcedures = new[] { "1000", "1100" };
		var importInvalidProcedures = new[] { "4000", "4100" };
		var invalidProcedures = MessageType == SharedJobMessageTypeList.Codes.Export ? exportInvalidProcedures : importInvalidProcedures;
		var invalidSubStyles = new[] { ImportDeclarationSubTypes.Codes.P, SimplifiedDeclarationSubType, RecalculationDeclarationSubType };

		entryHeader.CH_EntryStatus = ZString.Empty;
		entryInstruction.CEI_SubStyle = "N";

		foreach (var invalidProcedure in invalidProcedures)
		{
			entryInstruction.CEI_Procedure = invalidProcedure;
			AssertNotEquals($"MA is not set to default when CEI_Procedure is {invalidProcedure}", MessageSendingMessageTypes.Codes.ManualDeclaration, lookups.MessageTypeList.CodesAsString);
		}

		entryInstruction.CEI_Procedure = "5000";

		foreach (var invalidSubStyle in invalidSubStyles)
		{
			entryInstruction.CEI_SubStyle = invalidSubStyle;
			AssertNotEquals($"MA is not set to default when CEI_SubStyle is {invalidSubStyle}", MessageSendingMessageTypes.Codes.ManualDeclaration, lookups.MessageTypeList.CodesAsString);
		}

		entryInstruction.CEI_SubStyle = "N";
		entryHeader.CH_BGMReference = "1234567890123456789012345";

		AssertNotEquals("MA is not set to default when CH_BGMReference is not empty", MessageSendingMessageTypes.Codes.ManualDeclaration, lookups.MessageTypeList.CodesAsString);
	});

	public void TestDefaultMessageTypeIsKO() => CombineAssertions(() =>
	{
		var exportValidSubStyles = new[] { ExportDeclarationSubTypes.Codes.N, SimplifiedDeclarationSubType };
		var importValidSubStyles = new[] { ImportDeclarationSubTypes.Codes.N, ImportDeclarationSubTypes.Codes.C, SimplifiedDeclarationSubType };
		var validSubStyles = MessageType == SharedJobMessageTypeList.Codes.Export ? exportValidSubStyles : importValidSubStyles;

		entryHeader.CH_EntryStatus = UniversalReferenceConstants.CusEntryStatus.MEC;

		foreach (var validSubStyle in validSubStyles)
		{
			entryInstruction.CEI_SubStyle = validSubStyle;
			AssertEquals($"KO is set to default when CH_EntryStatus is ME and CEI_SubStyle is {validSubStyle}", MessageSendingMessageTypes.Codes.Correction, lookups.MessageTypeList.CodesAsString);
		}
	});

	public void TestDefaultMessageTypeIsNotKO() => CombineAssertions(() =>
	{
		var invalidSubStyles = new[] { ImportDeclarationSubTypes.Codes.P, RecalculationDeclarationSubType };
		var invalidStatusCodes = new[] { UniversalReferenceConstants.CusEntryStatus.UAR, MessageSendingStatusCodes.Codes.FinalApproval, MessageSendingStatusCodes.Codes.InputControlError, MessageSendingStatusCodes.Codes.RefusalOfDeclaration };

		entryHeader.CH_EntryStatus = UniversalReferenceConstants.CusEntryStatus.MEC;

		foreach (var invalidSubStyle in invalidSubStyles)
		{
			entryInstruction.CEI_SubStyle = invalidSubStyle;
			AssertNotEquals($"KO is not set to default when CEI_SubStyle is {invalidSubStyle}", MessageSendingMessageTypes.Codes.Correction, lookups.MessageTypeList.CodesAsString);
		}

		entryInstruction.CEI_SubStyle = ImportDeclarationSubTypes.Codes.N;

		foreach (var invalidStatusCode in invalidStatusCodes)
		{
			entryHeader.CH_EntryStatus = invalidStatusCode;
			AssertNotEquals($"KO is not set to default when CH_EntryStatus is {invalidStatusCode}", MessageSendingMessageTypes.Codes.Correction, lookups.MessageTypeList.CodesAsString);
		}
	});

	public void TestDefaultMessageTypeIsFO()
	{
		entryInstruction.CEI_SubStyle = ImportDeclarationSubTypes.Codes.P;

		AssertEquals("FO is set to default when CEI_SubStyle is P",MessageSendingMessageTypes.Codes.PreliminaryDeclaration, lookups.MessageTypeList.CodesAsString);
	}

	public void TestDefaultMessageTypeIsNotFO() => CombineAssertions(() =>
	{
		var exportInvalidSubStyles = new[] { ExportDeclarationSubTypes.Codes.N, SimplifiedDeclarationSubType, RecalculationDeclarationSubType };
		var importInvalidSubStyles = new[] { ImportDeclarationSubTypes.Codes.N, ImportDeclarationSubTypes.Codes.C, SimplifiedDeclarationSubType, RecalculationDeclarationSubType };
		var invalidSubStyles = MessageType == SharedJobMessageTypeList.Codes.Export ? exportInvalidSubStyles : importInvalidSubStyles;

		foreach (var invalidSubStyle in invalidSubStyles)
		{
			entryInstruction.CEI_SubStyle = invalidSubStyle;
			AssertNotEquals($"FO is not set to default when CEI_SubStyle is {invalidSubStyle}", MessageSendingMessageTypes.Codes.PreliminaryDeclaration, lookups.MessageTypeList.CodesAsString);
		}
	});

	public void TestDefaultMessageTypeIsEN()
	{
		entryHeader.CH_EntryStatus = UniversalReferenceConstants.CusEntryStatus.UAR;

		AssertEquals("EN is set to default when CH_EntryStatus is set to UAR", MessageSendingMessageTypes.Codes.FinalDeclaration, lookups.MessageTypeList.CodesAsString);
	}

	public void TestDefaultMessageTypeIsNotEN() => CombineAssertions(() =>
	{
		var invalidStatusCodes = new[] { MessageSendingStatusCodes.Codes.MessageFromCustoms, MessageSendingStatusCodes.Codes.FinalApproval, MessageSendingStatusCodes.Codes.InputControlError, MessageSendingStatusCodes.Codes.RefusalOfDeclaration };

		foreach (var invalidStatusCode in invalidStatusCodes)
		{
			entryHeader.CH_EntryStatus = invalidStatusCode;
			AssertNotEquals($"EN is not set to default when CH_EntryStatus is {invalidStatusCode}", MessageSendingMessageTypes.Codes.FinalDeclaration, lookups.MessageTypeList.CodesAsString);
		}
	});

	public void TestDefaultMessageTypeIsRE()
	{
		entryInstruction.CEI_SubStyle = RecalculationDeclarationSubType;

		AssertEquals("RE is set to default when CEI_SubStyle is REC", $"{MessageSendingMessageTypes.Codes.RefundDeclaration}, {MessageSendingMessageTypes.Codes.PostDeclaration}, {MessageSendingMessageTypes.Codes.StatisticalRecalculatedDeclaration}", lookups.MessageTypeList.CodesAsString);
	}
}
