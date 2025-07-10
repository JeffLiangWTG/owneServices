using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	internal class JobShipmentPreplanningOrderHelper
	{
		public JobShipmentPreplanningOrderHelper(JobShipmentPreplanning preAdvice)
		{
			Argument.NotNull(preAdvice, "preAdvice");
			this.preAdvice = preAdvice;
		}

		readonly JobShipmentPreplanning preAdvice;

		#region Set Values On Order

		public void SetValuesOnOrder(Order order)
		{
			if (order.JD_MasterWaybill != preAdvice.EF_MasterBill)
			{
				order.JD_MasterWaybill = preAdvice.EF_MasterBill;
			}

			if (order.JD_JS != preAdvice.EF_JS && preAdvice.EF_JS.IsValid)
			{
				order.JD_JS = preAdvice.EF_JS;
			}

			if (order.JD_JE != preAdvice.EF_JE && preAdvice.EF_JE.IsValid)
			{
				order.JD_JE = preAdvice.EF_JE;
			}

			if (order.JD_RL_NKPortOfLoading != preAdvice.EF_RL_NKPortLoad)
			{
				if (order.JD_RL_NKGoodsAvailableAt == order.JD_RL_NKPortOfLoading)
				{
					order.JD_RL_NKGoodsAvailableAt = preAdvice.EF_RL_NKPortLoad;
				}
				order.JD_RL_NKPortOfLoading = preAdvice.EF_RL_NKPortLoad;
			}

			if (order.JD_RL_NKPortOfDischarge != preAdvice.EF_RL_NKPortDisch)
			{
				if (order.JD_RL_NKGoodsDeliveredTo == order.JD_RL_NKPortOfDischarge)
				{
					order.JD_RL_NKGoodsDeliveredTo = preAdvice.EF_RL_NKPortDisch;
				}
				order.JD_RL_NKPortOfDischarge = preAdvice.EF_RL_NKPortDisch;
			}

			if (order.JD_Waybill != preAdvice.EF_HouseBill && (order.JD_Waybill == (ZString)preAdvice.EF_HouseBillInfo.OriginalValue || order.JD_Waybill.IsEmpty))
			{
				order.JD_Waybill = preAdvice.EF_HouseBill;
			}

			if (order.JD_OH_Carrier != preAdvice.EF_OH_Carrier)
			{
				order.JD_OH_Carrier = preAdvice.EF_OH_Carrier;
			}

			if (order.JD_OA_BuyerAddress != preAdvice.EF_OA_BuyerAddress)
			{
				order.JD_OA_BuyerAddress = preAdvice.EF_OA_BuyerAddress;
			}

			if (order.JD_OC_BuyerContact != preAdvice.EF_OC_BuyerContact)
			{
				order.JD_OC_BuyerContact = preAdvice.EF_OC_BuyerContact;
			}

			if (order.JD_OH_SendingAgent != preAdvice.EF_OH_SendingAgent)
			{
				order.JD_OH_SendingAgent = preAdvice.EF_OH_SendingAgent;
			}

			if (order.JD_OH_ReceivingAgent != preAdvice.EF_OH_ReceivingAgent)
			{
				order.JD_OH_ReceivingAgent = preAdvice.EF_OH_ReceivingAgent;
			}

			if (order.JD_RL_NKGoodsAvailableAt.IsEmpty)
			{
				order.JD_RL_NKGoodsAvailableAt = preAdvice.EF_RL_NKPortLoad;
			}

			if (order.JD_RL_NKGoodsDeliveredTo.IsEmpty)
			{
				order.JD_RL_NKGoodsDeliveredTo = preAdvice.EF_RL_NKPortDisch;
			}

			UpdatePlanningVesselsAndDatesFromPreAdvice(order);
			UpdateContainersFromPreAdvice(order);
		}

		void UpdateContainersFromPreAdvice(Order order)
		{
			var containersTouched = new List<OrderContainer>();

			foreach (OrderContainer containerForCopy in preAdvice.Containers)
			{
				ZQuery query = new ZQuery(JobOrderContainerSchema.J1_ContainerNumber, containerForCopy.J1_ContainerNumber);
				query.AddToFilter(JobOrderContainerSchema.J1_RC, containerForCopy.J1_RC);
				query.AddToFilter(JobOrderContainerSchema.J1_ContainerCount, containerForCopy.J1_ContainerCount);
				var matchedOrNewContainer =
					order.PlannedContainers.Find(query).OfType<OrderContainer>().FirstOrDefault() ??
					order.PlannedContainers.AddNew();

				matchedOrNewContainer.J1_ContainerNumber = containerForCopy.J1_ContainerNumber;
				matchedOrNewContainer.J1_ContainerCount = containerForCopy.J1_ContainerCount;
				matchedOrNewContainer.J1_RC = containerForCopy.J1_RC;
				matchedOrNewContainer.J1_SealNum = containerForCopy.J1_SealNum;
				matchedOrNewContainer.J1_AdditionalSealNum = containerForCopy.J1_AdditionalSealNum;
				matchedOrNewContainer.J1_Additional2SealNum = containerForCopy.J1_Additional2SealNum;

				containersTouched.Add(matchedOrNewContainer);
			}

			order.PlannedContainers.Except(containersTouched).ToArray().ForEach(order.PlannedContainers.RemoveAndDelete);
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void UpdatePlanningVesselsAndDatesFromPreAdvice(Order order)
		{
			if (preAdvice.PreAdviceTransports.Count > 0)
			{
				Transport departureTransport = preAdvice.PreAdviceTransports.FindTransportByLoadPort(preAdvice.EF_RL_NKPortLoad);
				Transport arrivalTransport = preAdvice.PreAdviceTransports.FindTransportByDischargePort(preAdvice.EF_RL_NKPortDisch);

				departureTransport = departureTransport ?? arrivalTransport;
				arrivalTransport = arrivalTransport ?? departureTransport;

				var otherTransport = SetOrderOtherTransport(arrivalTransport, departureTransport);

				order.JD_TransportMode = departureTransport != null ? departureTransport.JW_TransportMode : ZString.Empty;
				order.JD_DepartureVoyage = departureTransport != null ? departureTransport.JW_VoyageFlight : ZString.Empty;
				if (departureTransport != null && !departureTransport.JW_Vessel.IsEmpty)
				{
					order.JD_RV_NKDepartureVessel = departureTransport.JW_Vessel;
				}

				if (departureTransport != arrivalTransport)
				{
					order.JD_ArrivalVoyage = arrivalTransport != null ? arrivalTransport.JW_VoyageFlight : ZString.Empty;
					if (arrivalTransport != null && !arrivalTransport.JW_Vessel.IsEmpty)
					{
						order.JD_RV_NKArrivalVessel = arrivalTransport.JW_Vessel;
					}
				}

				if (otherTransport != null)
				{
					if (!otherTransport.JW_VoyageFlight.IsEmpty)
					{
						order.JD_IntermediateVoyage = otherTransport.JW_VoyageFlight;
					}

					if (!otherTransport.JW_Vessel.IsEmpty)
					{
						order.JD_RV_NKIntermediateVessel = otherTransport.JW_Vessel;
					}
				}

				if (departureTransport != null)
				{
					if (arrivalTransport != departureTransport || order.JD_E_ARV_1stIntermediate.IsEmpty)
					{
						order.JD_E_ARV_1stIntermediate = departureTransport.JW_ETA;
					}

					if (departureTransport.JW_ETD != departureTransport.PreviousJW_ETD || order.GetMilestoneEstimatedDate(Events.Departure).IsEmpty)
					{
						order.UpdateEventEstimate(Events.Departure, departureTransport.JW_ETD.ToOffset());
					}

					if (departureTransport.JW_ATD != departureTransport.PreviousJW_ATD || order.GetMilestoneActualDate(Events.Departure).IsEmpty)
					{
						order.UpdateEvent(Events.Departure, departureTransport.JW_ATD.ToOffset());
					}
				}

				if (otherTransport != null)
				{
					order.JD_E_DEP_2 = otherTransport.JW_ETD;
					order.JD_E_ARV_2ndIntermediate = otherTransport.JW_ETA;
				}

				if (arrivalTransport != null)
				{
					if (arrivalTransport != departureTransport || order.JD_E_DEP_3.IsEmpty)
					{
						order.JD_E_DEP_3 = arrivalTransport.JW_ETD;
					}

					if (arrivalTransport.JW_ETA != arrivalTransport.PreviousJW_ETA || order.GetMilestoneEstimatedDate(Events.Arrival).IsEmpty)
					{
						order.UpdateEventEstimate(Events.Arrival, arrivalTransport.JW_ETA.ToOffset());
					}

					if (arrivalTransport.JW_ATA != arrivalTransport.PreviousJW_ATA || order.GetMilestoneActualDate(Events.Arrival).IsEmpty)
					{
						order.UpdateEvent(Events.Arrival, arrivalTransport.JW_ATA.ToOffset());
					}
				}
			}
		}

		Transport SetOrderOtherTransport(Transport arrivalTransport, Transport departureTransport)
		{
			Transport otherTransport = null;

			if (departureTransport != arrivalTransport)
			{
				foreach (Transport trans in preAdvice.PreAdviceTransports)
				{
					if (trans.JW_RL_NKDiscPort != preAdvice.EF_RL_NKPortDisch &&
						trans.JW_RL_NKLoadPort != preAdvice.EF_RL_NKPortLoad)
					{
						otherTransport = trans;
						break;
					}
				}
			}

			return otherTransport;
		}

		#endregion
	}
}
