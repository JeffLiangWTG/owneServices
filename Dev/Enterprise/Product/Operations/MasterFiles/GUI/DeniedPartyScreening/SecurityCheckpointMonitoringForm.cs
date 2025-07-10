using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using CargoWise.Types;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.MasterFiles.GUI
{
	#region SuppressResourceStringsCheckRegion

	[TestExcludeZWinFormsAllHaveFormBashers]
	public partial class SecurityCheckpointMonitoringForm : ZChildForm
	{
		public SecurityCheckpointMonitoringForm()
			: base()
		{
			SecurityCheckpoint.CheckpointChecked += OnCheckpointChecked;
			InitializeComponent();
			SetButtonToggleStartStopInfo();
		}

		readonly HashSet<string> uniqueMessages = new HashSet<string>();

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		protected override void OnClosed(EventArgs e)
		{
			IsTracking = false;
			SecurityCheckpoint.CheckpointChecked -= OnCheckpointChecked;
			base.OnClosed(e);
		}

		void OnCheckpointChecked(SecurityCheckpoint securityCheckpoint, bool isAllowed, bool wasCached)
		{
			if (IsTracking)
			{
				var name = securityCheckpoint.DisplayTextPathToSecurityRight;
				if (string.IsNullOrEmpty(name))
				{
					name = securityCheckpoint.HumanReadableName;
				}
				var grantOrDeny = isAllowed ? "Granted" : "Denied";
				var cachedMsg = ""; //wasCached ? "(cached)" : "";
				WriteMessage(FormattableString.Invariant($@"{name}: {grantOrDeny} {cachedMsg}{System.Environment.NewLine}"));
			}
		}

		public void WriteMessage(string message)
		{
			var textBox = TextBoxStackTrace;
			var button = ButtonClear;
			if (IsTracking)
			{
				try
				{
					textBox.BeginInvoke(new Action(() =>
					{
						textBox.Select(textBox.Text.Length, 0);

						textBox.AppendText(message);
						textBox.Refresh();
						if (uniqueMessages.Contains(message))
						{
							button.Text = "Clear non-unique lines";
						}
						else
						{
							uniqueMessages.Add(message);
						}
					}));
				}
				catch (InvalidAsynchronousStateException) { } //e.g. window was closed before write finished
			}
		}

		void SetButtonToggleStartStopInfo()
		{
			ButtonToggleStartStop.Text = IsTracking ? "Stop tracking" : "Start tracking";
		}

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		public static bool IsTracking { get; set; }
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]

		void BtnCollect_Click(object sender, EventArgs e)
		{
			IsTracking = !IsTracking;

			SetButtonToggleStartStopInfo();

			if (IsTracking)
			{
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
			//algorithm:
			//Clear non-unique lines mode: Keep track of if we've seen a line yet or not, and if we haven't, add it to the new string. Then swap to 'Clear all lines mode'. (We'll swap back if we add a non-unique line again - see WriteMessage.)
			//Clear all lines mode: Clear everything.

			if (ButtonClear.Text == "Clear all lines")
			{
				uniqueMessages.Clear();
				TextBoxStackTrace.Text = "";
				return;
			}

			var orig = TextBoxStackTrace.Text;
			var newLines = "";
			var linesSeen = new HashSet<string>();
			using (StringReader sr = new StringReader(orig))
			{
				string line;
				while ((line = sr.ReadLine()) != null)
				{
					if (!linesSeen.Contains(line))
					{
						linesSeen.Add(line);
						newLines += (line) + System.Environment.NewLine;
					}
				}
			}
			TextBoxStackTrace.Text = newLines;
			ButtonClear.Text = "Clear all lines";
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		void WriteTrackingStartMessage()
		{
			string message = string.Format(@"Start tracking at {0}" + System.Environment.NewLine, ZDateTime.Now);

			TextBoxStackTrace.AppendText(message);
			TextBoxStackTrace.Refresh();
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		void WriteTrackingStopMessage()
		{
			string message = string.Format(@"Tracking stopped at {0}
" + System.Environment.NewLine, ZDateTime.Now);
			TextBoxStackTrace.AppendText(message);
			TextBoxStackTrace.Refresh();
		}
	}

	#endregion
}
