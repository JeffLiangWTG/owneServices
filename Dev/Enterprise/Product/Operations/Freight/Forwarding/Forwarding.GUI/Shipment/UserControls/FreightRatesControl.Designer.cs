using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class FreightRatesControl : ZUserControl
	{
		FreightRateControl FreightCostRate;
		FreightRateControl FreightGatewaySellRate;
		ZCodeFindBox FreightRateOrigin;
		ZCodeFindBox FreightRateDestination;
		ZDropEdit CompanyTariffLevelOverride;
		ZCodeFindBox RateCommodity;
		ZTextBox RateLocalCode;
		ZTextBox FMCTariffID;
		ZGroupBox GatewaysGroupBox;
		ZGroupBox FreightRatesGroupBox;
		ZCodeFindBox GatewayServiceLevelCodeFindBox;
		ZPanel GatewayServiceLevelPanel;
		ZGrid GatewaysGrid;
		MoveItemButton MoveDownButton;
		MoveItemButton MoveUpButton;
		private ZDropEdit PaymentTermAutoratingOverride;
		ZPanel GatewaysMovementPanel;
		ZDateEdit RevenueAutoratingDate;

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo zAddressDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo();
			this.FreightRatesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RevenueAutoratingDate = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CompanyTariffLevelOverride = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RateCommodity = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.RateLocalCode = new Enterprise.ZArchitecture.ZTextBox();
			this.FMCTariffID = new Enterprise.ZArchitecture.ZTextBox();
			this.FreightCostRate = new Enterprise.Freight.GUI.FreightRateControl();
			this.PaymentTermAutoratingOverride = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FreightGatewaySellRate = new Enterprise.Freight.GUI.FreightRateControl();
			this.FreightRateOrigin = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.FreightRateDestination = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.GatewaysGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.GatewaysGrid = new Enterprise.ZArchitecture.ZGrid();
			this.GatewaysMovementPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MoveDownButton = new Enterprise.Freight.Forwarding.GUI.FreightRatesControl.MoveItemButton();
			this.MoveUpButton = new Enterprise.Freight.Forwarding.GUI.FreightRatesControl.MoveItemButton();
			this.GatewayServiceLevelPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.GatewayServiceLevelCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FreightRatesGroupBox.SuspendLayout();
			this.RevenueAutoratingDate.SuspendLayout();
			this.CompanyTariffLevelOverride.SuspendLayout();
			this.RateCommodity.SuspendLayout();
			this.FreightCostRate.SuspendLayout();
			this.PaymentTermAutoratingOverride.SuspendLayout();
			this.FreightGatewaySellRate.SuspendLayout();
			this.FreightRateOrigin.SuspendLayout();
			this.FreightRateDestination.SuspendLayout();
			this.GatewaysGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.GatewaysGrid)).BeginInit();
			this.GatewaysGrid.SuspendLayout();
			this.GatewaysMovementPanel.SuspendLayout();
			this.GatewayServiceLevelPanel.SuspendLayout();
			this.GatewayServiceLevelCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.ForwardingShipment);
			//
			// FreightRatesGroupBox
			//
			this.FreightRatesGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("FreightRatesControl|7f7dedf3-6f5f-4378-b9d1-f875b0c08278", "Freight Rates");
			this.FreightRatesGroupBox.Controls.Add(this.RevenueAutoratingDate);
			this.FreightRatesGroupBox.Controls.Add(this.CompanyTariffLevelOverride);
			this.FreightRatesGroupBox.Controls.Add(this.RateCommodity);
			this.FreightRatesGroupBox.Controls.Add(this.RateLocalCode);
			this.FreightRatesGroupBox.Controls.Add(this.FMCTariffID);
			this.FreightRatesGroupBox.Controls.Add(this.FreightCostRate);
			this.FreightRatesGroupBox.Controls.Add(this.PaymentTermAutoratingOverride);
			this.FreightRatesGroupBox.Controls.Add(this.FreightGatewaySellRate);
			this.FreightRatesGroupBox.Controls.Add(this.FreightRateOrigin);
			this.FreightRatesGroupBox.Controls.Add(this.FreightRateDestination);
			this.FreightRatesGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.FreightRatesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FreightRatesGroupBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.FreightRatesGroupBox.Name = "FreightRatesGroupBox";
			this.FreightRatesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(439, 232, true);
			this.FreightRatesGroupBox.TabIndex = 0;
			this.FreightRatesGroupBox.TabStop = false;
			//
			// RevenueAutoratingDate
			//
			this.RevenueAutoratingDate.AllowDrop = true;
			this.RevenueAutoratingDate.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.RevenueAutoratingDate, "RevenueAutoratingDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).RevenueAutoratingDate)));
			this.RevenueAutoratingDate.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("FreightRatesControl|0a74b5c9-22b6-45b3-b5d6-d128ce729e6f", "Rev. Autorating Date", "Revenue Autorating Date", "Overridden");
			this.RevenueAutoratingDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 15, true);
			this.RevenueAutoratingDate.Name = "RevenueAutoratingDate";
			this.RevenueAutoratingDate.TabIndex = 0;
			//
			// CompanyTariffLevelOverride
			//
			this.CompanyTariffLevelOverride.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CompanyTariffLevelOverride, "CompanyTariffLevelOverrideAsString");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).CompanyTariffLevelOverrideAsString)));
			this.CompanyTariffLevelOverride.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("FreightRatesControl|09b8fe74-653a-4616-a55b-0790a3ee9202", "CT Level", "CT Level Override", "Company Tariff Level Override - Override the Company Tariff Level to be used for " +
        "Autorating Revenue");
			this.CompanyTariffLevelOverride.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 37, true);
			this.CompanyTariffLevelOverride.Name = "CompanyTariffLevelOverride";
			this.CompanyTariffLevelOverride.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(303, 17, true);
			this.CompanyTariffLevelOverride.TabIndex = 1;
			//
			// RateCommodity
			//
			this.RateCommodity.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RateCommodity, "JS_RH_NKRateCommodity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).JS_RH_NKRateCommodity)));
			this.RateCommodity.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("FreightRatesControl|c0cc8b99-0988-4541-b586-1e95579de256", "Rate Commodity");
			this.RateCommodity.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 57, true);
			this.RateCommodity.Name = "RateCommodity";
			this.RateCommodity.ParentType = null;
			this.RateCommodity.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(303, 17, true);
			this.RateCommodity.TabIndex = 2;
			//
			// RateLocalCode
			//
			this.BindingSource.SetBindingMember(this.RateLocalCode, "RateLocalCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).RateLocalCode)));
			this.RateLocalCode.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("FreightRatesControl|18115689-0f19-4885-a371-55e9c2705a65", "Comm. LC", "Commodity Local Code", "The Rating Local Code that represents the Rate Commodity");
			this.RateLocalCode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 79, true);
			this.RateLocalCode.Name = "RateLocalCode";
			this.RateLocalCode.ReadOnly = true;
			this.RateLocalCode.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 17, true);
			this.RateLocalCode.TabIndex = 2;
			//
			// FMCTariffID
			//
			this.BindingSource.SetBindingMember(this.FMCTariffID, "JS_FMCTariffID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).JS_FMCTariffID)));
			this.FMCTariffID.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("FreightRatesControl|125c5b2d-7b09-497e-b260-e47a64d37b78", "FMC TID", "FMC Tariff ID", "Federal Maritime Commission Tariff ID");
			this.FMCTariffID.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 99, true);
			this.FMCTariffID.Name = "FMCTariffID";
			this.FMCTariffID.ReadOnly = true;
			this.FMCTariffID.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 17, true);
			this.FMCTariffID.TabIndex = 4;
			//
			// FreightCostRate
			//
			this.FreightCostRate.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FreightCostRate, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Freight.Business.CommonShipment)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)))));
			this.FreightCostRate.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("f30e5429-70cc-4eec-8da4-be7bbd399968", "Negotiated Cost");
			this.FreightCostRate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 121, true);
			this.FreightCostRate.Name = "FreightCostRate";
			this.FreightCostRate.RateType = Enterprise.Freight.Business.FreightConstants.SpotRateType.ShipmentCostRate;
			this.FreightCostRate.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(303, 21, true);
			this.FreightCostRate.TabIndex = 4;
			//
			// PaymentTermAutoratingOverride
			//
			this.PaymentTermAutoratingOverride.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PaymentTermAutoratingOverride, "JS_PaymentTermAutoratingOverride");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).JS_PaymentTermAutoratingOverride)));
			this.PaymentTermAutoratingOverride.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 164, true);
			this.PaymentTermAutoratingOverride.Name = "PaymentTermAutoratingOverride";
			this.PaymentTermAutoratingOverride.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 17, true);
			this.PaymentTermAutoratingOverride.TabIndex = 7;
			//
			// FreightGatewaySellRate
			//
			this.FreightGatewaySellRate.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FreightGatewaySellRate, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Freight.Business.CommonShipment)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)))));
			this.FreightGatewaySellRate.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("277c0c7f-3922-410f-8c08-4129203bb123", "Gateway Sell");
			this.FreightGatewaySellRate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 142, true);
			this.FreightGatewaySellRate.Name = "FreightGatewaySellRate";
			this.FreightGatewaySellRate.RateType = Enterprise.Freight.Business.FreightConstants.SpotRateType.ShipmentGatewaySellRate;
			this.FreightGatewaySellRate.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(303, 21, true);
			this.FreightGatewaySellRate.TabIndex = 6;
			//
			// FreightRateOrigin
			//
			this.FreightRateOrigin.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FreightRateOrigin, "JS_RL_NKFreightRateOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).JS_RL_NKFreightRateOrigin)));
			this.FreightRateOrigin.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 184, true);
			this.FreightRateOrigin.Name = "FreightRateOrigin";
			this.FreightRateOrigin.ParentType = null;
			this.FreightRateOrigin.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(303, 17, true);
			this.FreightRateOrigin.TabIndex = 7;
			//
			// FreightRateDestination
			//
			this.FreightRateDestination.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FreightRateDestination, "JS_RL_NKFreightRateDestination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).JS_RL_NKFreightRateDestination)));
			this.FreightRateDestination.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 206, true);
			this.FreightRateDestination.Name = "FreightRateDestination";
			this.FreightRateDestination.ParentType = null;
			this.FreightRateDestination.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(303, 17, true);
			this.FreightRateDestination.TabIndex = 9;
			//
			// GatewaysGroupBox
			//
			this.GatewaysGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("FreightRatesControl|4ea78d50-a8e3-48b6-9c80-c3b253876d54", "Gateways");
			this.GatewaysGroupBox.Controls.Add(this.GatewaysGrid);
			this.GatewaysGroupBox.Controls.Add(this.GatewaysMovementPanel);
			this.GatewaysGroupBox.Controls.Add(this.GatewayServiceLevelPanel);
			this.GatewaysGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GatewaysGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 232, true);
			this.GatewaysGroupBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.GatewaysGroupBox.Name = "GatewaysGroupBox";
			this.GatewaysGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(439, 138, true);
			this.GatewaysGroupBox.TabIndex = 1;
			this.GatewaysGroupBox.TabStop = false;
			//
			// GatewaysGrid
			//
			this.GatewaysGrid.AllowNavigation = false;
			this.GatewaysGrid.AllowSorting = false;
			this.BindingSource.SetBindingMember(this.GatewaysGrid, "Gateways");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).Gateways)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ShipmentGateway)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).Gateways)).SyncRoot)).JSG_Sequence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Forwarding.Business.ShipmentGateway)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).Gateways)).SyncRoot)).ForwarderPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ShipmentGateway)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).Gateways)).SyncRoot)).Forwarder.OH_FullName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Forwarding.Business.ShipmentGateway)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).Gateways)).SyncRoot)).JSG_OA_ForwarderAddress)));
			this.GatewaysGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "JSG_Sequence";
			zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zGuidFindBoxColumnStyleInfo2.ColumnName = "ForwarderPK";
			zGuidFindBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo2.ColumnName = "Forwarder+OH_FullName";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zAddressDropEditColumnStyleInfo2.ColumnName = "JSG_OA_ForwarderAddress";
			zAddressDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zAddressDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			this.GatewaysGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.GatewaysGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.GatewaysGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.GatewaysGrid.ColumnStyles.Add(zAddressDropEditColumnStyleInfo2);
			this.GatewaysGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GatewaysGrid.GridId = "36073078-744f-47ea-a965-6eb3571b7915";
			this.GatewaysGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.GatewaysGrid.LayoutKey = "zGrid1";
			this.GatewaysGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 45, true);
			this.GatewaysGrid.Name = "GatewaysGrid";
			this.GatewaysGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 91, true);
			this.GatewaysGrid.TabIndex = 3;
			//
			// GatewaysMovementPanel
			//
			this.GatewaysMovementPanel.Controls.Add(this.MoveDownButton);
			this.GatewaysMovementPanel.Controls.Add(this.MoveUpButton);
			this.GatewaysMovementPanel.Dock = System.Windows.Forms.DockStyle.Right;
			this.GatewaysMovementPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(412, 45, true);
			this.GatewaysMovementPanel.Name = "GatewaysMovementPanel";
			this.GatewaysMovementPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(25, 91, true);
			this.GatewaysMovementPanel.TabIndex = 5;
			//
			// MoveDownButton
			//
			this.MoveDownButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.MoveDownButton.IsCaptionOverridden = true;
			this.MoveDownButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 38, true);
			this.MoveDownButton.Name = "MoveDownButton";
			this.MoveDownButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(18, 33, true);
			this.MoveDownButton.TabIndex = 8;
			this.MoveDownButton.Text = "↓";
			this.MoveDownButton.ToolTipCaption = null;
			this.MoveDownButton.Click += new System.EventHandler(this.MoveDownButton_Click);
			//
			// MoveUpButton
			//
			this.MoveUpButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.MoveUpButton.IsCaptionOverridden = true;
			this.MoveUpButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 2, true);
			this.MoveUpButton.Name = "MoveUpButton";
			this.MoveUpButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(18, 33, true);
			this.MoveUpButton.TabIndex = 8;
			this.MoveUpButton.Text = "↑";
			this.MoveUpButton.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.MoveUpButton.ToolTipCaption = null;
			this.MoveUpButton.Click += new System.EventHandler(this.MoveUpButton_Click);
			//
			// GatewayServiceLevelPanel
			//
			this.GatewayServiceLevelPanel.Controls.Add(this.GatewayServiceLevelCodeFindBox);
			this.GatewayServiceLevelPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.GatewayServiceLevelPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.GatewayServiceLevelPanel.Name = "GatewayServiceLevelPanel";
			this.GatewayServiceLevelPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(435, 30, true);
			this.GatewayServiceLevelPanel.TabIndex = 2;
			//
			// GatewayServiceLevelCodeFindBox
			//
			this.GatewayServiceLevelCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GatewayServiceLevelCodeFindBox, "JS_RS_NKGatewayServiceLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).JS_RS_NKGatewayServiceLevel)));
			this.GatewayServiceLevelCodeFindBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("FreightRatesControl|5A62B1EE-1200-4167-AD9D-F67AFF4068AB", "G/W Service");
			this.GatewayServiceLevelCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 5, true);
			this.GatewayServiceLevelCodeFindBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.GatewayServiceLevelCodeFindBox.Name = "GatewayServiceLevelCodeFindBox";
			this.GatewayServiceLevelCodeFindBox.ParentType = null;
			this.GatewayServiceLevelCodeFindBox.PreBoundMaxLength = 5;
			this.GatewayServiceLevelCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 17, true);
			this.GatewayServiceLevelCodeFindBox.TabIndex = 3;
			//
			// FreightRatesControl
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.GatewaysGroupBox);
			this.Controls.Add(this.FreightRatesGroupBox);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.Name = "FreightRatesControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(439, 370, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FreightRatesGroupBox.ResumeLayout(false);
			this.FreightRatesGroupBox.PerformLayout();
			this.RevenueAutoratingDate.ResumeLayout(true);
			this.RevenueAutoratingDate.PerformLayout();
			this.CompanyTariffLevelOverride.ResumeLayout(true);
			this.CompanyTariffLevelOverride.PerformLayout();
			this.RateCommodity.ResumeLayout(true);
			this.RateCommodity.PerformLayout();
			this.FreightCostRate.ResumeLayout(true);
			this.FreightCostRate.PerformLayout();
			this.PaymentTermAutoratingOverride.ResumeLayout(true);
			this.PaymentTermAutoratingOverride.PerformLayout();
			this.FreightGatewaySellRate.ResumeLayout(true);
			this.FreightGatewaySellRate.PerformLayout();
			this.FreightRateOrigin.ResumeLayout(true);
			this.FreightRateOrigin.PerformLayout();
			this.FreightRateDestination.ResumeLayout(true);
			this.FreightRateDestination.PerformLayout();
			this.GatewaysGroupBox.ResumeLayout(false);
			this.GatewaysGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.GatewaysGrid)).EndInit();
			this.GatewaysGrid.ResumeLayout(false);
			this.GatewaysGrid.PerformLayout();
			this.GatewaysMovementPanel.ResumeLayout(false);
			this.GatewaysMovementPanel.PerformLayout();
			this.GatewayServiceLevelPanel.ResumeLayout(false);
			this.GatewayServiceLevelPanel.PerformLayout();
			this.GatewayServiceLevelCodeFindBox.ResumeLayout(true);
			this.GatewayServiceLevelCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
	}
}
