using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DeniedPartyScreening.GUI
{
	// Turns out .NET Core winforms doesn't correctly support user controls with design-time children

	// Source: https://github.com/dotnet/winforms/issues/4304#issuecomment-1006018945

	// This helper is a substitute

	class TableExpanderHelper
	{
		static readonly Color LinkBlue = Color.FromArgb(18, 112, 154);
		readonly TableLayoutPanel tableLayoutPanel;
		readonly LinkLabel expandCollapseLink;
		readonly ZPanel headerPanel;
		readonly ZLabel expanderDescriptionLabel;
		readonly Action<bool> toggleChanged;

		bool collapsed;

		public TableExpanderHelper(TableLayoutPanel tableLayoutPanel, LinkLabel expandCollapseLink, ZPanel headerPanel, ZLabel expanderDescriptionLabel, Action<bool> toggleChanged)
		{
			this.tableLayoutPanel = tableLayoutPanel;
			this.expandCollapseLink = expandCollapseLink;
			this.headerPanel = headerPanel;
			this.expanderDescriptionLabel = expanderDescriptionLabel;
			this.toggleChanged = toggleChanged;
		}

		void InitStyles(bool enabled)
		{
			if (enabled)
			{
				expandCollapseLink.LinkBehavior = LinkBehavior.NeverUnderline;
				expandCollapseLink.LinkColor = LinkBlue;
			}
			else
			{
				expandCollapseLink.LinkColor = SystemColors.GrayText;
			}
		}

		public void InitBehaviour(bool collapsed, bool enabled)
		{
#if DEBUG
			// Stop the designer from collapsing the controls
			if (DesignModeFinder.IsDesigning)
			{
				return;
			}
#endif

			ExpandCollapse(collapsed);

			var controls = new List<Control>()
			{
				headerPanel
			};

			ClickableUserControlHelper.GetAllChildControls(headerPanel, controls);

			if (enabled)
			{
				foreach (var control in controls)
				{
					control.Cursor = Cursors.Hand;
					control.Click -= HeaderPanel_Click;
					control.Click += HeaderPanel_Click;
				}
			}
			else
			{
				foreach (var control in controls)
				{
					control.Cursor = Cursors.Default;
				}

				expandCollapseLink.Enabled = false;

				if (expanderDescriptionLabel != null)
				{
					expanderDescriptionLabel.Enabled = false;
				}
			}

			InitStyles(enabled);
		}

		RowStyle contentRowStyle;

		void HeaderPanel_Click(object sender, EventArgs e) => ExpandCollapse(!collapsed);

		void ExpandCollapse(bool newCollapsed)
		{
			if (contentRowStyle == null)
			{
				contentRowStyle = tableLayoutPanel.RowStyles[1];
			}

			// Set row height rather than remove/add the panel to prevent the form got freezing when the panel contains a lot of data. (like contains 200+ names)
			if (newCollapsed)
			{
				contentRowStyle.SizeType = SizeType.Absolute;
				contentRowStyle.Height = 0F;
				// Why are there spaces here? They're required otherwise the icon gets truncated when the link is disabled
				// 
				// It's a known issue, see
				// https://github.com/dotnet/winforms/issues/7341
				expandCollapseLink.Text = WinformConstants.ArrowDown;
			}
			else
			{
				contentRowStyle.SizeType = SizeType.Percent;
				contentRowStyle.Height = 100F;
				expandCollapseLink.Text = WinformConstants.ArrowUp;
			}

			collapsed = newCollapsed;
			toggleChanged(collapsed);
		}
	}
}
