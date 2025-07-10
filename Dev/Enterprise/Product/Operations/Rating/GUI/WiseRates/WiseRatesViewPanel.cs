using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public class WiseRatesViewPanel : RateEntryPanel, IEntryTabPage
	{
		public WiseRatesViewPanel()
		{
			Category = RatingConstants.RateCategory.RatesServiceCategory;
			InitializeComponent();
			this.SetReadOnlyIncludingChildren(true);
			WiseRatesAssignUniversalChargeCodeMenuItem.CreateForGlobalChargeCode(WiseRatesViewGrid, new AssignUniversalChargeCodeCommand(true));
			WiseRatesAssignUniversalChargeCodeMenuItem.CreateForLocalChargeCode(WiseRatesViewGrid, new AssignUniversalChargeCodeCommand());
			WiseRatesAssignCarrierCodeMenuItem.Create(WiseRatesViewGrid, new AssignCarrierCodeCommand());
			WiseRatesAssignIATACodeMenuItem.Create(WiseRatesViewGrid, new CreateAirlineAndAssignIATACodeCommand());
			WiseRatesCreateAirlineAndAssignIATACodeMenuItem.Create(WiseRatesViewGrid, new CreateAirlineAndAssignIATACodeCommand(shouldCreateAirline: true));
			WiseRatesAssignCarrierServiceLevelsMenuItem.Create(WiseRatesViewGrid, new AssignCarrierServiceLevelsCommand(WiseRatesViewGrid));
			WiseRatesAssignContainerMenuItem.Create(WiseRatesViewGrid, new AssignContainerCodeCommand());
			UniversalCommodityGroupMenu.Add(WiseRatesViewGrid);
		}

		#region Overrides

		public override ZGrid RateEntryGrid
		{
			get { return WiseRatesViewGrid; }
		}

		public override RateLinesAndItemsControl RateLinesAndItemsControl
		{
			get { return WiseRatesRateLinesAndItemsControl; }
		}

		public override CostingRateLineAndItemsControl CostingRateLineAndItemsControl
		{
			get { return null; }
		}

		#endregion

		#region IDisposable Members

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(isNotFinalizing);
		}

		#endregion

		DisplayGridWithNotifications WiseRatesViewGrid;
		RateLinesAndItemsControl WiseRatesRateLinesAndItemsControl;

		#region Component Designer generated code

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		readonly System.ComponentModel.Container components;

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.WiseRatesViewGrid = new DisplayGridWithNotifications();
			this.WiseRatesRateLinesAndItemsControl = new RateLinesAndItemsControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.WiseRatesViewGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(WiseRatingHeaderView);
			// 
			// EntryGridPanel
			// 
			EntryGridPanel.Controls.Add(this.WiseRatesViewGrid);
			// 
			// EntryRelatedControlsPanel
			// 
			CurrentEntryRelatedControlsPanel.Controls.Add(this.WiseRatesRateLinesAndItemsControl);
			CurrentEntryRelatedControlsPanelMininmum = 160;
			SplitterDistance = 436;
			// 
			// WiseRatesViewGrid
			// 
			this.WiseRatesViewGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.WiseRatesViewGrid, "WiseEntryViews");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((WiseRatingHeaderView)(null)).WiseEntryViews);
			this.WiseRatesViewGrid.CaptionVisible = false;
			this.WiseRatesViewGrid.GridId = "ea658592-8ef6-40ec-bb02-24325882d5b0";
			this.WiseRatesViewGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.WiseRatesViewGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.WiseRatesViewGrid.LayoutKey = "WiseRatesViewGrid";
			this.WiseRatesViewGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.WiseRatesViewGrid.Name = "WiseRatesViewGrid";
			this.WiseRatesViewGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.WiseRatesViewGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 436, true);

			this.WiseRatesViewGrid.TabIndex = 3;
			// 
			// WiseRatesRateLinesAndItemsControl
			// 
			this.BindingSource.SetBindingMember(this.WiseRatesRateLinesAndItemsControl, ".");
			this.WiseRatesRateLinesAndItemsControl.BindTo = "WiseEntryViews.ChildWiseRateLineViews";
			this.WiseRatesRateLinesAndItemsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.WiseRatesRateLinesAndItemsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.WiseRatesRateLinesAndItemsControl.Name = "WiseRatesRateLinesAndItemsControl";
			this.WiseRatesRateLinesAndItemsControl.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.WiseRatesRateLinesAndItemsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 160, true);
			this.WiseRatesRateLinesAndItemsControl.TabIndex = 4;
			// 
			// WiseRatesViewPanel
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "WiseRatesViewPanel";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 600, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.WiseRatesViewGrid)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

		class DisplayGridWithNotifications : ZDisplayGrid
		{
			protected override bool ShouldShowNotifications => true;
		}
	}
}
