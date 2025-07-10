using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture;

namespace Enterprise.TransportCommon.GUI.Registry
{
	public partial class ServiceTaskCreatorOptionControl
	{
		public ZGrid ServiceTaskCreatorOptionGrid;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.ServiceTaskCreatorOptionGrid = new ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ServiceTaskCreatorOptionGrid)).BeginInit();
			this.SuspendLayout();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(TransportCommon.Registry.ServiceTaskCreatorOptionCollection);
			//
			// ServiceTaskCreatorOptionGrid
			//
			this.ServiceTaskCreatorOptionGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ServiceTaskCreatorOptionGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(null);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((TransportCommon.Registry.ServiceTaskCreatorOption)(null)).ContainerMode);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((TransportCommon.Registry.ServiceTaskCreatorOption)(null)).TargetModule);
			this.ServiceTaskCreatorOptionGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.TransportCommon.GUI.Res.GetData("010e99a6-a954-4e47-906d-afc81c012b50", "Freight", "Freight Mode", "");
			zDropEditColumnStyleInfo1.ColumnName = "ContainerMode";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.TransportCommon.GUI.Res.GetData("965ecaac-267e-445c-b037-3063a4feb268", "Target", "Target Module", "");
			zDropEditColumnStyleInfo2.ColumnName = "TargetModule";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.ServiceTaskCreatorOptionGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ServiceTaskCreatorOptionGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ServiceTaskCreatorOptionGrid.CopySelectedRowsAllowed = true;
			this.ServiceTaskCreatorOptionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ServiceTaskCreatorOptionGrid.GridId = "5CCEEBDB-BF7A-4596-896A-BC88138F45CE";
			this.ServiceTaskCreatorOptionGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ServiceTaskCreatorOptionGrid.LayoutKey = "JobTemplateDefaultGrid";
			this.ServiceTaskCreatorOptionGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ServiceTaskCreatorOptionGrid.Name = "ServiceTaskCreatorOptionGrid";
			this.ServiceTaskCreatorOptionGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(457, 240, true);
			this.ServiceTaskCreatorOptionGrid.TabIndex = 0;
			//
			// ServiceTaskCreatorOptionControl
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ServiceTaskCreatorOptionGrid);
			this.Name = "ServiceTaskCreatorOptionControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(457, 240, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ServiceTaskCreatorOptionGrid)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
