using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ReportTesting.Accounting
{
	using System.Collections.Generic;
	using Enterprise.Accounting.Module;
	using Enterprise.DocumentEngine.RuntimeOptions;
	using NUnit.Framework;

	[TemplateName("GL Transaction Report")]
	public class TestGLTransactionReport : TemplateTestCase
	{
		protected override List<string> GetExcludedSheetNames()
		{
			return new List<string> { "GLTransactionList" };
		}

		protected override void FillReportWithDefaultValues()
		{
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccGLAggregateSchema.Constants.TableName);

			NewYearPeriodSettings testPeriodSettings = new NewYearPeriodSettings();
			testPeriodSettings.StartDate = new ZDateTime(2001, 1, 1);
			PeriodManager testManager = new PeriodManager(TestFactory);
			testManager.CreatePeriodData(testPeriodSettings, TestFactory);
			TestFactory.Save();

			base.FillReportWithDefaultValues();
		}

		protected override void PrepareReportForRender()
		{
			base.PrepareReportForRender();
			((AccountingPeriodRangeSerialiser)Report.FilterCollection["Period Range"]).PeriodFrom = 200101;
			((AccountingPeriodRangeSerialiser)Report.FilterCollection["Period Range"]).PeriodTo = 200101;
		}

		BusinessObjectFactory TestFactory
		{
			get
			{
				if (fBusinessObjectFactory == null)
				{
					fBusinessObjectFactory = new BusinessObjectFactory();
				}
				return fBusinessObjectFactory;
			}
		}
		BusinessObjectFactory fBusinessObjectFactory;

		public override bool ReportUsesGeneratedSQL
		{
			get
			{
				return true;
			}
		}
	}

	public class TestGLTransactionReportMenuSetup : Enterprise.ReportTesting.ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new GLReports(); }
		}

		public override string MenuName
		{
			get { return "Transactions"; }
		}

		public override string Hint
		{
			get
			{
				return

@"The Transactions Report is a detailed report for each general ledger account that shows financial transactions recorded against each accounting period. The report includes opening and closing balances of each account, and can be run for selected account ranges and accounting periods.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestGLTransactionReport();
		}
	}
}
