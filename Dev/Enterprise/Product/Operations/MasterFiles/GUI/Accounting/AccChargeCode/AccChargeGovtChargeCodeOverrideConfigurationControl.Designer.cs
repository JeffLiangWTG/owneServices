namespace Enterprise.MasterFiles.GUI
{
	partial class AccChargeGovtChargeCodeOverrideConfigurationControl
	{
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.govtChargeCodeOverrideGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.govtChargeCodeOverrideGrid)).BeginInit();
			this.govtChargeCodeOverrideGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccChargeGovtChargeCodeOverride);
			// 
			// zGrid1
			// 
			this.govtChargeCodeOverrideGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.govtChargeCodeOverrideGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccChargeGovtChargeCodeOverride)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeGovtChargeCodeOverride)(null)).ACG_CostSellAll)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeGovtChargeCodeOverride)(null)).ACG_JobType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeGovtChargeCodeOverride)(null)).ACG_TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeGovtChargeCodeOverride)(null)).ACG_Direction)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccChargeGovtChargeCodeOverride)(null)).ACG_GovtChargeCode)));
			this.govtChargeCodeOverrideGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("4711AEDC-DA2A-4824-981A-AA8DE71CB294", "Cost/Sell");
			zDropEditColumnStyleInfo1.ColumnName = "ACG_CostSellAll";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("B174CA15-DE20-4E51-9367-937AAE52F458", "Job Type");
			zDropEditColumnStyleInfo2.ColumnName = "ACG_JobType";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("C1B03875-7653-4869-B203-A42C381126EA", "Transport Mode");
			zDropEditColumnStyleInfo3.ColumnName = "ACG_TransportMode";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("1551B1C9-7B2B-40E5-A5B8-034002BD9542", "Direction");
			zDropEditColumnStyleInfo4.ColumnName = "ACG_Direction";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("8508B620-5D81-424B-BFA3-3D692A432901", "Govt Charge Code");
			zTextBoxColumnStyleInfo1.ColumnName = "ACG_GovtChargeCode";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.govtChargeCodeOverrideGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.govtChargeCodeOverrideGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.govtChargeCodeOverrideGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.govtChargeCodeOverrideGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.govtChargeCodeOverrideGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.govtChargeCodeOverrideGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.govtChargeCodeOverrideGrid.GridId = "89084E8D-B646-4ED5-932A-45BF7569D430";
			this.govtChargeCodeOverrideGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.govtChargeCodeOverrideGrid.LayoutKey = "zGrid1";
			this.govtChargeCodeOverrideGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.govtChargeCodeOverrideGrid.Name = "zGrid1";
			this.govtChargeCodeOverrideGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(687, 283, true);
			this.govtChargeCodeOverrideGrid.TabIndex = 2;
			// 
			// AccGovtChargeCodeConfigurationControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.govtChargeCodeOverrideGrid);
			this.Name = "AccGovtChargeCodeConfigurationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(687, 283, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.govtChargeCodeOverrideGrid)).EndInit();
			this.govtChargeCodeOverrideGrid.ResumeLayout(false);
			this.govtChargeCodeOverrideGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

	}
}
