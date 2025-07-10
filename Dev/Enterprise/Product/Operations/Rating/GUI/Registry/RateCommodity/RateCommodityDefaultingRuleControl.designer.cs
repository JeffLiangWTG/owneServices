using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	partial class RateCommodityDefaultingRuleControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{

			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo columnStyleCommodity = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo columnStyleOrigin = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo columnStyleDestination = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo columnStyleTransportMode = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo columnStyleContainerMode = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo columnStyleDirection = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo columnStyleServiceLevel = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();

			this.RateCommodityGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.RateCommodityGrid)).BeginInit();
			this.RateCommodityGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Rating.Business.RateCommodityDefaultingRuleCollection);
			// 
			// RateCommodityGrid
			//
			this.RateCommodityGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.RateCommodityGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			// This below will be linked to the RateCommodityDefaultingRule business objects in the Masterfiles once created .
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Rating.Business.RateCommodityDefaultingRule)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Rating.Business.RateCommodityDefaultingRule)(null)).RateCommodityCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Rating.Business.RateCommodityDefaultingRule)(null)).Origin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Rating.Business.RateCommodityDefaultingRule)(null)).Destination)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Rating.Business.RateCommodityDefaultingRule)(null)).TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Rating.Business.RateCommodityDefaultingRule)(null)).ContainerMode)));  
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Rating.Business.RateCommodityDefaultingRule)(null)).Direction)));  
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Rating.Business.RateCommodityDefaultingRule)(null)).ServiceLevel)));  

			columnStyleCommodity.BindToList = "Lookups.CommodityCodes";
			columnStyleCommodity.ColumnName = RateCommodityDefaultingRule.Schema.RateCommodityCode;
			columnStyleCommodity.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			columnStyleOrigin.BindToList = "Lookups.Locations";
			columnStyleOrigin.ColumnName = RateCommodityDefaultingRule.Schema.Origin;
			columnStyleOrigin.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			columnStyleDestination.BindToList = "Lookups.Locations";
			columnStyleDestination.ColumnName = RateCommodityDefaultingRule.Schema.Destination;
			columnStyleDestination.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			columnStyleDirection.BindToList = "Lookups.DirectionList";
			columnStyleDirection.ColumnName = RateCommodityDefaultingRule.Schema.Direction;
			columnStyleDirection.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			columnStyleTransportMode.BindToList = "Lookups.TransportModeList";
			columnStyleTransportMode.ColumnName = RateCommodityDefaultingRule.Schema.TransportMode;
			columnStyleTransportMode.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			columnStyleContainerMode.BindToList = "Lookups.ContainerModeList";
			columnStyleContainerMode.ColumnName = RateCommodityDefaultingRule.Schema.ContainerMode;
			columnStyleContainerMode.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			columnStyleServiceLevel.BindToList = "Lookups.ServiceLevels";
			columnStyleServiceLevel.ColumnName = RateCommodityDefaultingRule.Schema.ServiceLevel;
			columnStyleServiceLevel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			this.RateCommodityGrid.CaptionVisible = false;
			this.RateCommodityGrid.ColumnStyles.Add(columnStyleCommodity);
			this.RateCommodityGrid.ColumnStyles.Add(columnStyleOrigin);
			this.RateCommodityGrid.ColumnStyles.Add(columnStyleDestination);
			this.RateCommodityGrid.ColumnStyles.Add(columnStyleDirection);
			this.RateCommodityGrid.ColumnStyles.Add(columnStyleTransportMode);
			this.RateCommodityGrid.ColumnStyles.Add(columnStyleContainerMode);
			this.RateCommodityGrid.ColumnStyles.Add(columnStyleServiceLevel);

			this.RateCommodityGrid.CopySelectedRowsAllowed = true;
			this.RateCommodityGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RateCommodityGrid.GridId = "27243ffb-75db-4578-9c18-f6ce3292fa7a";
			this.RateCommodityGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RateCommodityGrid.LayoutKey = "RateCommodityGrid";
			this.RateCommodityGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RateCommodityGrid.Name = "RateCommodityGrid";
			this.RateCommodityGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 200, true);
			this.RateCommodityGrid.TabIndex = 0;
			// 
			// RateCommodityControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.RateCommodityGrid);
			this.Name = "RateCommodityControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 212, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RateCommodityGrid)).EndInit();
			this.RateCommodityGrid.ResumeLayout(false);
			this.RateCommodityGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZGrid RateCommodityGrid;

	}
}
