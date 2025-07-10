namespace Enterprise.ReportTesting.Accounting
{
	using CargoWise.EntityFramework;
	using Enterprise.DocumentEngine.RuntimeOptions;
	using NUnit.Framework;

	[TemplateName("AP Aged Outstanding Transactions - Detail in Invoice Currency")]
	public class TestAPAgedOutstandingTransactionsDetailinInvoiceCurrency : TemplateTestCase
	{
		[ExpectNoExceptions]
		public void TestReportOnlyRequiredFilters()
		{
			PrepareReportForRender();
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportAgeingByDueDate()
		{
			PrepareReportForRender();
			((IValueAsStringProviderForUnitTests)Report.FilterCollection["Aging"]).ValueAsStringForSerialisation = "DUE";
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportAgeingByNone()
		{
			PrepareReportForRender();
			((IValueAsStringProviderForUnitTests)Report.FilterCollection["Aging"]).ValueAsStringForSerialisation = "NON";
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportAgeingOptionDay()
		{
			PrepareReportForRender();
			((IValueAsStringProviderForUnitTests)Report.FilterCollection["Aging Options"]).ValueAsStringForSerialisation = "DAY";
			((NumberField)Report.FilterCollection["Current"]).Value = 30;
			((NumberField)Report.FilterCollection["Period 1"]).Value = 60;
			((NumberField)Report.FilterCollection["Period 2"]).Value = 90;
			((NumberField)Report.FilterCollection["Period 3"]).Value = 120;
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportStaffMember()
		{
			var staff = Factory.LoadTop1<Enterprise.MasterFiles.Business.GlbStaff>(new ZQuery());
			PrepareReportForRender();
			((IValueAsStringProviderForUnitTests)Report.FilterCollection["Staff Member"]).ValueAsStringForSerialisation = staff.GS_Code;
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportOrderByTRN()
		{
			PrepareReportForRender();
			((IValueAsStringProviderForUnitTests)Report.FilterCollection["Order By"]).ValueAsStringForSerialisation = "TRN";
			RunReport();
		}

		#region LookUps

		[ExpectNoExceptions]
		public void TestReportOrganisationsLookUp()
		{
			var org = Factory.LoadTop1<Enterprise.MasterFiles.Business.OrgHeader>(new ZQuery());
			PrepareReportForRender();
			((IValueAsStringProviderForUnitTests)Report.FilterCollection["Organizations"]).ValueAsStringForSerialisation = org.OH_Code;
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportSettlementGroupLookUp()
		{
			var orgSettlementGroups = Factory.LoadTop1<Enterprise.MasterFiles.Business.OrgHeader>(new ZQuery());
			PrepareReportForRender();
			((IValueAsStringProviderForUnitTests)Report.FilterCollection["Settlement Groups"]).ValueAsStringForSerialisation = orgSettlementGroups.OH_Code;
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportCreditorGroupLookUp()
		{
			var orgCreditorGrp = Factory.LoadTop1<Enterprise.MasterFiles.Business.OrgCreditorGroup>(new ZQuery());
			PrepareReportForRender();
			((IValueAsStringProviderForUnitTests)Report.FilterCollection["Creditor Groups"]).ValueAsStringForSerialisation = orgCreditorGrp.OG_Code;
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportTransactionBranchLookUp()
		{
			var branch = Factory.LoadTop1<Enterprise.MasterFiles.Business.GlbBranch>(new ZQuery());
			PrepareReportForRender();
			((IValueAsStringProviderForUnitTests)Report.FilterCollection["Transaction Branches"]).ValueAsStringForSerialisation = branch.GB_Code;
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportInvoiceCurrencyLookUp()
		{
			var currency = Factory.LoadTop1<Enterprise.MasterFiles.Business.RefCurrency>(new ZQuery());
			PrepareReportForRender();
			((IValueAsStringProviderForUnitTests)Report.FilterCollection["Invoice Currency"]).ValueAsStringForSerialisation = currency.Code;
			RunReport();
		}

		#endregion

		#region Consolidation Cat

		[ExpectNoExceptions]
		public void TestReportConsolidationCatUNR()
		{
			PrepareReportForRender();
			((IValueAsStringProviderForUnitTests)Report.FilterCollection["Consolidation Cat"]).ValueAsStringForSerialisation = "UNR";
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportConsolidationCatWHO()
		{
			PrepareReportForRender();
			((IValueAsStringProviderForUnitTests)Report.FilterCollection["Consolidation Cat"]).ValueAsStringForSerialisation = "WHO";
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportConsolidationCatMIR()
		{
			PrepareReportForRender();
			((IValueAsStringProviderForUnitTests)Report.FilterCollection["Consolidation Cat"]).ValueAsStringForSerialisation = "MIR";
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportConsolidationCatMIN()
		{
			PrepareReportForRender();
			((IValueAsStringProviderForUnitTests)Report.FilterCollection["Consolidation Cat"]).ValueAsStringForSerialisation = "MIN";
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportConsolidationCatRMI()
		{
			PrepareReportForRender();
			((IValueAsStringProviderForUnitTests)Report.FilterCollection["Consolidation Cat"]).ValueAsStringForSerialisation = "RMI";
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportConsolidationCatRMJ()
		{
			PrepareReportForRender();
			((IValueAsStringProviderForUnitTests)Report.FilterCollection["Consolidation Cat"]).ValueAsStringForSerialisation = "RMJ";
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportConsolidationCatRWH()
		{
			PrepareReportForRender();
			((IValueAsStringProviderForUnitTests)Report.FilterCollection["Consolidation Cat"]).ValueAsStringForSerialisation = "RWH";
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportConsolidationCatGMI()
		{
			PrepareReportForRender();
			((IValueAsStringProviderForUnitTests)Report.FilterCollection["Consolidation Cat"]).ValueAsStringForSerialisation = "GMI";
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportConsolidationCatGMJ()
		{
			PrepareReportForRender();
			((IValueAsStringProviderForUnitTests)Report.FilterCollection["Consolidation Cat"]).ValueAsStringForSerialisation = "GMJ";
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportConsolidationCatTST()
		{
			PrepareReportForRender();
			((IValueAsStringProviderForUnitTests)Report.FilterCollection["Consolidation Cat"]).ValueAsStringForSerialisation = "TST";
			RunReport();
		}

		#endregion

		#region Accounts Relation

		[ExpectNoExceptions]
		public void TestReportAccountsRelationSTD()
		{
			PrepareReportForRender();
			((IValueAsStringProviderForUnitTests)Report.FilterCollection["Accounts Relation"]).ValueAsStringForSerialisation = "STD";
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportAccountsRelationKEY()
		{
			PrepareReportForRender();
			((IValueAsStringProviderForUnitTests)Report.FilterCollection["Accounts Relation"]).ValueAsStringForSerialisation = "KEY";
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportAccountsRelationSIG()
		{
			PrepareReportForRender();
			((IValueAsStringProviderForUnitTests)Report.FilterCollection["Accounts Relation"]).ValueAsStringForSerialisation = "SIG";
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportAccountsRelationALT()
		{
			PrepareReportForRender();
			((IValueAsStringProviderForUnitTests)Report.FilterCollection["Accounts Relation"]).ValueAsStringForSerialisation = "ALT";
			RunReport();
		}

		#endregion

		#region GroupBys

		[ExpectNoExceptions]
		public void TestReportGroupByConsolidationCat()
		{
			PrepareReportForRender();
			Report.GroupByCollection["Consolidation Category"].Selected = true;
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportGroupByAccountsRelationship()
		{
			PrepareReportForRender();
			Report.GroupByCollection["Accounts Relationship"].Selected = true;
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportGroupByOrgBranch()
		{
			PrepareReportForRender();
			Report.GroupByCollection["Organization Branch"].Selected = true;
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportGroupByTransactionBranch()
		{
			PrepareReportForRender();
			Report.GroupByCollection["Transaction Branch"].Selected = true;
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportGroupBySalesRep()
		{
			PrepareReportForRender();
			Report.GroupByCollection["Sales Rep"].Selected = true;
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportGroupByCreditController()
		{
			PrepareReportForRender();
			Report.GroupByCollection["Credit Controller"].Selected = true;
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportGroupByCustomerServiceRep()
		{
			PrepareReportForRender();
			Report.GroupByCollection["Customer Service Rep"].Selected = true;
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportGroupBySettlementGroup()
		{
			PrepareReportForRender();
			Report.GroupByCollection["Settlement Group"].Selected = true;
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportGroupByCreditorGroup()
		{
			PrepareReportForRender();
			Report.GroupByCollection["Creditor Group"].Selected = true;
			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportGroupByCurrency()
		{
			PrepareReportForRender();
			Report.GroupByCollection["Currency"].Selected = true;
			RunReport();
		}

		#endregion
	}
}
