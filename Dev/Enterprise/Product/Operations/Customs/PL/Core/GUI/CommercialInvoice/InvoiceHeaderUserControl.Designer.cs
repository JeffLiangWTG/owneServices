
namespace Enterprise.Customs.PL.GUI
{
	partial class InvoiceHeaderUserControl
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
			this.JZ_IncoTermPlaceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AgreedPlaceCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ExportJZ_IncoTermDropDownEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ExportIncoTermExplainButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ValuationMethodDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JZ_ValuationCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.InvoiceDateDateEdit.SuspendLayout();
			this.RightTabControl.SuspendLayout();
			this.CustomFieldsTabPage.SuspendLayout();
			this.OrdersTabPage.SuspendLayout();
			this.ChargesGroupBox.SuspendLayout();
			this.ImporterOrganisationControl.SuspendLayout();
			this.SupplierOrganisationControl.SuspendLayout();
			this.GrossWeightCalcDropEdit.SuspendLayout();
			this.NetWeightCalcDropEdit.SuspendLayout();
			this.JZ_InvoiceAmountCurrencyControl.SuspendLayout();
			this.JZ_IncoTermDropDownEdit.SuspendLayout();
			this.BranchGuidFindBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceChargesGrid)).BeginInit();
			this.InvoiceChargesGrid.SuspendLayout();
			this.ShipmentTypeGroupBox.SuspendLayout();
			this.JE_MessageTypeDropDownEdit.SuspendLayout();
			this.DetailsGroupBox.SuspendLayout();
			this.InvCustomFieldsUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AgreedPlaceCodeFindBox.SuspendLayout();
			this.ExportJZ_IncoTermDropDownEdit.SuspendLayout();
			this.ValuationMethodDropEdit.SuspendLayout();
			this.JZ_ValuationCodeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// CustomFieldsTabPage
			// 
			this.CustomFieldsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.CustomFieldsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(323, 449, true);
			// 
			// OrdersTabPage
			// 
			this.OrdersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.OrdersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(323, 449, true);
			// 
			// JZ_InvoiceCurrExRateCalcEdit
			// 
			this.JZ_InvoiceCurrExRateCalcEdit.Visible = false;
			// 
			// GrossWeightCalcDropEdit
			// 
			this.GrossWeightCalcDropEdit.TabIndex = 8;
			// 
			// NetWeightCalcDropEdit
			// 
			this.NetWeightCalcDropEdit.TabIndex = 9;
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.Controls.Add(this.JZ_ValuationCodeDropEdit);
			this.DetailsGroupBox.Controls.Add(this.ValuationMethodDropEdit);
			this.DetailsGroupBox.Controls.Add(this.ExportIncoTermExplainButton);
			this.DetailsGroupBox.Controls.Add(this.AgreedPlaceCodeFindBox);
			this.DetailsGroupBox.Controls.Add(this.JZ_IncoTermPlaceTextBox);
			this.DetailsGroupBox.Controls.Add(this.ExportJZ_IncoTermDropDownEdit);
			this.DetailsGroupBox.Controls.SetChildIndex(this.ExportJZ_IncoTermDropDownEdit, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.NetWeightCalcDropEdit, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.InvoiceDateDateEdit, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.JZ_InvoiceNumberTextBox, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.BranchGuidFindBox, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.JZ_IncoTermDropDownEdit, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.JZ_InvoiceAmountCurrencyControl, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.GrossWeightCalcDropEdit, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.JZ_InvoiceCurrExRateCalcEdit, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.IncoTermExplainButton, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.JZ_IncoTermPlaceTextBox, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.AgreedPlaceCodeFindBox, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.ExportIncoTermExplainButton, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.ValuationMethodDropEdit, 0);
			this.DetailsGroupBox.Controls.SetChildIndex(this.JZ_ValuationCodeDropEdit, 0);
			// 
			// InvCustomFieldsUserControl
			// 
			this.InvCustomFieldsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(316, 426, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.PL.Business.Declaration.JobDeclaration);
			// 
			// JZ_IncoTermPlaceTextBox
			// 
			this.BindingSource.SetBindingMember(this.JZ_IncoTermPlaceTextBox, "Invoices.JZ_IncoTermPlace");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.PL.Business.Declaration.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.PL.Business.Declaration.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_IncoTermPlace)));
			this.JZ_IncoTermPlaceTextBox.CaptionResourceString = null;
			this.JZ_IncoTermPlaceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(266, 121, true);
			this.JZ_IncoTermPlaceTextBox.Name = "JZ_IncoTermPlaceTextBox";
			this.JZ_IncoTermPlaceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 18, true);
			this.JZ_IncoTermPlaceTextBox.TabIndex = 6;
			// 
			// AgreedPlaceCodeFindBox
			// 
			this.AgreedPlaceCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AgreedPlaceCodeFindBox, "Invoices.ZG_AgreedPlaceCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.PL.Business.Declaration.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.PL.Business.Declaration.JobDeclaration)(null)).Invoices)).SyncRoot)).ZG_AgreedPlaceCode)));
			this.AgreedPlaceCodeFindBox.CaptionResourceString = Enterprise.Customs.PL.GUI.Res.GetData("74C693C5-A550-41BA-B7AC-B2223C272D37", "Incoterm Place Code");
			this.AgreedPlaceCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 145, true);
			this.AgreedPlaceCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.AgreedPlaceCodeFindBox.Name = "AgreedPlaceCodeFindBox";
			this.AgreedPlaceCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.AgreedPlaceCodeFindBox.ParentType = null;
			this.AgreedPlaceCodeFindBox.PreBoundMaxLength = 5;
			this.AgreedPlaceCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 18, true);
			this.AgreedPlaceCodeFindBox.TabIndex = 7;
			// 
			// ExportJZ_IncoTermDropDownEdit
			// 
			this.ExportJZ_IncoTermDropDownEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExportJZ_IncoTermDropDownEdit, "Invoices.JZ_IncoTerm");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.PL.Business.Declaration.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.PL.Business.Declaration.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_IncoTerm)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.PL.Business.Declaration.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.PL.Business.Declaration.JobDeclaration)(null)).Invoices)).SyncRoot)).Lookups.JZ_IncoTerm_List)));
			this.ExportJZ_IncoTermDropDownEdit.BindToList = "Invoices.Lookups.JZ_IncoTerm_List";
			this.ExportJZ_IncoTermDropDownEdit.CaptionResourceString = Enterprise.Customs.PL.GUI.Res.GetData("PLInvoiceHeaderUserControl|8C8CAA6C-3EBA-40FA-864E-AFB33F230A4A", "Invoice Incoterm");
			this.ExportJZ_IncoTermDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 121, true);
			this.ExportJZ_IncoTermDropDownEdit.Name = "ExportJZ_IncoTermDropDownEdit";
			this.ExportJZ_IncoTermDropDownEdit.PreBoundMaxLength = 3;
			this.ExportJZ_IncoTermDropDownEdit.ShowDescriptionBox = false;
			this.ExportJZ_IncoTermDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 18, true);
			this.ExportJZ_IncoTermDropDownEdit.TabIndex = 5;
			// 
			// ExportIncoTermExplainButton
			// 
			this.ExportIncoTermExplainButton.IsCaptionOverridden = true;
			this.ExportIncoTermExplainButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(185, 119, true);
			this.ExportIncoTermExplainButton.Name = "ExportIncoTermExplainButton";
			this.ExportIncoTermExplainButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 21, true);
			this.ExportIncoTermExplainButton.TabIndex = 6;
			this.ExportIncoTermExplainButton.Text = "...";
			this.ExportIncoTermExplainButton.ToolTipCaption = null;
			this.ExportIncoTermExplainButton.Click += new System.EventHandler(this.ExportIncoTermExplainButton_Click);
			// 
			// ValuationMethodDropEdit
			// 
			this.ValuationMethodDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ValuationMethodDropEdit, "Invoices.ZG_ValuationMethod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.PL.Business.Declaration.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.PL.Business.Declaration.JobDeclaration)(null)).Invoices)).SyncRoot)).ZG_ValuationMethod)));
			this.ValuationMethodDropEdit.CaptionResourceString = Enterprise.Customs.PL.GUI.Res.GetData("c5c6dbc8-d16f-44c3-99b1-ed05d6f5551e", "Valuation Method");
			this.ValuationMethodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 215, true);
			this.ValuationMethodDropEdit.Name = "ValuationMethodDropEdit";
			this.ValuationMethodDropEdit.PreBoundMaxLength = 3;
			this.ValuationMethodDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 18, true);
			this.ValuationMethodDropEdit.TabIndex = 10;
			// 
			// JZ_ValuationCodeDropEdit
			// 
			this.JZ_ValuationCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JZ_ValuationCodeDropEdit, "Invoices.JZ_ValuationCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.PL.Business.Declaration.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.PL.Business.Declaration.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_ValuationCode)));
			this.JZ_ValuationCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 238, true);
			this.JZ_ValuationCodeDropEdit.Name = "JZ_ValuationCodeDropEdit";
			this.JZ_ValuationCodeDropEdit.PreBoundMaxLength = 2;
			this.JZ_ValuationCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 18, true);
			this.JZ_ValuationCodeDropEdit.TabIndex = 11;
			// 
			// InvoiceHeaderUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "InvoiceHeaderUserControl";
			this.InvoiceDateDateEdit.ResumeLayout(true);
			this.InvoiceDateDateEdit.PerformLayout();
			this.RightTabControl.ResumeLayout(false);
			this.RightTabControl.PerformLayout();
			this.CustomFieldsTabPage.ResumeLayout(false);
			this.CustomFieldsTabPage.PerformLayout();
			this.OrdersTabPage.ResumeLayout(false);
			this.OrdersTabPage.PerformLayout();
			this.ChargesGroupBox.ResumeLayout(false);
			this.ChargesGroupBox.PerformLayout();
			this.ImporterOrganisationControl.ResumeLayout(true);
			this.ImporterOrganisationControl.PerformLayout();
			this.SupplierOrganisationControl.ResumeLayout(true);
			this.SupplierOrganisationControl.PerformLayout();
			this.GrossWeightCalcDropEdit.ResumeLayout(true);
			this.GrossWeightCalcDropEdit.PerformLayout();
			this.NetWeightCalcDropEdit.ResumeLayout(true);
			this.NetWeightCalcDropEdit.PerformLayout();
			this.JZ_InvoiceAmountCurrencyControl.ResumeLayout(true);
			this.JZ_InvoiceAmountCurrencyControl.PerformLayout();
			this.JZ_IncoTermDropDownEdit.ResumeLayout(true);
			this.JZ_IncoTermDropDownEdit.PerformLayout();
			this.BranchGuidFindBox.ResumeLayout(true);
			this.BranchGuidFindBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceChargesGrid)).EndInit();
			this.InvoiceChargesGrid.ResumeLayout(false);
			this.InvoiceChargesGrid.PerformLayout();
			this.ShipmentTypeGroupBox.ResumeLayout(false);
			this.ShipmentTypeGroupBox.PerformLayout();
			this.JE_MessageTypeDropDownEdit.ResumeLayout(true);
			this.JE_MessageTypeDropDownEdit.PerformLayout();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.InvCustomFieldsUserControl.ResumeLayout(true);
			this.InvCustomFieldsUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AgreedPlaceCodeFindBox.ResumeLayout(true);
			this.AgreedPlaceCodeFindBox.PerformLayout();
			this.ExportJZ_IncoTermDropDownEdit.ResumeLayout(true);
			this.ExportJZ_IncoTermDropDownEdit.PerformLayout();
			this.ValuationMethodDropEdit.ResumeLayout(true);
			this.ValuationMethodDropEdit.PerformLayout();
			this.JZ_ValuationCodeDropEdit.ResumeLayout(true);
			this.JZ_ValuationCodeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.ZTextBox JZ_IncoTermPlaceTextBox;
		private ZArchitecture.GUI.ZCodeFindBox AgreedPlaceCodeFindBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit ExportJZ_IncoTermDropDownEdit;
		private ZArchitecture.GUI.ZButton ExportIncoTermExplainButton;
		private ZArchitecture.GUI.ZDropEdit ValuationMethodDropEdit;
		private ZArchitecture.GUI.ZDropEdit JZ_ValuationCodeDropEdit;
	}
}
