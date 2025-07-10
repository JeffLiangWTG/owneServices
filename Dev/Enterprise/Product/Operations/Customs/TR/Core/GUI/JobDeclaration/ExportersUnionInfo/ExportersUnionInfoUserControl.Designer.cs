namespace Enterprise.Customs.TR.GUI
{
	partial class ExportersUnionInfoUserControl
	{
		private void InitializeComponent()
		{
            this.ExportersUnionInfoOptionsSeparatorUserControl = new Enterprise.ZArchitecture.GUI.SeparatorUserControl();
            this.UnionSecretaryCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.UnionCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.ExportUnionCountryCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.InlandTransportTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.CurrentLoanCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.TotalPaymentCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.PaymentTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.UnionRecordNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.UnionApprovalCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.TPSReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.ExportUnionPaymentLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.ExportersUnionInfoOptionsSeparatorUserControl.SuspendLayout();
            this.UnionSecretaryCodeDropEdit.SuspendLayout();
            this.UnionCodeDropEdit.SuspendLayout();
            this.ExportUnionCountryCodeDropEdit.SuspendLayout();
            this.InlandTransportTypeDropEdit.SuspendLayout();
            this.PaymentTypeDropEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TR.Business.Declaration.JobDeclaration);
            // 
            // ExportersUnionInfoOptionsSeparatorUserControl
            // 
            this.ExportersUnionInfoOptionsSeparatorUserControl.AllowDrop = true;
            this.ExportersUnionInfoOptionsSeparatorUserControl.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("366e1464-1f01-44b4-9db4-16a07f62a57e", "Exporters\' Union Info");
            this.ExportersUnionInfoOptionsSeparatorUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(-14, 13, true);
            this.ExportersUnionInfoOptionsSeparatorUserControl.Name = "ExportersUnionInfoOptionsSeparatorUserControl";
            this.ExportersUnionInfoOptionsSeparatorUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(411, 15, true);
            this.ExportersUnionInfoOptionsSeparatorUserControl.TabIndex = 14;
            // 
            // UnionSecretaryCodeDropEdit
            // 
            this.UnionSecretaryCodeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.UnionSecretaryCodeDropEdit, "CusEntryInstruction.ZG_ExportUnionSecretaryCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).CusEntryInstruction.ZG_ExportUnionSecretaryCode)));
            this.UnionSecretaryCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(158, 33, true);
            this.UnionSecretaryCodeDropEdit.Name = "UnionSecretaryCodeDropEdit";
            this.UnionSecretaryCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 18, true);
            this.UnionSecretaryCodeDropEdit.TabIndex = 15;
            // 
            // UnionCodeDropEdit
            // 
            this.UnionCodeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.UnionCodeDropEdit, "CusEntryInstruction.ZG_ExportUnionCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).CusEntryInstruction.ZG_ExportUnionCode)));
            this.UnionCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 55, true);
            this.UnionCodeDropEdit.Name = "UnionCodeDropEdit";
            this.UnionCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 18, true);
            this.UnionCodeDropEdit.TabIndex = 16;
            // 
            // ExportUnionCountryCodeDropEdit
            // 
            this.ExportUnionCountryCodeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ExportUnionCountryCodeDropEdit, "CusEntryInstruction.ZG_ExportUnionCountryCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).CusEntryInstruction.ZG_ExportUnionCountryCode)));
            this.ExportUnionCountryCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 78, true);
            this.ExportUnionCountryCodeDropEdit.Name = "ExportUnionCountryCodeDropEdit";
            this.ExportUnionCountryCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 18, true);
            this.ExportUnionCountryCodeDropEdit.TabIndex = 17;
            // 
            // InlandTransportTypeDropEdit
            // 
            this.InlandTransportTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.InlandTransportTypeDropEdit, "CusEntryInstruction.ZG_InlandTransportType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).CusEntryInstruction.ZG_InlandTransportType)));
            this.InlandTransportTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 100, true);
            this.InlandTransportTypeDropEdit.Name = "InlandTransportTypeDropEdit";
            this.InlandTransportTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 18, true);
            this.InlandTransportTypeDropEdit.TabIndex = 18;
            // 
            // CurrentLoanCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.CurrentLoanCalcEdit, "CusEntryHeader.ExportUnionPayInfo.C9_PaymentAmount");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).CusEntryHeader.ExportUnionPayInfo.C9_PaymentAmount)));
            this.CurrentLoanCalcEdit.DecimalPlaces = 2;
            this.CurrentLoanCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 122, true);
            this.CurrentLoanCalcEdit.Name = "CurrentLoanCalcEdit";
            this.CurrentLoanCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 18, true);
            this.CurrentLoanCalcEdit.TabIndex = 19;
            this.CurrentLoanCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.CurrentLoanCalcEdit.TrackDisposedAccess = true;
            // 
            // TotalPaymentCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.TotalPaymentCalcEdit, "CusEntryHeader.ExportUnionCharges.C1_ChargeAmount");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).CusEntryHeader.ExportUnionCharges.C1_ChargeAmount)));
            this.TotalPaymentCalcEdit.DecimalPlaces = 2;
            this.TotalPaymentCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 145, true);
            this.TotalPaymentCalcEdit.Name = "TotalPaymentCalcEdit";
            this.TotalPaymentCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 18, true);
            this.TotalPaymentCalcEdit.TabIndex = 20;
            this.TotalPaymentCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.TotalPaymentCalcEdit.TrackDisposedAccess = true;
            // 
            // PaymentTypeDropEdit
            // 
            this.PaymentTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.PaymentTypeDropEdit, "CusEntryHeader.ExportUnionCharges.C1_MethodOfPayment");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).CusEntryHeader.ExportUnionCharges.C1_MethodOfPayment)));
            this.PaymentTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 167, true);
            this.PaymentTypeDropEdit.Name = "PaymentTypeDropEdit";
            this.PaymentTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 18, true);
            this.PaymentTypeDropEdit.TabIndex = 21;
            // 
            // UnionRecordNumberTextBox
            // 
            this.BindingSource.SetBindingMember(this.UnionRecordNumberTextBox, "CusEntryHeader.ExportUnionPayInfo.C9_PaymentReference");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).CusEntryHeader.ExportUnionPayInfo.C9_PaymentReference)));
            this.UnionRecordNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(158, 190, true);
            this.UnionRecordNumberTextBox.Name = "UnionRecordNumberTextBox";
            this.UnionRecordNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 18, true);
            this.UnionRecordNumberTextBox.TabIndex = 22;
            // 
            // UnionApprovalCodeTextBox
            // 
            this.BindingSource.SetBindingMember(this.UnionApprovalCodeTextBox, "CusEntryHeader.ExportUnionPayInfo.C9_IncomingPayResponseNo");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).CusEntryHeader.ExportUnionPayInfo.C9_IncomingPayResponseNo)));
            this.UnionApprovalCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 212, true);
            this.UnionApprovalCodeTextBox.Name = "UnionApprovalCodeTextBox";
            this.UnionApprovalCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 18, true);
            this.UnionApprovalCodeTextBox.TabIndex = 23;
            // 
            // TPSReferenceTextBox
            // 
            this.BindingSource.SetBindingMember(this.TPSReferenceTextBox, "CusEntryHeader.ExportUnionPayInfo.C9_BankAccount");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).CusEntryHeader.ExportUnionPayInfo.C9_BankAccount)));
            this.TPSReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 234, true);
            this.TPSReferenceTextBox.Name = "TPSReferenceTextBox";
            this.TPSReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 18, true);
            this.TPSReferenceTextBox.TabIndex = 24;
            // 
            // ExportUnionPaymentLinkLabel
            // 
            this.ExportUnionPaymentLinkLabel.AutoSize = true;
            this.ExportUnionPaymentLinkLabel.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("A1C5DC00-5862-493A-8637-5269C36136E3", "Export Union Payment");
            this.ExportUnionPaymentLinkLabel.IsFontBold = false;
            this.ExportUnionPaymentLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(259, 149, true);
            this.ExportUnionPaymentLinkLabel.Name = "ExportUnionPaymentLinkLabel";
            this.ExportUnionPaymentLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(118, 14, true);
            this.ExportUnionPaymentLinkLabel.TabIndex = 32;
            this.ExportUnionPaymentLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ExportUnionPaymentLinkLabel_LinkClicked);
            // 
            // ExportersUnionInfoUserControl
            // 
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.TPSReferenceTextBox);
            this.Controls.Add(this.UnionApprovalCodeTextBox);
            this.Controls.Add(this.UnionRecordNumberTextBox);
            this.Controls.Add(this.PaymentTypeDropEdit);
            this.Controls.Add(this.TotalPaymentCalcEdit);
            this.Controls.Add(this.CurrentLoanCalcEdit);
            this.Controls.Add(this.UnionCodeDropEdit);
            this.Controls.Add(this.UnionSecretaryCodeDropEdit);
            this.Controls.Add(this.ExportUnionCountryCodeDropEdit);
            this.Controls.Add(this.InlandTransportTypeDropEdit);
            this.Controls.Add(this.ExportersUnionInfoOptionsSeparatorUserControl);
            this.Controls.Add(this.ExportUnionPaymentLinkLabel);
            this.Name = "ExportersUnionInfoUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(406, 265, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ExportersUnionInfoOptionsSeparatorUserControl.ResumeLayout(true);
            this.ExportersUnionInfoOptionsSeparatorUserControl.PerformLayout();
            this.UnionSecretaryCodeDropEdit.ResumeLayout(true);
            this.UnionSecretaryCodeDropEdit.PerformLayout();
            this.UnionCodeDropEdit.ResumeLayout(true);
            this.UnionCodeDropEdit.PerformLayout();
            this.ExportUnionCountryCodeDropEdit.ResumeLayout(true);
            this.ExportUnionCountryCodeDropEdit.PerformLayout();
            this.InlandTransportTypeDropEdit.ResumeLayout(true);
            this.InlandTransportTypeDropEdit.PerformLayout();
            this.PaymentTypeDropEdit.ResumeLayout(true);
            this.PaymentTypeDropEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		internal ZArchitecture.GUI.SeparatorUserControl ExportersUnionInfoOptionsSeparatorUserControl;
		internal ZArchitecture.GUI.ZDropEdit UnionSecretaryCodeDropEdit;
		internal ZArchitecture.GUI.ZDropEdit UnionCodeDropEdit;
		internal ZArchitecture.GUI.ZDropEdit ExportUnionCountryCodeDropEdit;
		internal ZArchitecture.GUI.ZDropEdit InlandTransportTypeDropEdit;
		internal ZArchitecture.ZCalcEdit CurrentLoanCalcEdit;
		internal ZArchitecture.ZCalcEdit TotalPaymentCalcEdit;
		internal ZArchitecture.GUI.ZDropEdit PaymentTypeDropEdit;
		internal ZArchitecture.ZTextBox UnionRecordNumberTextBox;
		internal ZArchitecture.ZTextBox UnionApprovalCodeTextBox;
		internal ZArchitecture.ZTextBox TPSReferenceTextBox;
		internal ZArchitecture.GUI.ZLinkLabel ExportUnionPaymentLinkLabel;
	}
}
