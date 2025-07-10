namespace Enterprise.Customs.TR.ETrade.GUI
{
	partial class ETradePackedItemDetailsUserControl
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
			this.BanderolTariffFindBox = new Enterprise.Customs.Universal.GUI.TariffFindBox();
			this.SerialNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.UsedGoodsCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StatisticalValueCalcFindBox = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			this.AgriculturePolicyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ValueDeclarationFormTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CalculationMethodTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.QuotaCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.TariffAdditionalCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BanderolTariffFindBox.SuspendLayout();
			this.StatisticalValueCalcFindBox.SuspendLayout();
			this.TariffAdditionalCodeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BanderolTariffFindBox
			// 
			this.BanderolTariffFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BanderolTariffFindBox, "PackedItem.BanderolTariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.ETrade.Business.AsycudaPack)(null)).PackedItem.BanderolTariff)));
			this.BanderolTariffFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 3, true);
			this.BanderolTariffFindBox.Name = "BanderolTariffFindBox";
			this.BanderolTariffFindBox.PreBoundMaxLength = 8;
			this.BanderolTariffFindBox.ShouldResize = false;
			this.BanderolTariffFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(242, 17, true);
			this.BanderolTariffFindBox.TabIndex = 0;
			// 
			// SerialNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.SerialNoTextBox, "PackedItem.SerialNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.ETrade.Business.AsycudaPack)(null)).PackedItem.SerialNo)));
			this.SerialNoTextBox.CaptionResourceString = Enterprise.Customs.TR.ETrade.GUI.Res.GetData("ee2d3bd4-5a7b-4773-bed6-9efeaa45be1e", "Serial No");
			this.SerialNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 25, true);
			this.SerialNoTextBox.Name = "SerialNoTextBox";
			this.SerialNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 17, true);
			this.SerialNoTextBox.TabIndex = 2;
			// 
			// UsedGoodsCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.UsedGoodsCodeTextBox, "PackedItem.UsedGoodsCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.ETrade.Business.AsycudaPack)(null)).PackedItem.UsedGoodsCode)));
			this.UsedGoodsCodeTextBox.CaptionResourceString = Enterprise.Customs.TR.ETrade.GUI.Res.GetData("b20bdc67-0948-4683-9856-09bf453efdc9", "Used Goods Code");
			this.UsedGoodsCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 100, true);
			this.UsedGoodsCodeTextBox.Name = "UsedGoodsCodeTextBox";
			this.UsedGoodsCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 17, true);
			this.UsedGoodsCodeTextBox.TabIndex = 9;
			// 
			// StatisticalValueCalcFindBox
			// 
			this.StatisticalValueCalcFindBox.AllowDrop = true;
			this.StatisticalValueCalcFindBox.BindToAmount = "PackedItem.StatisticalValue";
			this.StatisticalValueCalcFindBox.BindToUnit = "PackedItem.StatisticalValueCurrency";
			this.StatisticalValueCalcFindBox.CaptionResourceString = Enterprise.Customs.TR.ETrade.GUI.Res.GetData("9257c936-e1eb-4384-949d-493e8f15027d", "Statistical Value");
			this.StatisticalValueCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.StatisticalValueCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.StatisticalValueCalcFindBox.Name = "StatisticalValueCalcFindBox";
			this.StatisticalValueCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.StatisticalValueCalcFindBox.TabIndex = 19;
			// 
			// AgriculturePolicyTextBox
			// 
			this.BindingSource.SetBindingMember(this.AgriculturePolicyTextBox, "PackedItem.AgriculturePolicy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.ETrade.Business.AsycudaPack)(null)).PackedItem.AgriculturePolicy)));
			this.AgriculturePolicyTextBox.CaptionResourceString = Enterprise.Customs.TR.ETrade.GUI.Res.GetData("81521fc3-f0d9-4563-8ac3-1066a2333510", "Agriculture Policy");
			this.AgriculturePolicyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 100, true);
			this.AgriculturePolicyTextBox.Name = "AgriculturePolicyTextBox";
			this.AgriculturePolicyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 17, true);
			this.AgriculturePolicyTextBox.TabIndex = 9;
			// 
			// ValueDeclarationFormTextBox
			// 
			this.BindingSource.SetBindingMember(this.ValueDeclarationFormTextBox, "PackedItem.ValueDeclarationForm");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.ETrade.Business.AsycudaPack)(null)).PackedItem.ValueDeclarationForm)));
			this.ValueDeclarationFormTextBox.CaptionResourceString = Enterprise.Customs.TR.ETrade.GUI.Res.GetData("8993ec5e-a739-4859-bc88-aba2046e18a0", "Val. Dec. Form");
			this.ValueDeclarationFormTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 100, true);
			this.ValueDeclarationFormTextBox.Name = "ValueDeclarationFormTextBox";
			this.ValueDeclarationFormTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 17, true);
			this.ValueDeclarationFormTextBox.TabIndex = 9;
			// 
			// CalculationMethodTextBox
			// 
			this.BindingSource.SetBindingMember(this.CalculationMethodTextBox, "PackedItem.CalculationMethod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.ETrade.Business.AsycudaPack)(null)).PackedItem.CalculationMethod)));
			this.CalculationMethodTextBox.CaptionResourceString = Enterprise.Customs.TR.ETrade.GUI.Res.GetData("8a7e1a34-28fe-492b-bfb6-87170720b93b", "Calculation Method");
			this.CalculationMethodTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 100, true);
			this.CalculationMethodTextBox.Name = "CalculationMethodTextBox";
			this.CalculationMethodTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 17, true);
			this.CalculationMethodTextBox.TabIndex = 9;
			// 
			// QuotaCheckBox
			// 
			this.BindingSource.SetBindingMember(this.QuotaCheckBox, "PackedItem.QuotaCheck");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.TR.ETrade.Business.AsycudaPack)(null)).PackedItem.QuotaCheck)));
			this.QuotaCheckBox.CaptionResourceString = Enterprise.Customs.TR.ETrade.GUI.Res.GetData("7bfe00c8-07e2-4770-abb0-c189f0045c5d", "Quota");
			this.QuotaCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.QuotaCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 48, true);
			this.QuotaCheckBox.Name = "QuotaCheckBox";
			this.QuotaCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 20, true);
			this.QuotaCheckBox.TabIndex = 6;
			// 
			// TariffAdditionalCodeDropEdit
			// 
			this.TariffAdditionalCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TariffAdditionalCodeDropEdit, "PackedItem.API_ChemicalSubstanceCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.ETrade.Business.AsycudaPack)(null)).PackedItem.API_ChemicalSubstanceCode)));
			this.TariffAdditionalCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 154, true);
			this.TariffAdditionalCodeDropEdit.Name = "TariffAdditionalCodeDropEdit";
			this.TariffAdditionalCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
			this.TariffAdditionalCodeDropEdit.TabIndex = 20;
			// 
			// ETradePackedItemDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TariffAdditionalCodeDropEdit);
			this.Controls.Add(this.BanderolTariffFindBox);
			this.Controls.Add(this.SerialNoTextBox);
			this.Controls.Add(this.UsedGoodsCodeTextBox);
			this.Controls.Add(this.StatisticalValueCalcFindBox);
			this.Controls.Add(this.AgriculturePolicyTextBox);
			this.Controls.Add(this.ValueDeclarationFormTextBox);
			this.Controls.Add(this.CalculationMethodTextBox);
			this.Controls.Add(this.QuotaCheckBox);
			this.Name = "ETradePackedItemDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(601, 247, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BanderolTariffFindBox.ResumeLayout(true);
			this.BanderolTariffFindBox.PerformLayout();
			this.StatisticalValueCalcFindBox.ResumeLayout(true);
			this.StatisticalValueCalcFindBox.PerformLayout();
			this.TariffAdditionalCodeDropEdit.ResumeLayout(true);
			this.TariffAdditionalCodeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

		internal Enterprise.Customs.Universal.GUI.TariffFindBox BanderolTariffFindBox;
		internal Enterprise.ZArchitecture.GUI.ZCheckBox QuotaCheckBox;
		internal Enterprise.ZArchitecture.ZTextBox SerialNoTextBox;
		internal Enterprise.ZArchitecture.ZTextBox UsedGoodsCodeTextBox;
		internal Enterprise.ZArchitecture.ZTextBox AgriculturePolicyTextBox;
		internal Enterprise.ZArchitecture.ZTextBox ValueDeclarationFormTextBox;
		internal Enterprise.ZArchitecture.ZTextBox CalculationMethodTextBox;
		internal Customs.GUI.ConvertToLocalCurrencyControl StatisticalValueCalcFindBox;
		internal ZArchitecture.GUI.ZDropEdit TariffAdditionalCodeDropEdit;
	}
}
