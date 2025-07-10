using Enterprise.Accounting.ServiceTasks;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ReportTesting.Accounting
{
	[TemplateName("Job Profit - Global Summary By Job")]
	class GlobalJobProfitSummaryByJobReportTest : TemplateTestCase
	{
		protected override void RunReport()
		{
			//Populating RptDtUnprocessedAccTransactionLines table by reset action (TTP)
			var resetAction = new JCDResetActionStrategy(TestConnection, new TestServiceLogger());
			resetAction.Process();

			var log = new TestServiceLogger();
			var queueTask = new JCDDefaultActionStrategy(TestConnection, log);
			queueTask.Process();

			base.RunReport();
		}
	}

	public class TestGlobalJobProfitSummaryByJobTestMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Accounting.Module.JobCostingReportModule(); }
		}

		public override string MenuName
		{
			get { return "Job Profit - Global Summary By Job"; }
		}

		public override string Hint
		{
			get
			{
				return string.Empty;
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new GlobalJobProfitSummaryByJobReportTest();
		}
	}
}
