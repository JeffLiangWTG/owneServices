namespace Enterprise.Customs.TR.GUI
{
	partial class MiscOptionsUserControl
	{
		private void InitializeComponent()
		{
			this.ManifestToOpenUserControl = new Enterprise.Customs.TR.GUI.ManifestToOpenUserControl();
			this.ExportersUnionInfoUserControl = new Enterprise.Customs.TR.GUI.ExportersUnionInfoUserControl();
			this.BondTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DedicatedAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.GuaranteeGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.GuaranteeInfoOptionsSeparatorUserControl = new Enterprise.ZArchitecture.GUI.SeparatorUserControl();
			this.RatioCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.GuaranteeDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SupportingInformationUserControl = new Enterprise.Customs.TR.GUI.SupportingInformationControl();
			this.TotalAmountsSeparatorUserControl = new Enterprise.ZArchitecture.GUI.SeparatorUserControl();
			this.InvoiceCountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalFreeOnBoardLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			this.TotalFreightLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			this.TotalInsuranceLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			this.TotalOverseasLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			this.LocalTotalChargesLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			this.TotalInvoiceAmountLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ManifestToOpenUserControl.SuspendLayout();
			this.ExportersUnionInfoUserControl.SuspendLayout();
			this.BondTypeDropEdit.SuspendLayout();
			this.GuaranteeGuidFindBox.SuspendLayout();
			this.GuaranteeInfoOptionsSeparatorUserControl.SuspendLayout();
			this.SupportingInformationUserControl.SuspendLayout();
			this.TotalAmountsSeparatorUserControl.SuspendLayout();
			this.TotalFreeOnBoardLocalCurrencyControl.SuspendLayout();
			this.TotalFreightLocalCurrencyControl.SuspendLayout();
			this.TotalInsuranceLocalCurrencyControl.SuspendLayout();
			this.TotalOverseasLocalCurrencyControl.SuspendLayout();
			this.LocalTotalChargesLocalCurrencyControl.SuspendLayout();
			this.TotalInvoiceAmountLocalCurrencyControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TR.Business.Declaration.JobDeclaration);
			// 
			// ManifestToOpenUserControl
			// 
			this.ManifestToOpenUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ManifestToOpenUserControl, ".");
			this.ManifestToOpenUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(21, 228, true);
			this.ManifestToOpenUserControl.Name = "ManifestToOpenUserControl";
			this.ManifestToOpenUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(470, 359, true);
			this.ManifestToOpenUserControl.TabIndex = 0;
			// 
			// ExportersUnionInfoUserControl
			// 
			this.ExportersUnionInfoUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExportersUnionInfoUserControl, ".");
			this.ExportersUnionInfoUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(21, 228, true);
			this.ExportersUnionInfoUserControl.Name = "ExportersUnionInfoUserControl";
			this.ExportersUnionInfoUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(470, 250, true);
			this.ExportersUnionInfoUserControl.TabIndex = 0;
			// 
			// BondTypeDropEdit
			// 
			this.BondTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BondTypeDropEdit, "CusEntryInstruction.Guarantee.PW_BondType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).CusEntryInstruction.Guarantee.PW_BondType)));
			this.BondTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(173, 16, true);
			this.BondTypeDropEdit.Name = "BondTypeDropEdit";
			this.BondTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(254, 20, true);
			this.BondTypeDropEdit.TabIndex = 1;
			// 
			// DedicatedAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DedicatedAmountCalcEdit, "CusEntryInstruction.ZG_DedicatedGuaranteeAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).CusEntryInstruction.ZG_DedicatedGuaranteeAmount)));
			this.DedicatedAmountCalcEdit.DecimalPlaces = 2;
			this.DedicatedAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(173, 42, true);
			this.DedicatedAmountCalcEdit.Name = "DedicatedAmountCalcEdit";
			this.DedicatedAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 20, true);
			this.DedicatedAmountCalcEdit.TabIndex = 7;
			this.DedicatedAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.DedicatedAmountCalcEdit.TrackDisposedAccess = true;
			// 
			// GuaranteeGuidFindBox
			// 
			this.GuaranteeGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GuaranteeGuidFindBox, "CusEntryInstruction.Guarantee.PW_CPH_Guarantee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).CusEntryInstruction.Guarantee.PW_CPH_Guarantee)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).CusEntryInstruction.Guarantee.Lookups.GuaranteeList)));
			this.GuaranteeGuidFindBox.BindToList = "CusEntryInstruction.Guarantee.Lookups.GuaranteeList";
			this.GuaranteeGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(173, 68, true);
			this.GuaranteeGuidFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Guarantees;
			this.GuaranteeGuidFindBox.Name = "GuaranteeGuidFindBox";
			this.GuaranteeGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.GuaranteeGuidFindBox.ParentType = null;
			this.GuaranteeGuidFindBox.ShowDescriptionBox = false;
			this.GuaranteeGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.GuaranteeGuidFindBox.TabIndex = 8;
			// 
			// GuaranteeInfoOptionsSeparatorUserControl
			// 
			this.GuaranteeInfoOptionsSeparatorUserControl.AllowDrop = true;
			this.GuaranteeInfoOptionsSeparatorUserControl.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("41b6c901-cc06-49bf-bbd9-3b9eb13afe8e", "Guarantee Info");
			this.GuaranteeInfoOptionsSeparatorUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(173, 94, true);
			this.GuaranteeInfoOptionsSeparatorUserControl.Name = "GuaranteeInfoOptionsSeparatorUserControl";
			this.GuaranteeInfoOptionsSeparatorUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 15, true);
			this.GuaranteeInfoOptionsSeparatorUserControl.TabIndex = 9;
			// 
			// RatioCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.RatioCalcEdit, "CusEntryInstruction.ZG_GuaranteeRatio");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).CusEntryInstruction.ZG_GuaranteeRatio)));
			this.RatioCalcEdit.DecimalPlaces = 2;
			this.RatioCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(173, 115, true);
			this.RatioCalcEdit.Name = "RatioCalcEdit";
			this.RatioCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 20, true);
			this.RatioCalcEdit.TabIndex = 10;
			this.RatioCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.RatioCalcEdit.TrackDisposedAccess = true;
			// 
			// ReferenceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReferenceNumberTextBox, "CusEntryInstruction.Guarantee.PW_BondNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).CusEntryInstruction.Guarantee.PW_BondNumber)));
			this.ReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(173, 141, true);
			this.ReferenceNumberTextBox.Name = "ReferenceNumberTextBox";
			this.ReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(254, 20, true);
			this.ReferenceNumberTextBox.TabIndex = 11;
			// 
			// AmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.AmountCalcEdit, "CusEntryInstruction.Guarantee.PW_BondAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).CusEntryInstruction.Guarantee.PW_BondAmount)));
			this.AmountCalcEdit.DecimalPlaces = 2;
			this.AmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(173, 167, true);
			this.AmountCalcEdit.Name = "AmountCalcEdit";
			this.AmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 20, true);
			this.AmountCalcEdit.TabIndex = 12;
			this.AmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.AmountCalcEdit.TrackDisposedAccess = true;
			// 
			// GuaranteeDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.GuaranteeDescriptionTextBox, "CusEntryInstruction.Guarantee.PW_GuaranteeDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).CusEntryInstruction.Guarantee.PW_GuaranteeDescription)));
			this.GuaranteeDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(173, 193, true);
			this.GuaranteeDescriptionTextBox.Name = "GuaranteeDescriptionTextBox";
			this.GuaranteeDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(254, 20, true);
			this.GuaranteeDescriptionTextBox.TabIndex = 13;
			// 
			// SupportingInformationUserControl
			// 
			this.SupportingInformationUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupportingInformationUserControl, ".");
			this.SupportingInformationUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(554, 250, true);
			this.SupportingInformationUserControl.Name = "SupportingInformationUserControl";
			this.SupportingInformationUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(391, 311, true);
			this.SupportingInformationUserControl.TabIndex = 0;
			// 
			// TotalAmountsSeparatorUserControl
			// 
			this.TotalAmountsSeparatorUserControl.AllowDrop = true;
			this.TotalAmountsSeparatorUserControl.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("7273e6d9-8af1-4e4d-be03-56474f64882e", "Total Amounts");
			this.TotalAmountsSeparatorUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(21, 593, true);
			this.TotalAmountsSeparatorUserControl.Name = "TotalAmountsSeparatorUserControl";
			this.TotalAmountsSeparatorUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 15, true);
			this.TotalAmountsSeparatorUserControl.TabIndex = 25;
			// 
			// InvoiceCountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.InvoiceCountCalcEdit, "InvoiceCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).InvoiceCount)));
			this.InvoiceCountCalcEdit.DecimalPlaces = 2;
			this.InvoiceCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(21, 614, true);
			this.InvoiceCountCalcEdit.Name = "InvoiceCountCalcEdit";
			this.InvoiceCountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
			this.InvoiceCountCalcEdit.TabIndex = 26;
			this.InvoiceCountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.InvoiceCountCalcEdit.TrackDisposedAccess = true;
			// 
			// TotalFreeOnBoardLocalCurrencyControl
			// 
			this.TotalFreeOnBoardLocalCurrencyControl.AllowDrop = true;
			this.TotalFreeOnBoardLocalCurrencyControl.BindToAmount = "TotalFreeOnBoardAmount";
			this.TotalFreeOnBoardLocalCurrencyControl.BindToUnit = "TotalFreeOnBoardCurrency";
			this.TotalFreeOnBoardLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(21, 667, true);
			this.TotalFreeOnBoardLocalCurrencyControl.Name = "TotalFreeOnBoardLocalCurrencyControl";
			this.TotalFreeOnBoardLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
			this.TotalFreeOnBoardLocalCurrencyControl.TabIndex = 27;
			// 
			// TotalFreightLocalCurrencyControl
			// 
			this.TotalFreightLocalCurrencyControl.AllowDrop = true;
			this.TotalFreightLocalCurrencyControl.BindToAmount = "TotalFreightAmount";
			this.TotalFreightLocalCurrencyControl.BindToUnit = "TotalFreightCurrency";
			this.TotalFreightLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(21, 693, true);
			this.TotalFreightLocalCurrencyControl.Name = "TotalFreightLocalCurrencyControl";
			this.TotalFreightLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
			this.TotalFreightLocalCurrencyControl.TabIndex = 28;
			// 
			// TotalInsuranceLocalCurrencyControl
			// 
			this.TotalInsuranceLocalCurrencyControl.AllowDrop = true;
			this.TotalInsuranceLocalCurrencyControl.BindToAmount = "TotalInsuranceAmount";
			this.TotalInsuranceLocalCurrencyControl.BindToUnit = "TotalInsuranceCurrency";
			this.TotalInsuranceLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(21, 719, true);
			this.TotalInsuranceLocalCurrencyControl.Name = "TotalInsuranceLocalCurrencyControl";
			this.TotalInsuranceLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
			this.TotalInsuranceLocalCurrencyControl.TabIndex = 29;
			// 
			// TotalOverseasLocalCurrencyControl
			// 
			this.TotalOverseasLocalCurrencyControl.AllowDrop = true;
			this.TotalOverseasLocalCurrencyControl.BindToAmount = "TotalOverseasAmount";
			this.TotalOverseasLocalCurrencyControl.BindToUnit = "TotalOverseasCurrency";
			this.TotalOverseasLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(21, 745, true);
			this.TotalOverseasLocalCurrencyControl.Name = "TotalOverseasLocalCurrencyControl";
			this.TotalOverseasLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
			this.TotalOverseasLocalCurrencyControl.TabIndex = 30;
			// 
			// LocalTotalChargesLocalCurrencyControl
			// 
			this.LocalTotalChargesLocalCurrencyControl.AllowDrop = true;
			this.LocalTotalChargesLocalCurrencyControl.BindToAmount = "LocalTotalChargesAmount";
			this.LocalTotalChargesLocalCurrencyControl.BindToUnit = "LocalTotalChargesCurrency";
			this.LocalTotalChargesLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(21, 771, true);
			this.LocalTotalChargesLocalCurrencyControl.Name = "LocalTotalChargesLocalCurrencyControl";
			this.LocalTotalChargesLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
			this.LocalTotalChargesLocalCurrencyControl.TabIndex = 31;
			// 
			// TotalInvoiceAmountLocalCurrencyControl
			// 
			this.TotalInvoiceAmountLocalCurrencyControl.AllowDrop = true;
			this.TotalInvoiceAmountLocalCurrencyControl.BindToAmount = "TotalInvoiceAmount";
			this.TotalInvoiceAmountLocalCurrencyControl.BindToUnit = "TotalInvoiceCurrency";
			this.TotalInvoiceAmountLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(21, 641, true);
			this.TotalInvoiceAmountLocalCurrencyControl.Name = "TotalInvoiceAmountLocalCurrencyControl";
			this.TotalInvoiceAmountLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
			this.TotalInvoiceAmountLocalCurrencyControl.TabIndex = 27;
			// 
			// MiscOptionsUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SupportingInformationUserControl);
			this.Controls.Add(this.GuaranteeDescriptionTextBox);
			this.Controls.Add(this.AmountCalcEdit);
			this.Controls.Add(this.ReferenceNumberTextBox);
			this.Controls.Add(this.RatioCalcEdit);
			this.Controls.Add(this.GuaranteeInfoOptionsSeparatorUserControl);
			this.Controls.Add(this.GuaranteeGuidFindBox);
			this.Controls.Add(this.DedicatedAmountCalcEdit);
			this.Controls.Add(this.BondTypeDropEdit);
			this.Controls.Add(this.ManifestToOpenUserControl);
			this.Controls.Add(this.ExportersUnionInfoUserControl);
			this.Controls.Add(this.TotalAmountsSeparatorUserControl);
			this.Controls.Add(this.InvoiceCountCalcEdit);
			this.Controls.Add(this.TotalInvoiceAmountLocalCurrencyControl);
			this.Controls.Add(this.TotalFreeOnBoardLocalCurrencyControl);
			this.Controls.Add(this.TotalFreightLocalCurrencyControl);
			this.Controls.Add(this.TotalInsuranceLocalCurrencyControl);
			this.Controls.Add(this.TotalOverseasLocalCurrencyControl);
			this.Controls.Add(this.LocalTotalChargesLocalCurrencyControl);
			this.Name = "MiscOptionsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(961, 875, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ManifestToOpenUserControl.ResumeLayout(true);
			this.ManifestToOpenUserControl.PerformLayout();
			this.ExportersUnionInfoUserControl.ResumeLayout(true);
			this.ExportersUnionInfoUserControl.PerformLayout();
			this.BondTypeDropEdit.ResumeLayout(true);
			this.BondTypeDropEdit.PerformLayout();
			this.GuaranteeGuidFindBox.ResumeLayout(true);
			this.GuaranteeGuidFindBox.PerformLayout();
			this.GuaranteeInfoOptionsSeparatorUserControl.ResumeLayout(true);
			this.GuaranteeInfoOptionsSeparatorUserControl.PerformLayout();
			this.SupportingInformationUserControl.ResumeLayout(true);
			this.SupportingInformationUserControl.PerformLayout();
			this.TotalAmountsSeparatorUserControl.ResumeLayout(true);
			this.TotalAmountsSeparatorUserControl.PerformLayout();
			this.TotalFreeOnBoardLocalCurrencyControl.ResumeLayout(true);
			this.TotalFreeOnBoardLocalCurrencyControl.PerformLayout();
			this.TotalFreightLocalCurrencyControl.ResumeLayout(true);
			this.TotalFreightLocalCurrencyControl.PerformLayout();
			this.TotalInsuranceLocalCurrencyControl.ResumeLayout(true);
			this.TotalInsuranceLocalCurrencyControl.PerformLayout();
			this.TotalOverseasLocalCurrencyControl.ResumeLayout(true);
			this.TotalOverseasLocalCurrencyControl.PerformLayout();
			this.LocalTotalChargesLocalCurrencyControl.ResumeLayout(true);
			this.LocalTotalChargesLocalCurrencyControl.PerformLayout();
			this.TotalInvoiceAmountLocalCurrencyControl.ResumeLayout(true);
			this.TotalInvoiceAmountLocalCurrencyControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal Enterprise.Customs.TR.GUI.ManifestToOpenUserControl ManifestToOpenUserControl;
		internal Enterprise.Customs.TR.GUI.ExportersUnionInfoUserControl ExportersUnionInfoUserControl;
		internal ZArchitecture.GUI.ZDropEdit BondTypeDropEdit;
		internal ZArchitecture.ZCalcEdit DedicatedAmountCalcEdit;
		internal ZArchitecture.GUI.ZGuidFindBox GuaranteeGuidFindBox;
		internal ZArchitecture.GUI.SeparatorUserControl GuaranteeInfoOptionsSeparatorUserControl;
		internal ZArchitecture.ZCalcEdit RatioCalcEdit;
		internal ZArchitecture.ZTextBox ReferenceNumberTextBox;
		internal ZArchitecture.ZCalcEdit AmountCalcEdit;
		internal ZArchitecture.ZTextBox GuaranteeDescriptionTextBox;
		internal SupportingInformationControl SupportingInformationUserControl;
		internal ZArchitecture.GUI.SeparatorUserControl TotalAmountsSeparatorUserControl;
		internal ZArchitecture.ZCalcEdit InvoiceCountCalcEdit;
		internal Enterprise.Customs.GUI.ConvertToLocalCurrencyControl TotalInvoiceAmountLocalCurrencyControl;
		internal Enterprise.Customs.GUI.ConvertToLocalCurrencyControl TotalFreeOnBoardLocalCurrencyControl;
		internal Enterprise.Customs.GUI.ConvertToLocalCurrencyControl TotalFreightLocalCurrencyControl;
		internal Enterprise.Customs.GUI.ConvertToLocalCurrencyControl TotalInsuranceLocalCurrencyControl;
		internal Enterprise.Customs.GUI.ConvertToLocalCurrencyControl TotalOverseasLocalCurrencyControl;
		internal Enterprise.Customs.GUI.ConvertToLocalCurrencyControl LocalTotalChargesLocalCurrencyControl;
	}
}
