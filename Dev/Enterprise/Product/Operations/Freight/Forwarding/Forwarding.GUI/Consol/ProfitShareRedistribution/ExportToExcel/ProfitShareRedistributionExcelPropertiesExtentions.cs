using System.Collections.Generic;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Excel;

namespace Enterprise.Freight.Forwarding.GUI.Consol.ProfitShareRedistribution.ExportToExcel
{
	internal static class ProfitShareRedistributionExcelPropertiesExtentions
	{
		const int DefaultColumnWith = 5000;

		public static List<ExcelExportColumnBase> GetExcelExportColumns(this ProfitShareForwardingConsolWrapper profitShareForwardingConsolWrapper)
		{
			var result = new List<ExcelExportColumnBase>()
			{
				new ExcelExportColumn<ProfitShareForwardingConsolWrapper>(nameof(ProfitShareForwardingConsolWrapper.JK_UniqueConsignRef), profitShareForwardingConsolWrapper.Consol.JK_UniqueConsignRefInfo.HumanReadableName, DefaultColumnWith),
				new ExcelExportColumn<ProfitShareForwardingConsolWrapper>(nameof(ProfitShareForwardingConsolWrapper.JK_TransportMode), profitShareForwardingConsolWrapper.Consol.JK_TransportModeInfo.HumanReadableName, DefaultColumnWith),
				new ExcelExportColumn<ProfitShareForwardingConsolWrapper>(nameof(ProfitShareForwardingConsolWrapper.JK_ConsolMode), profitShareForwardingConsolWrapper.Consol.JK_ConsolModeInfo.HumanReadableName, DefaultColumnWith),
				new ExcelExportColumn<ProfitShareForwardingConsolWrapper>(nameof(ProfitShareForwardingConsolWrapper.JK_MasterBillNum), profitShareForwardingConsolWrapper.Consol.JK_MasterBillNumInfo.HumanReadableName, DefaultColumnWith),
				new ExcelExportColumn<ProfitShareForwardingConsolWrapper>(nameof(ProfitShareForwardingConsolWrapper.JK_RL_NKLoadPort), profitShareForwardingConsolWrapper.Consol.JK_RL_NKLoadPortInfo.HumanReadableName, DefaultColumnWith),
				new ExcelExportColumn<ProfitShareForwardingConsolWrapper>(nameof(ProfitShareForwardingConsolWrapper.JK_RL_NKDischargePort), profitShareForwardingConsolWrapper.Consol.JK_RL_NKDischargePortInfo.HumanReadableName, DefaultColumnWith),
				new ExcelExportColumn<ProfitShareForwardingConsolWrapper>(nameof(ProfitShareForwardingConsolWrapper.JK_JX_JA_E_DEP), profitShareForwardingConsolWrapper.Consol.JK_JX_JA_E_DEPInfo.HumanReadableName, DefaultColumnWith),
				new ExcelExportColumn<ProfitShareForwardingConsolWrapper>(nameof(ProfitShareForwardingConsolWrapper.JK_JX_JB_E_ARV), profitShareForwardingConsolWrapper.Consol.JK_JX_JB_E_ARVInfo.HumanReadableName, DefaultColumnWith),
				new ExcelExportColumn<ProfitShareForwardingConsolWrapper>(nameof(ProfitShareForwardingConsolWrapper.JK_JX_JV_VoyageFlight), profitShareForwardingConsolWrapper.Consol.JK_JX_JV_VoyageFlightInfo.HumanReadableName, DefaultColumnWith),
				new ExcelExportColumn<ProfitShareForwardingConsolWrapper>(nameof(ProfitShareForwardingConsolWrapper.JK_ConsolChargeableRate), profitShareForwardingConsolWrapper.Consol.JK_ConsolChargeableRateInfo.HumanReadableName, DefaultColumnWith),
				new ExcelExportColumn<ProfitShareForwardingConsolWrapper>(nameof(ProfitShareForwardingConsolWrapper.JK_TotalShipmentWeightUnit), profitShareForwardingConsolWrapper.Consol.JK_TotalShipmentWeightUnitInfo.HumanReadableName, DefaultColumnWith),
				new ExcelExportColumn<ProfitShareForwardingConsolWrapper>(nameof(ProfitShareForwardingConsolWrapper.JK_TotalShipmentVolumeUnit), profitShareForwardingConsolWrapper.Consol.JK_TotalShipmentVolumeUnitInfo.HumanReadableName, DefaultColumnWith),
				new ExcelExportColumn<ProfitShareForwardingConsolWrapper>(nameof(ProfitShareForwardingConsolWrapper.JK_TotalShipmentChargeable), profitShareForwardingConsolWrapper.Consol.JK_TotalShipmentChargeableInfo.HumanReadableName, DefaultColumnWith),
				new ExcelExportColumn<ProfitShareForwardingConsolWrapper>(nameof(ProfitShareForwardingConsolWrapper.JK_Calc_TotalShipmentChargeableUnit), profitShareForwardingConsolWrapper.Consol.JK_Calc_TotalShipmentChargeableUnitInfo.HumanReadableName, DefaultColumnWith),
				new ExcelExportColumn<ProfitShareForwardingConsolWrapper>(nameof(ProfitShareForwardingConsolWrapper.JK_CorrectedConsolWeight), profitShareForwardingConsolWrapper.Consol.JK_CorrectedConsolWeightInfo.HumanReadableName, DefaultColumnWith),
				new ExcelExportColumn<ProfitShareForwardingConsolWrapper>(nameof(ProfitShareForwardingConsolWrapper.JK_CorrectedConsolWeightUnit), profitShareForwardingConsolWrapper.Consol.JK_CorrectedConsolWeightUnitInfo.HumanReadableName, DefaultColumnWith),
				new ExcelExportColumn<ProfitShareForwardingConsolWrapper>(nameof(ProfitShareForwardingConsolWrapper.JK_CorrectedConsolVolume), profitShareForwardingConsolWrapper.Consol.JK_CorrectedConsolVolumeInfo.HumanReadableName, DefaultColumnWith),
				new ExcelExportColumn<ProfitShareForwardingConsolWrapper>(nameof(ProfitShareForwardingConsolWrapper.JK_CorrectedConsolVolumeUnit), profitShareForwardingConsolWrapper.Consol.JK_CorrectedConsolVolumeUnitInfo.HumanReadableName, DefaultColumnWith),
				new ExcelExportColumn<ProfitShareForwardingConsolWrapper>(nameof(ProfitShareForwardingConsolWrapper.JK_SendingForwarderHandlingType), profitShareForwardingConsolWrapper.Consol.JK_SendingForwarderHandlingTypeInfo.HumanReadableName, DefaultColumnWith),
				new ExcelExportColumn<ProfitShareForwardingConsolWrapper>(nameof(ProfitShareForwardingConsolWrapper.JK_ReceivingForwarderHandlingType), profitShareForwardingConsolWrapper.Consol.JK_ReceivingForwarderHandlingTypeInfo.HumanReadableName, DefaultColumnWith),
				new ExcelExportColumn<ProfitShareForwardingConsolWrapper>(nameof(ProfitShareForwardingConsolWrapper.JK_Calc_SendingAgentCode), profitShareForwardingConsolWrapper.Consol.JK_Calc_SendingAgentCodeInfo.HumanReadableName, DefaultColumnWith),
				new ExcelExportColumn<ProfitShareForwardingConsolWrapper>(nameof(ProfitShareForwardingConsolWrapper.JK_Calc_ReceivingAgentCode), profitShareForwardingConsolWrapper.Consol.JK_Calc_ReceivingAgentCodeInfo.HumanReadableName, DefaultColumnWith),
				new ExcelExportColumn<ProfitShareForwardingConsolWrapper>(nameof(ProfitShareForwardingConsolWrapper.JK_Calc_TotalProfitAmount), Res.GetString("4A1B4F7C-23BB-4565-A3F4-AED6FBC20C25", "Total Profit Share Amount"), DefaultColumnWith),
				new ExcelExportColumn<ProfitShareForwardingConsolWrapper>(nameof(ProfitShareForwardingConsolWrapper.JK_Calc_RedistributedProfitAmount), Res.GetString("78900782-c58b-40b8-a5b0-1d18838006d0", "Redistributed Profit Share Amount"), DefaultColumnWith),
			};

			return result;
		}

