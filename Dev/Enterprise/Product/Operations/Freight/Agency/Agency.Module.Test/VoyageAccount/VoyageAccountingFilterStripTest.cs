using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Module.Testing
{
	[TestedType(typeof(VoyageAccountingFilterStrip))]
	internal class VoyageAccountingFilterStripTest : FilterStripBusinessObjectTestCase
	{
		public void TestBaseFilter()
		{
			GlbCompany otherCompany = Factory.New<GlbCompany>();
			otherCompany.GC_Code = "BOB";
			Account1.NA_GC = GlbCompany.CurrentCompany.PK;
			Account2.NA_GC = otherCompany.PK;
			Factory.Save();
			AssertMatches("", FilterStrip.Filter, Account1);
		}

		public void TestTradeLaneFilter()
		{
			JobTradeLane tradeLane1 = Factory.New<JobTradeLane>();
			JobTradeLane tradeLane2 = Factory.New<JobTradeLane>();
			tradeLane1.EJ_Code = "STL";
			tradeLane2.EJ_Code = "ATL";
			tradeLane1.EJ_OH_RelatedOrg = Principal1.PK;
			tradeLane2.EJ_OH_RelatedOrg = Principal2.PK;
			JobTradeLaneVoyage tradeLaneVoyage1 = Factory.New<JobTradeLaneVoyage>();
			JobTradeLaneVoyage tradeLaneVoyage2 = Factory.New<JobTradeLaneVoyage>();
			tradeLaneVoyage1.NB_EJ = tradeLane1.PK;
			tradeLaneVoyage2.NB_EJ = tradeLane2.PK;
			tradeLaneVoyage1.NB_JV = Voyage1.PK;
			tradeLaneVoyage2.NB_JV = Voyage2.PK;
			tradeLaneVoyage1.NB_OH = Principal1.PK;
			tradeLaneVoyage2.NB_OH = Principal2.PK;
			Account1.NA_JV = Voyage1.PK;
			Account1.NA_OH = Principal1.PK;
			Account2.NA_JV = Voyage2.PK;
			Account2.NA_OH = Principal2.PK;
			Factory.Save();
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStrip[VoyageAccountingFilterStrip.Descriptions.TradeLane];
			filter.IsActive = true;
			filter.Property = ZGuid.Empty;
			AssertMatches("empty filter", filter, Account1, Account2);
			filter.Property = tradeLane1.PK;
			AssertMatches("Account1", filter, Account1);
		}

		public void TestJobNumberFilter()
		{
			Account1.NA_JobNumber = "VA00010001";
			Account2.NA_JobNumber = "VA00010002";
			Factory.Save();
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[VoyageAccountingFilterStrip.Descriptions.JobNumber];
			filter.Property = "";
			AssertMatches("empty filter", filter.Query, Account1, Account2);
			filter.Property = "VA00010001";
			AssertMatches("Account1", filter.Query, Account1);
		}

		public void TestPrincipalFilter()
		{
			Account1.NA_OH = Principal1.PK;
			Account2.NA_OH = Principal2.PK;
			Factory.Save();
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStrip[VoyageAccountingFilterStrip.Descriptions.Principal];
			filter.Property = ZGuid.Empty;
			AssertMatches("empty filter", filter.Query, Account1, Account2);
			filter.Property = Principal1.PK;
			AssertMatches("Principal1", filter.Query, Account1);
		}

		public void TestValidatePrincipalFilter()
		{
			string expectedError = "Please select a principal to filter by";
			ModuleGuidFilter filter;
			Env.Security.AgencyPrincipalAccess.IsAllowed = true;
			filter = (ModuleGuidFilter)FilterStrip[VoyageAccountingFilterStrip.Descriptions.Principal];
			filter.Property = ZGuid.Empty;
			filter.Validation.ValidateProperty();
			AssertNoError(filter.PropertyInfo, expectedError);
			filterStrip = null;
			Env.Security.AgencyPrincipalAccess.IsAllowed = false;
			filter = (ModuleGuidFilter)FilterStrip[VoyageAccountingFilterStrip.Descriptions.Principal];
			filter.Validation.ValidateProperty();
			AssertHasError(filter.PropertyInfo, expectedError);
			filter.Property = ZGuid.NewZGuid();
			filter.Validation.ValidateProperty();
			AssertNoError(filter.PropertyInfo, expectedError);
		}

		public void TestVoyageVesselFilter()
		{
			var vessel1 = Factory.NewWithValidTestData<RefVessel>();
			vessel1.RV_Name = "VesselA";
			var vessel2 = Factory.NewWithValidTestData<RefVessel>();
			vessel2.RV_Name = "VesselB";
			Voyage1.JV_VoyageFlight = "0001";
			Voyage1.JV_RV_NKVessel = vessel1.RV_FK;
			Voyage2.JV_VoyageFlight = "0001";
			Voyage2.JV_RV_NKVessel = vessel2.RV_FK;
			Voyage3.JV_VoyageFlight = "002";
			Voyage3.JV_RV_NKVessel = vessel2.RV_FK;
			Factory.Save();
			var filter = (VoyageVesselModuleFilter)FilterStrip[VoyageAccountingFilterStrip.Descriptions.VoyageVessel];
			filter.VoyageFlightNo = "";
			AssertMatches("empty filter", filter, Account1, Account2, Account3);
			filter.VoyageFlightNo = "0001";
			AssertMatches("voyage", filter, Account1, Account2);
			filter.Vessel = "VesselB";
			AssertMatches("voyage + vessel", filter, Account2);
			filter.VoyageFlightNo = "";
			AssertMatches("vessel", filter, Account2, Account3);
			filter.Vessel = "VesselB";
			filter.VoyageFlightNo = "0";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			AssertMatches("vessel", filter, Account2, Account3);
			filter.Vessel = "";
			filter.VoyageFlightNo = "";
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			AssertMatches("Is not blank filter", filter, Account1, Account2, Account3);
		}

		public void TestInvoiceStatusFilter()
		{
			JobHeader job1 = new JobHeader.Loader(Account1).TryLoadOrCreate();
			job1.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job1.JH_Status = "WRK";
			JobHeader job2 = new JobHeader.Loader(Account2).TryLoadOrCreate();
			job2.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job2.JH_Status = "INV";
			Factory.Save();
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip["Invoice Status"];
			filter.Property = "";
			AssertMatches("Empty Filter", filter.Query, Account1, Account2);
			filter.Property = "INV";
			AssertMatches("INV", filter.Query, Account2);
		}

		/*Please create JobHeader via switching user context*/
		[SuspendToTestReportJobIsChangedByDifferentCompany]
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestInvoicedChargesFilter()
		{
			GlbBranch currentBranch = GlbBranch.CurrentBranch;
			GlbBranch otherBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, currentBranch.GB_GC));
			AccChargeCode code = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "CCLR"));
			JobHeader headerWithoutCharges = Factory.NewJobForTesting<JobHeader>();
			headerWithoutCharges.JH_ParentID = Account1.PK;
			headerWithoutCharges.JH_GB = currentBranch.PK;
			headerWithoutCharges.JH_GE = GlbDepartment.CurrentDepartment.PK;
			headerWithoutCharges.JH_JobNum = "Job1";
			JobHeader headerWithCharges = Factory.NewJobForTesting<JobHeader>();
			headerWithCharges.JH_ParentID = Account2.PK;
			headerWithCharges.JH_GB = currentBranch.PK;
			headerWithCharges.JH_GE = GlbDepartment.CurrentDepartment.PK;
			headerWithCharges.JH_JobNum = "Job2";
			JobHeader otherHeaderWithCharges = Factory.NewJobForTesting<JobHeader>();
			otherHeaderWithCharges.JH_ParentID = Account3.PK;
			otherHeaderWithCharges.JH_GC = otherBranch.GB_GC;
			otherHeaderWithCharges.JH_GB = otherBranch.PK;
			otherHeaderWithCharges.JH_GE = GlbDepartment.CurrentDepartment.PK;
			otherHeaderWithCharges.JH_JobNum = "Job3";
			JobHeader randomDetatchedHeader = Factory.NewJobForTesting<JobHeader>();
			randomDetatchedHeader.JH_ParentID = ZGuid.Empty;
			randomDetatchedHeader.JH_GB = currentBranch.PK;
			randomDetatchedHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;
			randomDetatchedHeader.JH_JobNum = "detatched";
			var invoice1 = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoice1.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice1.AH_Ledger = LedgerTypes.AccountsReceivable;
			invoice1.AH_TransactionType = TransactionTypes.Invoice;
			invoice1.AH_TransactionNum = "00001001";
			invoice1.AH_JH = headerWithCharges.PK;
			var invoiceLine1 = Factory.NewWithValidTestData<AccTransactionLines>();
			invoiceLine1.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			invoiceLine1.AL_AH = invoice1.PK;
			invoiceLine1.AL_JH = headerWithCharges.PK;
			invoiceLine1.AL_LineAmount = 10m;
			var charge = Factory.New<JobCharge>();
			charge.JR_JH = headerWithCharges.PK;
			charge.JR_AC = code.PK;
			charge.JR_GB = currentBranch.PK;
			charge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			charge.JR_AL_ARLine = invoiceLine1.PK;
			charge.SetAmountsFromLinkedLinesForTests();
			var invoice2 = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoice2.AH_GB = otherBranch.PK;
			invoice2.AH_Ledger = LedgerTypes.AccountsReceivable;
			invoice2.AH_TransactionType = TransactionTypes.Invoice;
			invoice2.AH_TransactionNum = "00001002";
			invoice2.AH_JH = otherHeaderWithCharges.PK;
			var invoiceLine2 = Factory.NewWithValidTestData<AccTransactionLines>();
			invoiceLine2.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			invoiceLine2.AL_AH = invoice2.PK;
			invoiceLine2.AL_JH = otherHeaderWithCharges.PK;
			invoiceLine2.AL_LineAmount = 0m;
			invoiceLine2.AL_GB = otherBranch.PK;
			var otherCharge = Factory.New<JobCharge>();
			otherCharge.JR_JH = otherHeaderWithCharges.PK;
			otherCharge.JR_AC = code.PK;
			ExceptionReporterTestListener.Instance.Clear(); // Clear Developer Exception reported because JobCharge and Job belong to different companies
			otherCharge.JR_GB = otherBranch.PK;
			otherCharge.JR_AL_ARLine = invoiceLine2.PK;
			otherCharge.SetAmountsFromLinkedLinesForTests();
			var invoice3 = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoice3.AH_Ledger = LedgerTypes.AccountsReceivable;
			invoice3.AH_TransactionType = TransactionTypes.Invoice;
			invoice3.AH_TransactionNum = "00001003";
			invoice3.AH_JH = randomDetatchedHeader.PK;
			var invoiceLine3 = Factory.NewWithValidTestData<AccTransactionLines>();
			invoiceLine3.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			invoiceLine3.AL_AH = invoice3.PK;
			invoiceLine3.AL_JH = randomDetatchedHeader.PK;
			invoiceLine3.AL_LineAmount = 10m;
			JobCharge randomCharge = Factory.New<JobCharge>();
			randomCharge.JR_JH = randomDetatchedHeader.PK;
			randomCharge.JR_AC = code.PK;
			randomCharge.JR_AL_ARLine = invoiceLine3.PK;
			randomCharge.SetAmountsFromLinkedLinesForTests();
			Factory.Save();
			ModuleFlagsFilter filter = (ModuleFlagsFilter)FilterStrip["Invoiced / Charges"];
			filter["No Charges"] = false;
			AssertMatches("Un-Ticked", filter.Query, Account1, Account2, Account3);
			filter["No Charges"] = true;
			AssertMatches("Ticked", filter.Query, Account1);
		}

		public void TestProfitLossReasonFilterWithOperators()
		{
			var job1 = new JobHeader.Loader(Account1).TryLoadOrCreate();
			job1.JH_ProfitLossReasonCode = "ND1";

			var job2 = new JobHeader.Loader(Account2).TryLoadOrCreate();
			job2.JH_ProfitLossReasonCode = "CD1";

			var job3 = new JobHeader.Loader(Account3).TryLoadOrCreate();
			job3.JH_ProfitLossReasonCode = string.Empty;

			Factory.Save();

			var profitLossReasonFilter = (ModuleTextFilter)FilterStrip["Profit/Loss Reason"];
			profitLossReasonFilter.IsActive = true;

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			profitLossReasonFilter.Property = "ND1";
			AssertMatches("Has a Match for Account1.", profitLossReasonFilter, Account1);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			profitLossReasonFilter.Property = "N";

			AssertMatches("Has a Match for Account1.", profitLossReasonFilter, Account1);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			profitLossReasonFilter.Property = "D";

			AssertMatches("Has a Match for Account1 and Account2", profitLossReasonFilter, Account1, Account2);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			profitLossReasonFilter.Property = "N";

			AssertMatches("Has a Match for Account2 and Account3", profitLossReasonFilter, Account2, Account3);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
			profitLossReasonFilter.Property = "N";

			AssertMatches("Has a Match for Account2 and Account3", profitLossReasonFilter, Account2, Account3);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			profitLossReasonFilter.Property = "ND1";

			AssertMatches("Has a Match for Account2 and Account3", profitLossReasonFilter, Account2, Account3);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			profitLossReasonFilter.Property = "";

			AssertMatches("Has a Match for Account3", profitLossReasonFilter, Account3);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			profitLossReasonFilter.Property = "ND1";

			AssertMatches("Has a Match for Account1 and Account2", profitLossReasonFilter, Account1, Account2);
		}

		#region Billing Filters
		public void TestAPInvoiceNumberFilter()
		{
			JobHeader job = new JobHeader.Loader(Account1).TryLoadOrCreate();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			AccTransactionHeader newInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			newInvoice.AH_GB = GlbBranch.CurrentBranch.PK;
			newInvoice.AH_Ledger = LedgerTypes.AccountsPayable;
			newInvoice.AH_TransactionType = TransactionTypes.Invoice;
			newInvoice.AH_TransactionNum = "00001001";
			AccTransactionLines newInvoiceLine = Factory.NewWithValidTestData<AccTransactionLines>();
			newInvoiceLine.AL_AH = newInvoice.PK;
			newInvoiceLine.AL_JH = job.PK;
			newInvoiceLine.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			Account2.NA_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();
			ModuleNumberFilter filter = (ModuleNumberFilter)FilterStrip[FilterStrip.AccountingFilterStrip.APInvoiceNumber];
			filter.Property = "";
			AssertMatches("Empty Filter", filter.Query, Account1, Account2);
			filter.Property = "00001001";
			AssertMatches("00001001", filter.Query, Account1);
			filter.Property = "00001002";
			AssertMatches("00001002", filter.Query);
		}

		public void TestARTransactionFilter()
		{
			JobHeader job = new JobHeader.Loader(Account1).TryLoadOrCreate();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			AccTransactionHeader newInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			newInvoice.AH_GB = GlbBranch.CurrentBranch.PK;
			newInvoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			newInvoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			newInvoice.AH_TransactionNum = "00001001";
			AccTransactionLines newInvoiceLine = Factory.NewWithValidTestData<AccTransactionLines>();
			newInvoiceLine.AL_AH = newInvoice.PK;
			newInvoiceLine.AL_JH = job.PK;
			newInvoiceLine.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			Account2.NA_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();
			ModuleNumberFilter filter = (ModuleNumberFilter)FilterStrip[FilterStrip.AccountingFilterStrip.ARTransactionNumber];
			filter.Property = "";
			AssertMatches("Empty Filter", filter.Query, Account1, Account2);
			filter.Property = "00001001";
			AssertMatches("00001001", filter.Query, Account1);
			filter.Property = "00001002";
			AssertMatches("00001002", filter.Query);
		}

		#endregion
		#region Implementation
		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheckForCommonTables()
		{
			var result = new List<Tuple<string, string>>();
			result.Add(TableFilter("JobHeader", "Job Local Reference"));
			result.Add(TableFilter("JobHeader", "Job Branch"));
			result.Add(TableFilter("JobHeader", "Job Department"));
			result.Add(TableFilter("JobHeader", "Job Operation Staff"));
			result.Add(TableFilter("JobHeader", "Job Sales Staff"));
			result.Add(TableFilter("JobHeader", "Job Branch Management Code"));
			result.Add(TableFilter("JobHeader", "Invoice Status"));
			result.Add(TableFilter("JobHeader", "AP Invoice #"));
			result.Add(TableFilter("JobHeader", "AR Transaction #"));
			result.Add(TableFilter("JobHeader", "Supplier Cost Reference"));
			result.Add(TableFilter("JobHeader", "Job Cost Amount"));
			result.Add(TableFilter("JobHeader", "Job Accrual Amount"));
			result.Add(TableFilter("JobHeader", "Job Profit Amount"));
			result.Add(TableFilter("JobHeader", "Job Revenue Amount"));
			result.Add(TableFilter("JobHeader", "Job WIP Amount"));
			result.Add(TableFilter("JobHeader", "Job WIP Amount (Excluding Deferred Charges)"));
			result.Add(TableFilter("JobHeader", "Job WIP Amount (Deferred Charges Only)"));
			result.Add(TableFilter("JobHeader", "Job Margin %"));
			result.Add(TableFilter("GlbBranch", "Job Branch Management Code"));
			result.Add(TableFilter("AccTransactionHeader", "AP Invoice #"));
			result.Add(TableFilter("AccTransactionHeader", "AR Transaction #"));
			result.Add(TableFilter("AccTransactionLines", "AP Invoice #"));
			result.Add(TableFilter("AccTransactionLines", "AR Transaction #"));
			result.Add(TableFilter("AccTransactionLines", "Job Cost Amount"));
			result.Add(TableFilter("AccTransactionLines", "Job Accrual Amount"));
			result.Add(TableFilter("AccTransactionLines", "Job Profit Amount"));
			result.Add(TableFilter("AccTransactionLines", "Job Revenue Amount"));
			result.Add(TableFilter("AccTransactionLines", "Job WIP Amount"));
			result.Add(TableFilter("AccTransactionLines", "Job WIP Amount (Excluding Deferred Charges)"));
			result.Add(TableFilter("AccTransactionLines", "Job WIP Amount (Deferred Charges Only)"));
			result.Add(TableFilter("AccTransactionLines", "Job Margin %"));
			result.Add(TableFilter("JobCharge", "Supplier Cost Reference"));
			result.Add(TableFilter("JobCharge", "Job Accrual Amount"));
			result.Add(TableFilter("JobCharge", "Job WIP Amount"));
			result.Add(TableFilter("JobCharge", "Job WIP Amount (Excluding Deferred Charges)"));
			result.Add(TableFilter("JobCharge", "Job WIP Amount (Deferred Charges Only)"));
			result.Add(TableFilter("JobCharge", "Job Margin %"));
			return result;
		}

		VoyageAccount Account1
		{
			get
			{
				CreateIfNessisary(ref account1, ref principal1, ref voyage1, 1);
				return account1;
			}
		}

		OrgHeader Principal1
		{
			get
			{
				CreateIfNessisary(ref account1, ref principal1, ref voyage1, 1);
				return principal1;
			}
		}

		JobVoyage Voyage1
		{
			get
			{
				CreateIfNessisary(ref account1, ref principal1, ref voyage1, 1);
				return voyage1;
			}
		}

		VoyageAccount account1;
		OrgHeader principal1;
		JobVoyage voyage1;
		VoyageAccount Account2
		{
			get
			{
				CreateIfNessisary(ref account2, ref principal2, ref voyage2, 2);
				return account2;
			}
		}

		OrgHeader Principal2
		{
			get
			{
				CreateIfNessisary(ref account2, ref principal2, ref voyage2, 2);
				return principal2;
			}
		}

		JobVoyage Voyage2
		{
			get
			{
				CreateIfNessisary(ref account2, ref principal2, ref voyage2, 2);
				return voyage2;
			}
		}

		VoyageAccount account2;
		OrgHeader principal2;
		JobVoyage voyage2;
		VoyageAccount Account3
		{
			get
			{
				CreateIfNessisary(ref account3, ref principal3, ref voyage3, 3);
				return account3;
			}
		}

		JobVoyage Voyage3
		{
			get
			{
				CreateIfNessisary(ref account3, ref principal3, ref voyage3, 3);
				return voyage3;
			}
		}

		VoyageAccount account3;
		OrgHeader principal3;
		JobVoyage voyage3;
		void CreateIfNessisary(ref VoyageAccount account, ref OrgHeader principal, ref JobVoyage voyage, int index)
		{
			if (principal == null)
			{
				principal = Factory.NewWithValidTestData<OrgHeader>();
				principal.OH_Code = "Principal" + index;
			}

			if (voyage == null)
			{
				var vessel = Factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Name = "Vessel" + index;
				voyage = Factory.New<JobVoyage>();
				voyage.JV_VoyageFlight = "Voyage" + index;
				voyage.JV_RV_NKVessel = vessel.RV_FK;
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";
			}

			if (account == null)
			{
				account = Factory.New<VoyageAccount>();
				account.NA_JobNumber = "Account" + index;
				account.NA_JV = voyage.PK;
				account.NA_OH = principal.PK;
				accountPKList.Add(account.PK);
			}
		}

		readonly List<ZGuid> accountPKList = new List<ZGuid>();
		VoyageAccountingFilterStrip FilterStrip
		{
			get
			{
				if (filterStrip == null)
				{
					filterStrip = new VoyageAccountingFilterStrip();
				}

				return filterStrip;
			}
		}

		VoyageAccountingFilterStrip filterStrip;
		void AssertMatches(string message, ZQuery filter, params VoyageAccount[] expectedVoyageAccounts)
		{
			ZQuery combinedFilter = new ZQuery();
			combinedFilter.AddToFilter(filter);
			combinedFilter.AddToFilter(JobVoyAccountSchema.PK, accountPKList);
			VoyageAccount[] actualVoyageAccounts = Factory.Load<VoyageAccount>(combinedFilter);
			AssertContainsExactElementsInAnyOrder(message, BusinessObjectEqualityComparer<VoyageAccount>.IgnoreFactoryComparer, (v) => v.NA_JobNumber, expectedVoyageAccounts, actualVoyageAccounts);
		}

		void AssertMatches(string message, ModuleFilter filter, params VoyageAccount[] expectedVoyageAccounts)
		{
			var query = filter.Query;
			if (!query.IsEmpty)
			{
				for (var sub = filter.SubGroup; sub != null; sub = sub.Parent)
				{
					query = ((ModuleFilterSubGroup)sub).GetSubQuery(query);
				}
			}

			AssertMatches(message, query, expectedVoyageAccounts);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new VoyageAccountingFilterStrip();
		}
		#endregion
	}
}
