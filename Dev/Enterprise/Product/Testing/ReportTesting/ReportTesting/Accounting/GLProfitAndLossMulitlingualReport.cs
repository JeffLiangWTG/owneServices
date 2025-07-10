using System;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ReportTesting.Accounting
{
	using Enterprise.Accounting.Registry.Business;
	using Enterprise.DocumentEngine.RuntimeOptions;
	using NUnit.Framework;

	[TemplateName("GL Profit And Loss Mulitlingual Report")]
	public class TestGLProfitAndLossMulitlingualReport : TemplateTestCase
	{
		[ExpectNoExceptions]
		public void TestReportOnlyRequiredFilters()
		{
			PrepareReportForRender();

			SetMultipleChoiceFilterValue("Language", Constants.Languages.English);

			RunReport();
		}

		[ExpectNoExceptions]
		public void TestReportLocalAccountNumberLengthTakes20Digits()
		{
			try
			{
				ReportOrderCollection reportOrderCollection = new ReportOrderCollection();

				ReportOrder reportOrder = reportOrderCollection.AddNew();
				reportOrder.Language = Constants.Languages.Malay;
				reportOrder.AccountsOrderBeginsWith = reportOrder.AccountOrderTypeList[0].Code;

				AccGLAccountDescriptor descr1 = (AccGLAccountDescriptor)Factory.NewWithValidTestData(typeof(AccGLAccountDescriptor));
				descr1.AJ_LocalAccountNumber = "1000.00.00";
				descr1.AJ_ReportCategory = AccountTypeComboBoxConstants.Header;
				descr1.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
				descr1.AJ_Language = Constants.Languages.Malay;
				descr1.AJ_LocalAccountNumber = "10345678910234567890";

				AccGLAccountDescriptor descr2 = (AccGLAccountDescriptor)Factory.NewWithValidTestData(typeof(AccGLAccountDescriptor));
				descr2.AJ_LocalAccountNumber = "2000.00.00";
				descr2.AJ_ReportCategory = AccountTypeComboBoxConstants.Header;
				descr2.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
				descr2.AJ_Language = Constants.Languages.Malay;
				descr2.AJ_LocalAccountNumber = "11345678910234567890";

				AccGLAccountDescriptor descr3 = (AccGLAccountDescriptor)Factory.NewWithValidTestData(typeof(AccGLAccountDescriptor));
				descr3.AJ_ReportCategory = AccountTypeComboBoxConstants.Consolidation;
				descr3.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
				descr3.AJ_Language = Constants.Languages.Malay;
				descr3.AJ_LocalAccountNumber = "12345678910234567890";

				Factory.Save();

				reportOrder.GLAccountSecondReportStartsFrom = descr2.PK;

				AccountingConfigurationRegistry.Instance.ReportOrder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, reportOrderCollection);

				AccGLAggregate aggr1 = (AccGLAggregate)Factory.NewWithValidTestData(typeof(AccGLAggregate));
				//aggr1.AA_GB = GlbBranch.CurrentBranch.PK;

				Factory.Save();

				PrepareReportForRender();
				SelectAllOptionalTemplates();
				((ISingleAccountingPeriodFieldUnitTestHelper)Report.FilterCollection["Period"]).SinglePeriod = 200606;
				SetMultipleChoiceFilterValue("Language", Constants.Languages.Malay);
				RunReport();
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.ReportOrder.Value.RemoveAndDeleteAll();
			}
		}
	}

	public class TestGLProfitAndLossMulitlingualReportMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Accounting.Module.GLReports(); }
		}

		public override string MenuName
		{
			get { return "Multi-Language Profit and Loss"; }
		}

		public override string Hint
		{
			get
			{
				return

@"The Multi-Language Profit and Loss Report is a local currency report with the selected Local Language General Ledger Account Number setup that lists both the selected accounting period and year to date balances of general ledger revenue and cost accounts.  This report summarizes profit movement within the selected accounting period.
This report documents the financial returns resulting from all business activity and overhead expenditure.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestGLProfitAndLossMulitlingualReport();
		}
	}
}
