
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.GUI
{
	partial class InvoiceHeaderAIIUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.TotalSubjectToGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DiscountValueCalcFindBox = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.ForeignTaxValueCalcFindBox = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.AIIOtherDetailsTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.RelatedDataTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DefaultRelatedDocumentsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AIIRelatedDocumentsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.AIIInvoiceDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.AIITermsOfDeliveryGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TermsOfDeliveryLocationCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.TermsOfDeliveryLocationIndicatorDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TermsOfDeliveryDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TermsOfDeliveryLocationScheduleDCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.TermsOfDeliveryLocationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.US_TermsOfDeliveryLocationScheduleKCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.AIIInvoiceTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AIIPaymentTermsDescTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AIIPaymentTermsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RelatedBillPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.RelatedBillGuidDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.DesExamSiteCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TotalSubjectToGroupBox.SuspendLayout();
			this.AIIOtherDetailsTabControl.SuspendLayout();
			this.RelatedDataTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AIIRelatedDocumentsGrid)).BeginInit();
			this.AIITermsOfDeliveryGroupBox.SuspendLayout();
			this.RelatedBillPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.IInvoicesProvider);
			// 
			// TotalSubjectToGroupBox
			// 
			this.TotalSubjectToGroupBox.Controls.Add(this.DiscountValueCalcFindBox);
			this.TotalSubjectToGroupBox.Controls.Add(this.ForeignTaxValueCalcFindBox);
			this.TotalSubjectToGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 104, true);
			this.TotalSubjectToGroupBox.Name = "TotalSubjectToGroupBox";
			this.TotalSubjectToGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(365, 66, true);
			this.TotalSubjectToGroupBox.TabIndex = 6;
			this.TotalSubjectToGroupBox.TabStop = false;
			this.TotalSubjectToGroupBox.Text = "Total Subject To";
			// 
			// DiscountValueCalcFindBox
			// 
			this.DiscountValueCalcFindBox.AllowDrop = true;
			this.DiscountValueCalcFindBox.BindToAmount = "FilteredInvoices.US_ValueForDiscount";
			this.DiscountValueCalcFindBox.BindToUnit = "FilteredInvoices.JZ_RX_NKInvoice_CurrencyReadOnly";
			this.DiscountValueCalcFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("InvoiceHeaderAIIUserControl|0bdf22ad-986b-4c0b-85e8-73b195b5e8bf", "Discount");
			this.DiscountValueCalcFindBox.Decimals = 0;
			this.DiscountValueCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.DiscountValueCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 19, true);
			this.DiscountValueCalcFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			this.DiscountValueCalcFindBox.Name = "DiscountValueCalcFindBox";
			this.DiscountValueCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(123, 20, true);
			this.DiscountValueCalcFindBox.TabIndex = 0;
			// 
			// ForeignTaxValueCalcFindBox
			// 
			this.ForeignTaxValueCalcFindBox.AllowDrop = true;
			this.ForeignTaxValueCalcFindBox.BindToAmount = "FilteredInvoices.US_ValueForForeignTax";
			this.ForeignTaxValueCalcFindBox.BindToUnit = "FilteredInvoices.JZ_RX_NKInvoice_CurrencyReadOnly";
			this.ForeignTaxValueCalcFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("InvoiceHeaderAIIUserControl|a114968f-357d-4b34-b2c3-4b7726e91f9c", "Foreign Tax");
			this.ForeignTaxValueCalcFindBox.Decimals = 0;
			this.ForeignTaxValueCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.ForeignTaxValueCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 42, true);
			this.ForeignTaxValueCalcFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			this.ForeignTaxValueCalcFindBox.Name = "ForeignTaxValueCalcFindBox";
			this.ForeignTaxValueCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(123, 20, true);
			this.ForeignTaxValueCalcFindBox.TabIndex = 1;
			// 
			// AIIOtherDetailsTabControl
			// 
			this.AIIOtherDetailsTabControl.Controls.Add(this.RelatedDataTabPage);
			this.AIIOtherDetailsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(378, 8, true);
			this.AIIOtherDetailsTabControl.Name = "AIIOtherDetailsTabControl";
			this.AIIOtherDetailsTabControl.SelectedIndex = 0;
			this.AIIOtherDetailsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(524, 279, true);
			this.AIIOtherDetailsTabControl.TabIndex = 9;
			// 
			// RelatedDataTabPage
			// 
			this.RelatedDataTabPage.Controls.Add(this.DefaultRelatedDocumentsButton);
			this.RelatedDataTabPage.Controls.Add(this.AIIRelatedDocumentsGrid);
			this.RelatedDataTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.RelatedDataTabPage.Name = "RelatedDataTabPage";
			this.RelatedDataTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(516, 252, true);
			this.RelatedDataTabPage.TabIndex = 2;
			this.RelatedDataTabPage.Text = "Related Document";
			// 
			// DefaultRelatedDocumentsButton
			// 
			this.DefaultRelatedDocumentsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.DefaultRelatedDocumentsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 226, true);
			this.DefaultRelatedDocumentsButton.Name = "DefaultRelatedDocumentsButton";
			this.DefaultRelatedDocumentsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 23, true);
			this.DefaultRelatedDocumentsButton.TabIndex = 1;
			this.DefaultRelatedDocumentsButton.Text = "Populate Default Values";
			this.DefaultRelatedDocumentsButton.UseVisualStyleBackColor = true;
			this.DefaultRelatedDocumentsButton.Click += new System.EventHandler(this.DefaultRelatedDocumentsButton_Click);
			// 
			// AIIRelatedDocumentsGrid
			// 
			this.AIIRelatedDocumentsGrid.AllowNavigation = false;
			this.AIIRelatedDocumentsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AIIRelatedDocumentsGrid, "FilteredInvoices.RelatedDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.IInvoicesProvider)(null)).FilteredInvoices)).SyncRoot)).RelatedDocuments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.RelatedDocument)(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.IInvoicesProvider)(null)).FilteredInvoices)).SyncRoot)).RelatedDocuments)).SyncRoot)).CY_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.RelatedDocument)(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.IInvoicesProvider)(null)).FilteredInvoices)).SyncRoot)).RelatedDocuments)).SyncRoot)).Lookups.CY_CodeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.RelatedDocument)(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.IInvoicesProvider)(null)).FilteredInvoices)).SyncRoot)).RelatedDocuments)).SyncRoot)).Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.RelatedDocument)(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.IInvoicesProvider)(null)).FilteredInvoices)).SyncRoot)).RelatedDocuments)).SyncRoot)).CY_Data)));
			this.AIIRelatedDocumentsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.BindToList = "Lookups+CY_CodeList";
			zDropEditColumnStyleInfo1.Caption = "Type";
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "CY_Code";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo1.Caption = "Description";
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "Description";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zTextBoxColumnStyleInfo2.Caption = "Number";
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "CY_Data";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			this.AIIRelatedDocumentsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.AIIRelatedDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AIIRelatedDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.AIIRelatedDocumentsGrid.GridId = "17421441-3287-4de1-861a-817dd0e5426c";
			this.AIIRelatedDocumentsGrid.CopySelectedRowsAllowed = true;
			this.AIIRelatedDocumentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AIIRelatedDocumentsGrid.LayoutKey = "AIIRelatedDocumentsGrid";
			this.AIIRelatedDocumentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 4, true);
			this.AIIRelatedDocumentsGrid.Name = "AIIRelatedDocumentsGrid";
			this.AIIRelatedDocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(510, 216, true);
			this.AIIRelatedDocumentsGrid.TabIndex = 0;
			// 
			// AIIInvoiceDateDateEdit
			// 
			this.AIIInvoiceDateDateEdit.AllowDrop = true;
			this.AIIInvoiceDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.AIIInvoiceDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.AIIInvoiceDateDateEdit, "FilteredInvoices.JZ_InvoiceDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.IInvoicesProvider)(null)).FilteredInvoices)).SyncRoot)).JZ_InvoiceDate)));
			this.AIIInvoiceDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(91, 3, true);
			this.AIIInvoiceDateDateEdit.Name = "AIIInvoiceDateDateEdit";
			this.AIIInvoiceDateDateEdit.TabIndex = 0;
			// 
			// AIITermsOfDeliveryGroupBox
			// 
			this.AIITermsOfDeliveryGroupBox.Controls.Add(this.TermsOfDeliveryLocationCountryCodeFindBox);
			this.AIITermsOfDeliveryGroupBox.Controls.Add(this.TermsOfDeliveryLocationIndicatorDropEdit);
			this.AIITermsOfDeliveryGroupBox.Controls.Add(this.TermsOfDeliveryDropEdit);
			this.AIITermsOfDeliveryGroupBox.Controls.Add(this.TermsOfDeliveryLocationScheduleDCodeFindBox);
			this.AIITermsOfDeliveryGroupBox.Controls.Add(this.TermsOfDeliveryLocationTextBox);
			this.AIITermsOfDeliveryGroupBox.Controls.Add(this.US_TermsOfDeliveryLocationScheduleKCodeFindBox);
			this.AIITermsOfDeliveryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 176, true);
			this.AIITermsOfDeliveryGroupBox.Name = "AIITermsOfDeliveryGroupBox";
			this.AIITermsOfDeliveryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(365, 75, true);
			this.AIITermsOfDeliveryGroupBox.TabIndex = 7;
			this.AIITermsOfDeliveryGroupBox.TabStop = false;
			this.AIITermsOfDeliveryGroupBox.Text = "Terms of Delivery";
			// 
			// TermsOfDeliveryLocationCountryCodeFindBox
			// 
			this.TermsOfDeliveryLocationCountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TermsOfDeliveryLocationCountryCodeFindBox, "FilteredInvoices.US_TermsOfDeliveryLocationCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.IInvoicesProvider)(null)).FilteredInvoices)).SyncRoot)).US_TermsOfDeliveryLocationCountry)));
			this.TermsOfDeliveryLocationCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(238, 44, true);
			this.TermsOfDeliveryLocationCountryCodeFindBox.Name = "TermsOfDeliveryLocationCountryCodeFindBox";
			this.TermsOfDeliveryLocationCountryCodeFindBox.PopupCaption = null;
			this.TermsOfDeliveryLocationCountryCodeFindBox.PreBoundMaxLength = 2;
			this.TermsOfDeliveryLocationCountryCodeFindBox.ShowDescriptionBox = false;
			this.TermsOfDeliveryLocationCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.TermsOfDeliveryLocationCountryCodeFindBox.TabIndex = 0;
			// 
			// TermsOfDeliveryLocationIndicatorDropEdit
			// 
			this.TermsOfDeliveryLocationIndicatorDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TermsOfDeliveryLocationIndicatorDropEdit, "FilteredInvoices.US_TermsOfDeliveryLocationIndicator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.IInvoicesProvider)(null)).FilteredInvoices)).SyncRoot)).US_TermsOfDeliveryLocationIndicator)));
			this.TermsOfDeliveryLocationIndicatorDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("InvoiceHeaderAIIUserControl|23540186-a6c6-4435-afb7-adf229d765be", "Loc. Indicator");
			this.TermsOfDeliveryLocationIndicatorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 44, true);
			this.TermsOfDeliveryLocationIndicatorDropEdit.Name = "TermsOfDeliveryLocationIndicatorDropEdit";
			this.TermsOfDeliveryLocationIndicatorDropEdit.PreBoundMaxLength = 1;
			this.TermsOfDeliveryLocationIndicatorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(123, 20, true);
			this.TermsOfDeliveryLocationIndicatorDropEdit.TabIndex = 1;
			// 
			// TermsOfDeliveryDropEdit
			// 
			this.TermsOfDeliveryDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TermsOfDeliveryDropEdit, "FilteredInvoices.US_TermsOfDeliveryLocationQualifier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.IInvoicesProvider)(null)).FilteredInvoices)).SyncRoot)).US_TermsOfDeliveryLocationQualifier)));
			this.TermsOfDeliveryDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("InvoiceHeaderAIIUserControl|b7d69cb2-6b92-463f-8a56-932f096f8f7b", "Loc. Qualifier");
			this.TermsOfDeliveryDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 21, true);
			this.TermsOfDeliveryDropEdit.Name = "TermsOfDeliveryDropEdit";
			this.TermsOfDeliveryDropEdit.PreBoundMaxLength = 2;
			this.TermsOfDeliveryDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(123, 20, true);
			this.TermsOfDeliveryDropEdit.TabIndex = 0;
			// 
			// TermsOfDeliveryLocationScheduleDCodeFindBox
			// 
			this.TermsOfDeliveryLocationScheduleDCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TermsOfDeliveryLocationScheduleDCodeFindBox, "FilteredInvoices.US_TermsOfDeliveryLocationScheduleD");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.IInvoicesProvider)(null)).FilteredInvoices)).SyncRoot)).US_TermsOfDeliveryLocationScheduleD)));
			this.TermsOfDeliveryLocationScheduleDCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(238, 44, true);
			this.TermsOfDeliveryLocationScheduleDCodeFindBox.Name = "TermsOfDeliveryLocationScheduleDCodeFindBox";
			this.TermsOfDeliveryLocationScheduleDCodeFindBox.PopupCaption = null;
			this.TermsOfDeliveryLocationScheduleDCodeFindBox.PreBoundMaxLength = 5;
			this.TermsOfDeliveryLocationScheduleDCodeFindBox.ShowDescriptionBox = false;
			this.TermsOfDeliveryLocationScheduleDCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.TermsOfDeliveryLocationScheduleDCodeFindBox.TabIndex = 4;
			// 
			// TermsOfDeliveryLocationTextBox
			// 
			this.TermsOfDeliveryLocationTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.TermsOfDeliveryLocationTextBox, "FilteredInvoices.US_TermsOfDeliveryLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.IInvoicesProvider)(null)).FilteredInvoices)).SyncRoot)).US_TermsOfDeliveryLocation)));
			this.TermsOfDeliveryLocationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(246, 44, true);
			this.TermsOfDeliveryLocationTextBox.Name = "TermsOfDeliveryLocationTextBox";
			this.TermsOfDeliveryLocationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(93, 20, true);
			this.TermsOfDeliveryLocationTextBox.TabIndex = 1;
			// 
			// US_TermsOfDeliveryLocationScheduleKCodeFindBox
			// 
			this.US_TermsOfDeliveryLocationScheduleKCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.US_TermsOfDeliveryLocationScheduleKCodeFindBox, "FilteredInvoices.US_TermsOfDeliveryLocationScheduleK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.IInvoicesProvider)(null)).FilteredInvoices)).SyncRoot)).US_TermsOfDeliveryLocationScheduleK)));
			this.US_TermsOfDeliveryLocationScheduleKCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(236, 45, true);
			this.US_TermsOfDeliveryLocationScheduleKCodeFindBox.Name = "US_TermsOfDeliveryLocationScheduleKCodeFindBox";
			this.US_TermsOfDeliveryLocationScheduleKCodeFindBox.PopupCaption = null;
			this.US_TermsOfDeliveryLocationScheduleKCodeFindBox.PreBoundMaxLength = 5;
			this.US_TermsOfDeliveryLocationScheduleKCodeFindBox.ShowDescriptionBox = false;
			this.US_TermsOfDeliveryLocationScheduleKCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.US_TermsOfDeliveryLocationScheduleKCodeFindBox.TabIndex = 5;
			// 
			// AIIInvoiceTypeDropEdit
			// 
			this.AIIInvoiceTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AIIInvoiceTypeDropEdit, "FilteredInvoices.US_InvoiceType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.IInvoicesProvider)(null)).FilteredInvoices)).SyncRoot)).US_InvoiceType)));
			this.AIIInvoiceTypeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("InvoiceHeaderAIIUserControl|51843166-b4bf-450c-9beb-03d803ad85c9", "Invoice Type");
			this.AIIInvoiceTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(91, 28, true);
			this.AIIInvoiceTypeDropEdit.Name = "AIIInvoiceTypeDropEdit";
			this.AIIInvoiceTypeDropEdit.PreBoundMaxLength = 2;
			this.AIIInvoiceTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(281, 20, true);
			this.AIIInvoiceTypeDropEdit.TabIndex = 1;
			// 
			// AIIPaymentTermsDescTextBox
			// 
			this.AIIPaymentTermsDescTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.AIIPaymentTermsDescTextBox, "FilteredInvoices.US_PaymentTermsDesc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.IInvoicesProvider)(null)).FilteredInvoices)).SyncRoot)).US_PaymentTermsDesc)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AIIPaymentTermsDescTextBox, false);
			this.AIIPaymentTermsDescTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 52, true);
			this.AIIPaymentTermsDescTextBox.Multiline = true;
			this.AIIPaymentTermsDescTextBox.Name = "AIIPaymentTermsDescTextBox";
			this.AIIPaymentTermsDescTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 20, true);
			this.AIIPaymentTermsDescTextBox.TabIndex = 3;
			// 
			// AIIPaymentTermsDropEdit
			// 
			this.AIIPaymentTermsDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AIIPaymentTermsDropEdit, "FilteredInvoices.US_PaymentTerms");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.IInvoicesProvider)(null)).FilteredInvoices)).SyncRoot)).US_PaymentTerms)));
			this.AIIPaymentTermsDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("InvoiceHeaderAIIUserControl|7a20547f-6fc6-4357-b87b-f039c3bb87b0", "Payment Terms");
			this.AIIPaymentTermsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(91, 52, true);
			this.AIIPaymentTermsDropEdit.Name = "AIIPaymentTermsDropEdit";
			this.AIIPaymentTermsDropEdit.PreBoundMaxLength = 2;
			this.AIIPaymentTermsDropEdit.ShowDescriptionBox = false;
			this.AIIPaymentTermsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 20, true);
			this.AIIPaymentTermsDropEdit.TabIndex = 2;
			// 
			// RelatedBillPanel
			// 
			this.RelatedBillPanel.Controls.Add(this.RelatedBillGuidDropEdit);
			this.RelatedBillPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 73, true);
			this.RelatedBillPanel.Name = "RelatedBillPanel";
			this.RelatedBillPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(369, 25, true);
			this.RelatedBillPanel.TabIndex = 5;
			// 
			// RelatedBillGuidDropEdit
			// 
			this.RelatedBillGuidDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RelatedBillGuidDropEdit, "FilteredInvoices.JZ_CU_RelatedHouseBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.IInvoicesProvider)(null)).FilteredInvoices)).SyncRoot)).JZ_CU_RelatedHouseBill)));
			this.RelatedBillGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 3, true);
			this.RelatedBillGuidDropEdit.Name = "RelatedBillGuidDropEdit";
			this.RelatedBillGuidDropEdit.PreBoundMaxLength = 15;
			this.RelatedBillGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(281, 20, true);
			this.RelatedBillGuidDropEdit.TabIndex = 0;
			// 
			// DesExamSiteCodeFindBox
			// 
			this.DesExamSiteCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DesExamSiteCodeFindBox, "FilteredInvoices.US_DES");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.IInvoicesProvider)(null)).FilteredInvoices)).SyncRoot)).US_DES)));
			this.DesExamSiteCodeFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("InvoiceHeaderAIIUserControl|dc08bed4-8069-4948-9a9e-e75b2e248ae3", "DES");
			this.DesExamSiteCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(305, 52, true);
			this.DesExamSiteCodeFindBox.Name = "DesExamSiteCodeFindBox";
			this.DesExamSiteCodeFindBox.PopupCaption = null;
			this.DesExamSiteCodeFindBox.PreBoundMaxLength = 4;
			this.DesExamSiteCodeFindBox.ShowDescriptionBox = false;
			this.DesExamSiteCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.DesExamSiteCodeFindBox.TabIndex = 4;
			// 
			// InvoiceHeaderAIIUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DesExamSiteCodeFindBox);
			this.Controls.Add(this.RelatedBillPanel);
			this.Controls.Add(this.TotalSubjectToGroupBox);
			this.Controls.Add(this.AIIOtherDetailsTabControl);
			this.Controls.Add(this.AIIInvoiceDateDateEdit);
			this.Controls.Add(this.AIITermsOfDeliveryGroupBox);
			this.Controls.Add(this.AIIInvoiceTypeDropEdit);
			this.Controls.Add(this.AIIPaymentTermsDescTextBox);
			this.Controls.Add(this.AIIPaymentTermsDropEdit);
			this.Name = "InvoiceHeaderAIIUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(902, 290, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TotalSubjectToGroupBox.ResumeLayout(false);
			this.AIIOtherDetailsTabControl.ResumeLayout(false);
			this.RelatedDataTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.AIIRelatedDocumentsGrid)).EndInit();
			this.AIITermsOfDeliveryGroupBox.ResumeLayout(false);
			this.AIITermsOfDeliveryGroupBox.PerformLayout();
			this.RelatedBillPanel.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox TotalSubjectToGroupBox;
		private Enterprise.ZArchitecture.GUI.ZCalcFindBox DiscountValueCalcFindBox;
		private Enterprise.ZArchitecture.GUI.ZCalcFindBox ForeignTaxValueCalcFindBox;
		internal Enterprise.ZArchitecture.GUI.ZTabControl AIIOtherDetailsTabControl;
		internal Enterprise.ZArchitecture.GUI.ZTabPage RelatedDataTabPage;
		internal Enterprise.ZArchitecture.GUI.ZButton DefaultRelatedDocumentsButton;
		private Enterprise.ZArchitecture.ZGrid AIIRelatedDocumentsGrid;
		private Enterprise.ZArchitecture.GUI.ZGroupBox AIITermsOfDeliveryGroupBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox TermsOfDeliveryLocationCountryCodeFindBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit TermsOfDeliveryLocationIndicatorDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit TermsOfDeliveryDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox TermsOfDeliveryLocationScheduleDCodeFindBox;
		internal Enterprise.ZArchitecture.ZTextBox TermsOfDeliveryLocationTextBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit AIIInvoiceTypeDropEdit;
		private Enterprise.ZArchitecture.ZTextBox AIIPaymentTermsDescTextBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit AIIPaymentTermsDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZPanel RelatedBillPanel;
		private Enterprise.ZArchitecture.GUI.ZGuidDropEdit RelatedBillGuidDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox US_TermsOfDeliveryLocationScheduleKCodeFindBox;
		public Enterprise.ZArchitecture.GUI.ZDateEdit AIIInvoiceDateDateEdit;
		private ZArchitecture.GUI.ZCodeFindBox DesExamSiteCodeFindBox;


	}
}
