
namespace Enterprise.Customs.TW.GUI
{
	partial class SupportingDocumentsUserControl
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
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.SpecialCodesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SpecialCodesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ImportExportRegulationsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ImportExportRegulationsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PermitNumbersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PermitNumberGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PreviousBondedEntryLineNumberCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PreviousBondedEntryNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CertificateOfOriginNumberItemNumberCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CertificateOfOriginNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JI_PreviousEntryLineNumberCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.JI_PreviousEntryNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.QuotaPermitNumberItemNumberCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.QuotaPermitNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CitesPermitTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.HighTechLicenseTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SpecialCodesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SpecialCodesGrid)).BeginInit();
			this.SpecialCodesGrid.SuspendLayout();
			this.ImportExportRegulationsGroupBox.SuspendLayout();
			this.PermitNumbersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PermitNumberGrid)).BeginInit();
			this.PermitNumberGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.Business.JobDeclaration);
			// 
			// SpecialCodesGroupBox
			// 
			this.SpecialCodesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.SpecialCodesGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("428abc2f-c95a-40d0-a0ab-0632440c1a13", "Permit Exemption Codes");
			this.SpecialCodesGroupBox.Controls.Add(this.SpecialCodesGrid);
			this.SpecialCodesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(342, 157, true);
			this.SpecialCodesGroupBox.Name = "SpecialCodesGroupBox";
			this.SpecialCodesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(311, 169, true);
			this.SpecialCodesGroupBox.TabIndex = 2;
			this.SpecialCodesGroupBox.TabStop = false;
			// 
			// SpecialCodesGrid
			// 
			this.SpecialCodesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SpecialCodesGrid, "FilteredInvoiceLines.ExemptionOfControllingAgenciesCusSupportings");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ExemptionOfControllingAgenciesCusSupportings)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.ExemptionOfControllingAgenciesCusSupporting)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ExemptionOfControllingAgenciesCusSupportings)).SyncRoot)).CSI_ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.ExemptionOfControllingAgenciesCusSupporting)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ExemptionOfControllingAgenciesCusSupportings)).SyncRoot)).SpecialCodeDescription)));
			this.SpecialCodesGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CSI_ReferenceNumber";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zMultiLineTextBoxColumnInfo1.ColumnName = "SpecialCodeDescription";
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(800);
			this.SpecialCodesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.SpecialCodesGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.SpecialCodesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SpecialCodesGrid.GridId = "8161618A-183A-456D-9FCE-41193264ACD6";
			this.SpecialCodesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SpecialCodesGrid.LayoutKey = "SpecialCodesGrid";
			this.SpecialCodesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.SpecialCodesGrid.MaximumRows = 5;
			this.SpecialCodesGrid.Name = "SpecialCodesGrid";
			this.SpecialCodesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(305, 150, true);
			this.SpecialCodesGrid.TabIndex = 2;
			// 
			// ImportExportRegulationsGroupBox
			// 
			this.ImportExportRegulationsGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("b2edd8fa-87c8-414f-a870-47cb6adf1377", "Import Regulations");
			this.ImportExportRegulationsGroupBox.Controls.Add(this.ImportExportRegulationsTextBox);
			this.ImportExportRegulationsGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.ImportExportRegulationsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ImportExportRegulationsGroupBox.Name = "ImportExportRegulationsGroupBox";
			this.ImportExportRegulationsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 329, true);
			this.ImportExportRegulationsGroupBox.TabIndex = 0;
			this.ImportExportRegulationsGroupBox.TabStop = false;
			// 
			// ImportExportRegulationsTextBox
			// 
			this.BindingSource.SetBindingMember(this.ImportExportRegulationsTextBox, "FilteredInvoiceLines.ImportExportRegulations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ImportExportRegulations)));
			this.ImportExportRegulationsTextBox.CaptionResourceString = null;
			this.ImportExportRegulationsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ImportExportRegulationsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ImportExportRegulationsTextBox, false);
			this.ImportExportRegulationsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ImportExportRegulationsTextBox.Multiline = true;
			this.ImportExportRegulationsTextBox.Name = "ImportExportRegulationsTextBox";
			this.ImportExportRegulationsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.ImportExportRegulationsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 310, true);
			this.ImportExportRegulationsTextBox.TabIndex = 1;
			// 
			// PermitNumbersGroupBox
			// 
			this.PermitNumbersGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("885a6bc7-65a5-4a91-95ce-968e5b69a960", "Permits");
			this.PermitNumbersGroupBox.Controls.Add(this.PermitNumberGrid);
			this.PermitNumbersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(342, 0, true);
			this.PermitNumbersGroupBox.Name = "PermitNumbersGroupBox";
			this.PermitNumbersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(311, 151, true);
			this.PermitNumbersGroupBox.TabIndex = 1;
			this.PermitNumbersGroupBox.TabStop = false;
			// 
			// PermitNumberGrid
			// 
			this.PermitNumberGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PermitNumberGrid, "FilteredInvoiceLines.PermitCusSupportingCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).PermitCusSupportingCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.PermitCusSupporting)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).PermitCusSupportingCollection)).SyncRoot)).CSI_ItemNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.PermitCusSupporting)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).PermitCusSupportingCollection)).SyncRoot)).CSI_ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.PermitCusSupporting)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).PermitCusSupportingCollection)).SyncRoot)).CSI_LineNo)));
			this.PermitNumberGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "CSI_ItemNumber";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CSI_ReferenceNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "CSI_LineNo";
			zCalcEditColumnStyleInfo2.ShowEmptyStringForEmptyValue = true;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			this.PermitNumberGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.PermitNumberGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PermitNumberGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.PermitNumberGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PermitNumberGrid.GridId = "0812b47c-aae0-48dd-8abe-003d36f7cef7";
			this.PermitNumberGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PermitNumberGrid.LayoutKey = "PermitNumberGrid";
			this.PermitNumberGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.PermitNumberGrid.MaximumRows = 5;
			this.PermitNumberGrid.Name = "PermitNumberGrid";
			this.PermitNumberGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(305, 132, true);
			this.PermitNumberGrid.TabIndex = 0;
			// 
			// PreviousBondedEntryLineNumberCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PreviousBondedEntryLineNumberCalcEdit, "FilteredInvoiceLines.PreviousBondedEntryLineNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).PreviousBondedEntryLineNumber)));
			this.PreviousBondedEntryLineNumberCalcEdit.CaptionResourceString = null;
			this.PreviousBondedEntryLineNumberCalcEdit.DecimalPlaces = 0;
			this.PreviousBondedEntryLineNumberCalcEdit.Decimals = 0;
			this.PreviousBondedEntryLineNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(963, 53, true);
			this.PreviousBondedEntryLineNumberCalcEdit.MaxValue = new decimal(new int[] {
            9999,
            0,
            0,
            0});
			this.PreviousBondedEntryLineNumberCalcEdit.Name = "PreviousBondedEntryLineNumberCalcEdit";
			this.PreviousBondedEntryLineNumberCalcEdit.ShowEmptyStringForEmptyValue = true;
			this.PreviousBondedEntryLineNumberCalcEdit.ShowGroupSeparators = false;
			this.PreviousBondedEntryLineNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(37, 20, true);
			this.PreviousBondedEntryLineNumberCalcEdit.TabIndex = 8;
			this.PreviousBondedEntryLineNumberCalcEdit.Text = "0";
			this.PreviousBondedEntryLineNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PreviousBondedEntryNumberTextBox
			// 
			this.PreviousBondedEntryNumberTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.PreviousBondedEntryNumberTextBox, "FilteredInvoiceLines.PreviousBondedEntryNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).PreviousBondedEntryNumber)));
			this.PreviousBondedEntryNumberTextBox.CaptionResourceString = null;
			this.PreviousBondedEntryNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(821, 53, true);
			this.PreviousBondedEntryNumberTextBox.Name = "PreviousBondedEntryNumberTextBox";
			this.PreviousBondedEntryNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.PreviousBondedEntryNumberTextBox.TabIndex = 7;
			// 
			// CertificateOfOriginNumberItemNumberCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.CertificateOfOriginNumberItemNumberCalcEdit, "FilteredInvoiceLines.CertificateOfOriginNumberItemNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CertificateOfOriginNumberItemNumber)));
			this.CertificateOfOriginNumberItemNumberCalcEdit.CaptionResourceString = null;
			this.CertificateOfOriginNumberItemNumberCalcEdit.DecimalPlaces = 0;
			this.CertificateOfOriginNumberItemNumberCalcEdit.Decimals = 0;
			this.CertificateOfOriginNumberItemNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(963, 3, true);
			this.CertificateOfOriginNumberItemNumberCalcEdit.MaxValue = new decimal(new int[] {
            9999,
            0,
            0,
            0});
			this.CertificateOfOriginNumberItemNumberCalcEdit.Name = "CertificateOfOriginNumberItemNumberCalcEdit";
			this.CertificateOfOriginNumberItemNumberCalcEdit.ShowEmptyStringForEmptyValue = true;
			this.CertificateOfOriginNumberItemNumberCalcEdit.ShowGroupSeparators = false;
			this.CertificateOfOriginNumberItemNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(37, 20, true);
			this.CertificateOfOriginNumberItemNumberCalcEdit.TabIndex = 4;
			this.CertificateOfOriginNumberItemNumberCalcEdit.Text = "0";
			this.CertificateOfOriginNumberItemNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CertificateOfOriginNumberTextBox
			// 
			this.CertificateOfOriginNumberTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CertificateOfOriginNumberTextBox, "FilteredInvoiceLines.CertificateOfOriginNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CertificateOfOriginNumber)));
			this.CertificateOfOriginNumberTextBox.CaptionResourceString = null;
			this.CertificateOfOriginNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CertificateOfOriginNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(821, 3, true);
			this.CertificateOfOriginNumberTextBox.Name = "CertificateOfOriginNumberTextBox";
			this.CertificateOfOriginNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.CertificateOfOriginNumberTextBox.TabIndex = 3;
			// 
			// JI_PreviousEntryLineNumberCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.JI_PreviousEntryLineNumberCalcEdit, "FilteredInvoiceLines.JI_PreviousEntryLineNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_PreviousEntryLineNumber)));
			this.JI_PreviousEntryLineNumberCalcEdit.CaptionResourceString = null;
			this.JI_PreviousEntryLineNumberCalcEdit.DecimalPlaces = 0;
			this.JI_PreviousEntryLineNumberCalcEdit.Decimals = 0;
			this.JI_PreviousEntryLineNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(963, 28, true);
			this.JI_PreviousEntryLineNumberCalcEdit.MaxValue = new decimal(new int[] {
            9999,
            0,
            0,
            0});
			this.JI_PreviousEntryLineNumberCalcEdit.Name = "JI_PreviousEntryLineNumberCalcEdit";
			this.JI_PreviousEntryLineNumberCalcEdit.ShowEmptyStringForEmptyValue = true;
			this.JI_PreviousEntryLineNumberCalcEdit.ShowGroupSeparators = false;
			this.JI_PreviousEntryLineNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(37, 20, true);
			this.JI_PreviousEntryLineNumberCalcEdit.TabIndex = 6;
			this.JI_PreviousEntryLineNumberCalcEdit.Text = "0";
			this.JI_PreviousEntryLineNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JI_PreviousEntryNumberTextBox
			// 
			this.JI_PreviousEntryNumberTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.JI_PreviousEntryNumberTextBox, "FilteredInvoiceLines.JI_PreviousEntryNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_PreviousEntryNumber)));
			this.JI_PreviousEntryNumberTextBox.CaptionResourceString = null;
			this.JI_PreviousEntryNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.JI_PreviousEntryNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(821, 28, true);
			this.JI_PreviousEntryNumberTextBox.Name = "JI_PreviousEntryNumberTextBox";
			this.JI_PreviousEntryNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.JI_PreviousEntryNumberTextBox.TabIndex = 5;
			// 
			// QuotaPermitNumberItemNumberCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.QuotaPermitNumberItemNumberCalcEdit, "FilteredInvoiceLines.QuotaPermitNumberItemNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).QuotaPermitNumberItemNumber)));
			this.QuotaPermitNumberItemNumberCalcEdit.CaptionResourceString = null;
			this.QuotaPermitNumberItemNumberCalcEdit.DecimalPlaces = 0;
			this.QuotaPermitNumberItemNumberCalcEdit.Decimals = 0;
			this.QuotaPermitNumberItemNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(963, 79, true);
			this.QuotaPermitNumberItemNumberCalcEdit.MaxValue = new decimal(new int[] {
            9999,
            0,
            0,
            0});
			this.QuotaPermitNumberItemNumberCalcEdit.Name = "QuotaPermitNumberItemNumberCalcEdit";
			this.QuotaPermitNumberItemNumberCalcEdit.ShowEmptyStringForEmptyValue = true;
			this.QuotaPermitNumberItemNumberCalcEdit.ShowGroupSeparators = false;
			this.QuotaPermitNumberItemNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(37, 20, true);
			this.QuotaPermitNumberItemNumberCalcEdit.TabIndex = 10;
			this.QuotaPermitNumberItemNumberCalcEdit.Text = "0";
			this.QuotaPermitNumberItemNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// QuotaPermitNumberTextBox
			// 
			this.QuotaPermitNumberTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.QuotaPermitNumberTextBox, "FilteredInvoiceLines.QuotaPermitNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).QuotaPermitNumber)));
			this.QuotaPermitNumberTextBox.CaptionResourceString = null;
			this.QuotaPermitNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(821, 79, true);
			this.QuotaPermitNumberTextBox.Name = "QuotaPermitNumberTextBox";
			this.QuotaPermitNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.QuotaPermitNumberTextBox.TabIndex = 9;
			// 
			// CitesPermitTextBox
			// 
			this.CitesPermitTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CitesPermitTextBox, "FilteredInvoiceLines.CitesPermit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CitesPermit)));
			this.CitesPermitTextBox.CaptionResourceString = null;
			this.CitesPermitTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CitesPermitTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(821, 131, true);
			this.CitesPermitTextBox.Name = "CitesPermitTextBox";
			this.CitesPermitTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(179, 20, true);
			this.CitesPermitTextBox.TabIndex = 12;
			// 
			// HighTechLicenseTextBox
			// 
			this.HighTechLicenseTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.HighTechLicenseTextBox, "FilteredInvoiceLines.HighTechLicense");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).HighTechLicense)));
			this.HighTechLicenseTextBox.CaptionResourceString = null;
			this.HighTechLicenseTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.HighTechLicenseTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(821, 105, true);
			this.HighTechLicenseTextBox.Name = "HighTechLicenseTextBox";
			this.HighTechLicenseTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(179, 20, true);
			this.HighTechLicenseTextBox.TabIndex = 11;
			// 
			// SupportingDocumentsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PermitNumbersGroupBox);
			this.Controls.Add(this.SpecialCodesGroupBox);
			this.Controls.Add(this.QuotaPermitNumberItemNumberCalcEdit);
			this.Controls.Add(this.QuotaPermitNumberTextBox);
			this.Controls.Add(this.CitesPermitTextBox);
			this.Controls.Add(this.HighTechLicenseTextBox);
			this.Controls.Add(this.ImportExportRegulationsGroupBox);
			this.Controls.Add(this.PreviousBondedEntryLineNumberCalcEdit);
			this.Controls.Add(this.PreviousBondedEntryNumberTextBox);
			this.Controls.Add(this.CertificateOfOriginNumberItemNumberCalcEdit);
			this.Controls.Add(this.CertificateOfOriginNumberTextBox);
			this.Controls.Add(this.JI_PreviousEntryLineNumberCalcEdit);
			this.Controls.Add(this.JI_PreviousEntryNumberTextBox);
			this.Name = "SupportingDocumentsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1026, 329, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SpecialCodesGroupBox.ResumeLayout(false);
			this.SpecialCodesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SpecialCodesGrid)).EndInit();
			this.SpecialCodesGrid.ResumeLayout(false);
			this.SpecialCodesGrid.PerformLayout();
			this.ImportExportRegulationsGroupBox.ResumeLayout(false);
			this.ImportExportRegulationsGroupBox.PerformLayout();
			this.PermitNumbersGroupBox.ResumeLayout(false);
			this.PermitNumbersGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PermitNumberGrid)).EndInit();
			this.PermitNumberGrid.ResumeLayout(false);
			this.PermitNumberGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox SpecialCodesGroupBox;
		public ZArchitecture.ZGrid SpecialCodesGrid;
		private ZArchitecture.GUI.ZGroupBox ImportExportRegulationsGroupBox;
		private ZArchitecture.ZTextBox ImportExportRegulationsTextBox;
		private ZArchitecture.GUI.ZGroupBox PermitNumbersGroupBox;
		private ZArchitecture.ZGrid PermitNumberGrid;
		private ZArchitecture.ZCalcEdit PreviousBondedEntryLineNumberCalcEdit;
		private ZArchitecture.ZTextBox PreviousBondedEntryNumberTextBox;
		private ZArchitecture.ZCalcEdit CertificateOfOriginNumberItemNumberCalcEdit;
		private ZArchitecture.ZTextBox CertificateOfOriginNumberTextBox;
		private ZArchitecture.ZCalcEdit JI_PreviousEntryLineNumberCalcEdit;
		private ZArchitecture.ZTextBox JI_PreviousEntryNumberTextBox;
		internal ZArchitecture.ZCalcEdit QuotaPermitNumberItemNumberCalcEdit;
		internal ZArchitecture.ZTextBox QuotaPermitNumberTextBox;
		public ZArchitecture.ZTextBox CitesPermitTextBox;
		public ZArchitecture.ZTextBox HighTechLicenseTextBox;
	}
}
