using System;
using System.Drawing;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Customs.GUI
{
	public partial class ZManifestMessageHistoryUserControl : ZUserControl
	{
		protected internal CargoWise.Windows.UI.KPanel TopPanel;
		//private Enterprise.ZArchitecture.ZTextBox zTextBox1;
		protected internal ZArchitecture.ZLabel CRNLabel;
		protected internal ZArchitecture.ZLabel StatusLabel;
		protected internal ZGroupBox HistoryGroupBox;
		protected internal ZGroupBox MessageTextGroupBox;
		protected internal CargoWise.Windows.UI.KPanel HistoryPanel;
		protected internal Messaging.GUI.MessageZGrid MessagesGrid;
		protected internal CargoWise.Windows.UI.KSplitter TheSplitter;
		protected internal CargoWise.Windows.UI.KPanel MessageTextPanel;
		protected internal ZArchitecture.ZTextBox MessageTextTextBox;
		protected internal ZArchitecture.ZTextBox E2_MessageStatusBoundTextBox;
		protected internal ZArchitecture.ZTextBox CustomsEntryNumberTextBox;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private readonly System.ComponentModel.Container components;

		public ZManifestMessageHistoryUserControl()
		{
			InitializeComponent();
			MessagesGrid.ReadOnly = true;
		}

		public ZManifestMessageHistoryUserControl(CustomsManifestStatus manifestStatus) : this()
		{
			this.manifestStatus = manifestStatus;
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			UpdateMessageStatusBoundTextBox4Color();
		}

		private void E2_MessageStatusBoundTextBox4_UpdateColor(object sender, EventArgs e)
		{
			UpdateMessageStatusBoundTextBox4Color();
		}

		protected readonly CustomsManifestStatus manifestStatus;
		void UpdateMessageStatusBoundTextBox4Color()
		{
			ColorPair pair = GetColorsForStatus(E2_MessageStatusBoundTextBox.Text);
			E2_MessageStatusBoundTextBox.BackColor = pair.BackColor;
			E2_MessageStatusBoundTextBox.ForeColor = pair.ForeColor;
		}

		protected virtual ColorPair GetColorsForStatus(string status)
		{
			ColorPair result;

			result.BackColor = Color.LightCoral;
			result.ForeColor = Color.White;

			if (status == ManifestStatus.Cleared.AsString || (manifestStatus != null && manifestStatus.IsClear))
			{
				result.BackColor = Color.ForestGreen;
			}
			else if (status == ManifestStatus.NotSent.AsString || string.IsNullOrEmpty(status))
			{
				result.BackColor = SystemColors.Control;
				result.ForeColor = SystemColors.WindowText;
			}

			return result;
		}

		protected struct ColorPair
		{
			public Color BackColor;
			public Color ForeColor;
		}
	}
}
