namespace Enterprise.Customs.TR.ETrade.GUI
{
	partial class ETradeMainUserControl
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
			this.RegistrationDateLongDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DepartureFlightTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TransshipmentLocationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TransshipmentReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TempRegNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TempRegNoDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DischargeRecordNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DischargeRecordNoDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ClosureNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ClosureNoDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.NumberOfBillsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TotalBoxQtyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.InspectionClerkTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LocationInformationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GoodsLocationCodeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.TransshipmentCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.DepartureCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.TransshipmentConveyanceCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.PreviousContainerNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NewContainerNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ProcedureCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.PresentationCustomsOfficeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ImportExportCustomsOfficeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DischargeLoadingCustomsOfficeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GoodsDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CustomsValueCalcFindBox = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			this.FreightValueCalcFindBox = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			this.InsuranceValueCalcFindBox = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			this.ExchangeRateCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.StampTaxValueTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OtherValueCalcFindBox = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			this.GuaranteeTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GuaranteeRefNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GuaranteeAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DateAtCustomsOfficeDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.MessageModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.RegistrationDateLongDateEdit.SuspendLayout();
			this.TempRegNoDateEdit.SuspendLayout();
			this.DischargeRecordNoDateEdit.SuspendLayout();
			this.ClosureNoDateEdit.SuspendLayout();
			this.GoodsLocationCodeCodeFindBox.SuspendLayout();
			this.TransshipmentCountryCodeFindBox.SuspendLayout();
			this.DepartureCountryCodeFindBox.SuspendLayout();
			this.TransshipmentConveyanceCountryCodeFindBox.SuspendLayout();
			this.ProcedureCodeFindBox.SuspendLayout();
			this.PresentationCustomsOfficeDropEdit.SuspendLayout();
			this.ImportExportCustomsOfficeDropEdit.SuspendLayout();
			this.DischargeLoadingCustomsOfficeDropEdit.SuspendLayout();
			this.CustomsValueCalcFindBox.SuspendLayout();
			this.FreightValueCalcFindBox.SuspendLayout();
			this.InsuranceValueCalcFindBox.SuspendLayout();
			this.OtherValueCalcFindBox.SuspendLayout();
			this.GuaranteeTypeDropEdit.SuspendLayout();
			this.DateAtCustomsOfficeDateEdit.SuspendLayout();
			this.MessageModeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader);
			// 
			// DepartureFlightTextBox
			// 
			this.BindingSource.SetBindingMember(this.DepartureFlightTextBox, "DepartureFlight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader)(null)).DepartureFlight)));
			this.DepartureFlightTextBox.CaptionResourceString = null;
			this.DepartureFlightTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(550, 248, true);
			this.DepartureFlightTextBox.Name = "DepartureFlightTextBox";
			this.DepartureFlightTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.DepartureFlightTextBox.TabIndex = 94;
			// 
			// TransshipmentLocationTextBox
			// 
			this.BindingSource.SetBindingMember(this.TransshipmentLocationTextBox, "TransshipmentLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader)(null)).TransshipmentLocation)));
			this.TransshipmentLocationTextBox.CaptionResourceString = null;
			this.TransshipmentLocationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(622, 229, true);
			this.TransshipmentLocationTextBox.Name = "TransshipmentLocationTextBox";
			this.TransshipmentLocationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.TransshipmentLocationTextBox.TabIndex = 95;
			// 
			// TransshipmentReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.TransshipmentReferenceTextBox, "TransshipmentReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader)(null)).TransshipmentReference)));
			this.TransshipmentReferenceTextBox.CaptionResourceString = null;
			this.TransshipmentReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(635, 108, true);
			this.TransshipmentReferenceTextBox.Name = "TransshipmentReferenceTextBox";
			this.TransshipmentReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.TransshipmentReferenceTextBox.TabIndex = 96;
			// 
			// TempRegNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.TempRegNoTextBox, "TempRegNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader)(null)).TempRegNo)));
			this.TempRegNoTextBox.CaptionResourceString = null;
			this.TempRegNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(588, 15, true);
			this.TempRegNoTextBox.Name = "TempRegNoTextBox";
			this.TempRegNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.TempRegNoTextBox.TabIndex = 1;
			this.TempRegNoTextBox.TabStop = false;
			// 
			// TempRegNoDateEdit
			// 
			this.TempRegNoDateEdit.AllowDrop = true;
			this.TempRegNoDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.TempRegNoDateEdit, "TempRegNoDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader)(null)).TempRegNoDate)));
			this.TempRegNoDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.TempRegNoDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(588, 15, true);
			this.TempRegNoDateEdit.Name = "TempRegNoDateEdit";
			this.TempRegNoDateEdit.TabIndex = 1;
			this.TempRegNoDateEdit.TabStop = false;
			// 
			// RegistrationDateLongDateEdit
			// 
			this.RegistrationDateLongDateEdit.AllowDrop = true;
			this.RegistrationDateLongDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.RegistrationDateLongDateEdit, "RegistrationDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader)(null)).RegistrationDate)));
			this.RegistrationDateLongDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.RegistrationDateLongDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(588, 15, true);
			this.RegistrationDateLongDateEdit.Name = "RegistrationDateLongDateEdit";
			this.RegistrationDateLongDateEdit.TabIndex = 1;
			this.RegistrationDateLongDateEdit.TabStop = false;
			// 
			// DischargeRecordNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.DischargeRecordNoTextBox, "DischargeRecordNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader)(null)).DischargeRecordNo)));
			this.DischargeRecordNoTextBox.CaptionResourceString = null;
			this.DischargeRecordNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(588, 15, true);
			this.DischargeRecordNoTextBox.Name = "DischargeRecordNoTextBox";
			this.DischargeRecordNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.DischargeRecordNoTextBox.TabIndex = 1;
			this.DischargeRecordNoTextBox.TabStop = false;
			// 
			// DischargeRecordNoDateEdit
			// 
			this.DischargeRecordNoDateEdit.AllowDrop = true;
			this.DischargeRecordNoDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.DischargeRecordNoDateEdit, "DischargeRecordNoDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader)(null)).DischargeRecordNoDate)));
			this.DischargeRecordNoDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.DischargeRecordNoDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(588, 15, true);
			this.DischargeRecordNoDateEdit.Name = "DischargeRecordNoDateEdit";
			this.DischargeRecordNoDateEdit.TabIndex = 1;
			this.DischargeRecordNoDateEdit.TabStop = false;
			// 
			// ClosureNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.ClosureNoTextBox, "ClosureNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader)(null)).ClosureNo)));
			this.ClosureNoTextBox.CaptionResourceString = null;
			this.ClosureNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(588, 15, true);
			this.ClosureNoTextBox.Name = "ClosureNoTextBox";
			this.ClosureNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.ClosureNoTextBox.TabIndex = 1;
			this.ClosureNoTextBox.TabStop = false;
			// 
			// ClosureNoDateEdit
			// 
			this.ClosureNoDateEdit.AllowDrop = true;
			this.ClosureNoDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.ClosureNoDateEdit, "ClosureNoDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader)(null)).ClosureNoDate)));
			this.ClosureNoDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(588, 15, true);
			this.ClosureNoDateEdit.Name = "ClosureNoDateEdit";
			this.ClosureNoDateEdit.TabIndex = 1;
			this.ClosureNoDateEdit.TabStop = false;
			// 
			// NumberOfBillsTextBox
			// 
			this.BindingSource.SetBindingMember(this.NumberOfBillsTextBox, "NumberOfBills");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader)(null)).NumberOfBills)));
			this.NumberOfBillsTextBox.CaptionResourceString = null;
			this.NumberOfBillsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(588, 15, true);
			this.NumberOfBillsTextBox.Name = "NumberOfBillsTextBox";
			this.NumberOfBillsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.NumberOfBillsTextBox.TabIndex = 1;
			this.NumberOfBillsTextBox.TabStop = false;
			// 
			// TotalBoxQtyTextBox
			// 
			this.BindingSource.SetBindingMember(this.TotalBoxQtyTextBox, "TotalBoxQty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader)(null)).TotalBoxQty)));
			this.TotalBoxQtyTextBox.CaptionResourceString = null;
			this.TotalBoxQtyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(588, 15, true);
			this.TotalBoxQtyTextBox.Name = "TotalBoxQtyTextBox";
			this.TotalBoxQtyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.TotalBoxQtyTextBox.TabIndex = 1;
			this.TotalBoxQtyTextBox.TabStop = false;
			// 
			// InspectionClerkTextBox
			// 
			this.BindingSource.SetBindingMember(this.InspectionClerkTextBox, "InspectionClerk");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader)(null)).InspectionClerk)));
			this.InspectionClerkTextBox.CaptionResourceString = null;
			this.InspectionClerkTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(588, 15, true);
			this.InspectionClerkTextBox.Name = "InspectionClerkTextBox";
			this.InspectionClerkTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.InspectionClerkTextBox.TabIndex = 1;
			this.InspectionClerkTextBox.TabStop = false;
			// 
			// LocationInformationTextBox
			// 
			this.BindingSource.SetBindingMember(this.LocationInformationTextBox, "LocationInformation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader)(null)).LocationInformation)));
			this.LocationInformationTextBox.CaptionResourceString = null;
			this.LocationInformationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(312, 248, true);
			this.LocationInformationTextBox.Name = "LocationInformationTextBox";
			this.LocationInformationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.LocationInformationTextBox.TabIndex = 96;
			// 
			// GoodsLocationCodeCodeFindBox
			// 
			this.GoodsLocationCodeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsLocationCodeCodeFindBox, "GoodsLocationCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader)(null)).GoodsLocationCode)));
			this.GoodsLocationCodeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(312, 183, true);
			this.GoodsLocationCodeCodeFindBox.Name = "GoodsLocationCodeCodeFindBox";
			this.GoodsLocationCodeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.GoodsLocationCodeCodeFindBox.ParentType = null;
			this.GoodsLocationCodeCodeFindBox.PreBoundMaxLength = 3;
			this.GoodsLocationCodeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 20, true);
			this.GoodsLocationCodeCodeFindBox.TabIndex = 5;
			// 
			// TransshipmentCountryCodeFindBox
			// 
			this.TransshipmentCountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransshipmentCountryCodeFindBox, "TransshipmentCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader)(null)).TransshipmentCountry)));
			this.TransshipmentCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 99, true);
			this.TransshipmentCountryCodeFindBox.Name = "TransshipmentCountryCodeFindBox";
			this.TransshipmentCountryCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.TransshipmentCountryCodeFindBox.ParentType = null;
			this.TransshipmentCountryCodeFindBox.PreBoundMaxLength = 2;
			this.TransshipmentCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.TransshipmentCountryCodeFindBox.TabIndex = 97;
			// 
			// DepartureCountryCodeFindBox
			// 
			this.DepartureCountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DepartureCountryCodeFindBox, "DepartureCountryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader)(null)).DepartureCountryCode)));
			this.DepartureCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 99, true);
			this.DepartureCountryCodeFindBox.Name = "DepartureCountryCodeFindBox";
			this.DepartureCountryCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.DepartureCountryCodeFindBox.ParentType = null;
			this.DepartureCountryCodeFindBox.PreBoundMaxLength = 2;
			this.DepartureCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.DepartureCountryCodeFindBox.TabIndex = 98;
			// 
			// TransshipmentConveyanceCountryCodeFindBox
			// 
			this.TransshipmentConveyanceCountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransshipmentConveyanceCountryCodeFindBox, "TransshipmentConveyanceCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader)(null)).TransshipmentConveyanceCountry)));
			this.TransshipmentConveyanceCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 99, true);
			this.TransshipmentConveyanceCountryCodeFindBox.Name = "TransshipmentConveyanceCountryCodeFindBox";
			this.TransshipmentConveyanceCountryCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.TransshipmentConveyanceCountryCodeFindBox.ParentType = null;
			this.TransshipmentConveyanceCountryCodeFindBox.PreBoundMaxLength = 2;
			this.TransshipmentConveyanceCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.TransshipmentConveyanceCountryCodeFindBox.TabIndex = 99;
			// 
			// PreviousContainerNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.PreviousContainerNoTextBox, "PreviousContainerNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader)(null)).PreviousContainerNo)));
			this.PreviousContainerNoTextBox.CaptionResourceString = null;
			this.PreviousContainerNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(756, 149, true);
			this.PreviousContainerNoTextBox.Name = "PreviousContainerNoTextBox";
			this.PreviousContainerNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.PreviousContainerNoTextBox.TabIndex = 1;
			// 
			// NewContainerNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.NewContainerNoTextBox, "NewContainerNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader)(null)).NewContainerNo)));
			this.NewContainerNoTextBox.CaptionResourceString = null;
			this.NewContainerNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(468, 195, true);
			this.NewContainerNoTextBox.Name = "NewContainerNoTextBox";
			this.NewContainerNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.NewContainerNoTextBox.TabIndex = 1;
			// 
			// ProcedureCodeFindBox
			// 
			this.ProcedureCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ProcedureCodeFindBox, "ProcedureCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader)(null)).ProcedureCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader)(null)).Lookups.Procedures)));
			this.ProcedureCodeFindBox.BindToList = "Lookups+Procedures";
			this.ProcedureCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(588, 15, true);
			this.ProcedureCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusProcedure;
			this.ProcedureCodeFindBox.Name = "ProcedureCodeFindBox";
			this.ProcedureCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ProcedureCodeFindBox.ParentType = null;
			this.ProcedureCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.ProcedureCodeFindBox.TabIndex = 1;
			// 
			// PresentationCustomsOfficeDropEdit
			// 
			this.PresentationCustomsOfficeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PresentationCustomsOfficeDropEdit, "PresentationCustomsOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader)(null)).PresentationCustomsOffice)));
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.PresentationCustomsOfficeDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
			this.PresentationCustomsOfficeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 139, true);
			this.PresentationCustomsOfficeDropEdit.Name = "PresentationCustomsOfficeDropEdit";
			this.PresentationCustomsOfficeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 20, true);
			this.PresentationCustomsOfficeDropEdit.TabIndex = 100;
			// 
			// ImportExportCustomsOfficeDropEdit
			// 
			this.ImportExportCustomsOfficeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ImportExportCustomsOfficeDropEdit, "ImportExportCustomsOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader)(null)).ImportExportCustomsOffice)));
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.ImportExportCustomsOfficeDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
			this.ImportExportCustomsOfficeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 201, true);
			this.ImportExportCustomsOfficeDropEdit.Name = "ImportExportCustomsOfficeDropEdit";
			this.ImportExportCustomsOfficeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 20, true);
			this.ImportExportCustomsOfficeDropEdit.TabIndex = 101;
			// 
			// DischargeLoadingCustomsOfficeDropEdit
			// 
			this.DischargeLoadingCustomsOfficeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DischargeLoadingCustomsOfficeDropEdit, "DischargeLoadingCustomsOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader)(null)).DischargeLoadingCustomsOffice)));
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.DischargeLoadingCustomsOfficeDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
			this.DischargeLoadingCustomsOfficeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 157, true);
			this.DischargeLoadingCustomsOfficeDropEdit.Name = "DischargeLoadingCustomsOfficeDropEdit";
			this.DischargeLoadingCustomsOfficeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 20, true);
			this.DischargeLoadingCustomsOfficeDropEdit.TabIndex = 102;
			// 
			// GoodsDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.GoodsDescriptionTextBox, "GoodsDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader)(null)).GoodsDescription)));
			this.GoodsDescriptionTextBox.CaptionResourceString = null;
			this.GoodsDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(377, 15, true);
			this.GoodsDescriptionTextBox.Name = "GoodsDescriptionTextBox";
			this.GoodsDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.GoodsDescriptionTextBox.TabIndex = 103;
			// 
			// CustomsValueCalcFindBox
			// 
			this.CustomsValueCalcFindBox.AllowDrop = true;
			this.CustomsValueCalcFindBox.BindToAmount = "CustomsValue";
			this.CustomsValueCalcFindBox.BindToUnit = "CustomsValueCurrency";
			this.CustomsValueCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.CustomsValueCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(635, 71, true);
			this.CustomsValueCalcFindBox.Name = "CustomsValueCalcFindBox";
			this.CustomsValueCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.CustomsValueCalcFindBox.TabIndex = 105;
			// 
			// FreightValueCalcFindBox
			// 
			this.FreightValueCalcFindBox.AllowDrop = true;
			this.FreightValueCalcFindBox.BindToAmount = "FreightValue";
			this.FreightValueCalcFindBox.BindToUnit = "FreightValueCurrency";
			this.FreightValueCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.FreightValueCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(448, 103, true);
			this.FreightValueCalcFindBox.Name = "FreightValueCalcFindBox";
			this.FreightValueCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.FreightValueCalcFindBox.TabIndex = 105;
			// 
			// InsuranceValueCalcFindBox
			// 
			this.InsuranceValueCalcFindBox.AllowDrop = true;
			this.InsuranceValueCalcFindBox.BindToAmount = "InsuranceValue";
			this.InsuranceValueCalcFindBox.BindToUnit = "InsuranceValueCurrency";
			this.InsuranceValueCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.InsuranceValueCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(433, 127, true);
			this.InsuranceValueCalcFindBox.Name = "InsuranceValueCalcFindBox";
			this.InsuranceValueCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.InsuranceValueCalcFindBox.TabIndex = 105;
			// 
			// ExchangeRateCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ExchangeRateCalcEdit, "ExchangeRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader)(null)).ExchangeRate)));
			this.ExchangeRateCalcEdit.CaptionResourceString = null;
			this.ExchangeRateCalcEdit.DecimalPlaces = 2;
			this.ExchangeRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(361, 76, true);
			this.ExchangeRateCalcEdit.Name = "ExchangeRateCalcEdit";
			this.ExchangeRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
			this.ExchangeRateCalcEdit.TabIndex = 104;
			this.ExchangeRateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// StampTaxValueTextBox
			// 
			this.BindingSource.SetBindingMember(this.StampTaxValueTextBox, "StampTaxValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader)(null)).StampTaxValue)));
			this.StampTaxValueTextBox.CaptionResourceString = null;
			this.StampTaxValueTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(286, 327, true);
			this.StampTaxValueTextBox.Name = "StampTaxValueTextBox";
			this.StampTaxValueTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
			this.StampTaxValueTextBox.TabIndex = 0;
			// 
			// OtherValueCalcFindBox
			// 
			this.OtherValueCalcFindBox.AllowDrop = true;
			this.OtherValueCalcFindBox.BindToAmount = "OtherValue";
			this.OtherValueCalcFindBox.BindToUnit = "OtherValueCurrency";
			this.OtherValueCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.OtherValueCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(433, 157, true);
			this.OtherValueCalcFindBox.Name = "OtherValueCalcFindBox";
			this.OtherValueCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.OtherValueCalcFindBox.TabIndex = 105;
			// 
			// GuaranteeTypeDropEdit
			// 
			this.GuaranteeTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GuaranteeTypeDropEdit, "GuaranteeType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader)(null)).GuaranteeType)));
			this.GuaranteeTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 64, true);
			this.GuaranteeTypeDropEdit.Name = "GuaranteeTypeDropEdit";
			this.GuaranteeTypeDropEdit.PreBoundMaxLength = 1;
			this.GuaranteeTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(155, 20, true);
			this.GuaranteeTypeDropEdit.TabIndex = 106;
			// 
			// GuaranteeRefNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.GuaranteeRefNoTextBox, "GuaranteeRefNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader)(null)).GuaranteeRefNo)));
			this.GuaranteeRefNoTextBox.CaptionResourceString = null;
			this.GuaranteeRefNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(561, 282, true);
			this.GuaranteeRefNoTextBox.Name = "GuaranteeRefNoTextBox";
			this.GuaranteeRefNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.GuaranteeRefNoTextBox.TabIndex = 107;
			// 
			// GuaranteeAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.GuaranteeAmountCalcEdit, "GuaranteeAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader)(null)).GuaranteeAmount)));
			this.GuaranteeAmountCalcEdit.CaptionResourceString = null;
			this.GuaranteeAmountCalcEdit.DecimalPlaces = 2;
			this.GuaranteeAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(408, 56, true);
			this.GuaranteeAmountCalcEdit.Name = "GuaranteeAmountCalcEdit";
			this.GuaranteeAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.GuaranteeAmountCalcEdit.TabIndex = 108;
			this.GuaranteeAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DateAtCustomsOfficeDateEdit
			// 
			this.DateAtCustomsOfficeDateEdit.AllowDrop = true;
			this.DateAtCustomsOfficeDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.DateAtCustomsOfficeDateEdit, "AMA_DateAtCustomsOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader)(null)).AMA_DateAtCustomsOffice)));
			this.DateAtCustomsOfficeDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 22, true);
			this.DateAtCustomsOfficeDateEdit.Name = "DateAtCustomsOfficeDateEdit";
			this.DateAtCustomsOfficeDateEdit.TabIndex = 4;
			// 
			// MessageModeDropEdit
			// 
			this.MessageModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessageModeDropEdit, "MessageMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.ETrade.Business.AsycudaManifestHeader)(null)).MessageMode)));
			this.MessageModeDropEdit.CaptionResourceString = Enterprise.Customs.TR.ETrade.GUI.Res.GetData("D20412A7-0A15-42FC-A2DF-BFA0150A147A", "Message Mode");
			this.MessageModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(536, 327, true);
			this.MessageModeDropEdit.Name = "MessageModeDropEdit";
			this.MessageModeDropEdit.PreBoundMaxLength = 3;
			this.MessageModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.MessageModeDropEdit.TabIndex = 109;
			// 
			// ETradeMainUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.RegistrationDateLongDateEdit);
			this.Controls.Add(this.MessageModeDropEdit);
			this.Controls.Add(this.TransshipmentCountryCodeFindBox);
			this.Controls.Add(this.DepartureCountryCodeFindBox);
			this.Controls.Add(this.TransshipmentConveyanceCountryCodeFindBox);
			this.Controls.Add(this.TransshipmentReferenceTextBox);
			this.Controls.Add(this.TransshipmentLocationTextBox);
			this.Controls.Add(this.DepartureFlightTextBox);
			this.Controls.Add(this.PreviousContainerNoTextBox);
			this.Controls.Add(this.NewContainerNoTextBox);
			this.Controls.Add(this.PresentationCustomsOfficeDropEdit);
			this.Controls.Add(this.ImportExportCustomsOfficeDropEdit);
			this.Controls.Add(this.DischargeLoadingCustomsOfficeDropEdit);
			this.Controls.Add(this.GoodsDescriptionTextBox);
			this.Controls.Add(this.CustomsValueCalcFindBox);
			this.Controls.Add(this.ExchangeRateCalcEdit);
			this.Controls.Add(this.StampTaxValueTextBox);
			this.Controls.Add(this.OtherValueCalcFindBox);
			this.Controls.Add(this.FreightValueCalcFindBox);
			this.Controls.Add(this.InsuranceValueCalcFindBox);
			this.Controls.Add(this.GuaranteeTypeDropEdit);
			this.Controls.Add(this.GuaranteeRefNoTextBox);
			this.Controls.Add(this.GuaranteeAmountCalcEdit);
			this.Controls.Add(this.TempRegNoTextBox);
			this.Controls.Add(this.TempRegNoDateEdit);
			this.Controls.Add(this.DischargeRecordNoTextBox);
			this.Controls.Add(this.DischargeRecordNoDateEdit);
			this.Controls.Add(this.ClosureNoTextBox);
			this.Controls.Add(this.ClosureNoDateEdit);
			this.Controls.Add(this.InspectionClerkTextBox);
			this.Controls.Add(this.NumberOfBillsTextBox);
			this.Controls.Add(this.TotalBoxQtyTextBox);
			this.Controls.Add(this.ProcedureCodeFindBox);
			this.Controls.Add(this.LocationInformationTextBox);
			this.Controls.Add(this.GoodsLocationCodeCodeFindBox);
			this.Controls.Add(this.DateAtCustomsOfficeDateEdit);
			this.Name = "ETradeMainUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(995, 453, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.RegistrationDateLongDateEdit.ResumeLayout(true);
			this.RegistrationDateLongDateEdit.PerformLayout();
			this.TempRegNoDateEdit.ResumeLayout(true);
			this.TempRegNoDateEdit.PerformLayout();
			this.DischargeRecordNoDateEdit.ResumeLayout(true);
			this.DischargeRecordNoDateEdit.PerformLayout();
			this.ClosureNoDateEdit.ResumeLayout(true);
			this.ClosureNoDateEdit.PerformLayout();
			this.GoodsLocationCodeCodeFindBox.ResumeLayout(true);
			this.GoodsLocationCodeCodeFindBox.PerformLayout();
			this.TransshipmentCountryCodeFindBox.ResumeLayout(true);
			this.TransshipmentCountryCodeFindBox.PerformLayout();
			this.DepartureCountryCodeFindBox.ResumeLayout(true);
			this.DepartureCountryCodeFindBox.PerformLayout();
			this.TransshipmentConveyanceCountryCodeFindBox.ResumeLayout(true);
			this.TransshipmentConveyanceCountryCodeFindBox.PerformLayout();
			this.ProcedureCodeFindBox.ResumeLayout(true);
			this.ProcedureCodeFindBox.PerformLayout();
			this.PresentationCustomsOfficeDropEdit.ResumeLayout(true);
			this.PresentationCustomsOfficeDropEdit.PerformLayout();
			this.ImportExportCustomsOfficeDropEdit.ResumeLayout(true);
			this.ImportExportCustomsOfficeDropEdit.PerformLayout();
			this.DischargeLoadingCustomsOfficeDropEdit.ResumeLayout(true);
			this.DischargeLoadingCustomsOfficeDropEdit.PerformLayout();
			this.CustomsValueCalcFindBox.ResumeLayout(true);
			this.CustomsValueCalcFindBox.PerformLayout();
			this.FreightValueCalcFindBox.ResumeLayout(true);
			this.FreightValueCalcFindBox.PerformLayout();
			this.InsuranceValueCalcFindBox.ResumeLayout(true);
			this.InsuranceValueCalcFindBox.PerformLayout();
			this.OtherValueCalcFindBox.ResumeLayout(true);
			this.OtherValueCalcFindBox.PerformLayout();
			this.GuaranteeTypeDropEdit.ResumeLayout(true);
			this.GuaranteeTypeDropEdit.PerformLayout();
			this.DateAtCustomsOfficeDateEdit.ResumeLayout(true);
			this.DateAtCustomsOfficeDateEdit.PerformLayout();
			this.MessageModeDropEdit.ResumeLayout(true);
			this.MessageModeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		internal Enterprise.ZArchitecture.GUI.ZDateEdit RegistrationDateLongDateEdit;
		internal Customs.GUI.ConvertToLocalCurrencyControl CustomsValueCalcFindBox;
		internal Customs.GUI.ConvertToLocalCurrencyControl OtherValueCalcFindBox;
		internal Customs.GUI.ConvertToLocalCurrencyControl FreightValueCalcFindBox;
		internal Customs.GUI.ConvertToLocalCurrencyControl InsuranceValueCalcFindBox;
		internal Enterprise.ZArchitecture.ZTextBox DepartureFlightTextBox;
		internal Enterprise.ZArchitecture.ZTextBox TransshipmentLocationTextBox;
		internal Enterprise.ZArchitecture.ZTextBox TransshipmentReferenceTextBox;
		internal Enterprise.ZArchitecture.ZTextBox TempRegNoTextBox;
		internal Enterprise.ZArchitecture.GUI.ZDateEdit TempRegNoDateEdit;
		internal Enterprise.ZArchitecture.ZTextBox DischargeRecordNoTextBox;
		internal Enterprise.ZArchitecture.GUI.ZDateEdit DischargeRecordNoDateEdit;
		internal Enterprise.ZArchitecture.ZTextBox ClosureNoTextBox;
		internal Enterprise.ZArchitecture.GUI.ZDateEdit ClosureNoDateEdit;
		internal Enterprise.ZArchitecture.ZTextBox InspectionClerkTextBox;
		internal Enterprise.ZArchitecture.ZTextBox NumberOfBillsTextBox;
		internal Enterprise.ZArchitecture.ZTextBox TotalBoxQtyTextBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox TransshipmentCountryCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox DepartureCountryCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox TransshipmentConveyanceCountryCodeFindBox;
		internal Enterprise.ZArchitecture.ZTextBox PreviousContainerNoTextBox;
		internal Enterprise.ZArchitecture.ZTextBox NewContainerNoTextBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox ProcedureCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit PresentationCustomsOfficeDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit ImportExportCustomsOfficeDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit DischargeLoadingCustomsOfficeDropEdit;
		internal Enterprise.ZArchitecture.ZTextBox GoodsDescriptionTextBox;
		internal Enterprise.ZArchitecture.ZCalcEdit ExchangeRateCalcEdit;
		internal Enterprise.ZArchitecture.ZTextBox LocationInformationTextBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox GoodsLocationCodeCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZDateEdit DateAtCustomsOfficeDateEdit;
		internal Enterprise.ZArchitecture.ZTextBox StampTaxValueTextBox;
		internal ZArchitecture.GUI.ZDropEdit GuaranteeTypeDropEdit;
		internal ZArchitecture.ZTextBox GuaranteeRefNoTextBox;
		internal ZArchitecture.ZCalcEdit GuaranteeAmountCalcEdit;
		internal ZArchitecture.GUI.ZDropEdit MessageModeDropEdit;
	}
}
