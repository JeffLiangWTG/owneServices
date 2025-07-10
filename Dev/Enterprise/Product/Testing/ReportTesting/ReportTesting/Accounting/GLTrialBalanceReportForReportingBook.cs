namespace Enterprise.ReportTesting.Accounting
{
	using CargoWise.Data;

	[TemplateName("GL Trial Balance Report For Reporting book")]
	public class TestGLTrialBalanceReportForReportingBook : TemplateTestCase
	{
		protected override void PrepareReportForRender()
		{
			base.PrepareReportForRender();
			DbRegistry.BiDataWarehouseServer.SaveValue(Db.ServerName, Db.Connection);
		}
	}
}
