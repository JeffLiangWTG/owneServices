namespace Enterprise.Customs.TR.ETrade.GUI
{
	partial class ETradeBillDetailsUserControl
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
			this.PrecedentFreightCostLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			this.ShipmentTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.NatureOfBusinessDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ExemptionCode1DropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ExemptionCode2DropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GuaranteeTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GuaranteeRefNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GuaranteeAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DepartureCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.TradeCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ExportCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ArrivalCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.PaymentMethodDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AccountantTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AccountantVATTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OtherValueCalcFindBox = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			this.ContainerNumberDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GoodsValueConvertToLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			this.SeparatedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PrecedentFreightCostLocalCurrencyControl.SuspendLayout();
			this.ShipmentTypeDropEdit.SuspendLayout();
			this.NatureOfBusinessDropEdit.SuspendLayout();
			this.ExemptionCode1DropEdit.SuspendLayout();
			this.ExemptionCode2DropEdit.SuspendLayout();
			this.GuaranteeTypeDropEdit.SuspendLayout();
			this.DepartureCountryCodeFindBox.SuspendLayout();
			this.TradeCountryCodeFindBox.SuspendLayout();
			this.ExportCountryCodeFindBox.SuspendLayout();
			this.ArrivalCountryCodeFindBox.SuspendLayout();
			this.PaymentMethodDropEdit.SuspendLayout();
			this.OtherValueCalcFindBox.SuspendLayout();
			this.ContainerNumberDropEdit.SuspendLayout();
			this.GoodsValueConvertToLocalCurrencyControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TR.ETrade.Business.AsycudaBill);
			// 
			// PrecedentFreightCostLocalCurrencyControl
			// 
			this.PrecedentFreightCostLocalCurrencyControl.AllowDrop = true;
			this.PrecedentFreightCostLocalCurrencyControl.BindToAmount = "PrecedentFreightToDisplay";
			this.PrecedentFreightCostLocalCurrencyControl.BindToUnit = "PrecedentFreightCostCurrency";
			this.PrecedentFreightCostLocalCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.PrecedentFreightCostLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(589, 111, true);
			this.PrecedentFreightCostLocalCurrencyControl.Name = "PrecedentFreightCostLocalCurrencyControl";
			this.PrecedentFreightCostLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.PrecedentFreightCostLocalCurrencyControl.TabIndex = 18;
			// 
			// ShipmentTypeDropEdit
			// 
			this.ShipmentTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipmentTypeDropEdit, "Header.AMA_Nature");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.ETrade.Business.AsycudaBill)(null)).Header.AMA_Nature)));
			this.ShipmentTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 12, true);
			this.ShipmentTypeDropEdit.Name = "ShipmentTypeDropEdit";
			this.ShipmentTypeDropEdit.PreBoundMaxLength = 1;
			this.ShipmentTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(155, 20, true);
			this.ShipmentTypeDropEdit.TabIndex = 1;
			// 
			// NatureOfBusinessDropEdit
			// 
			this.NatureOfBusinessDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NatureOfBusinessDropEdit, "NatureOfBusiness");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.ETrade.Business.AsycudaBill)(null)).NatureOfBusiness)));
			this.NatureOfBusinessDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 12, true);
			this.NatureOfBusinessDropEdit.Name = "NatureOfBusinessDropEdit";
			this.NatureOfBusinessDropEdit.PreBoundMaxLength = 1;
			this.NatureOfBusinessDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(155, 20, true);
			this.NatureOfBusinessDropEdit.TabIndex = 1;
			// 
			// ExemptionCode1DropEdit
			// 
			this.ExemptionCode1DropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExemptionCode1DropEdit, "ExemptionCode1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.ETrade.Business.AsycudaBill)(null)).ExemptionCode1)));
			this.ExemptionCode1DropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 38, true);
			this.ExemptionCode1DropEdit.Name = "ExemptionCode1DropEdit";
			this.ExemptionCode1DropEdit.PreBoundMaxLength = 1;
			this.ExemptionCode1DropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(155, 20, true);
			this.ExemptionCode1DropEdit.TabIndex = 1;
			// 
			// ExemptionCode2DropEdit
			// 
			this.ExemptionCode2DropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExemptionCode2DropEdit, "ExemptionCode2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.ETrade.Business.AsycudaBill)(null)).ExemptionCode2)));
			this.ExemptionCode2DropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 64, true);
			this.ExemptionCode2DropEdit.Name = "ExemptionCode2DropEdit";
			this.ExemptionCode2DropEdit.PreBoundMaxLength = 1;
			this.ExemptionCode2DropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(155, 20, true);
			this.ExemptionCode2DropEdit.TabIndex = 1;
			// 
			// GuaranteeTypeDropEdit
			// 
			this.GuaranteeTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GuaranteeTypeDropEdit, "GuaranteeType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.ETrade.Business.AsycudaBill)(null)).GuaranteeType)));
			this.GuaranteeTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 64, true);
			this.GuaranteeTypeDropEdit.Name = "GuaranteeTypeDropEdit";
			this.GuaranteeTypeDropEdit.PreBoundMaxLength = 1;
			this.GuaranteeTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(155, 20, true);
			this.GuaranteeTypeDropEdit.TabIndex = 1;
			// 
			// GuaranteeRefNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.GuaranteeRefNoTextBox, "GuaranteeRefNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.ETrade.Business.AsycudaBill)(null)).GuaranteeRefNo)));
			this.GuaranteeRefNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(691, 2, true);
			this.GuaranteeRefNoTextBox.Name = "GuaranteeRefNoTextBox";
			this.GuaranteeRefNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.GuaranteeRefNoTextBox.TabIndex = 1;
			// 
			// GuaranteeAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.GuaranteeAmountCalcEdit, "GuaranteeAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.ETrade.Business.AsycudaBill)(null)).GuaranteeAmount)));
			this.GuaranteeAmountCalcEdit.DecimalPlaces = 2;
			this.GuaranteeAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(408, 56, true);
			this.GuaranteeAmountCalcEdit.Name = "GuaranteeAmountCalcEdit";
			this.GuaranteeAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.GuaranteeAmountCalcEdit.TabIndex = 1;
			this.GuaranteeAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.GuaranteeAmountCalcEdit.TrackDisposedAccess = true;
			// 
			// DepartureCountryCodeFindBox
			// 
			this.DepartureCountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DepartureCountryCodeFindBox, "DepartureCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.ETrade.Business.AsycudaBill)(null)).DepartureCountry)));
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.DepartureCountryCodeFindBox, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
			this.DepartureCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 82, true);
			this.DepartureCountryCodeFindBox.Name = "DepartureCountryCodeFindBox";
			this.DepartureCountryCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.DepartureCountryCodeFindBox.ParentType = null;
			this.DepartureCountryCodeFindBox.PreBoundMaxLength = 5;
			this.DepartureCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
			this.DepartureCountryCodeFindBox.TabIndex = 1;
			// 
			// TradeCountryCodeFindBox
			// 
			this.TradeCountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TradeCountryCodeFindBox, "TradeCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.ETrade.Business.AsycudaBill)(null)).TradeCountry)));
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.TradeCountryCodeFindBox, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
			this.TradeCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 55, true);
			this.TradeCountryCodeFindBox.Name = "TradeCountryCodeFindBox";
			this.TradeCountryCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.TradeCountryCodeFindBox.ParentType = null;
			this.TradeCountryCodeFindBox.PreBoundMaxLength = 5;
			this.TradeCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
			this.TradeCountryCodeFindBox.TabIndex = 1;
			// 
			// ExportCountryCodeFindBox
			// 
			this.ExportCountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExportCountryCodeFindBox, "ExportCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.ETrade.Business.AsycudaBill)(null)).ExportCountry)));
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.ExportCountryCodeFindBox, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
			this.ExportCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 29, true);
			this.ExportCountryCodeFindBox.Name = "ExportCountryCodeFindBox";
			this.ExportCountryCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ExportCountryCodeFindBox.ParentType = null;
			this.ExportCountryCodeFindBox.PreBoundMaxLength = 5;
			this.ExportCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
			this.ExportCountryCodeFindBox.TabIndex = 1;
			// 
			// ArrivalCountryCodeFindBox
			// 
			this.ArrivalCountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ArrivalCountryCodeFindBox, "ArrivalCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.ETrade.Business.AsycudaBill)(null)).ArrivalCountry)));
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.ArrivalCountryCodeFindBox, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
			this.ArrivalCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 3, true);
			this.ArrivalCountryCodeFindBox.Name = "ArrivalCountryCodeFindBox";
			this.ArrivalCountryCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ArrivalCountryCodeFindBox.ParentType = null;
			this.ArrivalCountryCodeFindBox.PreBoundMaxLength = 5;
			this.ArrivalCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
			this.ArrivalCountryCodeFindBox.TabIndex = 1;
			// 
			// PaymentMethodDropEdit
			// 
			this.PaymentMethodDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PaymentMethodDropEdit, "PaymentMethod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.ETrade.Business.AsycudaBill)(null)).PaymentMethod)));
			this.PaymentMethodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 108, true);
			this.PaymentMethodDropEdit.Name = "PaymentMethodDropEdit";
			this.PaymentMethodDropEdit.PreBoundMaxLength = 1;
			this.PaymentMethodDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
			this.PaymentMethodDropEdit.TabIndex = 1;
			// 
			// AccountantTextBox
			// 
			this.BindingSource.SetBindingMember(this.AccountantTextBox, "AccountantName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.ETrade.Business.AsycudaBill)(null)).AccountantName)));
			this.AccountantTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(377, 3, true);
			this.AccountantTextBox.Name = "AccountantTextBox";
			this.AccountantTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
			this.AccountantTextBox.TabIndex = 24;
			// 
			// AccountantVATTextBox
			// 
			this.BindingSource.SetBindingMember(this.AccountantVATTextBox, "AccountantVAT");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.ETrade.Business.AsycudaBill)(null)).AccountantVAT)));
			this.AccountantVATTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(377, 29, true);
			this.AccountantVATTextBox.Name = "AccountantVATTextBox";
			this.AccountantVATTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
			this.AccountantVATTextBox.TabIndex = 24;
			// 
			// OtherValueCalcFindBox
			// 
			this.OtherValueCalcFindBox.AllowDrop = true;
			this.OtherValueCalcFindBox.BindToAmount = "ABL_OtherValue";
			this.OtherValueCalcFindBox.BindToUnit = "ABL_RX_NKOtherValueCurrency";
			this.OtherValueCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.OtherValueCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(635, 3, true);
			this.OtherValueCalcFindBox.Name = "OtherValueCalcFindBox";
			this.OtherValueCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.OtherValueCalcFindBox.TabIndex = 19;
			// 
			// ContainerNumberDropEdit
			// 
			this.ContainerNumberDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ContainerNumberDropEdit, "ContainerNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.ETrade.Business.AsycudaBill)(null)).ContainerNumber)));
			this.ContainerNumberDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(377, 99, true);
			this.ContainerNumberDropEdit.Name = "ContainerNumberDropEdit";
			this.ContainerNumberDropEdit.PreBoundMaxLength = 20;
			this.ContainerNumberDropEdit.ShowDescriptionBox = false;
			this.ContainerNumberDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.ContainerNumberDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
			this.ContainerNumberDropEdit.TabIndex = 1;
			// 
			// GoodsValueConvertToLocalCurrencyControl
			// 
			this.GoodsValueConvertToLocalCurrencyControl.AllowDrop = true;
			this.GoodsValueConvertToLocalCurrencyControl.BindToAmount = "ABL_GoodsValue";
			this.GoodsValueConvertToLocalCurrencyControl.BindToUnit = "ABL_RX_NKGoodsValueCurrency";
			this.GoodsValueConvertToLocalCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.GoodsValueConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(740, 111, true);
			this.GoodsValueConvertToLocalCurrencyControl.Name = "GoodsValueConvertToLocalCurrencyControl";
			this.GoodsValueConvertToLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.GoodsValueConvertToLocalCurrencyControl.TabIndex = 19;
			// 
			// SeparatedCheckBox
			// 
			this.BindingSource.SetBindingMember(this.SeparatedCheckBox, "Separated");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.TR.ETrade.Business.AsycudaBill)(null)).Separated)));
			this.SeparatedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 24, true);
			this.SeparatedCheckBox.Name = "SeparatedCheckBox";
			this.SeparatedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 24, true);
			this.SeparatedCheckBox.TabIndex = 25;
			this.SeparatedCheckBox.UseVisualStyleBackColor = true;
			// 
			// ETradeBillDetailsUserControl
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.PrecedentFreightCostLocalCurrencyControl);
			this.Controls.Add(this.SeparatedCheckBox);
			this.Controls.Add(this.ShipmentTypeDropEdit);
			this.Controls.Add(this.NatureOfBusinessDropEdit);
			this.Controls.Add(this.ExemptionCode1DropEdit);
			this.Controls.Add(this.ExemptionCode2DropEdit);
			this.Controls.Add(this.GuaranteeTypeDropEdit);
			this.Controls.Add(this.GuaranteeRefNoTextBox);
			this.Controls.Add(this.GuaranteeAmountCalcEdit);
			this.Controls.Add(this.DepartureCountryCodeFindBox);
			this.Controls.Add(this.TradeCountryCodeFindBox);
			this.Controls.Add(this.ExportCountryCodeFindBox);
			this.Controls.Add(this.ArrivalCountryCodeFindBox);
			this.Controls.Add(this.PaymentMethodDropEdit);
			this.Controls.Add(this.AccountantTextBox);
			this.Controls.Add(this.AccountantVATTextBox);
			this.Controls.Add(this.OtherValueCalcFindBox);
			this.Controls.Add(this.ContainerNumberDropEdit);
			this.Controls.Add(this.GoodsValueConvertToLocalCurrencyControl);
			this.Name = "ETradeBillDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(380, 91, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PrecedentFreightCostLocalCurrencyControl.ResumeLayout(true);
			this.PrecedentFreightCostLocalCurrencyControl.PerformLayout();
			this.ShipmentTypeDropEdit.ResumeLayout(true);
			this.ShipmentTypeDropEdit.PerformLayout();
			this.NatureOfBusinessDropEdit.ResumeLayout(true);
			this.NatureOfBusinessDropEdit.PerformLayout();
			this.ExemptionCode1DropEdit.ResumeLayout(true);
			this.ExemptionCode1DropEdit.PerformLayout();
			this.ExemptionCode2DropEdit.ResumeLayout(true);
			this.ExemptionCode2DropEdit.PerformLayout();
			this.GuaranteeTypeDropEdit.ResumeLayout(true);
			this.GuaranteeTypeDropEdit.PerformLayout();
			this.DepartureCountryCodeFindBox.ResumeLayout(true);
			this.DepartureCountryCodeFindBox.PerformLayout();
			this.TradeCountryCodeFindBox.ResumeLayout(true);
			this.TradeCountryCodeFindBox.PerformLayout();
			this.ExportCountryCodeFindBox.ResumeLayout(true);
			this.ExportCountryCodeFindBox.PerformLayout();
			this.ArrivalCountryCodeFindBox.ResumeLayout(true);
			this.ArrivalCountryCodeFindBox.PerformLayout();
			this.PaymentMethodDropEdit.ResumeLayout(true);
			this.PaymentMethodDropEdit.PerformLayout();
			this.OtherValueCalcFindBox.ResumeLayout(true);
			this.OtherValueCalcFindBox.PerformLayout();
			this.ContainerNumberDropEdit.ResumeLayout(true);
			this.ContainerNumberDropEdit.PerformLayout();
			this.GoodsValueConvertToLocalCurrencyControl.ResumeLayout(true);
			this.GoodsValueConvertToLocalCurrencyControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.Customs.GUI.ConvertToLocalCurrencyControl PrecedentFreightCostLocalCurrencyControl;
		internal ZArchitecture.GUI.ZDropEdit ShipmentTypeDropEdit;
		internal ZArchitecture.GUI.ZDropEdit NatureOfBusinessDropEdit;
		internal ZArchitecture.GUI.ZDropEdit ExemptionCode1DropEdit;
		internal ZArchitecture.GUI.ZDropEdit ExemptionCode2DropEdit;
		internal ZArchitecture.GUI.ZDropEdit GuaranteeTypeDropEdit;
		internal ZArchitecture.ZTextBox GuaranteeRefNoTextBox;
		internal ZArchitecture.ZCalcEdit GuaranteeAmountCalcEdit;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox DepartureCountryCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox TradeCountryCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox ExportCountryCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox ArrivalCountryCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit PaymentMethodDropEdit;
		internal Enterprise.ZArchitecture.ZTextBox AccountantTextBox;
		internal Enterprise.ZArchitecture.ZTextBox AccountantVATTextBox;
		internal Customs.GUI.ConvertToLocalCurrencyControl OtherValueCalcFindBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit ContainerNumberDropEdit;
		internal Enterprise.Customs.GUI.ConvertToLocalCurrencyControl GoodsValueConvertToLocalCurrencyControl;
		internal ZArchitecture.GUI.ZCheckBox SeparatedCheckBox;
	}
}
