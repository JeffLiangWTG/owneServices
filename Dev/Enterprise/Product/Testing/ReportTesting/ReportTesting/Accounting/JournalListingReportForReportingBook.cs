using CargoWise.Data;

namespace Enterprise.ReportTesting.Accounting
{
	[TemplateName("Journal Listing Report For Reportingbook")]
	public class TestJournalListingReportForReportingBook : TemplateTestCase
	{
		protected override void PrepareReportForRender()
		{
			base.PrepareReportForRender();
			DbRegistry.BiDataWarehouseServer.SaveValue(Db.ServerName, Db.Connection);
		}
	}
}
