namespace Enterprise.Customs.TR.NCTS.GUI
{
	partial class NctsPreviousDocumentsUserControl
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
			this.PrevTypeyCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.PaymentTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AmountCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.CountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.NatureOfBussinessDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GrossWeightrCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.DeclerationItemNoTextBox = new Enterprise.ZArchitecture.ZCalcEdit();
			this.BoxQuantityCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.NetWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.ExplanationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PrevDocsGroupBox.SuspendLayout();
			this.PrevDocsTypeDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PreviousDocumentsGrid)).BeginInit();
			this.PreviousDocumentsGrid.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PrevTypeyCodeFindBox.SuspendLayout();
			this.PaymentTypeDropEdit.SuspendLayout();
			this.AmountCalcDropEdit.SuspendLayout();
			this.CountryCodeFindBox.SuspendLayout();
			this.NatureOfBussinessDropEdit.SuspendLayout();
			this.GrossWeightrCalcDropEdit.SuspendLayout();
			this.NetWeightCalcDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// PrevDocsGroupBox
			// 
			this.PrevDocsGroupBox.Controls.Add(this.ExplanationTextBox);
			this.PrevDocsGroupBox.Controls.Add(this.NetWeightCalcDropEdit);
			this.PrevDocsGroupBox.Controls.Add(this.BoxQuantityCalcEdit);
			this.PrevDocsGroupBox.Controls.Add(this.DeclerationItemNoTextBox);
			this.PrevDocsGroupBox.Controls.Add(this.GrossWeightrCalcDropEdit);
			this.PrevDocsGroupBox.Controls.Add(this.NatureOfBussinessDropEdit);
			this.PrevDocsGroupBox.Controls.Add(this.CountryCodeFindBox);
			this.PrevDocsGroupBox.Controls.Add(this.AmountCalcDropEdit);
			this.PrevDocsGroupBox.Controls.Add(this.PaymentTypeDropEdit);
			this.PrevDocsGroupBox.Controls.Add(this.PrevTypeyCodeFindBox);
			this.PrevDocsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 235, true);
			this.PrevDocsGroupBox.Controls.SetChildIndex(this.PrevDocsTypeDropEdit, 0);
			this.PrevDocsGroupBox.Controls.SetChildIndex(this.PrevDocsReferenceTextBox, 0);
			this.PrevDocsGroupBox.Controls.SetChildIndex(this.PaymentTypeDropEdit, 0);
			this.PrevDocsGroupBox.Controls.SetChildIndex(this.PrevTypeyCodeFindBox, 0);
			this.PrevDocsGroupBox.Controls.SetChildIndex(this.AmountCalcDropEdit, 0);
			this.PrevDocsGroupBox.Controls.SetChildIndex(this.CountryCodeFindBox, 0);
			this.PrevDocsGroupBox.Controls.SetChildIndex(this.NatureOfBussinessDropEdit, 0);
			this.PrevDocsGroupBox.Controls.SetChildIndex(this.GrossWeightrCalcDropEdit, 0);
			this.PrevDocsGroupBox.Controls.SetChildIndex(this.DeclerationItemNoTextBox, 0);
			this.PrevDocsGroupBox.Controls.SetChildIndex(this.BoxQuantityCalcEdit, 0);
			this.PrevDocsGroupBox.Controls.SetChildIndex(this.NetWeightCalcDropEdit, 0);
			this.PrevDocsGroupBox.Controls.SetChildIndex(this.ExplanationTextBox, 0);
			// 
			// PrevDocsReferenceTextBox
			// 
			this.PrevDocsReferenceTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.PrevDocsReferenceTextBox, "PreviousDocuments.CSI_ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.NCTS.Business.NctsPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureCargoDesc)(null)).PreviousDocuments)).SyncRoot)).CSI_ReferenceNumber)));
			this.PrevDocsReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 36, true);
			this.PrevDocsReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 20, true);
			// 
			// PrevDocsTypeDropEdit
			// 
			this.PrevDocsTypeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.PrevDocsTypeDropEdit, "PreviousDocuments.Incoterm");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.NCTS.Business.NctsPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureCargoDesc)(null)).PreviousDocuments)).SyncRoot)).Incoterm)));
			this.PrevDocsTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 36, true);
			this.PrevDocsTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 20, true);
			this.PrevDocsTypeDropEdit.TabIndex = 7;
			// 
			// PreviousDocumentsGrid
			// 
			this.BindingSource.SetBindingMember(this.PreviousDocumentsGrid, "PreviousDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureCargoDesc)(null)).PreviousDocuments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.NCTS.Business.NctsPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureCargoDesc)(null)).PreviousDocuments)).SyncRoot)).CSI_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.NCTS.Business.NctsPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureCargoDesc)(null)).PreviousDocuments)).SyncRoot)).CSI_SubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.NCTS.Business.NctsPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureCargoDesc)(null)).PreviousDocuments)).SyncRoot)).CSI_ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.TR.NCTS.Business.NctsPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureCargoDesc)(null)).PreviousDocuments)).SyncRoot)).CSI_DateOfIssue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.NCTS.Business.NctsPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureCargoDesc)(null)).PreviousDocuments)).SyncRoot)).CSI_LineNo)));
			this.PreviousDocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 253, true);
			this.PreviousDocumentsGrid.TabIndex = 1;
			// 
			// TopPanel
			// 
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 253, true);
			// 
			// BottomPanel
			// 
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 253, true);
			this.BottomPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 95, true);
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 235, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TR.NCTS.Business.NctsDepartureCargoDesc);
			// 
			// PrevTypeyCodeFindBox
			// 
			this.PrevTypeyCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.PrevTypeyCodeFindBox, "PreviousDocuments.CSI_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.NCTS.Business.NctsPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureCargoDesc)(null)).PreviousDocuments)).SyncRoot)).CSI_Code)));
			this.PrevTypeyCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 12, true);
			this.PrevTypeyCodeFindBox.Name = "PrevTypeyCodeFindBox";
			this.PrevTypeyCodeFindBox.PreBoundMaxLength = 40;
			this.PrevTypeyCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 20, true);
			this.PrevTypeyCodeFindBox.TabIndex = 0;
			// 
			// PaymentTypeDropEdit
			// 
			this.PaymentTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PaymentTypeDropEdit, "PreviousDocuments.CSI_SubType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.NCTS.Business.NctsPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureCargoDesc)(null)).PreviousDocuments)).SyncRoot)).CSI_SubType)));
			this.PaymentTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 62, true);
			this.PaymentTypeDropEdit.Name = "PaymentTypeDropEdit";
			this.PaymentTypeDropEdit.PreBoundMaxLength = 4;
			this.PaymentTypeDropEdit.ShouldResizeByMaxLength = true;
			this.PaymentTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 20, true);
			this.PaymentTypeDropEdit.TabIndex = 8;
			// 
			// AmountCalcDropEdit
			// 
			this.AmountCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AmountCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.NCTS.Business.NctsPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureCargoDesc)(null)).PreviousDocuments)).SyncRoot)).CSI_Value)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.NCTS.Business.NctsPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureCargoDesc)(null)).PreviousDocuments)).SyncRoot)).CSI_RX_NKCurrency)));
			this.AmountCalcDropEdit.BindToAmount = "PreviousDocuments.CSI_Value";
			this.AmountCalcDropEdit.BindToUnit = "PreviousDocuments.CSI_RX_NKCurrency";
			this.AmountCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 88, true);
			this.AmountCalcDropEdit.Name = "AmountCalcDropEdit";
			this.AmountCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 20, true);
			this.AmountCalcDropEdit.TabIndex = 3;
			// 
			// CountryCodeFindBox
			// 
			this.CountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryCodeFindBox, "PreviousDocuments.CSI_RN_NKCountryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.NCTS.Business.NctsPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureCargoDesc)(null)).PreviousDocuments)).SyncRoot)).CSI_RN_NKCountryCode)));
			this.CountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 114, true);
			this.CountryCodeFindBox.Name = "CountryCodeFindBox";
			this.CountryCodeFindBox.PreBoundMaxLength = 4;
			this.CountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 20, true);
			this.CountryCodeFindBox.TabIndex = 10;
			// 
			// NatureOfBussinessDropEdit
			// 
			this.NatureOfBussinessDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NatureOfBussinessDropEdit, "PreviousDocuments.CSI_Procedure");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.NCTS.Business.NctsPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureCargoDesc)(null)).PreviousDocuments)).SyncRoot)).CSI_Procedure)));
			this.NatureOfBussinessDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 88, true);
			this.NatureOfBussinessDropEdit.Name = "NatureOfBussinessDropEdit";
			this.NatureOfBussinessDropEdit.PreBoundMaxLength = 4;
			this.NatureOfBussinessDropEdit.ShouldResizeByMaxLength = true;
			this.NatureOfBussinessDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 20, true);
			this.NatureOfBussinessDropEdit.TabIndex = 9;
			// 
			// GrossWeightrCalcDropEdit
			// 
			this.GrossWeightrCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GrossWeightrCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.NCTS.Business.NctsPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureCargoDesc)(null)).PreviousDocuments)).SyncRoot)).CSI_Quantity2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.NCTS.Business.NctsPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureCargoDesc)(null)).PreviousDocuments)).SyncRoot)).CSI_UnitOfQuantity2)));
			this.GrossWeightrCalcDropEdit.BindToAmount = "PreviousDocuments.CSI_Quantity2";
			this.GrossWeightrCalcDropEdit.BindToUnit = "PreviousDocuments.CSI_UnitOfQuantity2";
			this.GrossWeightrCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 140, true);
			this.GrossWeightrCalcDropEdit.Name = "GrossWeightrCalcDropEdit";
			this.GrossWeightrCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 20, true);
			this.GrossWeightrCalcDropEdit.TabIndex = 5;
			// 
			// DeclerationItemNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.DeclerationItemNoTextBox, "PreviousDocuments.CSI_LineNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.NCTS.Business.NctsPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureCargoDesc)(null)).PreviousDocuments)).SyncRoot)).CSI_LineNo)));
			this.DeclerationItemNoTextBox.CaptionResourceString = null;
			this.DeclerationItemNoTextBox.DecimalPlaces = 2;
			this.DeclerationItemNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 62, true);
			this.DeclerationItemNoTextBox.Name = "DeclerationItemNoTextBox";
			this.DeclerationItemNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 20, true);
			this.DeclerationItemNoTextBox.TabIndex = 2;
			this.DeclerationItemNoTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// BoxQuantityCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.BoxQuantityCalcEdit, "PreviousDocuments.CSI_Quantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.NCTS.Business.NctsPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureCargoDesc)(null)).PreviousDocuments)).SyncRoot)).CSI_Quantity)));
			this.BoxQuantityCalcEdit.CaptionResourceString = null;
			this.BoxQuantityCalcEdit.DecimalPlaces = 2;
			this.BoxQuantityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 114, true);
			this.BoxQuantityCalcEdit.Name = "BoxQuantityCalcEdit";
			this.BoxQuantityCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 20, true);
			this.BoxQuantityCalcEdit.TabIndex = 4;
			this.BoxQuantityCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// NetWeightCalcDropEdit
			// 
			this.NetWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NetWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.NCTS.Business.NctsPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureCargoDesc)(null)).PreviousDocuments)).SyncRoot)).CSI_Quantity3)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.NCTS.Business.NctsPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureCargoDesc)(null)).PreviousDocuments)).SyncRoot)).CSI_UnitOfQuantity3)));
			this.NetWeightCalcDropEdit.BindToAmount = "PreviousDocuments.CSI_Quantity3";
			this.NetWeightCalcDropEdit.BindToUnit = "PreviousDocuments.CSI_UnitOfQuantity3";
			this.NetWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 166, true);
			this.NetWeightCalcDropEdit.Name = "NetWeightCalcDropEdit";
			this.NetWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 20, true);
			this.NetWeightCalcDropEdit.TabIndex = 6;
			// 
			// ExplanationTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExplanationTextBox, "PreviousDocuments.CSI_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.NCTS.Business.NctsPreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureCargoDesc)(null)).PreviousDocuments)).SyncRoot)).CSI_Description)));
			this.ExplanationTextBox.CaptionResourceString = null;
			this.ExplanationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 140, true);
			this.ExplanationTextBox.Name = "ExplanationTextBox";
			this.ExplanationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 20, true);
			this.ExplanationTextBox.TabIndex = 11;
			// 
			// NctsPreviousDocumentsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "NctsPreviousDocumentsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 488, true);
			this.PrevDocsGroupBox.ResumeLayout(false);
			this.PrevDocsGroupBox.PerformLayout();
			this.PrevDocsTypeDropEdit.ResumeLayout(true);
			this.PrevDocsTypeDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PreviousDocumentsGrid)).EndInit();
			this.PreviousDocumentsGrid.ResumeLayout(false);
			this.PreviousDocumentsGrid.PerformLayout();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PrevTypeyCodeFindBox.ResumeLayout();
			this.PrevTypeyCodeFindBox.PerformLayout();
			this.PaymentTypeDropEdit.ResumeLayout(true);
			this.PaymentTypeDropEdit.PerformLayout();
			this.AmountCalcDropEdit.ResumeLayout(true);
			this.AmountCalcDropEdit.PerformLayout();
			this.CountryCodeFindBox.ResumeLayout(true);
			this.CountryCodeFindBox.PerformLayout();
			this.NatureOfBussinessDropEdit.ResumeLayout(true);
			this.NatureOfBussinessDropEdit.PerformLayout();
			this.GrossWeightrCalcDropEdit.ResumeLayout(true);
			this.GrossWeightrCalcDropEdit.PerformLayout();
			this.NetWeightCalcDropEdit.ResumeLayout(true);
			this.NetWeightCalcDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion
		private ZArchitecture.GUI.ZCodeFindBox PrevTypeyCodeFindBox;
		private ZArchitecture.GUI.ZDropEdit PaymentTypeDropEdit;
		private ZArchitecture.GUI.ZCalcDropEdit AmountCalcDropEdit;
		private ZArchitecture.GUI.ZCodeFindBox CountryCodeFindBox;
		private ZArchitecture.GUI.ZDropEdit NatureOfBussinessDropEdit;
		private ZArchitecture.GUI.ZCalcDropEdit GrossWeightrCalcDropEdit;
		private ZArchitecture.ZCalcEdit DeclerationItemNoTextBox;
		private ZArchitecture.ZTextBox ExplanationTextBox;
		private ZArchitecture.GUI.ZCalcDropEdit NetWeightCalcDropEdit;
		private ZArchitecture.ZCalcEdit BoxQuantityCalcEdit;
	}
}
