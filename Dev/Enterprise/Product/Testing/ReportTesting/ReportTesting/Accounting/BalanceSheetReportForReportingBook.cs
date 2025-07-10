namespace Enterprise.ReportTesting.Accounting;

using CargoWise.Data;

[TemplateName("Balance Sheet Report For Reporting book")]
public class TestBalanceSheetReportForReportingBook : TemplateTestCase
{
	protected override void PrepareReportForRender()
	{
		base.PrepareReportForRender();
		DbRegistry.BiDataWarehouseServer.SaveValue(Db.ServerName, Db.Connection);
	}
}
