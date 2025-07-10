using CargoWise.Application;
using Enterprise.Freight.Business;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	partial class ConsolDetailsControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo overallComplianceRiskColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo partyComplianceRiskColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo locationComplianceRiskColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo commodityComplianceRiskColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo co2eColumnStyleInfo = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();

			this.ConsolsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ConsolModuleButtonGrid = new Enterprise.Freight.Forwarding.GUI.ConsolModuleButtonGrid();
			this.panel4 = new CargoWise.Windows.UI.KPanel();
			this.JS_OA_BookedShippingLineAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.JS_RL_NKDischargePort = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JS_RL_NKLoadPort = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JK_JX_JA_E_DEPBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JK_JX_JB_E_ARVBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JK_JX_JV_NKVesselTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JK_JX_JV_VoyageFlightTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ConsolsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ConsolModuleButtonGrid.InnerGrid)).BeginInit();
			this.ConsolModuleButtonGrid.SuspendLayout();
			this.panel4.SuspendLayout();
			this.JS_RL_NKDischargePort.SuspendLayout();
			this.JS_RL_NKLoadPort.SuspendLayout();
			this.JK_JX_JA_E_DEPBoundDateEdit.SuspendLayout();
			this.JK_JX_JB_E_ARVBoundDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.ForwardingShipment);
			// 
			// ConsolsGroupBox
			// 
			this.ConsolsGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolDetailsControl|b0ace523-3585-498a-bfca-5b768f90b9e4", "Consolidation Details");
			this.ConsolsGroupBox.Controls.Add(this.ConsolModuleButtonGrid);
			this.ConsolsGroupBox.Controls.Add(this.panel4);
			this.ConsolsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.ConsolsGroupBox, true);
			this.ConsolsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ConsolsGroupBox.Name = "ConsolsGroupBox";
			this.ConsolsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 308, true);
			this.ConsolsGroupBox.TabIndex = 0;
			this.ConsolsGroupBox.TabStop = false;
			// 
			// ConsolModuleButtonGrid
			// 
			this.ConsolModuleButtonGrid.AllowDrop = true;
			this.ConsolModuleButtonGrid.AlwaysRequiresSaveBeforeEdit = true;
			this.BindingSource.SetBindingMember(this.ConsolModuleButtonGrid, "Consols");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).Consols)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).Lookups.Consols_List)));
			this.ConsolModuleButtonGrid.BindToFindBoxList = "Lookups+Consols_List";
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "JK_UniqueConsignRef";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "JK_RL_NKLoadPort";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "JK_RL_NKDischargePort";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.ColumnName = "JK_MasterBillNum";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.ColumnName = "JK_RS_NKGatewayServiceLevel";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.IsVisible = false;

			overallComplianceRiskColumnStyleInfo.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolDetailsControl|1aeb19c7-8d0b-4217-997a-239d1fcbc434", "Job Compliance", "Job Compliance Status", "");
			overallComplianceRiskColumnStyleInfo.ColumnName = "OverallComplianceRisk";
			overallComplianceRiskColumnStyleInfo.IsVisible = false;
			overallComplianceRiskColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);

			partyComplianceRiskColumnStyleInfo.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolDetailsControl|A5C13947-C539-4D2B-89A8-0E08126DDE41", "Party Risk", "Party Risk Status", "Party Compliance Risk Status", "");
			partyComplianceRiskColumnStyleInfo.ColumnName = "PartyComplianceRisk";
			partyComplianceRiskColumnStyleInfo.IsVisible = false;
			partyComplianceRiskColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);

			locationComplianceRiskColumnStyleInfo.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolDetailsControl|869A2BBA-BCB4-4C71-9A2E-EF51E131738D", "Location Risk", "Location Risk Status", "Location Compliance Risk Status", "");
			locationComplianceRiskColumnStyleInfo.ColumnName = "LocationComplianceRisk";
			locationComplianceRiskColumnStyleInfo.IsVisible = false;
			locationComplianceRiskColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);

			commodityComplianceRiskColumnStyleInfo.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolDetailsControl|21ABE7E2-F569-46FA-82A9-C0A718EC67DB", "Commodity Risk", "Commodity Risk Status", "Commodity Compliance Risk Status", "");
			commodityComplianceRiskColumnStyleInfo.ColumnName = "CommodityComplianceRisk";
			commodityComplianceRiskColumnStyleInfo.IsVisible = false;
			commodityComplianceRiskColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);

			co2eColumnStyleInfo.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolDetailsControl|3953d84b-d020-4330-8e73-755870280518", "CO2e (kg)");
			co2eColumnStyleInfo.ColumnName = "TotalCO2eForSorting";
			co2eColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			co2eColumnStyleInfo.IsVisible = false;
			co2eColumnStyleInfo.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			co2eColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);

			this.ConsolModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ConsolModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ConsolModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ConsolModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ConsolModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			if (ComplianceRiskHelper.IsFreightEnabledComplianceWise)
			{
				this.ConsolModuleButtonGrid.ColumnStyles.Add(overallComplianceRiskColumnStyleInfo);
				this.ConsolModuleButtonGrid.ColumnStyles.Add(partyComplianceRiskColumnStyleInfo);
				this.ConsolModuleButtonGrid.ColumnStyles.Add(locationComplianceRiskColumnStyleInfo);
				this.ConsolModuleButtonGrid.ColumnStyles.Add(commodityComplianceRiskColumnStyleInfo);
			}
			if (ObjectFactory.Get<ICO2eFeatureControlHelper>().Enabled)
			{
				this.ConsolModuleButtonGrid.ColumnStyles.Add(co2eColumnStyleInfo);
			}

			this.ConsolModuleButtonGrid.DetachMessage = Enterprise.Freight.Forwarding.GUI.Res.GetData("2136bfc0-0005-4a10-a682-f0cec151bffb", "Are you sure you want to detach the selected Consol(s)?");
			this.ConsolModuleButtonGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConsolModuleButtonGrid.GridId = "31921df6-7c13-4db3-b632-cb3764171d60";
			// 
			// 
			// 
			this.ConsolModuleButtonGrid.InnerGrid.AllowNavigation = false;
			this.ConsolModuleButtonGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ConsolModuleButtonGrid.InnerGrid.CaptionVisible = false;
			this.ConsolModuleButtonGrid.InnerGrid.GridId = null;
			this.ConsolModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ConsolModuleButtonGrid.InnerGrid.IsWholeRowSelectedOnClick = true;
			this.ConsolModuleButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.ConsolModuleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.ConsolModuleButtonGrid.InnerGrid.Name = "Grid";
			this.ConsolModuleButtonGrid.InnerGrid.ReadOnly = true;
			this.ConsolModuleButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(326, 163, true);
			this.ConsolModuleButtonGrid.InnerGrid.TabIndex = 0;
			this.ConsolModuleButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.ConsolModuleButtonGrid.Name = "ConsolModuleButtonGrid";
			this.ConsolModuleButtonGrid.NameOfAGridElement = Enterprise.Freight.Forwarding.GUI.Res.GetData("B0A04810-06F7-45C5-A74F-6ABA2C4947D8", "Consol");
			this.ConsolModuleButtonGrid.ReadOnly = true;
			this.ConsolModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 178, true);
			this.ConsolModuleButtonGrid.TabIndex = 0;
			// 
			// panel4
			// 
			this.panel4.Controls.Add(this.JS_RL_NKDischargePort);
			this.panel4.Controls.Add(this.JS_RL_NKLoadPort);
			this.panel4.Controls.Add(this.JK_JX_JA_E_DEPBoundDateEdit);
			this.panel4.Controls.Add(this.JK_JX_JB_E_ARVBoundDateEdit);
			this.panel4.Controls.Add(this.JK_JX_JV_NKVesselTextBox);
			this.panel4.Controls.Add(this.JK_JX_JV_VoyageFlightTextBox);
			this.panel4.Controls.Add(this.JS_OA_BookedShippingLineAddressControl);
			this.panel4.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.panel4, true);
			this.panel4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 192, true);
			this.panel4.Name = "panel4";
			this.panel4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 114, true);
			this.panel4.TabIndex = 1;
			// 
			// JS_OA_BookedShippingLineAddressControl
			// 
			this.JS_OA_BookedShippingLineAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JS_OA_BookedShippingLineAddressControl, "JS_OA_BookedShippingLineAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).JS_OA_BookedShippingLineAddress)));
			this.JS_OA_BookedShippingLineAddressControl.BindToOrgList = "Lookups.ShippingLine_List";
			this.JS_OA_BookedShippingLineAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 88, true);
			this.JS_OA_BookedShippingLineAddressControl.Name = "JS_OA_BookedShippingLineAddressControl";
			this.JS_OA_BookedShippingLineAddressControl.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("9e47e2e0-4932-9092-4968-959ba492b756", "Planned Carrier");
			this.JS_OA_BookedShippingLineAddressControl.PopupCaption = "Select Shipping Provider";
			this.JS_OA_BookedShippingLineAddressControl.ShowAddress = false;
			this.JS_OA_BookedShippingLineAddressControl.ShowOrganisationName = true;
			this.JS_OA_BookedShippingLineAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 21, true);
			this.JS_OA_BookedShippingLineAddressControl.TabIndex = 4;
			// 
			// JS_RL_NKDischargePort
			// 
			this.JS_RL_NKDischargePort.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JS_RL_NKDischargePort, "JS_RL_NKDischargePort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).JS_RL_NKDischargePort)));
			this.JS_RL_NKDischargePort.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(282, 67, true);
			this.JS_RL_NKDischargePort.Name = "JS_RL_NKDischargePort";
			this.JS_RL_NKDischargePort.ShouldResize = true;
			this.JS_RL_NKDischargePort.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 17, true);
			this.JS_RL_NKDischargePort.TabIndex = 5;
			// 
			// JS_RL_NKLoadPort
			// 
			this.JS_RL_NKLoadPort.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JS_RL_NKLoadPort, "JS_RL_NKLoadPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).JS_RL_NKLoadPort)));
			this.JS_RL_NKLoadPort.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(282, 46, true);
			this.JS_RL_NKLoadPort.Name = "JS_RL_NKLoadPort";
			this.JS_RL_NKLoadPort.ShouldResize = true;
			this.JS_RL_NKLoadPort.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 17, true);
			this.JS_RL_NKLoadPort.TabIndex = 4;
			// 
			// JK_JX_JA_E_DEPBoundDateEdit
			// 
			this.JK_JX_JA_E_DEPBoundDateEdit.AllowDrop = true;
			this.JK_JX_JA_E_DEPBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JK_JX_JA_E_DEPBoundDateEdit.AutoCompleteYear = true;
			this.JK_JX_JA_E_DEPBoundDateEdit.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.JK_JX_JA_E_DEPBoundDateEdit, "Consols.JK_JX_JA_E_DEP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).Consols)).SyncRoot)).JK_JX_JA_E_DEP)));
			this.JK_JX_JA_E_DEPBoundDateEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolDetailsControl|aac98db2-a35d-4550-afd7-5b7ad0ca1e99", "ETD", "Estimated Departure Date", "The Estimated Time of Departure for the Consol.\r\nThis is a read only field that shows the Estimated Time of Departure of the Consol that this shipment is attached to.");
			this.JK_JX_JA_E_DEPBoundDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JK_JX_JA_E_DEPBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(45, 25, true);
			this.JK_JX_JA_E_DEPBoundDateEdit.Name = "JK_JX_JA_E_DEPBoundDateEdit";
			this.JK_JX_JA_E_DEPBoundDateEdit.TabIndex = 1;
			// 
			// JK_JX_JB_E_ARVBoundDateEdit
			// 
			this.JK_JX_JB_E_ARVBoundDateEdit.AllowDrop = true;
			this.JK_JX_JB_E_ARVBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.JK_JX_JB_E_ARVBoundDateEdit.AutoCompleteYear = true;
			this.JK_JX_JB_E_ARVBoundDateEdit.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.JK_JX_JB_E_ARVBoundDateEdit, "Consols.JK_JX_JB_E_ARV");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).Consols)).SyncRoot)).JK_JX_JB_E_ARV)));
			this.JK_JX_JB_E_ARVBoundDateEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolDetailsControl|4194d759-c6cf-4833-a794-ccf8b1efc7b3", "ETA", "Estimated Arrival Date", "The Estimated Time of Arrival for the Consol.\r\n\r\nThis is a read only field that shows the Estimated Time of Arrival of the Consol that this shipment\r\n is attached to.");
			this.JK_JX_JB_E_ARVBoundDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JK_JX_JB_E_ARVBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(45, 3, true);
			this.JK_JX_JB_E_ARVBoundDateEdit.Name = "JK_JX_JB_E_ARVBoundDateEdit";
			this.JK_JX_JB_E_ARVBoundDateEdit.TabIndex = 0;
			// 
			// JK_JX_JV_NKVesselTextBox
			// 
			this.BindingSource.SetBindingMember(this.JK_JX_JV_NKVesselTextBox, "Consols.JK_JX_JV_NKVessel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).Consols)).SyncRoot)).JK_JX_JV_NKVessel)));
			this.JK_JX_JV_NKVesselTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolDetailsControl|e6704408-d459-46f8-a55b-842a19c07394", "Vessel");
			this.JK_JX_JV_NKVesselTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(282, 25, true);
			this.JK_JX_JV_NKVesselTextBox.Name = "JK_JX_JV_NKVesselTextBox";
			this.JK_JX_JV_NKVesselTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 17, true);
			this.JK_JX_JV_NKVesselTextBox.TabIndex = 3;
			// 
			// JK_JX_JV_VoyageFlightTextBox
			// 
			this.JK_JX_JV_VoyageFlightTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.JK_JX_JV_VoyageFlightTextBox, "Consols.JK_JX_JV_VoyageFlight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).Consols)).SyncRoot)).JK_JX_JV_VoyageFlight)));
			this.JK_JX_JV_VoyageFlightTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolDetailsControl|0ce8728c-06a9-4842-abf1-aaaba81e1d96", "Voyage", "Voyage / Flight No", "The Voyage or Flight Number.");
			this.JK_JX_JV_VoyageFlightTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(282, 3, true);
			this.JK_JX_JV_VoyageFlightTextBox.Name = "JK_JX_JV_VoyageFlightTextBox";
			this.JK_JX_JV_VoyageFlightTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 17, true);
			this.JK_JX_JV_VoyageFlightTextBox.TabIndex = 2;
			// 
			// ConsolDetailsControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ConsolsGroupBox);
			this.Name = "ConsolDetailsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 308, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ConsolsGroupBox.ResumeLayout(false);
			this.ConsolsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ConsolModuleButtonGrid.InnerGrid)).EndInit();
			this.ConsolModuleButtonGrid.ResumeLayout(true);
			this.ConsolModuleButtonGrid.PerformLayout();
			this.panel4.ResumeLayout(false);
			this.panel4.PerformLayout();
			this.JS_OA_BookedShippingLineAddressControl.ResumeLayout(true);
			this.JS_OA_BookedShippingLineAddressControl.PerformLayout();
			this.JS_RL_NKDischargePort.ResumeLayout(true);
			this.JS_RL_NKDischargePort.PerformLayout();
			this.JS_RL_NKLoadPort.ResumeLayout(true);
			this.JS_RL_NKLoadPort.PerformLayout();
			this.JK_JX_JA_E_DEPBoundDateEdit.ResumeLayout(true);
			this.JK_JX_JA_E_DEPBoundDateEdit.PerformLayout();
			this.JK_JX_JB_E_ARVBoundDateEdit.ResumeLayout(true);
			this.JK_JX_JB_E_ARVBoundDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZGroupBox ConsolsGroupBox;
		private ConsolModuleButtonGrid ConsolModuleButtonGrid;
		private CargoWise.Windows.UI.KPanel panel4;
		private Enterprise.ZArchitecture.GUI.ZDateEdit JK_JX_JA_E_DEPBoundDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit JK_JX_JB_E_ARVBoundDateEdit;
		private Enterprise.ZArchitecture.ZTextBox JK_JX_JV_NKVesselTextBox;
		private Enterprise.ZArchitecture.ZTextBox JK_JX_JV_VoyageFlightTextBox;
		private ZArchitecture.GUI.ZCodeFindBox JS_RL_NKLoadPort;
		private ZArchitecture.GUI.ZCodeFindBox JS_RL_NKDischargePort;
		private Enterprise.ZArchitecture.GUI.ZAddressControl JS_OA_BookedShippingLineAddressControl;

	}
}
