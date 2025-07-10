using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(CusAuthorizationUsage))]
sealed class CusAuthorizationUsageTest : EnterpriseBusinessObjectTestCase
{
	protected override BusinessObject GetNewBusinessObject()
	{
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		var cusAuthorizationUsage = CreateAuthorizationUsage();
		cusAuthorizationUsage.AGC_Code = "abc";
		cusAuthorizationUsage.AGC_Number = "123";
		cusAuthorizationUsage.AGC_OH_Owner = orgHeader.PK;
		return cusAuthorizationUsage;
	}

	CusAuthorizationUsage CreateAuthorizationUsage()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		return entryInstruction.CusAuthorizationUsages.AddNew();
	}

	public void TestGetNewValidation_EntryInstruction()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var authorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertType<ExportEntryInstructionCusAuthorizationUsageValidation>("Export", authorizationUsage.Validation);

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertType<ImportEntryInstructionCusAuthorizationUsageValidation>("Import", authorizationUsage.Validation);

			AssertType<CusAuthorizationUsageValidation>("Non CustomsEntryInstructions Authorisation", Factory.New<CusAuthorizationUsage>().Validation);
		});
	}

	public void TestGetNewValidation_InvoiceLine()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var authorizationUsage = invoiceLine.CusAuthorizationUsages.AddNew();

		CombineAssertions(() =>
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertType<ExportInvoiceLineCusAuthorizationUsageValidation>("Export", authorizationUsage.Validation);

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertType<CusAuthorizationUsageValidation>("Import", authorizationUsage.Validation);
		});
	}

	public void TestLookups() => AssertType<CusAuthorizationUsageLookups>(Factory.New<CusAuthorizationUsage>().Lookups);

	public void TestIsExport()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var authorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertEquals("EntryInstruction CusAuthorizationUsage Export", true, authorizationUsage.IsExport);

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertEquals("EntryInstruction CusAuthorizationUsage Export", false, authorizationUsage.IsExport);
		});
	}

	[TestDate(2023, 8, 12)]
	public void TestCustomsCodeValue_ValidDate()
	{
		CreateMapTypeData();
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var authorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("Empty code", ZString.Empty, authorizationUsage.CustomsCode);
			authorizationUsage.AGC_Code = "WAW";
			AssertEquals("Not exist data in mapping", ZString.Empty, authorizationUsage.CustomsCode);
			authorizationUsage.AGC_Code = "ACR";
			AssertEquals("Exist data in mapping", "C521", authorizationUsage.CustomsCode);
		});
	}

	[TestDate(2023, 9, 10)]
	public void TestCustomsCodeValue_InvalidDate()
	{
		CreateMapTypeData();
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var authorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("Empty code", ZString.Empty, authorizationUsage.CustomsCode);
			authorizationUsage.AGC_Code = "WAW";
			AssertEquals("Not exist data in mapping", ZString.Empty, authorizationUsage.CustomsCode);
			authorizationUsage.AGC_Code = "ACR";
			AssertEquals("Not exist data in mapping", ZString.Empty, authorizationUsage.CustomsCode);
		});
	}

	public void TestUseEffectiveReferenceNumber() => AssertEquals(true, Factory.New<CusAuthorizationUsage>().UseEffectiveReferenceNumber);

	void CreateMapTypeData()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var eunau = EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU;
		helper.CreateCusMapType(eunau, MapDirectionList.Codes.OUT, "EUNAU", true);
		helper.CreateCusMap(eunau, "ACR", "C521", new ZDateTime(2023, 8, 10), new ZDateTime(2023, 8, 30), Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		Factory.Save();
	}
}
