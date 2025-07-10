using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business.Declaration;
using NUnit.Framework;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(CustomsDeclarationMessageSendingObject))]
sealed class CustomsDeclarationMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
{
	public void TestActionList_Export()
	{
		var sendingObj = GetNewJobDeclarationMessageSendingObject();
		CombineAssertions(() =>
		{
			sendingObj.Header.Declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var list = sendingObj.ActionList;
			AssertEquals("Export - CodesAsString", "CC511, CC513, CC514, CC515, CC583", list.CodesAsString);
			AssertSame("Export - Cached", list, sendingObj.ActionList);
		});
	}

	public void TestActionList_Export_ControlledForExport()
	{
		var sendingObj = GetNewJobDeclarationMessageSendingObject();
		CombineAssertions(() =>
		{
			sendingObj.Header.Declaration.JE_MessageType = MessageTypeList.Codes.Export;
			sendingObj.Header.CH_EntryStatus = AESEntryStatusList.Codes.ControlledForExport;
			var list = sendingObj.ActionList;
			AssertEquals("Export - CodesAsString", "CC511, CC513, CC514, CC515, CC566, CC583", list.CodesAsString);
			AssertSame("Export - Cached", list, sendingObj.ActionList);
		});
	}

	public void TestActionList_Export_Import()
	{
		var sendingObj = GetNewJobDeclarationMessageSendingObject();
		CombineAssertions(() =>
		{
			sendingObj.Header.Declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var list = sendingObj.ActionList;
			AssertEquals("Import - CodesAsString", "ZC415", list.CodesAsString);
			AssertSame("Import - Cached", list, sendingObj.ActionList);
		});
	}

	public void TestEntryNumber_ReadOnly()
	{
		var sendingObj = GetNewJobDeclarationMessageSendingObject();
		var propertyInfo = sendingObj.EntryNumberInfo;
		AssertEquals("The EntryNumber should be always editable", false, propertyInfo.ReadOnly);
	}

	public void TestEntryNumberMaxLen()
	{
		var sendingObj = GetNewJobDeclarationMessageSendingObject();
		AssertEquals("No Action selected", 35, sendingObj.EntryNumberInfo.MaxLength);
	}

	public void TestExpectedEntryNumberLength()
	{
		var sendingObj = GetNewJobDeclarationMessageSendingObject();
		CombineAssertions(() =>
		{
			AssertEquals("No Action selected", 35, sendingObj.ExpectedEntryNumberLength);

			sendingObj.Action = MessageSendingObjectActionCodes.CC513;
			AssertEquals("IE513 Action type", 18, sendingObj.ExpectedEntryNumberLength);

			sendingObj.Action = MessageSendingObjectActionCodes.CC515;
			AssertEquals("Not IE513 Action type", 35, sendingObj.ExpectedEntryNumberLength);
		});
	}

	public void TestCheckRuleR0094E()
	{
		const string errorMessage = "[R0094E] For Declaration Type = 'EX' and Additional Declaration Type = 'A' or 'D' where Security = '2' Method of Payment is required.";
		var sendingObj = GetNewJobDeclarationMessageSendingObjectForSecurityValidation();
		sendingObj.Action = MessageSendingObjectActionCodes.CC513;
		var invoiceHeader = sendingObj.Header.Declaration.Invoices[0];
		CombineAssertions(() =>
		{
			AssertEquals("Method of Payment is empty by default", "", invoiceHeader.ZG_TransportChargesMethodOfPayment);
			AssertHasMessageError(sendingObj.SecurityInfo, errorMessage);
			sendingObj.Security = "0";
			AssertNoMessageErrorContaining(sendingObj.SecurityInfo, errorMessage);
			invoiceHeader.ZG_TransportChargesMethodOfPayment = "A";
			sendingObj.Security = "2";
			AssertNoMessageErrorContaining(sendingObj.SecurityInfo, errorMessage);
		});
	}

	public void TestCheckRuleR0095E()
	{
		const string errorMessage = "[R0095E] For Declaration Type = 'EX' and Additional Declaration Type = 'A' or 'D' Security must be value '2'";
		var sendingObj = GetNewJobDeclarationMessageSendingObjectForSecurityValidation();
		sendingObj.Action = MessageSendingObjectActionCodes.CC513;
		CombineAssertions(() =>
		{
			AssertEquals("Security is set to '2'", "2", sendingObj.Security);
			AssertNoMessageErrorContaining(sendingObj.SecurityInfo, errorMessage);
			sendingObj.Security = "0";
			AssertHasMessageError(sendingObj.SecurityInfo, errorMessage);
		});
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		return new CustomsDeclarationMessageSendingObject(entryHeader);
	}

	public void TestValidationType() => AssertType<CustomsDeclarationMessageSendingObjectValidation>(GetNewJobDeclarationMessageSendingObject().Validation);

	BaseMessageSendingObject GetNewJobDeclarationMessageSendingObject() => (CustomsDeclarationMessageSendingObject)GetNewBusinessObject();

	BaseMessageSendingObject GetNewJobDeclarationMessageSendingObjectForSecurityValidation()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		declaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportNormal;
		entryInstruction.CEI_SubStyle = SubStyleCodes.A;
		declaration.Invoices.AddNew();
		return new CustomsDeclarationMessageSendingObject(entryHeader);
	}
}
