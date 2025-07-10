using System.Linq;
using Enterprise.Accounting.Business;

namespace Enterprise.ReportTesting.Accounting
{
	using CargoWise.Data;
	using Enterprise.DocumentEngine.RuntimeOptions;
	using Enterprise.MasterFiles.Business;

	[TemplateName("GL Transaction Report For Reportingbook")]
	public class TestGLTransactionReportForReportingBook : TemplateTestCase
	{
		protected override void PrepareReportForRender()
		{
			base.PrepareReportForRender();
			DbRegistry.BiDataWarehouseServer.SaveValue(Db.ServerName, Db.Connection);
		}

		protected override void FillReportWithDefaultValues()
		{
			var filterFields = Report.FilterCollection
				.ToArray()
				.OfType<FilterFieldWithUTSupport>();

			foreach (var field in filterFields)
			{
				if (!field.DisplayName.Contains("GL Account"))
				{
					field.FillWithValidTestData();
					field.Factory.Save();
				}
			}

			var creator = new TestObjectCreator(Factory);
			var chart = creator.CreateAlternateChart("MGT", "Management Reporting", false, true, AccAlternateChartLookups.BalanceSheetStyleCode.ELA);
			creator.CreateAccAlternateChartFormat(chart, 1, "X", "2", ".");

			var gLHeader1 = creator.CreateAccGLHeader("10.00.1010", AccGLHeader.Constants.SectionTypes.Codes.TradingStatement, "desc", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Credit);
			Factory.Save();
			var alternateGlAccount1 = creator.CreateAccAlternateGlAccount(chart.PK, "10.00.1100", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit,
				0, AccGLHeader.Constants.SectionTypes.Codes.TradingStatement, 0, "U");
			var alternateGlAccount2 = creator.CreateAccAlternateGlAccount(chart.PK, "10.00.1110", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit,
				0, AccGLHeader.Constants.SectionTypes.Codes.TradingStatement, 0, "U");

			Factory.Save();

			creator.CreateAccAlternateGlAccountAttribute(alternateGlAccount1, gLHeader1.PK, 1, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG, "OCG");
			creator.CreateAccAlternateGlAccountAttribute(alternateGlAccount2, gLHeader1.PK, 2, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG, "OCG");
			creator.CreateReportingBook("iii", "desc", chart.PK, "");
			Factory.Save();
		}
	}
}
