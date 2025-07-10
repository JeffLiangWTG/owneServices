namespace Enterprise.ReportTesting.Accounting
{
	using CargoWise.EntityFramework;
	using Enterprise.Accounting.Business;
	using Enterprise.DocumentEngine.RuntimeOptions;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ReportTesting;
	using NUnit.Framework;

	[TemplateName("Forwarding Job Profit Analysis")]
	public class TestForwardingJobProfitAnalysisReportTest : TemplateTestCase
	{
		[SnailTest]
		[ExpectNoExceptions]
		[StressTest]
		[TestDate(2006, 12, 25)]
		public new void TestReportRunsWithNoException()
			=> base.TestReportRunsWithNoException();

		[SnailTest]
		[ExpectNoExceptions]
		[TestDate(2006, 12, 25)]
		public new void TestEnsureAllOptionalTemplatesRunWithAllFiltersSpecified()
			=> base.TestEnsureAllOptionalTemplatesRunWithAllFiltersSpecified();

		[ExpectNoExceptions]
		[TestDate(2006, 12, 25)]
		public void TestEnsureAcceptableCompileMemory()
			=> TestEnsureAcceptableCompileMemory(500000L);

		protected override void ApplyNonClearableFiltersValues()
		{
			RestrictFilter();
		}

		protected override void SelectAllOptionalTemplates()
		{
			RestrictFilter();
		}

		void RestrictFilter()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			((DateRangeField)Report.FilterCollection["Job Opened"]).ValueLow = CargoWise.Types.ZDateTime.Today.AddDays(-1);
			((DateRangeField)Report.FilterCollection["Job Opened"]).ValueHigh = CargoWise.Types.ZDateTime.Today.AddDays(1);
			((DateRangeField)Report.FilterCollection["Transaction Recognized"]).ValueLow = CargoWise.Types.ZDateTime.Today.AddDays(-1);
			((DateRangeField)Report.FilterCollection["Transaction Recognized"]).ValueHigh = CargoWise.Types.ZDateTime.Today.AddDays(1);

			var dummySaleGroup = testObjectCreator.CreateSalesGroup("Dummy");
			LimitAccGroup((LookupField)Report.FilterCollection["Sales Group"], dummySaleGroup);
			LimitAccGroup((LookupField)Report.FilterCollection["Expense Group"], dummySaleGroup);

			LimitGateway((MultipleChoice)Report.FilterCollection["Include Gateway"]);

			void LimitAccGroup(LookupField accGroupsLookups, AccGroups dummyValue)
			{
				accGroupsLookups.ClearValues();
				accGroupsLookups.BindToList.Add(dummyValue);
				accGroupsLookups.ValueAsStringForSerialisation = dummyValue.AR_Code;
			}

			void LimitGateway(MultipleChoice gatewayList)
			{
				gatewayList.ClearValues();
				gatewayList.List.AddPair("DUM", "Dummy Consol Type");
				gatewayList.ValueAsStringForSerialisation = "DUM";
			}
		}
	}

	public class TestForwardingJobProfitAnalysisReportTestMenuSetup : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Accounting.Module.JobCostingReportModule(); }
		}

		public override string MenuName
		{
			get { return "Job Profit – Forwarding and Customs Transaction Detail"; }
		}

		public override string Hint
		{
			get
			{
				return
@"The Job Profit – Forwarding and Customs Transaction Detail report can be used to itemize Revenue, WIP, Costs, Accruals and Profit movements  on Forwarding and Customs jobs.  
This menu supports a wide range of freight Job filters (e.g. transport mode, ETA/ETD, carrier, agent).
Other filter options include branch, department, local client, sales rep, transaction posted dates.
This report is a detail report.  It itemizes each recognized transaction line on a job.
Options include analysis by consol or job at a summary, charge code or transaction detail level.
You can limit the report to only including transactions and profit movements recognized in a specific date range.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestForwardingJobProfitAnalysisReportTest();
		}
	}
}
