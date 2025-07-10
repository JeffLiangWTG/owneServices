namespace Enterprise.Freight.GUI
{
	public partial class DefaultContainerModesControl
	{

		#region Component Designer generated code

		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.DefaultContainerModesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DefaultContainerModesGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Business.DefaultContainerModesCollection);
			// 
			// DefaultContainerModesGrid
			// 
			this.DefaultContainerModesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.DefaultContainerModesGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Business.DefaultContainerModes)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.DefaultContainerModes)(null)).TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Business.DefaultContainerModes)(null)).TransportModeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.DefaultContainerModes)(null)).ContainerMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Business.DefaultContainerModes)(null)).ContainerModeList)));
			this.DefaultContainerModesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.BindToList = "TransportModeList";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("DefaultContainerModesControl|95647993-40f9-4367-ad5f-7310fd85f0ec", "Transport Mode");
			zDropEditColumnStyleInfo1.ColumnName = "TransportMode";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo2.BindToList = "ContainerModeList";
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("DefaultContainerModesControl|2fc32368-efbd-4473-be44-cd6c1fe5ade2", "Container Mode");
			zDropEditColumnStyleInfo2.ColumnName = "ContainerMode";
			zDropEditColumnStyleInfo2.IsMandatory = true;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.DefaultContainerModesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.DefaultContainerModesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.DefaultContainerModesGrid.GridId = "fd450af9-c6ab-4a11-9c31-8a91f58e40bb";
			this.DefaultContainerModesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DefaultContainerModesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DefaultContainerModesGrid.LayoutKey = "DefaultContainerModesGrid";
			this.DefaultContainerModesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DefaultContainerModesGrid.Name = "DefaultContainerModesGrid";
			this.DefaultContainerModesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 376, true);
			this.DefaultContainerModesGrid.TabIndex = 0;
			// 
			// DefaultContainerModesControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DefaultContainerModesGrid);
			this.Name = "DefaultContainerModesControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 376, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DefaultContainerModesGrid)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

	}
}
