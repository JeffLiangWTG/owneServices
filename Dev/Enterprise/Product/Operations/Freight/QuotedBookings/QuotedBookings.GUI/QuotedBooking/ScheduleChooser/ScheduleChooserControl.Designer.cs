
namespace Enterprise.Freight.QuotedBookings.GUI
{
	partial class ScheduleChooserControl
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
			this.ETDZDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JS_RL_NKDestinationBoundCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JS_RL_NKOriginBoundCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.LoadingPortCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.DischargePortCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.SailingLoadingPortBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SailingDischargePortBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ETAZDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.BookingRefTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ViewLoadListButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ImportGlobalSchedulesButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ViewSailingButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AddNewSailingButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ClearSailingButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SailingTotalWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.BookedShippingLineBoundOrgFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			this.CreditorFindBox = new MasterFiles.GUI.ZOrganisationFindBox();
			this.VoyageNumberBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.VesselBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JS_Calc_DepotCutOffBoundReadOnlyDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ARVBoundReadOnlyDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JS_Calc_FCLCutOffBoundReadOnlyDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DEPBoundReadOnlyDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.SailingTotalVolumeCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.ButtonSelectFromConsortium = new Enterprise.ZArchitecture.GUI.ZDropButtonOnly();
			this.JS_IsDirect = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.JS_IsNeutral = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.MAWBNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MAWBLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MAWBPrefixTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MAWBHyphenLabel = new Enterprise.ZArchitecture.ZLabel();
			this.JS_AWBServiceLevelDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SailingSummarygroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MAWBSeaNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.HBLNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JS_AWBServiceLevelLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MAWBPendingAllocationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CarrierLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CreditorLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ETDZDateEdit.SuspendLayout();
			this.JS_RL_NKDestinationBoundCodeFindBox.SuspendLayout();
			this.JS_RL_NKOriginBoundCodeFindBox.SuspendLayout();
			this.LoadingPortCodeFindBox.SuspendLayout();
			this.DischargePortCodeFindBox.SuspendLayout();
			this.ETAZDateEdit.SuspendLayout();
			this.SailingTotalWeightCalcDropEdit.SuspendLayout();
			this.BookedShippingLineBoundOrgFindBox.SuspendLayout();
			this.JS_Calc_DepotCutOffBoundReadOnlyDateEdit.SuspendLayout();
			this.ARVBoundReadOnlyDateEdit.SuspendLayout();
			this.JS_Calc_FCLCutOffBoundReadOnlyDateEdit.SuspendLayout();
			this.DEPBoundReadOnlyDateEdit.SuspendLayout();
			this.SailingTotalVolumeCalcDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ButtonSelectFromConsortium)).BeginInit();
			this.JS_AWBServiceLevelDropEdit.SuspendLayout();
			this.SailingSummarygroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.QuotedBookings.Business.QuotedBooking);
			// 
			// ETDZDateEdit
			// 
			this.ETDZDateEdit.AutoCompleteMonthThreshold = 1;
			this.ETDZDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ETDZDateEdit, "ETD");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).ETD)));
			this.ETDZDateEdit.CaptionResourceString = Res.GetData("ScheduleChooserControl|f2b7648d-25d3-4316-8f38-5bb8ecf43b3f", "ETD");
			this.ETDZDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ETDZDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(272, 0, true);
			this.ETDZDateEdit.Name = "ETDZDateEdit";
			this.ETDZDateEdit.TabIndex = 1;
			// 
			// JS_RL_NKDestinationBoundCodeFindBox
			// 
			this.BindingSource.SetBindingMember(this.JS_RL_NKDestinationBoundCodeFindBox, "Destination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Destination)));
			this.JS_RL_NKDestinationBoundCodeFindBox.CaptionResourceString = Res.GetData("ScheduleChooserControl|0bdbdccf-43fb-4201-9ea1-dd74faf4673d", "Dest.", "Destination", "The destination of the chosen Sailing schedule.");
			this.JS_RL_NKDestinationBoundCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(48, 22, true);
			this.JS_RL_NKDestinationBoundCodeFindBox.Name = "JS_RL_NKDestinationBoundCodeFindBox";
			this.JS_RL_NKDestinationBoundCodeFindBox.PopupCaption = "Select Destination";
			this.JS_RL_NKDestinationBoundCodeFindBox.PreBoundMaxLength = 5;
			this.JS_RL_NKDestinationBoundCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 20, true);
			this.JS_RL_NKDestinationBoundCodeFindBox.TabIndex = 3;
			// 
			// JS_RL_NKOriginBoundCodeFindBox
			// 
			this.BindingSource.SetBindingMember(this.JS_RL_NKOriginBoundCodeFindBox, "Origin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Origin)));
			this.JS_RL_NKOriginBoundCodeFindBox.CaptionResourceString = Res.GetData("ScheduleChooserControl|474a0c42-39b6-47cd-9697-45209067688c", "Origin", "The origin of the chosen Sailing schedule.");
			this.JS_RL_NKOriginBoundCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(48, 0, true);
			this.JS_RL_NKOriginBoundCodeFindBox.Name = "JS_RL_NKOriginBoundCodeFindBox";
			this.JS_RL_NKOriginBoundCodeFindBox.PopupCaption = "Select Origin";
			this.JS_RL_NKOriginBoundCodeFindBox.PreBoundMaxLength = 5;
			this.JS_RL_NKOriginBoundCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 20, true);
			this.JS_RL_NKOriginBoundCodeFindBox.TabIndex = 0;
			// 
			// LoadingPortCodeFindBox
			// 
			this.BindingSource.SetBindingMember(this.LoadingPortCodeFindBox, "LoadPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).LoadPort)));
			this.LoadingPortCodeFindBox.CaptionResourceString = Res.GetData("ScheduleChooserControl|b0818fb0-4921-7ebf-4a7c-d9328cc5ddda", "Load", "Load Port");
			this.LoadingPortCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(452, 0, true);
			this.LoadingPortCodeFindBox.Name = "LoadingPortCodeFindBox";
			this.LoadingPortCodeFindBox.PopupCaption = "Select Load Port";
			this.LoadingPortCodeFindBox.PreBoundMaxLength = 5;
			this.LoadingPortCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 20, true);
			this.LoadingPortCodeFindBox.TabIndex = 2;
			// 
			// DischargePortCodeFindBox
			// 
			this.BindingSource.SetBindingMember(this.DischargePortCodeFindBox, "DischargePort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).DischargePort)));
			this.DischargePortCodeFindBox.CaptionResourceString = Res.GetData("ScheduleChooserControl|6e2196a7-1c6e-5e8b-46e9-98cc37214741", "Disch.", "Discharge", "Discharge Port");
			this.DischargePortCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(452, 22, true);
			this.DischargePortCodeFindBox.Name = "DischargePortCodeFindBox";
			this.DischargePortCodeFindBox.PopupCaption = "Select Discharge Port";
			this.DischargePortCodeFindBox.PreBoundMaxLength = 5;
			this.DischargePortCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 20, true);
			this.DischargePortCodeFindBox.TabIndex = 5;
			// 
			// ETAZDateEdit
			// 
			this.ETAZDateEdit.AutoCompleteMonthThreshold = 1;
			this.ETAZDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ETAZDateEdit, "ETA");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).ETA)));
			this.ETAZDateEdit.CaptionResourceString = Res.GetData("ScheduleChooserControl|dedf2a4e-d6d4-4354-ab29-2823dd0e021a", "ETA");
			this.ETAZDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ETAZDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(272, 22, true);
			this.ETAZDateEdit.Name = "ETAZDateEdit";
			this.ETAZDateEdit.TabIndex = 4;
			// 
			// BookingRefTextBox
			// 
			this.BindingSource.SetBindingMember(this.BookingRefTextBox, "Booking+JS_CFSReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.JS_CFSReference)));
			this.BookingRefTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(372, 104, true);
			this.BookingRefTextBox.Name = "BookingRefTextBox";
			this.BookingRefTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 20, true);
			this.BookingRefTextBox.TabIndex = 12;
			// 
			// ViewLoadListButton
			// 
			this.ViewLoadListButton.CaptionResourceString = Res.GetData("ScheduleChooserControl|9499fdf5-d95c-408a-b6a4-0509d2bf9bac", "Load List");
			this.ViewLoadListButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(539, 81, true);
			this.ViewLoadListButton.Name = "ViewLoadListButton";
			this.ViewLoadListButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 21, true);
			this.ViewLoadListButton.TabIndex = 13;
			this.ViewLoadListButton.Click += new System.EventHandler(this.ViewLoadListButton_Click);
			// 
			// ImportGlobalSchedulesButton
			// 
			this.ImportGlobalSchedulesButton.CaptionResourceString = Res.GetData("ScheduleChooserControl|ImportGlobalSchedulesButton", "Import Global Sailing Schedule", "Import...", "Import Global Sailing Schedule");
			this.ImportGlobalSchedulesButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(539, 81, true);
			this.ImportGlobalSchedulesButton.Name = "ImportGlobalSchedulesButton";
			this.ImportGlobalSchedulesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 21, true);
			this.ImportGlobalSchedulesButton.TabIndex = 13;
			this.ImportGlobalSchedulesButton.Click += new System.EventHandler(this.ImportGlobalSchedulesButton_Click);
			// 
			// ViewSailingButton
			// 
			this.ViewSailingButton.CaptionResourceString = Res.GetData("ScheduleChooserControl|4cec9593-8f81-4630-a6a3-604ced2ab515", "View Sailings");
			this.ViewSailingButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(539, 37, true);
			this.ViewSailingButton.Name = "ViewSailingButton";
			this.ViewSailingButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 21, true);
			this.ViewSailingButton.TabIndex = 5;
			this.ViewSailingButton.Click += new System.EventHandler(this.ViewSailingButton_Click_1);
			// 
			// AddNewSailingButton
			// 
			this.AddNewSailingButton.CaptionResourceString = Res.GetData("ScheduleChooserControl|26a301be-aa4c-496c-9a0a-6d3de3482a9f", "Add New Sailing");
			this.AddNewSailingButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(539, 15, true);
			this.AddNewSailingButton.Name = "AddNewSailingButton";
			this.AddNewSailingButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 21, true);
			this.AddNewSailingButton.TabIndex = 2;
			this.AddNewSailingButton.Click += new System.EventHandler(this.AddNewSailingButton_Click_1);
			// 
			// ClearSailingButton
			// 
			this.ClearSailingButton.CaptionResourceString = Res.GetData("ScheduleChooserControl|466185e7-bce1-4af3-a9aa-7132e2378957", "Clear Sailing");
			this.ClearSailingButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(539, 59, true);
			this.ClearSailingButton.Name = "ClearSailingButton";
			this.ClearSailingButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 21, true);
			this.ClearSailingButton.TabIndex = 8;
			this.ClearSailingButton.Click += new System.EventHandler(this.ClearSailingButton_Click_1);
			// 
			// SailingTotalWeightCalcDropEdit
			// 
			this.BindingSource.SetBindingMember(this.SailingTotalWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).ScheduleChooser.Sailing.TotalWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).ScheduleChooser.Sailing.TotalWeightUnit)));
			this.SailingTotalWeightCalcDropEdit.BindToAmount = "ScheduleChooser+Sailing+TotalWeight";
			this.SailingTotalWeightCalcDropEdit.BindToUnit = "ScheduleChooser+Sailing+TotalWeightUnit";
			this.SailingTotalWeightCalcDropEdit.CaptionResourceString = Res.GetData("ScheduleChooserControl|1d6886d9-4fcf-410a-91db-fb6532ea3761", "Total Current Weight");
			this.SailingTotalWeightCalcDropEdit.Decimals = 3;
			this.SailingTotalWeightCalcDropEdit.Enabled = false;
			this.SailingTotalWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 104, true);
			this.SailingTotalWeightCalcDropEdit.Name = "SailingTotalWeightCalcDropEdit";
			this.SailingTotalWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.SailingTotalWeightCalcDropEdit.TabIndex = 15;
			this.SailingTotalWeightCalcDropEdit.TabStop = false;
			this.SailingTotalWeightCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// JS_OH_BookedShippingLineBoundOrgFindBox
			// 
			this.BindingSource.SetBindingMember(this.BookedShippingLineBoundOrgFindBox, "Booking+BookedShippingLinePK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.BookedShippingLinePK)));
			this.BookedShippingLineBoundOrgFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 104, true);
			this.BookedShippingLineBoundOrgFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.BookedShippingLineBoundOrgFindBox.Name = "BookedShippingLineBoundOrgFindBox";
			this.BookedShippingLineBoundOrgFindBox.ShowDescriptionBox = false;
			this.BookedShippingLineBoundOrgFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.BookedShippingLineBoundOrgFindBox.TabIndex = 10;
			this.BookedShippingLineBoundOrgFindBox.Visible = false;
			// 
			// Creditor
			// 
			this.BindingSource.SetBindingMember(this.CreditorFindBox, "Creditor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Creditor)));
			this.CreditorFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 126, true);
			this.CreditorFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.CreditorFindBox.Name = "CreditorFindBox";
			this.CreditorFindBox.ShowDescriptionBox = false;
			this.CreditorFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.CreditorFindBox.TabIndex = 10;
			this.CreditorFindBox.Visible = false;
			// 
			// VoyageNumberBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.VoyageNumberBoundTextBox, "ScheduleChooser+Sailing+JX_JV_VoyageFlight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).ScheduleChooser.Sailing.JX_JV_VoyageFlight)));
			this.VoyageNumberBoundTextBox.CaptionResourceString = Res.GetData("ScheduleChooserControl|0fc7abe0-004f-4608-b64a-9ab3fd37a6c9", "Voyage No");
			this.VoyageNumberBoundTextBox.Enabled = false;
			this.VoyageNumberBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 38, true);
			this.VoyageNumberBoundTextBox.Name = "VoyageNumberBoundTextBox";
			this.VoyageNumberBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.VoyageNumberBoundTextBox.TabIndex = 0;
			this.VoyageNumberBoundTextBox.TabStop = false;
			// 
			// VesselBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.VesselBoundTextBox, "ScheduleChooser+Sailing+JX_JV_NKVessel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).ScheduleChooser.Sailing.JX_JV_NKVessel)));
			this.VesselBoundTextBox.CaptionResourceString = Res.GetData("ScheduleChooserControl|27a16db7-49d5-4b61-889a-682a2829f5af", "Vessel");
			this.VesselBoundTextBox.Enabled = false;
			this.VesselBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(372, 38, true);
			this.VesselBoundTextBox.Name = "VesselBoundTextBox";
			this.VesselBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 20, true);
			this.VesselBoundTextBox.TabIndex = 1;
			this.VesselBoundTextBox.TabStop = false;
			// 
			// JS_Calc_DepotCutOffBoundReadOnlyDateEdit
			// 
			this.JS_Calc_DepotCutOffBoundReadOnlyDateEdit.AutoCompleteMonthThreshold = 1;
			this.JS_Calc_DepotCutOffBoundReadOnlyDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JS_Calc_DepotCutOffBoundReadOnlyDateEdit, "ScheduleChooser+Sailing+JX_DepotCutOff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).ScheduleChooser.Sailing.JX_DepotCutOff)));
			this.JS_Calc_DepotCutOffBoundReadOnlyDateEdit.CaptionResourceString = Res.GetData("ScheduleChooserControl|9a6b7dc7-08a8-429c-8fb3-07a4e508cd04", "CFS Cut Off");
			this.JS_Calc_DepotCutOffBoundReadOnlyDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JS_Calc_DepotCutOffBoundReadOnlyDateEdit.Enabled = false;
			this.JS_Calc_DepotCutOffBoundReadOnlyDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 60, true);
			this.JS_Calc_DepotCutOffBoundReadOnlyDateEdit.Name = "JS_Calc_DepotCutOffBoundReadOnlyDateEdit";
			this.JS_Calc_DepotCutOffBoundReadOnlyDateEdit.TabIndex = 3;
			this.JS_Calc_DepotCutOffBoundReadOnlyDateEdit.TabStop = false;
			// 
			// ARVBoundReadOnlyDateEdit
			// 
			this.ARVBoundReadOnlyDateEdit.AutoCompleteMonthThreshold = 1;
			this.ARVBoundReadOnlyDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ARVBoundReadOnlyDateEdit, "ScheduleChooser+Sailing+JX_JB_E_ARV");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).ScheduleChooser.Sailing.JX_JB_E_ARV)));
			this.ARVBoundReadOnlyDateEdit.CaptionResourceString = Res.GetData("ScheduleChooserControl|aedd36b4-d880-4343-b603-9a7b114d9c76", "Estimated Arrival Date");
			this.ARVBoundReadOnlyDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ARVBoundReadOnlyDateEdit.Enabled = false;
			this.ARVBoundReadOnlyDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(372, 82, true);
			this.ARVBoundReadOnlyDateEdit.Name = "ARVBoundReadOnlyDateEdit";
			this.ARVBoundReadOnlyDateEdit.TabIndex = 7;
			this.ARVBoundReadOnlyDateEdit.TabStop = false;
			// 
			// JS_Calc_FCLCutOffBoundReadOnlyDateEdit
			// 
			this.JS_Calc_FCLCutOffBoundReadOnlyDateEdit.AutoCompleteMonthThreshold = 1;
			this.JS_Calc_FCLCutOffBoundReadOnlyDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JS_Calc_FCLCutOffBoundReadOnlyDateEdit, "ScheduleChooser+Sailing+JX_JA_CTOCutOff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).ScheduleChooser.Sailing.JX_JA_CTOCutOff)));
			this.JS_Calc_FCLCutOffBoundReadOnlyDateEdit.CaptionResourceString = Res.GetData("ScheduleChooserControl|5d8568c8-9bee-49bd-9dda-546435319fd9", "CTO Cut Off");
			this.JS_Calc_FCLCutOffBoundReadOnlyDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JS_Calc_FCLCutOffBoundReadOnlyDateEdit.Enabled = false;
			this.JS_Calc_FCLCutOffBoundReadOnlyDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(372, 60, true);
			this.JS_Calc_FCLCutOffBoundReadOnlyDateEdit.Name = "JS_Calc_FCLCutOffBoundReadOnlyDateEdit";
			this.JS_Calc_FCLCutOffBoundReadOnlyDateEdit.TabIndex = 4;
			this.JS_Calc_FCLCutOffBoundReadOnlyDateEdit.TabStop = false;
			// 
			// DEPBoundReadOnlyDateEdit
			// 
			this.DEPBoundReadOnlyDateEdit.AutoCompleteMonthThreshold = 1;
			this.DEPBoundReadOnlyDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DEPBoundReadOnlyDateEdit, "ScheduleChooser+Sailing+JX_JA_E_DEP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).ScheduleChooser.Sailing.JX_JA_E_DEP)));
			this.DEPBoundReadOnlyDateEdit.CaptionResourceString = Res.GetData("ScheduleChooserControl|3500020e-d280-46f1-9a98-3c8a0ef968f3", "Estimated Departure Date");
			this.DEPBoundReadOnlyDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.DEPBoundReadOnlyDateEdit.Enabled = false;
			this.DEPBoundReadOnlyDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 82, true);
			this.DEPBoundReadOnlyDateEdit.Name = "DEPBoundReadOnlyDateEdit";
			this.DEPBoundReadOnlyDateEdit.TabIndex = 6;
			this.DEPBoundReadOnlyDateEdit.TabStop = false;
			// 
			// SailingTotalVolumeCalcDropEdit
			// 
			this.BindingSource.SetBindingMember(this.SailingTotalVolumeCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).ScheduleChooser.Sailing.TotalVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).ScheduleChooser.Sailing.TotalVolumeUnit)));
			this.SailingTotalVolumeCalcDropEdit.BindToAmount = "ScheduleChooser+Sailing+TotalVolume";
			this.SailingTotalVolumeCalcDropEdit.BindToUnit = "ScheduleChooser+Sailing+TotalVolumeUnit";
			this.SailingTotalVolumeCalcDropEdit.CaptionResourceString = Res.GetData("ScheduleChooserControl|5dc8287d-f3ba-4ce0-98f9-16c21c70fec9", "Total Current Volume");
			this.SailingTotalVolumeCalcDropEdit.Decimals = 3;
			this.SailingTotalVolumeCalcDropEdit.Enabled = false;
			this.SailingTotalVolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(372, 104, true);
			this.SailingTotalVolumeCalcDropEdit.Name = "SailingTotalVolumeCalcDropEdit";
			this.SailingTotalVolumeCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.SailingTotalVolumeCalcDropEdit.TabIndex = 11;
			this.SailingTotalVolumeCalcDropEdit.UnitPreBoundMaxLength = 3;
			this.SailingTotalVolumeCalcDropEdit.Visible = false;
			// 
			// ButtonSelectFromConsortium
			// 
			this.ButtonSelectFromConsortium.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 102, true);
			this.ButtonSelectFromConsortium.Name = "ButtonSelectFromConsortium";
			this.ButtonSelectFromConsortium.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 23, true);
			this.ButtonSelectFromConsortium.TabIndex = 9;
			this.ButtonSelectFromConsortium.TabStop = false;
			this.ButtonSelectFromConsortium.Click += ButtonSelectFromConsortium_Click;
			// 
			// JS_IsDirect
			// 
			this.JS_IsDirect.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.JS_IsDirect.AutoSize = true;
			this.BindingSource.SetBindingMember(this.JS_IsDirect, "Booking+JS_IsDirectBooking");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.JS_IsDirectBooking)));
			this.JS_IsDirect.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.JS_IsDirect.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 147, true);
			this.JS_IsDirect.Name = "JS_IsDirect";
			this.JS_IsDirect.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, 17, true);
			this.JS_IsDirect.TabIndex = 15;
			this.JS_IsDirect.Visible = false;
			// 
			// JS_IsNeutral
			// 
			this.JS_IsNeutral.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.JS_IsNeutral, "Booking+JS_IsNeutralMaster");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.JS_IsNeutralMaster)));
			this.JS_IsNeutral.CaptionResourceString = Res.GetData("ScheduleChooserControl|87990849-e1f5-4f01-9dec-b495d9c99800", "Is Neutral", "Is Neutral", "Is Neutral", "");
			this.JS_IsNeutral.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.JS_IsNeutral.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(542, 105, true);
			this.JS_IsNeutral.Name = "JS_IsNeutral";
			this.JS_IsNeutral.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 17, true);
			this.JS_IsNeutral.TabIndex = 18;
			this.JS_IsNeutral.Visible = false;
			this.JS_IsNeutral.CheckedChanged += JS_IsNeutral_CheckedChanged;
			// 
			// MAWBNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.MAWBNumberTextBox, "ScheduleChooser+MasterBillMAWB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).ScheduleChooser.MasterBillMAWB)));
			this.MAWBNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(411, 126, true);
			this.MAWBNumberTextBox.Name = "MAWBNumberTextBox";
			this.MAWBNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.MAWBNumberTextBox.TabIndex = 22;
			this.MAWBNumberTextBox.Visible = false;
			// 
			// MAWBLabel
			// 
			this.MAWBLabel.AutoSize = true;
			this.MAWBLabel.CaptionResourceString = Res.GetData("ScheduleChooserControl|b99327f4-da52-4f34-97e4-590486c62131", "MAWB:");
			this.MAWBLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(324, 129, true);
			this.MAWBLabel.Name = "MAWBLabel";
			this.MAWBLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(44, 13, true);
			this.MAWBLabel.TabIndex = 19;
			this.MAWBLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.MAWBLabel.Visible = false;
			// 
			// MAWBPrefixTextBox
			// 
			this.BindingSource.SetBindingMember(this.MAWBPrefixTextBox, "ScheduleChooser+MasterBillAirlinePrefix");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).ScheduleChooser.MasterBillAirlinePrefix)));
			this.MAWBPrefixTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(372, 126, true);
			this.MAWBPrefixTextBox.Name = "MAWBPrefixTextBox";
			this.MAWBPrefixTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 20, true);
			this.MAWBPrefixTextBox.TabIndex = 20;
			this.MAWBPrefixTextBox.Visible = false;
			// 
			// MAWBHyphenLabel
			// 
			this.MAWBHyphenLabel.AutoSize = true;
			this.MAWBHyphenLabel.IsFontBold = true;
			this.MAWBHyphenLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(398, 129, true);
			this.MAWBHyphenLabel.Name = "MAWBHyphenLabel";
			this.MAWBHyphenLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(12, 13, true);
			this.MAWBHyphenLabel.TabIndex = 21;
			this.MAWBHyphenLabel.Text = "-";
			this.MAWBHyphenLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.MAWBHyphenLabel.Visible = false;
			// 
			// JS_AWBServiceLevelDropEdit
			// 
			this.BindingSource.SetBindingMember(this.JS_AWBServiceLevelDropEdit, "ScheduleChooser+AWBServiceLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).ScheduleChooser.AWBServiceLevel)));
			this.JS_AWBServiceLevelDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(183, 150, true);
			this.JS_AWBServiceLevelDropEdit.Name = "JS_AWBServiceLevelDropEdit";
			this.JS_AWBServiceLevelDropEdit.PreBoundMaxLength = 3;
			this.JS_AWBServiceLevelDropEdit.ShowDescriptionBox = false;
			this.JS_AWBServiceLevelDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.JS_AWBServiceLevelDropEdit.TabIndex = 17;
			this.JS_AWBServiceLevelDropEdit.Visible = false;
			// 
			// SailingSummarygroupBox
			// 
			this.SailingSummarygroupBox.CaptionResourceString = Res.GetData("ScheduleChooserControl|eb19e0b6-96f3-4039-8cc4-feb23e349508", "Sailing Summary");
			this.SailingSummarygroupBox.Controls.Add(this.MAWBSeaNumberTextBox);
			this.SailingSummarygroupBox.Controls.Add(this.HBLNumberTextBox);
			this.SailingSummarygroupBox.Controls.Add(this.JS_AWBServiceLevelLabel);
			this.SailingSummarygroupBox.Controls.Add(this.JS_AWBServiceLevelDropEdit);
			this.SailingSummarygroupBox.Controls.Add(this.MAWBHyphenLabel);
			this.SailingSummarygroupBox.Controls.Add(this.MAWBPrefixTextBox);
			this.SailingSummarygroupBox.Controls.Add(this.MAWBLabel);
			this.SailingSummarygroupBox.Controls.Add(this.MAWBNumberTextBox);
			this.SailingSummarygroupBox.Controls.Add(this.MAWBPendingAllocationTextBox);
			this.SailingSummarygroupBox.Controls.Add(this.JS_IsNeutral);
			this.SailingSummarygroupBox.Controls.Add(this.JS_IsDirect);
			this.SailingSummarygroupBox.Controls.Add(this.ButtonSelectFromConsortium);
			this.SailingSummarygroupBox.Controls.Add(this.SailingTotalVolumeCalcDropEdit);
			this.SailingSummarygroupBox.Controls.Add(this.DEPBoundReadOnlyDateEdit);
			this.SailingSummarygroupBox.Controls.Add(this.JS_Calc_FCLCutOffBoundReadOnlyDateEdit);
			this.SailingSummarygroupBox.Controls.Add(this.ARVBoundReadOnlyDateEdit);
			this.SailingSummarygroupBox.Controls.Add(this.JS_Calc_DepotCutOffBoundReadOnlyDateEdit);
			this.SailingSummarygroupBox.Controls.Add(this.VesselBoundTextBox);
			this.SailingSummarygroupBox.Controls.Add(this.VoyageNumberBoundTextBox);
			this.SailingSummarygroupBox.Controls.Add(this.CarrierLabel);
			this.SailingSummarygroupBox.Controls.Add(this.CreditorLabel);
			this.SailingSummarygroupBox.Controls.Add(this.BookedShippingLineBoundOrgFindBox);
			this.SailingSummarygroupBox.Controls.Add(this.CreditorFindBox);
			this.SailingSummarygroupBox.Controls.Add(this.SailingTotalWeightCalcDropEdit);
			this.SailingSummarygroupBox.Controls.Add(this.ClearSailingButton);
			this.SailingSummarygroupBox.Controls.Add(this.AddNewSailingButton);
			this.SailingSummarygroupBox.Controls.Add(this.ViewSailingButton);
			this.SailingSummarygroupBox.Controls.Add(this.ViewLoadListButton);
			this.SailingSummarygroupBox.Controls.Add(this.ImportGlobalSchedulesButton);
			this.SailingSummarygroupBox.Controls.Add(this.BookingRefTextBox);
			this.SailingSummarygroupBox.Controls.Add(this.SailingLoadingPortBoundTextBox);
			this.SailingSummarygroupBox.Controls.Add(this.SailingDischargePortBoundTextBox);
			this.SailingSummarygroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 48, true);
			this.SailingSummarygroupBox.Name = "SailingSummarygroupBox";
			this.SailingSummarygroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(640, 178, true);
			this.SailingSummarygroupBox.TabIndex = 6;
			this.SailingSummarygroupBox.TabStop = false;
			//
			// SailingLoadingPortBoundTextBox
			//
			this.BindingSource.SetBindingMember(this.SailingLoadingPortBoundTextBox, "ScheduleChooser+Sailing+JX_JA_RL_NKPortOfLoading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).ScheduleChooser.Sailing.JX_JA_RL_NKPortOfLoading)));
			this.SailingLoadingPortBoundTextBox.CaptionResourceString = Res.GetData("ScheduleChooserControl|212af610-9318-fbb0-49ea-527dd7d707c8", "Load Port");
			this.SailingLoadingPortBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 16, true);
			this.SailingLoadingPortBoundTextBox.Name = "SailingLoadingPortBoundTextBox";
			this.SailingLoadingPortBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.SailingLoadingPortBoundTextBox.Enabled = false;
			this.SailingLoadingPortBoundTextBox.TabIndex = 1;
			//
			// SailingDischargePortBoundTextBox
			//
			this.BindingSource.SetBindingMember(this.SailingDischargePortBoundTextBox, "ScheduleChooser+Sailing+JX_JB_RL_NKPortOfDischarge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).ScheduleChooser.Sailing.JX_JB_RL_NKPortOfDischarge)));
			this.SailingDischargePortBoundTextBox.CaptionResourceString = Res.GetData("ScheduleChooserControl|37521cdd-91e8-7ea0-4842-3a38531d70e1", "Discharge Port");
			this.SailingDischargePortBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(372, 16, true);
			this.SailingDischargePortBoundTextBox.Name = "SailingDischargePortBoundTextBox";
			this.SailingDischargePortBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.SailingDischargePortBoundTextBox.Enabled = false;
			this.SailingDischargePortBoundTextBox.TabIndex = 2;
			// 
			// MAWBSeaNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.MAWBSeaNumberTextBox, "Booking.JS_HouseBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.JS_HouseBill)));
			this.MAWBSeaNumberTextBox.CaptionResourceString = Res.GetData("ScheduleChooserControl|4f58eb1f-45e1-4a6a-9d26-86838b7c01c7", "Master Bill Number");
			this.MAWBSeaNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(372, 126, true);
			this.MAWBSeaNumberTextBox.Name = "MAWBSeaNumberTextBox";
			this.MAWBSeaNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(263, 20, true);
			this.MAWBSeaNumberTextBox.TabIndex = 23;
			// 
			// HBLNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.HBLNumberTextBox, "Booking.JS_HouseBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.JS_HouseBill)));
			this.HBLNumberTextBox.CaptionResourceString = Res.GetData("ScheduleChooserControl|732c3a0c-0aae-4730-9737-29f0e6440601", "House Bill Number");
			this.HBLNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(372, 126, true);
			this.HBLNumberTextBox.Name = "HBLNumberTextBox";
			this.HBLNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(263, 20, true);
			this.HBLNumberTextBox.TabIndex = 23;
			// 
			// JS_AWBServiceLevelLabel
			// 
			this.JS_AWBServiceLevelLabel.AutoSize = true;
			this.JS_AWBServiceLevelLabel.CaptionResourceString = Res.GetData("ScheduleChooserControl|8a3187bd-94d6-422b-8ed4-1a6d040e5aa1", "Carrier Service Level:");
			this.JS_AWBServiceLevelLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 147, true);
			this.JS_AWBServiceLevelLabel.Name = "JS_AWBServiceLevelLabel";
			this.JS_AWBServiceLevelLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 13, true);
			this.JS_AWBServiceLevelLabel.TabIndex = 16;
			this.JS_AWBServiceLevelLabel.Visible = false;
			// 
			// CarrierLabel
			// 
			this.CarrierLabel.CaptionResourceString = Res.GetData("ScheduleChooserControl|22da5055-14b5-4f5c-a3c5-9b2a0884eca0", "Carrier");
			this.CarrierLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.CarrierLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 104, true);
			this.CarrierLabel.Name = "CarrierLabel";
			this.CarrierLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.CarrierLabel.TabIndex = 14;
			this.CarrierLabel.Visible = false;
			// 
			// CreditorLabel
			// 
			this.CreditorLabel.CaptionResourceString = Res.GetData("ScheduleChooserControl|22da5055-14b5-4f5c-a3c5-9b2a0884eca0", "Creditor");
			this.CreditorLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.CreditorLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 124, true);
			this.CreditorLabel.Name = "CreditorLabel";
			this.CreditorLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.CreditorLabel.TabIndex = 14;
			this.CreditorLabel.Visible = false;
			// 
			// MAWBPendingAllocationTextBox
			// 
			this.BindingSource.SetBindingMember(this.MAWBPendingAllocationTextBox, "ScheduleChooser.MasterBillNeutralMAWB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).ScheduleChooser.MasterBillNeutralMAWB)));
			this.MAWBPendingAllocationTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.MAWBPendingAllocationTextBox.Enabled = true;
			this.MAWBPendingAllocationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(411, 126, true);
			this.MAWBPendingAllocationTextBox.Name = "MAWBPendingAllocationTextBox";
			this.MAWBPendingAllocationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.MAWBPendingAllocationTextBox.TabIndex = 26;
			this.MAWBPendingAllocationTextBox.TabStop = false;
			// 
			// ScheduleChooserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SailingSummarygroupBox);
			this.Controls.Add(this.ETDZDateEdit);
			this.Controls.Add(this.JS_RL_NKDestinationBoundCodeFindBox);
			this.Controls.Add(this.JS_RL_NKOriginBoundCodeFindBox);
			this.Controls.Add(this.LoadingPortCodeFindBox);
			this.Controls.Add(this.DischargePortCodeFindBox);
			this.Controls.Add(this.ETAZDateEdit);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(642, 177, true);
			this.Name = "ScheduleChooserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(642, 177, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ETDZDateEdit.ResumeLayout(true);
			this.ETDZDateEdit.PerformLayout();
			this.JS_RL_NKDestinationBoundCodeFindBox.ResumeLayout(true);
			this.JS_RL_NKDestinationBoundCodeFindBox.PerformLayout();
			this.JS_RL_NKOriginBoundCodeFindBox.ResumeLayout(true);
			this.JS_RL_NKOriginBoundCodeFindBox.PerformLayout();
			this.LoadingPortCodeFindBox.ResumeLayout(true);
			this.LoadingPortCodeFindBox.PerformLayout();
			this.DischargePortCodeFindBox.ResumeLayout(true);
			this.DischargePortCodeFindBox.PerformLayout();
			this.ETAZDateEdit.ResumeLayout(true);
			this.ETAZDateEdit.PerformLayout();
			this.SailingTotalWeightCalcDropEdit.ResumeLayout(true);
			this.SailingTotalWeightCalcDropEdit.PerformLayout();
			this.BookedShippingLineBoundOrgFindBox.ResumeLayout(true);
			this.BookedShippingLineBoundOrgFindBox.PerformLayout();
			this.CreditorFindBox.ResumeLayout(true);
			this.CreditorFindBox.PerformLayout();
			this.JS_Calc_DepotCutOffBoundReadOnlyDateEdit.ResumeLayout(true);
			this.JS_Calc_DepotCutOffBoundReadOnlyDateEdit.PerformLayout();
			this.ARVBoundReadOnlyDateEdit.ResumeLayout(true);
			this.ARVBoundReadOnlyDateEdit.PerformLayout();
			this.JS_Calc_FCLCutOffBoundReadOnlyDateEdit.ResumeLayout(true);
			this.JS_Calc_FCLCutOffBoundReadOnlyDateEdit.PerformLayout();
			this.DEPBoundReadOnlyDateEdit.ResumeLayout(true);
			this.DEPBoundReadOnlyDateEdit.PerformLayout();
			this.SailingTotalVolumeCalcDropEdit.ResumeLayout(true);
			this.SailingTotalVolumeCalcDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ButtonSelectFromConsortium)).EndInit();
			this.JS_AWBServiceLevelDropEdit.ResumeLayout(true);
			this.JS_AWBServiceLevelDropEdit.PerformLayout();
			this.SailingSummarygroupBox.ResumeLayout(false);
			this.SailingSummarygroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private Enterprise.ZArchitecture.GUI.ZDateEdit ETDZDateEdit;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox JS_RL_NKDestinationBoundCodeFindBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox JS_RL_NKOriginBoundCodeFindBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox LoadingPortCodeFindBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox DischargePortCodeFindBox;
		private Enterprise.ZArchitecture.ZTextBox SailingLoadingPortBoundTextBox;
		private Enterprise.ZArchitecture.ZTextBox SailingDischargePortBoundTextBox;
		private Enterprise.ZArchitecture.GUI.ZDateEdit ETAZDateEdit;
		private Enterprise.ZArchitecture.ZTextBox BookingRefTextBox;
		private Enterprise.ZArchitecture.GUI.ZButton ViewLoadListButton;
		private Enterprise.ZArchitecture.GUI.ZButton ImportGlobalSchedulesButton;
		private Enterprise.ZArchitecture.GUI.ZButton ViewSailingButton;
		private Enterprise.ZArchitecture.GUI.ZButton AddNewSailingButton;
		private Enterprise.ZArchitecture.GUI.ZButton ClearSailingButton;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit SailingTotalWeightCalcDropEdit;
		private Enterprise.MasterFiles.GUI.ZOrganisationFindBox BookedShippingLineBoundOrgFindBox;
		private Enterprise.MasterFiles.GUI.ZOrganisationFindBox CreditorFindBox;
		private Enterprise.ZArchitecture.ZTextBox VoyageNumberBoundTextBox;
		private Enterprise.ZArchitecture.ZTextBox VesselBoundTextBox;
		private Enterprise.ZArchitecture.GUI.ZDateEdit JS_Calc_DepotCutOffBoundReadOnlyDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit ARVBoundReadOnlyDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit JS_Calc_FCLCutOffBoundReadOnlyDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit DEPBoundReadOnlyDateEdit;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit SailingTotalVolumeCalcDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropButtonOnly ButtonSelectFromConsortium;
		private Enterprise.ZArchitecture.GUI.ZCheckBox JS_IsDirect;
		private Enterprise.ZArchitecture.GUI.ZCheckBox JS_IsNeutral;
		private Enterprise.ZArchitecture.ZTextBox MAWBNumberTextBox;
		private Enterprise.ZArchitecture.ZLabel MAWBLabel;
		private Enterprise.ZArchitecture.ZTextBox MAWBPrefixTextBox;
		private Enterprise.ZArchitecture.ZLabel MAWBHyphenLabel;
		private Enterprise.ZArchitecture.GUI.ZDropEdit JS_AWBServiceLevelDropEdit;
		private Enterprise.ZArchitecture.GUI.ZGroupBox SailingSummarygroupBox;
		private Enterprise.ZArchitecture.ZLabel JS_AWBServiceLevelLabel;
		private Enterprise.ZArchitecture.ZLabel CarrierLabel;
		private Enterprise.ZArchitecture.ZLabel CreditorLabel;
		private Enterprise.ZArchitecture.ZTextBox MAWBSeaNumberTextBox;
		private Enterprise.ZArchitecture.ZTextBox HBLNumberTextBox;
		private Enterprise.ZArchitecture.ZTextBox MAWBPendingAllocationTextBox;
	}
}