		public static List<ExcelExportColumnBase> GetExcelExportColumns(this ProfitShareForwardingShipmentWrapper profitShareForwardingShipmentWrapper)
		{
			var result = new List<ExcelExportColumnBase>()
			{
				new ExcelExportColumn<ProfitShareForwardingShipmentWrapper>(nameof(ProfitShareForwardingShipmentWrapper.JS_UniqueConsignRef), profitShareForwardingShipmentWrapper.Shipment.JS_UniqueConsignRefInfo.HumanReadableName, DefaultColumnWith),
				new ExcelExportColumn<ProfitShareForwardingShipmentWrapper>(nameof(ProfitShareForwardingShipmentWrapper.JS_RL_NKOrigin), profitShareForwardingShipmentWrapper.Shipment.JS_RL_NKOriginInfo.HumanReadableName, DefaultColumnWith),
				new ExcelExportColumn<ProfitShareForwardingShipmentWrapper>(nameof(ProfitShareForwardingShipmentWrapper.JS_RL_NKDestination), profitShareForwardingShipmentWrapper.Shipment.JS_RL_NKDestinationInfo.HumanReadableName, DefaultColumnWith),
				new ExcelExportColumn<ProfitShareForwardingShipmentWrapper>(nameof(ProfitShareForwardingShipmentWrapper.JS_HouseBill), profitShareForwardingShipmentWrapper.Shipment.JS_HouseBillInfo.HumanReadableName, DefaultColumnWith),
				new ExcelExportColumn<ProfitShareForwardingShipmentWrapper>(nameof(ProfitShareForwardingShipmentWrapper.JS_OuterPacks), profitShareForwardingShipmentWrapper.Shipment.JS_OuterPacksInfo.HumanReadableName, DefaultColumnWith),
				new ExcelExportColumn<ProfitShareForwardingShipmentWrapper>(nameof(ProfitShareForwardingShipmentWrapper.JS_ActualWeight), profitShareForwardingShipmentWrapper.Shipment.JS_ActualWeightInfo.HumanReadableName, DefaultColumnWith),
				new ExcelExportColumn<ProfitShareForwardingShipmentWrapper>(nameof(ProfitShareForwardingShipmentWrapper.JS_UnitOfWeight), profitShareForwardingShipmentWrapper.Shipment.JS_UnitOfWeightInfo.HumanReadableName, DefaultColumnWith),
				new ExcelExportColumn<ProfitShareForwardingShipmentWrapper>(nameof(ProfitShareForwardingShipmentWrapper.JS_ActualVolume), profitShareForwardingShipmentWrapper.Shipment.JS_ActualVolumeInfo.HumanReadableName, DefaultColumnWith),
				new ExcelExportColumn<ProfitShareForwardingShipmentWrapper>(nameof(ProfitShareForwardingShipmentWrapper.JS_UnitOfVolume), profitShareForwardingShipmentWrapper.Shipment.JS_UnitOfVolumeInfo.HumanReadableName, DefaultColumnWith),
				new ExcelExportColumn<ProfitShareForwardingShipmentWrapper>(nameof(ProfitShareForwardingShipmentWrapper.JS_E_DEP), profitShareForwardingShipmentWrapper.Shipment.JS_E_DEPInfo.HumanReadableName, DefaultColumnWith),
				new ExcelExportColumn<ProfitShareForwardingShipmentWrapper>(nameof(ProfitShareForwardingShipmentWrapper.JS_E_ARV), profitShareForwardingShipmentWrapper.Shipment.JS_E_ARVInfo.HumanReadableName, DefaultColumnWith),
				new ExcelExportColumn<ProfitShareForwardingShipmentWrapper>(nameof(ProfitShareForwardingShipmentWrapper.JS_JK_ConsolID), profitShareForwardingShipmentWrapper.Shipment.JS_JK_ConsolIDInfo.HumanReadableName, DefaultColumnWith),
				new ExcelExportColumn<ProfitShareForwardingShipmentWrapper>(nameof(ProfitShareForwardingShipmentWrapper.JS_Calc_PickupAgentProfitShare), Res.GetString("B6341C23-0BE7-4685-B955-65B4D98DA3F8", "Pickup Agent Profit Amount"), DefaultColumnWith),
				new ExcelExportColumn<ProfitShareForwardingShipmentWrapper>(nameof(ProfitShareForwardingShipmentWrapper.JS_Calc_DeliveryAgentProfitShare), Res.GetString("D40F9692-9132-43EE-B9F7-E662BA64B5F7", "Delivery Agent Profit Amount"), DefaultColumnWith),
			};

			return result;
		}
	}
}
