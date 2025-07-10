using System;
using System.Web.UI.HtmlControls;

namespace Enterprise.Tracking.Web.Testing
{
	class ReportFilterControlForTest : ReportFilterControl
	{
		public ReportFilterControlForTest()
		{
			FilterTable = new HtmlTable();
		}

		public void RunReportButton_ClickForTest()
		{
			RunReportButton_Click(this, EventArgs.Empty);
		}
	}
}
