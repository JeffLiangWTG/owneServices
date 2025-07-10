namespace Enterprise.MasterFiles.GUI
{
	partial class AccCommissionRuleRatesControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			this.ACT_CommissionPeriodDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ACT_RX_NKCommissionCurrencyCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ACT_CommissionAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ACT_CommissionPercentageSuffixLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ACT_CommissionPercentageCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ACT_CommissionTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RatesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ACT_CommissionPeriodDropEdit.SuspendLayout();
			this.ACT_RX_NKCommissionCurrencyCodeFindBox.SuspendLayout();
			this.ACT_CommissionTypeDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RatesGrid)).BeginInit();
			this.RatesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccCommissionRuleRateCollection);
			// 
			// ACT_CommissionPeriodDropEdit
			// 
			this.ACT_CommissionPeriodDropEdit.AllowDrop = true;
			this.ACT_CommissionPeriodDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ACT_CommissionPeriodDropEdit, "ACT_CommissionPeriod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AccCommissionRuleRate)(null)).ACT_CommissionPeriod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccCommissionRuleRate)(null)).ACT_CommissionPeriodDescription)));
			this.ACT_CommissionPeriodDropEdit.BindToForDescription = "ACT_CommissionPeriodDescription";
			this.ACT_CommissionPeriodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 205, true);
			this.ACT_CommissionPeriodDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.ACT_CommissionPeriodDropEdit.Name = "ACT_CommissionPeriodDropEdit";
			this.ACT_CommissionPeriodDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(228, 20, true);
			this.ACT_CommissionPeriodDropEdit.TabIndex = 1;
			// 
			// ACT_RX_NKCommissionCurrencyCodeFindBox
			// 
			this.ACT_RX_NKCommissionCurrencyCodeFindBox.AllowDrop = true;
			this.ACT_RX_NKCommissionCurrencyCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ACT_RX_NKCommissionCurrencyCodeFindBox, "ACT_RX_NKCommissionCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccCommissionRuleRate)(null)).ACT_RX_NKCommissionCurrency)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ACT_RX_NKCommissionCurrencyCodeFindBox, false);
			this.ACT_RX_NKCommissionCurrencyCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(164, 277, true);
			this.ACT_RX_NKCommissionCurrencyCodeFindBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.ACT_RX_NKCommissionCurrencyCodeFindBox.Name = "ACT_RX_NKCommissionCurrencyCodeFindBox";
			this.ACT_RX_NKCommissionCurrencyCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(169, 20, true);
			this.ACT_RX_NKCommissionCurrencyCodeFindBox.TabIndex = 6;
			// 
			// ACT_CommissionAmountCalcEdit
			// 
			this.ACT_CommissionAmountCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.ACT_CommissionAmountCalcEdit, "ACT_CommissionAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccCommissionRuleRate)(null)).ACT_CommissionAmount)));
			this.ACT_CommissionAmountCalcEdit.DecimalPlaces = 2;
			this.ACT_CommissionAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 277, true);
			this.ACT_CommissionAmountCalcEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.ACT_CommissionAmountCalcEdit.Name = "ACT_CommissionAmountCalcEdit";
			this.ACT_CommissionAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 20, true);
			this.ACT_CommissionAmountCalcEdit.TabIndex = 5;
			this.ACT_CommissionAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ACT_CommissionPercentageSuffixLabel
			// 
			this.ACT_CommissionPercentageSuffixLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ACT_CommissionPercentageSuffixLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("d9ebd867-fc84-4cc9-be01-225ce0a6218f", "%");
			this.ACT_CommissionPercentageSuffixLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 251, true);
			this.ACT_CommissionPercentageSuffixLabel.Name = "ACT_CommissionPercentageSuffixLabel";
			this.ACT_CommissionPercentageSuffixLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 23, true);
			this.ACT_CommissionPercentageSuffixLabel.TabIndex = 4;
			// 
			// ACT_CommissionPercentageCalcEdit
			// 
			this.ACT_CommissionPercentageCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.ACT_CommissionPercentageCalcEdit, "ACT_CommissionPercentage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccCommissionRuleRate)(null)).ACT_CommissionPercentage)));
			this.ACT_CommissionPercentageCalcEdit.DecimalPlaces = 2;
			this.ACT_CommissionPercentageCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 253, true);
			this.ACT_CommissionPercentageCalcEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.ACT_CommissionPercentageCalcEdit.Name = "ACT_CommissionPercentageCalcEdit";
			this.ACT_CommissionPercentageCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 20, true);
			this.ACT_CommissionPercentageCalcEdit.TabIndex = 3;
			this.ACT_CommissionPercentageCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ACT_CommissionTypeDropEdit
			// 
			this.ACT_CommissionTypeDropEdit.AllowDrop = true;
			this.ACT_CommissionTypeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ACT_CommissionTypeDropEdit, "ACT_CommissionType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AccCommissionRuleRate)(null)).ACT_CommissionType)));
			this.ACT_CommissionTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 229, true);
			this.ACT_CommissionTypeDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.ACT_CommissionTypeDropEdit.Name = "ACT_CommissionTypeDropEdit";
			this.ACT_CommissionTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(228, 20, true);
			this.ACT_CommissionTypeDropEdit.TabIndex = 2;
			// 
			// RatesGrid
			// 
			this.RatesGrid.AllowNavigation = false;
			this.RatesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.RatesGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccCommissionRuleRate)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccCommissionRuleRate)(null)).ACT_CommissionPeriod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccCommissionRuleRate)(null)).ACT_CommissionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccCommissionRuleRate)(null)).ACT_CommissionPercentage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccCommissionRuleRate)(null)).ACT_CommissionAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccCommissionRuleRate)(null)).ACT_RX_NKCommissionCurrency)));
			this.RatesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "ACT_CommissionPeriod";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo2.ColumnName = "ACT_CommissionType";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "ACT_CommissionPercentage";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "ACT_CommissionAmount";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "ACT_RX_NKCommissionCurrency";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.RatesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.RatesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.RatesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.RatesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.RatesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.RatesGrid.CopySelectedRowsAllowed = true;
			this.RatesGrid.GridId = "34a98ea5-21c3-420b-bb48-dfafbbc9a9fc";
			this.RatesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RatesGrid.LayoutKey = "ratesGrid";
			this.RatesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RatesGrid.Name = "RatesGrid";
			this.RatesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(340, 197, true);
			this.RatesGrid.TabIndex = 0;
			// 
			// AccCommissionRuleRatesControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ACT_RX_NKCommissionCurrencyCodeFindBox);
			this.Controls.Add(this.ACT_CommissionAmountCalcEdit);
			this.Controls.Add(this.ACT_CommissionPercentageSuffixLabel);
			this.Controls.Add(this.ACT_CommissionPercentageCalcEdit);
			this.Controls.Add(this.ACT_CommissionTypeDropEdit);
			this.Controls.Add(this.ACT_CommissionPeriodDropEdit);
			this.Controls.Add(this.RatesGrid);
			this.Name = "AccCommissionRuleRatesControl";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 3, 5, 3, true);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(340, 303, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ACT_CommissionPeriodDropEdit.ResumeLayout(true);
			this.ACT_CommissionPeriodDropEdit.PerformLayout();
			this.ACT_RX_NKCommissionCurrencyCodeFindBox.ResumeLayout(true);
			this.ACT_RX_NKCommissionCurrencyCodeFindBox.PerformLayout();
			this.ACT_CommissionTypeDropEdit.ResumeLayout(true);
			this.ACT_CommissionTypeDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.RatesGrid)).EndInit();
			this.RatesGrid.ResumeLayout(false);
			this.RatesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDropEdit ACT_CommissionTypeDropEdit;
		private ZArchitecture.ZCalcEdit ACT_CommissionPercentageCalcEdit;
		private ZArchitecture.ZLabel ACT_CommissionPercentageSuffixLabel;
		private ZArchitecture.ZCalcEdit ACT_CommissionAmountCalcEdit;
		private ZArchitecture.GUI.ZCodeFindBox ACT_RX_NKCommissionCurrencyCodeFindBox;
		private ZArchitecture.GUI.ZDropEdit ACT_CommissionPeriodDropEdit;
		protected System.Windows.Forms.TableLayoutPanel mainTableLayoutPanel;
		internal ZArchitecture.ZGrid RatesGrid;
	}
}
