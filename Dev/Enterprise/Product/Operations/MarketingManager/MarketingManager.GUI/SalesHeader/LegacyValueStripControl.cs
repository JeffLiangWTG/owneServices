using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MarketingManager.GUI
{
	public partial class LegacyValueStripControl : ZUserControl, IReadOnlyToggleControl, ISalesHeaderStripControl
	{
		public LegacyValueStripControl()
		{
			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
				SetupDeleteButton();

				var oldFont = salesProductNameLabel.Font;
				salesProductNameLabel.Font = new Font(oldFont.FontFamily, oldFont.Size * 1.5f, oldFont.Style);
				salesProductNameLabel.Text = Res.GetString("142cfa20-373d-4e7d-9ecf-9d66eee7acde", "Legacy Value Analysis");
				RefreshCollapsedControls();
				AddTopRightPanelFocusEventHandlers();
			}
		}

		#region CurrentDataItem

		public new OrgOpportunity CurrentDataItem
		{
			get { return (OrgOpportunity)base.CurrentDataItem; }
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			if (CurrentDataItem != null && valueAnalysisControl == null)
			{
				valueAnalysisControl = OpportunityManagementValueAnalysisControl.New();
				valueAnalysisControl.Dock = DockStyle.Top;
				valueAnalysisControl.SetDataBinding(CurrentDataItem, "");
				this.bottomRightPanel.Controls.Add(valueAnalysisControl);
			}
		}
		OpportunityManagementValueAnalysisControl valueAnalysisControl;

		#endregion

		#region Properties

		#region IsMandatory

		[DefaultValue(false)]
		public bool IsMandatory
		{
			get { return isMandatory; }
			set
			{
				isMandatory = value;
				RefreshDeleteButtonVisibility();
			}
		}
		bool isMandatory;

		#endregion

		#region Collapsed

		[DefaultValue(true)]
		public bool Collapsed
		{
			get { return collapsed; }
			set
			{
				collapsed = value;
				RefreshCollapsedControls();
			}
		}
		bool collapsed = true;

		void RefreshCollapsedControls()
		{
			SuspendLayout();
			try
			{
				topRightPanel.BackColor = Collapsed ? Color.White : headerExpandedColor;
				salesProductNameLabel.ForeColor = Collapsed ? Color.Black : Color.White;
			}
			finally
			{
				ResumeLayout();
			}
		}

		readonly Color headerExpandedColor = Color.FromArgb(0, 168, 225);

		#endregion

		#endregion

		#region Delete Button

		internal ZButton DeleteButton
		{
			get { return deleteButton; }
		}

		void SetupDeleteButton()
		{
			deleteButton.FlatStyle = FlatStyle.Flat;
			deleteButton.BackgroundImage = Icons.GetImage(IconTypes.DeleteButtonRest);
		}

		void RefreshDeleteButtonVisibility()
		{
			DeleteButton.Visible = !ReadOnly && !IsMandatory;
		}

		void DeleteButton_Click(object sender, EventArgs e)
		{
			if (CurrentDataItem != null)
			{
				var dialogResult = Globals.Message.Show(
						Res.GetString("2e281cee-fa05-4bce-bde8-212047d4f005", "Are you sure you want to delete all estimate values for Legacy Value Analysis?"),
						Res.GetString("b0e62c2f-1cca-47d8-bd01-37257d3b9919", "Delete all Legacy Estimate Values"),
						MessageBoxButtons.YesNo,
						DialogResult.No);

				if (dialogResult == DialogResult.Yes)
				{
					CurrentDataItem.ValueItems.RemoveAndDeleteAll();
					Parent.Controls.Remove(this);
					Dispose();
				}
			}
		}

		#endregion

		#region ReadOnly

		[DefaultValue(false)]
		public bool ReadOnly
		{
			get { return readOnly; }
			set
			{
				if (readOnly != value)
				{
					readOnly = value;
					RefreshDeleteButtonVisibility();
				}
			}
		}
		bool readOnly;

		#endregion

		#region TopRightPanelFocus

		void AddTopRightPanelFocusEventHandlers()
		{
			topRightPanel.Click += TopRightPanelControl_Click;
			foreach (Control control in topRightPanel.Controls)
			{
				control.Click += TopRightPanelControl_Click;
			}
		}

		void TopRightPanelControl_Click(object sender, EventArgs e)
		{
			if (!Focused)
			{
				var controlToFocus = valueAnalysisControl ?? (Control)this;
				controlToFocus.Focus();
			}
		}

		#endregion

		#region ISalesHeaderStripControl Members

		public bool IsDeleteButtonFocused
		{
			get { return DeleteButton.Focused; }
		}

		#endregion

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}

			if (valueAnalysisControl != null)
			{
				valueAnalysisControl.Dispose();
			}

			base.Dispose(disposing);
		}
	}
}
