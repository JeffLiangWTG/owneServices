using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Module.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	public abstract class ExtendedDocketFilterBusinessObjectTest<TDocketFilter, TDocket> : DocketFilterBusinessObjectTest<TDocketFilter, TDocket>
		where TDocketFilter : ExtendedDocketFilterBusinessObject, new()
		where TDocket : WhsDocket
	{
		#region TestGetCarrierServiceLevels

		public void TestGetCarrierServiceLevels()
		{
			if (SupportsTransportCoFilters)
			{
				var transportCo = Factory.NewWithValidTestData<OrgHeader>();
				var serviceLevel1 = transportCo.MiscServ.CarrierServiceLevels.AddNew();
				serviceLevel1.PL_Code = "SV1";
				serviceLevel1.PL_CarrierServiceLevelDescription = "Service 1";
				var serviceLevel2 = transportCo.MiscServ.CarrierServiceLevels.AddNew();
				serviceLevel2.PL_Code = "SV2";
				serviceLevel2.PL_CarrierServiceLevelDescription = "Service ";
				var serviceLevel3 = transportCo.MiscServ.CarrierServiceLevels.AddNew();
				serviceLevel3.PL_Code = "SV3";
				serviceLevel3.PL_CarrierServiceLevelDescription = "Service 3";

				Factory.Save();

				FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Transport Co", transportCo.PK);
				var filterCarrierServiceLevels = DocketFilter.GetCarrierServiceLevels();
				AssertEquals("STD service is always added to the collection", 4, filterCarrierServiceLevels.Count);
			}
			else
			{
				Assert("FilterBusinessObject does not support Transport Company Filters.", true);
			}
		}

		#endregion

		#region Billing Filters

		public void TestJobInvoicingStatusFilter()
		{
			SetupTestData();
			var jobstatusFilter = DocketFilter["Invoicing Job Status"];

			if (SupportsAccountingFilters)
			{
				if (jobstatusFilter == null)
				{
					Assert(true);
				}
				else
				{
					var job = new JobHeader.Loader(Docket11).TryLoadOrCreateWithoutMutexForTestOnly();
					AssertEquals(JobHeaderStatus.Working.Code, job.JH_Status);

					Factory.Save();

					var filterBO = new TDocketFilter();
					var filter = (ModuleTextBaseFilter)filterBO["Invoicing Job Status"];
					filter.Property = JobHeaderStatus.Working.Code;
					filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
					filter.IsActive = true;

					DocketCollection.AdditionalFilter = filterBO.Filter;
					AssertCollectionContains(Docket11, DocketCollection);

					filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
					DocketCollection.AdditionalFilter = filterBO.Filter;

					AssertCollectionNotContains(Docket11, DocketCollection);
				}
			}
			else
			{
				AssertNull(jobstatusFilter);
			}
		}

		public void TestAPInvoiceNumberFilter()
		{
			SetupTestData();

			if (SupportsAccountingFilters)
			{
				AssertNotNull(DocketFilter["AP Invoice #"]);

				var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				job.JH_ParentID = Docket11.PK;
				job.JH_ParentTableCode = WhsDocketSchema.Constants.Prefix;
				job.JH_GB = GlbBranch.CurrentBranch.PK;
				job.JH_GC = GlbCompany.CurrentCompany.PK;
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				job.Parent = Docket11;

				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_Code = "00001001";

				var newInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
				newInvoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsPayable;
				newInvoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
				newInvoice.AH_OH = org.PK;
				newInvoice.AH_TransactionNum = "00001001";
				newInvoice.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				newInvoice.AH_GB = GlbBranch.CurrentBranch.PK;
				newInvoice.AH_ConsolidatedInvoiceRef = "S00009999";

				var newInvoiceLine = Factory.NewWithValidTestData<AccTransactionLines>();
				newInvoiceLine.AL_AH = newInvoice.PK;
				newInvoiceLine.AL_JH = job.PK;
				newInvoiceLine.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;

				Factory.Save();

				FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "AP Invoice #", (ZString)"00001001");
				DocketAssert(true, false, false, false);

				FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "AP Invoice #", (ZString)"00001002");
				DocketAssert(false, false, false, false);

				FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "AP Invoice #", (ZString)"");
				DocketAssert(true, true, true, true);
			}
			else
			{
				AssertNull(DocketFilter["AP Invoice #"]);
			}
		}

		public void TestARTransactionFilter()
		{
			SetupTestData();

			if (SupportsAccountingFilters)
			{
				AssertNotNull(DocketFilter["AR Transaction #"]);

				var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				job.JH_ParentID = Docket11.PK;
				job.JH_ParentTableCode = WhsDocketSchema.Constants.Prefix;
				job.JH_GB = GlbBranch.CurrentBranch.PK;
				job.JH_GC = GlbCompany.CurrentCompany.PK;
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				job.Parent = Docket11;

				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_Code = "00001001";

				var newInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
				newInvoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
				newInvoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
				newInvoice.AH_OH = org.PK;
				newInvoice.AH_TransactionNum = "00001001";
				newInvoice.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				newInvoice.AH_GB = GlbBranch.CurrentBranch.PK;
				newInvoice.AH_ConsolidatedInvoiceRef = "S00009999";

				var newInvoiceLine = Factory.NewWithValidTestData<AccTransactionLines>();
				newInvoiceLine.AL_AH = newInvoice.PK;
				newInvoiceLine.AL_JH = job.PK;
				newInvoiceLine.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;

				Factory.Save();

				FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "AR Transaction #", (ZString)"00001001");
				DocketAssert(true, false, false, false);

				FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "AR Transaction #", (ZString)"00001002");
				DocketAssert(false, false, false, false);

				FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "AR Transaction #", (ZString)"");
				DocketAssert(true, true, true, true);
			}
			else
			{
				AssertNull(DocketFilter["AR Transaction #"]);
			}
		}

		public void TestProfitLossReasonFilterWithOperators()
		{
			SetupTestData();

			if (SupportsAccountingFilters)
			{
				var job1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				job1.JH_ParentID = Docket11.PK;

				var job2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				job2.JH_ParentID = Docket12.PK;

				var job3 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				job3.JH_ParentID = Docket21.PK;

				var job4 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				job4.JH_ParentID = Docket22.PK;

				job1.JH_ProfitLossReasonCode = "ND1";
				job2.JH_ProfitLossReasonCode = "CD1";
				job3.JH_ProfitLossReasonCode = string.Empty;
				Factory.Save();

				var filterBO = new TDocketFilter();
				var profitLossReasonFilter = (ModuleTextBaseFilter)filterBO["Profit/Loss Reason"];
				profitLossReasonFilter.IsActive = true;

				profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				profitLossReasonFilter.Property = "ND1";
				DocketCollection.AdditionalFilter = filterBO.Filter;

				AssertContainsExactElementsInAnyOrder(new[] { Docket11 }, DocketCollection);

				profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
				profitLossReasonFilter.Property = "N";
				DocketCollection.AdditionalFilter = filterBO.Filter;

				AssertContainsExactElementsInAnyOrder(new[] { Docket11 }, DocketCollection);

				profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
				profitLossReasonFilter.Property = "D";
				DocketCollection.AdditionalFilter = filterBO.Filter;

				AssertContainsExactElementsInAnyOrder(new[] { Docket11, Docket12 }, DocketCollection);

				profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
				profitLossReasonFilter.Property = "N";
				DocketCollection.AdditionalFilter = filterBO.Filter;

				AssertContainsExactElementsInAnyOrder(new[] { Docket12, Docket21, Docket22 }, DocketCollection);

				profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
				profitLossReasonFilter.Property = "N";
				DocketCollection.AdditionalFilter = filterBO.Filter;

				AssertContainsExactElementsInAnyOrder(new[] { Docket12, Docket21, Docket22 }, DocketCollection);

				profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
				profitLossReasonFilter.Property = "ND1";
				DocketCollection.AdditionalFilter = filterBO.Filter;

				AssertContainsExactElementsInAnyOrder(new[] { Docket12, Docket21, Docket22 }, DocketCollection);

				profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
				profitLossReasonFilter.Property = "ND1";
				DocketCollection.AdditionalFilter = filterBO.Filter;

				AssertContainsExactElementsInAnyOrder(new[] { Docket21, Docket22 }, DocketCollection);

				profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
				profitLossReasonFilter.Property = "ND1";
				DocketCollection.AdditionalFilter = filterBO.Filter;

				DocketCollection.AdditionalFilter = filterBO.Filter;
				AssertContainsExactElementsInAnyOrder(new[] { Docket11, Docket12 }, DocketCollection);
			}
			else
			{
				AssertNull(DocketFilter["Profit/Loss Reason"]);
			}
		}

		protected virtual bool SupportsAccountingFilters => true;

		#endregion

		#region Test Filter Max Length

		public override void TestFilterMaxLength()
		{
			base.TestFilterMaxLength();
			if (SupportsTransportCoFilters)
			{
				var transportCompMaxLength = Math.Min(OrgHeaderSchema.OH_FullName.MaxLength, JobDocAddressSchema.E2_CompanyName.MaxLength);
				AssertEquals("MaxLength of Transport Company Name should be set correctly.", transportCompMaxLength, FilterStripBizO["Transport Company Name"].MaxLength);
			}
		}

		#endregion

		#region Docket Statuses

		protected override CodeDescriptionPairList GetExpectedDocketStatus()
		{
			var status = new DocketStatus();
			status.RemoveAt(status.IndexOfCode(DocketStatus.Codes.New));
			status.AddPair(StatusFilterTypes.UnfinalisedFilterType, "Un-finalized");
			return status;
		}

		#endregion

	}
}
