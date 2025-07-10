
namespace Enterprise.Customs.TW.GUI
{
	partial class LicensingAnimalAndPlantUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.QuarantineTreatmentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.JI_QuarantineTreatmentTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.QuarantineFeaturesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.JI_QuarantineFeaturesTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.VaccinationTypeDateGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.JI_VaccinationTypeDateTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JI_AnimalAgeMonthCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.JI_AnimalFemaleQtyCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.JI_AnimalMaleQtyCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.JI_AnimalAgeYearCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.JI_MicrochipIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PackingHousesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PackingDatesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SlaughterDatesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PackingHousesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PackingDatesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SlaughterDatesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.QuarantineTreatmentGroupBox.SuspendLayout();
			this.QuarantineFeaturesGroupBox.SuspendLayout();
			this.VaccinationTypeDateGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackingHousesGrid)).BeginInit();
			this.PackingHousesGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackingDatesGrid)).BeginInit();
			this.PackingDatesGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SlaughterDatesGrid)).BeginInit();
			this.SlaughterDatesGrid.SuspendLayout();
			this.PackingHousesGroupBox.SuspendLayout();
			this.PackingDatesGroupBox.SuspendLayout();
			this.SlaughterDatesGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.Business.JobDeclaration);
			// 
			// QuarantineTreatmentGroupBox
			// 
			this.QuarantineTreatmentGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("b1fa5b76-43e4-4cda-8f3d-54407c679a8e", "Quarantine Treatment");
			this.QuarantineTreatmentGroupBox.Controls.Add(this.JI_QuarantineTreatmentTextBox);
			this.QuarantineTreatmentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.QuarantineTreatmentGroupBox.Name = "QuarantineTreatmentGroupBox";
			this.QuarantineTreatmentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 282, true);
			this.QuarantineTreatmentGroupBox.TabIndex = 0;
			this.QuarantineTreatmentGroupBox.TabStop = false;
			// 
			// JI_QuarantineTreatmentTextBox
			// 
			this.BindingSource.SetBindingMember(this.JI_QuarantineTreatmentTextBox, "FilteredInvoiceLines.JI_QuarantineTreatment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_QuarantineTreatment)));
			this.JI_QuarantineTreatmentTextBox.CaptionResourceString = null;
			this.JI_QuarantineTreatmentTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.JI_QuarantineTreatmentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.JI_QuarantineTreatmentTextBox.Multiline = true;
			this.JI_QuarantineTreatmentTextBox.Name = "JI_QuarantineTreatmentTextBox";
			this.JI_QuarantineTreatmentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 263, true);
			this.JI_QuarantineTreatmentTextBox.TabIndex = 0;
			// 
			// QuarantineFeaturesGroupBox
			// 
			this.QuarantineFeaturesGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("964ce309-cb0f-4a9e-a5a8-255c3eb2bb26", "Color/ Characteristics/ Botanical Nomenclature");
			this.QuarantineFeaturesGroupBox.Controls.Add(this.JI_QuarantineFeaturesTextBox);
			this.QuarantineFeaturesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(238, 2, true);
			this.QuarantineFeaturesGroupBox.Name = "QuarantineFeaturesGroupBox";
			this.QuarantineFeaturesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(310, 137, true);
			this.QuarantineFeaturesGroupBox.TabIndex = 1;
			this.QuarantineFeaturesGroupBox.TabStop = false;
			// 
			// JI_QuarantineFeaturesTextBox
			// 
			this.BindingSource.SetBindingMember(this.JI_QuarantineFeaturesTextBox, "FilteredInvoiceLines.JI_QuarantineFeatures");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_QuarantineFeatures)));
			this.JI_QuarantineFeaturesTextBox.CaptionResourceString = null;
			this.JI_QuarantineFeaturesTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.JI_QuarantineFeaturesTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.JI_QuarantineFeaturesTextBox.Multiline = true;
			this.JI_QuarantineFeaturesTextBox.Name = "JI_QuarantineFeaturesTextBox";
			this.JI_QuarantineFeaturesTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 118, true);
			this.JI_QuarantineFeaturesTextBox.TabIndex = 0;
			// 
			// VaccinationTypeDateGroupBox
			// 
			this.VaccinationTypeDateGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("08f28d66-bbef-44c1-954d-be97e4d628a9", "Vaccination Type/ Date");
			this.VaccinationTypeDateGroupBox.Controls.Add(this.JI_VaccinationTypeDateTextBox);
			this.VaccinationTypeDateGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(239, 147, true);
			this.VaccinationTypeDateGroupBox.Name = "VaccinationTypeDateGroupBox";
			this.VaccinationTypeDateGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(310, 137, true);
			this.VaccinationTypeDateGroupBox.TabIndex = 2;
			this.VaccinationTypeDateGroupBox.TabStop = false;
			// 
			// JI_VaccinationTypeDateTextBox
			// 
			this.BindingSource.SetBindingMember(this.JI_VaccinationTypeDateTextBox, "FilteredInvoiceLines.JI_VaccinationTypeDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_VaccinationTypeDate)));
			this.JI_VaccinationTypeDateTextBox.CaptionResourceString = null;
			this.JI_VaccinationTypeDateTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.JI_VaccinationTypeDateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.JI_VaccinationTypeDateTextBox.Multiline = true;
			this.JI_VaccinationTypeDateTextBox.Name = "JI_VaccinationTypeDateTextBox";
			this.JI_VaccinationTypeDateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 118, true);
			this.JI_VaccinationTypeDateTextBox.TabIndex = 0;
			// 
			// JI_AnimalAgeMonthCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.JI_AnimalAgeMonthCalcEdit, "FilteredInvoiceLines.JI_AnimalAgeMonth");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_AnimalAgeMonth)));
			this.JI_AnimalAgeMonthCalcEdit.CaptionResourceString = null;
			this.JI_AnimalAgeMonthCalcEdit.DecimalPlaces = 0;
			this.JI_AnimalAgeMonthCalcEdit.Decimals = 0;
			this.JI_AnimalAgeMonthCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(912, 66, true);
			this.JI_AnimalAgeMonthCalcEdit.MaxValue = new decimal(new int[] {
            99,
            0,
            0,
            0});
			this.JI_AnimalAgeMonthCalcEdit.Name = "JI_AnimalAgeMonthCalcEdit";
			this.JI_AnimalAgeMonthCalcEdit.ShowEmptyStringForEmptyValue = true;
			this.JI_AnimalAgeMonthCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.JI_AnimalAgeMonthCalcEdit.TabIndex = 5;
			this.JI_AnimalAgeMonthCalcEdit.Text = "0";
			this.JI_AnimalAgeMonthCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JI_AnimalFemaleQtyCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.JI_AnimalFemaleQtyCalcEdit, "FilteredInvoiceLines.JI_AnimalFemaleQty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_AnimalFemaleQty)));
			this.JI_AnimalFemaleQtyCalcEdit.CaptionResourceString = null;
			this.JI_AnimalFemaleQtyCalcEdit.DecimalPlaces = 0;
			this.JI_AnimalFemaleQtyCalcEdit.Decimals = 0;
			this.JI_AnimalFemaleQtyCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(912, 117, true);
			this.JI_AnimalFemaleQtyCalcEdit.MaxValue = new decimal(new int[] {
            999999,
            0,
            0,
            0});
			this.JI_AnimalFemaleQtyCalcEdit.Name = "JI_AnimalFemaleQtyCalcEdit";
			this.JI_AnimalFemaleQtyCalcEdit.ShowEmptyStringForEmptyValue = true;
			this.JI_AnimalFemaleQtyCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.JI_AnimalFemaleQtyCalcEdit.TabIndex = 7;
			this.JI_AnimalFemaleQtyCalcEdit.Text = "0";
			this.JI_AnimalFemaleQtyCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JI_AnimalMaleQtyCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.JI_AnimalMaleQtyCalcEdit, "FilteredInvoiceLines.JI_AnimalMaleQty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_AnimalMaleQty)));
			this.JI_AnimalMaleQtyCalcEdit.CaptionResourceString = null;
			this.JI_AnimalMaleQtyCalcEdit.DecimalPlaces = 0;
			this.JI_AnimalMaleQtyCalcEdit.Decimals = 0;
			this.JI_AnimalMaleQtyCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(912, 91, true);
			this.JI_AnimalMaleQtyCalcEdit.MaxValue = new decimal(new int[] {
            999999,
            0,
            0,
            0});
			this.JI_AnimalMaleQtyCalcEdit.Name = "JI_AnimalMaleQtyCalcEdit";
			this.JI_AnimalMaleQtyCalcEdit.ShowEmptyStringForEmptyValue = true;
			this.JI_AnimalMaleQtyCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.JI_AnimalMaleQtyCalcEdit.TabIndex = 6;
			this.JI_AnimalMaleQtyCalcEdit.Text = "0";
			this.JI_AnimalMaleQtyCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JI_AnimalAgeYearCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.JI_AnimalAgeYearCalcEdit, "FilteredInvoiceLines.JI_AnimalAgeYear");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_AnimalAgeYear)));
			this.JI_AnimalAgeYearCalcEdit.CaptionResourceString = null;
			this.JI_AnimalAgeYearCalcEdit.DecimalPlaces = 0;
			this.JI_AnimalAgeYearCalcEdit.Decimals = 0;
			this.JI_AnimalAgeYearCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(912, 41, true);
			this.JI_AnimalAgeYearCalcEdit.MaxValue = new decimal(new int[] {
            9999,
            0,
            0,
            0});
			this.JI_AnimalAgeYearCalcEdit.Name = "JI_AnimalAgeYearCalcEdit";
			this.JI_AnimalAgeYearCalcEdit.ShowEmptyStringForEmptyValue = true;
			this.JI_AnimalAgeYearCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.JI_AnimalAgeYearCalcEdit.TabIndex = 4;
			this.JI_AnimalAgeYearCalcEdit.Text = "0";
			this.JI_AnimalAgeYearCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JI_MicrochipIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.JI_MicrochipIDTextBox, "FilteredInvoiceLines.JI_MicrochipID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_MicrochipID)));
			this.JI_MicrochipIDTextBox.CaptionResourceString = null;
			this.JI_MicrochipIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(912, 16, true);
			this.JI_MicrochipIDTextBox.Name = "JI_MicrochipIDTextBox";
			this.JI_MicrochipIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(137, 20, true);
			this.JI_MicrochipIDTextBox.TabIndex = 3;
			// 
			// PackingHousesGrid
			// 
			this.PackingHousesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PackingHousesGrid, "FilteredInvoiceLines.PackingHouseCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).PackingHouseCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.PackingHouse)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).PackingHouseCollection)).SyncRoot)).CY_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.PackingHouse)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).PackingHouseCollection)).SyncRoot)).Description)));
			this.PackingHousesGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CY_Code";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo1.ColumnName = "Description";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(155);
			this.PackingHousesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.PackingHousesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PackingHousesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackingHousesGrid.GridId = "66115c73-3598-42a1-a5b0-b6e7505f255b";
			this.PackingHousesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PackingHousesGrid.LayoutKey = "PackingHousesGrid";
			this.PackingHousesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.PackingHousesGrid.MaximumRows = 5;
			this.PackingHousesGrid.Name = "PackingHousesGrid";
			this.PackingHousesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 115, true);
			this.PackingHousesGrid.TabIndex = 8;
			// 
			// PackingDatesGrid
			// 
			this.PackingDatesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PackingDatesGrid, "FilteredInvoiceLines.PackingDateCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).PackingDateCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.TW.Business.PackingDate)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).PackingDateCollection)).SyncRoot)).CY_Date)));
			this.PackingDatesGrid.CaptionVisible = false;
			zDateEditColumnStyleInfo1.ColumnName = "CY_Date";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.PackingDatesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.PackingDatesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackingDatesGrid.GridId = "60294274-5c06-4afe-9a98-ad270b00b918";
			this.PackingDatesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PackingDatesGrid.LayoutKey = "zGrid1";
			this.PackingDatesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.PackingDatesGrid.MaximumRows = 5;
			this.PackingDatesGrid.Name = "PackingDatesGrid";
			this.PackingDatesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 118, true);
			this.PackingDatesGrid.TabIndex = 9;
			// 
			// SlaughterDatesGrid
			// 
			this.SlaughterDatesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SlaughterDatesGrid, "FilteredInvoiceLines.SlaughterDateCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SlaughterDateCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.TW.Business.SlaughterDate)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SlaughterDateCollection)).SyncRoot)).CY_Date)));
			this.SlaughterDatesGrid.CaptionVisible = false;
			zDateEditColumnStyleInfo2.ColumnName = "CY_Date";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.SlaughterDatesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.SlaughterDatesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SlaughterDatesGrid.GridId = "9402d565-f55a-43c0-b96b-392740ff9627";
			this.SlaughterDatesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SlaughterDatesGrid.LayoutKey = "SlaughterDatesGrid";
			this.SlaughterDatesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.SlaughterDatesGrid.MaximumRows = 5;
			this.SlaughterDatesGrid.Name = "SlaughterDatesGrid";
			this.SlaughterDatesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 118, true);
			this.SlaughterDatesGrid.TabIndex = 10;
			// 
			// PackingHousesGroupBox
			// 
			this.PackingHousesGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("2dba6620-0cc2-4414-af63-d0e7d452f1de", "Packing Houses");
			this.PackingHousesGroupBox.Controls.Add(this.PackingHousesGrid);
			this.PackingHousesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(554, 2, true);
			this.PackingHousesGroupBox.Name = "PackingHousesGroupBox";
			this.PackingHousesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 134, true);
			this.PackingHousesGroupBox.TabIndex = 11;
			this.PackingHousesGroupBox.TabStop = false;
			// 
			// PackingDatesGroupBox
			// 
			this.PackingDatesGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("952c46df-9e25-4cb8-9408-f9d9c48e7e36", "Packing Dates");
			this.PackingDatesGroupBox.Controls.Add(this.PackingDatesGrid);
			this.PackingDatesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(554, 147, true);
			this.PackingDatesGroupBox.Name = "PackingDatesGroupBox";
			this.PackingDatesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 137, true);
			this.PackingDatesGroupBox.TabIndex = 12;
			this.PackingDatesGroupBox.TabStop = false;
			// 
			// SlaughterDatesGroupBox
			// 
			this.SlaughterDatesGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("68c59961-6e7d-426e-ac0f-daa60f92251d", "Slaughter Dates");
			this.SlaughterDatesGroupBox.Controls.Add(this.SlaughterDatesGrid);
			this.SlaughterDatesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(804, 147, true);
			this.SlaughterDatesGroupBox.Name = "SlaughterDatesGroupBox";
			this.SlaughterDatesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 137, true);
			this.SlaughterDatesGroupBox.TabIndex = 13;
			this.SlaughterDatesGroupBox.TabStop = false;
			// 
			// LicensingAnimalAndPlantUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SlaughterDatesGroupBox);
			this.Controls.Add(this.PackingDatesGroupBox);
			this.Controls.Add(this.PackingHousesGroupBox);
			this.Controls.Add(this.JI_AnimalAgeMonthCalcEdit);
			this.Controls.Add(this.JI_AnimalFemaleQtyCalcEdit);
			this.Controls.Add(this.JI_AnimalMaleQtyCalcEdit);
			this.Controls.Add(this.JI_AnimalAgeYearCalcEdit);
			this.Controls.Add(this.JI_MicrochipIDTextBox);
			this.Controls.Add(this.VaccinationTypeDateGroupBox);
			this.Controls.Add(this.QuarantineFeaturesGroupBox);
			this.Controls.Add(this.QuarantineTreatmentGroupBox);
			this.Name = "LicensingAnimalAndPlantUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1060, 286, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.QuarantineTreatmentGroupBox.ResumeLayout(false);
			this.QuarantineTreatmentGroupBox.PerformLayout();
			this.QuarantineFeaturesGroupBox.ResumeLayout(false);
			this.QuarantineFeaturesGroupBox.PerformLayout();
			this.VaccinationTypeDateGroupBox.ResumeLayout(false);
			this.VaccinationTypeDateGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackingHousesGrid)).EndInit();
			this.PackingHousesGrid.ResumeLayout(false);
			this.PackingHousesGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackingDatesGrid)).EndInit();
			this.PackingDatesGrid.ResumeLayout(false);
			this.PackingDatesGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SlaughterDatesGrid)).EndInit();
			this.SlaughterDatesGrid.ResumeLayout(false);
			this.SlaughterDatesGrid.PerformLayout();
			this.PackingHousesGroupBox.ResumeLayout(false);
			this.PackingHousesGroupBox.PerformLayout();
			this.PackingDatesGroupBox.ResumeLayout(false);
			this.PackingDatesGroupBox.PerformLayout();
			this.SlaughterDatesGroupBox.ResumeLayout(false);
			this.SlaughterDatesGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox QuarantineTreatmentGroupBox;
		private ZArchitecture.GUI.ZGroupBox QuarantineFeaturesGroupBox;
		private ZArchitecture.GUI.ZGroupBox VaccinationTypeDateGroupBox;
		private ZArchitecture.ZTextBox JI_QuarantineTreatmentTextBox;
		private ZArchitecture.ZTextBox JI_QuarantineFeaturesTextBox;
		private ZArchitecture.ZTextBox JI_VaccinationTypeDateTextBox;
		private ZArchitecture.ZCalcEdit JI_AnimalAgeMonthCalcEdit;
		private ZArchitecture.ZCalcEdit JI_AnimalFemaleQtyCalcEdit;
		private ZArchitecture.ZCalcEdit JI_AnimalMaleQtyCalcEdit;
		private ZArchitecture.ZCalcEdit JI_AnimalAgeYearCalcEdit;
		private ZArchitecture.ZTextBox JI_MicrochipIDTextBox;
		private ZArchitecture.ZGrid PackingHousesGrid;
		private ZArchitecture.ZGrid PackingDatesGrid;
		private ZArchitecture.ZGrid SlaughterDatesGrid;
		private ZArchitecture.GUI.ZGroupBox PackingHousesGroupBox;
		private ZArchitecture.GUI.ZGroupBox PackingDatesGroupBox;
		private ZArchitecture.GUI.ZGroupBox SlaughterDatesGroupBox;
	}
}
