using Enterprise.Accounting.Integration.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Module.Testing
{
#if !WINZOR

	sealed class JobDeclarationFilterBusinessObjectTest_AccountingFilterStripTest : AccountingFilterStripTest<BaseJobDeclaration>
	{
		protected override BaseJobDeclaration GetNewBusinessObjectForFilterCollection()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentID = declaration.PK;
			jobHeader.Company.GC_RX_NKLocalCurrency = "AUD";
			return declaration;
		}

		protected override ModuleIdentifier FilterStripModuleID
		{
			get { return ModuleIDs.Customs.JobDeclaration; }
		}

		protected override bool ShouldUseBillingFilters => false;

		public new void TestAddJobManagementFiltersOnlyIfSecurityIsAllowed()
		{
			Assert("This condition not sutible here.", true);
		}

		public new void TestBranchFilter()
		{
			Assert("There are not Organisation Filters in this module.", true);
		}

		public new void TestJobBranchManagementCodeFilter()
		{
			Assert("There are not Organisation Filters in this module.", true);
		}

		public new void TestDepartmentFilter()
		{
			Assert("There are not Organisation Filters in this module.", true);
		}

		public new void TestOperationStaffFilter()
		{
			Assert("There are not Organisation Filters in this module.", true);
		}

		public new void TestSalesStaffFilter()
		{
			Assert("There are not Organisation Filters in this module.", true);
		}

		public new void TestTaxBranchFilter()
		{
			Assert("There are not Organisation Filters in this module.", true);
		}

		public new void TestTaxBranchFilterExist()
		{
			Assert("There are not Organisation Filters in this module.", true);
		}

		public new void TestJobOpenDateFilter()
		{
			Assert("There are not Date Filters in this module.", true);
		}

		public new void TestJobCloseDateFilter()
		{
			Assert("There are not Date Filters in this module.", true);
		}

		public new void TestJobRevenueRecognitionDateFilter()
		{
			Assert("There are not Date Filters in this module.", true);
		}

		public new void TestJobOpenOrCloseDateFilter()
		{
			Assert("There are not Date Filters in this module.", true);
		}

		public new void TestLocalJobReferenceFilter()
		{
			Assert("There are not Numbers And References Filters in this module.", true);
		}
	}

#endif
}
