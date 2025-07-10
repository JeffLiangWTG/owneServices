using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

class JobComInvoiceLineLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestInvoiceLine()
	{
		var parent = Factory.New<JobComInvoiceLine>();
		AssertEquals(parent.Lookups.InvoiceLine, parent);
	}

	public void TestCPCList()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

		var helper = new UniversalReferenceTestDataHelper(Factory);
		var proc1 = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.Poland, "", "10", "11", "111", "One", "IMP", "");
		var proc2 = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.Poland, "", "10", "22", "222", "Two", "IMP", "");
		var proc3 = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.Poland, "", "10", "33", "333", "Three", "EXP", "");
		var proc4 = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.Poland, "", "20", "44", "444", "Four", "IMP", "");

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		CombineAssertions(() =>
		{
			invoiceLine.JI_CEI = entryInstruction.PK;
			var cpcList = invoiceLine.Lookups.CPCList;
			AssertEquals("CPC List should have procedure codes filtered by shipmentType & declarationType", 3, cpcList.Count);
			AssertEquals(true, cpcList.Contains(proc1));
			AssertEquals(true, cpcList.Contains(proc2));
			AssertEquals(true, cpcList.Contains(proc4));
			AssertEquals(false, cpcList.Contains(proc3));
			AssertEquals("CPC List does not contain default filter for CPC", false, cpcList.FilterBusinessObjectDefaults.ContainsDefaultFor("CPC:Property"));

			entryInstruction.CEI_Procedure = "10";
			cpcList = invoiceLine.Lookups.CPCList;
			AssertEquals("CPC List should have procedure codes filtered by shipmentType & declarationType", 2, cpcList.Count);
			AssertEquals(true, cpcList.Contains(proc1));
			AssertEquals(true, cpcList.Contains(proc2));
			AssertEquals(false, cpcList.Contains(proc4));
			AssertEquals(false, cpcList.Contains(proc3));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			cpcList = invoiceLine.Lookups.CPCList;
			AssertEquals("CPC List should have procedure codes filtered by shipmentType & declarationType", 1, cpcList.Count);
			AssertEquals(false, cpcList.Contains(proc1));
			AssertEquals(false, cpcList.Contains(proc2));
			AssertEquals(false, cpcList.Contains(proc4));
			AssertEquals(true, cpcList.Contains(proc3));
		});
	}

	public void TestPLMarkModelCollection()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		const string codeType = PL.Business.UniversalReferenceConstants.RefCusCodeListType.Codes.CarMarkModel;
		helper.CreateNewOrGetExistingCusCodeType(codeType, "Mark, Model");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Poland, codeType, "1111", "mark1,model1", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Poland, codeType, "2222", "mark2,model2", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		var list = invoiceLine.Lookups.PLMarkModelCollection;
		CombineAssertions(() =>
		{
			list.Load();
			var cusCodeListCombined = list.Cast<ZZRefCusCodeListCombined>().ToArray();
			AssertContainsExactElementsInAnyOrder("Codes", new ZString[] { "1111", "2222" }, cusCodeListCombined.Select(x => x.ZZD_Code));
			AssertContainsExactElementsInAnyOrder("Descriptions", new ZString[] { "mark1,model1", "mark2,model2" }, cusCodeListCombined.Select(x => x.ZZD_Description));
			AssertEquals("FilterBusinessObjectDefaults|Code:Property|NotExisting", false, list.FilterBusinessObjectDefaults.ContainsDefaultFor("Code:Property"));
			AssertEquals("FilterBusinessObjectDefaults|Description:Property|NotExisting", false, list.FilterBusinessObjectDefaults.ContainsDefaultFor("Description:Property"));
			invoiceLine.JI_MarkModel = "mark1,model1";
			list = invoiceLine.Lookups.PLMarkModelCollection;
			AssertEquals("FilterBusinessObjectDefaults|Code:Property|Value", "1111", list.FilterBusinessObjectDefaults["Code:Property"].Value);
			AssertEquals("FilterBusinessObjectDefaults|Description:Property|Value", "mark1,model1", list.FilterBusinessObjectDefaults["Description:Property"].Value);
			invoiceLine.JI_MarkModel = ZString.Empty;
			list = invoiceLine.Lookups.PLMarkModelCollection;
			AssertEquals("FilterBusinessObjectDefaults|Code:Property|Removed", false, list.FilterBusinessObjectDefaults.ContainsDefaultFor("Code:Property"));
			AssertEquals("FilterBusinessObjectDefaults|Description:Property|Removed", false, list.FilterBusinessObjectDefaults.ContainsDefaultFor("Description:Property"));
		});
	}

	public void TestRequestedCustomsProcedureCodes()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		const string codeType = UniversalReferenceConstants.RefCusCodeListType.Codes.CustomsProcedures;
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Poland, "Poland");
		helper.CreateNewOrGetExistingCusCodeType(codeType, "CustomsProcedures", Core.Constants.CountryCodes.Poland);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Poland, codeType, "99", "AB", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.Poland, ZString.Empty, "99", "00", ZString.Empty, ZString.Empty, "EXP", "");
		Factory.Save();
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		var list = invoiceLine.Lookups.RequestedCustomsProcedureCodes;

		CombineAssertions(() =>
		{
			AssertEquals("ElementsAsString", "99 - AB", list.ElementsAsString);
			AssertSame("Cached", list, invoiceLine.Lookups.RequestedCustomsProcedureCodes);
		});
	}

	public void TestPreviousCustomsProcedureCodes()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		const string codeType = UniversalReferenceConstants.RefCusCodeListType.Codes.CustomsProcedures;
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Poland, "Poland");
		helper.CreateNewOrGetExistingCusCodeType(codeType, "CustomsProcedures", Core.Constants.CountryCodes.Poland);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Poland, codeType, "99", "AB", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Poland, codeType, "00", "CD", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Poland, codeType, "10", "GH", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.Poland, ZString.Empty, "99", ZString.Empty, ZString.Empty, ZString.Empty, "EXP", "");
		helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.Poland, ZString.Empty, "99", "00", ZString.Empty, ZString.Empty, "EXP", "");
		helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.Poland, ZString.Empty, "99", "10", ZString.Empty, ZString.Empty, "EXP", "");
		helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.Poland, ZString.Empty, "99", "00", "1V1", ZString.Empty, "EXP", "");
		helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.Poland, ZString.Empty, "99", "10", "1V1", ZString.Empty, "EXP", "");
		Factory.Save();
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_Procedure = "9900000";
		var list = invoiceLine.Lookups.PreviousCustomsProcedureCodes;

		CombineAssertions(() =>
		{
			AssertEquals("ElementsAsString", @"00 - CD
10 - GH", list.ElementsAsString);
			AssertSame("Cached", list, invoiceLine.Lookups.PreviousCustomsProcedureCodes);
		});
	}

	public void TestPreviousCustomsProcedureCodes_ProcedureCodeBaseIsEmpty()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_Procedure = "  10000";
		var list = invoiceLine.Lookups.PreviousCustomsProcedureCodes;
		AssertEquals(0, list.Count);
	}

	public void TestConcessionCodes()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		const string codeType = UniversalReferenceConstants.RefCusCodeListType.Codes.CustomsConcessions;
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Poland, "Poland");
		helper.CreateNewOrGetExistingCusCodeType(codeType, "CustomsConcessions", Core.Constants.CountryCodes.Poland);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Poland, codeType, "1V1", "JK", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Poland, codeType, "0AB", "DD", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Poland, codeType, "2VV", "EE", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.Poland, ZString.Empty, "99", ZString.Empty, ZString.Empty, ZString.Empty, "EXP", "");
		helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.Poland, ZString.Empty, "99", "00", ZString.Empty, ZString.Empty, "EXP", "");
		helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.Poland, ZString.Empty, "99", "10", ZString.Empty, ZString.Empty, "EXP", "");
		helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.Poland, ZString.Empty, "99", "00", "1V1", ZString.Empty, "EXP", "");
		helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.Poland, ZString.Empty, "99", "10", "1V1", ZString.Empty, "EXP", "");
		helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.Poland, ZString.Empty, "99", "00", "0AB", ZString.Empty, "EXP", "");
		helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.Poland, ZString.Empty, "99", "10", "0AB", ZString.Empty, "EXP", "");
		helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.Poland, ZString.Empty, "99", "00", "2VV", ZString.Empty, "EXP", "");
		Factory.Save();
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_Procedure = "9910000";
		var list = invoiceLine.Lookups.ConcessionCodes;

		CombineAssertions(() =>
		{
			AssertEquals("ElementsAsString", @"0AB - DD
1V1 - JK", list.ElementsAsString);
			AssertSame("Cached", list, invoiceLine.Lookups.ConcessionCodes);
		});
	}

	public void TestConcessionCodes_ProcedureCodeBaseAndPreviousProcedureCodeAreEmpty()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_Procedure = "    000";
		var list = invoiceLine.Lookups.ConcessionCodes;
		AssertEquals(0, list.Count);
	}

	public void TestGetDateOfValuation()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		var lookups = new JobComInvoiceLineLookupsExposed(invoiceLine);

		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Import fallback GetDateOfValuation", invoiceLine.EffectiveAssessmentDate, lookups.GetDateOfValuation_Exposed());

			entryInstruction.CEI_DateForDuty = new ZDateTime(2022, 02, 11);
			AssertEquals("Import EntryInstruction GetDateOfValuation", invoiceLine.EffectiveAssessmentDate, lookups.GetDateOfValuation_Exposed());

			invoiceLine.JI_DateForDutyOverride = new ZDateTime(2022, 02, 02);
			AssertEquals("Import InvoiceLine GetDateOfValuation", invoiceLine.EffectiveAssessmentDate, lookups.GetDateOfValuation_Exposed());

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			entryInstruction.CEI_DateForDuty = ZDateTime.Empty;
			AssertEquals("Export fallback GetDateOfValuation", invoiceLine.EffectiveAssessmentDate, lookups.GetDateOfValuation_Exposed());

			entryInstruction.CEI_DateForDuty = new ZDateTime(2022, 02, 11);
			AssertEquals("Export EntryInstruction GetDateOfValuation", invoiceLine.EffectiveAssessmentDate, lookups.GetDateOfValuation_Exposed());
		});
	}

	public void TestCustomsUQListType()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		var lookups = new JobComInvoiceLineLookupsExposed(invoiceLine);

		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Export CustomsUQListType", UniversalReferenceConstants.RefCusCodeListType.Codes.ExportCustomsDeclarationUnitsOfQuantity, lookups.CustomsUQListTypeExposed);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Import CustomsUQListType", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, lookups.CustomsUQListTypeExposed);
		});
	}

	public void TestCountryOfOrigin()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var countryOfOrigins = invoiceLine.Lookups.CountryOfOrigins;

		AssertNotEquals(0, countryOfOrigins.Count);
	}

	class JobComInvoiceLineLookupsExposed : JobComInvoiceLineLookups
	{
		public JobComInvoiceLineLookupsExposed(JobComInvoiceLine parent) : base(parent)
		{
		}

		public ZDateTime GetDateOfValuation_Exposed() => GetDateOfValuation();

		public ZString CustomsUQListTypeExposed => base.CustomsUQListType;
	}
}
