using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.MasterFiles.GUI
{
	#region SuppressResourceStringsCheckRegion

	[TestExcludeZWinFormsAllHaveFormBashers]
	public partial class TraceMonitorForm : ZChildForm
	{
		public TraceMonitorForm(TraceMonitor host)
			: base(host)
		{
			InitializeComponent();
			MessageWriter = new TraceMessageWriter(TextBoxStackTrace);
			SetButtonToggleStartStopInfo();
		}

		void BtnCollect_Click(object sender, EventArgs e)
		{
			if (!TraceMonitorBizo.IsTracing)
			{
				TraceMonitorBizo.MessageWriter = MessageWriter;
				TraceMonitorBizo.IntitializeTraceSources();
				MessageStatusBarPanel.Text = "Tracking";
				MainStatusBar.ForeColor = Color.LimeGreen;
				MainStatusBar.Refresh();
				WriteTrackingStartMessage();
				TraceMonitorBizo.SetReadOnlyIncludingChildren(true);
			}
			else
			{
				MessageStatusBarPanel.Text = "Not Tracking";
				MainStatusBar.ForeColor = Color.Orange;
				MainStatusBar.Refresh();
				WriteTrackingStopMessage();
				TraceMonitorBizo.ClearTraceSources();
				TraceMonitorBizo.SetReadOnlyIncludingChildren(false);
			}

			SetButtonToggleStartStopInfo();
		}

		void BtnClear_Click(object sender, EventArgs e)
		{
			TextBoxStackTrace.Text = string.Empty;
		}

		void TraceForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (TraceMonitorBizo.IsTracing)
			{
				if (Globals.Message.Show(Res.GetString("8352613d-9c5c-4825-aa25-fdf7992062ad", "You have not stopped tracking. Do you want to stop tracking and close the window?"), Res.GetString("baa1d9c5-272d-4285-a367-eea00ec7a10e", "Confirm"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				{
					SetButtonToggleStartStopInfo();
					TraceMonitorBizo.ClearTraceSources();
					TraceMonitorBizo.MessageWriter = null;
				}
				else
				{
					e.Cancel = true;
				}
			}
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		void WriteTrackingStartMessage()
		{
			string message = string.Format(@"Start tracking at {0}", ZDateTime.UtcNow);
			TextBoxStackTrace.AppendText(message);
			TextBoxStackTrace.Refresh();
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		void WriteTrackingStopMessage()
		{
			string message = string.Format($"{System.Environment.NewLine}Tracking stopped at {ZDateTime.UtcNow}{System.Environment.NewLine}");
			TextBoxStackTrace.AppendText(message);
			TextBoxStackTrace.Refresh();
		}

		void SetButtonToggleStartStopInfo()
		{
			ButtonToggleStartStop.Text = TraceMonitorBizo.IsTracing ? "Stop tracking" : "Start tracking";
		}

		TraceMessageWriter MessageWriter { get; }

		TraceMonitor TraceMonitorBizo => BusinessEntity as TraceMonitor;
	}

	#endregion
}
