namespace Enterprise.MasterFiles.GUI
{
	public partial class LandedCostingPrefsPageControl
	{

		#region Component Designer generated code

		private Enterprise.ZArchitecture.GUI.ZGroupBox PreferenceGroupBox;
		private Enterprise.ZArchitecture.ZLabel ChargesHeadingLabel;
		private Enterprise.ZArchitecture.ZLabel GroupHeadingLabel;
		private Enterprise.ZArchitecture.ZLabel InstructionsLabel;
		private Enterprise.ZArchitecture.ZGrid LandedCostingChargesGrid;
		private Enterprise.ZArchitecture.ZGrid LandedCostingPrefsGrid;
		private Enterprise.ZArchitecture.ZLabel zLabel1;
		private Enterprise.ZArchitecture.ZCalcEdit zCalcEdit1;
		private Enterprise.ZArchitecture.ZCalcEdit LCMarginPercentage2CalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit LCMarginPercentage3CalcEdit;
		private Enterprise.ZArchitecture.GUI.ZGroupBox MarkUpPercentageGroupBox;

		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.PreferenceGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ChargesHeadingLabel = new Enterprise.ZArchitecture.ZLabel();
			this.GroupHeadingLabel = new Enterprise.ZArchitecture.ZLabel();
			this.InstructionsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.LandedCostingChargesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.LandedCostingPrefsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.MarkUpPercentageGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LCMarginPercentage3CalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.LCMarginPercentage2CalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zCalcEdit1 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PreferenceGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LandedCostingChargesGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.LandedCostingPrefsGrid)).BeginInit();
			this.MarkUpPercentageGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// PreferenceGroupBox
			// 
			this.PreferenceGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.PreferenceGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("LandedCostingPrefsPageControl|2724778e-407d-44fe-8248-13e0492e4d2c", "Landed Costing Group Preferences");
			this.PreferenceGroupBox.Controls.Add(this.ChargesHeadingLabel);
			this.PreferenceGroupBox.Controls.Add(this.GroupHeadingLabel);
			this.PreferenceGroupBox.Controls.Add(this.InstructionsLabel);
			this.PreferenceGroupBox.Controls.Add(this.LandedCostingChargesGrid);
			this.PreferenceGroupBox.Controls.Add(this.LandedCostingPrefsGrid);
			this.PreferenceGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 94, true);
			this.PreferenceGroupBox.Name = "PreferenceGroupBox";
			this.PreferenceGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(808, 484, true);
			this.PreferenceGroupBox.TabIndex = 1;
			this.PreferenceGroupBox.TabStop = false;
			// 
			// ChargesHeadingLabel
			// 
			this.ChargesHeadingLabel.AutoSize = true;
			this.ChargesHeadingLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("LandedCostingPrefsPageControl|ea8aea3e-f41a-406e-b883-357b307d8ff0", "Charge Groups and Charge Codes");
			this.ChargesHeadingLabel.IsFontBold = true;
			this.ChargesHeadingLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 222, true);
			this.ChargesHeadingLabel.Name = "ChargesHeadingLabel";
			this.ChargesHeadingLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(194, 13, true);
			this.ChargesHeadingLabel.TabIndex = 9;
			// 
			// GroupHeadingLabel
			// 
			this.GroupHeadingLabel.AutoSize = true;
			this.GroupHeadingLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("LandedCostingPrefsPageControl|65db6d81-d2a1-4872-959a-739306d1eadf", "Landed Costing Groups");
			this.GroupHeadingLabel.IsFontBold = true;
			this.GroupHeadingLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 59, true);
			this.GroupHeadingLabel.Name = "GroupHeadingLabel";
			this.GroupHeadingLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 13, true);
			this.GroupHeadingLabel.TabIndex = 8;
			// 
			// InstructionsLabel
			// 
			this.InstructionsLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("LandedCostingPrefsPageControl|f8e1d3aa-3f1a-45d2-aa93-04ba7929d097", "These preferences will be used when generating Landed Costings. You can specify a list of Landed Costing Groups, and then specify what Charge Code Groups and / or Charge Codes are part of this group.");
			this.InstructionsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 16, true);
			this.InstructionsLabel.Name = "InstructionsLabel";
			this.InstructionsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(784, 40, true);
			this.InstructionsLabel.TabIndex = 7;
			// 
			// LandedCostingChargesGrid
			// 
			this.LandedCostingChargesGrid.AllowNavigation = false;
			this.LandedCostingChargesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.LandedCostingChargesGrid, "LandedCostingPreferences.Charges");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgLandedCostingPrefs)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).LandedCostingPreferences)).SyncRoot)).Charges)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgLandedCostingPrefCharges)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgLandedCostingPrefs)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).LandedCostingPreferences)).SyncRoot)).Charges)).SyncRoot)).O0_ChargeGroup)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgLandedCostingPrefCharges)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgLandedCostingPrefs)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).LandedCostingPreferences)).SyncRoot)).Charges)).SyncRoot)).ChargeGroupDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgLandedCostingPrefCharges)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgLandedCostingPrefs)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).LandedCostingPreferences)).SyncRoot)).Charges)).SyncRoot)).O0_AC_ChargeCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgLandedCostingPrefCharges)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgLandedCostingPrefs)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).LandedCostingPreferences)).SyncRoot)).Charges)).SyncRoot)).ChargeCodeDesc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgLandedCostingPrefCharges)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgLandedCostingPrefs)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).LandedCostingPreferences)).SyncRoot)).Charges)).SyncRoot)).O0_Excluded)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgLandedCostingPrefCharges)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgLandedCostingPrefs)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).LandedCostingPreferences)).SyncRoot)).Charges)).SyncRoot)).CompanyName)));
			this.LandedCostingChargesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "O0_ChargeGroup";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("LandedCostingPrefsPageControl|6aa5808c-33fb-4a19-94a0-1e0f1a06b6d5", "Charge Group");
			zTextBoxColumnStyleInfo1.ColumnName = "ChargeGroupDescription";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "O0_AC_ChargeCode";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("LandedCostingPrefsPageControl|9ec527f1-648f-4f66-831f-2405d5d42411", "Charge Code");
			zTextBoxColumnStyleInfo2.ColumnName = "ChargeCodeDesc";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			zCheckBoxColumnStyleInfo1.ColumnName = "O0_Excluded";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo5.ColumnName = "CompanyName";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.LandedCostingChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.LandedCostingChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.LandedCostingChargesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.LandedCostingChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.LandedCostingChargesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.LandedCostingChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.LandedCostingChargesGrid.GridId = "3e044eb7-01ed-433d-808a-df8b604fdc3f";
			this.LandedCostingChargesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.LandedCostingChargesGrid.LayoutKey = "LandedCostingChargesGrid";
			this.LandedCostingChargesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 248, true);
			this.LandedCostingChargesGrid.Name = "LandedCostingChargesGrid";
			this.LandedCostingChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(793, 228, true);
			this.LandedCostingChargesGrid.TabIndex = 1;
			// 
			// LandedCostingPrefsGrid
			// 
			this.LandedCostingPrefsGrid.AllowNavigation = false;
			this.LandedCostingPrefsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.LandedCostingPrefsGrid, "LandedCostingPreferences");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).LandedCostingPreferences)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgLandedCostingPrefs)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).LandedCostingPreferences)).SyncRoot)).O9_LandedCostGroup)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgLandedCostingPrefs)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).LandedCostingPreferences)).SyncRoot)).O9_LandedCostGroupName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgLandedCostingPrefs)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).LandedCostingPreferences)).SyncRoot)).O9_DistributeCostBy)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgLandedCostingPrefs)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).LandedCostingPreferences)).SyncRoot)).CostDistributionDesc)));
			this.LandedCostingPrefsGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "O9_LandedCostGroup";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo3.ColumnName = "O9_LandedCostGroupName";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(230);
			zDropEditColumnStyleInfo2.ColumnName = "O9_DistributeCostBy";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("LandedCostingPrefsPageControl|632505e3-07ea-4757-8643-c2d7c6fe46ab", "Cost Distribution");
			zTextBoxColumnStyleInfo4.ColumnName = "CostDistributionDesc";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(230);
			this.LandedCostingPrefsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.LandedCostingPrefsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.LandedCostingPrefsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.LandedCostingPrefsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.LandedCostingPrefsGrid.GridId = "3933432d-3f37-491e-939d-22d1c28c2564";
			this.LandedCostingPrefsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.LandedCostingPrefsGrid.LayoutKey = "zGrid1";
			this.LandedCostingPrefsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 80, true);
			this.LandedCostingPrefsGrid.Name = "LandedCostingPrefsGrid";
			this.LandedCostingPrefsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(793, 128, true);
			this.LandedCostingPrefsGrid.TabIndex = 0;
			// 
			// MarkUpPercentageGroupBox
			// 
			this.MarkUpPercentageGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.MarkUpPercentageGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("LandedCostingPrefsPageControl|7e2b6521-fcf8-4a66-bedc-51788faad912", "Mark-Up Percentages");
			this.MarkUpPercentageGroupBox.Controls.Add(this.LCMarginPercentage3CalcEdit);
			this.MarkUpPercentageGroupBox.Controls.Add(this.LCMarginPercentage2CalcEdit);
			this.MarkUpPercentageGroupBox.Controls.Add(this.zCalcEdit1);
			this.MarkUpPercentageGroupBox.Controls.Add(this.zLabel1);
			this.MarkUpPercentageGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.MarkUpPercentageGroupBox.Name = "MarkUpPercentageGroupBox";
			this.MarkUpPercentageGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(808, 81, true);
			this.MarkUpPercentageGroupBox.TabIndex = 0;
			this.MarkUpPercentageGroupBox.TabStop = false;
			// 
			// LCMarginPercentage3CalcEdit
			// 
			this.BindingSource.SetBindingMember(this.LCMarginPercentage3CalcEdit, "MiscServ+OM_LandedCostMarginPercent3");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_LandedCostMarginPercent3)));
			this.LCMarginPercentage3CalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(674, 46, true);
			this.LCMarginPercentage3CalcEdit.Name = "LCMarginPercentage3CalcEdit";
			this.LCMarginPercentage3CalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.LCMarginPercentage3CalcEdit.TabIndex = 2;
			this.LCMarginPercentage3CalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// LCMarginPercentage2CalcEdit
			// 
			this.BindingSource.SetBindingMember(this.LCMarginPercentage2CalcEdit, "MiscServ+OM_LandedCostMarginPercent2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_LandedCostMarginPercent2)));
			this.LCMarginPercentage2CalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(403, 46, true);
			this.LCMarginPercentage2CalcEdit.Name = "LCMarginPercentage2CalcEdit";
			this.LCMarginPercentage2CalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.LCMarginPercentage2CalcEdit.TabIndex = 1;
			this.LCMarginPercentage2CalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zCalcEdit1
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit1, "MiscServ+OM_LandedCostMarginPercent1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_LandedCostMarginPercent1)));
			this.zCalcEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 46, true);
			this.zCalcEdit1.Name = "zCalcEdit1";
			this.zCalcEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.zCalcEdit1.TabIndex = 0;
			this.zCalcEdit1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zLabel1
			// 
			this.zLabel1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("LandedCostingPrefsPageControl|61b26a04-727e-4b46-bfc4-960a6e67936e", "These percentages will be applied to total cost of Landed Costing Line to calculate three different sell prices.");
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 16, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(542, 18, true);
			this.zLabel1.TabIndex = 8;
			// 
			// LandedCostingPrefsPageControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MarkUpPercentageGroupBox);
			this.Controls.Add(this.PreferenceGroupBox);
			this.Name = "LandedCostingPrefsPageControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(824, 592, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PreferenceGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.LandedCostingChargesGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.LandedCostingPrefsGrid)).EndInit();
			this.MarkUpPercentageGroupBox.ResumeLayout(false);
			this.MarkUpPercentageGroupBox.PerformLayout();
			this.ResumeLayout(false);
		}
		#endregion

	}
}
