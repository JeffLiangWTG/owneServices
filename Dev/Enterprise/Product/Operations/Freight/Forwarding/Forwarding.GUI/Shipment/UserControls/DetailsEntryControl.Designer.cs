using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Layout;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class DetailsEntryControl : ZUserControl
	{
		RowLayoutPanel DetailsPanel;
		ZTextBox CustomsEntryNumber;
		ZDropEdit CustomsEntryNumberTypeBoundDropEdit;
		protected ZLinkLabel CustomEntriesLinkLabel;
		ZCheckBox IsHighRisk;
		ZDropEdit AviationSecurity;
		ZDropEdit AdditionalInspectionType;
		ZCodeFindBox ServiceLevel;
		ZDropEdit ExportStatement;
		ZLabel JS_PaymentTermLabel;
		ZDropEdit PaymentTerm;
		ZDropEdit ChargesApply;
		ZCalcEdit JS_NoCopyBillsCalcEdit;
		ZDateEdit BillDetails;
		ZCalcEdit JS_NoOriginalBillsCalcEdit;
		ZButton IncoTermExplainButton;
		ZStmNotePopupEditWithBindableText MarksNumbers;
		ZDropEdit ContainerModeOverride;
		GoodsDescriptionTextBox Description;
		ZCalcFindBox JS_InsuranceBoundCurrencyControl;
		ZCalcDropEdit PacksValues;
		ZDateEdit JS_ShippedOnBoardDateEdit;
		ZDropEdit OnBoard;
		ZCalcDropEdit WeightVolumeChargeable;
		ZCalcEdit JS_Calc_ActualVolumeWeight;
		ZLabel JS_Calc_ActualVolumeWeightUnit;
		ZDropEdit HousebillType;
		ZCalcDropEdit JS_TotalPackageCountBoundDropCalcEdit;
		ZCodeFindBox JS_RL_NKDestinationCodeFindBox;
		ZCodeFindBox OriginDestinationDates;
		ZDropEdit AirwayBillDims;
		ZDropEdit ReleaseType;
		protected ZButton PiecesDetailButton;
		ZCalcDropEdit JS_ActualVolumeBoundCalcDropEdit;
		ZCalcFindBox JS_GoodsValueBoundCurrencyControl;
		ZDateEdit JS_E_ARVBoundDateEdit;
		ZLabel ChargeableUnitLabel;
		ZCheckBox DomesticCheckBox;
		ZTextBox HouseBill;
		ZCalcEdit Chargeable;
		ZDateEdit JS_E_DEPBoundDateEdit;
		CustomsEntryIssueAndExpiryDateControl customsEntryAdditionalInfoControl;
		ZDropEdit CODType;
		ZCalcEdit ShipperCOD;
		protected DeniedPartyScreening.GUI.DeniedPartyScreeningStatusDropEdit ScreeningStatus;
		private ZButton ScreenButton;
		private ZTextBox AdditionalTerms;
		private ZDropEdit Phase;
		private ZDropEdit ISFBillStatus;
		private ZCalcEdit LoadingMeters;
		ZGroupBox DetailsPanelGroupBox;
		private ZDropEdit CommunityTransitStatus;
		ZDateEdit EstExportCustomsClearDate;
		private FreightRateControl FreightSpotRate;
		ZLabel EstExportCustomsClearLabel;
		ZDropEdit EFreightStatus;
		ZDocAddressControl ControllingCustomerAddressControl;
		ZDocAddressControl ControllingAgentAddressControl;
		ZDateEdit OrderUpdateCutOff;
		ZTextBox TotalCO2e;
		ZLabel TotalCO2eUnit;
		ZCalcFindBox DestinationGoodsValueCalcFindBox;
		ZCalcEdit DestinationExchangeRateCalcEdit;

		void InitializeComponent()
		{
			this.DetailsPanelGroupBox = new ZGroupBox();
			this.DetailsPanel = new RowLayoutPanel();
			this.EstExportCustomsClearLabel = new ZLabel();
			this.EstExportCustomsClearDate = new ZDateEdit();
			this.CommunityTransitStatus = new ZDropEdit();
			this.LoadingMeters = new ZCalcEdit();
			this.ISFBillStatus = new ZDropEdit();
			this.Phase = new ZDropEdit();
			this.AdditionalTerms = new ZTextBox();
			this.ScreenButton = new ZButton();
			this.customsEntryAdditionalInfoControl = new CustomsEntryIssueAndExpiryDateControl();
			this.ScreeningStatus = new DeniedPartyScreening.GUI.DeniedPartyScreeningStatusDropEdit();
			this.CustomsEntryNumber = new ZTextBox();
			this.CODType = new ZDropEdit();
			this.CustomsEntryNumberTypeBoundDropEdit = new ZDropEdit();
			this.CustomEntriesLinkLabel = new ZLinkLabel();
			this.IsHighRisk = new ZCheckBox();
			this.AviationSecurity = new ZDropEdit();
			this.AdditionalInspectionType = new ZDropEdit();
			this.ShipperCOD = new ZCalcEdit();
			this.ServiceLevel = new ZCodeFindBox();
			this.ExportStatement = new ZDropEdit();
			this.JS_PaymentTermLabel = new ZLabel();
			this.ChargesApply = new ZDropEdit();
			this.PaymentTerm = new ZDropEdit();
			this.JS_NoCopyBillsCalcEdit = new ZCalcEdit();
			this.JS_NoOriginalBillsCalcEdit = new ZCalcEdit();
			this.BillDetails = new ZDateEdit();
			this.IncoTermExplainButton = new ZButton();
			this.MarksNumbers = new ZStmNotePopupEditWithBindableText();
			this.ContainerModeOverride = new ZDropEdit();
			this.Description = new GoodsDescriptionTextBox();
			this.JS_InsuranceBoundCurrencyControl = new ZCalcFindBox();
			this.JS_ShippedOnBoardDateEdit = new ZDateEdit();
			this.OnBoard = new ZDropEdit();
			this.HousebillType = new ZDropEdit();
			this.JS_TotalPackageCountBoundDropCalcEdit = new ZCalcDropEdit();
			this.JS_RL_NKDestinationCodeFindBox = new ZCodeFindBox();
			this.AirwayBillDims = new ZDropEdit();
			this.PacksValues = new ZCalcDropEdit();
			this.ReleaseType = new ZDropEdit();
			this.PiecesDetailButton = new ZButton();
			this.JS_ActualVolumeBoundCalcDropEdit = new ZCalcDropEdit();
			this.JS_GoodsValueBoundCurrencyControl = new ZCalcFindBox();
			this.JS_E_ARVBoundDateEdit = new ZDateEdit();
			this.ChargeableUnitLabel = new ZLabel();
			this.JS_Calc_ActualVolumeWeightUnit = new ZLabel();
			this.DomesticCheckBox = new ZCheckBox();
			this.Chargeable = new ZCalcEdit();
			this.HouseBill = new ZTextBox();
			this.WeightVolumeChargeable = new ZCalcDropEdit();
			this.JS_Calc_ActualVolumeWeight = new ZCalcEdit();
			this.JS_E_DEPBoundDateEdit = new ZDateEdit();
			this.OriginDestinationDates = new ZCodeFindBox();
			this.FreightSpotRate = new FreightRateControl();
			this.EFreightStatus = new ZDropEdit();
			this.ControllingCustomerAddressControl = new ZDocAddressControl();
			this.ControllingAgentAddressControl = new ZDocAddressControl();
			this.OrderUpdateCutOff = new ZDateEdit();
			this.TotalCO2e = new ZTextBox();
			this.TotalCO2eUnit = new ZLabel();
			this.DestinationGoodsValueCalcFindBox = new ZCalcFindBox();
			this.DestinationExchangeRateCalcEdit = new ZCalcEdit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DetailsPanelGroupBox.SuspendLayout();
			this.DetailsPanel.SuspendLayout();
			this.SuspendLayout();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(ForwardingShipment);
			//
			// DetailsPanelGroupBox
			//
			this.DetailsPanelGroupBox.AutoSize = true;
			this.DetailsPanelGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.DetailsPanelGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DetailsEntryControl|fd356657-3eb3-42df-93b7-ebfe520ac738", "Details");
			this.DetailsPanelGroupBox.Controls.Add(this.DetailsPanel);
			this.DetailsPanelGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.DetailsPanelGroupBox, true);
			this.DetailsPanelGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsPanelGroupBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.DetailsPanelGroupBox.Name = "DetailsPanelGroupBox";
			this.DetailsPanelGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(366, 866, true);
			this.DetailsPanelGroupBox.TabIndex = 0;
			this.DetailsPanelGroupBox.TabStop = false;
			//
			// DetailsPanel
			//
			this.DetailsPanel.AutoSize = true;
			this.DetailsPanel.Controls.Add(this.FreightSpotRate);
			this.DetailsPanel.Controls.Add(this.EstExportCustomsClearLabel);
			this.DetailsPanel.Controls.Add(this.EstExportCustomsClearDate);
			this.DetailsPanel.Controls.Add(this.CommunityTransitStatus);
			this.DetailsPanel.Controls.Add(this.LoadingMeters);
			this.DetailsPanel.Controls.Add(this.ISFBillStatus);
			this.DetailsPanel.Controls.Add(this.Phase);
			this.DetailsPanel.Controls.Add(this.AdditionalTerms);
			this.DetailsPanel.Controls.Add(this.ScreenButton);
			this.DetailsPanel.Controls.Add(this.customsEntryAdditionalInfoControl);
			this.DetailsPanel.Controls.Add(this.ScreeningStatus);
			this.DetailsPanel.Controls.Add(this.CustomsEntryNumber);
			this.DetailsPanel.Controls.Add(this.CODType);
			this.DetailsPanel.Controls.Add(this.CustomsEntryNumberTypeBoundDropEdit);
			this.DetailsPanel.Controls.Add(this.CustomEntriesLinkLabel);
			this.DetailsPanel.Controls.Add(this.IsHighRisk);
			this.DetailsPanel.Controls.Add(this.AviationSecurity);
			this.DetailsPanel.Controls.Add(this.AdditionalInspectionType);
			this.DetailsPanel.Controls.Add(this.ShipperCOD);
			this.DetailsPanel.Controls.Add(this.ServiceLevel);
			this.DetailsPanel.Controls.Add(this.ExportStatement);
			this.DetailsPanel.Controls.Add(this.JS_PaymentTermLabel);
			this.DetailsPanel.Controls.Add(this.ChargesApply);
			this.DetailsPanel.Controls.Add(this.PaymentTerm);
			this.DetailsPanel.Controls.Add(this.JS_NoCopyBillsCalcEdit);
			this.DetailsPanel.Controls.Add(this.JS_NoOriginalBillsCalcEdit);
			this.DetailsPanel.Controls.Add(this.BillDetails);
			this.DetailsPanel.Controls.Add(this.IncoTermExplainButton);
			this.DetailsPanel.Controls.Add(this.MarksNumbers);
			this.DetailsPanel.Controls.Add(this.ContainerModeOverride);
			this.DetailsPanel.Controls.Add(this.Description);
			this.DetailsPanel.Controls.Add(this.JS_InsuranceBoundCurrencyControl);
			this.DetailsPanel.Controls.Add(this.JS_ShippedOnBoardDateEdit);
			this.DetailsPanel.Controls.Add(this.OnBoard);
			this.DetailsPanel.Controls.Add(this.HousebillType);
			this.DetailsPanel.Controls.Add(this.JS_TotalPackageCountBoundDropCalcEdit);
			this.DetailsPanel.Controls.Add(this.JS_RL_NKDestinationCodeFindBox);
			this.DetailsPanel.Controls.Add(this.AirwayBillDims);
			this.DetailsPanel.Controls.Add(this.PacksValues);
			this.DetailsPanel.Controls.Add(this.ReleaseType);
			this.DetailsPanel.Controls.Add(this.PiecesDetailButton);
			this.DetailsPanel.Controls.Add(this.JS_ActualVolumeBoundCalcDropEdit);
			this.DetailsPanel.Controls.Add(this.JS_GoodsValueBoundCurrencyControl);
			this.DetailsPanel.Controls.Add(this.JS_E_ARVBoundDateEdit);
			this.DetailsPanel.Controls.Add(this.ChargeableUnitLabel);
			this.DetailsPanel.Controls.Add(this.JS_Calc_ActualVolumeWeightUnit);
			this.DetailsPanel.Controls.Add(this.DomesticCheckBox);
			this.DetailsPanel.Controls.Add(this.Chargeable);
			this.DetailsPanel.Controls.Add(this.HouseBill);
			this.DetailsPanel.Controls.Add(this.WeightVolumeChargeable);
			this.DetailsPanel.Controls.Add(this.JS_Calc_ActualVolumeWeight);
			this.DetailsPanel.Controls.Add(this.JS_E_DEPBoundDateEdit);
			this.DetailsPanel.Controls.Add(this.OriginDestinationDates);
			this.DetailsPanel.Controls.Add(this.EFreightStatus);
			this.DetailsPanel.Controls.Add(this.ControllingAgentAddressControl);
			this.DetailsPanel.Controls.Add(this.ControllingCustomerAddressControl);
			this.DetailsPanel.Controls.Add(this.OrderUpdateCutOff);
			this.DetailsPanel.Controls.Add(this.TotalCO2e);
			this.DetailsPanel.Controls.Add(this.TotalCO2eUnit);
			this.DetailsPanel.Controls.Add(this.DestinationGoodsValueCalcFindBox);
			this.DetailsPanel.Controls.Add(this.DestinationExchangeRateCalcEdit);
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.DetailsPanel, true);
			this.DetailsPanel.FixedRows = true;
			this.DetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 16, true);
			this.DetailsPanel.Name = "DetailsPanel";
			this.DetailsPanel.RowHeight = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(21);
			this.DetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(269, 836, true);
			this.DetailsPanel.TabIndex = 106;
			//
			// EstExportCustomsClearLabel
			//
			this.EstExportCustomsClearLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DetailsEntryControl|36F511E1-A608-4526-94F2-E626E8BE47E2", "Estimated Export Customs Clearance Date");
			this.EstExportCustomsClearLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 693, true);
			this.EstExportCustomsClearLabel.Name = "EstExportCustomsClearLabel";
			this.DetailsPanel.SetRow(this.EstExportCustomsClearLabel, 33);
			this.EstExportCustomsClearLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 21, true);
			this.EstExportCustomsClearLabel.TabIndex = 51;
			//
			// EstExportCustomsClearDate
			//
			this.EstExportCustomsClearDate.AllowDrop = true;
			this.EstExportCustomsClearDate.AutoCompleteMonthThreshold = 1;
			this.EstExportCustomsClearDate.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.EstExportCustomsClearDate, "JS_Calc_EstimatedExportClearanceDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((ForwardingShipment)(null)).JS_Calc_EstimatedExportClearanceDate)));
			this.EstExportCustomsClearDate.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.EstExportCustomsClearDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 714, true);
			this.EstExportCustomsClearDate.Name = "EstExportCustomsClearDate";
			this.DetailsPanel.SetRow(this.EstExportCustomsClearDate, 34);
			this.EstExportCustomsClearDate.TabIndex = 52;
			//
			// CommunityTransitStatus
			//
			this.CommunityTransitStatus.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CommunityTransitStatus, "JS_CommunityTransitStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((ForwardingShipment)(null)).JS_CommunityTransitStatus)));
			this.CommunityTransitStatus.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("20561ba0-ddac-413a-a052-a9c1eeca5004", "CTS", "Community Transit", "CT Status", "Community/Common Transit Status Code");
			this.CommunityTransitStatus.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 672, true);
			this.CommunityTransitStatus.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.CommunityTransitStatus.Name = "CommunityTransitStatus";
			this.DetailsPanel.SetRow(this.CommunityTransitStatus, 32);
			this.CommunityTransitStatus.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(263, 20, true);
			this.CommunityTransitStatus.TabIndex = 50;
			//
			// LoadingMeters
			//
			this.BindingSource.SetBindingMember(this.LoadingMeters, "JS_LoadingMeters");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((ForwardingShipment)(null)).JS_LoadingMeters)));
			this.LoadingMeters.DecimalPlaces = 3;
			this.LoadingMeters.Decimals = 3;
			this.LoadingMeters.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 588, true);
			this.LoadingMeters.Name = "LoadingMeters";
			this.DetailsPanel.SetRow(this.LoadingMeters, 30);
			this.LoadingMeters.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 20, true);
			this.LoadingMeters.TabIndex = 49;
			this.LoadingMeters.Text = "0.000";
			this.LoadingMeters.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// ISFBillStatus
			//
			this.ISFBillStatus.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ISFBillStatus, "ISFBillStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((ForwardingShipment)(null)).ISFBillStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ForwardingShipment)(null)).ISFBillStatusDescription)));
			this.ISFBillStatus.BindToForDescription = "ISFBillStatusDescription";
			this.ISFBillStatus.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DetailsEntryControl|3857A972-49FA-4B32-B507-3E73077E0D4A", "ISF Bill Status", "ISF Bill Status", "ISF Bill Status", "Importer Security Filing Bill Status");
			this.ISFBillStatus.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 567, true);
			this.ISFBillStatus.Name = "ISFBillStatus";
			this.ISFBillStatus.PreBoundMaxLength = 3;
			this.DetailsPanel.SetRow(this.ISFBillStatus, 29);
			this.ISFBillStatus.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(263, 20, true);
			this.ISFBillStatus.TabIndex = 48;
			//
			// Phase
			//
			this.Phase.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.Phase, "JS_Phase");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((ForwardingShipment)(null)).JS_Phase)));
			this.Phase.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 546, true);
			this.Phase.Name = "Phase";
			this.Phase.PreBoundMaxLength = 3;
			this.DetailsPanel.SetRow(this.Phase, 28);
			this.Phase.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 20, true);
			this.Phase.TabIndex = 47;
			//
			// AdditionalTerms
			//
			this.BindingSource.SetBindingMember(this.AdditionalTerms, "JS_AdditionalTerms");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ForwardingShipment)(null)).JS_AdditionalTerms)));
			this.AdditionalTerms.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AdditionalTerms.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 525, true);
			this.AdditionalTerms.Name = "AdditionalTerms";
			this.DetailsPanel.SetRow(this.AdditionalTerms, 27);
			this.AdditionalTerms.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 20, true);
			this.AdditionalTerms.TabIndex = 46;
			//
			// ScreenButton
			//
			this.ScreenButton.Font = new Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ScreenButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(154, 504, true);
			this.ScreenButton.Name = "ScreenButton";
			this.DetailsPanel.SetRow(this.ScreenButton, 26);
			this.ScreenButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 20, true);
			this.ScreenButton.TabIndex = 45;
			this.ScreenButton.Text = "...";
			this.ScreenButton.UseVisualStyleBackColor = false;
			this.ScreenButton.Click += new EventHandler(this.ScreenButton_Click);
			//
			// customsEntryAdditionalInfoControl
			//
			this.customsEntryAdditionalInfoControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.customsEntryAdditionalInfoControl, ".");
			this.customsEntryAdditionalInfoControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 273, true);
			this.customsEntryAdditionalInfoControl.Name = "customsEntryAdditionalInfoControl";
			this.DetailsPanel.SetRow(this.customsEntryAdditionalInfoControl, 13);
			this.customsEntryAdditionalInfoControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 20, true);
			this.customsEntryAdditionalInfoControl.TabIndex = 27;
			this.customsEntryAdditionalInfoControl.CaptionRenderingEnabled = true;
			//
			// ScreeningStatus
			//
			this.ScreeningStatus.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ScreeningStatus, "JS_ScreeningStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((ForwardingShipment)(null)).JS_ScreeningStatus)));
			this.ScreeningStatus.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 504, true);
			this.ScreeningStatus.Name = "ScreeningStatus";
			this.ScreeningStatus.PreBoundMaxLength = 3;
			this.DetailsPanel.SetRow(this.ScreeningStatus, 26);
			this.ScreeningStatus.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(153, 20, true);
			this.ScreeningStatus.TabIndex = 44;
			//
			// CustomsEntryNumber
			//
			this.CustomsEntryNumber.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CustomsEntryNumber, "CustomsEntryNumberForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ForwardingShipment)(null)).CustomsEntryNumber)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CustomsEntryNumber, false);
			this.CustomsEntryNumber.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(65, 253, true);
			this.CustomsEntryNumber.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.CustomsEntryNumber.Name = "CustomsEntryNumber";
			this.DetailsPanel.SetRow(this.CustomsEntryNumber, 12);
			this.CustomsEntryNumber.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.CustomsEntryNumber.TabIndex = 25;
			//
			// CODType
			//
			this.CODType.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CODType, "JS_ShipperCODPayMethod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((ForwardingShipment)(null)).JS_ShipperCODPayMethod)));
			this.CODType.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 483, true);
			this.CODType.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.CODType.Name = "CODType";
			this.CODType.PreBoundMaxLength = 3;
			this.DetailsPanel.SetRow(this.CODType, 25);
			this.CODType.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 20, true);
			this.CODType.TabIndex = 43;
			//
			// CustomsEntryNumberTypeBoundDropEdit
			//
			this.CustomsEntryNumberTypeBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsEntryNumberTypeBoundDropEdit, "CustomsEntryNumberType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((ForwardingShipment)(null)).CustomsEntryNumberType)));
			this.CustomsEntryNumberTypeBoundDropEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DetailsEntryControl|b98c2851-7b91-4cd5-b4d7-616e312d6e4f", "Entry", "Entry Details", "The Customs Entry Type and Number for this shipment.");
			this.CustomsEntryNumberTypeBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 252, true);
			this.CustomsEntryNumberTypeBoundDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.CustomsEntryNumberTypeBoundDropEdit.Name = "CustomsEntryNumberTypeBoundDropEdit";
			this.CustomsEntryNumberTypeBoundDropEdit.PreBoundMaxLength = 3;
			this.DetailsPanel.SetRow(this.CustomsEntryNumberTypeBoundDropEdit, 12);
			this.CustomsEntryNumberTypeBoundDropEdit.ShowDescriptionBox = false;
			this.CustomsEntryNumberTypeBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.CustomsEntryNumberTypeBoundDropEdit.TabIndex = 24;
			//
			// CustomEntriesLinkLabel
			//
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.CustomEntriesLinkLabel.AutoSize = true;
			this.CustomEntriesLinkLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("c8896650-9130-4547-b4bc-2911f1ea9745", "Customs Entries");
			this.CustomEntriesLinkLabel.IsFontBold = false;
			this.CustomEntriesLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(185, 252, true);
			this.CustomEntriesLinkLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.CustomEntriesLinkLabel.Name = "CustomsEntriesLinkLabel";
			this.DetailsPanel.SetRow(this.CustomEntriesLinkLabel, 12);
			this.CustomEntriesLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 17, true);
			this.CustomEntriesLinkLabel.TabIndex = 26;
			this.CustomEntriesLinkLabel.Visible = false;
			//
			// IsHighRisk
			//
			this.IsHighRisk.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsHighRisk, "JS_IsHighRisk");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((ForwardingShipment)(null)).JS_IsHighRisk)));
			this.IsHighRisk.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsHighRisk.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 296, true);
			this.IsHighRisk.Name = "IsHighRisk";
			this.DetailsPanel.SetRow(this.IsHighRisk, 15);
			this.IsHighRisk.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 17, true);
			this.IsHighRisk.TabIndex = 28;
			this.IsHighRisk.Visible = false;
			//
			// AviationSecurity
			//
			this.AviationSecurity.AllowDrop = true;
			this.AviationSecurity.Anchor = ((AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AviationSecurity, "JS_InspectionTypeCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((ForwardingShipment)(null)).JS_InspectionTypeCode)));
			this.AviationSecurity.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 294, true);
			this.AviationSecurity.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.AviationSecurity.Name = "AviationSecurity";
			this.AviationSecurity.PreBoundMaxLength = 3;
			this.DetailsPanel.SetRow(this.AviationSecurity, 14);
			this.AviationSecurity.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 20, true);
			this.AviationSecurity.TabIndex = 29;
			//
			// AdditionalInspectionType
			//
			this.AdditionalInspectionType.AllowDrop = true;
			this.AdditionalInspectionType.Anchor = ((AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
				| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AdditionalInspectionType, "JS_AdditionalInspectionTypeCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((ForwardingShipment)(null)).JS_AdditionalInspectionTypeCode)));
			this.AdditionalInspectionType.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 336, true);
			this.AdditionalInspectionType.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.AdditionalInspectionType.Name = "AdditionalInspectionType";
			this.AdditionalInspectionType.PreBoundMaxLength = 3;
			this.DetailsPanel.SetRow(this.AdditionalInspectionType, 16);
			this.AdditionalInspectionType.ShouldResizeByMaxLength = true;
			this.AdditionalInspectionType.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 20, true);
			this.AdditionalInspectionType.TabIndex = 30;
			this.AdditionalInspectionType.Visible = false;
			//
			// ShipperCOD
			//
			this.ShipperCOD.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ShipperCOD, "JS_ShipperCODAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((ForwardingShipment)(null)).JS_ShipperCODAmount)));
			this.ShipperCOD.DecimalPlaces = 2;
			this.ShipperCOD.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 483, true);
			this.ShipperCOD.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ShipperCOD.Name = "ShipperCOD";
			this.DetailsPanel.SetRow(this.ShipperCOD, 25);
			this.ShipperCOD.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 20, true);
			this.ShipperCOD.TabIndex = 42;
			this.ShipperCOD.Text = "0.00";
			this.ShipperCOD.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// ServiceLevel
			//
			this.ServiceLevel.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ServiceLevel, "JS_RS_NKServiceLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ForwardingShipment)(null)).JS_RS_NKServiceLevel)));
			this.ServiceLevel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 232, true);
			this.ServiceLevel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ServiceLevel.Name = "ServiceLevel";
			this.ServiceLevel.PopupCaption = null;
			this.ServiceLevel.PreBoundMaxLength = 3;
			this.DetailsPanel.SetRow(this.ServiceLevel, 11);
			this.ServiceLevel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 20, true);
			this.ServiceLevel.TabIndex = 23;
			//
			// ExportStatement
			//
			this.ExportStatement.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExportStatement, "DocsAndCartage+JP_ExportStatement");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((ForwardingShipment)(null)).DocsAndCartage.JP_ExportStatement)));
			this.ExportStatement.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 462, true);
			this.ExportStatement.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ExportStatement.Name = "ExportStatement";
			this.ExportStatement.PreBoundMaxLength = 3;
			this.DetailsPanel.SetRow(this.ExportStatement, 24);
			this.ExportStatement.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 20, true);
			this.ExportStatement.TabIndex = 41;
			//
			// JS_PaymentTermLabel
			//
			this.BindingSource.SetBindingMember(this.JS_PaymentTermLabel, "JS_PaymentTermDisplay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ForwardingShipment)(null)).JS_PaymentTermDisplay)));
			this.JS_PaymentTermLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DetailsEntryControl|437ef809-d096-4191-95f7-96f58a5f8498", "Freight Prepaid");
			this.JS_PaymentTermLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 210, true);
			this.JS_PaymentTermLabel.Name = "JS_PaymentTermLabel";
			this.DetailsPanel.SetRow(this.JS_PaymentTermLabel, 10);
			this.JS_PaymentTermLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 20, true);
			this.JS_PaymentTermLabel.TabIndex = 22;
			//
			// ChargesApply
			//
			this.ChargesApply.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ChargesApply, "JS_HBLAWBChargesDisplay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((ForwardingShipment)(null)).JS_HBLAWBChargesDisplay)));
			this.ChargesApply.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 441, true);
			this.ChargesApply.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ChargesApply.Name = "ChargesApply";
			this.ChargesApply.PreBoundMaxLength = 3;
			this.DetailsPanel.SetRow(this.ChargesApply, 23);
			this.ChargesApply.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 20, true);
			this.ChargesApply.TabIndex = 40;
			//
			// PaymentTerm
			//
			this.PaymentTerm.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PaymentTerm, "JS_INCO");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((ForwardingShipment)(null)).JS_INCO)));
			this.PaymentTerm.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 210, true);
			this.PaymentTerm.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.PaymentTerm.Name = "PaymentTerm";
			this.PaymentTerm.PreBoundMaxLength = 3;
			this.DetailsPanel.SetRow(this.PaymentTerm, 10);
			this.PaymentTerm.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 20, true);
			this.PaymentTerm.TabIndex = 20;
			//
			// JS_NoCopyBillsCalcEdit
			//
			this.JS_NoCopyBillsCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.JS_NoCopyBillsCalcEdit, "JS_NoCopyBills");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((ForwardingShipment)(null)).JS_NoCopyBills)));
			this.JS_NoCopyBillsCalcEdit.DecimalPlaces = 0;
			this.JS_NoCopyBillsCalcEdit.Decimals = 0;
			this.JS_NoCopyBillsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(233, 420, true);
			this.JS_NoCopyBillsCalcEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.JS_NoCopyBillsCalcEdit.Name = "JS_NoCopyBillsCalcEdit";
			this.DetailsPanel.SetRow(this.JS_NoCopyBillsCalcEdit, 22);
			this.JS_NoCopyBillsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.JS_NoCopyBillsCalcEdit.TabIndex = 39;
			this.JS_NoCopyBillsCalcEdit.Text = "0";
			this.JS_NoCopyBillsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// JS_NoOriginalBillsCalcEdit
			//
			this.JS_NoOriginalBillsCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.JS_NoOriginalBillsCalcEdit, "JS_NoOriginalBills");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((ForwardingShipment)(null)).JS_NoOriginalBills)));
			this.JS_NoOriginalBillsCalcEdit.DecimalPlaces = 0;
			this.JS_NoOriginalBillsCalcEdit.Decimals = 0;
			this.JS_NoOriginalBillsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(149, 420, true);
			this.JS_NoOriginalBillsCalcEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.JS_NoOriginalBillsCalcEdit.Name = "JS_NoOriginalBillsCalcEdit";
			this.DetailsPanel.SetRow(this.JS_NoOriginalBillsCalcEdit, 22);
			this.JS_NoOriginalBillsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(25, 20, true);
			this.JS_NoOriginalBillsCalcEdit.TabIndex = 38;
			this.JS_NoOriginalBillsCalcEdit.Text = "0";
			this.JS_NoOriginalBillsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// BillDetails
			//
			this.BillDetails.AllowDrop = true;
			this.BillDetails.AutoCompleteMonthThreshold = 1;
			this.BillDetails.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.BillDetails, "JS_HouseBillIssueDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((ForwardingShipment)(null)).JS_HouseBillIssueDate)));
			this.BillDetails.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 420, true);
			this.BillDetails.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.BillDetails.Name = "BillDetails";
			this.DetailsPanel.SetRow(this.BillDetails, 22);
			this.BillDetails.TabIndex = 37;
			//
			// IncoTermExplainButton
			//
			this.IncoTermExplainButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 210, true);
			this.IncoTermExplainButton.Name = "IncoTermExplainButton";
			this.DetailsPanel.SetRow(this.IncoTermExplainButton, 10);
			this.IncoTermExplainButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 20, true);
			this.IncoTermExplainButton.TabIndex = 21;
			this.IncoTermExplainButton.Text = "...";
			this.IncoTermExplainButton.Click += new EventHandler(this.IncoTermExplainButton_Click);
			//
			// MarksNumbers
			//
			this.MarksNumbers.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MarksNumbers, "JS_MarksAndNumbersShort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((ForwardingShipment)(null)).JS_MarksAndNumbersShort)));
			this.MarksNumbers.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DetailsEntryControl|cb823b48-8add-45e3-a7a9-81fc8c2c748f", "Marks & Nums.", "Marks and Numbers", "");
			this.MarksNumbers.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 187, true);
			this.MarksNumbers.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.MarksNumbers.MaximumNoteLength = 10000;
			this.MarksNumbers.Name = "MarksNumbers";
			this.MarksNumbers.NoteTypeDescription = "Marks & Numbers";
			this.MarksNumbers.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 0, 1, true);
			this.DetailsPanel.SetRow(this.MarksNumbers, 9);
			this.MarksNumbers.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 20, true);
			this.MarksNumbers.TabIndex = 19;
			//
			// ContainerModeOverride
			//
			this.ContainerModeOverride.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ContainerModeOverride, "JS_HBLContainerPackModeOverride");
			//// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((ForwardingShipment)(null)).JS_HBLContainerPackModeOverride)));
			this.ContainerModeOverride.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 399, true);
			this.ContainerModeOverride.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ContainerModeOverride.Name = "ContainerModeOverride";
			this.ContainerModeOverride.PreBoundMaxLength = 9;
			this.DetailsPanel.SetRow(this.ContainerModeOverride, 21);
			this.ContainerModeOverride.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 20, true);
			this.ContainerModeOverride.TabIndex = 36;
			//
			// Description
			//
			this.Description.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.Description, "JS_GoodsDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((ForwardingShipment)(null)).JS_GoodsDescription)));
			this.Description.ButtonText = Enterprise.Freight.Forwarding.GUI.Res.GetString("DetailsEntryControl|1665c0c1-6da3-4769-8c23-B2c6a88f9923", "Details");
			this.Description.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 167, true);
			this.Description.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.Description.Name = "Description";
			this.Description.NoteTypeDescription = "Detailed Goods Description";
			this.DetailsPanel.SetRow(this.Description, 8);
			this.Description.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 23, true);
			this.Description.TabIndex = 18;
			//
			// JS_InsuranceBoundCurrencyControl
			//
			this.JS_InsuranceBoundCurrencyControl.AllowDrop = true;
			this.JS_InsuranceBoundCurrencyControl.BindToAmount = "JS_InsuranceValue";
			this.JS_InsuranceBoundCurrencyControl.BindToUnit = "JS_RX_NKInsuranceCurrency";
			this.JS_InsuranceBoundCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.JS_InsuranceBoundCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 147, true);
			this.JS_InsuranceBoundCurrencyControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.JS_InsuranceBoundCurrencyControl.Name = "JS_InsuranceBoundCurrencyControl";
			this.DetailsPanel.SetRow(this.JS_InsuranceBoundCurrencyControl, 7);
			this.JS_InsuranceBoundCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 20, true);
			this.JS_InsuranceBoundCurrencyControl.TabIndex = 17;
			//
			// JS_ShippedOnBoardDateEdit
			//
			this.JS_ShippedOnBoardDateEdit.AllowDrop = true;
			this.JS_ShippedOnBoardDateEdit.AutoCompleteMonthThreshold = 1;
			this.JS_ShippedOnBoardDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JS_ShippedOnBoardDateEdit, "JS_ShippedOnBoardDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((ForwardingShipment)(null)).JS_ShippedOnBoardDate)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JS_ShippedOnBoardDateEdit, false);
			this.JS_ShippedOnBoardDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(181, 379, true);
			this.JS_ShippedOnBoardDateEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.JS_ShippedOnBoardDateEdit.Name = "JS_ShippedOnBoardDateEdit";
			this.DetailsPanel.SetRow(this.JS_ShippedOnBoardDateEdit, 20);
			this.JS_ShippedOnBoardDateEdit.TabIndex = 35;
			//
			// OnBoard
			//
			this.OnBoard.AllowDrop = true;
			this.OnBoard.Anchor = ((AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OnBoard, "JS_ShippedOnBoard");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((ForwardingShipment)(null)).JS_ShippedOnBoard)));
			this.OnBoard.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 378, true);
			this.OnBoard.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.OnBoard.Name = "OnBoard";
			this.OnBoard.PreBoundMaxLength = 3;
			this.DetailsPanel.SetRow(this.OnBoard, 20);
			this.OnBoard.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 20, true);
			this.OnBoard.TabIndex = 34;
			//
			// HousebillType
			//
			this.HousebillType.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HousebillType, "JS_HouseBillOfLadingType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((ForwardingShipment)(null)).JS_HouseBillOfLadingType)));
			this.HousebillType.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 357, true);
			this.HousebillType.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.HousebillType.Name = "HousebillType";
			this.HousebillType.PreBoundMaxLength = 3;
			this.DetailsPanel.SetRow(this.HousebillType, 19);
			this.HousebillType.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 20, true);
			this.HousebillType.TabIndex = 33;
			//
			// JS_TotalPackageCountBoundDropCalcEdit
			//
			this.JS_TotalPackageCountBoundDropCalcEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JS_TotalPackageCountBoundDropCalcEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((ForwardingShipment)(null)).JS_TotalPackageCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ForwardingShipment)(null)).JS_F3_NKTotalCountPackType)));
			this.JS_TotalPackageCountBoundDropCalcEdit.BindToAmount = "JS_TotalPackageCount";
			this.JS_TotalPackageCountBoundDropCalcEdit.BindToUnit = "JS_F3_NKTotalCountPackType";
			this.JS_TotalPackageCountBoundDropCalcEdit.Decimals = 0;
			this.JS_TotalPackageCountBoundDropCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(158, 106, true);
			this.JS_TotalPackageCountBoundDropCalcEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.JS_TotalPackageCountBoundDropCalcEdit.Name = "JS_TotalPackageCountBoundDropCalcEdit";
			this.DetailsPanel.SetRow(this.JS_TotalPackageCountBoundDropCalcEdit, 5);
			this.JS_TotalPackageCountBoundDropCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 20, true);
			this.JS_TotalPackageCountBoundDropCalcEdit.TabIndex = 14;
			this.JS_TotalPackageCountBoundDropCalcEdit.UnitPreBoundMaxLength = 3;
			//
			// JS_RL_NKDestinationCodeFindBox
			//
			this.JS_RL_NKDestinationCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JS_RL_NKDestinationCodeFindBox, "JS_RL_NKDestination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ForwardingShipment)(null)).JS_RL_NKDestination)));
			this.JS_RL_NKDestinationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 42, true);
			this.JS_RL_NKDestinationCodeFindBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.JS_RL_NKDestinationCodeFindBox.Name = "JS_RL_NKDestinationCodeFindBox";
			this.JS_RL_NKDestinationCodeFindBox.PopupCaption = "Destination Location";
			this.JS_RL_NKDestinationCodeFindBox.PreBoundMaxLength = 5;
			this.DetailsPanel.SetRow(this.JS_RL_NKDestinationCodeFindBox, 2);
			this.JS_RL_NKDestinationCodeFindBox.ShowDescriptionBox = false;
			this.JS_RL_NKDestinationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 21, true);
			this.JS_RL_NKDestinationCodeFindBox.TabIndex = 5;
			//
			// AirwayBillDims
			//
			this.AirwayBillDims.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AirwayBillDims, "DocsAndCartage+JP_PrintOptionForPackagesOnAWB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((ForwardingShipment)(null)).DocsAndCartage.JP_PrintOptionForPackagesOnAWB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((ForwardingShipment)(null)).Lookups.AWBDimsCodeDescriptionPairList)));
			this.AirwayBillDims.BindToList = "Lookups.AWBDimsCodeDescriptionPairList";
			this.AirwayBillDims.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 336, true);
			this.AirwayBillDims.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.AirwayBillDims.Name = "AirwayBillDims";
			this.AirwayBillDims.PreBoundMaxLength = 3;
			this.DetailsPanel.SetRow(this.AirwayBillDims, 18);
			this.AirwayBillDims.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 20, true);
			this.AirwayBillDims.TabIndex = 32;
			//
			// PacksValues
			//
			this.PacksValues.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PacksValues, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((ForwardingShipment)(null)).JS_OuterPacks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ForwardingShipment)(null)).JS_F3_NKPackType)));
			this.PacksValues.BindToAmount = "JS_OuterPacks";
			this.PacksValues.BindToUnit = "JS_F3_NKPackType";
			this.PacksValues.Decimals = 0;
			this.PacksValues.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 105, true);
			this.PacksValues.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.PacksValues.Name = "PacksValues";
			this.DetailsPanel.SetRow(this.PacksValues, 5);
			this.PacksValues.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 20, true);
			this.PacksValues.TabIndex = 13;
			this.PacksValues.UnitPreBoundMaxLength = 3;
			//
			// ReleaseType
			//
			this.ReleaseType.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReleaseType, "JS_ReleaseType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((ForwardingShipment)(null)).JS_ReleaseType)));
			this.ReleaseType.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 315, true);
			this.ReleaseType.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ReleaseType.Name = "ReleaseType";
			this.ReleaseType.PreBoundMaxLength = 3;
			this.DetailsPanel.SetRow(this.ReleaseType, 17);
			this.ReleaseType.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 20, true);
			this.ReleaseType.TabIndex = 31;
			//
			// PiecesDetailButton
			//
			this.PiecesDetailButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DetailsEntryControl|e526bc4a-d1d5-feb6-4334-0bce45ebc994", "Inner Packs Details");
			this.PiecesDetailButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 126, true);
			this.PiecesDetailButton.Name = "PiecesDetailButton";
			this.DetailsPanel.SetRow(this.PiecesDetailButton, 6);
			this.PiecesDetailButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 23, true);
			this.PiecesDetailButton.TabIndex = 16;
			this.PiecesDetailButton.Click += new EventHandler(this.PiecesDetailButton_Click);
			//
			// JS_ActualVolumeBoundCalcDropEdit
			//
			this.JS_ActualVolumeBoundCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JS_ActualVolumeBoundCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((ForwardingShipment)(null)).JS_ActualVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ForwardingShipment)(null)).JS_UnitOfVolume)));
			this.JS_ActualVolumeBoundCalcDropEdit.BindToAmount = "JS_ActualVolume";
			this.JS_ActualVolumeBoundCalcDropEdit.BindToUnit = "JS_UnitOfVolume";
			this.JS_ActualVolumeBoundCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 64, true);
			this.JS_ActualVolumeBoundCalcDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.JS_ActualVolumeBoundCalcDropEdit.Name = "JS_ActualVolumeBoundCalcDropEdit";
			this.DetailsPanel.SetRow(this.JS_ActualVolumeBoundCalcDropEdit, 3);
			this.JS_ActualVolumeBoundCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(117, 20, true);
			this.JS_ActualVolumeBoundCalcDropEdit.TabIndex = 8;
			this.JS_ActualVolumeBoundCalcDropEdit.UnitPreBoundMaxLength = 2;
			//
			// JS_GoodsValueBoundCurrencyControl
			//
			this.JS_GoodsValueBoundCurrencyControl.AllowDrop = true;
			this.JS_GoodsValueBoundCurrencyControl.BindToAmount = "JS_GoodsValue";
			this.JS_GoodsValueBoundCurrencyControl.BindToUnit = "JS_RX_NKGoodsValueCurr";
			this.JS_GoodsValueBoundCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.JS_GoodsValueBoundCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 126, true);
			this.JS_GoodsValueBoundCurrencyControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.JS_GoodsValueBoundCurrencyControl.Name = "JS_GoodsValueBoundCurrencyControl";
			this.DetailsPanel.SetRow(this.JS_GoodsValueBoundCurrencyControl, 6);
			this.JS_GoodsValueBoundCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 20, true);
			this.JS_GoodsValueBoundCurrencyControl.TabIndex = 15;
			//
			// JS_E_ARVBoundDateEdit
			//
			this.JS_E_ARVBoundDateEdit.AllowDrop = true;
			this.JS_E_ARVBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JS_E_ARVBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JS_E_ARVBoundDateEdit, "JS_E_ARV");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((ForwardingShipment)(null)).JS_E_ARV)));
			this.JS_E_ARVBoundDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JS_E_ARVBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 42, true);
			this.JS_E_ARVBoundDateEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.JS_E_ARVBoundDateEdit.Name = "JS_E_ARVBoundDateEdit";
			this.DetailsPanel.SetRow(this.JS_E_ARVBoundDateEdit, 2);
			this.JS_E_ARVBoundDateEdit.TabIndex = 6;
			//
			// ChargeableUnitLabel
			//
			this.ChargeableUnitLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ChargeableUnitLabel, "JS_ChargeableUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ForwardingShipment)(null)).JS_ChargeableUnit)));
			this.ChargeableUnitLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DetailsEntryControl|1425fe1e-00a9-49f0-96cd-da304270aa68", "(CBM)");
			this.ChargeableUnitLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 88, true);
			this.ChargeableUnitLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ChargeableUnitLabel.Name = "ChargeableUnitLabel";
			this.DetailsPanel.SetRow(this.ChargeableUnitLabel, 4);
			this.ChargeableUnitLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 13, true);
			this.ChargeableUnitLabel.TabIndex = 10;
			//
			// DomesticCheckBox
			//
			this.DomesticCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.DomesticCheckBox, "IsDomesticFreight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((ForwardingShipment)(null)).IsDomesticFreight)));
			this.DomesticCheckBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DetailsEntryControl|fed43e60-ad7d-4cb0-adcc-37a515f3b2fc", "Domestic", "Domestic Movement", "Specifies whether this shipment is a domestic movement, or an international movement.");
			this.DomesticCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.DomesticCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 2, true);
			this.DomesticCheckBox.Name = "DomesticCheckBox";
			this.DetailsPanel.SetRow(this.DomesticCheckBox, 0);
			this.DomesticCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 17, true);
			this.DomesticCheckBox.TabIndex = 2;
			//
			// Chargeable
			//
			this.Chargeable.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.Chargeable, "JS_ActualChargeable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((ForwardingShipment)(null)).JS_ActualChargeable)));
			this.Chargeable.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 84, true);
			this.Chargeable.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.Chargeable.Name = "Chargeable";
			this.DetailsPanel.SetRow(this.Chargeable, 4);
			this.Chargeable.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 20, true);
			this.Chargeable.TabIndex = 9;
			this.Chargeable.Text = "0.000";
			this.Chargeable.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// JS_Calc_ActualVolumeWeight
			//
			this.BindingSource.SetBindingMember(this.JS_Calc_ActualVolumeWeight, "JS_Calc_ActualVolumeWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((ForwardingShipment)(null)).JS_Calc_ActualVolumeWeight)));
			this.JS_Calc_ActualVolumeWeight.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 84, true);
			this.JS_Calc_ActualVolumeWeight.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.JS_Calc_ActualVolumeWeight.Name = "JS_Calc_ActualVolumeWeight";
			this.DetailsPanel.SetRow(this.JS_Calc_ActualVolumeWeight, 4);
			this.JS_Calc_ActualVolumeWeight.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 20, true);
			//
			// JS_Calc_ActualVolumeWeightUnit
			//
			this.JS_Calc_ActualVolumeWeightUnit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.JS_Calc_ActualVolumeWeightUnit, "JS_Calc_ActualVolumeWeightUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ForwardingShipment)(null)).JS_Calc_ActualVolumeWeightUnit)));
			this.JS_Calc_ActualVolumeWeightUnit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DetailsEntryControl|585ee899-0353-48c9-88b7-6db1cc039fb3", "(VWU)");
			this.JS_Calc_ActualVolumeWeightUnit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(222, 88, true);
			this.JS_Calc_ActualVolumeWeightUnit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.JS_Calc_ActualVolumeWeightUnit.Name = "JS_Calc_ActualVolumeWeightUnit";
			this.DetailsPanel.SetRow(this.JS_Calc_ActualVolumeWeightUnit, 4);
			this.JS_Calc_ActualVolumeWeightUnit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(34, 13, true);
			//
			// HouseBill
			//
			this.HouseBill.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.HouseBill, "JS_HouseBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ForwardingShipment)(null)).JS_HouseBill)));
			this.HouseBill.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HouseBill.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.HouseBill.Name = "HouseBill";
			this.DetailsPanel.SetRow(this.HouseBill, 0);
			this.HouseBill.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 20, true);
			this.HouseBill.TabIndex = 1;
			//
			// WeightVolumeChargeable
			//
			this.WeightVolumeChargeable.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WeightVolumeChargeable, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((ForwardingShipment)(null)).JS_ActualWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ForwardingShipment)(null)).JS_UnitOfWeight)));
			this.WeightVolumeChargeable.BindToAmount = "JS_ActualWeight";
			this.WeightVolumeChargeable.BindToUnit = "JS_UnitOfWeight";
			this.WeightVolumeChargeable.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 63, true);
			this.WeightVolumeChargeable.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.WeightVolumeChargeable.Name = "WeightVolumeChargeable";
			this.DetailsPanel.SetRow(this.WeightVolumeChargeable, 3);
			this.WeightVolumeChargeable.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 20, true);
			this.WeightVolumeChargeable.TabIndex = 7;
			this.WeightVolumeChargeable.UnitPreBoundMaxLength = 2;
			//
			// JS_E_DEPBoundDateEdit
			//
			this.JS_E_DEPBoundDateEdit.AllowDrop = true;
			this.JS_E_DEPBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JS_E_DEPBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JS_E_DEPBoundDateEdit, "JS_E_DEP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((ForwardingShipment)(null)).JS_E_DEP)));
			this.JS_E_DEPBoundDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JS_E_DEPBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 21, true);
			this.JS_E_DEPBoundDateEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.JS_E_DEPBoundDateEdit.Name = "JS_E_DEPBoundDateEdit";
			this.DetailsPanel.SetRow(this.JS_E_DEPBoundDateEdit, 1);
			this.JS_E_DEPBoundDateEdit.TabIndex = 4;
			//
			// OriginDestinationDates
			//
			this.OriginDestinationDates.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OriginDestinationDates, "JS_RL_NKOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ForwardingShipment)(null)).JS_RL_NKOrigin)));
			this.OriginDestinationDates.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 21, true);
			this.OriginDestinationDates.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.OriginDestinationDates.Name = "OriginDestinationDates";
			this.OriginDestinationDates.PopupCaption = "Origin Location";
			this.OriginDestinationDates.PreBoundMaxLength = 5;
			this.DetailsPanel.SetRow(this.OriginDestinationDates, 1);
			this.OriginDestinationDates.ShowDescriptionBox = false;
			this.OriginDestinationDates.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 21, true);
			this.OriginDestinationDates.TabIndex = 3;
			//
			// FreightSpotRate
			//
			this.FreightSpotRate.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FreightSpotRate, ".");
			this.FreightSpotRate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 735, true);
			this.FreightSpotRate.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("e3c67373-9c84-4a98-a145-2996895b1c2d", "Spot Rate");
			this.FreightSpotRate.CaptionRenderingEnabled = true;
			this.FreightSpotRate.AutoratingModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(133, 20, true);
			this.FreightSpotRate.RateType = Freight.Business.FreightConstants.SpotRateType.ShipmentSellRate;
			this.FreightSpotRate.Name = "FreightSpotRate";
			this.DetailsPanel.SetRow(this.FreightSpotRate, 35);
			this.FreightSpotRate.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 21, true);
			this.FreightSpotRate.TabIndex = 48;
			//
			// EFreightStatus
			//
			this.EFreightStatus.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EFreightStatus, "JS_EFreightStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ForwardingShipment)(null)).JS_EFreightStatus)));
			this.EFreightStatus.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 756, true);
			this.EFreightStatus.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.EFreightStatus.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("73c1dd55-2fd2-4f25-8419-0ae276b35667", "e-freight Status");
			this.EFreightStatus.Name = "EFreightStatus";
			this.EFreightStatus.PreBoundMaxLength = 3;
			this.DetailsPanel.SetRow(this.EFreightStatus, 36);
			this.EFreightStatus.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 20, true);
			this.EFreightStatus.TabIndex = 55;
			//
			// ControllingCustomerAddressControl
			//
			this.ControllingCustomerAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ControllingCustomerAddressControl, "ControllingCustomerAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDocAddress)(((ForwardingShipment)(null)).ControllingCustomerAddress)));
			this.ControllingCustomerAddressControl.BindToOrganisations = "Lookups.ControllingCustomerList";
			this.ControllingCustomerAddressControl.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ForwarderAdditionalDetailsControl|a26e7471-72c2-489d-8d0b-3466da88cb18", "Ctrl. Customer", "Controlling Customer");
			this.ControllingCustomerAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.ControllingCustomerAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 776, true);
			this.ControllingCustomerAddressControl.Name = "ControllingCustomerAddress";
			this.ControllingCustomerAddressControl.SingleLineNoGroupBoxPanelWidth = 263;
			this.ControllingCustomerAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(262, 20, true);
			this.ControllingCustomerAddressControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ControllingCustomerAddressControl.TabIndex = 50;
			//
			// ControllingAgentAddressControl
			//
			this.ControllingAgentAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ControllingAgentAddressControl, "ControllingAgentDocumentaryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDocAddress)(((ForwardingShipment)(null)).ControllingAgentDocumentaryAddress)));
			this.ControllingAgentAddressControl.BindToOrganisations = "Lookups.ControllingAgentList";
			this.ControllingAgentAddressControl.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ForwarderAdditionalDetailsControl|d88fbf3a-abf6-4632-93c8-889495004640", "Ctrl. Agent", "Controlling Agent");
			this.ControllingAgentAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.ControllingAgentAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 796, true);
			this.ControllingAgentAddressControl.Name = "ControllingAgentAddress";
			this.ControllingAgentAddressControl.SingleLineNoGroupBoxPanelWidth = 263;
			this.ControllingAgentAddressControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ControllingAgentAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(263, 20, true);
			this.ControllingAgentAddressControl.TabIndex = 53;
			//
			// JS_AttachedOrderXMLUpdateCutOffDateUtcDateEdit
			//
			this.OrderUpdateCutOff.AllowDrop = true;
			this.OrderUpdateCutOff.AutoCompleteMonthThreshold = 1;
			this.OrderUpdateCutOff.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.OrderUpdateCutOff, "JS_AttachedOrderXMLUpdateCutOffDateUtc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((ForwardingShipment)(null)).JS_AttachedOrderXMLUpdateCutOffDateUtc)));
			this.OrderUpdateCutOff.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("cde22ad0-afdf-46a2-bd98-7f20d579c49e", "Order Update Cutoff Date");
			this.OrderUpdateCutOff.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.OrderUpdateCutOff.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 816, true);
			this.OrderUpdateCutOff.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.OrderUpdateCutOff.Name = "OrderUpdateCutOff";
			this.OrderUpdateCutOff.TabIndex = 56;
			this.DetailsPanel.SetRow(this.OrderUpdateCutOff, 37);
			//
			// TotalCO2e
			//
			this.TotalCO2e.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.TotalCO2e, "TotalCO2eForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ForwardingShipment)(null)).TotalCO2eForBinding)));
			this.TotalCO2e.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("5fe2e2b6-c7ba-4625-9b7f-c685291f8c88", "CO2e");
			this.TotalCO2e.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 84, true);
			this.TotalCO2e.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.TotalCO2e.Name = "TotalCO2e";
			this.DetailsPanel.SetRow(this.TotalCO2e, 38);
			this.TotalCO2e.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 20, true);
			this.TotalCO2e.TabIndex = 57;
			this.TotalCO2e.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TotalCO2e.Visible = false;
			//
			// TotalCO2eUnit
			//
			this.TotalCO2eUnit.AutoSize = true;
			this.TotalCO2eUnit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 88, true);
			this.TotalCO2eUnit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.TotalCO2eUnit.Name = "TotalCO2eUnit";
			this.TotalCO2eUnit.Text = Enterprise.Freight.Forwarding.GUI.Res.GetString("ed8a6031-8d02-4a78-a5ad-e593d8dde66a", "KG");
			this.DetailsPanel.SetRow(this.TotalCO2eUnit, 38);
			this.TotalCO2eUnit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 13, true);
			this.TotalCO2eUnit.TabIndex = 58;
			//
			// DestinationGoodsValueCalcFindBox
			//
			this.DestinationGoodsValueCalcFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DestinationGoodsValueCalcFindBox, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((ForwardingShipment)(null)).DestinationGoodsValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ForwardingShipment)(null)).DestinationCurrencyCode)));
			this.DestinationGoodsValueCalcFindBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DetailsEntryControl|65c360b1-8103-44a6-ae37-439f0e88072d", "Destination Value");
			this.DestinationGoodsValueCalcFindBox.BindToAmount = "DestinationGoodsValue";
			this.DestinationGoodsValueCalcFindBox.BindToUnit = "DestinationCurrencyCode";
			this.DestinationGoodsValueCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.DestinationGoodsValueCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 777, true);
			this.DestinationGoodsValueCalcFindBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.DestinationGoodsValueCalcFindBox.Name = "DestinationGoodsValueCalcFindBox";
			this.DetailsPanel.SetRow(this.DestinationGoodsValueCalcFindBox, 39);
			this.DestinationGoodsValueCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 20, true);
			this.DestinationGoodsValueCalcFindBox.TabIndex = 59;
			//
			// DestinationExchangeRateCalcEdit
			//
			this.DestinationExchangeRateCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.DestinationExchangeRateCalcEdit, "DestinationExchangeRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((ForwardingShipment)(null)).DestinationExchangeRate)));
			this.DestinationExchangeRateCalcEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DetailsEntryControl|bd6fc19d-5ab2-4166-bbf6-eddcac7918ae", "Ex. Rate", "Dest. Ex. Rate", "Destination Exchange Rate");
			this.DestinationExchangeRateCalcEdit.DecimalPlaces = 6;
			this.DestinationExchangeRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(195, 777, true);
			this.DestinationExchangeRateCalcEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.DestinationExchangeRateCalcEdit.Name = "DestinationExchangeRateCalcEdit";
			this.DetailsPanel.SetRow(this.DestinationExchangeRateCalcEdit, 39);
			this.DestinationExchangeRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 20, true);
			this.DestinationExchangeRateCalcEdit.TabIndex = 60;
			this.DestinationExchangeRateCalcEdit.Text = "0.00";
			this.DestinationExchangeRateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// DetailsEntryControl
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DetailsPanelGroupBox);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.Name = "DetailsEntryControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(366, 826, true);
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.DetailsPanelGroupBox.ResumeLayout(false);
			this.DetailsPanelGroupBox.PerformLayout();
			this.DetailsPanel.ResumeLayout(false);
			this.DetailsPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
