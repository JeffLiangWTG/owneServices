namespace Enterprise.ReportTesting.Accounting
{
	using CargoWise.Data;

	[TemplateName("Profit And Loss Report For Reporting book")]
	public class TestProfitAndLossReportForReportingBook : TemplateTestCase
	{
		protected override void PrepareReportForRender()
		{
			base.PrepareReportForRender();
			DbRegistry.BiDataWarehouseServer.SaveValue(Db.ServerName, Db.Connection);
		}
	}
}
