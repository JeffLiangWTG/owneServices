using System;
using CargoWise.Types;
using Enterprise.ContractManagement.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.GUI
{
	sealed class ConsolContractAndRouteAllocationFormActions : IRatingContractSimulationFormActions
	{
		public ConsolContractAndRouteAllocationFormActions(ForwardingConsol consol)
		{
			this.consol = consol;
		}

		public ConsolContractAndRouteAllocationFormActions(ForwardingContainer container)
		{
			this.container = container;
			consol = container?.Consol;
		}

		readonly ForwardingConsol consol;
		readonly ForwardingContainer container;

		#region IRatingContractSimulationFormActions

		bool IRatingContractSimulationFormActions.IsEnabledAllocationToContract => consol != null;
		bool IRatingContractSimulationFormActions.IsEnabledAllocationToRoute => consol != null;

		bool IRatingContractSimulationFormActions.TryAllocateToContract(IRatingContract contract)
		{
			if (contract == null
				|| consol == null)
			{
				return false;
			}

			var contractCarrierAddressPK = contract.ServiceProvider?.MainAddress?.PK ?? ZGuid.Empty;
			if (!consol.JK_OA_ShippingLineAddress.IsEmpty
				&& consol.JK_OA_ShippingLineAddress != contractCarrierAddressPK
				&& ZDialogResult.No == ConsolCarrierOverridePromptUtil.PromptConfirmationForCarrierOverrideWithContract(contract, consol))
			{
				return false;
			}

			consol.JK_OA_ShippingLineAddress = contractCarrierAddressPK;
			consol.JK_CarrierContractNumber = contract.RCT_ContractNumber;
			consol.JK_RCA_AllocationLine = ZGuid.Empty;

			if (container != null)
			{
				container.JC_RCA_AllocationLine = ZGuid.Empty;
			}

			return true;
		}

		bool IRatingContractSimulationFormActions.TryAllocateToAllocationRoute(IRatingContractAllocationLine route)
		{
			if (route is not RatingContractAllocationLine allocationRoute
				|| consol == null)
			{
				return false;
			}

			var allocationRouteCarrierAddressPK = allocationRoute.Contract?.ServiceProvider?.MainAddress?.PK ?? ZGuid.Empty;

			if (!consol.JK_OA_ShippingLineAddress.IsEmpty
				&& consol.JK_OA_ShippingLineAddress != allocationRouteCarrierAddressPK
				&& ZDialogResult.No == ConsolCarrierOverridePromptUtil.PromptConfirmationForCarrierOverrideWithAllocationRoute(allocationRoute, consol))
			{
				return false;
			}

			if (!allocationRoute.RCA_JX_SailingSchedule.IsEmpty
			&& !CCARouteValidationHelper.DoAnyConsolTransportLegsMatchAllocationRouteSchedule(allocationRoute, consol))
			{
				var schedule = allocationRoute.JobSailing as JobSailing;
				var message = Res.GetString(
					"9655fbb0-c225-b6a7-4e4f-6a0616c63748",
					@"Linked Schedule: {0}, {1}, {2}, {3}, {4}, {5} of Allocation Route ({6}) should match one of the Routing Legs of {7}.
Would you like to use the Linked Schedule to create a new Routing Leg to complete the Allocation?",
					schedule.Origin.JA_RL_NKPortOfLoading,
					schedule.Destination.JB_RL_NKPortOfDischarge,
					schedule.Origin.JA_E_DEP.ToShortDateString(),
					schedule.Voyage.JV_RV_NKVessel,
					schedule.Voyage.JV_VoyageFlight,
					schedule.JX_ServiceString,
					allocationRoute.RCA_AllocationLineID,
					string.IsNullOrEmpty(consol.JK_UniqueConsignRef)
						? Res.GetString("eda270e6-9378-1fa2-4aaf-34cf6d8fc689", "the Consol")
						: Res.GetString("58277ec8-e9a4-6ca9-4dfc-88e07c5ee3fd", "Consol ({0})", consol.JK_UniqueConsignRef));

				var response = Globals.Message.Show(
					message,
					Res.GetString("e2d7aa15-9544-acbe-45e9-ce4be378f244", "Create a Routing Leg on the Consol"),
					ZMessageBoxButtons.YesNo,
					ZMessageBoxIcon.Warning);

				if (response == ZDialogResult.Yes)
				{
					var transportLegs = consol.Transports;

					Transport transportLeg;

					if (transportLegs.Count == 1
						&& transportLegs[0] is Transport leg
						&& (!leg.IsInDatabase || leg.HasChanges)
						&& DoesTransportLegPartiallyMatchSchedule(leg, schedule))
					{
						transportLeg = transportLegs[0];
					}
					else
					{
						transportLeg = consol.Transports.AddNew();
					}

					transportLeg.JW_IsLinked = true;
					transportLeg.JW_JX = allocationRoute.RCA_JX_SailingSchedule;
				}
				else
				{
					return false;
				}
			}

			consol.JK_OA_ShippingLineAddress = allocationRouteCarrierAddressPK;
			consol.JK_CarrierContractNumber = allocationRoute.Contract?.RCT_ContractNumber ?? ZString.Empty;

			if (container != null)
			{
				container.JC_RCA_AllocationLine = allocationRoute.PK;
			}
			else
			{
				consol.JK_RCA_AllocationLine = allocationRoute.PK;
			}

			return true;
		}

		bool DoesTransportLegPartiallyMatchSchedule(Transport transportLeg, JobSailing schedule)
		{
			var origin = schedule.Origin;
			var destination = schedule.Destination;
			var voyage = schedule.Voyage;

			return (transportLeg.JW_ETD.IsEmpty || transportLeg.JW_ETD == origin.JA_E_DEP)
				&& (transportLeg.JW_RL_NKLoadPort.IsEmpty || string.Equals(transportLeg.JW_RL_NKLoadPort, origin.JA_RL_NKPortOfLoading, StringComparison.OrdinalIgnoreCase))
				&& (transportLeg.JW_RL_NKDiscPort.IsEmpty || string.Equals(transportLeg.JW_RL_NKDiscPort, destination.JB_RL_NKPortOfDischarge, StringComparison.OrdinalIgnoreCase))
				&& (transportLeg.JW_VoyageFlight.IsEmpty || string.Equals(transportLeg.JW_VoyageFlight, voyage.JV_VoyageFlight, StringComparison.OrdinalIgnoreCase))
				&& (transportLeg.JW_Vessel.IsEmpty || string.Equals(transportLeg.JW_Vessel, voyage.JV_RV_NKVessel, StringComparison.OrdinalIgnoreCase))
				&& (transportLeg.JW_ServiceString.IsEmpty || string.Equals(transportLeg.JW_ServiceString, schedule.JX_ServiceString, StringComparison.OrdinalIgnoreCase))
				&& (transportLeg.CarrierPK.IsEmpty || transportLeg.CarrierPK == voyage.JV_OH_Line);
		}

		#endregion
	}
}
