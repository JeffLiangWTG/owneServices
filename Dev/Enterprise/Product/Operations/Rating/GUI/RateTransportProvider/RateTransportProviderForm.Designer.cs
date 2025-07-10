using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class RateTransportProviderForm
	{
		private ZGroupBox ZoneItemGroupBox;
		private RateTransportZoneItemsGrid ZoneItemGrid;
		private ZGroupBox ZonesGroupBox;
		protected ZGrid ZoneGrid;
		private MasterFiles.GUI.ZOrganisationFindBox TP_OH_RelatedPartyFindBox;
		private ZCodeFindBox CountryCodeFindBox;
		private ZCheckBox IsActiveCheckBox;
		private ZTimeEdit DefaultDeliveryDueTime;
		private ZTimeEdit DefaultHoldForPickupTime;
		private ZDropEdit ZoneSetTypeDropCodeBox;
		private ZDropEdit ZoneSetModeDropCodeBox;
		private ZGuidFindBox TP_R9_ZoneHubLocationFindBox;
		private ZLabel ZoneHubStateLabel;

		new void InitializeComponent()
		{
			CityTownColumnStyleInfo cityTownColumnStyleInfo1 = new CityTownColumnStyleInfo();
			PostCodeColumnStyleInfo postCodeColumnStyleInfo1 = new PostCodeColumnStyleInfo();
			PostCodeColumnStyleInfo postCodeColumnStyleInfo2 = new PostCodeColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new ZCodeFindBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZCalcEditColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZCheckBoxColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new ZCheckBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo pickupDeliveryZoneColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new ZCheckBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo = new ZDateEditColumnStyleInfo();
			this.ZoneItemGroupBox = new ZGroupBox();
			this.ZoneItemGrid = new RateTransportZoneItemsGrid();
			this.ZonesGroupBox = new ZGroupBox();
			this.ZoneGrid = new ZGrid();
			this.TP_OH_RelatedPartyFindBox = new MasterFiles.GUI.ZOrganisationFindBox();
			this.CountryCodeFindBox = new ZCodeFindBox();
			this.ZoneSetTypeDropCodeBox = new ZDropEdit();
			this.ZoneSetModeDropCodeBox = new ZDropEdit();
			this.IsActiveCheckBox = new ZCheckBox();
			this.DefaultDeliveryDueTime = new ZTimeEdit();
			this.DefaultHoldForPickupTime = new ZTimeEdit();
			this.TP_R9_ZoneHubLocationFindBox = new ZGuidFindBox();
			this.ZoneHubStateLabel = new ZLabel();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ZoneItemGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ZoneItemGrid)).BeginInit();
			this.ZoneItemGrid.SuspendLayout();
			this.ZonesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ZoneGrid)).BeginInit();
			this.ZoneGrid.SuspendLayout();
			this.TP_OH_RelatedPartyFindBox.SuspendLayout();
			this.CountryCodeFindBox.SuspendLayout();
			this.ZoneSetTypeDropCodeBox.SuspendLayout();
			this.ZoneSetModeDropCodeBox.SuspendLayout();
			this.TP_R9_ZoneHubLocationFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(956, 445, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.IsActiveCheckBox);
			this.MainTabPage.Controls.Add(this.ZoneHubStateLabel);
			this.MainTabPage.Controls.Add(this.TP_R9_ZoneHubLocationFindBox);
			this.MainTabPage.Controls.Add(this.CountryCodeFindBox);
			this.MainTabPage.Controls.Add(this.ZoneSetTypeDropCodeBox);
			this.MainTabPage.Controls.Add(this.ZoneSetModeDropCodeBox);
			this.MainTabPage.Controls.Add(this.ZoneItemGroupBox);
			this.MainTabPage.Controls.Add(this.ZonesGroupBox);
			this.MainTabPage.Controls.Add(this.TP_OH_RelatedPartyFindBox);
			this.MainTabPage.Controls.Add(this.DefaultDeliveryDueTime);
			this.MainTabPage.Controls.Add(this.DefaultHoldForPickupTime);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1300, 418, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(948, 398, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(948, 418, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(956, 445, true);
			// 
			// PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel
			// 
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.TabIndex = 0;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(956, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 0;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(325);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(325);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(RateTransportProvider);
			// 
			// ZoneItemGroupBox
			// 
			this.ZoneItemGroupBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom
			| System.Windows.Forms.AnchorStyles.Left
			| System.Windows.Forms.AnchorStyles.Right;
			this.ZoneItemGroupBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("dc15718c-03a4-48a9-82f8-e3a29e08fb80", "Zone Definition");
			this.ZoneItemGroupBox.Controls.Add(this.ZoneItemGrid);
			this.ZoneItemGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(232, 61, true);
			this.ZoneItemGroupBox.Name = "ZoneItemGroupBox";
			this.ZoneItemGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 353, true);
			this.ZoneItemGroupBox.TabIndex = 20;
			this.ZoneItemGroupBox.TabStop = false;
			// 
			// ZoneItemGrid
			// 
			this.ZoneItemGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ZoneItemGrid, "Zones.Items");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateTransportZone)(((System.Collections.IList)(((RateTransportProvider)(null)).Zones)).SyncRoot)).Items);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateTransportZoneItem)(((System.Collections.IList)(((RateTransportZone)(((System.Collections.IList)(((RateTransportProvider)(null)).Zones)).SyncRoot)).Items)).SyncRoot)).TQ_FromPostCode);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateTransportZoneItem)(((System.Collections.IList)(((RateTransportZone)(((System.Collections.IList)(((RateTransportProvider)(null)).Zones)).SyncRoot)).Items)).SyncRoot)).PostCodeControlType);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateTransportZoneItem)(((System.Collections.IList)(((RateTransportZone)(((System.Collections.IList)(((RateTransportProvider)(null)).Zones)).SyncRoot)).Items)).SyncRoot)).TQ_ToPostCode);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateTransportZoneItem)(((System.Collections.IList)(((RateTransportZone)(((System.Collections.IList)(((RateTransportProvider)(null)).Zones)).SyncRoot)).Items)).SyncRoot)).TQ_R9_CityTown);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateTransportZoneItem)(((System.Collections.IList)(((RateTransportZone)(((System.Collections.IList)(((RateTransportProvider)(null)).Zones)).SyncRoot)).Items)).SyncRoot)).CityTownControlType);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateTransportZoneItem)(((System.Collections.IList)(((RateTransportZone)(((System.Collections.IList)(((RateTransportProvider)(null)).Zones)).SyncRoot)).Items)).SyncRoot)).CityTown.State.RW_Code);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateTransportZoneItem)(((System.Collections.IList)(((RateTransportZone)(((System.Collections.IList)(((RateTransportProvider)(null)).Zones)).SyncRoot)).Items)).SyncRoot)).CityTown.State.RW_Description);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateTransportZoneItem)(((System.Collections.IList)(((RateTransportZone)(((System.Collections.IList)(((RateTransportProvider)(null)).Zones)).SyncRoot)).Items)).SyncRoot)).TQ_RN_NKCountry);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateTransportZoneItem)(((System.Collections.IList)(((RateTransportZone)(((System.Collections.IList)(((RateTransportProvider)(null)).Zones)).SyncRoot)).Items)).SyncRoot)).TQ_FromDistance);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateTransportZoneItem)(((System.Collections.IList)(((RateTransportZone)(((System.Collections.IList)(((RateTransportProvider)(null)).Zones)).SyncRoot)).Items)).SyncRoot)).TQ_ToDistance);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateTransportZoneItem)(((System.Collections.IList)(((RateTransportZone)(((System.Collections.IList)(((RateTransportProvider)(null)).Zones)).SyncRoot)).Items)).SyncRoot)).TQ_IsBeyond);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateTransportZoneItem)(((System.Collections.IList)(((RateTransportZone)(((System.Collections.IList)(((RateTransportProvider)(null)).Zones)).SyncRoot)).Items)).SyncRoot)).TQ_IsExcludingPostCode);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateTransportZoneItem)(((System.Collections.IList)(((RateTransportZone)(((System.Collections.IList)(((RateTransportProvider)(null)).Zones)).SyncRoot)).Items)).SyncRoot)).TQ_DeliveryDueTime);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateTransportZoneItem)(((System.Collections.IList)(((RateTransportZone)(((System.Collections.IList)(((RateTransportProvider)(null)).Zones)).SyncRoot)).Items)).SyncRoot)).TQ_PickupDeliveryZone);
			this.ZoneItemGrid.CaptionVisible = false;
			postCodeColumnStyleInfo1.BindToDecimalPlaces = null;
			postCodeColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			postCodeColumnStyleInfo1.ColumnName = "TQ_FromPostCode";
			postCodeColumnStyleInfo1.FieldTypeColumnName = "PostCodeControlType";
			postCodeColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			postCodeColumnStyleInfo2.BindToDecimalPlaces = null;
			postCodeColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			postCodeColumnStyleInfo2.ColumnName = "TQ_ToPostCode";
			postCodeColumnStyleInfo2.FieldTypeColumnName = "PostCodeControlType";
			postCodeColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			cityTownColumnStyleInfo1.BindToDecimalPlaces = null;
			cityTownColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			cityTownColumnStyleInfo1.ColumnName = "TQ_R9_CityTown";
			cityTownColumnStyleInfo1.FieldTypeColumnName = "CityTownControlType";
			cityTownColumnStyleInfo1.IsMandatory = true;
			cityTownColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("7b656709-5c94-4fbe-9510-0463adf3a7b2", "State", "Zone Hub State or Province", "");
			zTextBoxColumnStyleInfo1.ColumnName = "CityTown+State+RW_Code";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("82e435a9-3a54-4f67-9d50-84fd98398613", "State Description");
			zTextBoxColumnStyleInfo2.ColumnName = "CityTown+State+RW_Description";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCodeFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("b213de23-76ac-439c-ad6d-81a6bbef562f", "Country/Region");
			zCodeFindBoxColumnStyleInfo2.ColumnName = "TQ_RN_NKCountry";
			zCodeFindBoxColumnStyleInfo2.IsMandatory = true;
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "TQ_FromDistance";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "TQ_ToDistance";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "BeyondDays";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "BeyondHours";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo.ColumnName = "TQ_DeliveryDueTime";
			zDateEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zDateEditColumnStyleInfo.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Time;
			pickupDeliveryZoneColumnStyleInfo.ColumnName = "TQ_PickupDeliveryZone";
			pickupDeliveryZoneColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zCheckBoxColumnStyleInfo1.ColumnName = "TQ_IsBeyond";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCheckBoxColumnStyleInfo2.ColumnName = "TQ_IsExcludingPostCode";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.ZoneItemGrid.ColumnStyles.Add(postCodeColumnStyleInfo1);
			this.ZoneItemGrid.ColumnStyles.Add(postCodeColumnStyleInfo2);
			this.ZoneItemGrid.ColumnStyles.Add(cityTownColumnStyleInfo1);
			this.ZoneItemGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ZoneItemGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ZoneItemGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.ZoneItemGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ZoneItemGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ZoneItemGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ZoneItemGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.ZoneItemGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.ZoneItemGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.ZoneItemGrid.ColumnStyles.Add(zDateEditColumnStyleInfo);
			this.ZoneItemGrid.ColumnStyles.Add(pickupDeliveryZoneColumnStyleInfo);
			this.ZoneItemGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ZoneItemGrid.GridId = "f9373f07-0651-48a6-bb2d-49e8c888bd3c";
			this.ZoneItemGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ZoneItemGrid.LayoutKey = "zGrid1";
			this.ZoneItemGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ZoneItemGrid.Name = "ZoneItemGrid";
			this.ZoneItemGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(706, 334, true);
			this.ZoneItemGrid.TabIndex = 0;
			this.ZoneItemGrid.AfterBind += new System.EventHandler(this.ZoneItemGrid_AfterBind);
			// 
			// ZonesGroupBox
			// 
			this.ZonesGroupBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom
			| System.Windows.Forms.AnchorStyles.Left;
			this.ZonesGroupBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("572c2c82-a6ed-4c99-b6e0-354f225ab8d8", "Zones");
			this.ZonesGroupBox.Controls.Add(this.ZoneGrid);
			this.ZonesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 61, true);
			this.ZonesGroupBox.Name = "ZonesGroupBox";
			this.ZonesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 313, true);
			this.ZonesGroupBox.TabIndex = 11;
			this.ZonesGroupBox.TabStop = false;
			// 
			// ZoneGrid
			// 
			this.ZoneGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ZoneGrid, "Zones");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateTransportProvider)(null)).Zones);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateTransportZone)(((System.Collections.IList)(((RateTransportProvider)(null)).Zones)).SyncRoot)).TZ_ZoneName);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateTransportZone)(((System.Collections.IList)(((RateTransportProvider)(null)).Zones)).SyncRoot)).TZ_IsActive);
			this.ZoneGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo3.ColumnName = "TZ_ZoneName";
			zTextBoxColumnStyleInfo3.IsMandatory = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zCheckBoxColumnStyleInfo3.ColumnName = "TZ_IsActive";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(62);
			this.ZoneGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ZoneGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.ZoneGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ZoneGrid.GridId = "1087893b-bb06-47ae-b574-cfc6edfba813";
			this.ZoneGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ZoneGrid.LayoutKey = "ZoneGrid";
			this.ZoneGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ZoneGrid.Name = "ZoneGrid";
			this.ZoneGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(215, 294, true);
			this.ZoneGrid.TabIndex = 0;
			// 
			// TP_OH_RelatedPartyFindBox
			// 
			this.TP_OH_RelatedPartyFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TP_OH_RelatedPartyFindBox, "TP_OH_RelatedParty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateTransportProvider)(null)).TP_OH_RelatedParty);
			this.TP_OH_RelatedPartyFindBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("a1348697-8a3d-427a-91fe-be21190925f7", "Zone Owner");
			this.TP_OH_RelatedPartyFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(700, 33, true);
			this.TP_OH_RelatedPartyFindBox.ModuleID = ((ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.TP_OH_RelatedPartyFindBox.Name = "TP_OH_RelatedPartyFindBox";
			this.TP_OH_RelatedPartyFindBox.PreBoundMaxLength = 12;
			this.TP_OH_RelatedPartyFindBox.ShowDescriptionBox = false;
			this.TP_OH_RelatedPartyFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 20, true);
			this.TP_OH_RelatedPartyFindBox.TabIndex = 10;
			// 
			// CountryCodeFindBox
			// 
			this.CountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryCodeFindBox, "CountryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateTransportProvider)(null)).CountryCode);
			this.CountryCodeFindBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("c13ec091-ca69-4e1e-8276-5920fe2bd348", "Country/Region", "Zone Country/Region", "All Zone Definitions are restricted within this country/region.");
			this.CountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 33, true);
			this.CountryCodeFindBox.Name = "CountryCodeFindBox";
			this.CountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.CountryCodeFindBox.TabIndex = 7;
			// 
			// ZoneSetTypeDropCodeBox
			// 
			this.ZoneSetTypeDropCodeBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ZoneSetTypeDropCodeBox, "TP_ZoneType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateTransportProvider)(null)).TP_ZoneType);
			this.ZoneSetTypeDropCodeBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 6, true);
			this.ZoneSetTypeDropCodeBox.Name = "ZoneSetTypeDropCodeBox";
			this.ZoneSetTypeDropCodeBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.ZoneSetTypeDropCodeBox.TabIndex = 1;
			// 
			// ZoneSetModeDropCodeBox
			// 
			this.ZoneSetModeDropCodeBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ZoneSetModeDropCodeBox, "TP_ZoneMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateTransportProvider)(null)).TP_ZoneMode);
			this.ZoneSetModeDropCodeBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 6, true);
			this.ZoneSetModeDropCodeBox.Name = "ZoneSetModeDropCodeBox";
			this.ZoneSetModeDropCodeBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.ZoneSetModeDropCodeBox.TabIndex = 3;
			// 
			// IsActiveCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsActiveCheckBox, "TP_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateTransportProvider)(null)).TP_IsActive);
			this.IsActiveCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(700, 6, true);
			this.IsActiveCheckBox.Name = "IsActiveCheckBox";
			this.IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 24, true);
			this.IsActiveCheckBox.TabIndex = 5;
			this.IsActiveCheckBox.UseVisualStyleBackColor = true;
			// 
			// DefaultDeliveryDueTime
			// 
			this.BindingSource.SetBindingMember(this.DefaultDeliveryDueTime, "TP_DefaultDeliveryDueTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateTransportProvider)(null)).TP_DefaultDeliveryDueTime);
			this.DefaultDeliveryDueTime.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1100, 6, true);
			this.DefaultDeliveryDueTime.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("9f858880-8320-40fe-be29-89ad6170a079", "Default Delivery Due Time");
			this.DefaultDeliveryDueTime.Name = "DefaultDeliveryDueTime";
			this.DefaultDeliveryDueTime.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 24, true);
			this.DefaultDeliveryDueTime.TabIndex = 6;
			// 
			// DefaultHoldforPickupTime
			// 
			this.BindingSource.SetBindingMember(this.DefaultHoldForPickupTime, "TP_DefaultHoldForPickupTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateTransportProvider)(null)).TP_DefaultHoldForPickupTime);
			this.DefaultHoldForPickupTime.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1100, 33, true);
			this.DefaultHoldForPickupTime.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("8859b1ca-80c7-4890-83fa-129e2e9622b5", "Default Hold For Pickup Time");
			this.DefaultHoldForPickupTime.Name = "DefaultHoldforPickupTime";
			this.DefaultHoldForPickupTime.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 33, true);
			this.DefaultHoldForPickupTime.TabIndex = 11;
			// 
			// TP_R9_ZoneHubLocationFindBox
			// 
			this.TP_R9_ZoneHubLocationFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TP_R9_ZoneHubLocationFindBox, "TP_R9_ZoneHubLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateTransportProvider)(null)).TP_R9_ZoneHubLocation);
			this.TP_R9_ZoneHubLocationFindBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("86bba286-c2d2-441d-b81a-f1604116564a", "Zone Hub", "Zone Hub Location", "The City or Town where this Transport Zone Set originates from. Zones definitions are restricted to be within the Zone Country, but are not restricted by the Originating Hub Location.");
			this.TP_R9_ZoneHubLocationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 33, true);
			this.TP_R9_ZoneHubLocationFindBox.Name = "TP_R9_ZoneHubLocationFindBox";
			this.TP_R9_ZoneHubLocationFindBox.ShowDescriptionBox = false;
			this.TP_R9_ZoneHubLocationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.TP_R9_ZoneHubLocationFindBox.TabIndex = 9;
			// 
			// ZoneHubStateLabel
			// 
			this.ZoneHubStateLabel.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ZoneHubStateLabel, "ZoneHubLocation+R9_RW_NKState");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateTransportProvider)(null)).ZoneHubLocation.R9_RW_NKState);
			this.ZoneHubStateLabel.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("d90fe5e3-41b6-4db6-b77e-9facc913ccc1", "State");
			this.ZoneHubStateLabel.FontType = Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif;
			this.ZoneHubStateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(510, 32, true);
			this.ZoneHubStateLabel.Name = "ZoneHubStateLabel";
			this.ZoneHubStateLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(118, 19, true);
			this.ZoneHubStateLabel.TabIndex = 8;
			// 
			// RateTransportProviderForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("002b2129-049b-4b5d-887f-0c687fd45f29", "Transport Zone Set");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1300, 501, true);
			this.DataSourceAssemblyName = "Enterprise.Rating.Business";
			this.DataSourceType = typeof(RateTransportProvider);
			this.DataSourceTypeName = "Enterprise.Rating.Business.RateTransportProvider";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1300, 540, true);
			this.Name = "RateTransportProviderForm";
			this.ShouldSerializeTabPageMethods = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ZoneItemGroupBox.ResumeLayout(false);
			this.ZoneItemGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ZoneItemGrid)).EndInit();
			this.ZoneItemGrid.ResumeLayout(false);
			this.ZoneItemGrid.PerformLayout();
			this.ZonesGroupBox.ResumeLayout(false);
			this.ZonesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ZoneGrid)).EndInit();
			this.ZoneGrid.ResumeLayout(false);
			this.ZoneGrid.PerformLayout();
			this.TP_OH_RelatedPartyFindBox.ResumeLayout(true);
			this.TP_OH_RelatedPartyFindBox.PerformLayout();
			this.CountryCodeFindBox.ResumeLayout(true);
			this.CountryCodeFindBox.PerformLayout();
			this.ZoneSetTypeDropCodeBox.ResumeLayout(true);
			this.ZoneSetTypeDropCodeBox.PerformLayout();
			this.ZoneSetModeDropCodeBox.ResumeLayout(true);
			this.ZoneSetModeDropCodeBox.PerformLayout();
			this.TP_R9_ZoneHubLocationFindBox.ResumeLayout(true);
			this.TP_R9_ZoneHubLocationFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
