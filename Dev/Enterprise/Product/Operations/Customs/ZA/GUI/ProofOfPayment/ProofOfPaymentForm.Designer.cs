using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.GUI
{
	public partial class ProofOfPaymentForm
	{


		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
            this.importerFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
            this.fANumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.paymentDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.customsOfficeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.lRNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.mRNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.paymentAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.paymentReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.totalVatCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.saveButtonUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
            this.receiptDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.importerFindBox.SuspendLayout();
            this.paymentDateEdit.SuspendLayout();
            this.customsOfficeDropEdit.SuspendLayout();
            this.saveButtonUserControl.SuspendLayout();
            this.receiptDateEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 282, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(383, 24, true);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ZA.Business.CusEntryPayInfo);
            // 
            // importerFindBox
            // 
            this.importerFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.importerFindBox, "Importer+PK");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.ZA.Business.CusEntryPayInfo)(null)).Importer.PK)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryPayInfo)(null)).Lookups.Importers)));
            this.importerFindBox.BindToList = "Lookups.Importers";
            this.importerFindBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("ProofOfPaymentForm|76D25EA2-E09E-42EF-BEC7-E8132458FA74", "Importer");
            this.importerFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(153, 15, true);
            this.importerFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
            this.importerFindBox.Name = "importerFindBox";
            this.importerFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.importerFindBox.ParentType = null;
            this.importerFindBox.ShowDescriptionBox = false;
            this.importerFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
            this.importerFindBox.TabIndex = 0;
            // 
            // fANumberTextBox
            // 
            this.BindingSource.SetBindingMember(this.fANumberTextBox, "FANumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CusEntryPayInfo)(null)).FANumber)));
            this.fANumberTextBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("ProofOfPaymentForm|BC6A8EFF-AA07-45C3-9E3F-05622EC00BB8", "Financial Account Number");
            this.fANumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(153, 37, true);
            this.fANumberTextBox.Name = "fANumberTextBox";
            this.fANumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
            this.fANumberTextBox.TabIndex = 1;
            // 
            // paymentDateEdit
            // 
            this.paymentDateEdit.AllowDrop = true;
            this.paymentDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.paymentDateEdit, "C9_PaymentDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.Business.CusEntryPayInfo)(null)).C9_PaymentDate)));
            this.paymentDateEdit.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("ProofOfPaymentForm|0627E100-D275-4377-88BB-EA0047CD8E56", "Transaction Date");
            this.paymentDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(153, 147, true);
            this.paymentDateEdit.Name = "paymentDateEdit";
            this.paymentDateEdit.TabIndex = 6;
            // 
            // customsOfficeDropEdit
            // 
            this.customsOfficeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.customsOfficeDropEdit, "CustomsOffice");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.Business.CusEntryPayInfo)(null)).CustomsOffice)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.CusEntryPayInfo)(null)).Lookups.CustomsOffices)));
            this.customsOfficeDropEdit.BindToList = "Lookups.CustomsOffices";
            this.customsOfficeDropEdit.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("ProofOfPaymentForm|7A4515A8-6033-4510-8675-2AF4D71B2CBC", "Customs Office");
            this.customsOfficeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(153, 59, true);
            this.customsOfficeDropEdit.Name = "customsOfficeDropEdit";
            this.customsOfficeDropEdit.ShowDescriptionBox = false;
            this.customsOfficeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
            this.customsOfficeDropEdit.TabIndex = 2;
            // 
            // lRNumberTextBox
            // 
            this.BindingSource.SetBindingMember(this.lRNumberTextBox, "LRNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CusEntryPayInfo)(null)).LRNumber)));
            this.lRNumberTextBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("ProofOfPaymentForm|A27EC900-E5D3-4CB2-81CA-3C1B17886B29", "Local Reference Number");
            this.lRNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(153, 81, true);
            this.lRNumberTextBox.Name = "lRNumberTextBox";
            this.lRNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 20, true);
            this.lRNumberTextBox.TabIndex = 3;
            // 
            // mRNumberTextBox
            // 
            this.BindingSource.SetBindingMember(this.mRNumberTextBox, "MRNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CusEntryPayInfo)(null)).MRNumber)));
            this.mRNumberTextBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("ProofOfPaymentForm|D792A5A7-B590-4595-BAF2-7F93CAF94A8B", "MRN");
            this.mRNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(153, 103, true);
            this.mRNumberTextBox.Name = "mRNumberTextBox";
            this.mRNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 20, true);
            this.mRNumberTextBox.TabIndex = 4;
            // 
            // paymentAmountCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.paymentAmountCalcEdit, "C9_PaymentAmount");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.CusEntryPayInfo)(null)).C9_PaymentAmount)));
            this.paymentAmountCalcEdit.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("ProofOfPaymentForm|47694DE8-7B68-414E-A254-5A03EBC99964", "Payment Amount");
            this.paymentAmountCalcEdit.DecimalPlaces = 2;
            this.paymentAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(153, 125, true);
            this.paymentAmountCalcEdit.Name = "paymentAmountCalcEdit";
            this.paymentAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
            this.paymentAmountCalcEdit.TabIndex = 5;
            this.paymentAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.paymentAmountCalcEdit.TrackDisposedAccess = true;
            // 
            // paymentReferenceTextBox
            // 
            this.BindingSource.SetBindingMember(this.paymentReferenceTextBox, "C9_PaymentReference");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CusEntryPayInfo)(null)).C9_PaymentReference)));
            this.paymentReferenceTextBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("ProofOfPaymentForm|C29EB126-AF48-4ABC-A51D-252504BC0913", "Receipt Number");
            this.paymentReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(153, 169, true);
            this.paymentReferenceTextBox.Name = "paymentReferenceTextBox";
            this.paymentReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 20, true);
            this.paymentReferenceTextBox.TabIndex = 7;
            // 
            // totalVatCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.totalVatCalcEdit, "TotalVat");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.CusEntryPayInfo)(null)).TotalVat)));
            this.totalVatCalcEdit.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("ProofOfPaymentForm|68395649-46B3-4189-8A9F-1D1495ADC17C", "Total VAT for Receipt");
            this.totalVatCalcEdit.DecimalPlaces = 2;
            this.totalVatCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(153, 231, true);
            this.totalVatCalcEdit.Name = "totalVatCalcEdit";
            this.totalVatCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 20, true);
            this.totalVatCalcEdit.TabIndex = 9;
            this.totalVatCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.totalVatCalcEdit.TrackDisposedAccess = true;
            // 
            // saveButtonUserControl
            // 
            this.saveButtonUserControl.AllowDrop = true;
            this.saveButtonUserControl.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.saveButtonUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 257, true);
            this.saveButtonUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
            this.saveButtonUserControl.Name = "saveButtonUserControl";
            this.saveButtonUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(383, 25, true);
            this.saveButtonUserControl.TabIndex = 10;
            // 
            // receiptDateEdit
            // 
            this.receiptDateEdit.AllowDrop = true;
            this.receiptDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.receiptDateEdit, "C9_ReceiptDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.Business.CusEntryPayInfo)(null)).C9_ReceiptDate)));
            this.receiptDateEdit.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("ProofOfPaymentForm|41855E86-C6C2-4D97-BEAE-149D8CB655D4", "Receipt Date");
            this.receiptDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(153, 191, true);
            this.receiptDateEdit.Name = "receiptDateEdit";
            this.receiptDateEdit.TabIndex = 8;
            // 
            // ProofOfPaymentForm
            // 
            this.AutoAddPreviousNextButtons = false;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("6513616e-d119-4833-96c9-b0b8c403681e", "VAT 404 - Proof of Payment");
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(383, 306, true);
            this.Controls.Add(this.totalVatCalcEdit);
            this.Controls.Add(this.receiptDateEdit);
            this.Controls.Add(this.saveButtonUserControl);
            this.Controls.Add(this.paymentReferenceTextBox);
            this.Controls.Add(this.paymentAmountCalcEdit);
            this.Controls.Add(this.mRNumberTextBox);
            this.Controls.Add(this.lRNumberTextBox);
            this.Controls.Add(this.customsOfficeDropEdit);
            this.Controls.Add(this.paymentDateEdit);
            this.Controls.Add(this.fANumberTextBox);
            this.Controls.Add(this.importerFindBox);
            this.DataSourceType = typeof(Enterprise.Customs.ZA.Business.CusEntryPayInfo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MinimizeBox = false;
            this.Name = "ProofOfPaymentForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "VAT 404 - Proof of Payment";
            this.TopMost = true;
            this.Controls.SetChildIndex(this.importerFindBox, 0);
            this.Controls.SetChildIndex(this.MainStatusBar, 0);
            this.Controls.SetChildIndex(this.fANumberTextBox, 0);
            this.Controls.SetChildIndex(this.paymentDateEdit, 0);
            this.Controls.SetChildIndex(this.customsOfficeDropEdit, 0);
            this.Controls.SetChildIndex(this.lRNumberTextBox, 0);
            this.Controls.SetChildIndex(this.mRNumberTextBox, 0);
            this.Controls.SetChildIndex(this.paymentAmountCalcEdit, 0);
            this.Controls.SetChildIndex(this.paymentReferenceTextBox, 0);
            this.Controls.SetChildIndex(this.saveButtonUserControl, 0);
            this.Controls.SetChildIndex(this.receiptDateEdit, 0);
            this.Controls.SetChildIndex(this.totalVatCalcEdit, 0);
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.importerFindBox.ResumeLayout(true);
            this.importerFindBox.PerformLayout();
            this.paymentDateEdit.ResumeLayout(true);
            this.paymentDateEdit.PerformLayout();
            this.customsOfficeDropEdit.ResumeLayout(true);
            this.customsOfficeDropEdit.PerformLayout();
            this.saveButtonUserControl.ResumeLayout(true);
            this.saveButtonUserControl.PerformLayout();
            this.receiptDateEdit.ResumeLayout(true);
            this.receiptDateEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		ZArchitecture.ZTextBox fANumberTextBox;
		ZDateEdit paymentDateEdit;
		ZDropEdit customsOfficeDropEdit;
		ZArchitecture.ZTextBox lRNumberTextBox;
		ZArchitecture.ZTextBox mRNumberTextBox;
		ZArchitecture.ZCalcEdit paymentAmountCalcEdit;
		ZArchitecture.ZTextBox paymentReferenceTextBox;
		ZArchitecture.ZCalcEdit totalVatCalcEdit;
		MasterFiles.GUI.ZOrganisationFindBox importerFindBox;
		ZDateEdit receiptDateEdit;
		Core.Forms.ZPostingButtonsUserControl saveButtonUserControl;

		#endregion

	}
}
