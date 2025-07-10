using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
namespace Enterprise.TransportBookings.GUI
{
	partial class BookingControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>

		System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BookingControl));
			this.JobDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TransportProvidersAndOrgsTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.AllocatedPackagesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TransportProviderTab = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.OrganisationsTab = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.TemplateDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.WaybillTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ConsolIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TransportReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TransportRefLinkButton = new Enterprise.MasterFiles.GUI.ZButtonTransportCoHotlink();
			this.GoodsDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.StatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ParentLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.OverrideParentCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.BookingForGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OverrideChargeableCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ChargeableWeightUnitLabel = new Enterprise.ZArchitecture.ZLabel();
			this.VolumeWeightUnitLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TotalVolumeUnitLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TotalWeightUnitLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TotalPacksUnitLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TotalWeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalVolumeCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ChargeableWeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalPacksCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.VolumeWeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.BranchGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.CarrierAccountDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.BillingPartyOrClientPKAddressControl = new Enterprise.MasterFiles.GUI.ZOrgAddressControl();
			this.BookingPartyDocumentaryAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.RequiresRefrigerationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsHazardousCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RatingFreightModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ServiceLevelDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CarrierServiceLevelDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.BookingRequestedDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CarrierBookingAgentControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.MasterTBTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TotalCO2e = new Enterprise.ZArchitecture.ZTextBox();
			this.TotalCO2eUnit = new Enterprise.ZArchitecture.ZLabel();
			this.TransportModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.JobDetailsGroupBox.SuspendLayout();
			this.TransportProvidersAndOrgsTabControl.SuspendLayout();
			this.AllocatedPackagesGroupBox.SuspendLayout();
			this.TransportProviderTab.SuspendLayout();
			this.OrganisationsTab.SuspendLayout();
			this.TemplateDropEdit.SuspendLayout();
			this.DocAddressControl.SuspendLayout();
			this.BookingForGroupBox.SuspendLayout();
			this.BranchGuidFindBox.SuspendLayout();
			this.CarrierAccountDropEdit.SuspendLayout();
			this.BillingPartyOrClientPKAddressControl.SuspendLayout();
			this.BookingPartyDocumentaryAddressControl.SuspendLayout();
			this.RatingFreightModeDropEdit.SuspendLayout();
			this.ServiceLevelDropEdit.SuspendLayout();
			this.CarrierServiceLevelDropEdit.SuspendLayout();
			this.BookingRequestedDateEdit.SuspendLayout();
			this.CarrierBookingAgentControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.TransportBookings.Business.DtbBooking);
			//
			// JobDetailsGroupBox
			//
			this.JobDetailsGroupBox.Controls.Add(this.TemplateDropEdit);
			this.JobDetailsGroupBox.Controls.Add(this.RatingFreightModeDropEdit);
			this.JobDetailsGroupBox.Controls.Add(this.WaybillTextBox);
			this.JobDetailsGroupBox.Controls.Add(this.GoodsDescriptionTextBox);
			this.JobDetailsGroupBox.Controls.Add(this.BranchGuidFindBox);
			this.JobDetailsGroupBox.Controls.Add(this.ServiceLevelDropEdit);
			this.JobDetailsGroupBox.Controls.Add(this.ConsolIDTextBox);
			this.JobDetailsGroupBox.Controls.Add(this.MasterTBTextBox);
			this.JobDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.None;
			this.JobDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 17, true);
			this.JobDetailsGroupBox.Name = "JobDetailsGroupBox";
			this.JobDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 239, true);
			this.JobDetailsGroupBox.TabIndex = 3;
			this.JobDetailsGroupBox.TabStop = false;
			this.JobDetailsGroupBox.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("d3822195-b3b9-4836-83b7-6373af90916b", "Job Details");
			// 
			// TemplateDropEdit
			// 
			this.TemplateDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TemplateDropEdit, "KM_KT_NKBookingTemplate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).KM_KT_NKBookingTemplate)));
			this.TemplateDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 19, true);
			this.TemplateDropEdit.Name = "TemplateDropEdit";
			this.TemplateDropEdit.PreBoundMaxLength = 4;
			this.TemplateDropEdit.ShouldResizeByMaxLength = true;
			this.TemplateDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 20, true);
			this.TemplateDropEdit.TabIndex = 4;
			// 
			// RatingFreightModeDropEdit
			// 
			this.RatingFreightModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RatingFreightModeDropEdit, "KM_RatingFreightMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).KM_RatingFreightMode)));
			this.RatingFreightModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 41, true);
			this.RatingFreightModeDropEdit.Name = "RatingFreightModeDropEdit";
			this.RatingFreightModeDropEdit.PreBoundMaxLength = 3;
			this.RatingFreightModeDropEdit.ShouldResizeByMaxLength = true;
			this.RatingFreightModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 20, true);
			this.RatingFreightModeDropEdit.TabIndex = 5;
			// 
			// WaybillTextBox
			// 
			this.BindingSource.SetBindingMember(this.WaybillTextBox, "WayBillNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).WayBillNumber)));
			this.WaybillTextBox.CaptionResourceString = null;
			this.WaybillTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 63, true);
			this.WaybillTextBox.Name = "WaybillTextBox";
			this.WaybillTextBox.ReadOnly = true;
			this.WaybillTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.WaybillTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 20, true);
			this.WaybillTextBox.TabIndex = 6;
			// 
			// GoodsDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.GoodsDescriptionTextBox, "ConsolidationSingleJob+KB_GoodsDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).ConsolidationSingleJob.KB_GoodsDescription)));
			this.GoodsDescriptionTextBox.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("384d303c-bed0-408f-8753-8fd1125f36ac", "Goods Desc.", "Goods Description", "");
			this.GoodsDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 85, true);
			this.GoodsDescriptionTextBox.Name = "GoodsDescriptionTextBox";
			this.GoodsDescriptionTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.GoodsDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 20, true);
			this.GoodsDescriptionTextBox.TabIndex = 7;
			// 
			// BranchGuidFindBox
			// 
			this.BranchGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BranchGuidFindBox, "KM_GB_Branch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).KM_GB_Branch)));
			this.BranchGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 107, true);
			this.BranchGuidFindBox.Name = "BranchGuidFindBox";
			this.BranchGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.BranchGuidFindBox.ParentType = null;
			this.BranchGuidFindBox.PopupCaption = null;
			this.BranchGuidFindBox.PreBoundMaxLength = 3;
			this.BranchGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 20, true);
			this.BranchGuidFindBox.TabIndex = 8;
			// 
			// ServiceLevelDropEdit
			// 
			this.ServiceLevelDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ServiceLevelDropEdit, "KM_RS_NKServiceLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).KM_RS_NKServiceLevel)));
			this.ServiceLevelDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 129, true);
			this.ServiceLevelDropEdit.Name = "ServiceLevelDropEdit";
			this.ServiceLevelDropEdit.PreBoundMaxLength = 3;
			this.ServiceLevelDropEdit.ShouldResizeByMaxLength = true;
			this.ServiceLevelDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 20, true);
			this.ServiceLevelDropEdit.TabIndex = 9;
			// 
			// ConsolIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.ConsolIDTextBox, "ConsolidationMultiJob+KB_JobID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).ConsolidationMultiJob.KB_JobID)));
			this.ConsolIDTextBox.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("205fbdb4-d865-44ef-a0cb-108c016c1933", "TB Consol");
			this.ConsolIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 151, true);
			this.ConsolIDTextBox.Name = "ConsolIDTextBox";
			this.ConsolIDTextBox.ReadOnly = true;
			this.ConsolIDTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.ConsolIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.ConsolIDTextBox.TabIndex = 10;
			// 
			// MasterTBTextBox
			// 
			this.BindingSource.SetBindingMember(this.MasterTBTextBox, "MasterBooking.KM_JobID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).MasterBooking.KM_JobID)));
			this.MasterTBTextBox.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("34897ad2-548e-42c3-bac8-ddecbfa15aa6", "Master TB");
			this.MasterTBTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 173, true);
			this.MasterTBTextBox.Name = "MasterTBTextBox";
			this.MasterTBTextBox.ReadOnly = true;
			this.MasterTBTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.MasterTBTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.MasterTBTextBox.TabIndex = 11;
			this.MasterTBTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			// 
			// TransportProvidersAndOrgsTabControl
			// 
			this.TransportProvidersAndOrgsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.TransportProvidersAndOrgsTabControl.Controls.Add(this.TransportProviderTab);
			this.TransportProvidersAndOrgsTabControl.Controls.Add(this.OrganisationsTab);
			this.TransportProvidersAndOrgsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(286, 17, true);
			this.TransportProvidersAndOrgsTabControl.Name = "TransportProvidersAndOrgsTabControl";
			this.TransportProvidersAndOrgsTabControl.SelectedIndex = 0;
			this.TransportProvidersAndOrgsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(650, 239, true);
			this.TransportProvidersAndOrgsTabControl.TabIndex = 12;
			this.TransportProvidersAndOrgsTabControl.TabStop = false;
			// 
			// TransportProviderTab
			//
			this.TransportProviderTab.Controls.Add(this.DocAddressControl);
			this.TransportProviderTab.Controls.Add(this.CarrierBookingAgentControl);
			this.TransportProviderTab.Controls.Add(this.TransportModeDropEdit);
			this.TransportProviderTab.Controls.Add(this.TransportReferenceTextBox);
			this.TransportProviderTab.Controls.Add(this.TransportRefLinkButton);
			this.TransportProviderTab.Controls.Add(this.CarrierAccountDropEdit);
			this.TransportProviderTab.Controls.Add(this.CarrierServiceLevelDropEdit);
			this.TransportProviderTab.Controls.Add(this.BookingRequestedDateEdit);
			this.TransportProviderTab.Controls.Add(this.TotalCO2e);
			this.TransportProviderTab.Controls.Add(this.TotalCO2eUnit);
			this.TransportProviderTab.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("976457ed-9875-41db-a22b-fa779431b8c0", "Transport Provider");
			this.TransportProviderTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
			this.TransportProviderTab.Name = "TransportProviderTab";
			this.TransportProviderTab.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.TransportProviderTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 233, true);
			this.TransportProviderTab.TabIndex = 13;
			// 
			// DocAddressControl
			// 
			this.DocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DocAddressControl, "Address");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).Address)));
			this.DocAddressControl.BindToOrganisations = "Lookups.LocalTransportOrganisations";
			this.DocAddressControl.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("8CE8BACC-95F5-44CD-A30D-D53671CED3A2", "Transport Company");
			this.DocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.HideOverrideShowTabs;
			this.DocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
			this.DocAddressControl.Name = "DocAddressControl";
			this.DocAddressControl.ReadOnly = false;
			this.DocAddressControl.TabIndex = 14;
			this.DocAddressControl.ValidationJustForced = false;
			// 
			// CarrierBookingAgentControl
			// 
			this.CarrierBookingAgentControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CarrierBookingAgentControl, "CarrierBookingAgentDocAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).CarrierBookingAgentDocAddress)));
			this.CarrierBookingAgentControl.BindToOrganisations = "Lookups.BindToLists.AllOrganisations";
			this.CarrierBookingAgentControl.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("8CE8BACC-95F5-44CD-A30D-D53671CED3A3", "Carrier Booking Agent");
			this.CarrierBookingAgentControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverride;
			this.CarrierBookingAgentControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(258, 4, true);
			this.CarrierBookingAgentControl.Name = "CarrierBookingAgentControl";
			this.CarrierBookingAgentControl.ReadOnly = false;
			this.CarrierBookingAgentControl.TabIndex = 15;
			this.CarrierBookingAgentControl.ValidationJustForced = false;
			// 
			// TransportModeDropEdit
			// 
			this.TransportModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportModeDropEdit, "KM_TransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).KM_TransportMode)));
			this.TransportModeDropEdit.CaptionResourceString = null;
			this.TransportModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(350, 55, true);
			this.TransportModeDropEdit.Name = "TransportModeDropEdit";
			this.TransportModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(185, 20, true);
			this.TransportModeDropEdit.TabIndex = 16;
			// 
			// TransportReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.TransportReferenceTextBox, "KM_TransportReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).KM_TransportReference)));
			this.TransportReferenceTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.TransportReferenceTextBox.CaptionResourceString = null;
			this.TransportReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(350, 77, true);
			this.TransportReferenceTextBox.Name = "TransportReferenceTextBox";
			this.TransportReferenceTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.TransportReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(255, 20, true);
			this.TransportReferenceTextBox.TabIndex = 17;
			// 
			// TransportRefLinkButton
			// 
			this.BindingSource.SetBindingMember(this.TransportRefLinkButton, ".");
			this.TransportRefLinkButton.BindToTransportCo = "Address.Organisation";
			this.TransportRefLinkButton.BindToTransportRef = "KM_TransportReference";
			this.TransportRefLinkButton.Image = ((System.Drawing.Image)(resources.GetObject("TransportRefLinkButton.Image")));
			this.TransportRefLinkButton.IsCaptionOverridden = false;
			this.TransportRefLinkButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(609, 77, true);
			this.TransportRefLinkButton.Name = "TransportRefLinkButton";
			this.TransportRefLinkButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.TransportRefLinkButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 22, true);
			this.TransportRefLinkButton.TabIndex = 18;
			this.TransportRefLinkButton.TabStop = false;
			this.TransportRefLinkButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.TransportRefLinkButton.ToolTipCaption = null;
			this.TransportRefLinkButton.UseVisualStyleBackColor = true;
			this.TransportRefLinkButton.Click += new System.EventHandler(this.TransportRefLinkButton_Click);
			// 
			// CarrierAccountDropEdit
			// 
			this.CarrierAccountDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CarrierAccountDropEdit, "KM_OAN_CarrierAccount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).KM_OAN_CarrierAccount)));
			this.CarrierAccountDropEdit.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("003e2d09-ce46-44d1-a3a2-3090a12bbb0b", "Carrier Account");
			this.CarrierAccountDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(350, 99, true);
			this.CarrierAccountDropEdit.Name = "CarrierAccountDropEdit";
			this.CarrierAccountDropEdit.PreBoundMaxLength = 3;
			this.CarrierAccountDropEdit.ShouldResizeByMaxLength = true;
			this.CarrierAccountDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(254, 20, true);
			this.CarrierAccountDropEdit.TabIndex = 19;
			// 
			// CarrierServiceLevelDropEdit
			// 
			this.CarrierServiceLevelDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CarrierServiceLevelDropEdit, "KM_PL_NKCarrierServiceLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).KM_PL_NKCarrierServiceLevel)));
			this.CarrierServiceLevelDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(350, 121, true);
			this.CarrierServiceLevelDropEdit.Name = "CarrierServiceLevelDropEdit";
			this.CarrierServiceLevelDropEdit.PreBoundMaxLength = 3;
			this.CarrierServiceLevelDropEdit.ShouldResizeByMaxLength = true;
			this.CarrierServiceLevelDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(185, 20, true);
			this.CarrierServiceLevelDropEdit.TabIndex = 20;
			// 
			// BookingRequestedDateEdit
			// 
			this.BookingRequestedDateEdit.AllowDrop = true;
			this.BookingRequestedDateEdit.AutoCompleteMonthThreshold = 1;
			this.BookingRequestedDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.BookingRequestedDateEdit, "KM_BookingOfTransportRequestedDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).KM_BookingOfTransportRequestedDate)));
			this.BookingRequestedDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.BookingRequestedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(350, 143, true);
			this.BookingRequestedDateEdit.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("6f009e00-92ee-4481-973c-d5082ccc8da9", "Requested Date");
			this.BookingRequestedDateEdit.Name = "BookingRequestedDateEdit";
			this.BookingRequestedDateEdit.TabIndex = 21;
			//
			// TotalCO2e
			//
			this.BindingSource.SetBindingMember(this.TotalCO2e, "TotalCO2eForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).TotalCO2eForBinding)));
			this.TotalCO2e.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("59a2db48-1fdf-46f3-a703-488491a1ff14", "CO2e");
			this.TotalCO2e.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(350, 165, true);
			this.TotalCO2e.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.TotalCO2e.Name = "TotalCO2e";
			this.TotalCO2e.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 20, true);
			this.TotalCO2e.TabIndex = 22;
			this.TotalCO2e.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TotalCO2e.Visible = false;
			//
			// TotalCO2eUnit
			//
			this.TotalCO2eUnit.AutoSize = true;
			this.TotalCO2eUnit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(424, 165, true);
			this.TotalCO2eUnit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.TotalCO2eUnit.Name = "TotalCO2eUnit";
			this.TotalCO2eUnit.Text = Enterprise.TransportBookings.GUI.Res.GetString("6175f069-9797-4a75-8fad-d633d7991ebd", "KG");
			this.TotalCO2eUnit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 13, true);
			this.TotalCO2eUnit.TabIndex = 23;
			this.TotalCO2e.Visible = false;
			// 
			// OrganisationsTab
			//
			this.OrganisationsTab.Controls.Add(this.BillingPartyOrClientPKAddressControl);
			this.OrganisationsTab.Controls.Add(this.BookingPartyDocumentaryAddressControl);
			this.OrganisationsTab.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("45050278-cf30-4a13-aafe-5fcb0e1a1b2d", "Organizations");
			this.OrganisationsTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
			this.OrganisationsTab.Name = "OrganisationsTab";
			this.OrganisationsTab.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.OrganisationsTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 233, true);
			this.OrganisationsTab.TabIndex = 24;
			// 
			// BillingPartyOrClientPKAddressControl
			// 
			this.BillingPartyOrClientPKAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BillingPartyOrClientPKAddressControl, "BillingPartyOrLocalClientPK_ZAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Business.ZAddress)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).BillingPartyOrLocalClientPK_ZAddress)));
			this.BillingPartyOrClientPKAddressControl.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("9D25474C-CC07-4BE1-A268-41E836EED973", "Billing Party");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.BillingPartyOrClientPKAddressControl, false);
			this.BillingPartyOrClientPKAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
			this.BillingPartyOrClientPKAddressControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 175, true);
			this.BillingPartyOrClientPKAddressControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 175, true);
			this.BillingPartyOrClientPKAddressControl.Name = "BillingPartyOrClientPKAddressControl";
			this.BillingPartyOrClientPKAddressControl.OnlyStopOnDebtor = false;
			this.BillingPartyOrClientPKAddressControl.PopupCaption = "";
			this.BillingPartyOrClientPKAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.BillingPartyOrClientPKAddressControl.TabIndex = 25;
			// 
			// BookingPartyDocumentaryAddressControl
			// 
			this.BookingPartyDocumentaryAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BookingPartyDocumentaryAddressControl, "ConsolidationSingleJob.BookedByAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).ConsolidationSingleJob.BookedByAddress)));
			this.BookingPartyDocumentaryAddressControl.BindToOrganisations = "Lookups.BindToLists.AllOrganisations";
			this.BookingPartyDocumentaryAddressControl.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("ee4fa8a9-22b0-4e4e-bc62-4cc60e5cc440", "Booking Party");
			this.BookingPartyDocumentaryAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.HideOverrideShowTabs;
			this.BookingPartyDocumentaryAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(258, 4, true);
			this.BookingPartyDocumentaryAddressControl.Name = "BookingPartyDocumentaryAddressControl";
			this.BookingPartyDocumentaryAddressControl.ReadOnly = false;
			this.BookingPartyDocumentaryAddressControl.SingleLineNoGroupBoxPanelWidth = 293;
			this.BookingPartyDocumentaryAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.BookingPartyDocumentaryAddressControl.TabIndex = 26;
			this.BookingPartyDocumentaryAddressControl.ValidationJustForced = false;
			//
			// AllocatedPackagesGroupBox
			//
			this.AllocatedPackagesGroupBox.Controls.Add(this.TotalPacksCalcEdit);
			this.AllocatedPackagesGroupBox.Controls.Add(this.TotalPacksUnitLabel);
			this.AllocatedPackagesGroupBox.Controls.Add(this.TotalWeightCalcEdit);
			this.AllocatedPackagesGroupBox.Controls.Add(this.TotalWeightUnitLabel);
			this.AllocatedPackagesGroupBox.Controls.Add(this.TotalVolumeCalcEdit);
			this.AllocatedPackagesGroupBox.Controls.Add(this.TotalVolumeUnitLabel);
			this.AllocatedPackagesGroupBox.Controls.Add(this.VolumeWeightCalcEdit);
			this.AllocatedPackagesGroupBox.Controls.Add(this.VolumeWeightUnitLabel);
			this.AllocatedPackagesGroupBox.Controls.Add(this.ChargeableWeightCalcEdit);
			this.AllocatedPackagesGroupBox.Controls.Add(this.ChargeableWeightUnitLabel);
			this.AllocatedPackagesGroupBox.Controls.Add(this.OverrideChargeableCheckBox);
			this.AllocatedPackagesGroupBox.Controls.Add(this.RequiresRefrigerationCheckBox);
			this.AllocatedPackagesGroupBox.Controls.Add(this.IsHazardousCheckBox);
			this.AllocatedPackagesGroupBox.Controls.Add(this.StatusTextBox);
			this.AllocatedPackagesGroupBox.Dock = System.Windows.Forms.DockStyle.None;
			this.AllocatedPackagesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(940, 17, true);
			this.AllocatedPackagesGroupBox.Name = "AllocatedPackagesGroupBox";
			this.AllocatedPackagesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 239, true);
			this.AllocatedPackagesGroupBox.TabIndex = 27;
			this.AllocatedPackagesGroupBox.TabStop = false;
			this.AllocatedPackagesGroupBox.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("2b8d4cd8-5bb8-484e-8e67-2bb7ef47b8ce", "Allocated Packages");
			// 
			// TotalPacksCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalPacksCalcEdit, "TotalPackages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).TotalPackages)));
			this.TotalPacksCalcEdit.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("66ab1982-2f23-4411-b4af-906471f5f439", "Packs", "Alloc. Packs", "Allocated Packs", "");
			this.TotalPacksCalcEdit.DecimalPlaces = 2;
			this.TotalPacksCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 19, true);
			this.TotalPacksCalcEdit.Name = "TotalPacksCalcEdit";
			this.TotalPacksCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.TotalPacksCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.TotalPacksCalcEdit.TabIndex = 28;
			this.TotalPacksCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalPacksUnitLabel
			// 
			this.BindingSource.SetBindingMember(this.TotalPacksUnitLabel, "TotalPackagesType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).TotalPackagesType)));
			this.TotalPacksUnitLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.TotalPacksUnitLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(220, 21, true);
			this.TotalPacksUnitLabel.Name = "TotalPacksUnitLabel";
			this.TotalPacksUnitLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.TotalPacksUnitLabel.TabIndex = 29;
			this.TotalPacksUnitLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// TotalWeightCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalWeightCalcEdit, "TotalWeightExcludingDuplicatePackages.Amount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).TotalWeightExcludingDuplicatePackages.Amount)));
			this.TotalWeightCalcEdit.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("4e57996d-5e65-4526-9b37-13fc08a87685", "Wgt.", "Weight", "Allocated Weight", "");
			this.TotalWeightCalcEdit.DecimalPlaces = 2;
			this.TotalWeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 41, true);
			this.TotalWeightCalcEdit.Name = "TotalWeightCalcEdit";
			this.TotalWeightCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.TotalWeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.TotalWeightCalcEdit.TabIndex = 30;
			this.TotalWeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalWeightUnitLabel
			// 
			this.BindingSource.SetBindingMember(this.TotalWeightUnitLabel, "TotalWeightExcludingDuplicatePackages.Unit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).TotalWeightExcludingDuplicatePackages.Unit)));
			this.TotalWeightUnitLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.TotalWeightUnitLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(220, 43, true);
			this.TotalWeightUnitLabel.Name = "TotalWeightUnitLabel";
			this.TotalWeightUnitLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.TotalWeightUnitLabel.TabIndex = 31;
			this.TotalWeightUnitLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// TotalVolumeCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalVolumeCalcEdit, "TotalVolumeExcludingDuplicatePackages.Amount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).TotalVolumeExcludingDuplicatePackages.Amount)));
			this.TotalVolumeCalcEdit.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("e3fd846a-8e95-4f8a-96aa-6664786165be", "Vol.", "Volume", "Allocated Volume", "");
			this.TotalVolumeCalcEdit.DecimalPlaces = 3;
			this.TotalVolumeCalcEdit.Decimals = 3;
			this.TotalVolumeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 63, true);
			this.TotalVolumeCalcEdit.Name = "TotalVolumeCalcEdit";
			this.TotalVolumeCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.TotalVolumeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.TotalVolumeCalcEdit.TabIndex = 32;
			this.TotalVolumeCalcEdit.Text = "0.000";
			this.TotalVolumeCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalVolumeUnitLabel
			// 
			this.BindingSource.SetBindingMember(this.TotalVolumeUnitLabel, "TotalVolumeExcludingDuplicatePackages.Unit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).TotalVolumeExcludingDuplicatePackages.Unit)));
			this.TotalVolumeUnitLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.TotalVolumeUnitLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(220, 65, true);
			this.TotalVolumeUnitLabel.Name = "TotalVolumeUnitLabel";
			this.TotalVolumeUnitLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.TotalVolumeUnitLabel.TabIndex = 33;
			this.TotalVolumeUnitLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// VolumeWeightCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.VolumeWeightCalcEdit, "KM_Calc_ActualVolumeWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).KM_Calc_ActualVolumeWeight)));
			this.VolumeWeightCalcEdit.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("5e096a7c-233c-4fec-84fb-cc5888807db5", "VW", "Vol. Weight", "Volume Weight", "");
			this.VolumeWeightCalcEdit.DecimalPlaces = 2;
			this.VolumeWeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 85, true);
			this.VolumeWeightCalcEdit.Name = "VolumeWeightCalcEdit";
			this.VolumeWeightCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.VolumeWeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.VolumeWeightCalcEdit.TabIndex = 34;
			this.VolumeWeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// VolumeWeightUnitLabel
			// 
			this.BindingSource.SetBindingMember(this.VolumeWeightUnitLabel, "KM_Calc_ActualVolumeWeightUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).KM_Calc_ActualVolumeWeightUnit)));
			this.VolumeWeightUnitLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.VolumeWeightUnitLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(220, 87, true);
			this.VolumeWeightUnitLabel.Name = "VolumeWeightUnitLabel";
			this.VolumeWeightUnitLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 20, true);
			this.VolumeWeightUnitLabel.TabIndex = 35;
			this.VolumeWeightUnitLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// ChargeableWeightCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ChargeableWeightCalcEdit, "KM_Calc_RoundedChargeable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).KM_Calc_RoundedChargeable)));
			this.ChargeableWeightCalcEdit.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("57407c02-8c5d-4099-bb59-ebab4668b9bb", "Chargeable", "Chargeable Wgt.", "Chargeable Weight", "");
			this.ChargeableWeightCalcEdit.DecimalPlaces = 2;
			this.ChargeableWeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 107, true);
			this.ChargeableWeightCalcEdit.Name = "ChargeableWeightCalcEdit";
			this.ChargeableWeightCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.ChargeableWeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.ChargeableWeightCalcEdit.TabIndex = 36;
			this.ChargeableWeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ChargeableWeightUnitLabel
			// 
			this.BindingSource.SetBindingMember(this.ChargeableWeightUnitLabel, "ChargeableUnits");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).ChargeableUnits)));
			this.ChargeableWeightUnitLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ChargeableWeightUnitLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(220, 109, true);
			this.ChargeableWeightUnitLabel.Name = "ChargeableWeightUnitLabel";
			this.ChargeableWeightUnitLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 20, true);
			this.ChargeableWeightUnitLabel.TabIndex = 37;
			this.ChargeableWeightUnitLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// OverrideChargeableCheckBox
			// 
			this.BindingSource.SetBindingMember(this.OverrideChargeableCheckBox, "KM_OverrideChargeable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).KM_OverrideChargeable)));
			this.OverrideChargeableCheckBox.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("37313301-12dc-4682-bd01-13ffab204788", "Override Chargeable");
			this.OverrideChargeableCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OverrideChargeableCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 129, true);
			this.OverrideChargeableCheckBox.Name = "OverrideChargeableCheckBox";
			this.OverrideChargeableCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 20, true);
			this.OverrideChargeableCheckBox.TabIndex = 38;
			this.OverrideChargeableCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OverrideChargeableCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.OverrideChargeableCheckBox.UseVisualStyleBackColor = true;
			// 
			// RequiresRefrigerationCheckBox
			// 
			this.BindingSource.SetBindingMember(this.RequiresRefrigerationCheckBox, "KM_RequiresRefrigeration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).KM_RequiresRefrigeration)));
			this.RequiresRefrigerationCheckBox.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("b83a0962-f8c2-461c-b02b-ec409058ca34", "Refrigeration");
			this.RequiresRefrigerationCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.RequiresRefrigerationCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RequiresRefrigerationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 151, true);
			this.RequiresRefrigerationCheckBox.Name = "RequiresRefrigerationCheckBox";
			this.RequiresRefrigerationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 20, true);
			this.RequiresRefrigerationCheckBox.TabIndex = 39;
			this.RequiresRefrigerationCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.RequiresRefrigerationCheckBox.UseVisualStyleBackColor = true;
			// 
			// IsHazardousCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsHazardousCheckBox, "KM_IsHazardous");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).KM_IsHazardous)));
			this.IsHazardousCheckBox.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("f6f34d49-d00b-4ca9-b77e-b49e65af9493", "Hazardous");
			this.IsHazardousCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsHazardousCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsHazardousCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 151, true);
			this.IsHazardousCheckBox.Name = "IsHazardousCheckBox";
			this.IsHazardousCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.IsHazardousCheckBox.TabIndex = 40;
			this.IsHazardousCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.IsHazardousCheckBox.UseVisualStyleBackColor = true;
			// 
			// StatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.StatusTextBox, "StatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).StatusDescription)));
			this.StatusTextBox.CaptionResourceString = null;
			this.StatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(55, 195, true);
			this.StatusTextBox.Name = "StatusTextBox";
			this.StatusTextBox.ReadOnly = true;
			this.StatusTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.StatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.StatusTextBox.TabIndex = 41;
			// 
			// ParentLinkLabel
			// 
			this.ParentLinkLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ParentLinkLabel, "ConsolidationSingleJob+ParentJobDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).ConsolidationSingleJob.ParentJobDescription)));
			this.ParentLinkLabel.IsFontBold = false;
			this.ParentLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 0, true);
			this.ParentLinkLabel.Name = "ParentLinkLabel";
			this.ParentLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.ParentLinkLabel.TabIndex = 1;
			this.ParentLinkLabel.TabStop = false;
			this.ParentLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.parentLinkLabel_LinkClicked);
			// 
			// OverrideParentCheckBox
			// 
			this.BindingSource.SetBindingMember(this.OverrideParentCheckBox, "ConsolidationSingleJob.KB_IsOverridden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.TransportBookings.Business.DtbBooking)(null)).ConsolidationSingleJob.KB_IsOverridden)));
			this.OverrideParentCheckBox.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("aea78d8f-fa80-44ec-bc94-de4200528d0b", "Override?");
			this.OverrideParentCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OverrideParentCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.OverrideParentCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(153, -1, true);
			this.OverrideParentCheckBox.Name = "OverrideParentCheckBox";
			this.OverrideParentCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 16, true);
			this.OverrideParentCheckBox.TabIndex = 2;
			this.OverrideParentCheckBox.UseVisualStyleBackColor = true;
			// 
			// BookingForGroupBox
			//
			this.BookingForGroupBox.Controls.Add(this.JobDetailsGroupBox);
			this.BookingForGroupBox.Controls.Add(this.TransportProvidersAndOrgsTabControl);
			this.BookingForGroupBox.Controls.Add(this.AllocatedPackagesGroupBox);
			this.BookingForGroupBox.Controls.Add(this.OverrideParentCheckBox);
			this.BookingForGroupBox.Controls.Add(this.ParentLinkLabel);
			this.BookingForGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BookingForGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.BookingForGroupBox.Name = "BookingForGroupBox";
			this.BookingForGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1354, 261, true);
			this.BookingForGroupBox.TabIndex = 0;
			this.BookingForGroupBox.TabStop = false;
			// 
			// BookingControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.BookingForGroupBox);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1360, 263, true);
			this.Name = "BookingControl";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1360, 263, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.JobDetailsGroupBox.ResumeLayout(false);
			this.JobDetailsGroupBox.PerformLayout();
			this.TransportProvidersAndOrgsTabControl.ResumeLayout(false);
			this.AllocatedPackagesGroupBox.ResumeLayout(false);
			this.AllocatedPackagesGroupBox.PerformLayout();
			this.TransportProviderTab.ResumeLayout(false);
			this.TransportProviderTab.PerformLayout();
			this.OrganisationsTab.ResumeLayout(false);
			this.OrganisationsTab.PerformLayout();
			this.TemplateDropEdit.ResumeLayout(true);
			this.TemplateDropEdit.PerformLayout();
			this.DocAddressControl.ResumeLayout(true);
			this.DocAddressControl.PerformLayout();
			this.BookingForGroupBox.ResumeLayout(false);
			this.BookingForGroupBox.PerformLayout();
			this.BranchGuidFindBox.ResumeLayout(true);
			this.BranchGuidFindBox.PerformLayout();
			this.CarrierAccountDropEdit.ResumeLayout(true);
			this.CarrierAccountDropEdit.PerformLayout();
			this.BillingPartyOrClientPKAddressControl.ResumeLayout(true);
			this.BillingPartyOrClientPKAddressControl.PerformLayout();
			this.BookingPartyDocumentaryAddressControl.ResumeLayout(true);
			this.BookingPartyDocumentaryAddressControl.PerformLayout();
			this.RatingFreightModeDropEdit.ResumeLayout(true);
			this.RatingFreightModeDropEdit.PerformLayout();
			this.ServiceLevelDropEdit.ResumeLayout(true);
			this.ServiceLevelDropEdit.PerformLayout();
			this.CarrierServiceLevelDropEdit.ResumeLayout(true);
			this.CarrierServiceLevelDropEdit.PerformLayout();
			this.BookingRequestedDateEdit.ResumeLayout(true);
			this.BookingRequestedDateEdit.PerformLayout();
			this.CarrierBookingAgentControl.ResumeLayout(true);
			this.CarrierBookingAgentControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		void TransportRefLinkButton_Click(object sender, EventArgs e)
		{
			// quick fix for test that complains about no click event. test will be fixed and this removed.
		}

		#endregion

		ZArchitecture.GUI.ZDropEdit TemplateDropEdit;
		ZArchitecture.ZTextBox WaybillTextBox;
		ZArchitecture.ZTextBox ConsolIDTextBox;
		ZArchitecture.ZTextBox TransportReferenceTextBox;
		Enterprise.MasterFiles.GUI.ZButtonTransportCoHotlink TransportRefLinkButton;
		ZArchitecture.ZTextBox GoodsDescriptionTextBox;
		MasterFiles.GUI.ZDocAddressControl DocAddressControl;
		ZArchitecture.ZTextBox StatusTextBox;
		ZLinkLabel ParentLinkLabel;
		ZArchitecture.GUI.ZCheckBox OverrideParentCheckBox;
		ZArchitecture.GUI.ZGroupBox BookingForGroupBox;
		ZDropEdit RatingFreightModeDropEdit;
		ZDropEdit ServiceLevelDropEdit;
		ZDropEdit CarrierServiceLevelDropEdit;
		ZGuidDropEdit CarrierAccountDropEdit;
		ZDateEdit BookingRequestedDateEdit;
		private ZCheckBox RequiresRefrigerationCheckBox;
		private ZCheckBox IsHazardousCheckBox;
		private MasterFiles.GUI.ZDocAddressControl BookingPartyDocumentaryAddressControl;
		private ZOrgAddressControl BillingPartyOrClientPKAddressControl;
		private ZGuidFindBox BranchGuidFindBox;
		private MasterFiles.GUI.ZDocAddressControl CarrierBookingAgentControl;
		private ZArchitecture.ZCalcEdit TotalWeightCalcEdit;
		private ZArchitecture.ZCalcEdit TotalVolumeCalcEdit;
		private ZArchitecture.ZCalcEdit ChargeableWeightCalcEdit;
		private ZArchitecture.ZCalcEdit TotalPacksCalcEdit;
		private ZArchitecture.ZCalcEdit VolumeWeightCalcEdit;
		private ZArchitecture.ZLabel ChargeableWeightUnitLabel;
		private ZArchitecture.ZLabel VolumeWeightUnitLabel;
		private ZArchitecture.ZLabel TotalVolumeUnitLabel;
		private ZArchitecture.ZLabel TotalWeightUnitLabel;
		private ZArchitecture.ZLabel TotalPacksUnitLabel;
		private ZCheckBox OverrideChargeableCheckBox;
		ZArchitecture.ZTextBox MasterTBTextBox;
		ZTextBox TotalCO2e;
		ZLabel TotalCO2eUnit;
		ZArchitecture.GUI.ZGroupBox JobDetailsGroupBox;
		protected ZArchitecture.GUI.ZTemplateTabControl TransportProvidersAndOrgsTabControl;
		ZArchitecture.GUI.ZGroupBox AllocatedPackagesGroupBox;
		private ZArchitecture.GUI.ZTabPage TransportProviderTab;
#if DEBUG
		internal
#endif 
		ZArchitecture.GUI.ZTabPage OrganisationsTab;
		ZArchitecture.GUI.ZDropEdit TransportModeDropEdit;
	}
}
