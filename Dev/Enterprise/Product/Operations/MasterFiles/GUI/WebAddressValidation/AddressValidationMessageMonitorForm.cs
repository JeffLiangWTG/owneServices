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
	public partial class AddressValidationMessageMonitorForm : ZChildForm
	{
		public AddressValidationMessageMonitorForm()
			: base()
		{
			InitializeComponent();
			MessageWriter = new MessageWriter(TextBoxStackTrace);
			SetButtonToggleStartStopInfo();
		}

		void SetButtonToggleStartStopInfo()
		{
			ButtonToggleStartStop.Text = IsTrackingMessage ? "Stop tracking messages" : "Start tracking address validation messages";
		}

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		public static bool IsTrackingMessage { get; set; }
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		public static MessageWriter MessageWriter { get; set; }

		void BtnCollect_Click(object sender, EventArgs e)
		{
			IsTrackingMessage = !IsTrackingMessage;

			SetButtonToggleStartStopInfo();

			if (IsTrackingMessage)
			{
				AddressValidationService.MessageWriter = MessageWriter;
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
			var uris = AddressValidationServiceUrisProvider.GetAddressValidationServiceUris();
			string message = string.Format(@"Start tracking address validation messages at {0}
Address validation web service Primary URI: {1}
Address validation web service Secondary URI: {2}
" + System.Environment.NewLine, ZDateTime.Now, uris.Primary.Uri, uris.Secondary.Uri);

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

	public class MessageWriter : IMessageWriter
	{
		public MessageWriter(TextBox textBox)
		{
			this.textBox = textBox;
		}

		public void WriteMessage(string message)
		{
			if (AddressValidationMessageMonitorForm.IsTrackingMessage)
			{
				textBox.AppendText(message);
				textBox.Refresh();
			}
		}

		TextBox textBox { get; set; }
	}

	#endregion
}
