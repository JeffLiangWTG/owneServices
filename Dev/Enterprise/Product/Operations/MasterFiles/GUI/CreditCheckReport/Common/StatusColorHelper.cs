using System.Drawing;

namespace Enterprise.MasterFiles.GUI
{
	public static class StatusColorHelper
	{
		public static Color GetStatusColor(CreditReportStatusType creditReportStatus)
		{
			switch (creditReportStatus)
			{
				case CreditReportStatusType.UpToDate:
					return Color.Green;
				case CreditReportStatusType.Warning:
					return Color.Red;
				default:
					return Color.Empty;
			}
		}
	}
}
