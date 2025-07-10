using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.GUI
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
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo columnStyleCommodity = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo columnStyleOrigin = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo columnStyleDestination = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo columnStyleTransportMode = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo columnStyleContainerMode = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo columnStyleDirection = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo columnStyleServiceLevel = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			this.SchemaBindingSource = new System.Windows.Forms.BindingSource(this.components);

			this.RateCommodityGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SchemaBindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.RateCommodityGrid)).BeginInit();
			this.RateCommodityGrid.SuspendLayout();
			this.SuspendLayout();

			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgRateCommodityDefaultingRuleCollection);

			// 
			// RateCommodityGrid
			// 
			this.BindingSource.SetBindingMember(this.RateCommodityGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			// This below will be linked to the RateCommodityDefaultingRule business objects in the Masterfiles once created .
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgRateCommodityDefaultingRule)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgRateCommodityDefaultingRule)(null)).ORC_RH_NKCommodityCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgRateCommodityDefaultingRule)(null)).ORC_Origin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgRateCommodityDefaultingRule)(null)).ORC_Destination)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgRateCommodityDefaultingRule)(null)).ORC_ContainerMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgRateCommodityDefaultingRule)(null)).ORC_TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgRateCommodityDefaultingRule)(null)).ORC_Direction)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgRateCommodityDefaultingRule)(null)).ORC_RS_NKServiceLevel)));


			this.RateCommodityGrid.CaptionVisible = false;
			columnStyleCommodity.ColumnName = OrgRateCommodityDefaultingRuleSchema.Constants.ORC_RH_NKCommodityCode;
			columnStyleOrigin.ColumnName = OrgRateCommodityDefaultingRuleSchema.Constants.ORC_Origin;
			columnStyleDestination.ColumnName = OrgRateCommodityDefaultingRuleSchema.Constants.ORC_Destination;
			columnStyleDirection.ColumnName = OrgRateCommodityDefaultingRuleSchema.Constants.ORC_Direction;
			columnStyleTransportMode.ColumnName = OrgRateCommodityDefaultingRuleSchema.Constants.ORC_TransportMode;
			columnStyleContainerMode.ColumnName = OrgRateCommodityDefaultingRuleSchema.Constants.ORC_ContainerMode;
			columnStyleServiceLevel.ColumnName = OrgRateCommodityDefaultingRuleSchema.Constants.ORC_RS_NKServiceLevel;

			this.RateCommodityGrid.ColumnStyles.Add(columnStyleCommodity);
			this.RateCommodityGrid.ColumnStyles.Add(columnStyleOrigin);
			this.RateCommodityGrid.ColumnStyles.Add(columnStyleDestination);
			this.RateCommodityGrid.ColumnStyles.Add(columnStyleDirection);
			this.RateCommodityGrid.ColumnStyles.Add(columnStyleTransportMode);
			this.RateCommodityGrid.ColumnStyles.Add(columnStyleContainerMode);
			this.RateCommodityGrid.ColumnStyles.Add(columnStyleServiceLevel);

			this.RateCommodityGrid.CopySelectedRowsAllowed = true;
			this.RateCommodityGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RateCommodityGrid.GridId = "84e46f74-7dc5-4c83-9db9-24cbed8f8652";
			this.RateCommodityGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RateCommodityGrid.LayoutKey = "zGrid1";
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
			((System.ComponentModel.ISupportInitialize)(this.SchemaBindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RateCommodityGrid)).EndInit();
			this.RateCommodityGrid.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.ZGrid RateCommodityGrid;
		private System.Windows.Forms.BindingSource SchemaBindingSource;

	}
}
