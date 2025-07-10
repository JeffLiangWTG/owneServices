namespace Enterprise.MasterFiles.GUI
{
	public partial class TaxIdAndTaxMessageCombinationRulesRegistryControl
	{
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.TopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ValidationOptionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TaxIdAndTaxMessageCombinationRulesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TopPanel.SuspendLayout();
			this.ValidationOptionDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TaxIdAndTaxMessageCombinationRulesGrid)).BeginInit();
			this.TaxIdAndTaxMessageCombinationRulesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.TaxIdAndTaxMessageCombinationRulesConfiguration);
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.ValidationOptionDropEdit);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 28, true);
			this.TopPanel.TabIndex = 1;
			// 
			// ValidationOptionDropEdit
			// 
			this.ValidationOptionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ValidationOptionDropEdit, "ValidationOption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.TaxIdAndTaxMessageCombinationRulesConfiguration)(null)).ValidationOption)));
			this.ValidationOptionDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TaxIdAndTaxMessageCombinationRulesRegistryControl|6f8b862e-2b89-4185-9944-20762a8105af", "Validation Option");
			this.ValidationOptionDropEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ValidationOptionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ValidationOptionDropEdit.Name = "ValidationOptionDropEdit";
			this.ValidationOptionDropEdit.PreBoundMaxLength = 3;
			this.ValidationOptionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 20, true);
			this.ValidationOptionDropEdit.TabIndex = 2;
			// 
			// TaxIdAndTaxMessageCombinationRulesGrid
			// 
			this.TaxIdAndTaxMessageCombinationRulesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.TaxIdAndTaxMessageCombinationRulesGrid, "TaxIdAndTaxMessageCombinationRulesCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.TaxIdAndTaxMessageCombinationRulesConfiguration)(null)).TaxIdAndTaxMessageCombinationRulesCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.TaxIdAndTaxMessageCombinationRules)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.TaxIdAndTaxMessageCombinationRulesConfiguration)(null)).TaxIdAndTaxMessageCombinationRulesCollection)).SyncRoot)).LineType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.TaxIdAndTaxMessageCombinationRules)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.TaxIdAndTaxMessageCombinationRulesConfiguration)(null)).TaxIdAndTaxMessageCombinationRulesCollection)).SyncRoot)).TaxRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.TaxIdAndTaxMessageCombinationRules)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.TaxIdAndTaxMessageCombinationRulesConfiguration)(null)).TaxIdAndTaxMessageCombinationRulesCollection)).SyncRoot)).TaxMessage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.TaxIdAndTaxMessageCombinationRules)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.TaxIdAndTaxMessageCombinationRulesConfiguration)(null)).TaxIdAndTaxMessageCombinationRulesCollection)).SyncRoot)).TaxGroupCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.TaxIdAndTaxMessageCombinationRules)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.TaxIdAndTaxMessageCombinationRulesConfiguration)(null)).TaxIdAndTaxMessageCombinationRulesCollection)).SyncRoot)).TaxGroupDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.TaxIdAndTaxMessageCombinationRules)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.TaxIdAndTaxMessageCombinationRulesConfiguration)(null)).TaxIdAndTaxMessageCombinationRulesCollection)).SyncRoot)).GovernmentCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.TaxIdAndTaxMessageCombinationRules)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.TaxIdAndTaxMessageCombinationRulesConfiguration)(null)).TaxIdAndTaxMessageCombinationRulesCollection)).SyncRoot)).TaxRateType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.TaxIdAndTaxMessageCombinationRules)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.TaxIdAndTaxMessageCombinationRulesConfiguration)(null)).TaxIdAndTaxMessageCombinationRulesCollection)).SyncRoot)).TaxRateValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.TaxIdAndTaxMessageCombinationRules)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.TaxIdAndTaxMessageCombinationRulesConfiguration)(null)).TaxIdAndTaxMessageCombinationRulesCollection)).SyncRoot)).AuxiliaryType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.TaxIdAndTaxMessageCombinationRules)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.TaxIdAndTaxMessageCombinationRulesConfiguration)(null)).TaxIdAndTaxMessageCombinationRulesCollection)).SyncRoot)).ExtraTaxRate)));
			this.TaxIdAndTaxMessageCombinationRulesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("d4c20c52-e9a9-4f64-8256-e9f9ed12fd2c", "Line Type");
			zDropEditColumnStyleInfo1.ColumnName = "LineType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("908f65b6-766a-4a75-9a30-fa73793a6086", "Tax ID");
			zGuidFindBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "TaxRate";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("2f6850c8-6f90-4cd2-9b9c-62911c986a13", "Tax Message");
			zGuidFindBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zGuidFindBoxColumnStyleInfo2.ColumnName = "TaxMessage";
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("786577f4-5cc2-44da-8c6b-b61b57e24ed4", "Tax Group Code");
			zTextBoxColumnStyleInfo1.ColumnName = "TaxGroupCode";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("20adb08a-845c-458e-9c87-cf9abd17a078", "Tax Group Description");
			zTextBoxColumnStyleInfo2.ColumnName = "TaxGroupDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("9b53c1b5-2447-42f5-9288-cb229af6f063", "Government Code");
			zTextBoxColumnStyleInfo3.ColumnName = "GovernmentCode";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("147298b5-2f99-4cbd-ae1c-34f03ff4d6db", "Tax Rate Type");
			zTextBoxColumnStyleInfo4.ColumnName = "TaxRateType";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("3a47afed-53d6-4a28-9f19-ed3a0703181c", "Tax Rate");
			zCalcEditColumnStyleInfo1.ColumnName = "TaxRateValue";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("0d668139-e34b-42ae-94db-f17107e65ca8", "Auxiliary Type");
			zTextBoxColumnStyleInfo5.ColumnName = "AuxiliaryType";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("21c65e6e-aa27-4266-a104-4693c68454b5", "Extra Tax Rate");
			zCalcEditColumnStyleInfo2.ColumnName = "ExtraTaxRate";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.TaxIdAndTaxMessageCombinationRulesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.TaxIdAndTaxMessageCombinationRulesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.TaxIdAndTaxMessageCombinationRulesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.TaxIdAndTaxMessageCombinationRulesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.TaxIdAndTaxMessageCombinationRulesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.TaxIdAndTaxMessageCombinationRulesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.TaxIdAndTaxMessageCombinationRulesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.TaxIdAndTaxMessageCombinationRulesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.TaxIdAndTaxMessageCombinationRulesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.TaxIdAndTaxMessageCombinationRulesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.TaxIdAndTaxMessageCombinationRulesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TaxIdAndTaxMessageCombinationRulesGrid.GridId = "B838D4EE-0188-4390-BC45-A243AD6741E9";
			this.TaxIdAndTaxMessageCombinationRulesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TaxIdAndTaxMessageCombinationRulesGrid.LayoutKey = "TaxIdAndTaxMessageCombinationRulesGrid";
			this.TaxIdAndTaxMessageCombinationRulesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 28, true);
			this.TaxIdAndTaxMessageCombinationRulesGrid.Name = "TaxIdAndTaxMessageCombinationRulesGrid";
			this.TaxIdAndTaxMessageCombinationRulesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 267, true);
			this.TaxIdAndTaxMessageCombinationRulesGrid.TabIndex = 3;
			// 
			// TaxIdAndTaxMessageCombinationRulesRegistryControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TaxIdAndTaxMessageCombinationRulesGrid);
			this.Controls.Add(this.TopPanel);
			this.Name = "TaxIdAndTaxMessageCombinationRulesRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 295, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.ValidationOptionDropEdit.ResumeLayout(true);
			this.ValidationOptionDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.TaxIdAndTaxMessageCombinationRulesGrid)).EndInit();
			this.TaxIdAndTaxMessageCombinationRulesGrid.ResumeLayout(false);
			this.TaxIdAndTaxMessageCombinationRulesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		ZArchitecture.ZGrid TaxIdAndTaxMessageCombinationRulesGrid;
		ZArchitecture.GUI.ZPanel TopPanel;
		ZArchitecture.GUI.ZDropEdit ValidationOptionDropEdit;
	}
}
