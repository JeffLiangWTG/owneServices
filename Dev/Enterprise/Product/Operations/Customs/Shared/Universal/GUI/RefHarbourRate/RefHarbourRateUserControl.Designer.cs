namespace Enterprise.Customs.Universal.GUI
{
	partial class RefHarbourRateUserControl
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
			this.ZXF_ZZZ_NKDataGroupingCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ZXF_PortTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ZXF_RateFormulaTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ZXF_TypeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ZXF_ModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ZXF_CommodityTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ZXF_PortTaxTypeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ZXF_StartDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ZXF_EndDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ZXF_ZZZ_NKDataGroupingCodeFindBox.SuspendLayout();
			this.ZXF_ModeDropEdit.SuspendLayout();
			this.ZXF_StartDateEdit.SuspendLayout();
			this.ZXF_EndDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Universal.RefHarbourRate);
			// 
			// ZXF_ZZZ_NKDataGroupingCodeFindBox
			// 
			this.BindingSource.SetBindingMember(this.ZXF_ZZZ_NKDataGroupingCodeFindBox, "ZXF_ZZZ_NKDataGrouping");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.RefHarbourRate)(null)).ZXF_ZZZ_NKDataGrouping)));
			this.ZXF_ZZZ_NKDataGroupingCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 226, true);
			this.ZXF_ZZZ_NKDataGroupingCodeFindBox.Name = "ZXF_ZZZ_NKDataGroupingCodeFindBox";
			this.ZXF_ZZZ_NKDataGroupingCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 17, true);
			this.ZXF_ZZZ_NKDataGroupingCodeFindBox.TabIndex = 7;
			// 
			// ZXF_PortTextBox
			// 
			this.BindingSource.SetBindingMember(this.ZXF_PortTextBox, "ZXF_Port");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.RefHarbourRate)(null)).ZXF_Port)));
			this.ZXF_PortTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 45, true);
			this.ZXF_PortTextBox.Name = "ZXF_PortTextBox";
            this.ZXF_PortTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 17, true);
			this.ZXF_PortTextBox.TabIndex = 2;
			// 
			// ZXF_RateFormulaTextBox
			// 
			this.ZXF_RateFormulaTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ZXF_RateFormulaTextBox, "ZXF_RateFormula");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.RefHarbourRate)(null)).ZXF_RateFormula)));
			this.ZXF_RateFormulaTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 149, true);
			this.ZXF_RateFormulaTextBox.Multiline = true;
			this.ZXF_RateFormulaTextBox.Name = "ZXF_RateFormulaTextBox";
			this.ZXF_RateFormulaTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 68, true);
			this.ZXF_RateFormulaTextBox.TabIndex = 6;
			// 
			// ZXF_TypeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ZXF_TypeTextBox, "ZXF_Type");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.RefHarbourRate)(null)).ZXF_Type)));
			this.ZXF_TypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 19, true);
			this.ZXF_TypeTextBox.Name = "ZXF_TypeTextBox";
            this.ZXF_TypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 17, true);
			this.ZXF_TypeTextBox.TabIndex = 1;
			// 
			// ZXF_ModeDropEdit
			// 
			this.BindingSource.SetBindingMember(this.ZXF_ModeDropEdit, "ZXF_Mode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.RefHarbourRate)(null)).ZXF_Mode)));
			this.ZXF_ModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 71, true);
			this.ZXF_ModeDropEdit.Name = "ZXF_ModeDropEdit";
            this.ZXF_ModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 17, true);
			this.ZXF_ModeDropEdit.TabIndex = 3;
			// 
			// ZXF_CommodityTextBox
			// 
			this.BindingSource.SetBindingMember(this.ZXF_CommodityTextBox, "ZXF_Commodity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.RefHarbourRate)(null)).ZXF_Commodity)));
			this.ZXF_CommodityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 97, true);
			this.ZXF_CommodityTextBox.Name = "ZXF_CommodityTextBox";
            this.ZXF_CommodityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 17, true);
			this.ZXF_CommodityTextBox.TabIndex = 4;
			// 
			// ZXF_PortTaxTypeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ZXF_PortTaxTypeTextBox, "ZXF_PortTaxType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.RefHarbourRate)(null)).ZXF_PortTaxType)));
			this.ZXF_PortTaxTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 123, true);
			this.ZXF_PortTaxTypeTextBox.Name = "ZXF_PortTaxTypeTextBox";
            this.ZXF_PortTaxTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 17, true);
			this.ZXF_PortTaxTypeTextBox.TabIndex = 5;
			// 
			// ZXF_StartDateEdit
			// 
			this.ZXF_StartDateEdit.AllowDrop = true;
			this.ZXF_StartDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.ZXF_StartDateEdit, "ZXF_StartDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Customs.Universal.RefHarbourRate)(null)).ZXF_StartDate)));
			this.ZXF_StartDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 252, true);
			this.ZXF_StartDateEdit.Name = "ZXF_StartDateEdit";
			this.ZXF_StartDateEdit.TabIndex = 8;
			// 
			// ZXF_EndDateEdit
			// 
			this.ZXF_EndDateEdit.AllowDrop = true;
			this.ZXF_EndDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.ZXF_EndDateEdit, "ZXF_EndDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Customs.Universal.RefHarbourRate)(null)).ZXF_EndDate)));
			this.ZXF_EndDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 278, true);
			this.ZXF_EndDateEdit.Name = "ZXF_EndDateEdit";
			this.ZXF_EndDateEdit.TabIndex = 9;
			// 
			// ZZRefHarbourRateUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ZXF_PortTaxTypeTextBox);
			this.Controls.Add(this.ZXF_CommodityTextBox);
			this.Controls.Add(this.ZXF_ModeDropEdit);
			this.Controls.Add(this.ZXF_PortTextBox);
			this.Controls.Add(this.ZXF_RateFormulaTextBox);
			this.Controls.Add(this.ZXF_TypeTextBox);
			this.Controls.Add(this.ZXF_ZZZ_NKDataGroupingCodeFindBox);
			this.Controls.Add(this.ZXF_StartDateEdit);
			this.Controls.Add(this.ZXF_EndDateEdit);
			this.Name = "RefHarbourRateUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(510, 360, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ZXF_ZZZ_NKDataGroupingCodeFindBox.ResumeLayout(true);
			this.ZXF_ZZZ_NKDataGroupingCodeFindBox.PerformLayout();
			this.ZXF_ModeDropEdit.ResumeLayout(true);
			this.ZXF_ModeDropEdit.PerformLayout();
			this.ZXF_StartDateEdit.ResumeLayout(true);
			this.ZXF_StartDateEdit.PerformLayout();
			this.ZXF_EndDateEdit.ResumeLayout(true);
			this.ZXF_EndDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZCodeFindBox ZXF_ZZZ_NKDataGroupingCodeFindBox;
		private ZArchitecture.ZTextBox ZXF_PortTextBox;
		private ZArchitecture.ZTextBox ZXF_RateFormulaTextBox;
		private ZArchitecture.ZTextBox ZXF_TypeTextBox;
		private ZArchitecture.GUI.ZDropEdit ZXF_ModeDropEdit;
		private ZArchitecture.ZTextBox ZXF_CommodityTextBox;
		private ZArchitecture.ZTextBox ZXF_PortTaxTypeTextBox;
		private ZArchitecture.GUI.ZDateEdit ZXF_StartDateEdit;
		private ZArchitecture.GUI.ZDateEdit ZXF_EndDateEdit;
	}
}
