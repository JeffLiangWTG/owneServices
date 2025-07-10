namespace Enterprise.MasterFiles.GUI
{
	public sealed partial class RefAirlineForm
	{
		#region Windows Form Designer generated code

		new void InitializeComponent()
		{
			this.OtherDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.EFreightStatusTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CargoImpTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SpecialHandlingCodesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DefaultCommodityTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AirlineSpecificCodesTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.AirlineSpecificCodesTabControl.Controls.Add(DefaultCommodityTabPage);
			this.AirlineSpecificCodesTabControl.Controls.Add(SpecialHandlingCodesTabPage);
			this.AirlineSpecificCodesMainTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			AirlineSpecificCodesMainTabPage.Controls.Add(AirlineSpecificCodesTabControl);
			this.MainTabControl.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			//
			this.MainTabControl.Controls.Add(this.EFreightStatusTabPage);
			this.MainTabControl.Controls.Add(this.OtherDetailsTabPage);
			this.MainTabControl.Controls.Add(this.CargoImpTabPage);
			this.MainTabControl.Controls.Add(this.AirlineSpecificCodesMainTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(750, 363, true);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.CargoImpTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.OtherDetailsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.EFreightStatusTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.AirlineSpecificCodesMainTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(745, 351, true);
			this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPage_InitializeTab));
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(745, 341, true);
			this.NotesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.NotesTabPage_InitializeTab));
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(745, 341, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(750, 363, true);
			// 
			// PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel
			// 
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.TabIndex = 0;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(750, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 0;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(234);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(234);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefAirline);
			// 
			// OtherDetailsTabPage
			// 
			this.OtherDetailsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefAirlineForm|7e22f0ea-e3da-42ad-bc33-d7800a4dadf7", "Other Information");
			this.OtherDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.OtherDetailsTabPage.Name = "OtherDetailsTabPage";
			this.OtherDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.OtherDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(745, 341, true);
			this.OtherDetailsTabPage.TabIndex = 3;
			this.OtherDetailsTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.OtherDetailsTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefAirline)(null)).RM_ReservationsContactName)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefAirline)(null)).RM_ReservationsContactTeletype)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefAirline)(null)).RM_ReservationsContactTitle)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefAirline)(null)).RM_ReservationsDeptTeletype)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefAirline)(null)).RM_EmergencyTeletype)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefAirline)(null)).RM_EmergencyContactName)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefAirline)(null)).RM_EmergencyContactTitle)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefAirline)(null)).RM_TypeOfOperationsCode)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefAirline)(null)).RM_AccountingSecondaryFlag)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefAirline)(null)).RM_AccountingCode)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefAirline)(null)).RM_AirlinePrefixSecondaryFlag)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefAirline)(null)).RM_DuplicateFlagIndicator)));
			// 
			// EFreightStatusTabPage
			// 
			this.EFreightStatusTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefAirlineForm|41026e35-ebd4-4156-88cc-aa69e9e91d59", "e-freight Status");
			this.EFreightStatusTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.EFreightStatusTabPage.Name = "EFreightStatusTabPage";
			this.EFreightStatusTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.EFreightStatusTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(745, 341, true);
			this.EFreightStatusTabPage.TabIndex = 2;
			this.EFreightStatusTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.EFreightStatusTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefAirline)(null)).EFreightStatusCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefAirlineEFreightRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefAirline)(null)).EFreightStatusCollection)).SyncRoot)).RME_OriginLocation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefAirlineEFreightRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefAirline)(null)).EFreightStatusCollection)).SyncRoot)).RME_DestinationLocation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefAirlineEFreightRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefAirline)(null)).EFreightStatusCollection)).SyncRoot)).RME_EFreightStatus)));
			// 
			// CargoImpTabPage
			// 
			this.CargoImpTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefAirlineForm|cf4bf80e-ab8e-44cb-8bf9-05942cd4ff6c", "FWB/FHL Settings");
			this.CargoImpTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.CargoImpTabPage.Name = "CargoImpTabPage";
			this.CargoImpTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.CargoImpTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(745, 341, true);
			this.CargoImpTabPage.TabIndex = 4;
			this.CargoImpTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.CargoImpTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefAirline)(null)).RM_ContactPhoneOCIIdentifier)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefAirline)(null)).RM_ContactNameOCIIdentifier)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefAirline)(null)).RM_LabelShortName)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefAirline)(null)).RM_AirlineName2)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefAirline)(null)).RM_AirlineName1)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefAirline)(null)).RM_AirlineCountry)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefAirline)(null)).RM_AirlineCity)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefAirline)(null)).RM_AirlinePostalCode)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefAirline)(null)).RM_AirlineState)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefAirline)(null)).RM_AddressLine2)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefAirline)(null)).RM_AddressLine1)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefAirline)(null)).RM_TwoCharacterCode)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefAirline)(null)).RM_ThreeLetterCode)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefAirline)(null)).RM_EagleAddedAirlinePrefixOrAccountingCode)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefAirline)(null)).RM_IsCASSControlled)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefAirline)(null)).RM_MembershipFlagSITA)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefAirline)(null)).RM_MembershipFlagARINC)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefAirline)(null)).RM_MembershipFlagIATA)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefAirline)(null)).RM_MembershipFlagATA)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.MasterFiles.Business.RefAirline)(null)).HasSignedEAWBAgreement)));
			//
			// DefaultCommodityTabPage
			//
			this.DefaultCommodityTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefAirlineForm|080A2F9D-31A3-4A6F-97B7-0526732F262D", "Default Commodity Code");
			this.DefaultCommodityTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.DefaultCommodityTabPage.AutoSize = true;
			this.DefaultCommodityTabPage.Name = "DefaultCommodityTabPage";
			this.DefaultCommodityTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DefaultCommodityTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(745,341, true);
			this.DefaultCommodityTabPage.TabIndex = 5;
			this.DefaultCommodityTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.DefaultCommodityTabPage_InitializeTab));
			//
			// SpecialHandlingCodesTabPage
			//
			this.SpecialHandlingCodesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefAirlineForm|6F9A7FBD-7C44-43BB-82B0-6EE07EF36277", "Special Handling Codes");
			this.SpecialHandlingCodesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.SpecialHandlingCodesTabPage.AutoSize = true;
			this.SpecialHandlingCodesTabPage.Name = "SpecialHandlingCodesTabPage";
			this.SpecialHandlingCodesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.SpecialHandlingCodesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(745, 341, true);
			this.SpecialHandlingCodesTabPage.TabIndex = 5;
			this.SpecialHandlingCodesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.SpecialHandlingCodesTabPage_InitializeTab));
			//
			//AirlineSpecificCodesMainTabPage
			//
			this.AirlineSpecificCodesMainTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefAirlineForm|689A2D0C-7F8A-4421-ADBE-F17394845C63", "Airline Specific Codes");
			this.AirlineSpecificCodesMainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.AirlineSpecificCodesMainTabPage.Name = "AirlineSpecificCodesMainTabPage";
			this.AirlineSpecificCodesMainTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AirlineSpecificCodesMainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(745, 341, true);
			this.AirlineSpecificCodesMainTabPage.TabIndex = 5;
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check((CargoWise.Types.ZString)((Enterprise.MasterFiles.Business.RefAirlineDefaultCommodityCode)(null)).RDC_RAR_NKProductCode);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check((CargoWise.Types.ZString)((Enterprise.MasterFiles.Business.RefAirlineDefaultCommodityCode)(null)).RDC_RAC_NKCommodityCode);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check((CargoWise.Types.ZString)((Enterprise.MasterFiles.Business.RefAirlineDefaultCommodityCode)(null)).RDC_Description);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check((CargoWise.Types.ZString)((Enterprise.MasterFiles.Business.RefAirlineDefaultCommodityCode)(null)).RDC_RL_NKOrigin);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check((CargoWise.Types.ZString)((Enterprise.MasterFiles.Business.RefAirlineDefaultCommodityCode)(null)).RDC_RL_NKDestination);
			// 
			// RefAirlineForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefAirlineForm|eb6a0050-eb0c-438e-a9c2-d8b2d8dfd87b", "Airline");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(750, 419, true);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefAirline);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(568, 456, true);
			this.Name = "RefAirlineForm";
			this.ShouldSerializeTabPageMethods = true;
			this.Text = "";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private void MainTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RM_LabelShortNameText = new Enterprise.ZArchitecture.ZTextBox();
			this.RM_AirlineName2BoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RM_AirlineName1BoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RM_AirlineCountryBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RM_AirlineCityBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RM_AirlinePostalCodeBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RM_AirlineStateBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RM_AddressLine2BoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RM_AddressLine1BoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CodesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RM_TwoCharacterCodeBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RM_ThreeLetterCodeBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RM_EagleAddedAirlinePrefixOrAccountingCodeBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CASSControlledGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CASSControlledCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zCheckBox1 = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zCheckBox2 = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zCheckBox3 = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zCheckBox4 = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RM_IsActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RM_IsUpdatableCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.hasSignedEAWBAgreementCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.MainTabPage.SuspendLayout();
			this.DetailsGroupBox.SuspendLayout();
			this.CodesGroupBox.SuspendLayout();
			this.CASSControlledGroupBox.SuspendLayout();
			this.MainTabPage.Controls.Add(this.CASSControlledGroupBox);
			this.MainTabPage.Controls.Add(this.CodesGroupBox);
			this.MainTabPage.Controls.Add(this.DetailsGroupBox);
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefAirlineForm|458564de-dc83-44a6-a81d-5499744ca480", "Details");
			this.DetailsGroupBox.Controls.Add(this.hasSignedEAWBAgreementCheckBox);
			this.DetailsGroupBox.Controls.Add(this.RM_LabelShortNameText);
			this.DetailsGroupBox.Controls.Add(this.RM_AirlineName2BoundTextBox);
			this.DetailsGroupBox.Controls.Add(this.RM_AirlineName1BoundTextBox);
			this.DetailsGroupBox.Controls.Add(this.RM_AirlineCountryBoundTextBox);
			this.DetailsGroupBox.Controls.Add(this.RM_AirlineCityBoundTextBox);
			this.DetailsGroupBox.Controls.Add(this.RM_AirlinePostalCodeBoundTextBox);
			this.DetailsGroupBox.Controls.Add(this.RM_AirlineStateBoundTextBox);
			this.DetailsGroupBox.Controls.Add(this.RM_AddressLine2BoundTextBox);
			this.DetailsGroupBox.Controls.Add(this.RM_AddressLine1BoundTextBox);
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 104, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(715, 227, true);
			this.DetailsGroupBox.TabIndex = 2;
			this.DetailsGroupBox.TabStop = false;
			// 
			// RM_LabelShortNameText
			// 
			this.BindingSource.SetBindingMember(this.RM_LabelShortNameText, "RM_LabelShortName");
			this.RM_LabelShortNameText.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefAirlineForm|0315fa80-ed42-49fc-a997-11d9775ca0f6", "Short Airline");
			this.RM_LabelShortNameText.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RM_LabelShortNameText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 15, true);
			this.RM_LabelShortNameText.Name = "RM_LabelShortNameText";
			this.RM_LabelShortNameText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 17, true);
			this.RM_LabelShortNameText.TabIndex = 0;
			// 
			// RM_AirlineName2BoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.RM_AirlineName2BoundTextBox, "RM_AirlineName2");
			this.RM_AirlineName2BoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RM_AirlineName2BoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 59, true);
			this.RM_AirlineName2BoundTextBox.Name = "RM_AirlineName2BoundTextBox";
			this.RM_AirlineName2BoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 17, true);
			this.RM_AirlineName2BoundTextBox.TabIndex = 2;
			// 
			// RM_AirlineName1BoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.RM_AirlineName1BoundTextBox, "RM_AirlineName1");
			this.RM_AirlineName1BoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RM_AirlineName1BoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 37, true);
			this.RM_AirlineName1BoundTextBox.Name = "RM_AirlineName1BoundTextBox";
			this.RM_AirlineName1BoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 17, true);
			this.RM_AirlineName1BoundTextBox.TabIndex = 1;
			// 
			// RM_AirlineCountryBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.RM_AirlineCountryBoundTextBox, "RM_AirlineCountry");
			this.RM_AirlineCountryBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RM_AirlineCountryBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 171, true);
			this.RM_AirlineCountryBoundTextBox.Name = "RM_AirlineCountryBoundTextBox";
			this.RM_AirlineCountryBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 17, true);
			this.RM_AirlineCountryBoundTextBox.TabIndex = 8;
			// 
			// RM_AirlineCityBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.RM_AirlineCityBoundTextBox, "RM_AirlineCity");
			this.RM_AirlineCityBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RM_AirlineCityBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 126, true);
			this.RM_AirlineCityBoundTextBox.Name = "RM_AirlineCityBoundTextBox";
			this.RM_AirlineCityBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 17, true);
			this.RM_AirlineCityBoundTextBox.TabIndex = 5;
			// 
			// RM_AirlinePostalCodeBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.RM_AirlinePostalCodeBoundTextBox, "RM_AirlinePostalCode");
			this.RM_AirlinePostalCodeBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RM_AirlinePostalCodeBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(350, 149, true);
			this.RM_AirlinePostalCodeBoundTextBox.Name = "RM_AirlinePostalCodeBoundTextBox";
			this.RM_AirlinePostalCodeBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 17, true);
			this.RM_AirlinePostalCodeBoundTextBox.TabIndex = 7;
			// 
			// RM_AirlineStateBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.RM_AirlineStateBoundTextBox, "RM_AirlineState");
			this.RM_AirlineStateBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RM_AirlineStateBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 149, true);
			this.RM_AirlineStateBoundTextBox.Name = "RM_AirlineStateBoundTextBox";
			this.RM_AirlineStateBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 17, true);
			this.RM_AirlineStateBoundTextBox.TabIndex = 6;
			// 
			// RM_AddressLine2BoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.RM_AddressLine2BoundTextBox, "RM_AddressLine2");
			this.RM_AddressLine2BoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RM_AddressLine2BoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 104, true);
			this.RM_AddressLine2BoundTextBox.Name = "RM_AddressLine2BoundTextBox";
			this.RM_AddressLine2BoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 17, true);
			this.RM_AddressLine2BoundTextBox.TabIndex = 4;
			// 
			// RM_AddressLine1BoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.RM_AddressLine1BoundTextBox, "RM_AddressLine1");
			this.RM_AddressLine1BoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RM_AddressLine1BoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 82, true);
			this.RM_AddressLine1BoundTextBox.Name = "RM_AddressLine1BoundTextBox";
			this.RM_AddressLine1BoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 17, true);
			this.RM_AddressLine1BoundTextBox.TabIndex = 3;
			// 
			// CodesGroupBox
			// 
			this.CodesGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefAirlineForm|863712a5-7dc7-490e-9535-8fbd023e91e2", "Codes");
			this.CodesGroupBox.Controls.Add(this.RM_TwoCharacterCodeBoundTextBox);
			this.CodesGroupBox.Controls.Add(this.RM_ThreeLetterCodeBoundTextBox);
			this.CodesGroupBox.Controls.Add(this.RM_EagleAddedAirlinePrefixOrAccountingCodeBoundTextBox);
			this.CodesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.CodesGroupBox.Name = "CodesGroupBox";
			this.CodesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 89, true);
			this.CodesGroupBox.TabIndex = 0;
			this.CodesGroupBox.TabStop = false;
			// 
			// RM_TwoCharacterCodeBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.RM_TwoCharacterCodeBoundTextBox, "RM_TwoCharacterCode");
			this.RM_TwoCharacterCodeBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 61, true);
			this.RM_TwoCharacterCodeBoundTextBox.Name = "RM_TwoCharacterCodeBoundTextBox";
			this.RM_TwoCharacterCodeBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 17, true);
			this.RM_TwoCharacterCodeBoundTextBox.TabIndex = 2;
			// 
			// RM_ThreeLetterCodeBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.RM_ThreeLetterCodeBoundTextBox, "RM_ThreeLetterCode");
			this.RM_ThreeLetterCodeBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 39, true);
			this.RM_ThreeLetterCodeBoundTextBox.Name = "RM_ThreeLetterCodeBoundTextBox";
			this.RM_ThreeLetterCodeBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 17, true);
			this.RM_ThreeLetterCodeBoundTextBox.TabIndex = 1;
			// 
			// RM_EagleAddedAirlinePrefixOrAccountingCodeBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.RM_EagleAddedAirlinePrefixOrAccountingCodeBoundTextBox, "RM_EagleAddedAirlinePrefixOrAccountingCode");
			this.RM_EagleAddedAirlinePrefixOrAccountingCodeBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 17, true);
			this.RM_EagleAddedAirlinePrefixOrAccountingCodeBoundTextBox.Name = "RM_EagleAddedAirlinePrefixOrAccountingCodeBoundTextBox";
			this.RM_EagleAddedAirlinePrefixOrAccountingCodeBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 17, true);
			this.RM_EagleAddedAirlinePrefixOrAccountingCodeBoundTextBox.TabIndex = 0;
			// 
			// CASSControlledGroupBox
			// 
			this.CASSControlledGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefAirlineForm|346e24a4-d9a1-496b-97f9-4e6060901c02", "Membership Details");
			this.CASSControlledGroupBox.Controls.Add(this.zCheckBox4);
			this.CASSControlledGroupBox.Controls.Add(this.zCheckBox3);
			this.CASSControlledGroupBox.Controls.Add(this.zCheckBox2);
			this.CASSControlledGroupBox.Controls.Add(this.zCheckBox1);
			this.CASSControlledGroupBox.Controls.Add(this.RM_IsActiveCheckBox);
			this.CASSControlledGroupBox.Controls.Add(this.RM_IsUpdatableCheckBox);
			this.CASSControlledGroupBox.Controls.Add(this.CASSControlledCheckBox);
			this.CASSControlledGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(254, 7, true);
			this.CASSControlledGroupBox.Name = "CASSControlledGroupBox";
			this.CASSControlledGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(469, 89, true);
			this.CASSControlledGroupBox.TabIndex = 1;
			this.CASSControlledGroupBox.TabStop = false;
			// 
			// CASSControlledCheckBox
			// 
			this.CASSControlledCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.CASSControlledCheckBox, "RM_IsCASSControlled");
			this.CASSControlledCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CASSControlledCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(23, 18, true);
			this.CASSControlledCheckBox.Name = "CASSControlledCheckBox";
			this.CASSControlledCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.CASSControlledCheckBox.TabIndex = 0;
			// 
			// zCheckBox1
			// 
			this.zCheckBox1.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zCheckBox1, "RM_MembershipFlagSITA");
			this.zCheckBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zCheckBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(216, 18, true);
			this.zCheckBox1.Name = "zCheckBox1";
			this.zCheckBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.zCheckBox1.TabIndex = 3;
			// 
			// zCheckBox2
			// 
			this.zCheckBox2.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zCheckBox2, "RM_MembershipFlagARINC");
			this.zCheckBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zCheckBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(216, 42, true);
			this.zCheckBox2.Name = "zCheckBox2";
			this.zCheckBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.zCheckBox2.TabIndex = 4;
			// 
			// zCheckBox3
			// 
			this.zCheckBox3.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zCheckBox3, "RM_MembershipFlagIATA");
			this.zCheckBox3.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zCheckBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(23, 42, true);
			this.zCheckBox3.Name = "zCheckBox3";
			this.zCheckBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.zCheckBox3.TabIndex = 1;
			// 
			// zCheckBox4
			// 
			this.zCheckBox4.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zCheckBox4, "RM_MembershipFlagATA");
			this.zCheckBox4.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zCheckBox4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(23, 66, true);
			this.zCheckBox4.Name = "zCheckBox4";
			this.zCheckBox4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.zCheckBox4.TabIndex = 2;
			// 
			// RM_IsActiveCheckBox
			// 
			this.RM_IsActiveCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.RM_IsActiveCheckBox, "RM_IsActive");
			this.RM_IsActiveCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RM_IsActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 66, true);
			this.RM_IsActiveCheckBox.Name = "RM_IsActiveCheckBox";
			this.RM_IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.RM_IsActiveCheckBox.TabIndex = 5;
			// 
			// RM_IsUpdatableCheckBox
			// 
			this.RM_IsUpdatableCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.RM_IsUpdatableCheckBox, "RM_IsUpdatable");
			this.RM_IsUpdatableCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RM_IsUpdatableCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(250, 66, true);
			this.RM_IsUpdatableCheckBox.Name = "RM_IsActiveCheckBox";
			this.RM_IsUpdatableCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.RM_IsUpdatableCheckBox.TabIndex = 6;
			// 
			// hasSignedEAWBAgreementCheckBox
			// 
			this.hasSignedEAWBAgreementCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.hasSignedEAWBAgreementCheckBox, "HasSignedEAWBAgreement");
			this.hasSignedEAWBAgreementCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefAirlineForm|25E1F139-9D31-4630-9E17-8407325848B7", "Has Signed e-AWB Agreement", "Has Signed e-AWB Agreement", "Has Signed e-AWB Agreement", "Has Signed an e-AWB Agreement with this company which means that FWB messages will be sent with the ‘EAW’ special handling code declared by default");
			this.hasSignedEAWBAgreementCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.hasSignedEAWBAgreementCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 197, true);
			this.hasSignedEAWBAgreementCheckBox.Name = "hasSignedEAWBAgreementCheckBox";
			this.hasSignedEAWBAgreementCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.hasSignedEAWBAgreementCheckBox.TabIndex = 9;
			this.MainTabPage.PerformLayout();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.CodesGroupBox.ResumeLayout(false);
			this.CodesGroupBox.PerformLayout();
			this.CASSControlledGroupBox.ResumeLayout(false);
			this.CASSControlledGroupBox.PerformLayout();
			this.MainTabPage.ResumeLayout(true);

		}

		private void OtherDetailsTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.ReservationDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zTextBox11 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox12 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox13 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox14 = new Enterprise.ZArchitecture.ZTextBox();
			this.EmergencyDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox2 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox3 = new Enterprise.ZArchitecture.ZTextBox();
			this.OtherDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zTextBox4 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox5 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox6 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox7 = new Enterprise.ZArchitecture.ZTextBox();
			this.zCheckBox5 = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OtherDetailsTabPage.SuspendLayout();
			this.ReservationDetailsGroupBox.SuspendLayout();
			this.EmergencyDetailsGroupBox.SuspendLayout();
			this.OtherDetailsGroupBox.SuspendLayout();
			this.OtherDetailsTabPage.Controls.Add(this.OtherDetailsGroupBox);
			this.OtherDetailsTabPage.Controls.Add(this.ReservationDetailsGroupBox);
			this.OtherDetailsTabPage.Controls.Add(this.EmergencyDetailsGroupBox);
			// 
			// ReservationDetailsGroupBox
			// 
			this.ReservationDetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefAirlineForm|1e5e2cd4-9523-4609-8070-abff24a2b5a4", "Reservations Contact");
			this.ReservationDetailsGroupBox.Controls.Add(this.zTextBox11);
			this.ReservationDetailsGroupBox.Controls.Add(this.zTextBox12);
			this.ReservationDetailsGroupBox.Controls.Add(this.zTextBox13);
			this.ReservationDetailsGroupBox.Controls.Add(this.zTextBox14);
			this.ReservationDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 107, true);
			this.ReservationDetailsGroupBox.Name = "ReservationDetailsGroupBox";
			this.ReservationDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(538, 118, true);
			this.ReservationDetailsGroupBox.TabIndex = 1;
			this.ReservationDetailsGroupBox.TabStop = false;
			// 
			// zTextBox11
			// 
			this.BindingSource.SetBindingMember(this.zTextBox11, "RM_ReservationsContactName");
			this.zTextBox11.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox11.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 41, true);
			this.zTextBox11.Name = "zTextBox11";
			this.zTextBox11.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(406, 17, true);
			this.zTextBox11.TabIndex = 1;
			// 
			// zTextBox12
			// 
			this.BindingSource.SetBindingMember(this.zTextBox12, "RM_ReservationsContactTeletype");
			this.zTextBox12.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox12.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 64, true);
			this.zTextBox12.Name = "zTextBox12";
			this.zTextBox12.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(406, 17, true);
			this.zTextBox12.TabIndex = 2;
			// 
			// zTextBox13
			// 
			this.BindingSource.SetBindingMember(this.zTextBox13, "RM_ReservationsContactTitle");
			this.zTextBox13.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox13.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 19, true);
			this.zTextBox13.Name = "zTextBox13";
			this.zTextBox13.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(406, 17, true);
			this.zTextBox13.TabIndex = 0;
			// 
			// zTextBox14
			// 
			this.BindingSource.SetBindingMember(this.zTextBox14, "RM_ReservationsDeptTeletype");
			this.zTextBox14.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox14.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 86, true);
			this.zTextBox14.Name = "zTextBox14";
			this.zTextBox14.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(406, 17, true);
			this.zTextBox14.TabIndex = 3;
			// 
			// EmergencyDetailsGroupBox
			// 
			this.EmergencyDetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefAirlineForm|1776055c-8024-4680-a9bf-fc264947fb62", "Emergency Contact");
			this.EmergencyDetailsGroupBox.Controls.Add(this.zTextBox1);
			this.EmergencyDetailsGroupBox.Controls.Add(this.zTextBox2);
			this.EmergencyDetailsGroupBox.Controls.Add(this.zTextBox3);
			this.EmergencyDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 6, true);
			this.EmergencyDetailsGroupBox.Name = "EmergencyDetailsGroupBox";
			this.EmergencyDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(538, 95, true);
			this.EmergencyDetailsGroupBox.TabIndex = 0;
			this.EmergencyDetailsGroupBox.TabStop = false;
			// 
			// zTextBox1
			// 
			this.BindingSource.SetBindingMember(this.zTextBox1, "RM_EmergencyTeletype");
			this.zTextBox1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 64, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(406, 17, true);
			this.zTextBox1.TabIndex = 2;
			// 
			// zTextBox2
			// 
			this.BindingSource.SetBindingMember(this.zTextBox2, "RM_EmergencyContactName");
			this.zTextBox2.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 41, true);
			this.zTextBox2.Name = "zTextBox2";
			this.zTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(406, 17, true);
			this.zTextBox2.TabIndex = 1;
			// 
			// zTextBox3
			// 
			this.BindingSource.SetBindingMember(this.zTextBox3, "RM_EmergencyContactTitle");
			this.zTextBox3.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 19, true);
			this.zTextBox3.Name = "zTextBox3";
			this.zTextBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(406, 17, true);
			this.zTextBox3.TabIndex = 0;
			// 
			// OtherDetailsGroupBox
			// 
			this.OtherDetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefAirlineForm|46278ad6-0d21-471e-b87c-6efa4d396049", "Additional Information");
			this.OtherDetailsGroupBox.Controls.Add(this.zCheckBox5);
			this.OtherDetailsGroupBox.Controls.Add(this.zTextBox4);
			this.OtherDetailsGroupBox.Controls.Add(this.zTextBox5);
			this.OtherDetailsGroupBox.Controls.Add(this.zTextBox6);
			this.OtherDetailsGroupBox.Controls.Add(this.zTextBox7);
			this.OtherDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 231, true);
			this.OtherDetailsGroupBox.Name = "OtherDetailsGroupBox";
			this.OtherDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(538, 85, true);
			this.OtherDetailsGroupBox.TabIndex = 2;
			this.OtherDetailsGroupBox.TabStop = false;
			// 
			// zTextBox4
			// 
			this.BindingSource.SetBindingMember(this.zTextBox4, "RM_TypeOfOperationsCode");
			this.zTextBox4.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 41, true);
			this.zTextBox4.Name = "zTextBox4";
			this.zTextBox4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 17, true);
			this.zTextBox4.TabIndex = 3;
			// 
			// zTextBox5
			// 
			this.BindingSource.SetBindingMember(this.zTextBox5, "RM_AccountingSecondaryFlag");
			this.zTextBox5.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(309, 19, true);
			this.zTextBox5.Name = "zTextBox5";
			this.zTextBox5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(34, 17, true);
			this.zTextBox5.TabIndex = 1;
			// 
			// zTextBox6
			// 
			this.BindingSource.SetBindingMember(this.zTextBox6, "RM_AccountingCode");
			this.zTextBox6.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 19, true);
			this.zTextBox6.Name = "zTextBox6";
			this.zTextBox6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 17, true);
			this.zTextBox6.TabIndex = 0;
			// 
			// zTextBox7
			// 
			this.BindingSource.SetBindingMember(this.zTextBox7, "RM_AirlinePrefixSecondaryFlag");
			this.zTextBox7.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(309, 44, true);
			this.zTextBox7.Name = "zTextBox7";
			this.zTextBox7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(34, 17, true);
			this.zTextBox7.TabIndex = 4;
			// 
			// zCheckBox5
			// 
			this.zCheckBox5.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zCheckBox5, "RM_DuplicateFlagIndicator");
			this.zCheckBox5.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zCheckBox5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(426, 21, true);
			this.zCheckBox5.Name = "zCheckBox5";
			this.zCheckBox5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 16, true);
			this.zCheckBox5.TabIndex = 2;
			this.OtherDetailsTabPage.PerformLayout();
			this.ReservationDetailsGroupBox.ResumeLayout(false);
			this.ReservationDetailsGroupBox.PerformLayout();
			this.EmergencyDetailsGroupBox.ResumeLayout(false);
			this.EmergencyDetailsGroupBox.PerformLayout();
			this.OtherDetailsGroupBox.ResumeLayout(false);
			this.OtherDetailsGroupBox.PerformLayout();
			this.OtherDetailsTabPage.ResumeLayout(true);

		}

		private void EFreightStatusTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.RulesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RulesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.EFreightStatusTabPage.SuspendLayout();
			this.RulesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RulesGrid)).BeginInit();
			this.RulesGrid.SuspendLayout();
			this.EFreightStatusTabPage.Controls.Add(this.RulesGroupBox);
			//
			// RulesGroupBox
			//
			this.RulesGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("3f913c19-623f-4d57-b70a-cfb1f0026200", "Rules");
			this.RulesGroupBox.Controls.Add(this.RulesGrid);
			this.RulesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RulesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.RulesGroupBox.Name = "RulesGroupBox";
			this.RulesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(739, 335, true);
			this.RulesGroupBox.TabIndex = 1;
			this.RulesGroupBox.TabStop = false;
			// 
			// RulesGrid
			// 
			this.RulesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.RulesGrid, "EFreightStatusCollection");
			this.RulesGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("68135ee2-bfb0-481f-a752-4dcea5e79ad1", "Origin");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "RME_OriginLocation";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCodeFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("0fd34e49-86af-47fa-87a4-9b7c6849d9ff", "Destination");
			zCodeFindBoxColumnStyleInfo2.ColumnName = "RME_DestinationLocation";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("b606a9d1-6ce2-46ea-adc2-f65454f52a2d", "e-freight Status");
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "RME_EFreightStatus";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.RulesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.RulesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.RulesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.RulesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RulesGrid.GridId = "fb806f39-42bd-434d-84d7-9f1dce0fa283";
			this.RulesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RulesGrid.LayoutKey = "RulesGrid";
			this.RulesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.RulesGrid.Name = "RulesGrid";
			this.RulesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(735, 319, true);
			this.RulesGrid.TabIndex = 0;
			this.EFreightStatusTabPage.PerformLayout();
			this.RulesGroupBox.ResumeLayout(false);
			this.RulesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.RulesGrid)).EndInit();
			this.RulesGrid.ResumeLayout(false);
			this.RulesGrid.PerformLayout();
			this.EFreightStatusTabPage.ResumeLayout(true);

		}

		private void NotesTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.NotesTabPage.SuspendLayout();
			this.NotesTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(true);

		}

		private void CargoImpTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.ChinaCustomsRequiementsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ContactPhoneOCIIdentifierTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ContactNameOCIIdentifierTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ChinaCustomsRequirementsDescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CargoImpTabPage.SuspendLayout();
			this.ChinaCustomsRequiementsGroupBox.SuspendLayout();
			this.CargoImpTabPage.Controls.Add(this.ChinaCustomsRequiementsGroupBox);
			// 
			// ChinaCustomsRequiementsGroupBox
			// 
			this.ChinaCustomsRequiementsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("93557514-7bfc-4126-99fa-efacf9c49f4e", "China Customs Requirements (GACC Circular No.56) for OCI Segment");
			this.ChinaCustomsRequiementsGroupBox.Controls.Add(this.ContactNameOCIIdentifierTextBox);
			this.ChinaCustomsRequiementsGroupBox.Controls.Add(this.ContactPhoneOCIIdentifierTextBox);
			this.ChinaCustomsRequiementsGroupBox.Controls.Add(this.ChinaCustomsRequirementsDescriptionLabel);
			this.ChinaCustomsRequiementsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChinaCustomsRequiementsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ChinaCustomsRequiementsGroupBox.Name = "ChinaCustomsRequiementsGroupBox";
			this.ChinaCustomsRequiementsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(739, 335, true);
			this.ChinaCustomsRequiementsGroupBox.TabIndex = 2;
			this.ChinaCustomsRequiementsGroupBox.TabStop = false;
			// 
			// ContactPhoneOCIIdentifierTextBox
			// 
			this.BindingSource.SetBindingMember(this.ContactPhoneOCIIdentifierTextBox, "RM_ContactPhoneOCIIdentifier");
			this.ContactPhoneOCIIdentifierTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(323, 251, true);
			this.ContactPhoneOCIIdentifierTextBox.Name = "ContactPhoneOCIIdentifierTextBox";
			this.ContactPhoneOCIIdentifierTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 17, true);
			this.ContactPhoneOCIIdentifierTextBox.TabIndex = 1;
			// 
			// ContactNameOCIIdentifierTextBox
			// 
			this.BindingSource.SetBindingMember(this.ContactNameOCIIdentifierTextBox, "RM_ContactNameOCIIdentifier");
			this.ContactNameOCIIdentifierTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(323, 213, true);
			this.ContactNameOCIIdentifierTextBox.Name = "ContactNameOCIIdentifierTextBox";
			this.ContactNameOCIIdentifierTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 17, true);
			this.ContactNameOCIIdentifierTextBox.TabIndex = 0;
			// 
			// ChinaCustomsRequirementsDescriptionLabel
			// 
			this.ChinaCustomsRequirementsDescriptionLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("db9749ca-0a32-4898-9125-0a0fc87cde46", "", "Use this section to configure airline specific requirements for the OCI segment in FWB and FHL for shipments origin or destination China when default values are not otherwise calculated by CW1.\r\n\r\n\t\t\tPlease refer to the following example codes used by different airlines and/or recommended by IATA:\r\n\r\n\t\t\t\t•\tKC – Contact Name for Consignee/Notify Party (when airlines re-use KC – Known Consignor)\r\n\t\t\t\t•\tST – Contact Name for Consignee/Notify Party (when airlines re-use ST – Security Textual Statement)\r\n\t\t\t\t•\tCP – Contact Name for Consignee/Notify Party (new identifier suggested by IATA and adopted by some airlines)\r\n\t\t\t\t•\tU – Telephone Number for Consignee/Notify Party (when airlines re-use U – Unique Consignment Reference Number)\r\n\t\t\t\t•\tCT – Telephone Number for Consignee/Notify Party (new identifier suggested by IATA and adopted by some airlines)\r\n\r\n\t\t\tAny alternate value(s) requested by an individual airline can be entered below.");
			this.ChinaCustomsRequirementsDescriptionLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.ChinaCustomsRequirementsDescriptionLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ChinaCustomsRequirementsDescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.ChinaCustomsRequirementsDescriptionLabel.Name = "ChinaCustomsRequirementsDescriptionLabel";
			this.ChinaCustomsRequirementsDescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(735, 177, true);
			this.ChinaCustomsRequirementsDescriptionLabel.TabIndex = 2;
			this.CargoImpTabPage.PerformLayout();
			this.ChinaCustomsRequiementsGroupBox.ResumeLayout(false);
			this.ChinaCustomsRequiementsGroupBox.PerformLayout();
			this.CargoImpTabPage.ResumeLayout(true);

		}

		void SpecialHandlingCodesTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			//
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo specialHandlingCodeFindBoxColumn = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo descriptionTextBoxColumn = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo originCodeFindBoxColumn = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo destinationCodeFindBoxColumn = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			this.SpecialHandlingCodesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SpecialHandlingCodesDescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SpecialHandlingCodesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SpecialHandlingGridPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.SpecialHandlingCodesTabPage.SuspendLayout();
			this.SpecialHandlingCodesGroupBox.SuspendLayout();
			this.SpecialHandlingGridPanel.SuspendLayout();
			this.SpecialHandlingCodesTabPage.Controls.Add(this.SpecialHandlingCodesGroupBox);
			// 
			// SpecialHandlingCodesGroupBox
			//
			this.SpecialHandlingCodesGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("E7E28F29-CD7F-4C46-A1FE-457EC36AEEC3", "Special Handling Codes");
			this.SpecialHandlingCodesGroupBox.Controls.Add(this.SpecialHandlingCodesDescriptionLabel);
			this.SpecialHandlingCodesGroupBox.Controls.Add(this.SpecialHandlingGridPanel);
			this.SpecialHandlingCodesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SpecialHandlingCodesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.SpecialHandlingCodesGroupBox.Name = "SpecialHandlingCodesGroupBox";
			this.SpecialHandlingCodesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(739, 335, true);
			this.SpecialHandlingCodesGroupBox.TabIndex = 2;
			this.SpecialHandlingCodesGroupBox.TabStop = false;
			// 
			// SpecialHandlingCodesDescriptionLabel
			// 
			this.SpecialHandlingCodesDescriptionLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("C95BE1F7-6D8F-4DA4-A70B-3C935F3BDC13", "", "Use this section to configure airline specific (non-IATA) Special Handling Codes for use on the Air Waybill and inclusion in the FWB message.\r\n\r\n\t\t\tThe Airline specific Codes recorded in the below table with their respective Special Handling Description will be available for selection in the Detail >\r\nDocs > Special Handling list of an air consol for this airline");
			this.SpecialHandlingCodesDescriptionLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.SpecialHandlingCodesDescriptionLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.SpecialHandlingCodesDescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 22, true);
			this.SpecialHandlingCodesDescriptionLabel.Name = "SpecialHandlingCodesDescriptionLabel";
			this.SpecialHandlingCodesDescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(665, 105, true);
			this.SpecialHandlingCodesDescriptionLabel.TabIndex = 1;
			// 
			// SpecialHandlingGridPanel
			//
			this.SpecialHandlingGridPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.SpecialHandlingGridPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 145, true);
			this.SpecialHandlingGridPanel.Name = "SpecialHandlingGridPanel";
			this.SpecialHandlingGridPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 175, true);
			this.SpecialHandlingGridPanel.Controls.Add(this.SpecialHandlingCodesGrid);
			this.SpecialHandlingGridPanel.TabIndex = 2;
			this.SpecialHandlingGridPanel.TabStop = false;
			this.SpecialHandlingGridPanel.ResumeLayout(false);
			this.SpecialHandlingGridPanel.PerformLayout();
			//
			// SpecialHandlingCodesGrid
			//
			this.SpecialHandlingCodesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SpecialHandlingCodesGrid, "RefAirlineSpecialHandlingCodeCollection");
			this.SpecialHandlingCodesGrid.CaptionVisible = false;
			this.SpecialHandlingCodesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			specialHandlingCodeFindBoxColumn.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("9CD612E2-F96A-4CBF-BA1C-3A734B0E67AA", "Code");
			specialHandlingCodeFindBoxColumn.ColumnName = "RHC_Code";
			specialHandlingCodeFindBoxColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			descriptionTextBoxColumn.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("31072887-7CFF-47A2-A56B-CC871D30C6ED", "Special Handling Description");
			descriptionTextBoxColumn.ColumnName = "RHC_Description";
			descriptionTextBoxColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			originCodeFindBoxColumn.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("73F5FF3A-E2DC-447E-8B73-4C9BE28C6CB7", "Origin");
			originCodeFindBoxColumn.ColumnName = "RHC_OriginPortOrCountry";
			originCodeFindBoxColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			destinationCodeFindBoxColumn.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("2DD2C8C1-BC21-4DB5-B825-8B38B8713CA6", "Destination");
			destinationCodeFindBoxColumn.ColumnName = "RHC_DestinationPortOrCountry";
			destinationCodeFindBoxColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.SpecialHandlingCodesGrid.ColumnStyles.Add(originCodeFindBoxColumn);
			this.SpecialHandlingCodesGrid.ColumnStyles.Add(destinationCodeFindBoxColumn);
			this.SpecialHandlingCodesGrid.ColumnStyles.Add(specialHandlingCodeFindBoxColumn);
			this.SpecialHandlingCodesGrid.ColumnStyles.Add(descriptionTextBoxColumn);
			this.SpecialHandlingCodesGrid.GridId = "E40B986C-8258-4BF0-9447-6181C4045193";
			this.SpecialHandlingCodesGrid.LayoutKey = "SpecialHandlingCodesGrid";
			this.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 147, true);
			this.SpecialHandlingCodesGrid.Name = "SpecialHandlingCodesGrid";
			this.SpecialHandlingCodesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(670, 165, true);
			this.SpecialHandlingCodesGrid.TabIndex = 0;
			this.SpecialHandlingCodesGrid.ResumeLayout(false);
			this.SpecialHandlingCodesGrid.PerformLayout();
			this.SpecialHandlingCodesTabPage.PerformLayout();
			this.SpecialHandlingCodesGroupBox.ResumeLayout(false);
			this.SpecialHandlingCodesGroupBox.PerformLayout();
			this.SpecialHandlingCodesTabPage.ResumeLayout(true);
		}

		void DefaultCommodityTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			//
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo productDropEditColumn = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo commodityCodeFindBoxColumn = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo descriptionTextBoxColumn = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo originCodeFindBoxColumn = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo destinationCodeFindBoxColumn = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			this.CommodityCodesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CommodityCodesDescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CommodityCodesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CommodityGridPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DefaultCommodityTabPage.SuspendLayout();
			this.CommodityCodesGroupBox.SuspendLayout();
			this.CommodityGridPanel.SuspendLayout();
			this.DefaultCommodityTabPage.Controls.Add(this.CommodityCodesGroupBox);
			// 
			// CommodityCodesGroupBox
			//
			this.CommodityCodesGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("A7F7A38F-47A4-4C2E-99A7-2068BBCE00DA", "Commodity Codes");
			this.CommodityCodesGroupBox.Controls.Add(this.CommodityCodesDescriptionLabel);
			this.CommodityCodesGroupBox.Controls.Add(this.CommodityGridPanel);
			this.CommodityCodesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CommodityCodesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.CommodityCodesGroupBox.Name = "CommodityCodesGroupBox";
			this.CommodityCodesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(739, 335, true);
			this.CommodityCodesGroupBox.TabIndex = 2;
			this.CommodityCodesGroupBox.TabStop = false;
			// 
			// CommodityCodesDescriptionLabel
			// 
			this.CommodityCodesDescriptionLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("15CCAC29-B146-4493-8A77-F5F2F978263C", "", "Use this section to configure airline specific commodity codes for defaulting directly into the eBooking from upon Product selection.\r\n\r\n\t\t\tThe available list of commodity codes supported by the airline is dependent on the respective product selected in the below table. The settings can\r\nalso be applied based on the origin and/or destination of the eBooking as well. To apply a setting to ALL origins and/or destinations,please leave the respective field in the below table blank.\r\n\r\n\t\t\tPlease note that the same Product cannot be duplicated for the same origin and destination combination.");
			this.CommodityCodesDescriptionLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CommodityCodesDescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 22, true);
			this.CommodityCodesDescriptionLabel.Name = "CommodityCodesDescriptionLabel";
			this.CommodityCodesDescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(665, 115, true);
			this.CommodityCodesDescriptionLabel.TabIndex = 1;
			// 
			// CommodityGridPanel
			//
			this.CommodityGridPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.CommodityGridPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 145, true);
			this.CommodityGridPanel.Name = "CommodityGridPanel";
			this.CommodityGridPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 175, true);
			this.CommodityGridPanel.Controls.Add(this.CommodityCodesGrid);
			this.CommodityGridPanel.TabIndex = 2;
			this.CommodityGridPanel.TabStop = false;
			this.CommodityGridPanel.ResumeLayout(false);
			this.CommodityGridPanel.PerformLayout();
			//
			// CommodityCodesGrid
			//
			this.CommodityCodesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CommodityCodesGrid, "DefaultCommodityCodeCollection");
			this.CommodityCodesGrid.CaptionVisible = false;
			this.CommodityCodesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			productDropEditColumn.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("EB07156E-63A5-40EC-9A59-623F379E0A93", "Product");
			productDropEditColumn.ColumnName = "RDC_RAR_NKProductCode";
			productDropEditColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			commodityCodeFindBoxColumn.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("C4BCFC03-1669-4AD7-A261-B3F1ADA9D015", "Commodity Code");
			commodityCodeFindBoxColumn.ColumnName = "RDC_RAC_NKCommodityCode";
			commodityCodeFindBoxColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			descriptionTextBoxColumn.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("87259BEB-2808-410A-8A3A-679B0F82C0E9", "Description");
			descriptionTextBoxColumn.ColumnName = "RDC_Description";
			descriptionTextBoxColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			originCodeFindBoxColumn.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("94937066-2C73-4289-B7DC-1641EAC0AEEA", "Origin");
			originCodeFindBoxColumn.ColumnName = "RDC_RL_NKOrigin";
			originCodeFindBoxColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			destinationCodeFindBoxColumn.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("9E0E1F07-0285-444E-8765-0572D800FE3C", "Destination");
			destinationCodeFindBoxColumn.ColumnName = "RDC_RL_NKDestination";
			destinationCodeFindBoxColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.CommodityCodesGrid.ColumnStyles.Add(productDropEditColumn);
			this.CommodityCodesGrid.ColumnStyles.Add(commodityCodeFindBoxColumn);
			this.CommodityCodesGrid.ColumnStyles.Add(descriptionTextBoxColumn);
			this.CommodityCodesGrid.ColumnStyles.Add(originCodeFindBoxColumn);
			this.CommodityCodesGrid.ColumnStyles.Add(destinationCodeFindBoxColumn);
			this.CommodityCodesGrid.GridId = "8B3E0ABE-870D-41F3-97FE-93CF2B3F8619";
			this.CommodityCodesGrid.LayoutKey = "CommodityCodesGrid";
			this.CommodityCodesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3 + 3, 147, true);
			this.CommodityCodesGrid.Name = "CommodityCodesGrid";
			this.CommodityCodesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700 - 30, 165, true);
			this.CommodityCodesGrid.TabIndex = 0;
			this.CommodityCodesGrid.ResumeLayout(false);
			this.CommodityCodesGrid.PerformLayout();
			this.DefaultCommodityTabPage.PerformLayout();
			this.CommodityCodesGroupBox.ResumeLayout(false);
			this.CommodityCodesGroupBox.PerformLayout();
			this.DefaultCommodityTabPage.ResumeLayout(true);
		}

		#endregion

		Enterprise.ZArchitecture.ZTextBox RM_AirlineName2BoundTextBox;
		Enterprise.ZArchitecture.ZTextBox RM_AirlineName1BoundTextBox;
		Enterprise.ZArchitecture.ZTextBox RM_AirlineCountryBoundTextBox;
		Enterprise.ZArchitecture.ZTextBox RM_AirlineCityBoundTextBox;
		Enterprise.ZArchitecture.ZTextBox RM_AirlinePostalCodeBoundTextBox;
		Enterprise.ZArchitecture.ZTextBox RM_AirlineStateBoundTextBox;
		Enterprise.ZArchitecture.ZTextBox RM_AddressLine2BoundTextBox;
		Enterprise.ZArchitecture.ZTextBox RM_AddressLine1BoundTextBox;
		Enterprise.ZArchitecture.GUI.ZGroupBox DetailsGroupBox;
		Enterprise.ZArchitecture.GUI.ZGroupBox CodesGroupBox;
		Enterprise.ZArchitecture.ZTextBox RM_TwoCharacterCodeBoundTextBox;
		Enterprise.ZArchitecture.ZTextBox RM_ThreeLetterCodeBoundTextBox;
		Enterprise.ZArchitecture.ZTextBox RM_EagleAddedAirlinePrefixOrAccountingCodeBoundTextBox;
		Enterprise.ZArchitecture.GUI.ZGroupBox CASSControlledGroupBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox CASSControlledCheckBox;
		Enterprise.ZArchitecture.ZTextBox RM_LabelShortNameText;
		Enterprise.ZArchitecture.GUI.ZTabPage OtherDetailsTabPage;
		Enterprise.ZArchitecture.GUI.ZGroupBox ReservationDetailsGroupBox;
		Enterprise.ZArchitecture.ZTextBox zTextBox11;
		Enterprise.ZArchitecture.ZTextBox zTextBox12;
		Enterprise.ZArchitecture.ZTextBox zTextBox13;
		Enterprise.ZArchitecture.ZTextBox zTextBox14;
		Enterprise.ZArchitecture.GUI.ZGroupBox EmergencyDetailsGroupBox;
		Enterprise.ZArchitecture.ZTextBox zTextBox1;
		Enterprise.ZArchitecture.ZTextBox zTextBox2;
		Enterprise.ZArchitecture.ZTextBox zTextBox3;
		Enterprise.ZArchitecture.GUI.ZCheckBox zCheckBox4;
		Enterprise.ZArchitecture.GUI.ZCheckBox zCheckBox3;
		Enterprise.ZArchitecture.GUI.ZCheckBox zCheckBox2;
		Enterprise.ZArchitecture.GUI.ZCheckBox zCheckBox1;
		Enterprise.ZArchitecture.GUI.ZCheckBox RM_IsActiveCheckBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox RM_IsUpdatableCheckBox;
		Enterprise.ZArchitecture.GUI.ZGroupBox OtherDetailsGroupBox;
		Enterprise.ZArchitecture.ZTextBox zTextBox4;
		Enterprise.ZArchitecture.ZTextBox zTextBox5;
		Enterprise.ZArchitecture.ZTextBox zTextBox6;
		Enterprise.ZArchitecture.ZTextBox zTextBox7;
		Enterprise.ZArchitecture.GUI.ZCheckBox zCheckBox5;
		Enterprise.ZArchitecture.GUI.ZCheckBox hasSignedEAWBAgreementCheckBox;
		Enterprise.ZArchitecture.GUI.ZTabPage EFreightStatusTabPage;
		Enterprise.ZArchitecture.GUI.ZGroupBox RulesGroupBox;
		Enterprise.ZArchitecture.ZGrid RulesGrid;
		Enterprise.ZArchitecture.GUI.ZTabPage AirlineSpecificCodesMainTabPage;
		Enterprise.ZArchitecture.GUI.ZTabPage DefaultCommodityTabPage;
		Enterprise.ZArchitecture.GUI.ZTabPage SpecialHandlingCodesTabPage;
		Enterprise.ZArchitecture.GUI.ZTabControl AirlineSpecificCodesTabControl;
		Enterprise.ZArchitecture.GUI.ZGroupBox CommodityCodesGroupBox;
		Enterprise.ZArchitecture.GUI.ZGroupBox SpecialHandlingCodesGroupBox;
		Enterprise.ZArchitecture.ZGrid CommodityCodesGrid;
		Enterprise.ZArchitecture.ZGrid SpecialHandlingCodesGrid;
		private Enterprise.ZArchitecture.GUI.ZTabPage CargoImpTabPage;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ChinaCustomsRequiementsGroupBox;
		private Enterprise.ZArchitecture.ZTextBox ContactPhoneOCIIdentifierTextBox;
		private Enterprise.ZArchitecture.ZTextBox ContactNameOCIIdentifierTextBox;
		Enterprise.ZArchitecture.ZLabel ChinaCustomsRequirementsDescriptionLabel;
		Enterprise.ZArchitecture.ZLabel CommodityCodesDescriptionLabel;
		Enterprise.ZArchitecture.ZLabel SpecialHandlingCodesDescriptionLabel;
		Enterprise.ZArchitecture.GUI.ZPanel CommodityGridPanel;
		Enterprise.ZArchitecture.GUI.ZPanel SpecialHandlingGridPanel;
	}
}
