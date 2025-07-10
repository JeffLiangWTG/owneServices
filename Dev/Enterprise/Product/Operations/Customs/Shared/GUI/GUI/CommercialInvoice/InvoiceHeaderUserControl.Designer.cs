using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	partial class InvoiceHeaderUserControl
	{
		protected ZDateEdit InvoiceDateDateEdit;

		#region Windows Forms Designer Generated
		protected Enterprise.ZArchitecture.ZCalcEdit JZ_InvoiceCurrExRateCalcEdit;
		protected Enterprise.ZArchitecture.GUI.ZCalcDropEdit GrossWeightCalcDropEdit;
		protected Enterprise.ZArchitecture.GUI.ZCalcDropEdit NetWeightCalcDropEdit;
		protected Enterprise.Customs.GUI.ConvertToLocalCurrencyControl JZ_InvoiceAmountCurrencyControl;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit JZ_IncoTermDropDownEdit;
		protected Enterprise.ZArchitecture.GUI.ZGuidFindBox BranchGuidFindBox;
		protected Enterprise.ZArchitecture.ZTextBox JZ_InvoiceNumberTextBox;
		protected Enterprise.ZArchitecture.GUI.ZButton IncoTermExplainButton;

		void InitializeComponent()
		{
			this.InvoiceDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.IncoTermExplainButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.JZ_InvoiceCurrExRateCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.NetWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.GrossWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.JZ_InvoiceAmountCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			this.JZ_IncoTermDropDownEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.BranchGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.JZ_InvoiceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RightTabControl.SuspendLayout();
			this.CustomFieldsTabPage.SuspendLayout();
			this.OrdersTabPage.SuspendLayout();
			this.ordersAttachUserControl.SuspendLayout();
			this.ChargesGroupBox.SuspendLayout();
			this.ImporterOrganisationControl.SuspendLayout();
			this.SupplierOrganisationControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceChargesGrid)).BeginInit();
			this.InvoiceChargesGrid.SuspendLayout();
			this.ShipmentTypeGroupBox.SuspendLayout();
			this.JE_MessageTypeDropDownEdit.SuspendLayout();
			this.InvCustomFieldsUserControl.SuspendLayout();
			this.DetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.InvoiceDateDateEdit.SuspendLayout();
			this.NetWeightCalcDropEdit.SuspendLayout();
			this.GrossWeightCalcDropEdit.SuspendLayout();
			this.JZ_InvoiceAmountCurrencyControl.SuspendLayout();
			this.JZ_IncoTermDropDownEdit.SuspendLayout();
			this.BranchGuidFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.Controls.Add(this.InvoiceDateDateEdit);
			this.DetailsGroupBox.Controls.Add(this.IncoTermExplainButton);
			this.DetailsGroupBox.Controls.Add(this.JZ_InvoiceCurrExRateCalcEdit);
			this.DetailsGroupBox.Controls.Add(this.NetWeightCalcDropEdit);
			this.DetailsGroupBox.Controls.Add(this.GrossWeightCalcDropEdit);
			this.DetailsGroupBox.Controls.Add(this.JZ_InvoiceAmountCurrencyControl);
			this.DetailsGroupBox.Controls.Add(this.JZ_IncoTermDropDownEdit);
			this.DetailsGroupBox.Controls.Add(this.BranchGuidFindBox);
			this.DetailsGroupBox.Controls.Add(this.JZ_InvoiceNumberTextBox);
			// 
			// InvoiceDateDateEdit
			// 
			this.InvoiceDateDateEdit.AllowDrop = true;
			this.InvoiceDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.InvoiceDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.InvoiceDateDateEdit, "Invoices.JZ_InvoiceDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Invoices)).SyncRoot)).JZ_InvoiceDate)));
			this.InvoiceDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 47, true);
			this.InvoiceDateDateEdit.Name = "InvoiceDateDateEdit";
			this.InvoiceDateDateEdit.TabIndex = 1;
			// 
			// IncoTermExplainButton
			// 
			this.IncoTermExplainButton.IsCaptionOverridden = true;
			this.IncoTermExplainButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(318, 144, true);
			this.IncoTermExplainButton.Name = "IncoTermExplainButton";
			this.IncoTermExplainButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.IncoTermExplainButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 21, true);
			this.IncoTermExplainButton.TabIndex = 6;
			this.IncoTermExplainButton.Text = "...";
			this.IncoTermExplainButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.IncoTermExplainButton.ToolTipCaption = null;
			this.IncoTermExplainButton.Click += new System.EventHandler(this.IncoTermExplainButton_Click);
			// 
			// JZ_InvoiceCurrExRateCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.JZ_InvoiceCurrExRateCalcEdit, "Invoices.JZ_InvoiceCurrExRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Invoices)).SyncRoot)).JZ_InvoiceCurrExRate)));
			this.JZ_InvoiceCurrExRateCalcEdit.CaptionResourceString = null;
			this.JZ_InvoiceCurrExRateCalcEdit.DecimalPlaces = 2;
			this.JZ_InvoiceCurrExRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 121, true);
			this.JZ_InvoiceCurrExRateCalcEdit.Name = "JZ_InvoiceCurrExRateCalcEdit";
			this.JZ_InvoiceCurrExRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 25, true);
			this.JZ_InvoiceCurrExRateCalcEdit.TabIndex = 4;
			this.JZ_InvoiceCurrExRateCalcEdit.Text = "0.000000";
			this.JZ_InvoiceCurrExRateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// NetWeightCalcDropEdit
			// 
			this.NetWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NetWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Invoices)).SyncRoot)).JZ_NetWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Invoices)).SyncRoot)).JZ_NetWeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Invoices)).SyncRoot)).Lookups.JZ_WeightUQ_List)));
			this.NetWeightCalcDropEdit.BindToAmount = "Invoices.JZ_NetWeight";
			this.NetWeightCalcDropEdit.BindToList = "Invoices.Lookups.JZ_WeightUQ_List";
			this.NetWeightCalcDropEdit.BindToUnit = "Invoices.JZ_NetWeightUQ";
			this.NetWeightCalcDropEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("InvoiceHeaderUserControl|5a7b2c8a-7b38-4b13-9e3b-462e812d3410", "Inv. Net Weight");
			this.NetWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 193, true);
			this.NetWeightCalcDropEdit.Name = "NetWeightCalcDropEdit";
			this.NetWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 25, true);
			this.NetWeightCalcDropEdit.TabIndex = 8;
			this.NetWeightCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// GrossWeightCalcDropEdit
			// 
			this.GrossWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GrossWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Invoices)).SyncRoot)).JZ_Weight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Invoices)).SyncRoot)).JZ_WeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Invoices)).SyncRoot)).Lookups.JZ_WeightUQ_List)));
			this.GrossWeightCalcDropEdit.BindToAmount = "Invoices.JZ_Weight";
			this.GrossWeightCalcDropEdit.BindToList = "Invoices.Lookups.JZ_WeightUQ_List";
			this.GrossWeightCalcDropEdit.BindToUnit = "Invoices.JZ_WeightUQ";
			this.GrossWeightCalcDropEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("InvoiceHeaderUserControl|3619c7c5-5268-41ac-8ad5-576bff13f982", "Inv. Gross Weight");
			this.GrossWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 169, true);
			this.GrossWeightCalcDropEdit.Name = "GrossWeightCalcDropEdit";
			this.GrossWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 25, true);
			this.GrossWeightCalcDropEdit.TabIndex = 7;
			this.GrossWeightCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// JZ_InvoiceAmountCurrencyControl
			// 
			this.JZ_InvoiceAmountCurrencyControl.AllowDrop = true;
			this.JZ_InvoiceAmountCurrencyControl.BindToAmount = "Invoices.JZ_InvoiceAmount";
			this.JZ_InvoiceAmountCurrencyControl.BindToList = "Lookups.CurrencyList";
			this.JZ_InvoiceAmountCurrencyControl.BindToUnit = "Invoices.JZ_RX_NKInvoice_Currency";
			this.JZ_InvoiceAmountCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.JZ_InvoiceAmountCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 97, true);
			this.JZ_InvoiceAmountCurrencyControl.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			this.JZ_InvoiceAmountCurrencyControl.Name = "JZ_InvoiceAmountCurrencyControl";
			this.JZ_InvoiceAmountCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 20, true);
			this.JZ_InvoiceAmountCurrencyControl.TabIndex = 3;
			// 
			// JZ_IncoTermDropDownEdit
			// 
			this.JZ_IncoTermDropDownEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JZ_IncoTermDropDownEdit, "Invoices.JZ_IncoTerm");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Invoices)).SyncRoot)).JZ_IncoTerm)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Invoices)).SyncRoot)).Lookups.JZ_IncoTerm_List)));
			this.JZ_IncoTermDropDownEdit.BindToList = "Invoices.Lookups.JZ_IncoTerm_List";
			this.JZ_IncoTermDropDownEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("InvoiceHeaderUserControl|6fec690b-ceb1-b4a9-4547-20e1672c11f3", "Invoice Incoterm");
			this.JZ_IncoTermDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 145, true);
			this.JZ_IncoTermDropDownEdit.Name = "JZ_IncoTermDropDownEdit";
			this.JZ_IncoTermDropDownEdit.PreBoundMaxLength = 3;
			this.JZ_IncoTermDropDownEdit.ShouldResizeByMaxLength = true;
			this.JZ_IncoTermDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 25, true);
			this.JZ_IncoTermDropDownEdit.TabIndex = 5;
			// 
			// BranchGuidFindBox
			// 
			this.BranchGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BranchGuidFindBox, "Invoices.JZ_GB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Invoices)).SyncRoot)).JZ_GB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Lookups.BranchCollection)));
			this.BranchGuidFindBox.BindToList = "Lookups.BranchCollection";
			this.BranchGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 73, true);
			this.BranchGuidFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbBranch;
			this.BranchGuidFindBox.Name = "BranchGuidFindBox";
			this.BranchGuidFindBox.PreBoundMaxLength = 3;
			this.BranchGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 25, true);
			this.BranchGuidFindBox.TabIndex = 2;
			// 
			// JZ_InvoiceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.JZ_InvoiceNumberTextBox, "Invoices.JZ_InvoiceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Invoices)).SyncRoot)).JZ_InvoiceNumber)));
			this.JZ_InvoiceNumberTextBox.CaptionResourceString = null;
			this.JZ_InvoiceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 21, true);
			this.JZ_InvoiceNumberTextBox.Name = "JZ_InvoiceNumberTextBox";
			this.JZ_InvoiceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 25, true);
			this.JZ_InvoiceNumberTextBox.TabIndex = 0;
			// 
			// InvoiceHeaderUserControl
			// 
			this.Name = "InvoiceHeaderUserControl";
			this.RightTabControl.ResumeLayout(false);
			this.RightTabControl.PerformLayout();
			this.CustomFieldsTabPage.ResumeLayout(false);
			this.CustomFieldsTabPage.PerformLayout();
			this.OrdersTabPage.ResumeLayout(false);
			this.OrdersTabPage.PerformLayout();
			this.ordersAttachUserControl.ResumeLayout(true);
			this.ordersAttachUserControl.PerformLayout();
			this.ChargesGroupBox.ResumeLayout(false);
			this.ChargesGroupBox.PerformLayout();
			this.ImporterOrganisationControl.ResumeLayout(true);
			this.ImporterOrganisationControl.PerformLayout();
			this.SupplierOrganisationControl.ResumeLayout(true);
			this.SupplierOrganisationControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceChargesGrid)).EndInit();
			this.InvoiceChargesGrid.ResumeLayout(false);
			this.InvoiceChargesGrid.PerformLayout();
			this.ShipmentTypeGroupBox.ResumeLayout(false);
			this.ShipmentTypeGroupBox.PerformLayout();
			this.JE_MessageTypeDropDownEdit.ResumeLayout(true);
			this.JE_MessageTypeDropDownEdit.PerformLayout();
			this.InvCustomFieldsUserControl.ResumeLayout(true);
			this.InvCustomFieldsUserControl.PerformLayout();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.InvoiceDateDateEdit.ResumeLayout(true);
			this.InvoiceDateDateEdit.PerformLayout();
			this.NetWeightCalcDropEdit.ResumeLayout(true);
			this.NetWeightCalcDropEdit.PerformLayout();
			this.GrossWeightCalcDropEdit.ResumeLayout(true);
			this.GrossWeightCalcDropEdit.PerformLayout();
			this.JZ_InvoiceAmountCurrencyControl.ResumeLayout(true);
			this.JZ_InvoiceAmountCurrencyControl.PerformLayout();
			this.JZ_IncoTermDropDownEdit.ResumeLayout(true);
			this.JZ_IncoTermDropDownEdit.PerformLayout();
			this.BranchGuidFindBox.ResumeLayout(true);
			this.BranchGuidFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion
	}
}
