using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class ProfitShareConsolWrapperModuleButtonGrid : ZModuleButtonGrid, IGridControl
	{
		public ProfitShareConsolWrapperModuleButtonGrid()
		{
			InitializeComponent();

			ReadOnly = true;
			ShowNewButton = false;
			ShowEditButton = false;

			if (!DesignModeFinder.IsDesigning)
			{
				SetupColumns();
			}
		}

		public void SetToolStripVisibility(bool visible)
		{
			toolStrip.Visible = visible;
		}

		protected override ZRecordAttacher GetNewRecordAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID)
		{
			return new GatewayConsolProfitShareGridAttacher(destinationCollection, findBoxList, moduleID);
		}

		protected override bool AlwaysShowFormInReadOnlyMode => true;

		protected override bool ShouldSkipActionsMenuItemCreation => true;

		void SetupColumns()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo uniqueConsignRefColumnInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo transportModeColumnInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo consolModeColumnInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo masterbillNumberColumnInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo loadPortColumnInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo dischargePortColumnInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo etdColumnInfo = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo etaColumnInfo = new ZArchitecture.ZDateEditColumnStyleInfo();

			ZArchitecture.ZTextBoxColumnStyleInfo sendingAgentTypeColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo sendingAgentColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo receivingAgentTypeColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo receivingAgentColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();

			ZArchitecture.ZCalcEditColumnStyleInfo totalProfitShareAmountColumnInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo redistributedProfitShareAmountColumnInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();

			ZArchitecture.ZTextBoxColumnStyleInfo flightNumberColumnInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo chargeableRateColumnInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();

			ZArchitecture.ZCalcEditColumnStyleInfo totalShipmentWeightColumnInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo totalShipmentWeightUnitColumnInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo totalShipmentVolumeColumnInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo totalShipmentVolumeUnitColumnInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo totalShipmentChargeableColumnInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo totalShipmentChargeableUnitColumnInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();

			ZArchitecture.ZCalcEditColumnStyleInfo correctedConsolWeightColumnInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo correctedConsolWeightUnitColumnInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo correctedConsolVolumeColumnInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo correctedConsolVolumeUnitColumnInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();

			uniqueConsignRefColumnInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			uniqueConsignRefColumnInfo.ColumnName = "JK_UniqueConsignRef";
			uniqueConsignRefColumnInfo.IsMandatory = true;
			uniqueConsignRefColumnInfo.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("8d2c052f-15ab-4cb2-a432-f520b1b10418", "Consol ID");
			ControlDpiScalingHelper.SetWidth(ref uniqueConsignRefColumnInfo, 100, true);

			transportModeColumnInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			transportModeColumnInfo.ColumnName = "JK_TransportMode";
			ControlDpiScalingHelper.SetWidth(ref transportModeColumnInfo, 60, true);

			consolModeColumnInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			consolModeColumnInfo.ColumnName = "JK_ConsolMode";
			ControlDpiScalingHelper.SetWidth(ref consolModeColumnInfo, 60, true);

			masterbillNumberColumnInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			masterbillNumberColumnInfo.ColumnName = "JK_MasterBillNum";
			ControlDpiScalingHelper.SetWidth(ref masterbillNumberColumnInfo, 100, true);

			loadPortColumnInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			loadPortColumnInfo.ColumnName = "JK_RL_NKLoadPort";
			loadPortColumnInfo.IsVisible = true;
			ControlDpiScalingHelper.SetWidth(ref loadPortColumnInfo, 60, true);

			dischargePortColumnInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			dischargePortColumnInfo.ColumnName = "JK_RL_NKDischargePort";
			dischargePortColumnInfo.IsVisible = true;
			ControlDpiScalingHelper.SetWidth(ref dischargePortColumnInfo, 60, true);

			etdColumnInfo.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("468fc053-0664-4b17-9f37-696cea0cee98", "ETD", "Departure (Estimated)");
			etdColumnInfo.ColumnName = "JK_JX_JA_E_DEP";
			ControlDpiScalingHelper.SetWidth(ref etdColumnInfo, 100, true);

			etaColumnInfo.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("32a48585-2140-462d-8b9f-8ba12440a66c", "ETA", "Arrival (Estimated)");
			etaColumnInfo.ColumnName = "JK_JX_JB_E_ARV";
			ControlDpiScalingHelper.SetWidth(ref etaColumnInfo, 100, true);

			sendingAgentTypeColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			sendingAgentTypeColumnStyleInfo.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("21F213D8-A675-4C3F-95DA-D5D0F21A24F4", "Sending Agent Type");
			sendingAgentTypeColumnStyleInfo.ColumnName = "JK_SendingForwarderHandlingType";
			ControlDpiScalingHelper.SetWidth(ref sendingAgentTypeColumnStyleInfo, 80, true);

			sendingAgentColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			sendingAgentColumnStyleInfo.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("3BC1E55F-4D27-42DE-B973-21F0AE6F086D", "Sending Agent");
			sendingAgentColumnStyleInfo.ColumnName = "JK_Calc_SendingAgentCode";
			ControlDpiScalingHelper.SetWidth(ref sendingAgentColumnStyleInfo, 80, true);

			receivingAgentTypeColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			receivingAgentTypeColumnStyleInfo.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("25FB07D0-7801-49B5-A4EF-03503BF87114", "Receiving Agent Type");
			receivingAgentTypeColumnStyleInfo.ColumnName = "JK_ReceivingForwarderHandlingType";
			ControlDpiScalingHelper.SetWidth(ref receivingAgentTypeColumnStyleInfo, 80, true);

			receivingAgentColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			receivingAgentColumnStyleInfo.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("91791F96-DC13-49D7-A2E6-AF5D0284C1BC", "Receiving Agent");
			receivingAgentColumnStyleInfo.ColumnName = "JK_Calc_ReceivingAgentCode";
			ControlDpiScalingHelper.SetWidth(ref receivingAgentColumnStyleInfo, 80, true);

			totalProfitShareAmountColumnInfo.ColumnName = "JK_Calc_TotalProfitAmount";
			totalProfitShareAmountColumnInfo.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("52B6F782-E3A1-49BE-9AEF-DC0B4A483B9B", "Total Profit Share Amount");
			totalProfitShareAmountColumnInfo.Decimals = 4;
			ControlDpiScalingHelper.SetWidth(ref totalProfitShareAmountColumnInfo, 100, true);

			redistributedProfitShareAmountColumnInfo.ColumnName = "JK_Calc_RedistributedProfitAmount";
			redistributedProfitShareAmountColumnInfo.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("CE3691D5-5A8B-43E9-8E3C-E64EEEFD7FA5", "Redistributed Profit Share Amount");
			redistributedProfitShareAmountColumnInfo.Decimals = 4;
			ControlDpiScalingHelper.SetWidth(ref redistributedProfitShareAmountColumnInfo, 100, true);

			flightNumberColumnInfo.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("45716c9c-9b05-484e-ab68-c8f1c1d45829", "Voyage/Flight");
			flightNumberColumnInfo.ColumnName = "JK_JX_JV_VoyageFlight";
			ControlDpiScalingHelper.SetWidth(ref flightNumberColumnInfo, 60, true);

			chargeableRateColumnInfo.BindToDecimalPlaces = null;
			chargeableRateColumnInfo.ColumnName = "JK_ConsolChargeableRate";
			ControlDpiScalingHelper.SetWidth(ref chargeableRateColumnInfo, 60, true);

			totalShipmentWeightColumnInfo.BindToDecimalPlaces = null;
			totalShipmentWeightColumnInfo.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("b470716e-8495-4220-9be0-cf46123142f8", "Total Wgt.", "Total Shipment Wgt.", "Total Shipment Weight", "");
			totalShipmentWeightColumnInfo.ColumnName = "JK_TotalShipmentWeight";
			totalShipmentWeightColumnInfo.Decimals = 3;
			totalShipmentWeightColumnInfo.IsVisible = false;
			ControlDpiScalingHelper.SetWidth(ref totalShipmentWeightColumnInfo, 60, true);

			totalShipmentWeightUnitColumnInfo.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("83aff665-7a30-4ff7-b024-ca14aae57df9", "WU", "Weight Unit", "Total Shipment Weight Unit", "");
			totalShipmentWeightUnitColumnInfo.ColumnName = "JK_TotalShipmentWeightUnit";
			ControlDpiScalingHelper.SetWidth(ref totalShipmentWeightUnitColumnInfo, 30, true);

			totalShipmentVolumeColumnInfo.BindToDecimalPlaces = null;
			totalShipmentVolumeColumnInfo.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("87e7159e-c1f6-4835-a0fc-c08a6eec22c3", "Total Vol.", "Total Shipment Vol.", "Total Shipment Volume", "");
			totalShipmentVolumeColumnInfo.ColumnName = "JK_TotalShipmentVolume";
			totalShipmentVolumeColumnInfo.Decimals = 3;
			totalShipmentVolumeColumnInfo.IsVisible = false;
			ControlDpiScalingHelper.SetWidth(ref totalShipmentVolumeColumnInfo, 60, true);

			totalShipmentVolumeUnitColumnInfo.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("dc6ab899-27ed-47bb-9c2a-0c674cbe6445", "VU", "Volume Unit", "Total Shipment Volume Unit", "");
			totalShipmentVolumeUnitColumnInfo.ColumnName = "JK_TotalShipmentVolumeUnit";
			ControlDpiScalingHelper.SetWidth(ref totalShipmentVolumeUnitColumnInfo, 30, true);

			totalShipmentChargeableColumnInfo.BindToDecimalPlaces = null;
			totalShipmentChargeableColumnInfo.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("aa3350d6-5ac2-4f7e-9150-7b231088f79c", "Total Chrg.", "Total Shipment Chrg.", "Total Shipment Chargeable", "");
			totalShipmentChargeableColumnInfo.ColumnName = "JK_TotalShipmentChargeable";
			totalShipmentChargeableColumnInfo.Decimals = 3;
			ControlDpiScalingHelper.SetWidth(ref totalShipmentChargeableColumnInfo, 60, true);

			totalShipmentChargeableUnitColumnInfo.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("c247d48b-410f-4a8d-b4f1-d1f837a9355a", "CU", "Chrg. Unit", "Total Shipment Chargeable Unit", "");
			totalShipmentChargeableUnitColumnInfo.ColumnName = "JK_Calc_TotalShipmentChargeableUnit";
			ControlDpiScalingHelper.SetWidth(ref totalShipmentChargeableUnitColumnInfo, 30, true);

			correctedConsolWeightColumnInfo.BindToDecimalPlaces = null;
			correctedConsolWeightColumnInfo.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ebac4b9a-f3a3-4058-97db-db546482e715", "Corrected Weight");
			correctedConsolWeightColumnInfo.ColumnName = "JK_CorrectedConsolWeight";
			correctedConsolWeightColumnInfo.Decimals = 3;
			ControlDpiScalingHelper.SetWidth(ref correctedConsolWeightColumnInfo, 60, true);

			correctedConsolWeightUnitColumnInfo.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("76204ddc-b8af-46f2-924e-9b3b1f83baba", "WU", "Corr. Wgt. Unit", "Corrected Weight Unit", "");
			correctedConsolWeightUnitColumnInfo.ColumnName = "JK_CorrectedConsolWeightUnit";
			ControlDpiScalingHelper.SetWidth(ref correctedConsolWeightUnitColumnInfo, 30, true);

			correctedConsolVolumeColumnInfo.BindToDecimalPlaces = null;
			correctedConsolVolumeColumnInfo.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("4ec2fada-9074-4b55-beb0-ae9b8867ab94", "Corrected Volume");
			correctedConsolVolumeColumnInfo.ColumnName = "JK_CorrectedConsolVolume";
			correctedConsolVolumeColumnInfo.Decimals = 3;
			ControlDpiScalingHelper.SetWidth(ref correctedConsolVolumeColumnInfo, 60, true);

			correctedConsolVolumeUnitColumnInfo.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("3ba87e39-3a8f-423a-beac-471f14fa4bf2", "VU", "Corr. Vol. Unit", "Corrected Volume Unit", "");
			correctedConsolVolumeUnitColumnInfo.ColumnName = "JK_CorrectedConsolVolumeUnit";
			ControlDpiScalingHelper.SetWidth(ref correctedConsolVolumeUnitColumnInfo, 30, true);

			this.ColumnStyles.Add(uniqueConsignRefColumnInfo);
			this.ColumnStyles.Add(transportModeColumnInfo);
			this.ColumnStyles.Add(consolModeColumnInfo);
			this.ColumnStyles.Add(masterbillNumberColumnInfo);
			this.ColumnStyles.Add(loadPortColumnInfo);
			this.ColumnStyles.Add(dischargePortColumnInfo);
			this.ColumnStyles.Add(etdColumnInfo);
			this.ColumnStyles.Add(etaColumnInfo);
			this.ColumnStyles.Add(flightNumberColumnInfo);
			this.ColumnStyles.Add(chargeableRateColumnInfo);

			this.ColumnStyles.Add(totalShipmentWeightColumnInfo);
			this.ColumnStyles.Add(totalShipmentWeightUnitColumnInfo);
			this.ColumnStyles.Add(totalShipmentVolumeColumnInfo);
			this.ColumnStyles.Add(totalShipmentVolumeUnitColumnInfo);
			this.ColumnStyles.Add(totalShipmentChargeableColumnInfo);
			this.ColumnStyles.Add(totalShipmentChargeableUnitColumnInfo);

			this.ColumnStyles.Add(correctedConsolWeightColumnInfo);
			this.ColumnStyles.Add(correctedConsolWeightUnitColumnInfo);
			this.ColumnStyles.Add(correctedConsolVolumeColumnInfo);
			this.ColumnStyles.Add(correctedConsolVolumeUnitColumnInfo);

			this.ColumnStyles.Add(sendingAgentTypeColumnStyleInfo);
			this.ColumnStyles.Add(receivingAgentTypeColumnStyleInfo);
			this.ColumnStyles.Add(sendingAgentColumnStyleInfo);
			this.ColumnStyles.Add(receivingAgentColumnStyleInfo);
			this.ColumnStyles.Add(totalProfitShareAmountColumnInfo);
			this.ColumnStyles.Add(redistributedProfitShareAmountColumnInfo);
		}

		internal class GatewayConsolProfitShareGridAttacher : ZRecordAttacher
		{
			internal GatewayConsolProfitShareGridAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID)
				: base(destinationCollection, findBoxList, moduleID) { }

			protected override bool AttachCore(BusinessObject bizO, List<BusinessObject> listToBulkAdd)
			{
				if (DestinationCollection?.FindByPK(bizO.PK) != null)
				{
					return false;
				}

				var loaded = DestinationCollection.Factory.Load<ForwardingConsol>(bizO.PK);
				if (loaded != null && !loaded.IsDeleted)
				{
					listToBulkAdd.Add(loaded);
					return true;
				}

				return false;
			}
		}
	}
}
