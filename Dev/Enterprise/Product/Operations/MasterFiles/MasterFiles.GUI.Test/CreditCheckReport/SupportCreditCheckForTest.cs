using System;
using System.Collections.Generic;
using WTG.ROPE.Model;

namespace Enterprise.MasterFiles.GUI.Tests
{
	public class SupportCreditCheckForTest : CreditReportUserControl
	{
		internal override IEnumerable<(CreditReportType ReportType, DateTime LastGetReportDate)> PurchasedReports => PurchasedReportsForTest;
		internal IEnumerable<(CreditReportType ReportType, DateTime LastGetReportDate)> PurchasedReportsForTest { get; set; }

		internal override Dictionary<(CreditReportType ReportType, bool IsGet), (bool IsAllowed, string ErrorMessageForNotAllowed)> SecurityCheckpoints => SecurityCheckpointsForTest;
		internal Dictionary<(CreditReportType ReportType, bool IsGet), (bool IsAllowed, string ErrorMessageForNotAllowed)> SecurityCheckpointsForTest { get; set; }
	}
}
