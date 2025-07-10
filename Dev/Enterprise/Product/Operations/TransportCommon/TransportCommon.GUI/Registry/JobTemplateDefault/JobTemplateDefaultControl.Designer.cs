using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture;

namespace Enterprise.TransportCommon.GUI.Registry
{
	public partial class JobTemplateDefaultControl
	{
		public ZGrid JobTemplateDefaultGrid;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.JobTemplateDefaultGrid = new ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.JobTemplateDefaultGrid)).BeginInit();
			this.SuspendLayout();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(TransportCommon.Registry.JobTemplateDefaultCollection);
			//
			// JobTemplateDefaultGrid
			//
			this.JobTemplateDefaultGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.JobTemplateDefaultGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(null);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((TransportCommon.Registry.JobTemplateDefault)(null)).Parent);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((TransportCommon.Registry.JobTemplateDefault)(null)).Direction);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((TransportCommon.Registry.JobTemplateDefault)(null)).TransportMode);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((TransportCommon.Registry.JobTemplateDefault)(null)).ContainerMode);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((TransportCommon.Registry.JobTemplateDefault)(null)).HasOrganisation);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((TransportCommon.Registry.JobTemplateDefault)(null)).BookingTemplate);
			this.JobTemplateDefaultGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.TransportCommon.GUI.Res.GetData("JobTemplateDefaultControl|a5b0ecaa-1e5b-4c5f-b27b-a426fdd24e62", "Parent", "Parent Type", "");
			zDropEditColumnStyleInfo1.ColumnName = "Parent";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.TransportCommon.GUI.Res.GetData("JobTemplateDefaultControl|eeb76b4b-a848-4356-85f4-2f7c766fa745", "Direction");
			zDropEditColumnStyleInfo2.ColumnName = "Direction";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.TransportCommon.GUI.Res.GetData("JobTemplateDefaultControl|05a94aa6-fe75-4eeb-80fe-f778b34a0812", "Transport", "Transport Mode", "");
			zDropEditColumnStyleInfo3.ColumnName = "TransportMode";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.TransportCommon.GUI.Res.GetData("JobTemplateDefaultControl|187058d3-aabc-4272-a90b-3c3a2fee92cd", "Freight", "Freight Mode", "");
			zDropEditColumnStyleInfo4.ColumnName = "ContainerMode";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo5.CaptionResourceString = Enterprise.TransportCommon.GUI.Res.GetData("JobTemplateDefaultControl|74d051b5-913b-4f49-bdd2-1e53a2c9e03e", "Has Org.", "Has Organization", "");
			zDropEditColumnStyleInfo5.ColumnName = "HasOrganisation";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo6.CaptionResourceString = Enterprise.TransportCommon.GUI.Res.GetData("JobTemplateDefaultControl|3cc1cf18-2fe6-4057-8132-77ff07ddfa61", "Template", "Booking Template", "");
			zDropEditColumnStyleInfo6.ColumnName = "BookingTemplate";
			this.JobTemplateDefaultGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.JobTemplateDefaultGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.JobTemplateDefaultGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.JobTemplateDefaultGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.JobTemplateDefaultGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.JobTemplateDefaultGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.JobTemplateDefaultGrid.GridId = "5CCEEBDB-BF7A-4596-896A-BC88138F45CE";
			this.JobTemplateDefaultGrid.CopySelectedRowsAllowed = true;
			this.JobTemplateDefaultGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.JobTemplateDefaultGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.JobTemplateDefaultGrid.LayoutKey = "JobTemplateDefaultGrid";
			this.JobTemplateDefaultGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.JobTemplateDefaultGrid.Name = "JobTemplateDefaultGrid";
			this.JobTemplateDefaultGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(457, 240, true);
			this.JobTemplateDefaultGrid.TabIndex = 0;
			//
			// JobTemplateDefaultControl
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.JobTemplateDefaultGrid);
			this.Name = "JobTemplateDefaultControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(457, 240, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.JobTemplateDefaultGrid)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
