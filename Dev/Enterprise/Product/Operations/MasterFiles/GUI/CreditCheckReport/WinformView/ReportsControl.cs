using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ReportsControl : ZUserControl
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1046:Do Not Specify Tooltips Manually Rule", Justification = "The pictureBox is not a button")]
		public ReportsControl()
		{
			InitializeComponent();
			AfterFirstBinding += InitReportItemControls;
			flowLayoutPanel.ControlRemoved += ReportItemRemoved;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (flowLayoutPanel != null)
				{
					flowLayoutPanel.Controls.RemoveAndDisposeAll();
					flowLayoutPanel.Dispose();
				}
			}

			base.Dispose(disposing);
		}

		void InitReportItemControls(object sender, EventArgs e)
		{
			var report = BindingSource.Current as ReportsModel;
			foreach (var reportItemInfo in report.Reports)
			{
				var reportItemControl = new ReportItemControl();
				reportItemControl.SetDataBinding(reportItemInfo, string.Empty);
				reportItemControl.creditEventPictureBox.MouseLeave += (mouseLeaveSender, mouseLeaveE) =>
				{
					report.RestoreEvents();
				};

				flowLayoutPanel.Controls.Add(reportItemControl);
				if (reportItemInfo.GetReportLineVisible)
				{
					var reportItemSplitter = new ReportItemSplitter();
					flowLayoutPanel.Controls.Add(reportItemSplitter);
				}
			}
		}

		void ReportItemRemoved(object sender, ControlEventArgs e)
		{
			var sourceControl = e.Control;
			if (sourceControl != null && !sourceControl.IsDisposed)
			{
				sourceControl.Controls.RemoveAndDisposeAll();
				sourceControl.Dispose();
			}
		}
	}
}
