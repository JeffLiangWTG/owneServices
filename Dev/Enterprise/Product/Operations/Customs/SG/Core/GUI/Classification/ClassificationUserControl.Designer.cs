namespace Enterprise.Customs.SG.V4.GUI
{
	public partial class ClassificationUserControl : Customs.GUI.BaseClassificationUserControl
	{
		ZArchitecture.ZCalcEdit cC_PercAlcoholCalcEdit;
		ZArchitecture.GUI.ZGroupBox productCodesGroupBox;
		ZArchitecture.ZGrid productCodesGrid;
		Universal.GUI.TariffFindBox cC_TariffNumCodeFindBox;

		void InitializeComponent()
		{
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.cC_TariffNumCodeFindBox = new Universal.GUI.TariffFindBox();
			this.cC_PercAlcoholCalcEdit = new ZArchitecture.ZCalcEdit();
			this.productCodesGroupBox = new ZArchitecture.GUI.ZGroupBox();
			this.productCodesGrid = new ZArchitecture.ZGrid();
			this.BaseClassificationGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.productCodesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.productCodesGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BaseClassificationGroupBox
			// 
			this.BaseClassificationGroupBox.Controls.Add(this.productCodesGroupBox);
			this.BaseClassificationGroupBox.Controls.Add(this.cC_PercAlcoholCalcEdit);
			this.BaseClassificationGroupBox.Controls.Add(this.cC_TariffNumCodeFindBox);
			this.BaseClassificationGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BaseClassificationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(418, 356, true);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.CC_IsActiveCheckBox, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.LastAuditDateEdit, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.AuditStaffCodeFindBox, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.DescriptionTextBox, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.cC_TariffNumCodeFindBox, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.cC_PercAlcoholCalcEdit, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.LookupCodeTextBox, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.productCodesGroupBox, 0);
			// 
			// LookupCodeTextBox
			// 
			this.LookupCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 24, true);
			this.LookupCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(287, 20, true);
			this.LookupCodeTextBox.TabIndex = 1;
			// 
			// CC_IsActiveCheckBox
			// 
			this.CC_IsActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(339, 324, true);
			this.CC_IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 17, true);
			this.CC_IsActiveCheckBox.TabIndex = 13;
			// 
			// DescriptionTextBox
			// 
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 48, true);
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(287, 55, true);
			this.DescriptionTextBox.TabIndex = 3;
			// 
			// LastAuditDateEdit
			// 
			this.LastAuditDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(251, 324, true);
			this.LastAuditDateEdit.TabIndex = 12;
			// 
			// AuditStaffCodeFindBox
			// 
			this.AuditStaffCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 324, true);
			this.AuditStaffCodeFindBox.ShowDescriptionBox = false;
			this.AuditStaffCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 20, true);
			this.AuditStaffCodeFindBox.TabIndex = 10;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.Classification);
			// 
			// CC_TariffNumCodeFindBox
			// 
			this.cC_TariffNumCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.cC_TariffNumCodeFindBox, "CC_TariffNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.Classification)(null)).CC_TariffNum)));
			this.cC_TariffNumCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 107, true);
			this.cC_TariffNumCodeFindBox.Name = "CC_TariffNumCodeFindBox";
			this.cC_TariffNumCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(287, 20, true);
			this.cC_TariffNumCodeFindBox.TabIndex = 5;
			this.cC_TariffNumCodeFindBox.TariffType = Universal.Constants.TariffTypes.HarmonizedSystem;
			// 
			// CC_PercAlcoholCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.cC_PercAlcoholCalcEdit, "CC_PercAlcohol");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.Classification)(null)).CC_PercAlcohol)));
			this.cC_PercAlcoholCalcEdit.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("ClassificationUserControl|4427b340-22b5-4b2a-8354-98d2984d148a", "% of Alcohol");
			this.cC_PercAlcoholCalcEdit.DecimalPlaces = 3;
			this.cC_PercAlcoholCalcEdit.Decimals = 3;
			this.cC_PercAlcoholCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 131, true);
			this.cC_PercAlcoholCalcEdit.Name = "CC_PercAlcoholCalcEdit";
			this.cC_PercAlcoholCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.cC_PercAlcoholCalcEdit.TabIndex = 7;
			this.cC_PercAlcoholCalcEdit.Text = "0.000";
			this.cC_PercAlcoholCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ProductCodesGroupBox
			// 
			this.productCodesGroupBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("ClassificationUserControl|bd3b36e1-372a-4c51-b9d9-b3afd1b037db", "Product Codes");
			this.productCodesGroupBox.Controls.Add(this.productCodesGrid);
			this.productCodesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 155, true);
			this.productCodesGroupBox.Name = "ProductCodesGroupBox";
			this.productCodesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(287, 165, true);
			this.productCodesGroupBox.TabIndex = 8;
			this.productCodesGroupBox.TabStop = false;
			// 
			// ProductCodesGrid
			// 
			this.productCodesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.productCodesGrid, "ProductCodes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.Classification)(null)).ProductCodes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.ProductCode)(((System.Collections.IList)(((Business.Classification)(null)).ProductCodes)).SyncRoot)).CY_Data)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.ProductCode)(((System.Collections.IList)(((Business.Classification)(null)).ProductCodes)).SyncRoot)).Lookups.ProductCodes)));
			this.productCodesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "CY_Data";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(132);
			this.productCodesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.productCodesGrid.CopySelectedRowsAllowed = true;
			this.productCodesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.productCodesGrid.GridId = "314bb978-afc9-4fff-9aaa-bf04e841b156";
			this.productCodesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.productCodesGrid.LayoutKey = "ProductCodesGrid";
			this.productCodesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.productCodesGrid.Name = "ProductCodesGrid";
			this.productCodesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(281, 146, true);
			this.productCodesGrid.TabIndex = 0;
			// 
			// ClassificationUserControl
			// 
			this.Name = "ClassificationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(418, 356, true);
			this.BaseClassificationGroupBox.ResumeLayout(false);
			this.BaseClassificationGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.productCodesGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.productCodesGrid)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
