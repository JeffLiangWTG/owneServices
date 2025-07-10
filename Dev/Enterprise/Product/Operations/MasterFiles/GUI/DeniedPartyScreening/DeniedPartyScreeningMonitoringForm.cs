using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core.Diagnostics;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.MasterFiles.GUI
{
	#region SuppressResourceStringsCheckRegion

	[TestExcludeZWinFormsAllHaveFormBashers]
	public partial class DeniedPartyScreeningMonitoringForm : ZChildForm
	{
		public DeniedPartyScreeningMonitoringForm()
		{
			InitializeComponent();
			MessageWriter = new DeniedPartyScreeningMessageWriter(TextBoxStackTrace);
			SetButtonToggleStartStopInfo();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (IsTrackingMessage && DeniedPartyScreenerAsync.MessageWriter != MessageWriter)
			{
				DeniedPartyScreenerAsync.MessageWriter = MessageWriter;
			}
		}

		void SetButtonToggleStartStopInfo()
		{
			ButtonToggleStartStop.Text = IsTrackingMessage ? "Stop tracking messages" : "Start tracking Denied Party Screening";
		}

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		public static bool IsTrackingMessage { get; private set; }

		public DeniedPartyScreeningMessageWriter MessageWriter { get; }

		void BtnCollect_Click(object sender, EventArgs e)
		{
			IsTrackingMessage = !IsTrackingMessage;

			SetButtonToggleStartStopInfo();

			if (IsTrackingMessage)
			{
				DeniedPartyScreenerAsync.MessageWriter = MessageWriter;
				MessageStatusBarPanel.Text = "Tracking";
				MainStatusBar.ForeColor = Color.LimeGreen;
				MainStatusBar.Refresh();
				WriteTrackingStartMessage();
			}
			else
			{
				MessageStatusBarPanel.Text = "Not Tracking";
				MainStatusBar.ForeColor = Color.Orange;
				MainStatusBar.Refresh();
				WriteTrackingStopMessage();
			}
		}

		void BtnClear_Click(object sender, EventArgs e)
		{
			TextBoxStackTrace.Text = string.Empty;
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		void WriteTrackingStartMessage()
		{
			var message = string.Format("Start tracking Denied Party Screening at {0}" + System.Environment.NewLine, ZDateTime.Now);

			TextBoxStackTrace.AppendText(message);
			TextBoxStackTrace.Refresh();
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		void WriteTrackingStopMessage()
		{
			var message = string.Format("Tracking stopped at {0}" + System.Environment.NewLine, ZDateTime.Now);

			TextBoxStackTrace.AppendText(message);
			TextBoxStackTrace.Refresh();
		}
	}

	public class DeniedPartyScreeningMessageWriter : IMessageWriter
	{
		public DeniedPartyScreeningMessageWriter(TextBox textBox)
		{
			this.textBox = textBox;
		}

		public void WriteMessage(string message)
		{
			if (DeniedPartyScreeningMonitoringForm.IsTrackingMessage)
			{
				textBox.Select(textBox.Text.Length, 0);

				textBox.AppendText(message);
				textBox.Refresh();
			}
		}

		readonly TextBox textBox;
	}

	#endregion
}
