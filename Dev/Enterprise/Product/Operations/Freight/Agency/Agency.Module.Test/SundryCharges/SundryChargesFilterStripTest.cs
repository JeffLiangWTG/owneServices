using System.Collections;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Module.Testing
{
	[TestedType(typeof(SundryChargesFilterStrip))]
	internal class SundryChargesFilterStripTest : FilterStripBusinessObjectTestCase
	{
		public void TestJobNumber()
		{
			Sundry1.D4_JobNumber = "SC00000101";
			Sundry2.D4_JobNumber = "SC00000102";
			Factory.Save();
			ModuleFountainFilter filter = (ModuleFountainFilter)FilterStrip[SundryChargesFilterStrip.Descriptions.JobNumber];
			filter.Property = "";
			Asserter.AssertMatches("Empty", filter, Sundry1, Sundry2);
			filter.Property = "SC00000101";
			Asserter.AssertMatches("Sundry1", filter, Sundry1);
		}

		public void TestFromDate()
		{
			ZDateTime now = ZDateTime.Now;
			Sundry1.D4_FromDate = now.AddDays(-4);
			Sundry2.D4_FromDate = now.AddDays(-3);
			Sundry3.D4_FromDate = now.AddDays(-2);
			Sundry4.D4_FromDate = now.AddDays(-1);
			Factory.Save();
			Asserter.AddFieldOfInterest(JobSundryChargesSchema.Constants.D4_FromDate);
			Asserter.AddFieldOfInterest(JobSundryChargesSchema.Constants.D4_ToDate);
			ModuleDateFilter filter = (ModuleDateFilter)FilterStrip[SundryChargesFilterStrip.Descriptions.FromDate];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = ZDateTime.Empty;
			Asserter.AssertMatches("Empty", filter, Sundry1, Sundry2, Sundry3, Sundry4);
			filter.Property1 = now.AddDays(-3);
			Asserter.AssertMatches("From-", filter, Sundry2, Sundry3, Sundry4);
			filter.Property2 = now.AddDays(-2);
			Asserter.AssertMatches("From-To", filter, Sundry2, Sundry3);
			filter.Property1 = ZDateTime.Empty;
			Asserter.AssertMatches("-To", filter, Sundry1, Sundry2, Sundry3);
		}

		public void TestToDate()
		{
			ZDateTime now = ZDateTime.Now;
			Sundry1.D4_ToDate = now.AddDays(-4);
			Sundry2.D4_ToDate = now.AddDays(-3);
			Sundry3.D4_ToDate = now.AddDays(-2);
			Sundry4.D4_ToDate = now.AddDays(-1);
			Factory.Save();
			Asserter.AddFieldOfInterest(JobSundryChargesSchema.Constants.D4_FromDate);
			Asserter.AddFieldOfInterest(JobSundryChargesSchema.Constants.D4_ToDate);
			ModuleDateFilter filter = (ModuleDateFilter)FilterStrip[SundryChargesFilterStrip.Descriptions.ToDate];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = ZDateTime.Empty;
			Asserter.AssertMatches("Empty", filter, Sundry1, Sundry2, Sundry3, Sundry4);
			filter.Property1 = now.AddDays(-3);
			Asserter.AssertMatches("From-", filter, Sundry2, Sundry3, Sundry4);
			filter.Property2 = now.AddDays(-2);
			Asserter.AssertMatches("From-To", filter, Sundry2, Sundry3);
			filter.Property1 = ZDateTime.Empty;
			Asserter.AssertMatches("-To", filter, Sundry1, Sundry2, Sundry3);
		}

		public void TestBillToParty()
		{
			Sundry1.D4_OH_BillToParty = Org1.PK;
			Sundry2.D4_OH_BillToParty = Org2.PK;
			Factory.Save();
			Asserter.AddFieldOfInterest("BillToParty+OH_Code");
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStrip[SundryChargesFilterStrip.Descriptions.BillToParty];
			filter.Property = ZGuid.Empty;
			Asserter.AssertMatches("Empty", filter, Sundry1, Sundry2);
			filter.Property = Org1.PK;
			Asserter.AssertMatches("Org1", filter, Sundry1);
		}

		public void TestActivity()
		{
			Sundry1.D4_SundryJobActivity = "AC1";
			Sundry2.D4_SundryJobActivity = "AC2";
			Factory.Save();
			Asserter.AddFieldOfInterest(JobSundryChargesSchema.Constants.D4_SundryJobActivity);
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[SundryChargesFilterStrip.Descriptions.Activity];
			filter.Property = ZString.Empty;
			Asserter.AssertMatches("Empty", filter, Sundry1, Sundry2);
			filter.Property = "AC1";
			Asserter.AssertMatches("AC1", filter, Sundry1);
		}

		public void TestMode()
		{
			Sundry1.D4_SundryJobMode = "MD1";
			Sundry2.D4_SundryJobMode = "MD2";
			Factory.Save();
			Asserter.AddFieldOfInterest(JobSundryChargesSchema.Constants.D4_SundryJobMode);
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[SundryChargesFilterStrip.Descriptions.Mode];
			filter.Property = ZString.Empty;
			Asserter.AssertMatches("Empty", filter, Sundry1, Sundry2);
			filter.Property = "MD1";
			Asserter.AssertMatches("AC1", filter, Sundry1);
		}

		public void TestType()
		{
			Sundry1.D4_SundriesJobType = "TP1";
			Sundry2.D4_SundriesJobType = "TP2";
			Factory.Save();
			Asserter.AddFieldOfInterest(JobSundryChargesSchema.Constants.D4_SundriesJobType);
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[SundryChargesFilterStrip.Descriptions.Type];
			filter.Property = ZString.Empty;
			Asserter.AssertMatches("Empty", filter, Sundry1, Sundry2);
			filter.Property = "TP1";
			Asserter.AssertMatches("TP1", filter, Sundry1);
		}

		public void TestDescription()
		{
			Sundry1.D4_SundriesDescription = "Text1";
			Sundry2.D4_SundriesDescription = "Text2";
			Factory.Save();
			Asserter.AddFieldOfInterest(JobSundryChargesSchema.Constants.D4_SundriesDescription);
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip[SundryChargesFilterStrip.Descriptions.Description];
			filter.Property = ZString.Empty;
			Asserter.AssertMatches("Empty", filter, Sundry1, Sundry2);
			filter.Property = "Text1";
			Asserter.AssertMatches("Text1", filter, Sundry1);
		}

		public void TestInvoiceStatusFilter()
		{
			JobHeader job1 = new JobHeader.Loader(Sundry1).TryLoadOrCreate();
			job1.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job1.JH_Status = "WRK";
			JobHeader job2 = new JobHeader.Loader(Sundry2).TryLoadOrCreate();
			job2.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job2.JH_Status = "INV";
			Factory.Save();
			ModuleTextFilter filter = (ModuleTextFilter)FilterStrip["Invoice Status"];
			filter.Property = "";
			Asserter.AssertMatches("Empty Filter", filter, Sundry1, Sundry2);
			filter.Property = "INV";
			Asserter.AssertMatches("INV", filter, Sundry2);
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
			headerWithoutCharges.JH_ParentID = Sundry1.PK;
			headerWithoutCharges.JH_GB = currentBranch.PK;
			headerWithoutCharges.JH_GE = GlbDepartment.CurrentDepartment.PK;
			headerWithoutCharges.JH_JobNum = "Job1";
			JobHeader headerWithCharges = Factory.NewJobForTesting<JobHeader>();
			headerWithCharges.JH_ParentID = Sundry2.PK;
			headerWithCharges.JH_GB = currentBranch.PK;
			headerWithCharges.JH_GE = GlbDepartment.CurrentDepartment.PK;
			headerWithCharges.JH_JobNum = "Job2";
			JobHeader otherHeaderWithCharges = Factory.NewJobForTesting<JobHeader>();
			otherHeaderWithCharges.JH_ParentID = Sundry3.PK;
			otherHeaderWithCharges.JH_GC = otherBranch.GB_GC;
			otherHeaderWithCharges.JH_GB = otherBranch.PK;
			otherHeaderWithCharges.JH_GE = GlbDepartment.CurrentDepartment.PK;
			otherHeaderWithCharges.JH_JobNum = "Job3";
			JobHeader randomDetatchedHeader = Factory.NewJobForTesting<JobHeader>();
			randomDetatchedHeader.JH_ParentID = ZGuid.Empty;
			randomDetatchedHeader.JH_GB = currentBranch.PK;
			randomDetatchedHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;
			randomDetatchedHeader.JH_JobNum = "detatched";
			JobCharge charge = Factory.New<JobCharge>();
			charge.JR_JH = headerWithCharges.PK;
			charge.JR_AC = code.PK;
			charge.JR_GB = currentBranch.PK;
			charge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			charge.JR_LocalSellAmt = 10m;
			JobCharge otherCharge = Factory.New<JobCharge>();
			otherCharge.JR_JH = otherHeaderWithCharges.PK;
			otherCharge.JR_AC = code.PK;
			ExceptionReporterTestListener.Instance.Clear(); // Clear Developer Exception reported because JobCharge and Job belong to different companies
			otherCharge.JR_GB = currentBranch.PK;
			otherCharge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			otherCharge.JR_LocalSellAmt = 0m;
			JobCharge randomCharge = Factory.New<JobCharge>();
			randomCharge.JR_JH = randomDetatchedHeader.PK;
			randomCharge.JR_AC = code.PK;
			randomCharge.JR_GB = otherBranch.PK;
			randomCharge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();
			ModuleFlagsFilter filter = (ModuleFlagsFilter)FilterStrip["Invoiced / Charges"];
			filter["No Charges"] = false;
			Asserter.AssertMatches("Un-Ticked", filter, Sundry1, Sundry2, Sundry3);
			filter["No Charges"] = true;
			Asserter.AssertMatches("Ticked", filter, Sundry1);
		}

		public void TestModuleIds()
		{
			FilterStripBusinessObject fbo = GetNewFilterStripBusinessObject();
			Dictionary<string, IList<string>> result = new Dictionary<string, IList<string>>();
			List<string> errors = new List<string>();
			foreach (ModuleFilter filter in fbo.ModuleFilters)
			{
				ModuleGuidFilter guidFilter;
				ModuleGuidsFilter guidsFilter;
				ModuleNkFilter nkFilter;
				ModuleTextAndNkFilter textNkFilter;
				if ((guidFilter = filter as ModuleGuidFilter) != null)
				{
					CheckForIdMissMatch(errors, guidFilter.ModuleId, "List", guidFilter.List);
				}
				else if ((guidsFilter = filter as ModuleGuidsFilter) != null)
				{
					CheckForIdMissMatch(errors, guidsFilter.ModuleId, "List1", guidsFilter.List1);
					CheckForIdMissMatch(errors, guidsFilter.ModuleId, "List2", guidsFilter.List2);
				}
				else if ((nkFilter = filter as ModuleNkFilter) != null)
				{
					CheckForIdMissMatch(errors, nkFilter.ModuleId, "List", nkFilter.List);
				}
				else if ((textNkFilter = filter as ModuleTextAndNkFilter) != null)
				{
					CheckForIdMissMatch(errors, textNkFilter.ModuleId, "List", textNkFilter.List);
				}

				if (errors.Count > 0)
				{
					result.Add(filter.Description, errors);
					errors = new List<string>();
				}
			}

			AssertGroupedErrorList("These filters have module ids that are incompatible with their lookup collections.", result);
		}

		public void TestProfitLossReasonFilterWithOperators()
		{
			var job1 = new JobHeader.Loader(Sundry1).TryCreate();
			job1.JH_ProfitLossReasonCode = "ND1";

			var job2 = new JobHeader.Loader(Sundry2).TryCreate();
			job2.JH_ProfitLossReasonCode = "CD1";

			var job3 = new JobHeader.Loader(Sundry3).TryCreate();
			job3.JH_ProfitLossReasonCode = string.Empty;

			Factory.Save();

			var profitLossReasonFilter = (ModuleTextFilter)FilterStrip["Profit/Loss Reason"];
			profitLossReasonFilter.IsActive = true;

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			profitLossReasonFilter.Property = "ND1";

			Asserter.AssertMatches("Has a Match for Sundry1.", profitLossReasonFilter, Sundry1);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			profitLossReasonFilter.Property = "N";

			Asserter.AssertMatches("Has a Match for Sundry1.", profitLossReasonFilter, Sundry1);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			profitLossReasonFilter.Property = "D";

			Asserter.AssertMatches("Has a Match for Sundry1 and Sundry2", profitLossReasonFilter, Sundry1, Sundry2);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			profitLossReasonFilter.Property = "N";

			Asserter.AssertMatches("Has a Match for Sundry2 and Sundry3", profitLossReasonFilter, Sundry2, Sundry3);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
			profitLossReasonFilter.Property = "N";

			Asserter.AssertMatches("Has a Match for Sundry2 and Sundry3", profitLossReasonFilter, Sundry2, Sundry3);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			profitLossReasonFilter.Property = "ND1";

			Asserter.AssertMatches("Has a Match for Sundry2 and Sundry3", profitLossReasonFilter, Sundry2, Sundry3);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			profitLossReasonFilter.Property = "";

			Asserter.AssertMatches("Has a Match for Sundry3", profitLossReasonFilter, Sundry3);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			profitLossReasonFilter.Property = "ND1";

			Asserter.AssertMatches("Has a Match for Sundry1 and Sundry2", profitLossReasonFilter, Sundry1, Sundry2);
		}

		#region Billing Filters
		public void TestAPInvoiceNumberFilter()
		{
			JobHeader job = new JobHeader.Loader(Sundry1).TryLoadOrCreate();
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
			Sundry2.D4_SundriesDescription = "Some Description";
			Factory.Save();
			ModuleNumberFilter filter = (ModuleNumberFilter)FilterStrip[FilterStrip.AccountingFilterStrip.APInvoiceNumber];
			filter.Property = "";
			Asserter.AssertMatches("Empty Filter", filter, Sundry1, Sundry2);
			filter.Property = "00001001";
			Asserter.AssertMatches("00001001", filter, Sundry1);
			filter.Property = "00001002";
			Asserter.AssertMatches("00001002", filter);
		}

		public void TestARTransactionFilter()
		{
			JobHeader job = new JobHeader.Loader(Sundry1).TryLoadOrCreate();
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
			Sundry2.D4_SundriesDescription = "Some Description";
			Factory.Save();
			ModuleNumberFilter filter = (ModuleNumberFilter)FilterStrip[FilterStrip.AccountingFilterStrip.ARTransactionNumber];
			filter.Property = "";
			Asserter.AssertMatches("Empty Filter", filter, Sundry1, Sundry2);
			filter.Property = "00001001";
			Asserter.AssertMatches("00001001", filter, Sundry1);
			filter.Property = "00001002";
			Asserter.AssertMatches("00001002", filter);
		}

		#endregion
		#region Implementation
		static void CheckForIdMissMatch(IList<string> errors, ModuleIdentifier id, string listName, IList list)
		{
			ModuleIdentifier actual = ZMetaData.GetModuleId(list);
			if (actual == null || actual.ID != id.ID)
			{
				errors.Add(string.Format("The filter ModuleID ({0}) does not match the {1} ModuleID ({2}).", id, listName, ZMetaData.GetModuleId(list)));
			}
		}

		SundryChargesFilterStrip FilterStrip
		{
			get
			{
				if (filterStrip == null)
				{
					filterStrip = new SundryChargesFilterStrip();
				}

				return filterStrip;
			}
		}

		SundryChargesFilterStrip filterStrip;
		FilterStripAsserter<SundryCharges> Asserter
		{
			get
			{
				return asserter ?? (asserter = new FilterStripAsserter<SundryCharges>(Factory, (s) => s.D4_JobNumber));
			}
		}

		FilterStripAsserter<SundryCharges> asserter;
		OrgHeader Org1
		{
			get
			{
				if (org1 == null)
				{
					org1 = Factory.NewWithValidTestData<OrgHeader>();
					org1.OH_Code = "Org1";
				}

				return org1;
			}
		}

		OrgHeader org1;
		OrgHeader Org2
		{
			get
			{
				if (org2 == null)
				{
					org2 = Factory.NewWithValidTestData<OrgHeader>();
					org1.OH_Code = "Org2";
				}

				return org2;
			}
		}

		OrgHeader org2;
		OrgHeader Org3
		{
			get
			{
				if (org3 == null)
				{
					org3 = Factory.NewWithValidTestData<OrgHeader>();
					org3.OH_Code = "Org3";
				}

				return org3;
			}
		}

		OrgHeader org3;
		OrgHeader Org4
		{
			get
			{
				if (org4 == null)
				{
					org4 = Factory.NewWithValidTestData<OrgHeader>();
					org1.OH_Code = "Org4";
				}

				return org4;
			}
		}

		OrgHeader org4;
		SundryCharges Sundry1
		{
			get
			{
				if (sundry1 == null)
				{
					sundry1 = Factory.New<SundryCharges>();
					sundry1.D4_JobNumber = "Sundry1";
					sundry1.D4_OH_BillToParty = Org1.PK;
					Asserter.AddToScope(sundry1);
				}

				return sundry1;
			}
		}

		SundryCharges sundry1;
		SundryCharges Sundry2
		{
			get
			{
				if (sundry2 == null)
				{
					sundry2 = Factory.New<SundryCharges>();
					sundry2.D4_JobNumber = "Sundry2";
					sundry2.D4_OH_BillToParty = Org2.PK;
					Asserter.AddToScope(sundry2);
				}

				return sundry2;
			}
		}

		SundryCharges sundry2;
		SundryCharges Sundry3
		{
			get
			{
				if (sundry3 == null)
				{
					sundry3 = Factory.New<SundryCharges>();
					sundry3.D4_JobNumber = "Sundry3";
					sundry3.D4_OH_BillToParty = Org3.PK;
					Asserter.AddToScope(sundry3);
				}

				return sundry3;
			}
		}

		SundryCharges sundry3;
		SundryCharges Sundry4
		{
			get
			{
				if (sundry4 == null)
				{
					sundry4 = Factory.New<SundryCharges>();
					sundry4.D4_JobNumber = "Sundry4";
					sundry4.D4_OH_BillToParty = Org4.PK;
					Asserter.AddToScope(sundry4);
				}

				return sundry4;
			}
		}

		SundryCharges sundry4;
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new SundryChargesFilterStrip();
		}
		#endregion
	}
}
