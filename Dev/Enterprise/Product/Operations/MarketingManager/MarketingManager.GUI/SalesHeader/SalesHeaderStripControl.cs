using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MarketingManager.GUI
{
	public partial class SalesHeaderStripControl : ZUserControl, IReadOnlyToggleControl, ISalesHeaderStripControl
	{
		public SalesHeaderStripControl()
		{
			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
				SetupDeleteButton();

				var oldFont = salesProductNameLabel.Font;
				salesProductNameLabel.Font = new Font(oldFont.FontFamily, oldFont.Size * 1.5f, oldFont.Style);
				RefreshCollapsedControls();
				AddTopRightPanelFocusEventHandlers();
			}
		}

		#region CurrentDataItem

		public new SalesHeader CurrentDataItem
		{
			get { return (SalesHeader)base.CurrentDataItem; }
		}

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
				tradeLaneWithDetailsControl.Collapsed = Collapsed;
				tradeLaneWithDetailsControl.ShowAssociatedActivities = !Collapsed;
				topRightPanel.BackColor = Collapsed ? Color.White : headerExpandedColor;
				salesProductNameLabel.ForeColor = Collapsed ? Color.Black : Color.White;
				monthlyTotalLabel.ForeColor = Collapsed ? Color.Black : Color.White;
				annualTotalLabel.ForeColor = Collapsed ? Color.Black : Color.White;
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
				var productName = CurrentDataItem.SalesProductName;
				var dialogResult = Globals.Message.Show(
					Res.GetString("ae712c51-5b68-4f71-a2e2-3ac67957e189", "Are you sure you want to delete all estimate values for {0}?", productName),
					Res.GetString("ee32bb09-3f29-4784-8f78-27599ddca5e1", "Delete all {0} Estimate Values", productName),
					MessageBoxButtons.YesNo,
					DialogResult.No);

				if (dialogResult == DialogResult.Yes)
				{
					CurrentDataItem.Delete();
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

		#region Focus

		public void Focus(bool onNewRow)
		{
			var tradeLanesGrid = tradeLaneWithDetailsControl.TradeLanesGrid;
			if (tradeLanesGrid != null)
			{
				tradeLanesGrid.Focus();
				if (onNewRow && tradeLanesGrid.ListManager != null && tradeLanesGrid.Columns.Count > 0)
				{
					tradeLanesGrid.BeginEdit(tradeLanesGrid.Columns[0].ColumnStyle, tradeLanesGrid.ListManager.Count);
				}
			}
			else
			{
				Focus();
			}
		}

		public void Focus(EntitySalesWrapper entitySales)
		{
			var tradeLanesGrid = tradeLaneWithDetailsControl.TradeLanesGrid;
			if (tradeLanesGrid != null)
			{
				tradeLanesGrid.Focus();
				for (var i = 0; i < tradeLanesGrid.List.Count; i++)
				{
					if (((EntitySalesWrapper)tradeLanesGrid.List[i]).PK == entitySales.PK)
					{
						tradeLanesGrid.ListManager.Position = i;
						tradeLanesGrid.Select(i);
						break;
					}
				}
			}
			else
			{
				Focus();
			}
		}

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
				var controlToFocus = tradeLaneWithDetailsControl.TradeLanesGrid ?? (Control)this;
				controlToFocus.Focus();
			}
		}

		#endregion

		#region Fetch Hints

		public static void AddFetchHintsForView(SalesHeader salesHeader)
		{
			if (salesHeader != null)
			{
				salesHeader.FetchStrategy.FetchForView(new[] {
					new TableColumn("", SalesHeader.Schema.TotalEstimatedAnnualValue),
					new TableColumn("", SalesHeader.Schema.TotalEstimatedMonthlyAverage) }
				);
			}
		}

		#endregion

		#region ISalesHeaderStripControl Members

		public bool IsDeleteButtonFocused
		{
			get { return DeleteButton.Focused; }
		}

		#endregion
	}
}
