using System;
using WTG.ROPE.Model;

namespace Enterprise.MasterFiles.GUI
{
	public class ReportItemModel
	{
		ReportItemModel()
		{
		}

		public ReportItemModel(CreditReportType creditReportType)
		{
			CreditReportType = creditReportType;
		}

		public CreditReportType CreditReportType { get; }

		public DateTime? LastReportDate { get; set; }
	}
}
