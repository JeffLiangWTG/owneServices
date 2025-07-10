using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.DataTransfer
{
	public class AgencyContainerWorkflowTemplateApplicationExtender : WorkflowTemplateApplicationExtenderWithPortCountry<AgencyShipmentContainer>
	{
		protected override bool IsCondition1Met(AgencyShipmentContainer workflowProvider, ZString conditionCode)
		{
			if (workflowProvider.Booking == null)
			{
				return false;
			}

			switch (conditionCode)
			{
				case AgencyContainerWorkflowCondition1CodeList.Codes.Import:
					return workflowProvider.Booking.IsImport();
				case AgencyContainerWorkflowCondition1CodeList.Codes.Export:
					return workflowProvider.Booking.IsExport();
				case AgencyContainerWorkflowCondition1CodeList.Codes.Domestic:
					return workflowProvider.Booking.IsDomestic();

				case AgencyContainerWorkflowCondition1CodeList.Codes.Confirmed:
					return workflowProvider.Booking.JS_ShipmentStatus == ShipmentStatusList.Codes.Confirmed;
				case AgencyContainerWorkflowCondition1CodeList.Codes.Booked:
					return workflowProvider.Booking.JS_ShipmentStatus == ShipmentStatusList.Codes.Booked;
				case AgencyContainerWorkflowCondition1CodeList.Codes.WaitListed:
					return workflowProvider.Booking.JS_ShipmentStatus == ShipmentStatusList.Codes.WaitListed;

				case AgencyContainerWorkflowCondition1CodeList.Codes.OriginDifferentFromFirstLoad:
					return workflowProvider.Booking.JS_RL_NKOrigin != workflowProvider.Booking.JS_NKLoadPort;
				case AgencyContainerWorkflowCondition1CodeList.Codes.DestinationDifferentFromFinalDischarge:
					return workflowProvider.Booking.JS_RL_NKDestination != workflowProvider.Booking.JS_NKDischargePort;

				default:
					return false;
			}
		}

		protected override bool IsCondition2Met(AgencyShipmentContainer workflowProvider, ZString conditionCode, ZString value)
		{
			if (workflowProvider.Booking == null)
			{
				return false;
			}

			switch (conditionCode)
			{
				case AgencyContainerWorkflowCondition2CodeList.Codes.Import:
					return workflowProvider.Booking.IsImport();
				case AgencyContainerWorkflowCondition2CodeList.Codes.Export:
					return workflowProvider.Booking.IsExport();
				case AgencyContainerWorkflowCondition2CodeList.Codes.Domestic:
					return workflowProvider.Booking.IsDomestic();

				case AgencyContainerWorkflowCondition2CodeList.Codes.FCL:
					return workflowProvider.JC_ContainerMode == Core.Constants.ContainerModes.FCL;
				case AgencyContainerWorkflowCondition2CodeList.Codes.LCL:
					return workflowProvider.JC_ContainerMode == Core.Constants.ContainerModes.LCL;
				case AgencyContainerWorkflowCondition2CodeList.Codes.BCN:
					return workflowProvider.JC_ContainerMode == Core.Constants.ContainerModes.BuyersConsol;
				case AgencyContainerWorkflowCondition2CodeList.Codes.GRP:
					return workflowProvider.JC_ContainerMode == Core.Constants.ContainerModes.Groupage;

				default:
					return false;
			}
		}

		protected override ZString OriginCountry(AgencyShipmentContainer workflowProvider)
		{
			return workflowProvider.Booking?.CalcLoadPort?.RL_RN_NKCountryCode ?? ZString.Empty;
		}

		protected override ZString DestinationCountry(AgencyShipmentContainer workflowProvider)
		{
			return workflowProvider.Booking?.CalcDischargePort?.RL_RN_NKCountryCode ?? ZString.Empty;
		}
	}
}
