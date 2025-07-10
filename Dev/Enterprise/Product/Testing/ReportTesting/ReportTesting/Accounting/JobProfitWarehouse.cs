namespace Enterprise.ReportTesting.Accounting
{
	using CargoWise.Application;
	using Enterprise.ReportTesting;
	using Enterprise.Warehouse.Integration;

	[TemplateName("Job Profit - Warehouse")]
	public class TestJobProfitWarehouse : TemplateTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var warehouse = helper.CreateWarehouse("WH1");
			Factory.Save();
		}

		public override bool ReportUsesGeneratedSQL
		{
			get
			{
				return true;
			}
		}
	}

	public class TestJobProfitWarehouseMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Accounting.Module.JobCostingReportModule(); }
		}

		public override string MenuName
		{
			get { return "Job Profit - Warehouse"; }
		}

		public override string Hint
		{
			get
			{
				return

								@"This report specifically targets the analysis of Job Profits for Warehouse jobs. It supports an extensive range of freight and invoicing job filters as well as three layout options.

The layout style selected when running the report determines the type of analysis and level of detail included in the report. Using this report you can now produce reports that analyze Revenue, WIP, Costs, Accruals and Job Profit by:

· Charge Code for each Job
· Totals for each Job
· Transaction Details by Job";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestJobProfitWarehouse();
		}
	}
}
